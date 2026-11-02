using System.Net;
using System.Runtime.ExceptionServices;
using System.Text.Json;
using FluentValidation;
using Grpc.Core;
using ProductStorage.Application.Exceptions;

namespace ProductStorage.Presentation.Middleware;

public class ErrorHandlingMiddleware(
    RequestDelegate next,
    ILogger<ErrorHandlingMiddleware> logger,
    IHostEnvironment environment)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
        {
            logger.LogDebug(
                "Request {Method} {Path} was canceled by the client",
                context.Request.Method,
                context.Request.Path);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var statusCode = GetStatusCode(exception);
        var detail = GetDetail(exception);

        logger.LogError(
            exception,
            "Request {Method} {Path} failed with status code {StatusCode}. TraceId: {TraceId}",
            context.Request.Method,
            context.Request.Path,
            statusCode,
            context.TraceIdentifier);

        if (context.Response.HasStarted)
        {
            ExceptionDispatchInfo.Capture(exception).Throw();
            return;
        }

        context.Response.Clear();
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var response = new ErrorResponse(
            statusCode,
            GetTitle(statusCode),
            detail,
            context.TraceIdentifier);

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions));
    }

    private string GetDetail(Exception exception) =>
        exception switch
        {
            ValidationException validationException =>
                string.Join("; ", validationException.Errors.Select(error => error.ErrorMessage)),
            ProductNotFoundException productNotFoundException => productNotFoundException.Message,
            RpcException rpcException => rpcException.Status.Detail,
            _ when environment.IsDevelopment() => exception.Message,
            _ => "Unexpected error"
        };

    private static int GetStatusCode(Exception exception) =>
        exception switch
        {
            ValidationException => (int)HttpStatusCode.BadRequest,
            ProductNotFoundException => (int)HttpStatusCode.NotFound,
            RpcException rpcException => rpcException.StatusCode switch
            {
                StatusCode.InvalidArgument => (int)HttpStatusCode.BadRequest,
                StatusCode.NotFound => (int)HttpStatusCode.NotFound,
                StatusCode.AlreadyExists => (int)HttpStatusCode.Conflict,
                StatusCode.Unauthenticated => (int)HttpStatusCode.Unauthorized,
                StatusCode.PermissionDenied => (int)HttpStatusCode.Forbidden,
                _ => (int)HttpStatusCode.InternalServerError
            },
            _ => (int)HttpStatusCode.InternalServerError
        };

    private static string GetTitle(int statusCode) =>
        statusCode switch
        {
            400 => "Bad Request",
            401 => "Unauthorized",
            403 => "Forbidden",
            404 => "Not Found",
            409 => "Conflict",
            _ => "Internal Server Error"
        };

    private record ErrorResponse(
        int StatusCode,
        string Title,
        string Detail,
        string TraceId);
}
