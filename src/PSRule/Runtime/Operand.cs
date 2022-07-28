// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;

namespace PSRule.Runtime
{
    /// <summary>
    /// The type of operand that is compared with the expression.
    /// </summary>
    public enum OperandKind
    {
        /// <summary>
        /// Unknown.
        /// </summary>
        None = 0,

        /// <summary>
        /// An object path.
        /// </summary>
        Path = 1,

        /// <summary>
        /// The object target type.
        /// </summary>
        Type = 2,

        /// <summary>
        /// The object target name.
        /// </summary>
        Name = 3,

        /// <summary>
        /// The object source information.
        /// </summary>
        Source = 4,

        /// <summary>
        /// The target object itself.
        /// </summary>
        Target = 5
    }

    /// <summary>
    /// An operand that is compared with PSRule expressions.
    /// </summary>
    public interface IOperand
    {
        /// <summary>
        /// The value of the operand.
        /// </summary>
        object Value { get; }

        /// <summary>
        /// The type of operand.
        /// </summary>
        OperandKind Kind { get; }

        /// <summary>
        /// The object path to the operand.
        /// </summary>
        string Path { get; }

        /// <summary>
        /// A logical prefix to add to the object path.
        /// </summary>
        string Prefix { get; set; }
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

        public string Prefix { get; set; }

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
            return string.IsNullOrEmpty(Path) || Kind == OperandKind.Target ? null : OperandString();
        }

        private string OperandString()
        {
            var kind = Enum.GetName(typeof(OperandKind), Kind);
            return string.IsNullOrEmpty(Prefix) ? string.Concat(kind, " ", Path, ": ") : string.Concat(kind, " ", Prefix, ".", Path, ": ");
        }
    }
}
