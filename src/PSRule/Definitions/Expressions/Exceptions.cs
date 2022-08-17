// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Runtime.Serialization;
using System.Security.Permissions;
using PSRule.Pipeline;

namespace PSRule.Definitions.Expressions
{
    /// <summary>
    /// A base class for runtime exceptions.
    /// </summary>
    public abstract class SelectorException : PipelineException
    {
        protected SelectorException()
            : base() { }

        protected SelectorException(string message)
            : base(message) { }

        protected SelectorException(string message, Exception innerException)
            : base(message, innerException) { }

        protected SelectorException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }

    [Serializable]
    public sealed class ExpressionParseException : SelectorException
    {
        public ExpressionParseException()
        {
        }

        public ExpressionParseException(string message)
            : base(message) { }

        public ExpressionParseException(string message, Exception innerException)
            : base(message, innerException) { }

        internal ExpressionParseException(string expression, string message)
            : base(message)
        {
            Expression = expression;
        }

        internal ExpressionParseException(string expression, string message, Exception innerException)
            : base(message, innerException)
        {
            Expression = expression;
        }

        private ExpressionParseException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        public string Expression { get; }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null) throw new ArgumentNullException(nameof(info));
            base.GetObjectData(info, context);
        }
    }

    public abstract class ExpressionException : SelectorException
    {
        protected ExpressionException()
        {
        }

        protected ExpressionException(string message)
            : base(message) { }

        protected ExpressionException(string message, Exception innerException)
            : base(message, innerException) { }

        protected ExpressionException(string expression, string message)
            : base(message)
        {
            Expression = expression;
        }

        protected ExpressionException(string expression, string message, Exception innerException)
            : base(message, innerException)
        {
            Expression = expression;
        }

        protected ExpressionException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        public string Expression { get; }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null) throw new ArgumentNullException(nameof(info));
            base.GetObjectData(info, context);
        }
    }

    [Serializable]
    public sealed class ExpressionReferenceException : SelectorException
    {
        public ExpressionReferenceException()
        {
        }

        public ExpressionReferenceException(string message)
            : base(message) { }

        public ExpressionReferenceException(string message, Exception innerException)
            : base(message, innerException) { }

        internal ExpressionReferenceException(string expression, string message)
            : base(message)
        {
            Expression = expression;
        }

        internal ExpressionReferenceException(string expression, string message, Exception innerException)
            : base(message, innerException)
        {
            Expression = expression;
        }

        private ExpressionReferenceException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        public string Expression { get; }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null) throw new ArgumentNullException(nameof(info));
            base.GetObjectData(info, context);
        }
    }


    [Serializable]
    public sealed class ExpressionArgumentException : ExpressionException
    {
        public ExpressionArgumentException()
        {
        }

        public ExpressionArgumentException(string message)
            : base(message) { }

        public ExpressionArgumentException(string message, Exception innerException)
            : base(message, innerException) { }

        internal ExpressionArgumentException(string expression, string message)
            : base(expression, message) { }

        private ExpressionArgumentException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null) throw new ArgumentNullException(nameof(info));
            base.GetObjectData(info, context);
        }
    }
}
