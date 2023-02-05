// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Pipeline;

namespace PSRule.Benchmark
{
    internal sealed class BenchmarkHostContext : HostContext
    {
        public override bool ShouldProcess(string target, string action)
        {
            return true;
        }
    }
}
