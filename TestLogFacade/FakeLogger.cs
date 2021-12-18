using System;
using System.Collections.Generic;

// Pulled from https://stackoverflow.com/questions/5646820/logger-wrapper-best-practice#5646876
// dateTime added 2021/10/13

public interface ILogger
{
    void Log(LogEntry entry);
}

//public sealed class ConsoleLogger : ILogger
//{
//    public void Log(LogEntry entry)
//}

public enum LoggingEventType { Debug, Information, Warning, Error, Fatal };

// Immutable DTO that contains the log information.
public struct LogEntry
{
    public DateTimeOffset DateTime { get; }
    public LoggingEventType Severity { get; }
    public string Message { get; }
    public Exception Exception { get; }

    public LogEntry(LoggingEventType severity, string msg, Exception ex = null)
    {
        if (msg is null) throw new ArgumentNullException("msg");
        if (msg == string.Empty) throw new ArgumentException("empty", "msg");

        this.DateTime = DateTimeOffset.Now;
        this.Severity = severity;
        this.Message = msg;
        this.Exception = ex;
    }
}

public static class LoggerExtensions
{
    public static void Log(this ILogger logger, string message) =>
        logger.Log(new LogEntry(LoggingEventType.Information, message));

    public static void Log(this ILogger logger, Exception ex) =>
        logger.Log(new LogEntry(LoggingEventType.Error, ex.Message, ex));

    // More methods here.
}

public class ConsoleLogger : ILogger
{
    public void Log(LogEntry entry) => Console.WriteLine(
      $"[{entry.Severity}] {DateTime.Now} {entry.Message} {entry.Exception}");
}

public class FakeLogger : List<LogEntry>, ILogger
{
    public void Log(LogEntry entry) => this.Add(entry);
}

