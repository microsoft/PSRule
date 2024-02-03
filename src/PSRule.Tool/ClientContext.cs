// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine.Invocation;
using PSRule.Configuration;

namespace PSRule.Tool;

internal sealed class ClientContext
{
    public ClientContext(InvocationContext invocation, string? option, bool verbose, bool debug)
    {
        Path = AppDomain.CurrentDomain.BaseDirectory;
        Invocation = invocation ?? throw new ArgumentNullException(nameof(invocation));
        Verbose = verbose;
        Debug = debug;
        Host = new ClientHost(this, verbose, debug);
        Option = GetOption(Host, option);
    }

    public string Path { get; }

    public InvocationContext Invocation { get; }

    public ClientHost Host { get; }

    public PSRuleOption Option { get; }

    public bool Verbose { get; }

    public bool Debug { get; }

    private static PSRuleOption GetOption(ClientHost host, string? path)
    {
        PSRuleOption.UseHostContext(host);
        var option = PSRuleOption.FromFileOrEmpty(path);
        option.Execution.InitialSessionState = Options.SessionState.Minimal;
        option.Input.Format = InputFormat.File;
        option.Output.Style ??= OutputStyle.Client;
        return option;
    }
}
