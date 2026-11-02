using System.Diagnostics;
using Grpc.Core;
using Grpc.Core.Interceptors;

namespace ProductStorage.Presentation.Interceptors;

public class GrpcLoggingInterceptor(ILogger<GrpcLoggingInterceptor> logger) : Interceptor
{
    public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(
        TRequest request,
        ServerCallContext context,
        UnaryServerMethod<TRequest, TResponse> continuation)
    {
        var stopwatch = Stopwatch.StartNew();
        logger.LogInformation(
            "gRPC request {Method}: {@Request}",
            context.Method,
            request);

        try
        {
            var response = await continuation(request, context);

            logger.LogInformation(
                "gRPC response {Method} ({ElapsedMilliseconds} ms): {@Response}",
                context.Method,
                stopwatch.ElapsedMilliseconds,
                response);

            return response;
        }
        catch (RpcException exception)
        {
            logger.LogWarning(
                exception,
                "gRPC request {Method} failed with {StatusCode} ({ElapsedMilliseconds} ms): {Detail}",
                context.Method,
                exception.StatusCode,
                stopwatch.ElapsedMilliseconds,
                exception.Status.Detail);
            throw;
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "gRPC request {Method} failed ({ElapsedMilliseconds} ms)",
                context.Method,
                stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}
