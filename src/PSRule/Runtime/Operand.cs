// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using PSRule.Data;

namespace PSRule.Runtime
{
    internal enum OperandKind
    {
        None = 0,

        Field = 1,

        Type = 2,

        Name = 3,

        Source = 4
    }

    internal interface IOperand
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

        internal static IOperand FromName(string name)
        {
            return new Operand(OperandKind.Name, name);
        }

        internal static IOperand FromType(string type)
        {
            return new Operand(OperandKind.Type, type);
        }

        internal static IOperand FromField(string field, object value)
        {
            return new Operand(OperandKind.Field, field, value);
        }

        internal static IOperand FromSource(TargetSourceInfo source)
        {
            return new Operand(OperandKind.Source, source);
        }

        public override string ToString()
        {
            return string.Concat(Enum.GetName(typeof(OperandKind), Kind), " ", Path);
        }
    }
}
