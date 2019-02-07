using System;
using System.Management.Automation;

namespace PSRule
{
    internal static class PSObjectExtensions
    {
        public static T PropertyValue<T>(this PSObject value, string propertyName)
        {
            if (typeof(T).IsValueType)
            {
                return (T)Convert.ChangeType(value.Properties[propertyName].Value, typeof(T));
            }

            return (T)value.Properties[propertyName].Value;
        }
    }
}
