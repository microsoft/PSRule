using System;

namespace PSRule.Pipeline
{
    /// <summary>
    /// A rule exception.
    /// </summary>
    public sealed class RuleRuntimeException : PipelineExeception
    {
        /// <summary>
        /// Creates a rule exception.
        /// </summary>
        public RuleRuntimeException()
        {
        }

        /// <summary>
        /// Creates a rule exception.
        /// </summary>
        /// <param name="message">The detail of the exception.</param>
        public RuleRuntimeException(string message) : base(message)
        {
        }

        /// <summary>
        /// Creates a rule exception.
        /// </summary>
        /// <param name="message">The detail of the exception.</param>
        /// <param name="innerException">A nested exception that caused the issue.</param>
        public RuleRuntimeException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
