// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Runtime;

namespace PSRule.Definitions;

#nullable enable

/// <summary>
/// A reason for the rule result.
/// </summary>
public sealed class ResultReason : IResultReason
{
    private readonly string _ParentPath;

    private string? _Path;
    private string? _Formatted;
    private string? _Message;
    private string? _FullPath;

    internal ResultReason(string? parentPath, IOperand? operand, string text, object[]? args)
    {
        _ParentPath = parentPath ?? string.Empty;
        Operand = operand;
        _Path = Operand?.Path;
        Text = text;
        Args = args;
    }

    internal IOperand? Operand { get; }

    /// <summary>
    /// The object path that failed.
    /// </summary>
    public string Path
    {
        get
        {
            return _Path ??= GetPath();
        }
    }

    /// <summary>
    /// A prefix to add to the object path that failed.
    /// </summary>
    internal string? Prefix
    {
        get { return Operand?.Prefix; }
        set
        {
            if (Operand != null && Operand.Prefix != value)
            {
                Operand.Prefix = value;
                _Formatted = _Path = _FullPath = null;
            }
        }
    }

    /// <summary>
    /// The object path including the path of the parent object.
    /// </summary>
    public string FullPath
    {
        get
        {
            return _FullPath ??= GetFullPath();
        }
    }

    public string Text { get; }

    public object[]? Args { get; }

    /// <inheritdoc/>
    public string Message
    {
        get
        {
            return _Message ??= Args == null || Args.Length == 0 ? Text : string.Format(Thread.CurrentThread.CurrentCulture, Text, Args);
        }
    }

    public override string ToString()
    {
        return Format();
    }

    public override int GetHashCode()
    {
        return ToString().GetHashCode();
    }

    public override bool Equals(object obj)
    {
        return obj is IResultReason other && Equals(other);
    }

    /// <inheritdoc/>
    public string Format()
    {
        return _Formatted ??= string.Concat(
            Operand?.ToString(),
            Message
        );
    }

    private string GetPath()
    {
        return Runtime.Operand.JoinPath(Prefix, Operand?.Path);
    }

    private string GetFullPath()
    {
        return Runtime.Operand.JoinPath(_ParentPath, Path);
    }

    #region IEquatable<IResultReason>

    public bool Equals(IResultReason? other)
    {
        return other != null &&
            string.Equals(FullPath, other.FullPath, StringComparison.Ordinal) &&
            string.Equals(Message, other.Message, StringComparison.Ordinal);
    }

    #endregion IEquatable<IResultReason>
}

#nullable restore
