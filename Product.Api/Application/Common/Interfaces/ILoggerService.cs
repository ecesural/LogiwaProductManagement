namespace Product.Api.Application.Common.Interfaces;

public interface ILoggerService<T>
{
    void LogInfo(string message);
    void LogWarning(string message);
    void LogError(string message, Exception ex);
}