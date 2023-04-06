using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using VirtualFinlandDevelopment.Shared.Exceptions;

namespace VirtualFinlandDevelopment.Shared.Middlewares;

public static class ErrorHandlerExtensions
{
    public static IApplicationBuilder UseErrorHandlerMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ErrorHandlerMiddleware>();
    }
}

public class ErrorHandlerMiddleware
{
    private readonly ILogger<ErrorHandlerMiddleware> _logger;
    private readonly RequestDelegate _next;

    public ErrorHandlerMiddleware(RequestDelegate next, ILogger<ErrorHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception error)
        {
            _logger.LogError(error, "Request processing failure!");

            var response = context.Response;
            response.ContentType = "application/json";

            var validationErrorDetails = new Dictionary<string, List<string>>();

            switch (error)
            {
                case NotAuthorizedException:
                    response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    break;
                case NotFoundException:
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    break;
                case BadRequestException e:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    e.ValidationErrors?.ForEach(
                        o => validationErrorDetails.Add(o.Field, new List<string> { o.Message }));
                    break;
                default:
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    break;
            }

            var errorResponseDetails = validationErrorDetails.Count == 0
                ? new ProblemDetails
                {
                    Type = "https://tools.ietf.org/html/rfc7231",
                    Title = error.Message,
                    Detail = error.Message,
                    Status = response.StatusCode,
                    Instance = response.HttpContext.Request.Path
                }
                : new ProblemDetails
                {
                    Type = "https://tools.ietf.org/html/rfc7231",
                    Title = error.Message,
                    Detail = error.Message,
                    Status = response.StatusCode,
                    Instance = response.HttpContext.Request.Path,
                    Extensions = { new KeyValuePair<string, object?>("errors", validationErrorDetails) }
                };

            var result = JsonSerializer.Serialize(errorResponseDetails,
                new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true
                });

            await response.WriteAsync(result);
        }
    }
}
