// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Management.Automation;
using System.Management.Automation.Runspaces;
using PSRule.Configuration;
using PSRule.Definitions;
using PSRule.Host;
using PSRule.Options;
using PSRule.Pipeline;

namespace PSRule.Runtime.Scripting;

/// <summary>
/// A context that holds the state of a PowerShell runspace.
/// This is used to manage the lifecycle of the runspace and its associated resources.
/// </summary>
internal sealed class RunspaceContext : IRunspaceContext
{
    private const string ErrorPreference = "ErrorActionPreference";
    private const string WarningPreference = "WarningPreference";
    private const string VerbosePreference = "VerbosePreference";
    private const string DebugPreference = "DebugPreference";

    private readonly EventId _EmptyEventId = new(0);
    private readonly LanguageMode _LanguageMode;
    private readonly Options.SessionState _SessionState;
    private readonly ILogger _Logger;
    private readonly Stack<IResourceContext> _ContextStack = new();

    /// <summary>
    /// Track the current runspace scope.
    /// </summary>
    private readonly Stack<RunspaceScope> _Scope = new();

    private Runspace? _Runspace;
    private bool _Disposed;
    private IResourceContext? _CurrentContext;


    public RunspaceContext(PSRuleOption option, ILogger? logger)
    {
        if (option == null) throw new ArgumentNullException(nameof(option));

        // Configure.
        _LanguageMode = option.Execution?.LanguageMode ?? ExecutionOption.Default.LanguageMode!.Value;
        _SessionState = option.Execution?.InitialSessionState ?? ExecutionOption.Default.InitialSessionState!.Value;
        RestrictScriptSource = option.Execution?.RestrictScriptSource ?? ExecutionOption.Default.RestrictScriptSource!.Value;
        _Logger = logger ?? NullLogger.Instance;
        ErrorCount = 0;
    }

    /// <summary>
    /// The number of errors.
    /// </summary>
    public int ErrorCount { get; private set; }

    /// <summary>
    /// The last error record.
    /// This is set when an error is added to the error stream.
    /// </summary>
    public ErrorRecord? LastError { get; private set; }

    /// <summary>
    /// The script source restriction option.
    /// </summary>
    public RestrictScriptSource RestrictScriptSource { get; }

    public IResourceContext? ResourceContext => _CurrentContext;

    public void ResetErrorCount()
    {
        ErrorCount = 0;
        LastError = null;
    }

    public PowerShell GetPowerShell()
    {
        var result = PowerShell.Create();
        result.Runspace = GetRunspace();

        // Enable forwarding of events to the logger.
        result.Streams.Error.DataAdded += ErrorAdded;
        result.Streams.Warning.DataAdded += WarningAdded;
        result.Streams.Verbose.DataAdded += VerboseAdded;
        result.Streams.Information.DataAdded += InformationAdded;
        result.Streams.Debug.DataAdded += DebugAdded;

        return result;
    }

    public void EnterResourceContext(IResourceContext context)
    {
        _ContextStack.Push(context);
        _CurrentContext = context;
    }

    public void ExitResourceContext(IResourceContext context)
    {
        if (_ContextStack.Count == 0) throw new InvalidOperationException("No resource context to exit.");
        if (_ContextStack.Peek() != context) throw new InvalidOperationException("Resource context mismatch.");

        _ContextStack.Pop();
        _CurrentContext = _ContextStack.Count > 0 ? _ContextStack.Peek() : null;
    }

    public bool IsScope(RunspaceScope scope)
    {
        if (scope == RunspaceScope.None && (_Scope == null || _Scope.Count == 0))
            return true;

        if (_Scope == null || _Scope.Count == 0)
            return false;

        var current = _Scope.Peek();
        return scope.HasFlag(current);
    }

    public void PushScope(RunspaceScope scope)
    {
        _Scope.Push(scope);
    }

    public void PopScope(RunspaceScope scope)
    {
        var current = _Scope.Peek();
        if (current != scope)
            throw new RuntimeScopeException();

        _Scope.Pop();
    }

    private Runspace GetRunspace()
    {
        return _Runspace ??= CreateRunspace();
    }

    private Runspace CreateRunspace()
    {
        var state = HostState.CreateSessionState(_SessionState, _LanguageMode);
        var runspace = RunspaceFactory.CreateRunspace(state);
        Runspace.DefaultRunspace ??= runspace;

        runspace.Open();
        runspace.SessionStateProxy.PSVariable.Set(new PSRuleVariable(this));
        runspace.SessionStateProxy.PSVariable.Set(new RuleVariable());
        runspace.SessionStateProxy.PSVariable.Set(new LocalizedDataVariable());
        runspace.SessionStateProxy.PSVariable.Set(new AssertVariable());
        runspace.SessionStateProxy.PSVariable.Set(new TargetObjectVariable());
        runspace.SessionStateProxy.PSVariable.Set(new ConfigurationVariable(this));
        runspace.SessionStateProxy.PSVariable.Set(ErrorPreference, ActionPreference.Continue);
        runspace.SessionStateProxy.PSVariable.Set(WarningPreference, ActionPreference.Continue);
        runspace.SessionStateProxy.PSVariable.Set(VerbosePreference, ActionPreference.Continue);
        runspace.SessionStateProxy.PSVariable.Set(DebugPreference, ActionPreference.Continue);
        runspace.SessionStateProxy.PSVariable.Set(new PSVariable("PSRuleRunspaceContext", this, ScopedItemOptions.ReadOnly));

        runspace.SessionStateProxy.Path.SetLocation(Environment.GetWorkingPath());

        return runspace;
    }

    private void DebugAdded(object sender, DataAddedEventArgs e)
    {
        if (!TryRecord<DebugRecord>(sender, e, out var record) || record == null)
            return;

        _Logger.LogDebug(_EmptyEventId, record.Message);
    }

    private void InformationAdded(object sender, DataAddedEventArgs e)
    {
        if (!TryRecord<InformationRecord>(sender, e, out var record) || record == null)
            return;

        _Logger.LogInformation(_EmptyEventId, record.MessageData.ToString());
    }

    private void VerboseAdded(object sender, DataAddedEventArgs e)
    {
        if (!TryRecord<VerboseRecord>(sender, e, out var record) || record == null)
            return;

        _Logger.LogVerbose(_EmptyEventId, record.Message);
    }

    private void WarningAdded(object sender, DataAddedEventArgs e)
    {
        if (!TryRecord<WarningRecord>(sender, e, out var record) || record == null)
            return;

        _Logger.LogWarning(_EmptyEventId, record.Message);
    }

    private void ErrorAdded(object sender, DataAddedEventArgs e)
    {
        if (!TryRecord<ErrorRecord>(sender, e, out var record) || record == null)
            return;

        ErrorCount++;
        LastError = record;

        _Logger.LogError(_EmptyEventId, record.Exception, record.Exception?.Message);
    }

    private static bool TryRecord<TRecord>(object sender, DataAddedEventArgs e, out TRecord? record) where TRecord : class
    {
        record = default;
        if (sender is not PSDataCollection<TRecord> collection)
            return false;

        record = collection[e.Index];
        return record != null;
    }

    #region IDisposable

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (!_Disposed)
        {
            if (disposing)
            {
                ResetErrorCount();
                _Runspace?.Dispose();
            }
            _Disposed = true;
        }
    }

    #endregion IDisposable
}
