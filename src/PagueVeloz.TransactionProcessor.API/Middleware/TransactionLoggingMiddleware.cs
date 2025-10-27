using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Serilog;
using Serilog.Context;

namespace PagueVeloz.TransactionProcessor.API.Middleware;

public class TransactionLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TransactionLoggingMiddleware> _logger;

    public TransactionLoggingMiddleware(RequestDelegate next, ILogger<TransactionLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var correlationId = Guid.NewGuid().ToString();
        
        using (LogContext.PushProperty("CorrelationId", correlationId))
        using (LogContext.PushProperty("RequestPath", context.Request.Path))
        using (LogContext.PushProperty("RequestMethod", context.Request.Method))
        using (LogContext.PushProperty("UserAgent", context.Request.Headers.UserAgent.ToString()))
        using (LogContext.PushProperty("RemoteIpAddress", context.Connection.RemoteIpAddress?.ToString()))
        {
            _logger.LogInformation("Request started: {Method} {Path}", 
                context.Request.Method, context.Request.Path);

            try
            {
                await _next(context);
                
                stopwatch.Stop();
                
                _logger.LogInformation("Request completed: {Method} {Path} - Status: {StatusCode} - Duration: {Duration}ms",
                    context.Request.Method, 
                    context.Request.Path, 
                    context.Response.StatusCode,
                    stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                
                _logger.LogError(ex, "Request failed: {Method} {Path} - Status: {StatusCode} - Duration: {Duration}ms",
                    context.Request.Method, 
                    context.Request.Path, 
                    context.Response.StatusCode,
                    stopwatch.ElapsedMilliseconds);
                
                throw;
            }
        }
    }
}
