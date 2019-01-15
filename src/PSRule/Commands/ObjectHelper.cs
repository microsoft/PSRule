using System;
using System.Collections;
using System.Collections.Generic;
using System.Management.Automation;
using System.Text;

namespace PSRule.Commands
{
    internal static class ObjectHelper
    {
        private sealed class NameToken
        {
            public string Name;

            public NameToken Next;
        }

        public static bool GetField(object targetObject, string name, bool caseSensitive, out object value)
        {
            var nameToken = GetNameTokens(name);

            var comparer = caseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase;

            return GetField(targetObject: targetObject, token: nameToken, comparer: comparer, value: out value);
        }

        private static bool GetField(object targetObject, NameToken token, StringComparer comparer, out object value)
        {
            var baseObject = GetBaseObject(targetObject);
            var baseType = baseObject.GetType();

            object field = null;
            bool foundField = false;

            // Handle dictionaries and hashtables
            if (typeof(IDictionary).IsAssignableFrom(baseType))
            {
                var dictionary = (IDictionary)baseObject;

                foreach (var k in dictionary.Keys)
                {
                    if (comparer.Equals(token.Name, k))
                    {
                        field = dictionary[k];
                        foundField = true;
                    }
                }
            }
            // Handle PSObjects
            else if (targetObject is PSObject)
            {
                var psobject = (PSObject)targetObject;

                foreach (var p in psobject.Properties)
                {
                    if (comparer.Equals(token.Name, p.Name))
                    {
                        field = p.Value;
                        foundField = true;
                    }
                }
            }
            // Handle all other CLR types
            else
            {
                foreach (var p in baseType.GetProperties())
                {
                    if (comparer.Equals(token.Name, p.Name))
                    {
                        field = p.GetValue(targetObject);
                        foundField = true;
                    }
                }

                if (!foundField)
                {
                    foreach (var p in baseType.GetFields())
                    {
                        if (comparer.Equals(token.Name, p.Name))
                        {
                            field = p.GetValue(targetObject);
                            foundField = true;
                        }
                    }
                }
            }

            if (foundField)
            {
                if (token.Next == null)
                {
                    value = field;
                    return true;
                }
                else
                {
                    return GetField(targetObject: field, token: token.Next, comparer: comparer, value: out value);
                }
            }

            value = null;
            return false;
        }

        private static NameToken GetNameTokens(string name)
        {
            var nameParts = name.Split('.');
            var token = new NameToken();
            var result = token;

            for (var i = 0; i < nameParts.Length; i++)
            {
                token.Name = nameParts[i];

                if (i + 1 < nameParts.Length)
                {
                    token.Next = new NameToken();
                    token = token.Next;
                }
            }

            return result;
        }

        private static object GetBaseObject(object value)
        {
            if (value == null)
            {
                return null;
            }

            if (value is PSObject)
            {
                var baseObject = ((PSObject)value).BaseObject;

                if (baseObject != null)
                {
                    return baseObject;
                }
            }

            return value;
        }
    }
}
