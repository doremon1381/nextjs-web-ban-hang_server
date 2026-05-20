using System.Text.Json;
using System.Security.Claims;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NhanViet.Api.Auth;
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
            ValidAlgorithms = ["RS256", "ES256"], // Supabase may use either RSA or ECDSA for JWT signing, depending on the key type configured in the project settings.
            ClockSkew = TimeSpan.FromSeconds(60),
            ValidateIssuerSigningKey = true,
            NameClaimType = "sub",
        };
        options.MapInboundClaims = false; // Don't map claims to Microsoft-specific types. We'll handle claims manually in the OnTokenValidated event.

        if (builder.Environment.IsDevelopment())
        {
            options.RequireHttpsMetadata = false;
            options.MetadataAddress = $"{supabaseUrl}/auth/v1/.well-known/jwks.json";
            // OR explicit static key:
            // options.TokenValidationParameters.IssuerSigningKey = new JsonWebKey(File.ReadAllText("dev-jwks.json"));
        }
        else
        {
            options.RequireHttpsMetadata = true;
        }
        
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                var identity = (ClaimsIdentity)context.Principal!.Identity!;

                // TODO: If we add more claims from app_metadata, consider namespacing them to avoid collisions with user_metadata claims. For now, we only allow-list a few keys so it's not an issue.

                // TODO: testing only
                Console.WriteLine("Raw claims from JWT:");
                foreach (var claim in identity.Claims)
                {
                    Console.WriteLine($"  {claim.Type}: {claim.Value}");
                }

                // Trusted: app_metadata is service-role only. Allow-list keys we actually use.
                FlattenJsonClaim(identity, "app_metadata",
                    allowedKeys: Program.AppMetadataAllowedKeys);

                // Untrusted: user_metadata is writable by the user. Namespace it so it can NEVER
                // collide with an auth-relevant claim name like "role".
                FlattenJsonClaim(identity, "user_metadata", prefix: "umeta:");
                return Task.CompletedTask;
            }
        };
    });

// --- Authorization ---
builder.Services.AddSingleton<IAuthorizationHandler, DbRoleAuthorizationHandler>();
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("AdminOnly", policy =>
    {
        policy.RequireAuthenticatedUser();
        // Layer 1: JWT claim must say Admin (sourced from app_metadata only).
        policy.RequireClaim("role", "Admin");
        // Layer 2: DB row in public.app_users must also say Admin.
        policy.Requirements.Add(new DbRoleRequirement("Admin"));
    });

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

static void FlattenJsonClaim(
    ClaimsIdentity identity,
    string sourceClaim,
    IReadOnlySet<string>? allowedKeys = null,
    string? prefix = null)
{
    var raw = identity.FindFirst(sourceClaim)?.Value;
    if (string.IsNullOrEmpty(raw)) return;
    try
    {
        using var doc = JsonDocument.Parse(raw);
        foreach (var prop in doc.RootElement.EnumerateObject())
        {
            if (allowedKeys is not null && !allowedKeys.Contains(prop.Name)) continue;

            var claimType = prefix is null ? prop.Name : prefix + prop.Name;

            // Never let a later source overwrite an existing claim.
            if (identity.HasClaim(c => c.Type == claimType)) continue;

            var value = prop.Value.ValueKind switch
            {
                JsonValueKind.String => prop.Value.GetString(),
                _ => prop.Value.GetRawText(),
            };
            if (value is not null)
                identity.AddClaim(new Claim(claimType, value));
        }
    }
    catch { /* malformed metadata — skip */ }
}

public partial class Program
{
    // Keys allowed to be projected from the trusted `app_metadata` JSON claim into
    // top-level claims. Add to this set only after a security review.
    internal static readonly IReadOnlySet<string> AppMetadataAllowedKeys =
        new HashSet<string>(StringComparer.Ordinal) { "role", "roles", "tenant_id", "provider" };
}
