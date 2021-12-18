using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Linq;

namespace LoggingHelpers
{
    /// <summary>
    /// Build a replacement Exception object that _can_ be XML serialized in
    /// place of an Exception. (XmlSerializer is too simple-minded to handle
    /// the IDictionary in ex.Data.)
    /// </summary>
    /// <remarks>
    ///     Originally from
    ///     http://seattlesoftware.wordpress.com/2008/08/22/serializing-exceptions-to-xml/ and
    ///     https://stackoverflow.com/questions/486460/how-to-serialize-an-exception-object-in-c
    ///     but extended to handle AggregateException and use EncodeName.
    ///     TODO: Needs more reflection to find other missing properties/fields
    ///       (System.IO.FileNotFoundException.FileName)
    /// </remarks>
    public class ExceptionXElement : XElement
    {
        public ExceptionXElement(Exception exception)
            : this(exception, false)
        {; }


        public ExceptionXElement(Exception exception, bool omitStackTrace)
            : base(new Func<XElement>(() =>
            {
            // Validate arguments
            if (exception == null)
            {
                throw new ArgumentNullException("exception");
            }

            // The root element is the Exception's type
            Type exceptionType = exception.GetType();
            XElement root = new XElement(XmlConvert.EncodeName(exceptionType.ToString()));
            if (exception.Message != null)
            {
                root.Add(new XElement("Message", exception.Message));
            }

            // StackTrace can be null, e.g.:
            // new ExceptionAsXml(new Exception())
            if (!omitStackTrace && exception.StackTrace != null)
            {
                root.Add(
                    new XElement("StackTrace",
                    from frame in exception.StackTrace.Split('\n')
                    let prettierFrame = ((frame?.Length > 6) ? frame.Substring(6).Trim() : String.Empty)
                    select new XElement("Frame", prettierFrame))
                );
            }

            // Data is never null; it's empty if there is no data
            if (exception.Data.Count > 0)
            {
                root.Add(
                    new XElement("Data",
                        from entry in exception.Data.Cast<DictionaryEntry>()
                        let key = XmlConvert.EncodeName(entry.Key.ToString())
                        let value = (entry.Value?.ToString() ?? "null")
                        select new XElement(key, value))
                );
            }

            // Add the InnerException if it exists

            // If an AggregateException, take the other inner exceptions as well
            if (exception is AggregateException aggregate)
            {
                foreach (Exception ex in aggregate.InnerExceptions)
                {
                    root.Add(new ExceptionXElement(ex, omitStackTrace));
                }
            }
            else if (exception.InnerException != null)
            {
                root.Add(new ExceptionXElement(exception.InnerException, omitStackTrace));
            }

            foreach (var prop in exceptionType.GetProperties())
            {
                if ((prop.Name == "Message") ||
                    (prop.Name == "StackTrace") ||
                    (prop.Name == "Data") ||
                    (prop.Name == "InnerException") ||
                    (prop.Name == "InnerExceptions")
                    )
                {
                    continue;
                }
                root.Add(RecurseProperty(prop, exception));
            }

            foreach (var field in exceptionType.GetFields())
            {
                root.Add(RecurseField(field, exception));
            }

            return root;
            })())
        {; }

        private static object RecurseField(FieldInfo field, Exception exception)
        {
            throw new NotImplementedException();
        }

        private static XElement RecurseProperty(PropertyInfo prop, object obj)
        {
            if (prop is ICollection icoll)
            {
                XElement coll = new XElement(XmlConvert.EncodeName(prop.Name));
                foreach (var elem in (ICollection)prop.GetValue(obj))
                {
                    coll.Add(RecurseProperty(prop, elem));
                }
                return coll;
            }
            else
            {
                return new XElement(XmlConvert.EncodeName(prop.Name), prop.GetValue(obj)?.ToString());
            }
        }
    }
}
