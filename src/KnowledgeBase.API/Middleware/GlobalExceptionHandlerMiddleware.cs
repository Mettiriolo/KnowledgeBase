using System.Net;
using System.Text.Json;
using KnowledgeBase.API.Exceptions;

namespace KnowledgeBase.API.Middleware
{
    public class GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        private static readonly JsonSerializerOptions CachedJsonSerializerOptions = new()
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
            var response = new ErrorResponse();

            switch (exception)
            {
                case NotFoundException notFoundException:
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    response.Message = notFoundException.Message;
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    break;

                case BadRequestException badRequestException:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Message = badRequestException.Message;
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    break;

                case ConflictException conflictException:
                    context.Response.StatusCode = (int)HttpStatusCode.Conflict;
                    response.Message = conflictException.Message;
                    response.StatusCode = (int)HttpStatusCode.Conflict;
                    break;

                case UnauthorizedAccessException unauthorizedException:
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    response.Message = unauthorizedException.Message;
                    response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    break;

                default:
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    response.Message = "服务器内部错误";
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    break;
            }

            var jsonResponse = JsonSerializer.Serialize(response, CachedJsonSerializerOptions);

            await context.Response.WriteAsync(jsonResponse);
        }
    }

    public class ErrorResponse
    {
        public string Message { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}