// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Runtime
{
    /// <summary>
    /// A set of log levels which indicate different types of diagnostic messages.
    /// </summary>
    [Flags]
    internal enum LogLevel
    {
        None = 0,

        Error = 1,

        Warning = 2,

        Info = 4,

        Verbose = 8,

        Debug = 16,
    }

    /// <summary>
    /// A generic interface for diagnostic logging within PSRule.
    /// </summary>
    internal interface ILogger
    {
        /// <summary>
        /// Determines if a specific log level should be written.
        /// </summary>
        /// <param name="level">The level to query.</param>
        /// <returns>Returns <c>true</c> when the log level should be written or <c>false</c> otherwise.</returns>
        bool ShouldLog(LogLevel level);

        /// <summary>
        /// Write a warning.
        /// </summary>
        /// <param name="message">The warning message write.</param>
        /// <param name="args">Any arguments to format the string with.</param>
        void Warning(string message, params object[] args);

        /// <summary>
        /// Write an error from an exception.
        /// </summary>
        /// <param name="exception">The exception to write.</param>
        /// <param name="errorId">A string identififer for the error.</param>
        void Error(Exception exception, string errorId = null);
    }
}
