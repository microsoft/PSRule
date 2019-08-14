using System;

namespace PSRule.Pipeline
{
    /// <summary>
    /// A parser exception.
    /// </summary>
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
    }
}
