using PSRule.Annotations;
using PSRule.Rules;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;

namespace PSRule.Host
{
    internal sealed class HostHelper
    {
        public static IEnumerable<Rule> GetRule(LanguageContext context, string[] scriptPaths, RuleFilter filter)
        {
            return ToRule(GetLanguageBlock(context, scriptPaths), filter).Values.ToArray();
        }

        /// <summary>
        /// Called from PowerShell to get additional metdata from a language block, such as comment help.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="start"></param>
        /// <returns></returns>
        public static BlockMetadata GetCommentMeta(string path, int start)
        {
            var scriptBlockContent = File.ReadAllText(path, Encoding.UTF8);

            var errors = new Collection<PSParseError>();

            var tokens = PSParser.Tokenize(scriptBlockContent, out errors);

            if (tokens.Count == 0)
            {
                return new BlockMetadata();
            }

            var i = 0;
            var comments = new List<string>();

            for (var t = tokens[0]; i < tokens.Count && t.Start < start; i++)
            {
                t = tokens[i];

                if (t.Start == start)
                {
                    // If tokens was a new line back track so that all comments are found
                    if (i > 1 && tokens[i - 1].Type == PSTokenType.NewLine)
                    {
                        for (var j = i - 2; j >= 0 && tokens[j].Type == PSTokenType.Comment; j--)
                        {
                            comments.Insert(0, tokens[j].Content);
                        }
                    }
                }
            }

            var metadata = new BlockMetadata();

            // Check if any comments were found
            if (comments.Count > 0)
            {
                foreach (var comment in comments)
                {
                    if (comment.StartsWith("# Description: "))
                    {
                        metadata.Description = comment.Substring(15);
                    }
                }

            }

            return metadata;
        }

        /// <summary>
        /// Execute one or more PowerShell script files to get language blocks.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="scriptPaths"></param>
        /// <returns></returns>
        private static IEnumerable<ILanguageBlock> GetLanguageBlock(LanguageContext context, string[] scriptPaths)
        {
            var results = new Collection<ILanguageBlock>();

            var scopeContext = context.New();

            var state = HostState.CreateDefault();
            state.LanguageMode = PSLanguageMode.FullLanguage;

            var runspace = RunspaceFactory.CreateRunspace(state);
            runspace.ThreadOptions = PSThreadOptions.UseCurrentThread;

            runspace.Open();
            runspace.SessionStateProxy.PSVariable.Set(new EnvironmentVariable("Environment"));

            var ps = PowerShell.Create();

            ps.Runspace = runspace;

            foreach (var path in scriptPaths)
            {
                if (!File.Exists(path))
                {
                    throw new FileNotFoundException("The script was not found.", path);
                }

                ps.AddScript(path, true);
                var invokeResults = ps.Invoke();

                if (ps.HadErrors)
                {
                    throw new System.Exception(ps.Streams.Error[0].Exception.Message, ps.Streams.Error[0].Exception);
                }

                foreach (var ir in invokeResults)
                {
                    if (ir.BaseObject is ILanguageBlock)
                    {
                        var block = ir.BaseObject as ILanguageBlock;

                        results.Add(block);
                    }
                }
            }

            return results;
        }

        /// <summary>
        /// Convert matching langauge blocks to rules.
        /// </summary>
        /// <param name="blocks"></param>
        /// <param name="filter"></param>
        /// <returns></returns>
        private static IDictionary<string, Rule> ToRule(IEnumerable<ILanguageBlock> blocks, RuleFilter filter)
        {
            // Index deployments by environment/name
            var results = new Dictionary<string, Rule>(System.StringComparer.OrdinalIgnoreCase);

            foreach (var block in blocks.OfType<RuleBlock>())
            {
                // Ignore deployment blocks that don't match
                if (filter != null && !filter.Match(block))
                {
                    continue;
                }

                Rule rule = null;

                if (!results.ContainsKey(block.Name))
                {
                    rule = new Rule
                    {
                        Name = block.Name,
                        Description = block.Description
                    };

                    results[block.Name] = rule;
                }
                else
                {
                    rule = results[block.Name];
                }
            }

            return results;
        }
    }
}
