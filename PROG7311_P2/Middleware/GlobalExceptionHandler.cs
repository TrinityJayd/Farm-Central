using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using PROG7311_P2.Exceptions;
using System.Net;
using System.Text.Json;

namespace PROG7311_P2.Middleware
{
    public class GlobalExceptionHandler
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(RequestDelegate next, ILogger<GlobalExceptionHandler> logger)
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

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var response = new
            {
                error = new
                {
                    message = "An error occurred while processing your request.",
                    details = exception.Message
                }
            };

            switch (exception)
            {
                case ProductNotFoundException:
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    response = new
                    {
                        error = new
                        {
                            message = "Product not found.",
                            details = exception.Message
                        }
                    };
                    break;

                case ProductValidationException:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response = new
                    {
                        error = new
                        {
                            message = "Product validation failed.",
                            details = exception.Message
                        }
                    };
                    break;

                case PROG7311_P2.Exceptions.UnauthorizedAccessException:
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    response = new
                    {
                        error = new
                        {
                            message = "Unauthorized access.",
                            details = exception.Message
                        }
                    };
                    break;

                case DatabaseException:
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    _logger.LogError(exception, "Database error occurred");
                    response = new
                    {
                        error = new
                        {
                            message = "A database error occurred. Please try again later.",
                            details = "Internal server error"
                        }
                    };
                    break;

                default:
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    _logger.LogError(exception, "An unhandled exception occurred");
                    response = new
                    {
                        error = new
                        {
                            message = "An unexpected error occurred. Please try again later.",
                            details = "Internal server error"
                        }
                    };
                    break;
            }

            var jsonResponse = JsonSerializer.Serialize(response);
            await context.Response.WriteAsync(jsonResponse);
        }
    }
} 