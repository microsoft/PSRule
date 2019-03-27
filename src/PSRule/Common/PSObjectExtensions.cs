using System;
using System.Management.Automation;

namespace PSRule
{
    internal static class PSObjectExtensions
    {
        public static T PropertyValue<T>(this PSObject o, string propertyName)
        {
            if (typeof(T).IsValueType)
            {
                return (T)Convert.ChangeType(o.Properties[propertyName].Value, typeof(T));
            }

            return (T)o.Properties[propertyName].Value;
        }

        public static string ValueAsString(this PSObject o, string propertyName, bool caseSensitive)
        {
            var p = o.Properties[propertyName];

            if (p == null)
            {
                return null;
            }

            if (caseSensitive && !StringComparer.Ordinal.Equals(p.Name, propertyName))
            {
                return null;
            }

            return p.Value.ToString();
        }

        public static bool PropertyValue(this PSObject o, string propertyName, bool caseSensitive, out object value)
        {
            value = null;
            var p = o.Properties[propertyName];

            if (p == null)
            {
                return false;
            }

            if (caseSensitive && !StringComparer.Ordinal.Equals(p.Name, propertyName))
            {
                return false;
            }

            value = p.Value;
            return true;
        }
    }
}
