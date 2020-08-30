// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Parser
{
    internal enum MarkdownReaderMode
    {
        None,

        List
    }

    /// <summary>
    /// Stateful markdown reader.
    /// </summary>
    internal sealed class MarkdownReader
    {
        private readonly TokenStream _Output;
        private readonly bool _YamlHeaderOnly;

        /// <summary>
        /// Preserve formatting skips processing inlines and treats them as raw text.
        /// </summary>
        private readonly bool _PreserveFormatting;

        private MarkdownReaderMode _Context;
        private MarkdownStream _Stream;

        /// <summary>
        /// Line ending characters: \r, \n
        /// </summary>
        private readonly static char[] LineEndingCharacters = new char[] { '\r', '\n' };

        private const char Hash = '#';
        private const char Asterix = '*';
        private const char Backtick = '`';
        private const char Underscore = '_';
        private const char Whitespace = ' ';
        private const char Colon = ':';
        private const char Dash = '-';
        private const char BracketOpen = '[';
        private const char BracketClose = ']';
        private const char ParenthesesOpen = '(';
        private const char ParenthesesClose = ')';
        private const char EqualSign = '=';
        private const string TripleBacktick = "```";
        private static readonly char[] LinkNameStopCharacters = new char[] { '\r', '\n', ']' };
        private static readonly char[] LinkUrlStopCharacters = new char[] { '\r', '\n', ')' };
        private static readonly char[] YamlHeaderStopCharacters = new char[] { '\r', '\n', ':' };

        private const string TripleDash = "---";

        internal MarkdownReader(bool yamlHeaderOnly)
        {
            _Output = new TokenStream();
            _YamlHeaderOnly = yamlHeaderOnly;
            _PreserveFormatting = false;
        }

        public TokenStream Read(string markdown, string path)
        {
            if (string.IsNullOrEmpty(markdown))
                return _Output;

            _Context = MarkdownReaderMode.None;
            _Stream = new MarkdownStream(markdown);

            YamlHeader();

            if (_YamlHeaderOnly)
                return _Output;

            while (!_Stream.EOF)
            {
                var processed = UnderlineHeader() ||
                    HashHeader() ||
                    FencedBlock() ||
                    Link() ||
                    LineBreak();

                if (!processed)
                    Text();
            }

            return _Output;
        }

        private void YamlHeader()
        {
            if (_Stream.EOF || _Stream.Line > 1 || _Stream.Current != Dash)
                return;

            // Check if the line is just dashes indicating start of yaml header
            if (!_Stream.PeakLine(Dash, out int count) || count < 2)
                return;

            _Stream.Skip(count + 1);
            _Stream.SkipLineEnding();

            while (!_Stream.EOF && !_Stream.IsSequence(TripleDash, onNewLine: true))
            {
                var key = _Stream.CaptureUntil(YamlHeaderStopCharacters).Trim();
                _Stream.SkipWhitespace();

                if (!string.IsNullOrEmpty(key) && _Stream.Skip(Colon))
                {
                    _Stream.SkipWhitespace();

                    var value = _Stream.CaptureUntil(LineEndingCharacters).TrimEnd();
                    _Stream.SkipLineEnding();
                    _Output.YamlKeyValue(key, value);
                }
                else
                {
                    _Stream.Next();
                }
            }
            _Stream.Skip(TripleDash);
            _Stream.SkipLineEnding();
        }

        private bool UnderlineHeader()
        {
            if ((_Stream.Current != Dash && _Stream.Current != EqualSign) || !_Stream.IsStartOfLine)
                return false;

            // Check the line is made up of the same characters
            if (!_Stream.PeakLine(_Stream.Current, out int count))
                return false;

            char currentChar = _Stream.Current;

            // Remove the previous token and replace with a header
            if (_Output.Current?.Type == MarkdownTokenType.Text)
            {
                var previousToken = _Output.Pop();

                _Stream.Skip(count + 1);
                _Output.Header(currentChar == EqualSign ? 1 : 2, previousToken.Text, null, lineBreak: (_Stream.SkipLineEnding(max: 0) > 1));

                return true;
            }
            return false;
        }

        /// <summary>
        /// Process hash header.
        /// </summary>
        private bool HashHeader()
        {
            if (_Stream.Current != Hash || !_Stream.IsStartOfLine)
                return false;

            _Stream.MarkExtentStart();
            _Stream.Next();

            // Get the header depth
            var headerDepth = _Stream.Skip(Hash, max: 0) + 1;

            // Capture to the end of the line
            _Stream.SkipWhitespace();
            var text = _Stream.CaptureLine();
            var extent = _Stream.GetExtent();

            _Output.Header(headerDepth, text, extent, lineBreak: (_Stream.SkipLineEnding(max: 0) > 1));
            return true;
        }

        /// <summary>
        /// Process a fenced block.
        /// </summary>
        private bool FencedBlock()
        {
            if (_Stream.Current != Backtick || !_Stream.IsSequence(TripleBacktick, onNewLine: true))
            {
                return false;
            }

            _Stream.MarkExtentStart();

            // Skip backticks
            _Stream.Skip(3);

            // Get info-string
            var info = _Stream.CaptureLine();
            _Stream.SkipLineEnding();

            // Capture text within code fence
            var text = _Stream.CaptureUntil(TripleBacktick, onNewLine: true, ignoreEscaping: true);

            // Skip backticks
            _Stream.Skip(TripleBacktick);

            // Write code fence beginning
            _Output.FencedBlock(info, text, null, lineBreak: _Stream.SkipLineEnding(max: 0) > 1);
            return true;
        }

        private bool LineBreak()
        {
            if (_Stream.Current != '\r' && _Stream.Current != '\n')
                return false;

            if (_Stream.IsSequence("\r\n"))
            {
                var breakCount = _Stream.SkipLineEnding(max: 0, ignoreEscaping: _PreserveFormatting);

                if (_PreserveFormatting)
                    _Output.LineBreak(count: breakCount);

                return true;
            }
            return false;
        }

        private void Text()
        {
            _Stream.MarkExtentStart();

            // Set the default style
            var textStyle = MarkdownTokens.None;

            var startOfLine = _Stream.IsStartOfLine;

            // Get the text
            var text = _PreserveFormatting ? _Stream.CaptureUntil(LineEndingCharacters, ignoreEscaping: true) : UnwrapStyleMarkers(_Stream, out textStyle);

            // Set the line ending
            var ending = GetEnding(_Stream.SkipLineEnding(max: 2));

            if (string.IsNullOrWhiteSpace(text) && !_PreserveFormatting)
                return;

            if (_Context != MarkdownReaderMode.List && startOfLine && IsList(text))
            {
                _Context = MarkdownReaderMode.List;
                if (_Output.Current != null && _Output.Current.Flag.IsEnding() && !_Output.Current.Flag.ShouldPreserve())
                    _Output.Current.Flag |= MarkdownTokens.Preserve;
            }

            // Override line ending if the line was a list item so that the line ending is preserved
            if (_Context == MarkdownReaderMode.List && ending.IsEnding())
                ending |= MarkdownTokens.Preserve;

            // Add the text to the output stream
            _Output.Text(text, flag: textStyle | ending);

            if (_Context == MarkdownReaderMode.List && ending.IsEnding())
                _Context = MarkdownReaderMode.None;
        }

        private static string UnwrapStyleMarkers(MarkdownStream stream, out MarkdownTokens flag)
        {
            flag = MarkdownTokens.None;

            // Check for style
            var styleChar = stream.Current;
            var stylePrevious = stream.Previous;
            var styleCount = styleChar == Asterix || styleChar == Underscore ? stream.Skip(styleChar, max: 0) : 0;
            var codeCount = styleChar == Backtick ? stream.Skip(Backtick, max: 0) : 0;

            var text = stream.CaptureUntil(IsTextStop, ignoreEscaping: false);

            // Check for italic and bold endings
            if (styleCount > 0)
            {
                if (stream.Current == styleChar)
                {
                    var styleEnding = stream.Skip(styleChar, max: styleCount);

                    // Add back underscores within text
                    if (styleChar == Underscore && stylePrevious != Whitespace)
                        return Pad(text, styleChar, left: styleCount, right: styleCount);

                    // Add back asterixes/underscores that are part of text
                    if (styleEnding < styleCount)
                        text = Pad(text, styleChar, left: styleCount - styleEnding);

                    if (styleEnding == 1 || styleEnding == 3)
                        flag |= MarkdownTokens.Italic;

                    if (styleEnding >= 2)
                        flag |= MarkdownTokens.Bold;
                }
                else
                {
                    // Add back asterixes/underscores that are part of text
                    text = Pad(text, styleChar, left: styleCount);
                }
            }

            if (codeCount > 0)
            {
                if (stream.Current == styleChar)
                {
                    var codeEnding = stream.Skip(styleChar, max: 1);

                    // Add back backticks that are part of text
                    if (codeEnding < codeCount)
                        text = Pad(text, styleChar, left: codeCount - codeEnding);

                    if (codeEnding == 1)
                        flag |= MarkdownTokens.Code;
                }
                else
                {
                    // Add back backticks that are part of text
                    text = Pad(text, styleChar, left: codeCount);
                }
            }
            return text;
        }

        private static string Pad(string text, char c, int left = 0, int right = 0)
        {
            return text.PadLeft(text.Length + left, c).PadRight(text.Length + left + right, c);
        }

        private static bool IsList(string text)
        {
            var clean = text.Trim();

            if (string.IsNullOrEmpty(clean))
                return false;

            var firstChar = clean[0];

            if (firstChar == Dash || firstChar == Asterix)
                return true;

            return false;
        }

        private static MarkdownTokens GetEnding(int lineEndings)
        {
            if (lineEndings == 0)
                return MarkdownTokens.None;

            return (lineEndings == 1) ? MarkdownTokens.LineEnding : MarkdownTokens.LineBreak;
        }

        /// <summary>
        /// Process link.
        /// </summary>
        private bool Link()
        {
            if (_Stream.Current != BracketOpen || _Stream.IsEscaped)
                return false;

            _Stream.MarkExtentStart();
            _Stream.Checkpoint();

            // Skip [
            _Stream.Next();

            // Find end ]
            var text = _Stream.CaptureUntil(LinkNameStopCharacters);

            // Check if closing bracket was found in line
            if (_Stream.Current != BracketClose)
            {
                // Ignore and add as text
                _Stream.Rollback();
                _Stream.Next();

                var ending = GetEnding(_Stream.SkipLineEnding(max: 0));

                _Output.Text("[", flag: ending);

                return true;
            }

            // Skip ]
            _Stream.Next();

            if (string.IsNullOrEmpty(text))
            {
                var ending = GetEnding(_Stream.SkipLineEnding(max: 0));

                _Output.Text("[]", flag: ending);

                return true;
            }

            // Check for link destination indicated by '('. i.e. [text](destination)
            if (_Stream.Skip(ParenthesesOpen))
            {
                var uri = _Stream.CaptureUntil(LinkUrlStopCharacters);

                // Check if closing bracket was found in line
                if (_Stream.Current != ParenthesesClose)
                {
                    // TODO: Looks like error, double check, will position be lost
                    return true;
                }

                // Skip )
                _Stream.Next();

                _Output.Link(text, uri);
            }
            // Check for link label indicated by '['. i.e. [text][label]
            else if (_Stream.Skip(BracketOpen))
            {
                var linkRef = _Stream.CaptureUntil(LinkNameStopCharacters);

                // Skip ]
                _Stream.Next();

                _Output.LinkReference(text, linkRef);
            }
            // Check for link reference definition indicated by ':'. i.e. [label]: destination
            else if (_Stream.Skip(Colon))
            {
                _Stream.SkipWhitespace();

                var destination = _Stream.CaptureUntil(LineEndingCharacters);

                _Output.LinkReferenceDefinition(text, destination);
            }
            else
            {
                _Output.LinkReference(text, text);
            }

            var extent = _Stream.GetExtent();
            return true;
        }

        private static bool IsTextStop(char c)
        {
            return c == '\r' || c == '\n' || c == '[' || c == '*' || c == '`' || c == '_';
        }
    }
}
