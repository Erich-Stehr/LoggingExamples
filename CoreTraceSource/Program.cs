using System;
using System.Diagnostics;

namespace CoreTraceSource
{
    class Program
    {
        private static readonly TraceSource ts = new TraceSource("CoreTraceSource", SourceLevels.Verbose | SourceLevels.ActivityTracing);
        static void Main(string[] args)
        {
            Trace.AutoFlush = true;
            ts.TraceEvent(TraceEventType.Verbose, 0, "Verbose message");
            Console.WriteLine($"{ts.Listeners.Count} listeners on TraceSource");
            ts.Listeners.Add(new XmlWriterTraceListener("CoreTraceSource.svclog", "xmlwriter"));
            ts.Listeners.Add(new LoggingHelpers.TextWriterTraceListener("%LOCALAPPDATA%\\CoreTraceSource.txt.log", "extendedTWTL"));

            try
            {
                // reminder: no stacktrace unless thrown
                var oorex = new ApplicationException("Application", new ArgumentOutOfRangeException("index", -1, "not at end"));
                oorex.Data["foo"] = "bar";
                AggregateException aggex = new AggregateException("both",
                    new System.IO.FileNotFoundException("missing file", "foo.bar"),
                    oorex
                );
                aggex.Data["m"] = 5.01m;
                aggex.Data["obj"] = new { title = "Anonymouse type", flag = true };
                ts.TraceData(TraceEventType.Critical, 1, DateTime.Now.ToString("o"), aggex);
                ts.TraceData(TraceEventType.Critical, 2, (new LoggingHelpers.ExceptionXElement(aggex)).ToString());
                ts.TraceData(TraceEventType.Critical, 3, LoggingHelpers.NavigateException.Navigate(aggex));
                ts.TraceData(TraceEventType.Critical, 4, System.Text.Json.JsonSerializer.Serialize(aggex));
            }
            finally
            {
                ts.Close();
            }
        }
    }
}
