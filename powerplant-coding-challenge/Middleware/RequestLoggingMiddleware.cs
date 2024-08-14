using Serilog;
using System.Diagnostics;
using System.Text.Json;

namespace powerplant_coding_challenge.Middleware;

public class RequestLoggingMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    // Static instance of JsonSerializerOptions to be reused
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();

        // Log Incoming Request
        var request = await FormatRequest(context.Request);
        Log.Information("Incoming Request: {Method} {Path}\nBody:\n{RequestBody}",
                        context.Request.Method, context.Request.Path, request);

        // Capture Response
        var originalBodyStream = context.Response.Body;
        await using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        // Process Request
        await _next(context);

        // Log Outgoing Response
        var response = await FormatResponse(context.Response);
        Log.Information("Outgoing Response:\n{ResponseBody}", response);

        // Log Completion Time
        stopwatch.Stop();
        Log.Information("Request processed in {ElapsedMilliseconds} ms\n", stopwatch.ElapsedMilliseconds);

        // Reset the body stream position so the response can be sent to the client
        await responseBody.CopyToAsync(originalBodyStream);
    }

    private static async Task<string> FormatRequest(HttpRequest request)
    {
        request.EnableBuffering();
        request.Body.Position = 0;
        using var reader = new StreamReader(request.Body, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        request.Body.Position = 0;

        return FormatJson(body);
    }

    private static async Task<string> FormatResponse(HttpResponse response)
    {
        response.Body.Seek(0, SeekOrigin.Begin);
        string text = await new StreamReader(response.Body).ReadToEndAsync();
        response.Body.Seek(0, SeekOrigin.Begin);

        return FormatJson(text);
    }

    private static string FormatJson(string json)
    {
        try
        {
            var jsonObject = JsonSerializer.Deserialize<object>(json);
            return JsonSerializer.Serialize(jsonObject, JsonOptions);
        }
        catch
        {
            // In case the body is not a valid JSON, return it as is
            return json;
        }
    }
}
