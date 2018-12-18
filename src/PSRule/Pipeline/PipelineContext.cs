using System;
using System.Management.Automation;
using PSRule.Host;
using PSRule.Rules;

namespace PSRule.Pipeline
{
    internal sealed class PipelineContext
    {
        [ThreadStatic]
        internal static PipelineContext CurrentThread;

        private string _LogPrefix;
        private int _ObjectNumber;
        private readonly ILogger _Logger;

        private PipelineContext(ILogger logger)
        {
            _ObjectNumber = -1;
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

        public static void WriteVerbose(string message, bool usePrefix = true)
        {
            CurrentThread.DoWriteVerbose(message, usePrefix);
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

        private void DoWriteVerbose(string message, bool usePrefix)
        {
            if (_Logger == null)
            {
                return;
            }

            var outMessage = message;

            if (usePrefix)
            {
                outMessage = string.Concat(_LogPrefix, message);
            }

            _Logger.WriteVerbose(outMessage);
        }

        /// <summary>
        /// Increment the pipeline object number.
        /// </summary>
        public void Next()
        {
            _ObjectNumber++;
        }

        private void DoWriteWarning(string message)
        {
            if (_Logger == null)
            {
                return;
            }

            _Logger.WriteVerbose(message);
        }

        public void Enter(DependencyGraph<RuleBlock>.DependencyTarget target)
        {
            _LogPrefix = $"[PSRule][R][{_ObjectNumber}][{target.Value.RuleId}]";
        }

        public void Exit()
        {
            _LogPrefix = string.Empty;
        }
    }
}
