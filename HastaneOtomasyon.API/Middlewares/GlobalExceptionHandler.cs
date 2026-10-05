using System.Net;
using HastaneOtomasyon.Shared.DTOs.Common;
using Microsoft.AspNetCore.Diagnostics;

namespace HastaneOtomasyon.API.Middlewares
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;
        private readonly IHostEnvironment _environment;

        public GlobalExceptionHandler(
            ILogger<GlobalExceptionHandler> logger,
            IHostEnvironment environment)
        {
            _logger = logger;
            _environment = environment;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            _logger.LogError(
                exception,
                "Islenmeyen hata: {Message}",
                exception.Message);

            Console.WriteLine($"[HATA] {DateTime.UtcNow:O} - {exception.Message}");

            var (statusCode, message) = MapException(exception);

            httpContext.Response.StatusCode = (int)statusCode;
            httpContext.Response.ContentType = "application/json";

            var response = new ApiResponse<object>(
                success: false,
                message: message,
                data: null,
                errors: _environment.IsDevelopment()
                    ? new List<string> { exception.Message, exception.StackTrace ?? string.Empty }
                    : null);

            await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

            return true;
        }

        private (HttpStatusCode StatusCode, string Message) MapException(Exception exception)
        {
            return exception switch
            {
                ArgumentException => (HttpStatusCode.BadRequest, "Gecersiz istek parametresi."),
                KeyNotFoundException => (HttpStatusCode.NotFound, "Istenen kayit bulunamadi."),
                UnauthorizedAccessException => (HttpStatusCode.Unauthorized, "Bu islem icin yetkiniz yok."),
                _ => (
                    HttpStatusCode.InternalServerError,
                    _environment.IsDevelopment()
                        ? exception.Message
                        : "Sunucuda beklenmeyen bir hata olustu. Lutfen daha sonra tekrar deneyin.")
            };
        }
    }
}
