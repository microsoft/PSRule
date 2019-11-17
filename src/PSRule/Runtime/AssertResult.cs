// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Pipeline;
using PSRule.Resources;
using System;
using System.Collections.Generic;

namespace PSRule.Runtime
{
    public sealed class AssertResult : IEquatable<bool>
    {
        private readonly Assert _Assert;
        private readonly List<string> _Reason;

        internal AssertResult(Assert assert, bool value, string reason)
        {
            _Assert = assert;
            Result = value;

            if (!Result)
            {
                _Reason = new List<string>();
                if (!string.IsNullOrEmpty(reason))
                    _Reason.Add(reason);
            }
        }

        public static explicit operator bool(AssertResult result)
        {
            return result.Result;
        }

        /// <summary>
        /// Success of the condition. True indicate pass, false indicates fail.
        /// </summary>
        public bool Result { get; private set; }

        /// <summary>
        /// Add a reason.
        /// </summary>
        /// <param name="text">The text of a reason to add. This text should already be localized for the currently culture.</param>
        public void AddReason(string text)
        {
            // Ignore reasons if this is a pass.
            if (Result)
                return;

            _Reason.Add(text);
        }

        /// <summary>
        /// Get an reasons that are currently set.
        /// </summary>
        /// <returns>Returns an array of reasons. This will always return null when the Value is true.</returns>
        public string[] GetReason()
        {
            if (!Result || _Reason == null || _Reason.Count == 0)
                return null;

            return _Reason.ToArray();
        }

        /// <summary>
        /// Complete an assertion by writing an provided reasons and returning a boolean.
        /// </summary>
        /// <returns>Returns true or false.</returns>
        public bool Complete()
        {
            // Check that the scope is still valid
            if (PipelineContext.CurrentThread.ExecutionScope != ExecutionScope.Condition)
                throw new RuleRuntimeException(string.Format(PSRuleResources.VariableConditionScope, "Assert"));

            // Continue
            for (var i = 0; _Reason != null && i < _Reason.Count; i++)
            {
                PipelineContext.CurrentThread.WriteReason(_Reason[i]);
            }
            return Result;
        }

        public void Ignore()
        {
            _Reason.Clear();
        }

        public bool Equals(bool other)
        {
            return Result == other;
        }
    }
}
