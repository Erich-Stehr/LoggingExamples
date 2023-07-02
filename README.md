# LoggingExamples
Examples using .NET logging libraries, including System.Diagnostics

LoggingHelpers library includes:

- StoryOfTraceSource: history and description of the original built-in .NET logging library System.Diagnostics
- ExceptionXElement: dynamic recreation of Exception derived classes into serializable form
- NavigateException: converts Exception into XmlNavigator object for use with XmlWriterTraceListener
- TextWriterTraceListener: wraps System.Diagnostics.TextWriterTraceListener constructors taking paths with System.Environment.ExpandEnvironmentVariables to allow destinations in the form of "%LOCALAPPDATA%\apptrace.log"