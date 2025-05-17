// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;

namespace PSRule.Help;

internal delegate bool CharacterMatchDelegate(char c);

[DebuggerDisplay("Position = {Position}, Current = {Current}")]
internal sealed class MarkdownStream
{
    private sealed class StreamCursor
    {
        public int Position;
        public int Line;
        public int Column;
    }

    private readonly string _Source;
    private readonly int _Length;

    /// <summary>
    /// The current character position in the markdown string. Call Next() to change the position.
    /// </summary>
    private int _Position;
    private int _Line;
    private int _Column;
    private char _Current;
    private char _Previous;
    private int _EscapeLength;

    private int? _ExtentMarker;
    private StreamCursor _Checkpoint;

    // The maximum length of a markdown document. ~32 MB in UTF-8
    private const int MaxLength = 4194304;

    private const char NewLine = '\n';
    private const char CarrageReturn = '\r';
    private const char Whitespace = ' ';
    private const char Backtick = '`';
    private const char BracketOpen = '[';
    private const char BracketClose = ']';
    private const char ParenthesesOpen = '(';
    private const char ParenthesesClose = ')';
    private const char AngleOpen = '<';
    private const char AngleClose = '>';
    private const char Backslash = '\\';
    private static readonly char[] NewLineStopCharacters = new char[] { '\r', '\n' };

    public MarkdownStream(string markdown)
    {
        _Source = markdown;
        _Length = _Source.Length;
        _Position = 0;
        _Line = 0;
        _Column = 0;
        _EscapeLength = 0;

        if (_Length < 0 || _Length > MaxLength)
            throw new ArgumentOutOfRangeException(nameof(markdown));

        UpdateCurrent();

        if (_Source.Length > 0)
            _Line = 1;
    }

    #region Properties

    public bool EOF => _Position >= _Length;

    public bool IsStartOfLine => _Column == 0;

    /// <summary>
    /// The character at the current position in the stream.
    /// </summary>
    public char Current => _Current;

    public char Previous => _Previous;

    public int Line => _Line;

    public int Column => _Column;

#if DEBUG

    /// <summary>
    /// Used for interactive debugging of current position and next characters in the stream.
    /// </summary>
    public string Preview => _Source.Substring(_Position);

#endif

    public int Position => _Position;

    private int Remaining => _Length - Position;

    public bool IsEscaped => _EscapeLength > 0;

    #endregion Properties

    /// <summary>
    /// Skip if the current character is whitespace.
    /// </summary>
    public void SkipWhitespace()
    {
        Skip(Whitespace, max: 0);
    }

    /// <summary>
    /// If the current character and sequential characters are line ending control characters, skip ahead.
    /// </summary>
    /// <param name="max">The number of line endings to skip. When max is 0, sequential line endings will be skipped.</param>
    /// <param name="ignoreEscaping">Determines if escaped characters are skipped.</param>
    /// <returns>The number of line endings skipped.</returns>
    public int SkipLineEnding(int max = 1, bool ignoreEscaping = false)
    {
        var skipped = 0;
        while ((Current == CarrageReturn || Current == NewLine) && (max == 0 || skipped < max))
        {
            if (Remaining == 0)
                break;

            if (Current == CarrageReturn && Peak() == NewLine)
                Next();

            Next(ignoreEscaping);
            skipped++;
        }
        return skipped;
    }

    /// <summary>
    /// Skip ahead if the next character is expected.
    /// </summary>
    /// <param name="c">The character to skip.</param>
    public int SkipNext(char c)
    {
        var skipped = 0;
        while (Peak() == c)
        {
            Next();
            skipped++;
        }
        return skipped;
    }

    public bool Skip(char c)
    {
        if (_Current != c)
            return false;

        Next();
        return true;
    }

