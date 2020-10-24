// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Configuration;
using PSRule.Host;
using PSRule.Resources;
using PSRule.Rules;
using System;
using System.Diagnostics;
using System.Management.Automation;
using System.Threading;

namespace PSRule.Pipeline
{
    public interface IHelpPipelineBuilder : IPipelineBuilder
    {
        void Full();

        void Online();
    }

    internal sealed class GetRuleHelpPipelineBuilder : PipelineBuilderBase, IHelpPipelineBuilder
    {
        private bool _Full;
        private bool _Online;

        internal GetRuleHelpPipelineBuilder(Source[] source, HostContext hostContext)
            : base(source, hostContext) { }

        public override IPipelineBuilder Configure(PSRuleOption option)
        {
            if (option == null)
                return this;

            Option.Execution.LanguageMode = option.Execution.LanguageMode ?? ExecutionOption.Default.LanguageMode;
            Option.Output.Culture = GetCulture(option.Output.Culture);

            if (option.Rule != null)
                Option.Rule = new RuleOption(option.Rule);

            return this;
        }
        public void Full()
        {
            _Full = true;
        }

        public void Online()
        {
            _Online = true;
        }
        
        public override IPipeline Build()
        {
            return new GetRuleHelpPipeline(PrepareContext(null, null, null), Source, PrepareReader(), PrepareWriter());
        }

        private sealed class HelpWriter : PipelineWriter
        {
            private const string OUTPUT_TYPENAME_FULL = "PSRule.Rules.RuleHelpInfo+Full";
            private const string OUTPUT_TYPENAME_COLLECTION = "PSRule.Rules.RuleHelpInfo+Collection";

            private readonly LanguageMode _LanguageMode;
            private readonly bool _InSession;
            private readonly bool _ShouldOutput;
            private readonly string _TypeName;

            internal HelpWriter(PipelineWriter inner, PSRuleOption option, LanguageMode languageMode, bool inSession, bool online, bool full)
                : base(inner, option)
            {
                _LanguageMode = languageMode;
                _InSession = inSession;
                _ShouldOutput = !online;
                _TypeName = full ? OUTPUT_TYPENAME_FULL : null;
            }

            public override void WriteObject(object sendToPipeline, bool enumerateCollection)
            {
                if (!(sendToPipeline is RuleHelpInfo[] result))
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
                languageMode: Option.Execution.LanguageMode.GetValueOrDefault(ExecutionOption.Default.LanguageMode.Value),
                inSession: HostContext.InSession,
                online: _Online,
                full: _Full
            );
        }
    }

    internal sealed class GetRuleHelpPipeline : RulePipeline, IPipeline
    {
        internal GetRuleHelpPipeline(PipelineContext pipeline, Source[] source, PipelineReader reader, PipelineWriter writer)
            : base(pipeline, source, reader, writer)
        {
            // Do nothing
        }

        public override void End()
        {
            Writer.WriteObject(HostHelper.GetRuleHelp(Source, Context), true);
        }
    }
}
