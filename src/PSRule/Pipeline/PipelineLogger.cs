using System;
using System.Management.Automation;

namespace PSRule.Pipeline
{
    internal sealed class PipelineLogger : ILogger
    {
        internal Action<string> OnWriteWarning;
        internal Action<string> OnWriteVerbose;
        internal Action<ErrorRecord> OnWriteError;

        public void WriteError(ErrorRecord errorRecord)
        {
            if (OnWriteError == null)
            {
                return;
            }

            OnWriteError(errorRecord);
        }

        public void WriteVerbose(string message)
        {
            if (OnWriteVerbose == null)
            {
                return;
            }

            OnWriteVerbose(message);
        }

        public void WriteWarning(string message)
        {
            if (OnWriteWarning == null)
            {
                return;
            }

            OnWriteWarning(message);
        }
    }
}
