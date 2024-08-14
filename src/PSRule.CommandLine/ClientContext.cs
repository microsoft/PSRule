// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine.Invocation;
using PSRule.Configuration;

namespace PSRule.CommandLine;

/// <summary>
/// 
/// </summary>
public sealed class ClientContext
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="invocation"></param>
    /// <param name="option"></param>
    /// <param name="verbose"></param>
    /// <param name="debug"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public ClientContext(InvocationContext invocation, string? option, bool verbose, bool debug)
    {
        Path = AppDomain.CurrentDomain.BaseDirectory;
        Invocation = invocation ?? throw new ArgumentNullException(nameof(invocation));
        Verbose = verbose;
        Debug = debug;
        Host = new ClientHost(this, verbose, debug);
        Option = GetOption(Host, option);
        CachePath = Path;
    }

    /// <summary>
    /// 
    /// </summary>
    public string Path { get; }

    /// <summary>
    /// 
    /// </summary>
    public InvocationContext Invocation { get; }

    /// <summary>
    /// 
    /// </summary>
    public ClientHost Host { get; }

    /// <summary>
    /// 
    /// </summary>
    public PSRuleOption Option { get; }

    /// <summary>
    /// Determines if verbose level diagnostic information should be displayed.
    /// </summary>
    public bool Verbose { get; }

    /// <summary>
    /// Determines if debug level diagnostic information should be displayed.
    /// </summary>
    public bool Debug { get; }

    /// <summary>
    /// Configures the path to use for caching rules modules.
    /// </summary>
    public string CachePath { get; }

    private static PSRuleOption GetOption(ClientHost host, string? path)
    {
        PSRuleOption.UseHostContext(host);
        var option = PSRuleOption.FromFileOrEmpty(path);
        option.Execution.InitialSessionState = Options.SessionState.Minimal;
        //option.Input.Format = InputFormat.File;
        option.Output.Style ??= OutputStyle.Client;
        return option;
    }
}
