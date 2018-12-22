using System;
using System.Management.Automation;
using PSRule.Configuration;

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
                if (p.Value == null || !StringComparer.Ordinal.Equals(StringTypeName, p.TypeNameOfValue))
                {
                    continue;
                }

                if (StringComparer.OrdinalIgnoreCase.Equals(p.Name, Property_TargetName))
                {
                    return p.Value.ToString();
                }
                else if (StringComparer.OrdinalIgnoreCase.Equals(p.Name, Property_Name))
                {
                    targetName = p.Value.ToString();
                }
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
        public static string CustomTargetNameBinding(string[] propertyNames, PSObject targetObject, BindTargetName next)
        {
            string targetName = null;
            int score = int.MaxValue;

            foreach (var p in targetObject.Properties)
            {
                if (p.Value == null || !StringComparer.Ordinal.Equals(StringTypeName, p.TypeNameOfValue))
                {
                    continue;
                }

                for (var i = 0; i < propertyNames.Length && score > 0; i++)
                {
                    if (i < score && StringComparer.OrdinalIgnoreCase.Equals(p.Name, propertyNames[i]))
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
    }
}
