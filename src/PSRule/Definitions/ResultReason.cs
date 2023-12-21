// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Runtime;

namespace PSRule.Definitions;

internal sealed class ResultReason : IResultReasonV2
{
    private string _Path;
    private string _Formatted;
    private string _Message;
    private string _FullPath;
    private readonly string _ParentPath;

    internal ResultReason(string parentPath, IOperand operand, string text, object[] args)
    {
        _ParentPath = parentPath;
        Operand = operand;
        _Path = Operand?.Path;
        Text = text;
        Args = args;
    }

    internal IOperand Operand { get; }

    /// <summary>
    /// The object path that failed.
    /// </summary>
    public string Path
    {
        get
        {
            _Path ??= GetPath();
            return _Path;
        }
    }

    /// <summary>
    /// A prefix to add to the object path that failed.
    /// </summary>
    internal string Prefix
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
            _FullPath ??= GetFullPath();
            return _FullPath;
        }
    }

    public string Text { get; }

    public object[] Args { get; }

    public string Message
    {
        get
        {
            _Message ??= Args == null || Args.Length == 0 ? Text : string.Format(Thread.CurrentThread.CurrentCulture, Text, Args);
            return _Message;
        }
    }

    public override string ToString()
    {
        return Format();
    }

    public string Format()
    {
        _Formatted ??= string.Concat(
            Operand?.ToString(),
            Message
        );
        return _Formatted;
    }

    private string GetPath()
    {
        return Runtime.Operand.JoinPath(Prefix, Operand?.Path);
    }

    private string GetFullPath()
    {
        return Runtime.Operand.JoinPath(_ParentPath, Path);
    }
}
