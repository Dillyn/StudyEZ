using Microsoft.AspNetCore.Mvc;
using studyez_backend.Core.Exceptions;

namespace StudyEZ_WebApi.Infrastructure
{
    public class ExceptionMappingMiddleware(RequestDelegate next, ILogger<ExceptionMappingMiddleware> log)
    {
        // Maps exceptions to HTTP responses
        public async Task Invoke(HttpContext ctx)
        {
            try { await next(ctx); }
            catch (AppException app)
            {
                var (status, title) = app switch
                {
                    NotFoundException => (StatusCodes.Status404NotFound, "Not Found"),
                    ConflictException => (StatusCodes.Status409Conflict, "Conflict"),
                    ValidationException => (StatusCodes.Status400BadRequest, "Validation Error"),
                    ForbiddenException => (StatusCodes.Status403Forbidden, "Forbidden"),
                    UnauthorizedException => (StatusCodes.Status401Unauthorized, "Unauthorized"),
                    BadRequestException => (StatusCodes.Status400BadRequest, "Bad Request"),
                    _ => (StatusCodes.Status400BadRequest, "Bad Request")
                };

                log.LogWarning(app, "Handled application error");
                ctx.Response.StatusCode = status;
                await ctx.Response.WriteAsJsonAsync(new ProblemDetails
                {
                    Status = status,
                    Title = title,
                    Detail = app.Message
                });
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Unhandled error");
                ctx.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await ctx.Response.WriteAsJsonAsync(new ProblemDetails
                {
                    Status = 500,
                    Title = "Server Error"
                });
            }
        }
    }
}
