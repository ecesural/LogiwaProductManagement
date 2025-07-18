using MediatR;
using Product.Api.Application.Common.Interfaces;
namespace Product.Api.Application.Behaviors;

public class LoggingBehavior<TRequest, TResponse>(ILoggerService<TRequest> logger)
    : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
    {
        logger.LogInfo($"Handling {typeof(TRequest).Name}");
        var response = await next();
        logger.LogInfo($"Handled {typeof(TResponse).Name}");
        return response;
    }
}