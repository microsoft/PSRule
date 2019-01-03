using System.Management.Automation;

namespace PSRule.Pipeline
{
    public delegate void MessageHook(string message);

    public delegate void ErrorRecordHook(ErrorRecord errorRecord);

    public interface ILogger
    {
        void WriteVerbose(string message);

        void WriteWarning(string message);

        void WriteError(ErrorRecord errorRecord);

        void WriteInformation(InformationRecord informationRecord);
    }
}
