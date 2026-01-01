// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text;
using Xunit.Abstractions;

namespace PSRule.CommandLine;

internal sealed class TestOutputWriter(ITestOutputHelper testOutput) : IConsole
{
    /// <inheritdoc />
    public IStandardStreamWriter Out => new StandardStreamWriter(testOutput);

    /// <inheritdoc />
    public IStandardStreamWriter Error => new StandardStreamWriter(testOutput);

    /// <inheritdoc />
    public bool IsOutputRedirected { get; }

    /// <inheritdoc />
    public bool IsErrorRedirected { get; }

    /// <inheritdoc />
    public bool IsInputRedirected { get; }

    public Stream OpenStandardInput()
    {
        throw new NotImplementedException();
    }

    public Stream OpenStandardOutput()
    {
        throw new NotImplementedException();
    }

    internal sealed class StandardStreamWriter(ITestOutputHelper testOutput) : TextWriter, IStandardStreamWriter
    {
        public override void Write(string? value)
        {
            if (value == null) return;

            testOutput.WriteLine(value.Trim('\n', '\r'));
        }

        public override Encoding Encoding { get; } = Encoding.Unicode;
    }
}
