// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Management.Automation;

namespace PSRule.Pipeline
{
    internal sealed class HostContext
    {
        internal readonly PSCmdlet CmdletContext;
        internal readonly EngineIntrinsics ExecutionContext;
        internal readonly ShouldProcess ShouldProcess;

        /// <summary>
        /// Determine if running is remote session.
        /// </summary>
        internal bool InSession;

        public HostContext(PSCmdlet commandRuntime, EngineIntrinsics executionContext)
        {
            InSession = false;
            CmdletContext = commandRuntime;
            ExecutionContext = executionContext;
            ShouldProcess = (target, action) => true;
            if (commandRuntime != null)
                ShouldProcess = commandRuntime.ShouldProcess;

            InSession = executionContext != null && executionContext.SessionState.PSVariable.GetValue("PSSenderInfo") != null;
        }
    }
}
