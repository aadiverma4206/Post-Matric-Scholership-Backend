using System.Net;
using System.Text.Json;
using Scholarship.Api.Exceptions;
using Scholarship.Api.Helpers;

namespace Scholarship.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
        context.Response.ContentType = "application/json";

        var response = new ApiResponse<object>
        {
            Success = false
        };

        if (exception is ValidationAppException valEx)
        {
            context.Response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;
            response.Message = valEx.Message;
            response.Errors = valEx.Errors;
        }
        else if (exception is NotFoundException notFoundEx)
        {
            context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            response.Message = notFoundEx.Message;
            response.Errors = new List<string> { notFoundEx.Message };
        }
        else if (exception is UnauthorizedAppException unauthEx)
        {
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            response.Message = unauthEx.Message;
            response.Errors = new List<string> { unauthEx.Message };
        }
        else if (exception is ForbiddenAppException forbidEx)
        {
            context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            response.Message = forbidEx.Message;
            response.Errors = new List<string> { forbidEx.Message };
        }
        else if (exception is AppException appEx)
        {
            context.Response.StatusCode = appEx.StatusCode;
            response.Message = appEx.Message;
            response.Errors = new List<string> { appEx.Message };
        }
        else
        {
            _logger.LogError(exception, "Unhandled system error during request: {Path}", context.Request.Path);
            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            response.Message = "An unexpected server error occurred. Please try again later.";
            response.Errors = new List<string> { "Internal Server Error" };
        }

        string result = JsonSerializer.Serialize(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        return context.Response.WriteAsync(result);
    }
}
