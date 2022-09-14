// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using Microsoft.CodeAnalysis;

namespace PSRule.BuildTask.Generators
{
    /// <summary>
    /// Generator contants for PSRule engine version.
    /// </summary>
    [Generator]
    public sealed class EngineVersionGenerator : ISourceGenerator
    {
        // Detailed from Roslyn SDK: https://docs.microsoft.com/dotnet/csharp/roslyn-sdk/source-generators-overview

        /// <inheritdoc/>
        public void Execute(GeneratorExecutionContext context)
        {
            var result = GetPartialContent(context);
            context.AddSource("EngineVersion.g.cs", result);
        }

        /// <inheritdoc/>
        public void Initialize(GeneratorInitializationContext context)
        {
            // Not required.
        }

        private static string GetPartialContent(GeneratorExecutionContext context)
        {
            if (!context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.PSRule_Version", out var productVersion))
                productVersion = "0.0.1";

            // Build up the source code
            return $@"// <auto-generated/>
using System;

namespace PSRule
{{
    internal static partial class Engine
    {{
        private const string _Version = ""{productVersion}"";
    }}
}}
";
        }
    }
}
