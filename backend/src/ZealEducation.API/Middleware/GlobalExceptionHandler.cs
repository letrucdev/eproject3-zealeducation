using System.Net;
using System.Text.Json;
using FluentValidation;
using ZealEducation.Application.Common.Exceptions;

namespace ZealEducation.API.Middleware;

public class GlobalExceptionHandler(RequestDelegate next, ILogger<GlobalExceptionHandler> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, message) = exception switch
        {
            ValidationException validationEx => (
                HttpStatusCode.BadRequest,
                JsonSerializer.Serialize(new { errors = validationEx.Errors.Select(e => e.ErrorMessage) })
            ),
            NotFoundException => (HttpStatusCode.NotFound, JsonSerializer.Serialize(new { error = exception.Message })),
            _ => (HttpStatusCode.InternalServerError, JsonSerializer.Serialize(new { error = "An internal server error occurred." }))
        };

        context.Response.StatusCode = (int)statusCode;
        await context.Response.WriteAsync(message);
    }
}
