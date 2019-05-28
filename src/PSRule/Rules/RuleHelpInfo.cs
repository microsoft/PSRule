using System;
using System.Collections;

namespace PSRule.Rules
{
    /// <summary>
    /// Output view helper class for rule help.
    /// </summary>
    public sealed class RuleHelpInfo
    {
        private const string ONLINE_HELP_LINK_ANNOTATION = "online version";

        /// <summary>
        /// The name of the rule.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The synopsis of the rule.
        /// </summary>
        public string Synopsis { get; set; }

        /// <summary>
        /// The recommendation for the rule.
        /// </summary>
        public string Recommendation { get; set; }

        /// <summary>
        /// Additional notes for the rule.
        /// </summary>
        public string Notes { get; set; }

        /// <summary>
        /// Metadata annotations for the rule.
        /// </summary>
        public Hashtable Annotations { get; set; }

        /// <summary>
        /// Get the URI for the online version of the documentation.
        /// </summary>
        /// <returns>A URI when a valid link is set. Otherwise null is returned.</returns>
        public Uri GetOnlineHelpUri()
        {
            if (Annotations == null || !Annotations.ContainsKey(ONLINE_HELP_LINK_ANNOTATION))
            {
                return null;
            }

            if (Uri.TryCreate(Annotations[ONLINE_HELP_LINK_ANNOTATION].ToString(), UriKind.Absolute, out Uri result))
            {
                return result;
            }

            return null;
        }
    }
}
