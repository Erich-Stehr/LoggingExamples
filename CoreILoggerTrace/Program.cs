using System;
using System.Diagnostics;
// using Microsoft.Extensions.DependencyInjection;  // Can't do it from 3+ without Host per https://github.com/aspnet/Announcements/issues/353
using Microsoft.Extensions.Logging;
using LoggingHelpers;
using System.Reflection;

namespace CoreILoggerTrace
{
    class Program
    {
        //static void Main(string[] args)
        //{

        //    var services = new ServiceCollection();
        //    services.AddLogging(logBuilder =>
        //    {
        //        logBuilder.AddConsole();
        //        logBuilder.AddTraceSource(new SourceSwitch("CoreILoggerTrace", SourceLevels.All.ToString()), xwtl);
        //    })
        //    .Configure<LoggerFilterOptions>(options => options.MinLevel = LogLevel.Trace)
        //    .AddTransient<IWorker, CoreILoggerTraceWorker>()
        //    ;
        //    var provider = services.BuildServiceProvider();

        //    ILogger l = provider.GetService<ILogger<Program>>();
        //    IWorker w = provider.GetService<IWorker>();
        //       w.DoWork();
        //}

        static void Main(string[] args)
        {
            Trace.AutoFlush = true;
            var xwtl = new XmlWriterTraceListener(".\\CoreILoggerTrace.svclog", "xwtl");
            ILoggerFactory _loggerFactory = LoggerFactory.Create(logBuilder =>
                logBuilder.AddConsole()
                .AddTraceSource(new SourceSwitch("CoreILoggerTrace", SourceLevels.All.ToString()), xwtl)
                .SetMinimumLevel(LogLevel.Trace)
            );

            try
            {
                var worker = new CoreILoggerTraceWorker(_loggerFactory.CreateLogger<CoreILoggerTraceWorker>());
                worker.DoWork();
            }
            finally
            {
                _loggerFactory.Dispose();
            }
        }

    }
    interface IWorker
    {
        void DoWork();
    }

    public class CoreILoggerTraceWorker : IWorker
    {
        ILogger logger = null;
        public CoreILoggerTraceWorker(ILogger logger)
        {
            this.logger = logger;
        }

        public void DoWork()
        {
            logger.LogTrace(0, "Trace entry");
            logger.LogDebug(1, "Debug entry");
            logger.LogInformation(2, "Information entry");
            logger.LogWarning(3, "Warning entry");
            logger.LogError(4, "Error entry");
            var ex = new System.IO.FileNotFoundException("Not there", "foo.bar");
            logger.LogCritical(5, ex, "Logging FileNotFoundException");
            logger.LogCritical(6, ex, "Logging FileNotFoundException (nav): {nav}", LoggingHelpers.NavigateException.Navigate(ex));
            logger.LogCritical(7, ex, "Logging FileNotFoundException (json): {json}", System.Text.Json.JsonSerializer.Serialize((Exception)ex));
            logger.LogCritical(8, ex, "Logging {exceptionType} (json): {json}",
                ex.GetType().FullName,
                ((string)System.Text.Json.JsonSerializer.Serialize((dynamic)ex).ToString()) );
        }
    }
}
