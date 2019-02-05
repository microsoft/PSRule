using System.Management.Automation;

namespace PSRule.Pipeline
{
    /// <summary>
    /// A stream that receives pipeline objects.
    /// </summary>
    public interface IPipelineStream
    {
        void Process(PSObject targetObject);
    }
}
