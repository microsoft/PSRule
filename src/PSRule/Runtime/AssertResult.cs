// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Definitions;
using PSRule.Pipeline;
using PSRule.Resources;

namespace PSRule.Runtime;

#nullable enable

/// <summary>
/// The result of a single assertion.
/// </summary>
public sealed class AssertResult : IEquatable<bool>
{
    private List<ResultReason>? _Reason;

    internal AssertResult(IOperand? operand, bool value, string reason, object[] args)
    {
        Result = value;
        if (!Result)
        {
            _Reason = [];
            AddReason(operand, reason, args);
        }
    }

    /// <summary>
    /// Convert the result into a boolean value.
    /// </summary>
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

        _Reason ??= [];
        _Reason.AddRange(result._Reason.Where(r => !_Reason.Contains(r)));
    }

    /// <summary>
    /// Add a reason.
    /// </summary>
    /// <param name="operand">Identifies the operand that was the reason for the failure.</param>
    /// <param name="text">The text of a reason to add. This text should already be localized for the currently culture.</param>
    /// <param name="args">Replacement arguments for the format string.</param>
    internal void AddReason(IOperand? operand, string text, params object[]? args)
    {
        // Ignore reasons if this is a pass.
        if (Result || string.IsNullOrEmpty(text))
            return;

        _Reason ??= [];

        var reason = new ResultReason(LegacyRunspaceContext.CurrentThread?.TargetObject?.Path, operand, text, args);
        if (_Reason.Contains(reason))
            return;

        _Reason.Add(reason);
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
    /// Adds a logical path prefix on to each reason path.
    /// </summary>
    /// <param name="prefix">A string to prefix on each path.</param>
    public AssertResult PathPrefix(string prefix)
    {
        for (var i = 0; _Reason != null && i < _Reason.Count; i++)
            _Reason[i].Prefix = prefix;

        return this;
    }

    /// <summary>
    /// Replace the existing reason with the supplied format string.
    /// </summary>
    /// <param name="text">The text of a reason to use. This text should already be localized for the currently culture.</param>
    /// <param name="args">Replacement arguments for the format string.</param>
    public AssertResult Reason(string text, params object[] args)
    {
        _Reason?.Clear();

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
        _Reason?.Clear();

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
        if (!LegacyRunspaceContext.CurrentThread!.IsScope(RunspaceScope.Rule))
            throw new RuleException(string.Format(Thread.CurrentThread.CurrentCulture, PSRuleResources.VariableConditionScope, "Assert"));

        // Continue
        for (var i = 0; _Reason != null && i < _Reason.Count; i++)
            LegacyRunspaceContext.CurrentThread.WriteReason(_Reason[i]);

        return Result;
    }

    /// <summary>
    /// Clear any reasons for this result.
    /// </summary>
    public void Ignore()
    {
        _Reason?.Clear();
    }

    /// <inheritdoc/>
    public bool Equals(bool other)
    {
        return Result == other;
    }

    /// <summary>
    /// Get a formatted string of the result reasons.
    /// </summary>
    public override string ToString()
    {
        return IsNullOrEmptyReason() ? string.Empty : string.Join(" ", _Reason.GetStrings());
    }

    /// <summary>
    /// Convert the result into a boolean value.
    /// </summary>
    public bool ToBoolean()
    {
        return Result;
    }

    internal IResultReason[] ToResultReason()
    {
        return _Reason == null || _Reason.Count == 0 ? [] : _Reason.ToArray();
    }

    private bool IsNullOrEmptyReason()
    {
        return _Reason == null || _Reason.Count == 0;
    }
}

#nullable restore
