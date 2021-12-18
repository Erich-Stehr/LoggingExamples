using System;
using System.Xml.XPath;

namespace LoggingHelpers
{
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
