using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace PSRule.Pipeline
{
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
}
