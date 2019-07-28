using System.IO;

namespace PSRule.Rules
{
    internal static class RuleHelper
    {
        /// <summary>
        /// Checks each RuleName and converts each to a RuleId.
        /// </summary>
        /// <param name="ruleNames">An array of names. Qualified names (RuleIds) supplied are left intact.</param>
        /// <param name="sourcePath">A source path to use to qualify each name.</param>
        /// <returns>An array of RuleIds.</returns>
        public static string[] ExpandRuleName(string[] ruleNames, string sourcePath, string moduleName)
        {
            if (ruleNames == null)
            {
                return null;
            }

            var scriptFileName = Path.GetFileName(sourcePath);

            var result = new string[ruleNames.Length];
            ruleNames.CopyTo(result, 0);

            for (var i = 0; i < ruleNames.Length; i++)
            {
                if (ruleNames[i] == null)
                {
                    continue;
                }

                // The name is not already qualified
                if (ruleNames[i].IndexOf('\\') == -1)
                {
                    result[i] = ExpandRuleName(ruleName: ruleNames[i], scriptFileName: scriptFileName, moduleName: moduleName);
                }
            }

            return (result.Length == 0) ? null : result;
        }

        public static string ExpandRuleName(string ruleName, string scriptFileName, string moduleName)
        {
            return (moduleName == null) ? ruleName : string.Concat(moduleName, '\\', ruleName);
        }
    }
}
