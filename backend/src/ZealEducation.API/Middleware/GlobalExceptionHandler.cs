using System.Net;
using System.Text.Json;
using FluentValidation;
using ZealEducation.API.Common.Models;
using ZealEducation.Application.Common.Exceptions;

namespace ZealEducation.API.Middleware;

public class GlobalExceptionHandler(RequestDelegate next, ILogger<GlobalExceptionHandler> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

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

        var (statusCode, payload) = exception switch
        {
            ValidationException validationEx => (
                HttpStatusCode.BadRequest,
                JsonSerializer.Serialize(
                    ApiResponse<object>.Error("Validation failed",
                        new { errors = validationEx.Errors.Select(e => e.ErrorMessage) }),
                    JsonOptions)
            ),
            NotFoundException => (
                HttpStatusCode.NotFound,
                JsonSerializer.Serialize(ApiResponse<object>.Error(exception.Message), JsonOptions)
            ),
            UnauthorizedException => (
                HttpStatusCode.Unauthorized,
                JsonSerializer.Serialize(ApiResponse<object>.Error(exception.Message), JsonOptions)
            ),
            ConflictException => (
                HttpStatusCode.Conflict,
                JsonSerializer.Serialize(ApiResponse<object>.Error(exception.Message), JsonOptions)
            ),
            _ => (
                HttpStatusCode.InternalServerError,
                JsonSerializer.Serialize(ApiResponse<object>.Error("An internal server error occurred."), JsonOptions)
            )
        };

        context.Response.StatusCode = (int)statusCode;
        await context.Response.WriteAsync(payload);
    }
}
