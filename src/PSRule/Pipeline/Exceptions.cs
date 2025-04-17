// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Management.Automation;
using System.Runtime.Serialization;
using System.Security.Permissions;
using PSRule.Runtime;

namespace PSRule.Pipeline;

/// <summary>
/// A base class for runtime exceptions.
/// </summary>
public abstract class RuntimeException : PipelineException
{
    /// <summary>
    /// Initialize a new instance of a PSRule exception that is thrown during runtime.
    /// </summary>
    protected RuntimeException()
        : base() { }

    /// <summary>
    /// Initialize a new instance of a PSRule exception that is thrown during runtime.
    /// </summary>
    protected RuntimeException(string message)
        : base(message) { }

    /// <summary>
    /// Initialize a new instance of a PSRule exception that is thrown during runtime.
    /// </summary>
    protected RuntimeException(string message, Exception innerException)
        : base(message, innerException) { }

    /// <summary>
    /// Initialize a new instance of a PSRule exception that is thrown during runtime.
    /// </summary>
    protected RuntimeException(Exception innerException, InvocationInfo invocationInfo, string ruleId)
        : base(innerException?.Message, innerException)
    {
        CommandInvocation = invocationInfo;
        RuleId = ruleId;
    }

    /// <summary>
    /// Initialize a new instance of a PSRule exception that is thrown during runtime.
    /// </summary>
    protected RuntimeException(SerializationInfo info, StreamingContext context)
        : base(info, context) { }

    /// <summary>
    /// Additional information about the invocation when executing PowerShell language elements.
    /// </summary>
    public InvocationInfo CommandInvocation { get; }

    /// <summary>
    /// A unique identifier for the rule that was executing if currently within the context of a rule.
    /// </summary>
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
    /// <param name="eventId">An event identifier for the exception.</param>
    /// <param name="message">The detail of the exception.</param>
    public PipelineBuilderException(EventId eventId, string message)
        : base(eventId, message) { }

    /// <summary>
    /// Creates a pipeline builder exception.
    /// </summary>
    /// <param name="message">The detail of the exception.</param>
    /// <param name="innerException">A nested exception that caused the issue.</param>
    public PipelineBuilderException(string message, Exception innerException)
        : base(message, innerException) { }

    /// <summary>
    /// Creates a pipeline builder exception.
    /// </summary>
    /// <param name="eventId">An event identifier for the exception.</param>
    /// <param name="message">The detail of the exception.</param>
    /// <param name="innerException">A nested exception that caused the issue.</param>
    public PipelineBuilderException(EventId eventId, string message, Exception innerException)
        : base(eventId, message, innerException) { }

    private PipelineBuilderException(SerializationInfo info, StreamingContext context)
        : base(info, context) { }

    /// <inheritdoc/>
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
    /// Creates a parser exception.
    /// </summary>
    public ParseException()
    {
    }

    /// <summary>
    /// Creates a parser exception.
    /// </summary>
    public ParseException(string message)
        : base(message) { }

    /// <summary>
    /// Creates a parser exception.
    /// </summary>
    public ParseException(string message, Exception innerException)
        : base(message, innerException) { }

    /// <summary>
    /// Creates a parser exception.
    /// </summary>
    /// <param name="message">The detail of the exception.</param>
    /// <param name="errorId">A unique identifier related to the exception.</param>
    internal ParseException(string message, string errorId) : base(message)
    {
        ErrorId = errorId;
    }

    /// <summary>
    /// Creates a parser exception.
    /// </summary>
    /// <param name="message">The detail of the exception.</param>
    /// <param name="errorId">A unique identifier related to the exception.</param>
    /// <param name="innerException">A nested exception that caused the issue.</param>
    internal ParseException(string message, string errorId, Exception innerException) : base(message, innerException)
    {
        ErrorId = errorId;
    }

    /// <summary>
    /// Creates a parser exception.
    /// </summary>
    private ParseException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
        ErrorId = info.GetString("ErrorId");
    }

    /// <summary>
    /// An associated identifier related to why the exception was thrown.
    /// </summary>
    public string ErrorId { get; }

    /// <inheritdoc/>
    [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        if (info == null) throw new ArgumentNullException(nameof(info));

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

    /// <summary>
    /// Creates a rule runtime exception.
    /// </summary>
    internal RuleException(Exception innerException, InvocationInfo invocationInfo, string ruleId, ErrorRecord errorRecord)
        : base(innerException, invocationInfo, ruleId)
    {
        ErrorRecord = errorRecord;
    }

    /// <summary>
    /// Creates a rule runtime exception.
    /// </summary>
    private RuleException(SerializationInfo info, StreamingContext context)
        : base(info, context) { }

    /// <summary>
    /// An associated error record related to the exception.
    /// </summary>
    public ErrorRecord ErrorRecord { get; }

    /// <inheritdoc/>
    [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        if (info == null)
            throw new ArgumentNullException(nameof(info));

        base.GetObjectData(info, context);
    }
}

