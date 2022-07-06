// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace PSRule.Runtime
{
    public enum OperandKind
    {
        None = 0,

        Path = 1,

        Type = 2,

        Name = 3,

        Source = 4,

        Target = 5
    }

    public interface IOperand
    {
        object Value { get; }

        OperandKind Kind { get; }

        string Path { get; }
    }

    internal sealed class Operand : IOperand
    {
        private Operand(OperandKind kind, object value)
        {
            Kind = kind;
            Value = value;
        }

        private Operand(OperandKind kind, string path, object value)
            : this(kind, value)
        {
            Path = path;
        }

        public object Value { get; }

        public string Path { get; }

        public OperandKind Kind { get; }

        internal static IOperand FromName(string name, string path)
        {
            return new Operand(OperandKind.Name, path, name);
        }

        internal static IOperand FromType(string type, string path)
        {
            return new Operand(OperandKind.Type, path, type);
        }

        internal static IOperand FromPath(string path, object value = null)
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

        public override string ToString()
        {
            return string.IsNullOrEmpty(Path) || Kind == OperandKind.Target ? null : string.Concat(Enum.GetName(typeof(OperandKind), Kind), " ", Path, ": ");
        }
    }
}
