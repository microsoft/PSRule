// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Newtonsoft.Json;
using PSRule.Runtime;
using System;
using System.Collections;
using System.Globalization;
using System.Management.Automation;

namespace PSRule
{
    internal static class PSObjectExtensions
    {
        public static T PropertyValue<T>(this PSObject o, string propertyName)
        {
            if (o.BaseObject is Hashtable hashtable)
                return ConvertValue<T>(hashtable[propertyName]);

            return ConvertValue<T>(o.Properties[propertyName].Value);
        }

        public static PSObject PropertyValue(this PSObject o, string propertyName)
        {
            if (o.BaseObject is Hashtable hashtable)
                return PSObject.AsPSObject(hashtable[propertyName]);

            return PSObject.AsPSObject(o.Properties[propertyName].Value);
        }

        public static string ValueAsString(this PSObject o, string propertyName, bool caseSensitive)
        {
            return ObjectHelper.GetField(o, propertyName, caseSensitive, out object value) && value != null ? value.ToString() : null;
        }

        public static bool HasProperty(this PSObject o, string propertyName)
        {
            return o.Properties[propertyName] != null;
        }

        /// <summary>
        /// Determines if the PSObject has any note properties.
        /// </summary>
        public static bool HasNoteProperty(this PSObject o)
        {
            foreach (var property in o.Properties)
            {
                if (property.MemberType == PSMemberTypes.NoteProperty)
                    return true;
            }
            return false;
        }

        public static string ToJson(this PSObject o)
        {
            var settings = new JsonSerializerSettings { Formatting = Formatting.None, TypeNameHandling = TypeNameHandling.None, MaxDepth = 1024, Culture = CultureInfo.InvariantCulture };
            settings.Converters.Insert(0, new PSObjectJsonConverter());
            return JsonConvert.SerializeObject(o, settings);
        }

        private static T ConvertValue<T>(object value)
        {
            if (value == null)
                return default;

            return typeof(T).IsValueType ? (T)Convert.ChangeType(value, typeof(T)) : (T)value;
        }
    }
}
