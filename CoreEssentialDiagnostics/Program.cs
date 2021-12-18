using Essential.Diagnostics;
using System;
using System.Diagnostics;

namespace CoreEssentialDiagnostics
{
    class Program
    {
        static void Main(string[] args)
        {
            Trace.AutoFlush = true;
            var ts = new TraceSource("CoreEssentialDiagnostics", SourceLevels.All);
            ts.Listeners.Add(new RollingXmlTraceListener("{ApplicationName}-{DateTime:yyyy-MM-dd}.rolling.svclog"));
            ts.Listeners.Add(new ColoredConsoleTraceListener());

            ts.TraceInformation("Hello World!");
        }
    }
}
