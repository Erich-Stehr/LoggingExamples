using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace CoreNLogFileJson
{
    public class Runner
    {
        private readonly ILogger<Runner> _logger;

        public Runner(ILogger<Runner> logger)
        {
            _logger = logger;
        }

        public void DoAction(string name)
        {
            _logger.LogDebug(20, "Doing hard work! {Action}", name);

            // reminder: no stacktrace unless thrown
            var oorex = new ArgumentOutOfRangeException("index", -1, "not at end");
            oorex.Data["foo"] = "bar";
            AggregateException aggex = new AggregateException("both",
                new System.IO.FileNotFoundException("missing file", "foo.bar"),
                oorex
            );
            _logger.LogCritical(aggex, "Message");

        }
    }
}
