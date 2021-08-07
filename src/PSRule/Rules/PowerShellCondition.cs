// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Definitions;
using System.Collections.ObjectModel;
using System.Management.Automation;

namespace PSRule.Rules
{
    internal sealed class PowerShellCondition : ICondition
    {
        private const string ERROR_ACTION_PREFERENCE = "ErrorActionPreference";

        private readonly PowerShell _Condition;
        private readonly ActionPreference _ErrorAction;

        private bool _Disposed;

        internal PowerShellCondition(PowerShell condition, ActionPreference errorAction)
        {
            _Condition = condition;
            _ErrorAction = errorAction;
        }

        private void Dispose(bool disposing)
        {
            if (!_Disposed)
            {
                if (disposing)
                    _Condition.Dispose();

                _Disposed = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            System.GC.SuppressFinalize(this);
        }

        public IConditionResult If()
        {
            _Condition.Streams.ClearStreams();
            _Condition.Runspace.SessionStateProxy.SetVariable(ERROR_ACTION_PREFERENCE, _ErrorAction);
            return GetResult(_Condition.Invoke<Runtime.RuleConditionResult>());
        }

        private static Runtime.RuleConditionResult GetResult(Collection<Runtime.RuleConditionResult> value)
        {
            if (value == null || value.Count == 0)
                return null;

            return value[0];
        }
    }
}
