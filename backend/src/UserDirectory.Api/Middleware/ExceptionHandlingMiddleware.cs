using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using UserDirectory.Api.Contracts;

namespace UserDirectory.Api.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException validationException)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/json";

            var validationErrors = validationException.Errors
                .GroupBy(error => error.PropertyName)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(error => error.ErrorMessage).ToArray());

            await context.Response.WriteAsJsonAsync(new ValidationProblemDetails(validationErrors)
            {
                Title = "Validation failed.",
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unhandled exception occurred while processing request.");

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";

            await context.Response.WriteAsJsonAsync(new ErrorResponse(
                "An unexpected error occurred. Please try again later."));
        }
    }
}
