using PSRule.Pipeline;
using System;
using System.IO;
using System.Reflection;
using Xunit;

namespace PSRule
{
    public sealed class InvokePipelineTests
    {
        [Fact]
        public void BuildInvokePipeline()
        {
            var builder = PipelineBuilder.Invoke();
            builder.Source(new string[] { Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FromFile.Rule.ps1") });
            builder.Build();
        }
    }
}
