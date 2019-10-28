// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Configuration;
using System;
using System.Collections.Generic;
using System.Management.Automation;

namespace PSRule.Pipeline
{
    internal sealed class TargetBinder
    {
        private readonly BindTargetMethod _BindTargetName;
        private readonly BindTargetMethod _BindTargetType;
        private readonly HashSet<string> _TypeFilter;

        internal TargetBinder(BindTargetMethod bindTargetName, BindTargetMethod bindTargetType, string[] typeFilter)
        {
            _BindTargetName = bindTargetName;
            _BindTargetType = bindTargetType;
            if (typeFilter != null && typeFilter.Length > 0)
                _TypeFilter = new HashSet<string>(typeFilter, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// The bound TargetName of the target object.
        /// </summary>
        public string TargetName { get; private set; }

        /// <summary>
        /// The bound TargetType of the target object.
        /// </summary>
        public string TargetType { get; private set; }

        /// <summary>
        /// Determines if the target object should be filtered.
        /// </summary>
        public bool ShouldFilter { get; private set; }

        /// <summary>
        /// Bind target object based on the supplied baseline.
        /// </summary>
        public void Bind(BaselineContext baseline, PSObject targetObject)
        {
            var binding = baseline.GetTargetBinding();
            TargetName = _BindTargetName(binding.TargetName, !binding.IgnoreCase, targetObject);
            TargetType = _BindTargetType(binding.TargetType, !binding.IgnoreCase, targetObject);
            ShouldFilter = !(_TypeFilter == null || _TypeFilter.Contains(TargetType));
        }
    }
}
