using System;
using System.Collections.Generic;
using System.Text;

namespace LoggingHelpers
{
    // To the extent possible under law, Erich Stehr has waived all copyright and related or neighboring rights to this work. This work is published from: United States.

    public class TextWriterTraceListener : System.Diagnostics.TextWriterTraceListener
    {
        public TextWriterTraceListener(string initializeData) : base(Environment.ExpandEnvironmentVariables(initializeData)) { }
        public TextWriterTraceListener(string initializeData, string name) : base(Environment.ExpandEnvironmentVariables(initializeData), name) { }
    }
}
