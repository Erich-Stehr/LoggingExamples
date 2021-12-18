using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Formatting.Compact;
using System;

namespace CoreSerilogFileJson
{
    class Program
    {
        static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .Enrich.FromLogContext()
                .Enrich.WithExceptionDetails()
                .WriteTo.Console()
                //.WriteTo.RollingFile(new RenderedCompactJsonFormatter(), "CoreSerilogFileJson.log.json")
                .WriteTo.Seq(Environment.GetEnvironmentVariable("SEQ_URL") ?? "http://localhost:5341")
                .CreateLogger();

            var myLog = Log.ForContext<Program>();

            try
            {
                var oorex = new ArgumentOutOfRangeException("index", -1, "not at end");
                oorex.Data["foo"] = "bar";
                AggregateException aggex = new AggregateException("both",
                    new System.IO.FileNotFoundException("missing file", "foo.bar"),
                    oorex
                );

                throw aggex;
            }
            catch (Exception ex)
            {
                myLog.Fatal(ex, "Terminating");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}
