// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.


using System.Text;

namespace PSRule.CommandLine;

/// <summary>
/// A console implementation that writes to the system console.
/// </summary>
public sealed class Console : IConsole
{
    /// <inheritdoc/>
    public IStandardStreamWriter Out => new StandardStreamWriter(System.Console.Out);

    /// <inheritdoc/>
    public IStandardStreamWriter Error => new StandardStreamWriter(System.Console.Error);

    /// <inheritdoc/>
    public Stream OpenStandardInput()
    {
        return System.Console.OpenStandardInput();
    }

    /// <inheritdoc/>
    public Stream OpenStandardOutput()
    {
        return System.Console.OpenStandardOutput();
    }

    internal sealed class StandardStreamWriter(TextWriter inner) : TextWriter, IStandardStreamWriter
    {
        public override void Write(string? value)
        {
            if (value == null) return;

            inner.WriteLine(value.Trim('\n', '\r'));
        }

        public override void WriteLine(string? value)
        {
            inner.WriteLine(value);
        }

        public override Encoding Encoding { get; } = Encoding.Unicode;
    }
}
