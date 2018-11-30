using System;
using System.Management.Automation;

namespace PSRule.Pipeline
{
    internal sealed class PipelineContext
    {
        [ThreadStatic]
        internal static PipelineContext CurrentThread;

        private readonly ILogger _Logger;

        private PipelineContext(ILogger logger)
        {
            _Logger = logger;
        }

        public static PipelineContext New(ILogger logger)
        {
            var context = new PipelineContext(logger);
            CurrentThread = context;
            return context;
        }

        public static void WriteError(ErrorRecord errorRecord)
        {
            CurrentThread.DoWriteError(errorRecord);
        }

        public static void WriteVerbose(string message)
        {
            CurrentThread.DoWriteVerbose(message);
        }

        public static void WriteWarning(string message)
        {
            CurrentThread.DoWriteWarning(message);
        }

        private void DoWriteError(ErrorRecord errorRecord)
        {
            if (_Logger == null)
            {
                return;
            }

            _Logger.WriteError(errorRecord);
        }

        private void DoWriteVerbose(string message)
        {
            if (_Logger == null)
            {
                return;
            }

            // TODO: Prefix entries with context. i.e. //[Rule][$($Context.Index)][$($Rule.Name)]
            _Logger.WriteVerbose(message);
        }

        private void DoWriteWarning(string message)
        {
            if (_Logger == null)
            {
                return;
            }

            _Logger.WriteVerbose(message);
        }
    }
}
