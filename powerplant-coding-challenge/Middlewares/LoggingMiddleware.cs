using powerplant_coding_challenge.Helpers;

namespace powerplant_coding_challenge.Middlewares;

public class LoggingMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        // Log Request.
        await LoggingHelper.LogRequestAsync(context);

        // Capture the original response stream.
        var originalResponseBodyStream = context.Response.Body;
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        await next(context);

        // Log Response.
        await LoggingHelper.LogResponseAsync(context);

        // Copy the response back to the original stream.
        await responseBody.CopyToAsync(originalResponseBodyStream);
    }
}