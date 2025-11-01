using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Serilog;

namespace universal_payment_platform.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                // Log incoming request
                Log.Information("Handling request {Method} {Path}", context.Request.Method, context.Request.Path);

                await _next(context); // Call next middleware

                stopwatch.Stop();
                Log.Information("Finished handling request {Method} {Path} in {ElapsedMilliseconds}ms with status {StatusCode}",
                    context.Request.Method,
                    context.Request.Path,
                    stopwatch.ElapsedMilliseconds,
                    context.Response.StatusCode);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                Log.Error(ex, "Error handling request {Method} {Path} after {ElapsedMilliseconds}ms",
                    context.Request.Method,
                    context.Request.Path,
                    stopwatch.ElapsedMilliseconds);
                throw; // Re-throw to let ExceptionHandlerMiddleware handle it
            }
        }
    }
}
