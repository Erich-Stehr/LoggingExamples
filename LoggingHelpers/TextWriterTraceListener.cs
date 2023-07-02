using System;

namespace LoggingHelpers
{
    // To the extent possible under law, Erich Stehr has waived all copyright and related or neighboring rights to this work. This work is published from: United States.

    /// <summary>
    /// Wraps System.Diagnostics.TextWriterTraceListener constructors to expand
    /// environment variables in the paths. Use the original if you aren't
    /// passing a path to the constructor through the Full Framework
    /// configuration file.
    /// </summary>
    /// <remarks>
    /// Remove the namespace and compile into the executable and the @type
    /// becomes "TextWriterTraceListener".
    /// </remarks>
    /// <example>
    /// <configuration><system.diagnostics><sharedListeners><add name="shared.textExample" type="LoggingHelpers.TextWriterTraceListener" initializeData="%LOCALAPPDATA%\textExample.log"/></sharedListeners></system.diagnostics></configuration>
    /// </example>
    public class TextWriterTraceListener : System.Diagnostics.TextWriterTraceListener
    {
        public TextWriterTraceListener(string initializeData) : base(Environment.ExpandEnvironmentVariables(initializeData)) { }
        public TextWriterTraceListener(string initializeData, string name) : base(Environment.ExpandEnvironmentVariables(initializeData), name) { }
    }
}
