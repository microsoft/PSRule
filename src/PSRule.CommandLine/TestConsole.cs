// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Text;

namespace PSRule.CommandLine;

/// <summary>
/// Provides access to in-memory standard streams that are not attached to <see cref="System.Console"/>.
/// </summary>
public sealed class TestConsole : IConsole
{
    private readonly MemoryStream _InStream;
    private readonly MemoryStream _OutStream;
    private readonly MemoryStream _ErrorStream;

    /// <summary>
    /// Initializes a new instance of <see cref="TestConsole"/>.
    /// </summary>
    public TestConsole()
    {
        _InStream = new MemoryStream();
        _OutStream = new MemoryStream();
        _ErrorStream = new MemoryStream();

        Out = new StandardStreamWriter(_OutStream);
        Error = new StandardStreamWriter(_ErrorStream);
    }

    /// <inheritdoc />
    public IStandardStreamWriter Error { get; }

    /// <inheritdoc />
    public IStandardStreamWriter Out { get; }

    // /// <inheritdoc />
    // public bool IsOutputRedirected { get; protected set; }

    // /// <inheritdoc />
    // public bool IsErrorRedirected { get; protected set; }

    // /// <inheritdoc />
    // public bool IsInputRedirected { get; protected set; }

    /// <inheritdoc />
    public Stream OpenStandardInput()
    {
        return _InStream;
    }

    /// <inheritdoc />
    public Stream OpenStandardOutput()
    {
        return _OutStream;
    }

    internal class StandardStreamWriter(MemoryStream stream) : TextWriter, IStandardStreamWriter
    {
        private readonly MemoryStream _Stream = stream ?? throw new ArgumentNullException(nameof(stream));
        private readonly StreamWriter _Writer = new(stream, Encoding.Unicode);

        public override void Write(char value)
        {
            _Writer.Write(value);
        }

        public override void Write(string? value)
        {
            _Writer.Write(value);
        }

        public override Encoding Encoding => _Writer.Encoding;

        public override string ToString()
        {
            _Writer.Flush();
            _Stream.Position = 0;
            using var reader = new StreamReader(_Stream, Encoding.Unicode, false, 1024, true);
            return reader.ReadToEnd();
        }
    }
}
