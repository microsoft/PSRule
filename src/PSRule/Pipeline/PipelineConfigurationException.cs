using System;

namespace PSRule.Pipeline
{
    /// <summary>
    /// A pipeline configuration exception.
    /// </summary>
    public sealed class PipelineConfigurationException : PipelineExeception
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
    }
}
