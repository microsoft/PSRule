using System.Management.Automation;

namespace PSRule.Pipeline
{
    public interface IStreamManager
    {
        void Process(PSObject targetObject);
    }
}
