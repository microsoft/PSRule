// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Configuration;
using System.Management.Automation;

namespace PSRule.Pipeline
{
    internal sealed class TargetBinder
    {
        private readonly BindTargetMethod _BindTargetName;
        private readonly BindTargetMethod _BindTargetType;

        public TargetBinder(BindTargetMethod bindTargetName, BindTargetMethod bindTargetType)
        {
            _BindTargetName = bindTargetName;
            _BindTargetType = bindTargetType;
        }

        public string TargetName { get; private set; }

        public string TargetType { get; private set; }

        public void Bind(BaselineContext baseline, PSObject targetObject)
        {
            var binding = baseline.GetTargetBinding();

            // Bind TargetName
            TargetName = _BindTargetName(binding.TargetName, !binding.IgnoreCase, targetObject);

            // Bind TargetType
            TargetType = _BindTargetType(binding.TargetType, !binding.IgnoreCase, targetObject);
        }
    }
}
