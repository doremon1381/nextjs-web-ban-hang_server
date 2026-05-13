using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using NhanViet.Application.Common.Exceptions;
using NhanViet.Domain.Exceptions;

namespace NhanViet.Api.Middleware;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, problem) = exception switch
        {
            NotFoundException e => (404, new ProblemDetails
            {
                Title = "Not Found",
                Detail = e.Message,
                Status = 404,
            }),
            ValidationException e => (400, new ProblemDetails
            {
                Title = "Validation Error",
                Detail = "One or more validation errors occurred.",
                Status = 400,
                Extensions = { ["errors"] = e.Errors.Select(f => new { f.PropertyName, f.ErrorMessage }) },
            }),
            DomainException e => (422, new ProblemDetails
            {
                Title = "Business Rule Violation",
                Detail = e.Message,
                Status = 422,
            }),
            UnauthorizedAccessException => (401, new ProblemDetails
            {
                Title = "Unauthorized",
                Status = 401,
            }),
            _ => (500, new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An unexpected error occurred.",
                Status = 500,
            }),
        };

        if (statusCode == 500)
            logger.LogError(exception, "Unhandled exception");

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(problem);
    }
}