    /// <summary>
    /// Skip ahead if the current character is expected. Keep skipping when the character is repeated.
    /// </summary>
    /// <param name="c">The character to skip.</param>
    /// <param name="max">The maximum number of characters to skip.</param>
    /// <returns>The number of characters that where skipped.</returns>
    public int Skip(char c, int max)
    {
        var skipped = 0;
        while (Current == c && (max == 0 || skipped < max))
        {
            Next();
            skipped++;
        }
        return skipped;
    }

    public int Skip(string sequence, int max = 0, bool ignoreEscaping = false)
    {
        var skipped = 0;
        while (IsSequence(sequence) && (max == 0 || skipped < max))
        {
            Skip(sequence.Length, ignoreEscaping);
            skipped++;
        }
        return skipped;
    }

    /// <summary>
    /// Skip ahead a number of characters. Use Next() in preference of Skip if the number to skip is 1.
    /// </summary>
    /// <param name="toSkip">The number of characters to skip</param>
    /// <param name="ignoreEscaping">Determines if escaped characters are skipped.</param>
    public void Skip(int toSkip, bool ignoreEscaping = false)
    {
        toSkip = HasRemaining(toSkip) ? toSkip : Remaining;
        for (var i = 0; i < toSkip; i++)
            Next(ignoreEscaping);
    }

    /// <summary>
    /// Peak at the n'th character from the current position. Check remaining characters prior to calling.
    /// </summary>
    /// <param name="offset">The offset from the current position.</param>
    /// <returns>The character at the offset.</returns>
    public char Peak(int offset = 1)
    {
        return _Source[_Position + offset];
    }

    public bool PeakAnyOf(int offset = 1, params char[] c)
    {
        return c.Contains(Peak(offset));
    }

    public bool PeakLine(char c, out int count)
    {
        var offset = 1;

        while (Peak(offset) == c)
        {
            offset++;
        }

        count = offset - 1;

        return NewLineStopCharacters.Contains(Peak(offset));
    }

    public int PeakCount(char c)
    {
        var count = 1;

        while (Peak(count) == c)
        {
            count++;
        }

        return count;
    }

    public void MarkExtentStart()
    {
        _ExtentMarker = _Position;
    }

    /// <summary>
    /// Get the extent and clear previous marker.
    /// </summary>
    /// <returns></returns>
    public SourceExtent? GetExtent()
    {
        if (!_ExtentMarker.HasValue)
        {
            return null;
        }

        var extent = new SourceExtent(
            source: _Source,
            path: null,
            start: _ExtentMarker.Value,
            end: _Position,
            line: _Line,
            column: _Column);

        _ExtentMarker = null;

        return extent;
    }

    /// <summary>
    /// Create a position checkpoint that can be rolled back.
    /// </summary>
    public void Checkpoint()
    {
        _Checkpoint = new StreamCursor { Position = _Position, Line = _Line, Column = _Column };
    }

    /// <summary>
    /// Rollback a position checkpoint.
    /// </summary>
    public void Rollback()
    {
        _Position = _Checkpoint.Position;
        _Line = _Checkpoint.Line;
        _Column = _Checkpoint.Column;

        UpdateCurrent();
    }

    /// <summary>
    /// Move to the next character in the stream.
    /// </summary>
    /// <returns>Is True when more characters exist in the stream.</returns>
    public bool Next(bool ignoreEscaping = false)
    {
        _Position += _EscapeLength > 0 ? _EscapeLength + 1 : 1;
        if (_Position >= _Length)
        {
            _Current = char.MinValue;
            return false;
        }

        // Update line and column counters
        if (_Current == NewLine)
        {
            _Line++;
            _Column = 0;
        }
        else
        {
            _Column += _EscapeLength + 1;
        }
        UpdateCurrent(ignoreEscaping);
        return true;
    }

    private void UpdateCurrent(bool ignoreEscaping = false)
    {
        // Handle escape sequences
        _EscapeLength = ignoreEscaping ? 0 : GetEscapeCount(_Position);

        _Previous = _Current;
        _Current = _Source[_Position + _EscapeLength];
    }

