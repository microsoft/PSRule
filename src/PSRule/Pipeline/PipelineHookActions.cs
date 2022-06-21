// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Globalization;
using System.Linq;
using System.Management.Automation;
using System.Text;
using Newtonsoft.Json;
using PSRule.Configuration;
using PSRule.Data;
using PSRule.Runtime;

namespace PSRule.Pipeline
{
    internal delegate bool ShouldProcess(string target, string action);

    internal static class PipelineHookActions
    {
        private const string Property_TargetName = "TargetName";
        private const string Property_Name = "Name";

        public static string BindTargetName(string[] propertyNames, bool caseSensitive, bool preferTargetInfo, PSObject targetObject, out string path)
        {
            path = null;
            if (preferTargetInfo && TryGetInfoTargetName(targetObject, out var targetName))
                return targetName;

            if (propertyNames != null)
                return propertyNames.Any(n => n.Contains('.'))
                    ? NestedTargetPropertyBinding(propertyNames, caseSensitive, targetObject, DefaultTargetNameBinding, out path)
                    : CustomTargetPropertyBinding(propertyNames, caseSensitive, targetObject, DefaultTargetNameBinding, out path);

            return DefaultTargetNameBinding(targetObject);
        }

        public static string BindTargetType(string[] propertyNames, bool caseSensitive, bool preferTargetInfo, PSObject targetObject, out string path)
        {
            path = null;
            if (preferTargetInfo && TryGetInfoTargetType(targetObject, out var targetType))
                return targetType;

            if (propertyNames != null)
                return propertyNames.Any(n => n.Contains('.'))
                    ? NestedTargetPropertyBinding(propertyNames, caseSensitive, targetObject, DefaultTargetTypeBinding, out path)
                    : CustomTargetPropertyBinding(propertyNames, caseSensitive, targetObject, DefaultTargetTypeBinding, out path);

            return DefaultTargetTypeBinding(targetObject);
        }

        public static string BindField(string[] propertyNames, bool caseSensitive, bool preferTargetInfo, PSObject targetObject, out string path)
        {
            path = null;
            if (propertyNames != null)
                return propertyNames.Any(n => n.Contains('.'))
                    ? NestedTargetPropertyBinding(propertyNames, caseSensitive, targetObject, DefaultFieldBinding, out path)
                    : CustomTargetPropertyBinding(propertyNames, caseSensitive, targetObject, DefaultFieldBinding, out path);

            return DefaultFieldBinding(targetObject);
        }

        /// <summary>
        /// Get the TargetName of the object by looking for a TargetName or Name property.
        /// </summary>
        /// <param name="targetObject">A PSObject to bind.</param>
        /// <returns>The TargetName of the object.</returns>
        private static string DefaultTargetNameBinding(PSObject targetObject)
        {
            return TryGetInfoTargetName(targetObject, out var targetName) ||
                TryGetTargetName(targetObject, propertyName: Property_TargetName, targetName: out targetName) ||
                TryGetTargetName(targetObject, propertyName: Property_Name, targetName: out targetName)
                ? targetName
                : GetUnboundObjectTargetName(targetObject);
        }

        /// <summary>
        /// Get the TargetName of the object by using any of the specified property names.
        /// </summary>
        /// <param name="propertyNames">One or more property names to use to bind TargetName.</param>
        /// <param name="targetObject">A PSObject to bind.</param>
        /// <param name="next">The next delegate function to check if all of the property names can not be found.</param>
        /// <returns>The TargetName of the object.</returns>
        private static string CustomTargetPropertyBinding(string[] propertyNames, bool caseSensitive, PSObject targetObject, BindTargetName next, out string path)
        {
            path = null;
            string targetName = null;
            for (var i = 0; i < propertyNames.Length && targetName == null; i++)
            {
                targetName = targetObject.ValueAsString(propertyName: propertyNames[i], caseSensitive: caseSensitive);
                if (targetName != null)
                    path = propertyNames[i];
            }
            // If TargetName is found return, otherwise continue to next delegate
            return targetName ?? next(targetObject);
        }

        /// <summary>
        /// Get the TargetName of the object by using any of the specified property names.
        /// </summary>
        /// <param name="propertyNames">One or more property names to use to bind TargetName.</param>
        /// <param name="targetObject">A PSObject to bind.</param>
        /// <param name="next">The next delegate function to check if all of the property names can not be found.</param>
        /// <returns>The TargetName of the object.</returns>
        private static string NestedTargetPropertyBinding(string[] propertyNames, bool caseSensitive, PSObject targetObject, BindTargetName next, out string path)
        {
            path = null;
            string targetName = null;
            var score = int.MaxValue;
            for (var i = 0; i < propertyNames.Length && score > propertyNames.Length; i++)
            {
                if (ObjectHelper.GetPath(
                    bindingContext: PipelineContext.CurrentThread,
                    targetObject: targetObject,
                    path: propertyNames[i],
                    caseSensitive: caseSensitive,
                    value: out object value))
                {
                    path = propertyNames[i];
                    targetName = value.ToString();
                    score = i;
                }
            }
            // If TargetName is found return, otherwise continue to next delegate
            return targetName ?? next(targetObject);
        }

        /// <summary>
        /// Calculate a SHA1 hash for an object to use as TargetName.
        /// </summary>
        /// <param name="targetObject">A PSObject to hash.</param>
        /// <returns>The TargetName of the object.</returns>
        private static string GetUnboundObjectTargetName(PSObject targetObject)
        {
            var settings = new JsonSerializerSettings
            {
                Formatting = Formatting.None,
                TypeNameHandling = TypeNameHandling.None,
                MaxDepth = 1024,
                Culture = CultureInfo.InvariantCulture
            };

            settings.Converters.Insert(0, new PSObjectJsonConverter());
            var json = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(targetObject, settings));
            return PipelineContext.CurrentThread.ObjectHashAlgorithm.GetDigest(json);
        }

        /// <summary>
        /// Try to get TargetName from specified property.
        /// </summary>
        private static bool TryGetTargetName(PSObject targetObject, string propertyName, out string targetName)
        {
            targetName = targetObject.ValueAsString(propertyName, false);
            return targetName != null;
        }

        /// <summary>
        /// Get the TargetType by reading TypeNames of the PSObject.
        /// </summary>
        /// <param name="targetObject">A PSObject to bind.</param>
        /// <returns>The TargetObject of the object.</returns>
        private static string DefaultTargetTypeBinding(PSObject targetObject)
        {
            return TryGetInfoTargetType(targetObject, out var targetType) ? targetType : targetObject.TypeNames[0];
        }

        private static string DefaultFieldBinding(PSObject targetObject)
        {
            return null;
        }

        private static bool TryGetInfoTargetName(PSObject targetObject, out string targetName)
        {
            targetName = null;
            if (!(targetObject.BaseObject is ITargetInfo info))
                return false;

            targetName = info.TargetName;
            return true;
        }

        private static bool TryGetInfoTargetType(PSObject targetObject, out string targetType)
        {
            targetType = null;
            if (!(targetObject.BaseObject is ITargetInfo info))
                return false;

            targetType = info.TargetType;
            return true;
        }
    }
}
