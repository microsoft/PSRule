using System.Management.Automation;

namespace PSRule.Pipeline
{
    public delegate void MessageHook(string message);

    public delegate void ErrorRecordHook(ErrorRecord errorRecord);

    public interface ILogger
    {
        void WriteVerbose(string message);

        bool ShouldWriteVerbose();

        void WriteWarning(string message);

        bool ShouldWriteWarning();

        void WriteError(ErrorRecord errorRecord);

        bool ShouldWriteError();

        void WriteInformation(InformationRecord informationRecord);

        void WriteHost(HostInformationMessage info);

        bool ShouldWriteInformation();

        void WriteDebug(DebugRecord debugRecord);

        bool ShouldWriteDebug();

        void EnterScope(string scopeName);

        void ExitScope();
    }
}
