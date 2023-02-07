// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.CommandLine;
using System.Threading.Tasks;

namespace PSRule.BuildTool
{
    static class Program
    {
        /// <summary>
        /// Entry point for build tool.
        /// </summary>
        static async Task Main(string[] args)
        {
            await Build().InvokeAsync(args);
        }

        private static Command Build()
        {
            var builder = ClientBuilder.New();
            builder.AddBadgeResource();
            return builder.Command;
        }
    }
}
