// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace PSRule.Parser
{
    internal static class TokenStreamExtensions
    {
        /// <summary>
        /// Add a header.
        /// </summary>
        public static void Header(this TokenStream stream, int depth, string text, SourceExtent extent, bool lineBreak)
        {
            stream.Add(new MarkdownToken()
            {
                Depth = depth,
                Extent = extent,
                Text = text,
                Type = MarkdownTokenType.Header,
                Flag = lineBreak ? MarkdownTokens.LineBreak : MarkdownTokens.LineEnding | MarkdownTokens.Preserve
            });
        }

        public static void YamlKeyValue(this TokenStream stream, string key, string value)
        {
            stream.Add(new MarkdownToken()
            {
                Meta = key,
                Text = value,
                Type = MarkdownTokenType.YamlKeyValue
            });
        }

        /// <summary>
        /// Add a code fence.
        /// </summary>
        public static void FencedBlock(this TokenStream stream, string meta, string text, SourceExtent extent, bool lineBreak)
        {
            stream.Add(new MarkdownToken()
            {
                Extent = extent,
                Meta = meta,
                Text = text,
                Type = MarkdownTokenType.FencedBlock,
                Flag = (lineBreak ? MarkdownTokens.LineBreak : MarkdownTokens.LineEnding) | MarkdownTokens.Preserve
            });
        }

        /// <summary>
        /// Add a line break.
        /// </summary>
        public static void LineBreak(this TokenStream stream, int count)
        {
            // Ignore line break at the very start of file
            if (stream.Count == 0)
            {
                return;
            }

            for (var i = 0; i < count; i++)
            {
                stream.Add(new MarkdownToken() { Type = MarkdownTokenType.LineBreak, Flag = MarkdownTokens.LineBreak });
            }
        }

        public static void Text(this TokenStream stream, string text, MarkdownTokens flag = MarkdownTokens.None)
        {
            if (MergeText(stream.Current, text, flag))
            {
                return;
            }

            stream.Add(new MarkdownToken() { Type = MarkdownTokenType.Text, Text = text, Flag = flag });
        }

        private static bool MergeText(MarkdownToken current, string text, MarkdownTokens flag)
        {
            // Only allow merge if the previous token was text
            if (current == null || current.Type != MarkdownTokenType.Text)
            {
                return false;
            }

            if (current.Flag.ShouldPreserve())
            {
                return false;
            }

            // If the previous token was text, lessen the break but still don't allow merging
            if (current.Flag.HasFlag(MarkdownTokens.LineBreak) && !current.Flag.ShouldPreserve())
            {
                return false;
            }

            // Text must have the same flags set
            if (current.Flag.HasFlag(MarkdownTokens.Italic) != flag.HasFlag(MarkdownTokens.Italic))
            {
                return false;
            }

            if (current.Flag.HasFlag(MarkdownTokens.Bold) != flag.HasFlag(MarkdownTokens.Bold))
            {
                return false;
            }

            if (current.Flag.HasFlag(MarkdownTokens.Code) != flag.HasFlag(MarkdownTokens.Code))
            {
                return false;
            }

            if (!current.Flag.IsEnding())
            {
                current.Text = string.Concat(current.Text, text);
            }
            else if (current.Flag == MarkdownTokens.LineEnding)
            {
                return false;
            }

            // Take on the ending of the merged token
            current.Flag = flag;

            return true;
        }

        public static void Link(this TokenStream stream, string text, string uri)
        {
            stream.Add(new MarkdownToken() { Type = MarkdownTokenType.Link, Meta = text, Text = uri });
        }

        public static void LinkReference(this TokenStream stream, string text, string linkRef)
        {
            stream.Add(new MarkdownToken() { Type = MarkdownTokenType.LinkReference, Meta = text, Text = linkRef });
        }

        public static void LinkReferenceDefinition(this TokenStream stream, string text, string linkTarget)
        {
            stream.Add(new MarkdownToken() { Type = MarkdownTokenType.LinkReferenceDefinition, Meta = text, Text = linkTarget });
        }

        /// <summary>
        /// Add a marker for the start of a paragraph.
        /// </summary>
        public static void ParagraphStart(this TokenStream stream)
        {
            stream.Add(new MarkdownToken() { Type = MarkdownTokenType.ParagraphStart });
        }

        /// <summary>
        /// Add a marker for the end of a paragraph.
        /// </summary>
        public static void ParagraphEnd(this TokenStream stream)
        {
            if (stream.Count > 0)
            {
                if (stream.Current.Type == MarkdownTokenType.ParagraphStart)
                {
                    stream.Pop();

                    return;
                }

                stream.Add(new MarkdownToken() { Type = MarkdownTokenType.ParagraphEnd });
            }
        }

        public static IEnumerable<MarkdownToken> GetSection(this TokenStream stream, string header)
        {
            if (stream.Count == 0)
                return Enumerable.Empty<MarkdownToken>();

            return stream
                // Skip until we reach the header
                .SkipWhile(token => token.Type != MarkdownTokenType.Header || token.Text != header)

                // Get all tokens to the next header
                .Skip(1)
                .TakeWhile(token => token.Type != MarkdownTokenType.Header);
        }

        public static IEnumerable<MarkdownToken> GetSections(this TokenStream stream)
        {
            if (stream.Count == 0)
                return Enumerable.Empty<MarkdownToken>();

            return stream
                // Skip until we reach the header
                .SkipWhile(token => token.Type != MarkdownTokenType.Header)

                // Get all tokens to the next header
                .Skip(1)
                .TakeWhile(token => token.Type != MarkdownTokenType.Header);
        }
    }

    [DebuggerDisplay("Current = {Current?.Text}")]
    internal sealed class TokenStream : IEnumerable<MarkdownToken>
    {
        private readonly List<MarkdownToken> _Token;
        private readonly Dictionary<string, MarkdownToken> _LinkTargetIndex;

        private int _Position;

        public TokenStream()
        {
            _Token = new List<MarkdownToken>();
            _LinkTargetIndex = new Dictionary<string, MarkdownToken>(StringComparer.OrdinalIgnoreCase);
        }

        public TokenStream(IEnumerable<MarkdownToken> tokens)
            : this()
        {
            foreach (var token in tokens)
                Add(token);
        }

        #region Properties

        public bool EOF => _Position >= _Token.Count;

        public MarkdownToken Current => (_Token.Count <= _Position) ? null : _Token[_Position];

        public int Position => _Position;

        public int Count => _Token.Count;

        #endregion Properties

        public bool IsTokenType(params MarkdownTokenType[] tokenType)
        {
            if (Current == null || tokenType == null)
                return false;

            if (tokenType.Length == 1)
                return tokenType[0] == Current.Type;

            return tokenType.Contains(Current.Type);
        }

        public MarkdownTokenType PeakTokenType(int offset = 1)
        {
            var p = _Position + offset;
            return p < 0 || p >= _Token.Count ? MarkdownTokenType.None : _Token[p].Type;
        }

        public MarkdownToken Peak(int offset = 1)
        {
            var p = _Position + offset;
            return p < 0 || p >= _Token.Count ? null : _Token[p];
        }

        public void SkipUntilHeader()
        {
            SkipUntil(MarkdownTokenType.Header);
        }

        public void SkipUntil(MarkdownTokenType tokenType)
        {
            while (!EOF && Current.Type != tokenType)
                Next();
        }

        public IEnumerable<MarkdownToken> CaptureUntil(MarkdownTokenType tokenType)
        {
            var start = Position;
            var count = 0;
            while (!EOF && Current.Type != tokenType)
            {
                count++;
                Next();
            }
            return _Token.GetRange(start, count);
        }

        public IEnumerable<MarkdownToken> CaptureWhile(params MarkdownTokenType[] tokenType)
        {
            var start = Position;
            var count = 0;
            while (!EOF && IsTokenType(tokenType))
            {
                count++;
                Next();
            }
            return _Token.GetRange(start, count);
        }

        public void Add(MarkdownToken token)
        {
            _Token.Add(token);
            _Position = _Token.Count - 1;

            // CommonMark specifies that link labels are case-insensitive, and
            // first reference definition takes prescidence when multiple definitions use the same link label
            if (token.Type == MarkdownTokenType.LinkReferenceDefinition && !_LinkTargetIndex.ContainsKey(token.Meta))
            {
                _LinkTargetIndex[token.Meta] = token;
            }
        }

        public bool Next()
        {
            _Position++;
            return !EOF;
        }

        public MarkdownToken Pop()
        {
            if (Count == 0)
                return null;

            var token = _Token[_Position];
            _Token.RemoveAt(_Position);
            _Position = _Token.Count - 1;
            return token;
        }

        public void MoveTo(int position)
        {
            _Position = position;
        }

        public MarkdownToken ResolveLinkTarget(string name)
        {
            if (!_LinkTargetIndex.ContainsKey(name))
                return null;

            return _LinkTargetIndex[name];
        }

        public IEnumerator<MarkdownToken> GetEnumerator()
        {
            return ((IEnumerable<MarkdownToken>)_Token).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<MarkdownToken>)_Token).GetEnumerator();
        }
    }
}
