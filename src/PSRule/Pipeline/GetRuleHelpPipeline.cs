using PSRule.Configuration;
using PSRule.Host;
using PSRule.Resources;
using PSRule.Rules;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Management.Automation;
using System.Threading;

namespace PSRule.Pipeline
{
    public interface IHelpPipelineBuilder : IPipelineBuilder
    {
        void Online();
    }

    internal sealed class GetRuleHelpPipelineBuilder : PipelineBuilderBase, IHelpPipelineBuilder
    {
        private bool _Online;

        internal GetRuleHelpPipelineBuilder(Source[] source)
            : base(source) { }

        public override IPipelineBuilder Configure(PSRuleOption option)
        {
            if (option == null)
            {
                return this;
            }

            Option.Execution.LanguageMode = option.Execution.LanguageMode ?? ExecutionOption.Default.LanguageMode;
            Option.Output.Culture = option.Output.Culture ?? new string[] { Thread.CurrentThread.CurrentCulture.ToString() };

            if (option.Rule != null)
            {
                Option.Rule = new RuleOption(option.Rule);
            }

            ConfigureLogger(Option);
            return this;
        }

        public void Online()
        {
            _Online = true;
        }

        public override IPipeline Build()
        {
            return new GetRuleHelpPipeline(PrepareContext(null, null), Source, PrepareReader(), PrepareWriter());
        }

        private sealed class HelpWriter : PipelineWriter
        {
            private readonly PipelineLogger _Logger;
            private readonly List<InvokeResult> _Result;
            private readonly LanguageMode _LanguageMode;
            private readonly bool _InSession;
            private readonly bool ShouldOutput;

            internal HelpWriter(WriteOutput output, LanguageMode languageMode, bool inSession, PipelineLogger logger, bool online)
                : base(output)
            {
                _Logger = logger;
                _Result = new List<InvokeResult>();
                _LanguageMode = languageMode;
                _InSession = inSession;
                ShouldOutput = !online;
            }

            public override void Write(object o, bool enumerate)
            {
                if (!(o is RuleHelpInfo[] result))
                {
                    base.Write(o, enumerate);
                    return;
                }

                if (result.Length == 1)
                {
                    if (ShouldOutput || !TryLaunchBrowser(result[0].GetOnlineHelpUri()))
                        base.Write(result[0], false);

                    return;
                }

                for (var i = 0; i < result.Length; i++)
                {
                    var pso = PSObject.AsPSObject(result[i]);
                    pso.TypeNames.Insert(0, "PSRule.Rules.RuleHelpInfo+Collection");
                    base.Write(pso, false);
                }
            }

            private bool TryLaunchBrowser(Uri uri)
            {
                return uri == null || TryProcess(uri.OriginalString) || TryConstrained(uri.OriginalString);
            }

            private bool TryConstrained(string uri)
            {
                _Logger.WriteObject(string.Format(PSRuleResources.LaunchBrowser, uri), false);
                return true;
            }

            private bool TryProcess(string uri)
            {
                if (_LanguageMode == LanguageMode.ConstrainedLanguage || _InSession)
                    return false;

                var browser = new Process();
                browser.StartInfo.FileName = uri;
                browser.StartInfo.UseShellExecute = true;
                return browser.Start();
            }
        }

        protected override PipelineWriter PrepareWriter()
        {
            return new HelpWriter(
                GetOutput(),
                Option.Execution.LanguageMode.GetValueOrDefault(ExecutionOption.Default.LanguageMode.Value),
                HostContext.InSession,
                Logger,
                _Online
            );
        }
    }

    internal sealed class GetRuleHelpPipeline : RulePipeline, IPipeline
    {
        internal GetRuleHelpPipeline(PipelineContext context, Source[] source, PipelineReader reader, PipelineWriter writer)
            : base(context, source, reader, writer)
        {
            // Do nothing
        }

        public override void End()
        {
            Writer.Write(HostHelper.GetRuleHelp(source: Source, context: Context), true);
        }
    }
}