    private int GetEscapeCount(int position)
    {
        // Check for escape sequences
        if (position < _Length && _Source[position] == Backslash)
        {
            var next = _Source[position + 1];

            // Check against list of escapable characters
            if (next == Backslash ||
                next == BracketOpen ||
                next == ParenthesesOpen ||
                next == AngleOpen ||
                next == AngleClose ||
                next == Backtick ||
                next == BracketClose ||
                next == ParenthesesClose)
            {
                return 1;
            }
        }

        return 0;
    }

    public string CaptureWithinLineUntil(char c)
    {
        var start = Position;
        var length = 0;

        while (Current != c && !EOF)
        {
            if (NewLineStopCharacters.Contains(Current))
                break;

            length++;
            Next();
        }
        return Substring(start, length);
    }

    /// <summary>
    /// Capture text until the sequence is found.
    /// </summary>
    /// <param name="sequence">A specific sequence that ends the capture.</param>
    /// <param name="onNewLine"></param>
    /// <param name="ignoreEscaping">Interprets the string literally instead of processing escape sequences.</param>
    /// <returns>Returns the captured text up until the sequence.</returns>
    public string CaptureUntil(string sequence, bool onNewLine = false, bool ignoreEscaping = false)
    {
        var start = Position;
        var length = 0;

        while (!IsSequence(sequence, onNewLine) && !EOF)
        {
            length++;
            Next(ignoreEscaping);
        }

        // Back track line endings so they are not included in the captured string
        for (var i = 1; i < length; i++)
        {
            if (!IsLineEnding(Peak(-i)))
                break;

            length--;
        }
        return Substring(start, length, ignoreEscaping);
    }

    public string CaptureUntil(char[] c, bool ignoreEscaping = false)
    {
        var start = Position;
        var length = 0;

        while (!EOF)
        {
            if (!IsEscaped && c.Contains(Current))
                break;

            length++;
            Next(ignoreEscaping);
        }
        return Substring(start, length, ignoreEscaping);
    }

    public string CaptureUntil(CharacterMatchDelegate match, bool ignoreEscaping = false)
    {
        var start = Position;
        var length = 0;

        while (!EOF)
        {
            if (!IsEscaped && match(Current))
                break;

            length++;
            Next(ignoreEscaping);
        }
        return Substring(start, length, ignoreEscaping);
    }

    private string Substring(int start, int length, bool ignoreEscaping = false)
    {
        var newLine = System.Environment.NewLine.ToCharArray();

        var position = start;
        var i = 0;
        var buffer = new char[length * 2];
        while (i < length)
        {
            var ending = GetLineEndingCount(_Source, position);
            if (ending > 0)
            {
                newLine.CopyTo(buffer, i);
                i += newLine.Length;
                position += ending;

                // Adjust based on difference in line endings
                length += newLine.Length - ending;
                continue;
            }
            var offset = ignoreEscaping ? 0 : GetEscapeCount(position);
            buffer[i] = _Source[position + offset];
            position += offset + 1;
            i++;
        }
        return new string(buffer, 0, i);
    }

    /// <summary>
    /// Capture text until the end of the line.
    /// </summary>
    /// <returns>Returns the captured text up until the end of the line.</returns>
    public string CaptureLine()
    {
        return CaptureUntil(NewLineStopCharacters);
    }

    public bool IsSequence(string sequence, bool onNewLine = false)
    {
        if (onNewLine && !IsStartOfLine)
            return false;

        if (!HasRemaining(sequence.Length))
            return false;

        for (var i = 0; i < sequence.Length; i++)
        {
            if (Peak(i) != sequence[i])
                return false;
        }
        return true;
    }

    private bool HasRemaining(int length)
    {
        return Remaining >= length;
    }

    private static bool IsLineEnding(char c)
    {
        return c == CarrageReturn || c == NewLine;
    }

    private static int GetLineEndingCount(string s, int pos)
    {
        var c = s[pos];
        if (!IsLineEnding(c))
            return 0;

        return c == CarrageReturn && pos < s.Length - 1 && s[pos + 1] == NewLine ? 2 : 1;
    }
}
