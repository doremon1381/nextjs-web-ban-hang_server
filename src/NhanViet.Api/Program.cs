using System.Text.Json;
using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NhanViet.Application.Common.Behaviors;
using NhanViet.Application.Orders.Commands;
using NhanViet.Api.Middleware;
using NhanViet.Infrastructure;
using Serilog;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// --- Serilog ---
builder.Host.UseSerilog((ctx, cfg) => cfg
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "NhanViet.Api")
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day));

// --- Controllers + Swagger ---
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Nhãn Việt API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Supabase JWT token — obtain from Supabase Auth.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// --- MediatR + FluentValidation ---
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(CreateOrderCommand).Assembly));
builder.Services.AddValidatorsFromAssembly(typeof(CreateOrderCommand).Assembly);
// for any TRequest, TResponse
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>)); 

// --- Infrastructure ---
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHttpContextAccessor();

// --- Authentication: Supabase OIDC/JWKS (RS256) ---
var supabaseUrl = builder.Configuration["Supabase:Url"]
    ?? throw new InvalidOperationException("Supabase:Url is not configured.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.Authority = $"{supabaseUrl}/auth/v1";

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuers = [$"{supabaseUrl}/auth/v1", $"{supabaseUrl}/auth/v1/"],
            ValidateAudience = true,
            ValidAudience = "authenticated",
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(60),
            ValidateIssuerSigningKey = true,
            NameClaimType = "sub",
        };

        if (builder.Environment.IsDevelopment())
            options.RequireHttpsMetadata = false;

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                var identity = (ClaimsIdentity)context.Principal!.Identity!;
                FlattenJsonClaim(identity, "app_metadata");
                FlattenJsonClaim(identity, "user_metadata");
                return Task.CompletedTask;
            }
        };
    });

// --- Authorization ---
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("AdminOnly", policy =>
        policy.RequireAssertion(ctx => ctx.User.FindFirst("role")?.Value == "Admin"));

// --- CORS ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins(
                builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [])
            .AllowAnyHeader()
            .AllowAnyMethod()
            .WithExposedHeaders("X-NV-Session");
    });
});

// --- Rate Limiting ---
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("api", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 100;
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 10;
    });
    options.AddFixedWindowLimiter("contact", opt =>
    {
        opt.Window = TimeSpan.FromHours(1);
        opt.PermitLimit = 3;
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 0;
    });
});

// --- Health Checks ---
var dbConn = builder.Configuration.GetConnectionString("SupabaseDirectConnection")
    ?? throw new InvalidOperationException("ConnectionStrings:SupabaseDirectConnection is not configured.");

builder.Services.AddHealthChecks()
    .AddNpgSql(dbConn, name: "supabase-postgres", timeout: TimeSpan.FromSeconds(3))
    .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy());

var app = builder.Build();

// --- Middleware Pipeline ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseCors("Frontend");
app.UseAuthentication();
app.UseMiddleware<UserProvisioningMiddleware>();
app.UseAuthorization();
app.UseRateLimiter();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

static void FlattenJsonClaim(ClaimsIdentity identity, string sourceClaim)
{
    var raw = identity.FindFirst(sourceClaim)?.Value;
    if (string.IsNullOrEmpty(raw)) return;
    try
    {
        using var doc = JsonDocument.Parse(raw);
        foreach (var prop in doc.RootElement.EnumerateObject())
        {
            var value = prop.Value.ValueKind switch
            {
                JsonValueKind.String => prop.Value.GetString(),
                _ => prop.Value.GetRawText(),
            };
            if (value is not null && !identity.HasClaim(c => c.Type == prop.Name))
                identity.AddClaim(new Claim(prop.Name, value));
        }
    }
    catch { /* malformed metadata — skip */ }
}

public partial class Program { }
