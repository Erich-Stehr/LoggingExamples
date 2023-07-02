using System;
using System.Xml.XPath;

namespace LoggingHelpers
{
    /// <summary>
    /// XmlWriterTraceListener special-cases XmlNavigator objects for direct
    /// serialization into the element stream being output. NavigateException
    /// takes an exception and creates a read-only XPathNavigator object around
    /// an ExceptionXElement from that exception, allowing insertion.
    /// </summary>
    public static class NavigateException
    {
        public static XPathNavigator Navigate(Exception ex)
        {
            return (new ExceptionXElement(ex)).CreateNavigator();
        }

        public static XPathNavigator Navigate(Exception ex, bool omitStackTrace)
        {
            return (new ExceptionXElement(ex, omitStackTrace)).CreateNavigator();
        }
    }
}
