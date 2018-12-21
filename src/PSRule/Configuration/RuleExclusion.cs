using System.Linq;

namespace PSRule.Configuration
{
    /// <summary>
    /// A rule exclusion.
    /// </summary>
    public sealed class RuleExclusion
    {
        public RuleExclusion()
        {

        }

        private RuleExclusion(string[] targetNames)
        {
            TargetName = targetNames;
        }

        /// <summary>
        /// One of more target names to exclude.
        /// </summary>
        public string[] TargetName { get; set; }

        public static implicit operator RuleExclusion(string value)
        {
            return FromString(value);
        }

        public static implicit operator RuleExclusion(string[] value)
        {
            return FromString(value);
        }

        internal static RuleExclusion FromString(params string[] value)
        {
            return new RuleExclusion(value);
        }

        internal static RuleExclusion FromObject(object value)
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
