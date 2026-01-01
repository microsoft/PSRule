// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Configuration;
using PSRule.Options;
using PSRule.Pipeline.Dependencies;

namespace PSRule.CommandLine;

/// <summary>
/// 
/// </summary>
public sealed class ClientContext
{
    private string? _OptionPath;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="console"></param>
    /// <param name="option"></param>
    /// <param name="verbose"></param>
    /// <param name="debug"></param>
    /// <param name="workingPath"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public ClientContext(IConsole console, string? option, bool verbose, bool debug, string? workingPath = null)
    {
        Path = AppDomain.CurrentDomain.BaseDirectory;
        Console = console ?? throw new ArgumentNullException(nameof(console));
        Verbose = verbose;
        Debug = debug;
        Host = new ClientHost(this, verbose, debug);
        _OptionPath = option;
        Option = GetOption(Host, option);
        CachePath = Path;
        IntegrityAlgorithm = Option.Execution.HashAlgorithm.GetValueOrDefault(ExecutionOption.Default.HashAlgorithm!.Value).ToIntegrityAlgorithm();
        WorkingPath = workingPath;
    }

    /// <summary>
    /// 
    /// </summary>
    public string Path { get; }

    /// <summary>
    /// The console for this context.
    /// </summary>
    public IConsole Console { get; }

    /// <summary>
    /// 
    /// </summary>
    public ClientHost Host { get; }

    /// <summary>
    /// 
    /// </summary>
    public PSRuleOption Option { get; private set; }

    /// <summary>
    /// Determines if verbose level diagnostic information should be displayed.
    /// </summary>
    public bool Verbose { get; }

    /// <summary>
    /// Determines if debug level diagnostic information should be displayed.
    /// </summary>
    public bool Debug { get; }

    /// <summary>
    /// Configures the root path to use for caching artifacts including modules.
    /// Each artifact is in a subdirectory of the root path.
    /// </summary>
    public string CachePath { get; }

    /// <summary>
    /// The default integrity algorithm to use.
    /// </summary>
    public IntegrityAlgorithm IntegrityAlgorithm { get; }

    /// <summary>
    /// A map of resolved module versions when no version is specified.
    /// </summary>
    public Dictionary<string, string> ResolvedModuleVersions { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// The current working path.
    /// </summary>
    public string? WorkingPath { get; }

    /// <summary>
    /// The last error code for the current context.
    /// </summary>
    public int? LastErrorCode { get; private set; }

    private static PSRuleOption GetOption(ClientHost host, string? path)
    {
        PSRuleOption.UseHostContext(host);
        var option = PSRuleOption.FromFileOrEmpty(path);
        option.Execution.InitialSessionState = SessionState.Minimal;
        option.Output.Style ??= OutputStyle.Client;
        return option;
    }

    /// <summary>
    /// Set the last error code for the current context.
    /// </summary>
    internal void SetLastErrorCode(Runtime.EventId? eventId)
    {
        if (eventId == null) return;

        LastErrorCode = eventId.Value.Id;
    }

    /// <summary>
    /// The current options path used by this context.
    /// </summary>
    public string? OptionsPath => _OptionPath;

    /// <summary>
    /// Update the options path used by this context.
    /// </summary>
    public void UpdateOptionsPath(string optionPath)
    {
        _OptionPath = optionPath;
    }

    /// <summary>
    /// Reload the PSRule options from the configured path.
    /// </summary>
    public void ReloadOptions()
    {
        Option = GetOption(Host, _OptionPath);
    }
}
