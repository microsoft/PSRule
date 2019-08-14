using System;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace PSRule.Pipeline
{
    /// <summary>
    /// A parser exception.
    /// </summary>
    [Serializable]
    public sealed class RuleParseException : PipelineExeception
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
}
