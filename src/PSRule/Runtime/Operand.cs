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
        Target = 5,

        /// <summary>
        /// A literal value or function.
        /// </summary>
        Value = 6,

        /// <summary>
        /// The object scope.
        /// </summary>
        Scope = 7
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
        private const string Dot = ".";
        private const string Space = " ";

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

        internal static IOperand FromValue(object value)
        {
            return new Operand(OperandKind.Value, null, value);
        }

        internal static IOperand FromScope(string scope)
        {
            return new Operand(OperandKind.Scope, scope);
        }

        internal static string JoinPath(string p1, string p2)
        {
            if (IsEmptyPath(p1))
                return p2;

            return IsEmptyPath(p2) ? p1 : string.Concat(p1, Dot, p2);
        }

        public override string ToString()
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
}
