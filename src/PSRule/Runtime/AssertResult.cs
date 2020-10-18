// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Pipeline;
using PSRule.Resources;
using System;
using System.Collections.Generic;
using System.Threading;

namespace PSRule.Runtime
{
    public sealed class AssertResult : IEquatable<bool>
    {
        private readonly Assert _Assert;
        private readonly List<string> _Reason;

        internal AssertResult(Assert assert, bool value, string reason, object[] args)
        {
            _Assert = assert;
            Result = value;
            if (!Result)
            {
                _Reason = new List<string>();
                AddReason(reason, args);
            }
        }

        public static explicit operator bool(AssertResult result)
        {
            return result != null && result.Result;
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
            if (Result || string.IsNullOrEmpty(text))
                return;

            _Reason.Add(text);
        }

        /// <summary>
        /// Add a reason.
        /// </summary>
        /// <param name="text">The text of a reason to add. This text should already be localized for the currently culture.</param>
        internal void AddReason(string text, params object[] args)
        {
            // Ignore reasons if this is a pass.
            if (Result || string.IsNullOrEmpty(text))
                return;

            if (args == null || args.Length == 0)
                _Reason.Add(text);
            else
                _Reason.Add(string.Format(Thread.CurrentThread.CurrentCulture, text, args));
        }

        /// <summary>
        /// Adds a reason, and optionally replace existing reasons.
        /// </summary>
        /// <param name="text">The text of a reason to add. This text should already be localized for the currently culture.</param>
        /// <param name="replace">When set to true, existing reasons are cleared.</param>
        public AssertResult WithReason(string text, bool replace = false)
        {
            if (replace && _Reason != null)
                _Reason.Clear();

            AddReason(text);
            return this;
        }

        /// <summary>
        /// Replace the existing reason with the supplied format string.
        /// </summary>
        /// <param name="text">The text of a reason to add. This text should already be localized for the currently culture.</param>
        /// <param name="args">Replacement arguments for the format string.</param>
        public AssertResult Reason(string text, params object[] args)
        {
            if (_Reason != null)
                _Reason.Clear();

            AddReason(text, args);
            return this;
        }

        /// <summary>
        /// Get an reasons that are currently set.
        /// </summary>
        /// <returns>Returns an array of reasons. This will always return null when the Value is true.</returns>
        public string[] GetReason()
        {
            if (!Result || _Reason == null || _Reason.Count == 0)
                return Array.Empty<string>();

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
                throw new RuleException(string.Format(Thread.CurrentThread.CurrentCulture, PSRuleResources.VariableConditionScope, "Assert"));

            // Continue
            for (var i = 0; _Reason != null && i < _Reason.Count; i++)
                RunspaceContext.CurrentThread.WriteReason(_Reason[i]);

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

        public override string ToString()
        {
            if (_Reason == null)
                return string.Empty;

            return string.Join(" ", _Reason.ToArray());
        }

        public bool ToBoolean()
        {
            return Result;
        }
    }
}
