using Microsoft.EntityFrameworkCore;
using QRDrinkOrder.Shared.Exceptions;
using System.Net;
using System.Text.Json;

namespace QRDrinkOrder.API.Middlewares;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
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
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;
        response.ContentType = "application/json";

        var (statusCode, message) = exception switch
        {
            BusinessException ex => ((int)HttpStatusCode.BadRequest, ex.Message),
            DbUpdateConcurrencyException => ((int)HttpStatusCode.Conflict, "Dữ liệu đã bị thay đổi bởi người khác. Vui lòng thử lại."),
            _ => ((int)HttpStatusCode.InternalServerError, "Đã xảy ra lỗi hệ thống cục bộ. Vui lòng thử lại sau.")
        };

        if (statusCode == (int)HttpStatusCode.InternalServerError)
        {
            _logger.LogError(exception, "Unhandled Exception");
        }
        else
        {
            _logger.LogWarning(exception, "Business/Concurrency Exception: {Message}", message);
        }

        response.StatusCode = statusCode;

        var result = JsonSerializer.Serialize(new { Message = message });
        return response.WriteAsync(result);
    }
}
