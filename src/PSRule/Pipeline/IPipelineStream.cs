using PSRule.Rules;
using System.Collections.Generic;
using System.Management.Automation;

namespace PSRule.Pipeline
{
    /// <summary>
    /// A stream that receives pipeline objects.
    /// </summary>
    public interface IPipelineStream
    {
        IStreamManager Manager { get; set; }

        void Begin();

        void Process(PSObject targetObject);

        void End(IEnumerable<RuleSummaryRecord> summary);

        void Output(InvokeResult result);
    }
}
