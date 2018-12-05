using PSRule.Annotations;
using PSRule.Configuration;
using PSRule.Pipeline;
using PSRule.Rules;
using System;
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
        public static IEnumerable<Rule> GetRule(PSRuleOption option, LanguageContext context, string[] scriptPaths, RuleFilter filter)
        {
            return ToRule(GetLanguageBlock(option, context, scriptPaths), filter).Values.ToArray();
        }

        public static DependencyGraph<RuleBlock> GetRuleBlockGraph(PSRuleOption option, LanguageContext context, string[] scriptPaths, RuleFilter filter)
        {
            var builder = new DependencyGraphBuilder<RuleBlock>();
            builder.Include(items: GetLanguageBlock(option, context, scriptPaths).OfType<RuleBlock>(), filter: (b) => filter == null || filter.Match(b));
            return builder.Build();
        }

        /// <summary>
        /// Called from PowerShell to get additional metdata from a language block, such as comment help.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="start"></param>
        /// <returns></returns>
        public static BlockMetadata GetCommentMeta(string path, int lineNumber, int offset)
        {
            if (lineNumber == 1)
            {
                return new BlockMetadata();
            }

            var lines = File.ReadAllLines(path, Encoding.UTF8);

            var i = lineNumber - 1;
            var comments = new List<string>();

            for (; i >= 0; i--)
            {
                if (lines[i].Contains("#"))
                {
                    comments.Insert(0, lines[i]);
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
        private static IEnumerable<ILanguageBlock> GetLanguageBlock(PSRuleOption option, LanguageContext context, string[] scriptPaths)
        {
            var results = new Collection<ILanguageBlock>();
            var state = HostState.CreateDefault();

            // Set PowerShell language mode
            state.LanguageMode = option.Execution.LanguageMode == LanguageMode.FullLanguage ? PSLanguageMode.FullLanguage : PSLanguageMode.ConstrainedLanguage;

            // Configure runspace
            var runspace = RunspaceFactory.CreateRunspace(state);
            runspace.ThreadOptions = PSThreadOptions.UseCurrentThread;

            Runspace.DefaultRunspace = runspace;

            runspace.Open();
            runspace.SessionStateProxy.PSVariable.Set(new RuleVariable("Rule"));
            runspace.SessionStateProxy.PSVariable.Set(new TargetObjectVariable("TargetObject"));

            var ps = PowerShell.Create();

            ps.Runspace = runspace;

            // Process scripts

            foreach (var path in scriptPaths)
            {
                if (!File.Exists(path))
                {
                    throw new FileNotFoundException("The script was not found.", path);
                }

                PipelineContext.WriteVerbose($"[PSRule][D] -- Scanning: {path}");

                if (!File.Exists(path))
                {
                    throw new FileNotFoundException("Can't find file", path);
                }

                ps.AddScript(path, true);
                var invokeResults = ps.Invoke();

                if (ps.HadErrors)
                {
                    throw new Exception(ps.Streams.Error[0].Exception.Message, ps.Streams.Error[0].Exception);
                }

                foreach (var ir in invokeResults)
                {
                    if (ir.BaseObject is ILanguageBlock)
                    {
                        var block = ir.BaseObject as ILanguageBlock;
                        block.SourcePath = path;

                        results.Add(block);
                    }
                }
            }

            return results;
        }

        public static DetailResult InvokeRuleBlock(PSRuleOption option, RuleBlock block, PSObject inputObject)
        {
            try
            {
                //PipelineContext.WriteVerbose($"[PSRule][R][{block.Id}]::BEGIN");

                var result = new DetailResult(block.Id)
                {
                    TargetObject = inputObject,
                    TargetName = BindName(inputObject),
                    Tag = block.Tag?.ToHashtable()
                };

                LanguageContext._Rule = result;

                if (block.If != null)
                {
                    if (!block.If.Invoke())
                    {
                        //result.Status = RuleResultOutcome.Skipped;
                        return result;
                    }
                }

                result.Status = RuleOutcome.InProgress;

                var invokeResults = block.Body.Invoke();

                if (invokeResults == null)
                {
                    result.Status = RuleOutcome.Inconclusive;
                }
                else
                {
                    result.Success = invokeResults.Success;
                    result.Status = result.Success ? RuleOutcome.Passed : RuleOutcome.Failed;
                }

                PipelineContext.WriteVerbose($"[PSRule][R][{block.Id}] -- [{result.Status}]");

                return result;
            }
            finally
            {
                LanguageContext._Rule = null;
            }
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

                if (!results.ContainsKey(block.Id))
                {
                    rule = new Rule
                    {
                        Name = block.Name,
                        Description = block.Description
                    };

                    results[block.Id] = rule;
                }
                else
                {
                    rule = results[block.Id];
                }
            }

            return results;
        }

        /// <summary>
        /// Get the name of the object by looking for a TargetName or Name property.
        /// </summary>
        /// <param name="targetObject">A PSObject to bind.</param>
        /// <returns>The target name of the object.</returns>
        private static string BindName(PSObject targetObject)
        {
            string result = null;

            var comparer = StringComparer.OrdinalIgnoreCase;

            foreach (var p in targetObject.Properties)
            {
                if (comparer.Equals(p.Name, "TargetName"))
                {
                    result = targetObject.Properties[p.Name].Value?.ToString();
                }
                else if (comparer.Equals(p.Name, "Name") && result == null)
                {
                    result = targetObject.Properties[p.Name].Value?.ToString();
                }
            }

            return result;
        }
    }
}
