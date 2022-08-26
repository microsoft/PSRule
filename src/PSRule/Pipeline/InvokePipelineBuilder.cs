// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Configuration;
using PSRule.Data;

namespace PSRule.Pipeline
{
    /// <summary>
    /// A helper to build a pipeline for executing rules and conventions within a PSRule sandbox.
    /// </summary>
    public interface IInvokePipelineBuilder : IPipelineBuilder
    {
        /// <summary>
        /// Configures paths that will be scanned for input.
        /// </summary>
        /// <param name="path">An array of relative or absolute path specs to be scanned. Directories will be recursively scanned for all files not excluded matching the file path spec.</param>
        void InputPath(string[] path);

        /// <summary>
        /// Configures a variable that will recieve all results in addition to the host context.
        /// </summary>
        /// <param name="variableName">The name of the variable to set.</param>
        void ResultVariable(string variableName);
    }

    internal abstract class InvokePipelineBuilderBase : PipelineBuilderBase, IInvokePipelineBuilder
    {
        protected InputFileInfo[] _InputPath;
        protected string _ResultVariableName;

        protected InvokePipelineBuilderBase(Source[] source, IHostContext hostContext)
            : base(source, hostContext)
        {
            _InputPath = null;
        }

        public void InputPath(string[] path)
        {
            if (path == null || path.Length == 0)
                return;

            var builder = new InputPathBuilder(GetOutput(), PSRuleOption.GetWorkingPath(), "*", GetInputFilter());
            builder.Add(path);
            _InputPath = builder.Build();
        }

        public void ResultVariable(string variableName)
        {
            _ResultVariableName = variableName;
        }

        public override IPipelineBuilder Configure(PSRuleOption option)
        {
            if (option == null)
                return this;

            base.Configure(option);

            Option.Logging.RuleFail = option.Logging.RuleFail ?? LoggingOption.Default.RuleFail;
            Option.Logging.RulePass = option.Logging.RulePass ?? LoggingOption.Default.RulePass;
            Option.Logging.LimitVerbose = option.Logging.LimitVerbose;
            Option.Logging.LimitDebug = option.Logging.LimitDebug;

            Option.Output.As = option.Output.As ?? OutputOption.Default.As;
            Option.Output.Culture = GetCulture(option.Output.Culture);
            Option.Output.Encoding = option.Output.Encoding ?? OutputOption.Default.Encoding;
            Option.Output.Format = option.Output.Format ?? OutputOption.Default.Format;
            Option.Output.Path = option.Output.Path ?? OutputOption.Default.Path;
            Option.Output.JsonIndent = NormalizeJsonIndentRange(option.Output.JsonIndent);

            if (option.Rule != null)
                Option.Rule = new RuleOption(option.Rule);

            if (option.Configuration != null)
                Option.Configuration = new ConfigurationOption(option.Configuration);

            ConfigureBinding(option);
            Option.Requires = new RequiresOption(option.Requires);
            if (option.Suppression.Count > 0)
                Option.Suppression = new SuppressionOption(option.Suppression);

            return this;
        }

        public override IPipeline Build(IPipelineWriter writer = null)
        {
            return !RequireModules() || !RequireSources()
                ? null
                : (IPipeline)new InvokeRulePipeline(PrepareContext(BindTargetNameHook, BindTargetTypeHook, BindFieldHook), Source, writer ?? PrepareWriter(), Option.Output.Outcome.Value);
        }

        protected override PipelineReader PrepareReader()
        {
            if (!string.IsNullOrEmpty(Option.Input.ObjectPath))
            {
                AddVisitTargetObjectAction((sourceObject, next) =>
                {
                    return PipelineReceiverActions.ReadObjectPath(sourceObject, next, Option.Input.ObjectPath, true);
                });
            }

            if (Option.Input.Format == InputFormat.Yaml)
            {
                AddVisitTargetObjectAction((sourceObject, next) =>
                {
                    return PipelineReceiverActions.ConvertFromYaml(sourceObject, next);
                });
            }
            else if (Option.Input.Format == InputFormat.Json)
            {
                AddVisitTargetObjectAction((sourceObject, next) =>
                {
                    return PipelineReceiverActions.ConvertFromJson(sourceObject, next);
                });
            }
            else if (Option.Input.Format == InputFormat.Markdown)
            {
                AddVisitTargetObjectAction((sourceObject, next) =>
                {
                    return PipelineReceiverActions.ConvertFromMarkdown(sourceObject, next);
                });
            }
            else if (Option.Input.Format == InputFormat.PowerShellData)
            {
                AddVisitTargetObjectAction((sourceObject, next) =>
                {
                    return PipelineReceiverActions.ConvertFromPowerShellData(sourceObject, next);
                });
            }
            else if (Option.Input.Format == InputFormat.File)
            {
                AddVisitTargetObjectAction((sourceObject, next) =>
                {
                    return PipelineReceiverActions.ConvertFromGitHead(sourceObject, next);
                });
            }
            else if (Option.Input.Format == InputFormat.Detect && _InputPath != null)
            {
                AddVisitTargetObjectAction((sourceObject, next) =>
                {
                    return PipelineReceiverActions.DetectInputFormat(sourceObject, next);
                });
            }
            return new PipelineReader(VisitTargetObject, _InputPath, GetInputObjectSourceFilter());
        }
    }

    /// <summary>
    /// A helper to construct the pipeline for Invoke-PSRule.
    /// </summary>
    internal sealed class InvokeRulePipelineBuilder : InvokePipelineBuilderBase
    {
        internal InvokeRulePipelineBuilder(Source[] source, IHostContext hostContext)
            : base(source, hostContext) { }
    }
}
