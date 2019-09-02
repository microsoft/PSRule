using PSRule.Configuration;
using PSRule.Rules;
using System;
using System.Management.Automation;
using System.Threading;

namespace PSRule.Pipeline
{
    public sealed class GetBaselinePipelineBuilder : PipelineBuilderBase
    {
        private Action<object, bool> _Output;
        private IPipelineStream _Stream;
        private readonly VisitTargetObject _VisitTargetObject;
        private string[] _Name;

        internal GetBaselinePipelineBuilder()
        {
            _Output = (r, b) => { };
            _VisitTargetObject = PipelineReceiverActions.PassThru;
        }

        /// <summary>
        /// Filter returned baselines by name.
        /// </summary>
        public new void Name(string[] name)
        {
            if (name == null || name.Length == 0)
                return;

            _Name = name;
        }

        public override void UseCommandRuntime(ICommandRuntime2 commandRuntime)
        {
            base.UseCommandRuntime(commandRuntime);
            _Output = commandRuntime.WriteObject;
        }

        public GetBaselinePipelineBuilder Configure(PSRuleOption option)
        {
            if (option == null)
                return this;

            _Option.Output.As = ResultFormat.Detail;
            _Option.Output.Culture = option.Output.Culture ?? new string[] { Thread.CurrentThread.CurrentCulture.ToString() };
            _Option.Output.Format = OutputFormat.None;

            return this;
        }

        public GetBaselinePipeline Build()
        {
            if (_Stream == null)
                _Stream = new PowerShellPipelineStream(option: _Option, output: _Output, returnBoolean: false, inputPath: null);

            var filter = new BaselineFilter(_Name);
            var context = PrepareContext(bindTargetName: null, bindTargetType: null);
            return new GetBaselinePipeline(
                streamManager: new StreamManager(option: _Option, stream: _Stream, input: _VisitTargetObject),
                context: context,
                source: GetSource(),
                filter: filter
            );
        }
    }
}