/// <summary>
/// A base class for configuration exceptions.
/// </summary>
public abstract class ConfigurationException : PipelineException
{
    /// <summary>
    /// Initialize a new instance of a PSRule exception that is thrown when attempting to read configuration.
    /// </summary>
    protected ConfigurationException()
        : base() { }

    /// <summary>
    /// Initialize a new instance of a PSRule exception that is thrown when attempting to read configuration.
    /// </summary>
    protected ConfigurationException(string message)
        : base(message) { }

    /// <summary>
    /// Initialize a new instance of a PSRule exception that is thrown when attempting to read configuration.
    /// </summary>
    protected ConfigurationException(string message, Exception innerException)
        : base(message, innerException) { }

    /// <summary>
    /// Initialize a new instance of a PSRule exception that is thrown when attempting to read configuration.
    /// </summary>
    protected ConfigurationException(SerializationInfo info, StreamingContext context)
        : base(info, context) { }
}

/// <summary>
/// A pipeline configuration exception.
/// </summary>
[Serializable]
public sealed class PipelineConfigurationException : ConfigurationException
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
    public PipelineConfigurationException(string message) : base(message)
    {
    }

    /// <summary>
    /// Creates a pipeline configuration exception.
    /// </summary>
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

    /// <inheritdoc/>
    private PipelineConfigurationException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    /// <inheritdoc/>
    [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        if (info == null)
            throw new ArgumentNullException(nameof(info));

        base.GetObjectData(info, context);
    }
}

/// <summary>
/// A configuration parse exception.
/// </summary>
[Serializable]
public sealed class ConfigurationParseException : ConfigurationException
{
    /// <summary>
    /// Creates a configuration parse exception.
    /// </summary>
    public ConfigurationParseException()
    {
    }

    /// <summary>
    /// Creates a configuration parse exception.
    /// </summary>
    /// <param name="path">The path to the options file they cause the exception.</param>
    /// <param name="message">The detail of the exception.</param>
    public ConfigurationParseException(string path, string message)
        : base(message)
    {
        Path = path;
    }

    /// <summary>
    /// Creates a configuration parse exception.
    /// </summary>
    public ConfigurationParseException(string message)
        : base(message) { }

    /// <summary>
    /// Creates a configuration parse exception.
    /// </summary>
    public ConfigurationParseException(string message, Exception innerException)
        : base(message, innerException) { }

    /// <summary>
    /// Creates a configuration parse exception.
    /// </summary>
    /// <param name="path">The path to the options file they cause the exception</param>
    /// <param name="message">The detail of the exception.</param>
    /// <param name="innerException">A nested exception that caused the issue.</param>
    public ConfigurationParseException(string path, string message, Exception innerException)
        : base(message, innerException)
    {
        Path = path;
    }

    /// <inheritdoc/>
    private ConfigurationParseException(SerializationInfo info, StreamingContext context)
        : base(info, context) { }

    /// <inheritdoc/>
    [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        if (info == null)
            throw new ArgumentNullException(nameof(info));

        base.GetObjectData(info, context);
    }

    /// <summary>
    /// The path to the options file used.
    /// </summary>
    public string Path { get; }
}

/// <summary>
/// An exception thrown by PSRule when the calling PowerShell environment should fail because one or more rules failed or errored.
/// </summary>
[Serializable]
public sealed class FailPipelineException : PipelineException
{
    /// <inheritdoc/>
    public FailPipelineException()
    {
    }

    /// <inheritdoc/>
    public FailPipelineException(string message) : base(message)
    {
    }

    /// <inheritdoc/>
    public FailPipelineException(string message, Exception innerException) : base(message, innerException)
    {
    }

    /// <inheritdoc/>
    private FailPipelineException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    /// <inheritdoc/>
    [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        if (info == null)
            throw new ArgumentNullException(nameof(info));

        base.GetObjectData(info, context);
    }
}

/// <summary>
/// An exception thrown by PSRule when a runtime property or method is used outside of the intended scope.
/// Avoid using runtime variables outside of a PSRule pipeline.
/// </summary>
[Serializable]
public sealed class RuntimeScopeException : PipelineException
{
    /// <inheritdoc/>
    public RuntimeScopeException()
    {
    }

    /// <inheritdoc/>
    public RuntimeScopeException(string message) : base(message)
    {
    }

    /// <inheritdoc/>
    public RuntimeScopeException(string message, Exception innerException) : base(message, innerException)
    {
    }

    /// <inheritdoc/>
    private RuntimeScopeException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    /// <inheritdoc/>
    [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        if (info == null)
            throw new ArgumentNullException(nameof(info));

        base.GetObjectData(info, context);
    }
}
