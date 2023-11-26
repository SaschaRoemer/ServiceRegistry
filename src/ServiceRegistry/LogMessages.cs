using Microsoft.Extensions.Logging;

internal static partial class LogMessages
{
    [LoggerMessage(Message = "[{method}] {action}, Service: {service}", Level = LogLevel.Information)]
    internal static partial void LogService(this ILogger logger, string method, string action, Service? service);

    [LoggerMessage(Message = "[{method}] {action}, Service: {service}, Count: {count}", Level = LogLevel.Information )]
    internal static partial void LogService(this ILogger logger, string method, string action, Service? service, int count);
}