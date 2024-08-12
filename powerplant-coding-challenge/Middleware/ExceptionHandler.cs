using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Serilog.Context;
using System.Text.Json;

namespace powerplant_coding_challenge.Middleware;

public class ExceptionHandler(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task Invoke(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (ValidationException ex)
        {
            await HandleValidationExceptionAsync(httpContext, ex);
        }
        catch (Exception ex)
        {
            using (LogContext.PushProperty("RequestMethod", httpContext.Request.Method))
            using (LogContext.PushProperty("RequestPath", httpContext.Request.Path))
            {
                if (httpContext.Response.HasStarted)
                {
                    Log.Warning("The response has already started, the error page middleware will not be executed.");
                    throw;
                }

                Log.Error(ex, "An unhandled exception has been captured");
                await HandleExceptionAsync(httpContext);
            }
        }
    }

    private static async Task HandleValidationExceptionAsync(HttpContext context, ValidationException exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = StatusCodes.Status400BadRequest;

        var validationErrors = exception.Errors.Select(e => new
        {
            Field = e.PropertyName,
            Error = e.ErrorMessage
        });

        var problemDetails = new ValidationProblemDetails
        {
            Status = context.Response.StatusCode,
            Title = "Validation Failed",
            Detail = "One or more validation errors occurred.",
            Instance = context.Request.Path,
            Errors = validationErrors.ToDictionary(e => e.Field, e => new[] { e.Error })
        };

        var responseString = JsonSerializer.Serialize(problemDetails, _jsonSerializerOptions);
        await context.Response.WriteAsync(responseString);
    }

    private static async Task HandleExceptionAsync(HttpContext context)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;

        var problemDetails = new ProblemDetails
        {
            Status = context.Response.StatusCode,
            Title = "An error occurred",
            Detail = "An unexpected error occurred. Please try again later.",
            Instance = context.Request.Path
        };

        try
        {
            var responseString = JsonSerializer.Serialize(problemDetails, _jsonSerializerOptions);
            await context.Response.WriteAsync(responseString);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred while serializing the error response");
            await context.Response.WriteAsync("{\"status\":500,\"title\":\"Critical error\",\"detail\":\"A critical error occurred.\"}");
        }
    }
}
