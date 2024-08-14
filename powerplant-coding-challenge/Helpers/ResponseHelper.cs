using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace powerplant_coding_challenge.Helpers;

public static class ResponseHelper
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static async Task WriteProblemDetailsResponse(HttpContext context, ProblemDetails problemDetails)
    {
        context.Response.ContentType = "application/json";
        var responseString = JsonSerializer.Serialize(problemDetails, JsonSerializerOptions);
        await context.Response.WriteAsync(responseString);
    }
}