// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Management.Automation;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace PSRule.Pipeline
{
    /// <summary>
    /// A base class for all pipeline exceptions.
    /// </summary>
    public abstract class PipelineException : Exception
    {
        protected PipelineException()
            : base() { }

        protected PipelineException(string message)
            : base(message) { }

        protected PipelineException(string message, Exception innerException)
            : base(message, innerException) { }

        protected PipelineException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }
    }

    /// <summary>
    /// A base class for runtime exceptions.
    /// </summary>
    public abstract class RuntimeException : PipelineException
    {
        protected RuntimeException()
            : base() { }

        protected RuntimeException(string message)
            : base(message) { }

        protected RuntimeException(string message, Exception innerException)
            : base(message, innerException) { }

        protected RuntimeException(Exception innerException, InvocationInfo invocationInfo, string ruleId)
            : base(innerException?.Message, innerException)
        {
            CommandInvocation = invocationInfo;
            RuleId = ruleId;
        }

        protected RuntimeException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        public InvocationInfo CommandInvocation { get; }

        public string RuleId { get; }
    }

    /// <summary>
    /// An exception when building the pipeline.
    /// </summary>
    [Serializable]
    public sealed class PipelineBuilderException : PipelineException
    {
        /// <summary>
        /// Creates a pipeline builder exception.
        /// </summary>
        public PipelineBuilderException()
            : base() { }

        /// <summary>
        /// Creates a pipeline builder exception.
        /// </summary>
        /// <param name="message">The detail of the exception.</param>
        public PipelineBuilderException(string message)
            : base(message) { }

        /// <summary>
        /// Creates a pipeline builder exception.
        /// </summary>
        /// <param name="message">The detail of the exception.</param>
        /// <param name="innerException">A nested exception that caused the issue.</param>
        public PipelineBuilderException(string message, Exception innerException)
            : base(message, innerException) { }

        private PipelineBuilderException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            base.GetObjectData(info, context);
        }
    }

    /// <summary>
    /// A serialization exception.
    /// </summary>
    [Serializable]
    public sealed class PipelineSerializationException : PipelineException
    {
        /// <summary>
        /// Creates a serialization exception.
        /// </summary>
        public PipelineSerializationException()
        {
        }

        internal PipelineSerializationException(string message, string path, Exception innerException)
            : this(message, innerException)
        {
            Path = path;
        }

        /// <summary>
        /// Creates a serialization exception.
        /// </summary>
        /// <param name="message">The detail of the exception.</param>
        public PipelineSerializationException(string message) : base(message)
        {
        }

        /// <summary>
        /// Creates a serialization exception.
        /// </summary>
        /// <param name="message">The detail of the exception.</param>
        /// <param name="innerException">A nested exception that caused the issue.</param>
        public PipelineSerializationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        private PipelineSerializationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        /// <summary>
        /// The path to the file.
        /// </summary>
        public string Path { get; }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            base.GetObjectData(info, context);
        }
    }

    /// <summary>
    /// A parser exception.
    /// </summary>
    [Serializable]
    public sealed class ParseException : PipelineException
    {
        /// <summary>
        /// Creates a rule exception.
        /// </summary>
        public ParseException()
        {
        }

        public ParseException(string message)
            : base(message) { }

        public ParseException(string message, Exception innerException)
            : base(message, innerException) { }

        /// <summary>
        /// Creates a rule exception.
        /// </summary>
        /// <param name="message">The detail of the exception.</param>
        internal ParseException(string message, string errorId) : base(message)
        {
            ErrorId = errorId;
        }

        /// <summary>
        /// Creates a rule exception.
        /// </summary>
        /// <param name="message">The detail of the exception.</param>
        /// <param name="innerException">A nested exception that caused the issue.</param>
        internal ParseException(string message, string errorId, Exception innerException) : base(message, innerException)
        {
            ErrorId = errorId;
        }

        private ParseException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            ErrorId = info.GetString("ErrorId");
        }

        public string ErrorId { get; }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            info.AddValue("ErrorId", ErrorId);
            base.GetObjectData(info, context);
        }
    }

    /// <summary>
    /// A rule runtime exception.
    /// </summary>
    [Serializable]
    public sealed class RuleException : RuntimeException
    {
        /// <summary>
        /// Creates a rule runtime exception.
        /// </summary>
        public RuleException()
        {
        }

        /// <summary>
        /// Creates a rule runtime exception.
        /// </summary>
        /// <param name="message">The detail of the exception.</param>
        public RuleException(string message)
            : base(message) { }

        /// <summary>
        /// Creates a rule runtime exception.
        /// </summary>
        /// <param name="message">The detail of the exception.</param>
        /// <param name="innerException">A nested exception that caused the issue.</param>
        public RuleException(string message, Exception innerException)
            : base(message, innerException) { }

        internal RuleException(Exception innerException, InvocationInfo invocationInfo, string ruleId, ErrorRecord errorRecord)
            : base(innerException, invocationInfo, ruleId)
        {
            ErrorRecord = errorRecord;
        }

        private RuleException(SerializationInfo info, StreamingContext context)
            : base(info, context) { }

        public ErrorRecord ErrorRecord { get; }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            base.GetObjectData(info, context);
        }
    }

    /// <summary>
    /// A pipeline configuration exception.
    /// </summary>
    [Serializable]
    public sealed class PipelineConfigurationException : PipelineException
    {
        /// <summary>
        /// Creates a pipeline configuration exception.
        /// </summary>
        public PipelineConfigurationException()
        {
        }

        /// <summary>
        /// Creates a pipeline configuration exception.
        /// </summary>
        /// <param name="optionName">The name of the option that caused the exception.</param>
        /// <param name="message">The detail of the exception.</param>
        public PipelineConfigurationException(string optionName, string message) : base(message)
        {
        }

        public PipelineConfigurationException(string message) : base(message)
        {
        }

        public PipelineConfigurationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary>
        /// Creates a pipeline configuration exception.
        /// </summary>
        /// <param name="optionName">The name of the option that caused the exception.</param>
        /// <param name="message">The detail of the exception.</param>
        /// <param name="innerException">A nested exception that caused the issue.</param>
        public PipelineConfigurationException(string optionName, string message, Exception innerException) : base(message, innerException)
        {
        }

        private PipelineConfigurationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            base.GetObjectData(info, context);
        }
    }

    [Serializable]
    public sealed class FailPipelineException : PipelineException
    {
        /// <summary>
        /// Creates a rule runtime exception.
        /// </summary>
        public FailPipelineException()
        {
        }

        /// <summary>
        /// Creates a rule runtime exception.
        /// </summary>
        /// <param name="message">The detail of the exception.</param>
        public FailPipelineException(string message) : base(message)
        {
        }

        public FailPipelineException(string message, Exception innerException) : base(message, innerException)
        {
        }

        private FailPipelineException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            base.GetObjectData(info, context);
        }
    }

    [Serializable]
    public sealed class RuntimeScopeException : PipelineException
    {
        /// <summary>
        /// Creates a rule runtime exception.
        /// </summary>
        public RuntimeScopeException()
        {
        }

        /// <summary>
        /// Creates a rule runtime exception.
        /// </summary>
        /// <param name="message">The detail of the exception.</param>
        public RuntimeScopeException(string message) : base(message)
        {
        }

        public RuntimeScopeException(string message, Exception innerException) : base(message, innerException)
        {
        }

        private RuntimeScopeException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            base.GetObjectData(info, context);
        }
    }
}
