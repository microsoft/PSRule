// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Threading;
using PSRule.Definitions;
using PSRule.Pipeline;
using PSRule.Resources;

namespace PSRule.Runtime
{
    public sealed class AssertResult : IEquatable<bool>
    {
        private readonly Assert _Assert;
        private readonly List<ResultReason> _Reason;

        internal AssertResult(Assert assert, IOperand operand, bool value, string reason, object[] args)
        {
            _Assert = assert;
            Result = value;
            if (!Result)
            {
                _Reason = new List<ResultReason>();
                AddReason(operand, reason, args);
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
            AddReason(null, text, null);
        }

        /// <summary>
        /// Add a reasons from an existing result.
        /// </summary>
        internal void AddReason(AssertResult result)
        {
            if (result == null || Result || result.Result || result._Reason == null || result._Reason.Count == 0)
                return;

            _Reason.AddRange(result._Reason);
        }

        /// <summary>
        /// Add a reason.
        /// </summary>
        /// <param name="text">The text of a reason to add. This text should already be localized for the currently culture.</param>
        internal void AddReason(IOperand operand, string text, params object[] args)
        {
            // Ignore reasons if this is a pass.
            if (Result || string.IsNullOrEmpty(text))
                return;

            _Reason.Add(new ResultReason(RunspaceContext.CurrentThread?.TargetObject?.Path, operand, text, args));
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
        /// <param name="text">The text of a reason to use. This text should already be localized for the currently culture.</param>
        /// <param name="args">Replacement arguments for the format string.</param>
        public AssertResult Reason(string text, params object[] args)
        {
            if (_Reason != null)
                _Reason.Clear();

            AddReason(Operand.FromTarget(), text, args);
            return this;
        }

        /// <summary>
        /// Replace the existing reason with the supplied format string.
        /// </summary>
        /// <param name="path">The object path that affected the reason.</param>
        /// <param name="text">The text of a reason to use. This text should already be localized for the currently culture.</param>
        /// <param name="args">Replacement arguments for the format string.</param>
        public AssertResult ReasonFrom(string path, string text, params object[] args)
        {
            if (_Reason != null)
                _Reason.Clear();

            AddReason(Operand.FromPath(path), text, args);
            return this;
        }

        /// <summary>
        /// Replace the existing reason with the supplied format string if the condition is true.
        /// </summary>
        /// <param name="condition">When true the reason will be used. When false the existing reason will be used.</param>
        /// <param name="text">The text of a reason to use. This text should already be localized for the currently culture.</param>
        /// <param name="args">Replacement arguments for the format string.</param>
        public AssertResult ReasonIf(bool condition, string text, params object[] args)
        {
            return !condition ? this : Reason(text, args);
        }

        /// <summary>
        /// Replace the existing reason with the supplied format string if the condition is true.
        /// </summary>
        /// <param name="path">The object path that affected the reason.</param>
        /// <param name="condition">When true the reason will be used. When false the existing reason will be used.</param>
        /// <param name="text">The text of a reason to use. This text should already be localized for the currently culture.</param>
        /// <param name="args">Replacement arguments for the format string.</param>
        public AssertResult ReasonIf(string path, bool condition, string text, params object[] args)
        {
            return !condition ? this : ReasonFrom(path, text, args);
        }

        /// <summary>
        /// Get an reasons that are currently set.
        /// </summary>
        /// <returns>Returns an array of reasons. This will always return null when the Value is true.</returns>
        public string[] GetReason()
        {
            return (Result || IsNullOrEmptyReason()) ? Array.Empty<string>() : _Reason.GetStrings();
        }

        /// <summary>
        /// Complete an assertion by writing an provided reasons and returning a boolean.
        /// </summary>
        /// <returns>Returns true or false.</returns>
        public bool Complete()
        {
            // Check that the scope is still valid
            if (!RunspaceContext.CurrentThread.IsScope(RunspaceScope.Rule))
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
            return IsNullOrEmptyReason() ? string.Empty : string.Join(" ", _Reason.GetStrings());
        }

        public bool ToBoolean()
        {
            return Result;
        }

        internal IResultReasonV2[] ToResultReason()
        {
            return _Reason == null || _Reason.Count == 0 ? Array.Empty<IResultReasonV2>() : _Reason.ToArray();
        }

        private bool IsNullOrEmptyReason()
        {
            return _Reason == null || _Reason.Count == 0;
        }
    }
}
