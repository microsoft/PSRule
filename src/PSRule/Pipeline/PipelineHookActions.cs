using System;
using System.Management.Automation;

namespace PSRule.Pipeline
{
    internal static class PipelineHookActions
    {
        private const string StringTypeName = "System.String";
        private const string Property_TargetName = "TargetName";
        private const string Property_Name = "Name";

        /// <summary>
        /// Get the name of the object by looking for a TargetName or Name property.
        /// </summary>
        /// <param name="targetObject">A PSObject to bind.</param>
        /// <returns>The target name of the object.</returns>
        public static string DefaultBindTargetName(PSObject targetObject)
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
    }
}
