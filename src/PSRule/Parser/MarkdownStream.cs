using System.Diagnostics;
using System.Linq;

namespace PSRule.Parser
{
    internal delegate bool CharacterMatchDelegate(char c);

    [DebuggerDisplay("StartPos = (L: {Start}, C: {Column}), EndPos = (L: {End}, C: {Column.End}), Text = {Text}")]
    public sealed class SourceExtent
    {
        private string _Text;

        internal SourceExtent(string markdown, string path, int start, int end, int line, int column)
        {
            _Text = null;

            Markdown = markdown;
            Path = path;
            Start = start;
            End = end;
            Line = line;
            Column = column;
        }

        public string Markdown { get; private set; }

        public string Path { get; private set; }

        public int Start { get; private set; }

        public int End { get; private set; }

        public int Line { get; private set; }

        public int Column { get; private set; }

        public string Text
        {
            get
            {
                if (_Text == null)
                {
                    _Text = Markdown.Substring(Start, (End - Start));
                }

                return _Text;
            }
        }
    }

    internal sealed class MarkdownStream
    {
        private sealed class StreamCursor
        {
            public int Position = 0;
            public int Line = 0;
            public int Column = 0;
        }

        private readonly string _Markdown;
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

        private const char NewLine = '\n';
        private const char CarrageReturn = '\r';
        public const char Dash = '-';
        public const char Whitespace = ' ';
        public const char Hash = '#';
        public const char Backtick = '`';
        private const char BracketOpen = '[';
        private const char BracketClose = ']';
        private const char ParenthesesOpen = '(';
        private const char ParenthesesClose = ')';
        private const char AngleOpen = '<';
        private const char AngleClose = '>';
        public const char Backslash = '\\';
        public const string TripleBacktick = "```";
        public const string NewLineTripleBacktick = "\r\n```";
        public const char EqualSign = '=';
        public readonly static char[] NewLineStopCharacters = new char[] { '\r', '\n' };
        public readonly static char[] UnorderListCharacters = new char[] { '-', '*' };

        public MarkdownStream(string markdown)
        {
            _Markdown = markdown;
            _Length = _Markdown.Length;
            _Position = _Line = _Column = _EscapeLength = 0;

            UpdateCurrent();

            if (_Markdown.Length > 0)
            {
                _Line = 1;
            }
        }

        #region Properties

        public bool EOF
        {
            get { return _Position >= _Length; }
        }

        public bool IsStartOfLine
        {
            get { return _Column == 0; }
        }

        /// <summary>
        /// The character at the current position in the stream.
        /// </summary>
        public char Current
        {
            get { return _Current; }
        }

        public char Previous
        {
            get { return _Previous; }
        }

        public int Line
        {
            get { return _Line; }
        }

        public int Column
        {
            get { return _Column; }
        }

#if DEBUG

        /// <summary>
        /// Used for interactive debugging of current position and next characters in the stream.
        /// </summary>
        public string Preview
        {
            get { return _Markdown.Substring(_Position); }
        }

#endif

        public int Position
        {
            get { return _Position; }
        }

        private int Remaining
        {
            get { return _Length - Position; }
        }

        public string Body
        {
            get { return _Markdown; }
        }

        public bool IsEscaped
        {
            get { return _EscapeLength > 0; }
        }

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
        /// <returns>The number of line endings skipped.</returns>
        public int SkipLineEnding(int max = 1, bool ignoreEscaping = false)
        {
            var skipped = 0;

            while ((Current == CarrageReturn || Current == NewLine) && (max == 0 || skipped < max))
            {
                if (Current == CarrageReturn && (Remaining == 0 || Peak() != NewLine))
                {
                    break;
                }
                else
                {
                    Next();
                }

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
            {
                return false;
            }

            Next();

            return true;
        }

        /// <summary>
        /// Skip ahead if the current character is expected. Keep skipping when the character is repeated.
        /// </summary>
        /// <param name="c">The character to skip.</param>
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
        public void Skip(int toSkip, bool ignoreEscaping = false)
        {
            toSkip = HasRemaining(toSkip) ? toSkip : Remaining;

            for (var i = 0; i < toSkip; i++)
            {
                Next(ignoreEscaping);
            }
        }

        /// <summary>
        /// Peak at the n'th character from the current position. Check remaining characters prior to calling.
        /// </summary>
        /// <param name="offset">The offset from the current position.</param>
        /// <returns>The character at the offset.</returns>
        public char Peak(int offset = 1)
        {
            return _Markdown[_Position + offset];
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
            int count = 1;

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
        public SourceExtent GetExtent()
        {
            if (!_ExtentMarker.HasValue)
            {
                return null;
            }

            var extent = new SourceExtent(_Markdown, null, _ExtentMarker.Value, _Position, _Line, _Column);

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
            _Position += _EscapeLength + 1;

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
            _Position = _Position < 0 ? 0 : _Position;
            _EscapeLength = ignoreEscaping ? 0 : GetEscapeCount(_Position);
            
            _Previous = _Current;
            _Current = _Markdown[_Position + _EscapeLength];
        }

        private int GetEscapeCount(int position)
        {
            // Check for escape sequences
            if (position >= 0 && position < _Length && _Markdown[position] == Backslash)
            {
                var next = _Markdown[position + 1];

                // Check against list of escapable characters
                if (next == Backslash || next == BracketOpen || next == ParenthesesOpen ||next == AngleOpen || next == AngleClose || next == Backtick || next == BracketClose || next == ParenthesesClose)
                {
                    return 1;
                }
            }

            return 0;
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
                {
                    break;
                }

                length--;
            }

            return Substring(start, length, ignoreEscaping);
        }

        public string CaptureWithinLineUntil(char c)
        {
            var start = Position;
            var length = 0;

            while (Current != c && !EOF)
            {
                if (NewLineStopCharacters.Contains(Current))
                {
                    break;
                }

                length++;

                Next();
            }

            return Substring(start, length);
        }

        public string CaptureUntil(char[] c, bool ignoreEscaping = false)
        {
            var start = Position;
            var length = 0;

            while (!EOF)
            {
                if (!IsEscaped && c.Contains(Current))
                {
                    break;
                }

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
                {
                    break;
                }

                length++;

                Next(ignoreEscaping);
            }

            return Substring(start, length, ignoreEscaping);
        }

        private string Substring(int start, int length, bool ignoreEscaping = false)
        {
            if (ignoreEscaping)
            {
                return _Markdown.Substring(start, length);
            }

            var position = start;
            var i = 0;

            var buffer = new char[length];

            while (i < length)
            {
                var offset = GetEscapeCount(position);

                buffer[i] = _Markdown[position + offset];

                position += offset + 1;

                i++;
            }

            return new string(buffer);
        }

        /// <summary>
        /// Capture text until the end of the line.
        /// </summary>
        /// <returns>Returns the captured text up until the end of the line.</returns>
        public string CaptureLine()
        {
            return CaptureUntil("\r\n");
        }

        public bool IsSequence(string sequence, bool onNewLine = false)
        {
            if (onNewLine && !IsStartOfLine)
            {
                return false;
            }

            if (!HasRemaining(sequence.Length))
            {
                return false;
            }
            
            for (var i = 0; i < sequence.Length; i++)
            {
                if (Peak(i) != sequence[i])
                {
                    return false;
                }
            }

            return true;
        }

        private bool HasRemaining(int length)
        {
            return Remaining >= length;
        }

        private bool IsLineEnding(char c)
        {
            return c == CarrageReturn || c == NewLine;
        }
    }
}
