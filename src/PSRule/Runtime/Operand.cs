// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Runtime;

internal sealed class Operand : IOperand
{
    private const string Dot = ".";
    private const string Space = " ";

    private Operand(OperandKind kind, object? value)
    {
        Kind = kind;
        Value = value;
    }

    private Operand(OperandKind kind, string? path, object? value)
        : this(kind, value)
    {
        Path = path;
    }

    public object? Value { get; }

    public string? Path { get; }

    public string? Prefix { get; set; }

    public OperandKind Kind { get; }

    internal static IOperand FromName(string name, string? path)
    {
        return new Operand(OperandKind.Name, path, name);
    }

    internal static IOperand FromType(string type, string? path)
    {
        return new Operand(OperandKind.Type, path, type);
    }

    internal static IOperand FromPath(string path, object? value = null)
    {
        return new Operand(OperandKind.Path, path, value);
    }

    internal static IOperand FromSource(string source)
    {
        return new Operand(OperandKind.Source, source);
    }

    internal static IOperand FromTarget()
    {
        return new Operand(OperandKind.Target, null, null);
    }

    internal static IOperand FromValue(object value)
    {
        return new Operand(OperandKind.Value, null, value);
    }

    internal static IOperand FromScope(string[]? scope)
    {
        return new Operand(OperandKind.Scope, scope);
    }

    internal static string JoinPath(string p1, string p2)
    {
        if (IsEmptyPath(p1))
            return p2;

        return IsEmptyPath(p2) ? p1 : string.Concat(p1, Dot, p2);
    }

    public override string? ToString()
    {
        return string.IsNullOrEmpty(Path) || Kind == OperandKind.Target ? null : OperandString();
    }

    private string OperandString()
    {
        var kind = Enum.GetName(typeof(OperandKind), Kind);
        return IsEmptyPath(Prefix) ? string.Concat(kind, Space, Path, ": ") : string.Concat(kind, Space, Prefix, Dot, Path, ": ");
    }

    private static bool IsEmptyPath(string s)
    {
        return string.IsNullOrEmpty(s) ||
            string.IsNullOrWhiteSpace(s) ||
            s == Dot;
    }
}
