using System;
using System.Collections;
using System.Collections.Generic;
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
    ///     2022/01 rewritten to use `dynamic` and reflection to find other
    ///     missing properties/fields (FileNotFoundException.FileName)
    /// </remarks>
    public class ExceptionXElement : XElement
    {
        private static Action<dynamic, string, XElement> insertAttribute =
            (dynamic val, string name, XElement xe) =>
            {
                xe.SetAttributeValue(name, val);
            };
        private static Action<dynamic, string, XElement> insertElement =
            (dynamic val, string name, XElement xe) =>
            {
                xe.Add(new XElement(name, val));
            };


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
                dynamic value = default;

                if (exception.Message != null)
                {
                    //root.Add(new XElement("Message", exception.Message));
                    value = exception.Message;
                    Recurse(value, nameof(exception.Message), root, insertAttribute);
                }

                // StackTrace can be null, e.g.:
                // new ExceptionAsXml(new Exception())
                if (!omitStackTrace && exception.StackTrace != null)
                {
                    //root.Add(
                    //    new XElement("StackTrace",
                    //    from frame in exception.StackTrace.Split('\n')
                    //    let prettierFrame = ((frame?.Length > 6) ? frame.Substring(6).Trim() : String.Empty)
                    //    select new XElement("Frame", prettierFrame))
                    //);
                    value = exception.StackTrace;
                    Recurse(value, nameof(exception.StackTrace), root, insertAttribute);
                }

                // Data is never null; it's empty if there is no data
                if (exception.Data.Count > 0)
                {
                    //root.Add(
                    //    new XElement("Data",
                    //        from entry in exception.Data.Cast<DictionaryEntry>()
                    //        let key = XmlConvert.EncodeName(entry.Key.ToString())
                    //        let value = (entry.Value?.ToString() ?? "null")
                    //        select new XElement(key, value))
                    //);
                    value = exception.Data;
                    Recurse(value, nameof(exception.Data), root, insertElement);
                }

                // Add the InnerException if it exists

                // If an AggregateException, take the other inner exceptions as well
                if (exception is AggregateException aggregate)
                {
                    //foreach (Exception ex in aggregate.InnerExceptions)
                    //{
                    //    root.Add(new ExceptionXElement(ex, omitStackTrace));
                    //}
                    value = aggregate.InnerExceptions;
                    Recurse((IEnumerable)value, nameof(aggregate.InnerExceptions), root, insertAttribute);
                }
                else if (exception.InnerException != null)
                {
                    //root.Add(new ExceptionXElement(exception.InnerException, omitStackTrace));
                    Recurse((dynamic)exception.InnerException, nameof(exception.InnerException), root, insertAttribute);
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

                    dynamic val = prop.GetValue(exception);
                    if (val != null)
                    {
                        Recurse(val, prop.Name, root, insertAttribute);
                    }
                }

                foreach (var field in exceptionType.GetFields())
                {
                    dynamic val = field.GetValue(exception);
                    if (val != null)
                    {
                        Recurse(val, field.Name, root, insertAttribute);
                    }
                }


                return root;
            })())
        {; }

        private static void Recurse(string val, string name, XElement xe, Action<dynamic, string, XElement> op)
        {
            op(val, name, xe);
        }

        private static void Recurse(int val, string name, XElement xe, Action<dynamic, string, XElement> op)
        {
            op(val, name, xe);
        }

        private static void Recurse(long val, string name, XElement xe, Action<dynamic, string, XElement> op)
        {
            op(val, name, xe);
        }

        private static void Recurse(bool val, string name, XElement xe, Action<dynamic, string, XElement> op)
        {
            op(val, name, xe);
        }

        private static void Recurse(char val, string name, XElement xe, Action<dynamic, string, XElement> op)
        {
            op(val, name, xe);
        }

        private static void Recurse(decimal val, string name, XElement xe, Action<dynamic, string, XElement> op)
        {
            op(val, name, xe);
        }

        private static void Recurse(float val, string name, XElement xe, Action<dynamic, string, XElement> op)
        {
            op(val, name, xe);
        }

        private static void Recurse(double val, string name, XElement xe, Action<dynamic, string, XElement> op)
        {
            op(val, name, xe);
        }

        private static void Recurse(IEnumerable val, string name, XElement xe, Action<dynamic, string, XElement> op)
        {
            XElement coll = new XElement(XmlConvert.EncodeName(name));

            foreach (dynamic entry in val)
            {
                Recurse((dynamic)(entry ?? "nil"), XmlConvert.EncodeName(entry.GetType().Name), coll, insertAttribute);
            }

            if (coll.HasElements) // don't add unless there are some
            {
                xe.Add(coll);
            }
        }

        private static void Recurse(IDictionary val, string name, XElement xe, Action<dynamic, string, XElement> op)
        {
            XElement dict = new XElement(XmlConvert.EncodeName(name));

            foreach (var entry in val.Cast<DictionaryEntry>())
            {
                Recurse((dynamic)(entry.Value ?? "nil"), XmlConvert.EncodeName(entry.Key.ToString()), dict, insertElement);
            }

            if (dict.HasElements) // don't add unless there are some
            {
                xe.Add(dict);
            }
        }

        private static void Recurse(object obj, string name, XElement xe, Action<dynamic, string, XElement> op)
        {
            if (string.IsNullOrEmpty(name))
            {
                name = XmlConvert.EncodeName(obj.GetType().FullName);
            }

            var elem = new XElement(name);

            foreach (var prop in obj.GetType().GetProperties())
            {
                if (prop.IsSpecialName)
                    break; // don't display specials
                dynamic val = prop.GetValue(obj);  // Must be dynamic for the double dispatch on the runtime type of the parameter
                if (val != null)
                {
                    Recurse(val, prop.Name, elem, op);
                }
            }

            foreach (var field in obj.GetType().GetFields())
            {
                if (field.IsSecurityCritical || field.IsSecuritySafeCritical || field.IsSpecialName)
                {
                    continue; // don't display security marked or specials
                }

                dynamic val = field.GetValue(obj);  //Must be dynamc for the double dispatch on the runtime type of the parameter
                if (val != null)
                {
                    Recurse(val, field.Name, elem, op);
                }
            }

            xe.Add(elem);
        }


        private static void Recurse(Exception obj, string name, XElement xe, Action<dynamic, string, XElement> op)

        {
            if (string.IsNullOrEmpty(name))
            {
                name = XmlConvert.EncodeName(obj.GetType().FullName);
            }

            var elem = new XElement(name);

            foreach (var prop in obj.GetType().GetProperties())
            {
                if (prop.IsSpecialName)
                    break; // don't display specials
                dynamic val = prop.GetValue(obj);  //Must be dynamc for the double dispatch on the runtime type of the parameter
                if (val != null)
                {
                    Recurse(val, prop.Name, elem, op);
                }
            }

            foreach (var field in obj.GetType().GetFields())
            {
                if (field.IsSecurityCritical || field.IsSecuritySafeCritical || field.IsSpecialName)
                {
                    continue; // don't display security marked or specials
                }

                dynamic val = field.GetValue(obj);  //Must be dynamc for the double dispatch on the runtime type of the parameter
                if (val != null)
                {
                    Recurse(val, field.Name, elem, op);
                }
            }

            xe.Add(elem);
        }

    }
}
