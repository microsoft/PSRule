using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace PSRule.Pipeline
{
    /// <summary>
    /// A base class for all pipeline exceptions.
    /// </summary>
    public abstract class PipelineException : Exception
    {
        /// <summary>
        /// Creates a pipeline exception.
        /// </summary>
        protected PipelineException()
        {
        }

        /// <summary>
        /// Creates a pipeline exception.
        /// </summary>
        /// <param name="message">The detail of the exception.</param>
        protected PipelineException(string message) : base(message)
        {
        }

        /// <summary>
        /// Creates a pipeline exception.
        /// </summary>
        /// <param name="message">The detail of the exception.</param>
        /// <param name="innerException">A nested exception that caused the issue.</param>
        protected PipelineException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected PipelineException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
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

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null) throw new ArgumentNullException("info");
            base.GetObjectData(info, context);
        }
    }

    /// <summary>
    /// A parser exception.
    /// </summary>
    [Serializable]
    public sealed class RuleParseException : PipelineException
    {
        public readonly string ErrorId;

        /// <summary>
        /// Creates a rule exception.
        /// </summary>
        public RuleParseException()
        {
        }

        /// <summary>
        /// Creates a rule exception.
        /// </summary>
        /// <param name="message">The detail of the exception.</param>
        public RuleParseException(string message, string errorId) : base(message)
        {
            ErrorId = errorId;
        }

        /// <summary>
        /// Creates a rule exception.
        /// </summary>
        /// <param name="message">The detail of the exception.</param>
        /// <param name="innerException">A nested exception that caused the issue.</param>
        public RuleParseException(string message, string errorId, Exception innerException) : base(message, innerException)
        {
            ErrorId = errorId;
        }

        private RuleParseException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            ErrorId = info.GetString("ErrorId");
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null) throw new ArgumentNullException("info");
            info.AddValue("ErrorId", ErrorId);
            base.GetObjectData(info, context);
        }
    }

    /// <summary>
    /// A rule runtime exception.
    /// </summary>
    [Serializable]
    public sealed class RuleRuntimeException : PipelineException
    {
        /// <summary>
        /// Creates a rule runtime exception.
        /// </summary>
        public RuleRuntimeException()
        {
        }

        /// <summary>
        /// Creates a rule runtime exception.
        /// </summary>
        /// <param name="message">The detail of the exception.</param>
        public RuleRuntimeException(string message) : base(message)
        {
        }

        /// <summary>
        /// Creates a rule runtime exception.
        /// </summary>
        /// <param name="message">The detail of the exception.</param>
        /// <param name="innerException">A nested exception that caused the issue.</param>
        public RuleRuntimeException(string message, Exception innerException) : base(message, innerException)
        {
        }

        private RuleRuntimeException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null) throw new ArgumentNullException("info");
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
            if (info == null) throw new ArgumentNullException("info");
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

        private FailPipelineException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null) throw new ArgumentNullException("info");
            base.GetObjectData(info, context);
        }
    }
}
