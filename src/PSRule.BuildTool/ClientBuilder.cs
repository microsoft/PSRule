// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.BuildTool.Resources;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;

namespace PSRule.BuildTool
{
    internal sealed class ClientBuilder : CommandBuilder
    {
        private ClientBuilder(RootCommand cmd) : base(cmd) { }

        public static ClientBuilder New()
        {
            var cmd = new RootCommand();
            return new ClientBuilder(cmd);
        }

        public ClientBuilder AddBadgeResource()
        {
            var cmd = new Command("badge", CmdStrings.Badge_Description);
            cmd.AddOption(new Option<string>(
                new string[] { "--output-path" }
            ));
            cmd.Handler = CommandHandler.Create<BadgeResourceOption, InvocationContext>(BadgeResource.Build);
            Command.AddCommand(cmd);
            return this;
        }
    }
}
