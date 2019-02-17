using Newtonsoft.Json;
using PSRule.Configuration;
using PSRule.Runtime;
using System;
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using System.Text;

namespace PSRule.Pipeline
{
    internal static class PipelineHookActions
    {
        private const string StringTypeName = "System.String";
        private const string Property_TargetName = "TargetName";
        private const string Property_Name = "Name";

        /// <summary>
        /// Get the TargetName of the object by looking for a TargetName or Name property.
        /// </summary>
        /// <param name="targetObject">A PSObject to bind.</param>
        /// <returns>The TargetName of the object.</returns>
        public static string DefaultTargetNameBinding(PSObject targetObject)
        {
            string targetName = null;

            foreach (var p in targetObject.Properties)
            {
                if (ShouldSkipBindingProperty(p))
                {
                    continue;
                }

                if (p.Name[0] == 't' || p.Name[0] == 'T' || p.Name[0] == 'n' || p.Name[0] == 'N')
                {
                    if (StringComparer.OrdinalIgnoreCase.Equals(p.Name, Property_TargetName))
                    {
                        return p.Value.ToString();
                    }
                    else if (StringComparer.OrdinalIgnoreCase.Equals(p.Name, Property_Name))
                    {
                        targetName = p.Value.ToString();
                    }
                }
            }

            if (targetName == null)
            {
                return GetUnboundObjectTargetName(targetObject);
            }

            return targetName;
        }

        /// <summary>
        /// Get the TargetName of the object by using any of the specified property names.
        /// </summary>
        /// <param name="propertyNames">One or more property names to use to bind TargetName.</param>
        /// <param name="targetObject">A PSObject to bind.</param>
        /// <param name="next">The next delegate function to check if all of the property names can not be found.</param>
        /// <returns>The TargetName of the object.</returns>
        public static string CustomTargetNameBinding(string[] propertyNames, bool caseSensitive, PSObject targetObject, BindTargetName next)
        {
            string targetName = null;
            int score = int.MaxValue;

            var comparer = caseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase;

            foreach (var p in targetObject.Properties)
            {
                if (ShouldSkipBindingProperty(p))
                {
                    continue;
                }

                for (var i = 0; i < propertyNames.Length && score > 0; i++)
                {
                    if (i < score && comparer.Equals(p.Name, propertyNames[i]))
                    {
                        targetName = p.Value.ToString();
                        score = i;
                    }
                }

                if (score == 0)
                {
                    break;
                }
            }

            // If TargetName is found return, otherwise continue to next delegate
            return (targetName == null) ? next(targetObject) : targetName;
        }

        /// <summary>
        /// Get the TargetName of the object by using any of the specified property names.
        /// </summary>
        /// <param name="propertyNames">One or more property names to use to bind TargetName.</param>
        /// <param name="targetObject">A PSObject to bind.</param>
        /// <param name="next">The next delegate function to check if all of the property names can not be found.</param>
        /// <returns>The TargetName of the object.</returns>
        public static string NestedTargetNameBinding(string[] propertyNames, bool caseSensitive, PSObject targetObject, BindTargetName next)
        {
            string targetName = null;
            int score = int.MaxValue;

            for (var i = 0; i < propertyNames.Length && score > propertyNames.Length; i++)
            {
                if (ObjectHelper.GetField(targetObject: targetObject, name: propertyNames[i], caseSensitive: caseSensitive, value: out object value))
                {
                    targetName = value.ToString();
                    score = i;
                }
            }

            // If TargetName is found return, otherwise continue to next delegate
            return (targetName == null) ? next(targetObject) : targetName;
        }

        /// <summary>
        /// Calculate a SHA1 hash for an object to use as TargetName.
        /// </summary>
        /// <param name="targetObject">A PSObject to hash.</param>
        /// <returns>The TargetName of the object.</returns>
        private static string GetUnboundObjectTargetName(PSObject targetObject)
        {
            var settings = new JsonSerializerSettings { Formatting = Formatting.None, TypeNameHandling = TypeNameHandling.None, MaxDepth = 1024, Culture = CultureInfo.InvariantCulture };
            settings.Converters.Insert(0, new PSObjectJsonConverter());
            var json = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(targetObject, settings));
            var hash = PipelineContext.CurrentThread.ObjectHashAlgorithm.ComputeHash(json);
            return string.Join("", hash.Select(b => b.ToString("x2")).ToArray());
        }

        /// <summary>
        /// Only consider properties that are strings with a value set.
        /// </summary>
        private static bool ShouldSkipBindingProperty(PSPropertyInfo propertyInfo)
        {
            return (!propertyInfo.IsGettable || propertyInfo.Value == null || !StringComparer.Ordinal.Equals(StringTypeName, propertyInfo.TypeNameOfValue));
        }
    }
}
