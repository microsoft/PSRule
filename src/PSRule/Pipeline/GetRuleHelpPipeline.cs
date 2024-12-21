// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;
using System.Management.Automation;
using PSRule.Configuration;
using PSRule.Host;
using PSRule.Options;
using PSRule.Resources;
using PSRule.Rules;

namespace PSRule.Pipeline;

internal sealed class GetRuleHelpPipelineBuilder : PipelineBuilderBase, IHelpPipelineBuilder
{
    private bool _Full;
    private bool _Online;

    internal GetRuleHelpPipelineBuilder(Source[] source, IHostContext hostContext)
        : base(source, hostContext) { }

    /// <inheritdoc/>
    public override IPipelineBuilder Configure(PSRuleOption option)
    {
        if (option == null)
            return this;

        Option.Execution = GetExecutionOption(option.Execution);
        Option.Output.Culture = GetCulture(option.Output.Culture);

        if (option.Rule != null)
            Option.Rule = new RuleOption(option.Rule);

        return this;
    }

    /// <inheritdoc/>
    public void Full()
    {
        _Full = true;
    }

    /// <inheritdoc/>
    public void Online()
    {
        _Online = true;
    }

    /// <inheritdoc/>
    public override IPipeline Build(IPipelineWriter writer = null)
    {
        return new GetRuleHelpPipeline(
            pipeline: PrepareContext(PipelineHookActions.Empty, writer: writer ?? PrepareWriter()),
            source: Source
        );
    }

    private sealed class HelpWriter : PipelineWriter
    {
        private const string OUTPUT_TYPENAME_FULL = "PSRule.Rules.RuleHelpInfo+Full";
        private const string OUTPUT_TYPENAME_COLLECTION = "PSRule.Rules.RuleHelpInfo+Collection";

        private readonly LanguageMode _LanguageMode;
        private readonly bool _InSession;
        private readonly bool _ShouldOutput;
        private readonly string _TypeName;

        internal HelpWriter(PipelineWriter inner, PSRuleOption option, ShouldProcess shouldProcess, LanguageMode languageMode, bool inSession, bool online, bool full)
            : base(inner, option, shouldProcess)
        {
            _LanguageMode = languageMode;
            _InSession = inSession;
            _ShouldOutput = !online;
            _TypeName = full ? OUTPUT_TYPENAME_FULL : null;
        }

        public override void WriteObject(object sendToPipeline, bool enumerateCollection)
        {
            if (sendToPipeline is not RuleHelpInfo[] result)
            {
                base.WriteObject(sendToPipeline, enumerateCollection);
                return;
            }
            if (result.Length == 1)
            {
                if (_ShouldOutput || !TryLaunchBrowser(result[0].GetOnlineHelpUri()))
                    WriteHelpInfo(result[0], _TypeName);

                return;
            }

            for (var i = 0; i < result.Length; i++)
                WriteHelpInfo(result[i], OUTPUT_TYPENAME_COLLECTION);
        }

        private bool TryLaunchBrowser(Uri uri)
        {
            return uri == null || TryProcess(uri.OriginalString) || TryConstrained(uri.OriginalString);
        }

        private bool TryConstrained(string uri)
        {
            base.WriteObject(string.Format(Thread.CurrentThread.CurrentCulture, PSRuleResources.LaunchBrowser, uri), false);
            return true;
        }

        private bool TryProcess(string uri)
        {
            if (_LanguageMode == LanguageMode.ConstrainedLanguage || _InSession)
                return false;

            var browser = new Process();
            try
            {
                browser.StartInfo.FileName = uri;
                browser.StartInfo.UseShellExecute = true;
                return browser.Start();
            }
            finally
            {
                browser.Dispose();
            }
        }

        private void WriteHelpInfo(object o, string typeName)
        {
            if (typeName == null)
            {
                base.WriteObject(o, false);
                return;
            }
            var pso = PSObject.AsPSObject(o);
            pso.TypeNames.Insert(0, typeName);
            base.WriteObject(pso, false);
        }
    }

    protected override PipelineWriter PrepareWriter()
    {
        return new HelpWriter(
            inner: GetOutput(),
            option: Option,
            shouldProcess: ShouldProcess,
            languageMode: Option.Execution.LanguageMode.GetValueOrDefault(ExecutionOption.Default.LanguageMode.Value),
            inSession: InSession,
            online: _Online,
            full: _Full
        );
    }
}

internal sealed class GetRuleHelpPipeline : RulePipeline, IPipeline
{
    internal GetRuleHelpPipeline(PipelineContext pipeline, Source[] source)
        : base(pipeline, source)
    {
        // Do nothing
    }

    public override void End()
    {
        Pipeline.Writer.WriteObject(HostHelper.GetRuleHelp(Context), true);
    }
}
