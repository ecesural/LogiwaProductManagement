using Product.Api.Application.Common.Interfaces;

namespace Product.Api.Infrastructure.Logging;

public class LoggerService<T>(ILogger<T> logger) : ILoggerService<T>
{
    public void LogInfo(string message) => logger.LogInformation(message);

    public void LogWarning(string message) => logger.LogWarning(message);

    public void LogError(string message, Exception ex) => logger.LogError(ex, message);
}