// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Linq;

namespace PSRule.Configuration
{
    /// <summary>
    /// A suppression rule, that specifies TargetNames that will not be processed by individual rules.
    /// </summary>
    public sealed class SuppressionRule
    {
        public SuppressionRule()
        {

        }

        private SuppressionRule(string[] targetNames)
        {
            TargetName = targetNames;
        }

        /// <summary>
        /// One of more target names to suppress.
        /// </summary>
        public string[] TargetName { get; set; }

        public static implicit operator SuppressionRule(string value)
        {
            return FromString(value);
        }

        public static implicit operator SuppressionRule(string[] value)
        {
            return FromString(value);
        }

        internal static SuppressionRule FromString(params string[] value)
        {
            return new SuppressionRule(value);
        }

        internal static SuppressionRule FromObject(object value)
        {
            if (value is string)
            {
                return FromString(value.ToString());
            }

            if (value.GetType().IsArray)
            {
                return FromString(((object[])value).OfType<string>().ToArray());
            }

            return null;
        }
    }
}
