// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Diagnostics;

namespace PSRule.Runtime.ObjectPath;

/// <summary>
/// A helper to tokenize an object path expression.
/// </summary>
internal static class PathTokenizer
{
    private sealed class TokenStream : ITokenWriter
    {
        private readonly List<IPathToken> _Items;

        public TokenStream()
        {
            _Items = new List<IPathToken>();
        }

        public IPathToken? Last
        {
            get
            {
                return _Items.Count > 0 ? _Items[_Items.Count - 1] : null;
            }
        }

        public void Add(IPathToken token)
        {
            _Items.Add(token);
        }

        public IPathToken[] ToArray()
        {
            return _Items.ToArray();
        }
    }

    [DebuggerDisplay("Position = {Position}")]
    private sealed class PathStream
    {
        private const char ROOTREF = '$';
        private const char CURRENTREF = '@';
        private const char DOT = '.';
        private const char QUOTED_SINGLE = '\'';
        private const char QUOTED_DOUBLE = '"';
        private const char INDEX_OPEN = '[';
        private const char INDEX_CLOSE = ']';
        private const char ANY = '*';
        private const char DASH = '-';
        private const char QUERY = '?';
        private const char GROUP_OPEN = '(';
        private const char GROUP_CLOSE = ')';
        private const char EQUALS = '=';
        private const char NOT = '!';
        private const char LESSTHAN = '<';
        private const char GREATERTHAN = '>';
        private const char TILDA = '~';
        private const char NULL = '\0';
        private const char COLON = ':';
        private const char COMMA = ',';
        private const char OR = '|';
        private const char AND = '&';
        private const char UNDERSCORE = '_';
        private const char PLUS = '+';

        private readonly string _Path;
        private readonly int _Last;

        public PathStream(string path)
        {
            _Path = path;
            _Last = _Path.Length - 1;
        }

        public bool EOF(int position)
        {
            return position > _Last;
        }

        public char Current(int pos)
        {
            return pos > _Last ? NULL : _Path[pos];
        }

        public bool Current(int pos, char c)
        {
            return pos <= _Last && _Path[pos] == c;
        }

        /// <summary>
        /// Find the start of the sequence.
        /// </summary>
        /// <returns>Return true when more characters follow.</returns>
        public void Next(ref int position)
        {
            if (position <= _Last)
                position++;
        }

        /// <summary>
        /// Capture a token for $ and @.
        /// </summary>
        internal bool TryConsumeRef(ref int position, ITokenWriter tokens)
        {
            if ((Current(position) == ROOTREF && !IsMemberName(position)) || (position == 0 && Current(position) == DOT))
            {
                tokens.Add(PathToken.RootRef);
                Next(ref position);
                return true;
            }
            if (Current(position) == CURRENTREF)
            {
                tokens.Add(position == 0 ? PathToken.RootRef : PathToken.CurrentRef);
                Next(ref position);
                return true;
            }
            return false;
        }

        internal bool TryConsumeChild(ref int position, ITokenWriter tokens)
        {
            return TryConsumeDotWildSelector(ref position, tokens) ||
                TryConsumeDotSelector(ref position, tokens) ||
                TryConsumeIndexSelector(ref position, tokens) ||
                TryConsumeDescendantSelector(ref position, tokens);
        }

        /// <summary>
        /// Capture a token for "[?(@.enabled==true)]".
        /// </summary>
        internal bool TryConsumeFilter(ref int position, ITokenWriter tokens)
        {
            if (Current(position) != INDEX_OPEN || position + 5 >= _Last || _Path[position + 1] != QUERY)
                return false;

            var groupOpen = _Path[position + 2] == GROUP_OPEN;
            var pos = groupOpen ? position + 3 : position + 2;
            tokens.Add(new PathToken(PathTokenType.StartFilter));
            if (groupOpen)
                tokens.Add(new PathToken(PathTokenType.StartGroup));

            while (!EOF(pos) && Current(pos) != INDEX_CLOSE)
            {
                if (!TryConsumeBooleanExpression(ref pos, tokens) && !TryConsumeGroup(ref pos, tokens))
                    Next(ref pos);

                pos = SkipPadding(pos);
            }

            if (Current(pos) == INDEX_CLOSE)
                Next(ref pos);

            tokens.Add(new PathToken(PathTokenType.EndFilter));
            position = SkipPadding(pos);
            return true;
        }

        private bool TryConsumeGroup(ref int position, ITokenWriter tokens)
        {
            if (Current(position) != GROUP_OPEN && Current(position) != GROUP_CLOSE)
                return false;

            tokens.Add(new PathToken(Current(position) == GROUP_OPEN ? PathTokenType.StartGroup : PathTokenType.EndGroup));
            position++;
            TryConsumeLogicalOperator(ref position, tokens);
            return true;
        }

        private bool TryConsumeBooleanExpression(ref int position, ITokenWriter tokens)
        {
            if (!TryConsumeRef(ref position, tokens) && !TryConsumeDescendantSelector(ref position, tokens) && !TryConsumeDotSelector(ref position, tokens) && !TryConsumeNot(ref position, tokens))
                return false;

            if (tokens.Last.Type == PathTokenType.NotOperator)
                TryConsumeRef(ref position, tokens);

            TryConsumeDescendantSelector(ref position, tokens);
            TryConsumeDotSelector(ref position, tokens);
            TryConsumeComparisonOperator(ref position, tokens);
            TryConsumePrimitive(ref position, tokens);
            TryConsumeLogicalOperator(ref position, tokens);
            return true;
        }

        private bool TryConsumeNot(ref int position, ITokenWriter tokens)
        {
            if (Current(position) != NOT)
                return false;

            tokens.Add(new PathToken(PathTokenType.NotOperator));
            position += 1;
            return true;
        }

        private bool TryConsumePrimitive(ref int position, ITokenWriter tokens)
        {
            return TryConsumeNumberLiteral(ref position, tokens) ||
                TryConsumeStringLiteral(ref position, tokens) ||
                TryConsumeBooleanLiteral(ref position, tokens);
        }

        private bool TryConsumeStringLiteral(ref int position, ITokenWriter tokens)
        {
            if (!IsQuoted(Current(position)))
                return false;

            if (!UntilQuote(ref position, out var value))
                return false;

            tokens.Add(new PathToken(PathTokenType.String, value));
            position = SkipPadding(position);
            return true;
        }

        private bool TryConsumeNumberLiteral(ref int position, ITokenWriter tokens)
        {
            var pos = SkipPadding(position);
            if (!TryInteger(pos, out var value))
                return false;

            tokens.Add(new PathToken(PathTokenType.Integer, value));
            position = pos + value.ToString().Length;
            return true;
        }

        private bool TryConsumeBooleanLiteral(ref int position, ITokenWriter tokens)
        {
            var pos = SkipPadding(position);
            if (!TryBoolean(pos, out var value))
                return false;

            tokens.Add(new PathToken(PathTokenType.Boolean, value));
            position = pos + value.ToString().Length;
            return true;
        }

        private void TryConsumeComparisonOperator(ref int position, ITokenWriter tokens)
        {
            var pos = SkipPadding(position);
            if (!IsComparisonOperator(Current(pos)))
                return;

            var op = FilterOperator.None;
            var c1 = ConsumeChar(ref pos);
            var c2 = Current(pos);

            if (c1 == EQUALS && c2 == EQUALS)
                op = FilterOperator.Equal;

            if (c1 == NOT && c2 == EQUALS)
                op = FilterOperator.NotEqual;

            if (c1 == LESSTHAN && c2 == EQUALS)
                op = FilterOperator.LessOrEqual;

            if (c1 == LESSTHAN && !IsComparisonOperator(c2))
                op = FilterOperator.Less;

            if (c1 == GREATERTHAN && c2 == EQUALS)
                op = FilterOperator.GreaterOrEqual;

            if (c1 == GREATERTHAN && !IsComparisonOperator(c2))
                op = FilterOperator.Greater;

            if (c1 == TILDA && c2 == EQUALS)
                op = FilterOperator.RegEx;

            if (op != FilterOperator.None)
            {
                position = SkipPadding(op == FilterOperator.Less || op == FilterOperator.Greater ? pos : pos + 1);
                tokens.Add(new PathToken(PathTokenType.ComparisonOperator, op));
            }
        }

        private void TryConsumeLogicalOperator(ref int position, ITokenWriter tokens)
        {
            var pos = SkipPadding(position);
            if (!IsLogicalOperator(Current(pos)))
                return;

            IPathToken? token = null;
            var c1 = ConsumeChar(ref pos);
            var c2 = ConsumeChar(ref pos);

            if (c1 == OR && c2 == OR)
                token = new PathToken(PathTokenType.LogicalOperator, FilterOperator.Or);

            if (c1 == AND && c2 == AND)
                token = new PathToken(PathTokenType.LogicalOperator, FilterOperator.And);

            if (token != null)
            {
                position = SkipPadding(pos);
                tokens.Add(token);
            }
        }

        private char ConsumeChar(ref int position)
        {
            return position > _Last ? NULL : _Path[position++];
        }

        /// <summary>
        /// Check if current is a property operator.
        /// </summary>
        /// <remarks>
        /// "." or "+" but not ".."
        /// </remarks>
        private bool IsDotSelector(int position)
        {
            return (Current(position, DOT) && !Current(position + 1, DOT)) || Current(position, PLUS);
        }

        private bool IsDotWildcardSelector(int position)
        {
            return Current(position, DOT) && Current(position + 1, ANY);
        }

        private bool IsDescendantSelector(int position)
        {
            return Current(position, DOT) && Current(position + 1, DOT);
        }

        private static bool IsComparisonOperator(char c)
        {
            return c == EQUALS || c == NOT || c == LESSTHAN || c == GREATERTHAN || c == TILDA;
        }

        private static bool IsLogicalOperator(char c)
        {
            return c == OR || c == AND;
        }

        private static bool IsMemberNameCharacter(char c)
        {
            return char.IsLetterOrDigit(c) || c == UNDERSCORE || c == DASH;
        }

        private static bool IsQuoted(char c)
        {
            return c == QUOTED_SINGLE || c == QUOTED_DOUBLE;
        }

        private bool TryConsumeDotSelector(ref int position, ITokenWriter tokens)
        {
            if (!IsDotSelector(position) && !Current(position, QUOTED_SINGLE) && !Current(position, QUOTED_DOUBLE) && !IsMemberName(position))
                return false;

            var pos = IsDotSelector(position) ? position + 1 : position;
            var option = Current(position, PLUS) ? PathTokenOption.CaseSensitive : PathTokenOption.None;
            var field = CaptureMemberName(ref pos);
            if (string.IsNullOrEmpty(field))
                return false;

            tokens.Add(new PathToken(PathTokenType.DotSelector, field, option));
            position = pos;
            return true;
        }

        private bool TryConsumeDotWildSelector(ref int position, ITokenWriter tokens)
        {
            if (!IsDotWildcardSelector(position))
                return false;

            tokens.Add(new PathToken(PathTokenType.DotWildSelector));
            position += 2;
            return true;
        }

        private bool TryConsumeDescendantSelector(ref int position, ITokenWriter tokens)
        {
            if (!IsDescendantSelector(position))
                return false;

            var pos = position + 2;
            var field = CaptureMemberName(ref pos);
            if (string.IsNullOrEmpty(field))
                return false;

            tokens.Add(new PathToken(PathTokenType.DescendantSelector, field));
            position = pos;
            return true;
        }

        private bool TryConsumeIndexSelector(ref int position, ITokenWriter tokens)
        {
            if (!(Current(position) == INDEX_OPEN && Current(position + 1) != QUERY && position + 1 < _Last))
                return false;

            // Move past "["
            position++;
            return TryConsumeArraySliceSelector(ref position, tokens) ||
                TryConsumeUnionSelector(ref position, tokens) ||
                TryConsumeNumericIndex(ref position, tokens) ||
                TryConsumeIndexWildSelector(ref position, tokens) ||
                TryConsumeStringIndex(ref position, tokens);
        }

        /// <summary>
        /// Capture a token for: [::1]
        /// </summary>
        private bool TryConsumeArraySliceSelector(ref int position, ITokenWriter tokens)
        {
            if (!AnyUntilIndexClose(position, COLON))
                return false;

            var pos = SkipPadding(position);
            var slice = new int?[] { null, null, null };
            for (var i = 0; i <= 2 && pos <= _Last && _Path[pos] != INDEX_CLOSE; i++)
            {
                if (WhileNumeric(pos, out var end) && end > pos)
                {
                    slice[i] = int.Parse(Substring(pos, end));
                    pos = Current(end, COLON) ? end + 1 : end;
                }
                else
                {
                    pos++;
                }
            }
            position = ++pos;
            tokens.Add(new PathToken(PathTokenType.ArraySliceSelector, slice));
            return true;
        }

        /// <summary>
        /// Capture a token for: [,]
        /// </summary>
        private bool TryConsumeUnionSelector(ref int position, ITokenWriter tokens)
        {
            return AnyUntilIndexClose(position, COMMA) &&
                (TryConsumeUnionQuotedMemberSelector(ref position, tokens) || TryConsumeUnionIndexSelector(ref position, tokens));
        }

        private bool TryConsumeUnionIndexSelector(ref int position, ITokenWriter tokens)
        {
            var pos = SkipPadding(position);
            if (pos + 2 >= _Last || !WhileNumeric(pos, out var end) || end == pos)
                return false;

            var members = new List<int>();
            while (pos <= _Last && _Path[pos] != INDEX_CLOSE)
            {
                pos = SkipPadding(pos);
                if (!WhileNumeric(pos, out end) || !int.TryParse(Substring(pos, end), out var member))
                    break;

                members.Add(member);
                pos = SkipPadding(end);
                if (Current(pos, COMMA))
                    pos++;
            }
            position = pos + 1;
            tokens.Add(new PathToken(PathTokenType.UnionIndexSelector, members.ToArray()));
            return true;
        }

        private bool TryConsumeUnionQuotedMemberSelector(ref int position, ITokenWriter tokens)
        {
            var pos = SkipPadding(position);
            if (pos + 3 >= _Last || !IsQuoted(_Path[pos]))
                return false;

            var members = new List<string>();
            while (pos <= _Last && _Path[pos] != INDEX_CLOSE)
            {
                pos = SkipPadding(pos);
                var member = CaptureMemberName(ref pos);
                if (string.IsNullOrEmpty(member))
                    break;

                members.Add(member);
                pos = SkipPadding(pos);
                if (Current(pos, COMMA))
                    pos++;
            }
            position = pos + 1;
            tokens.Add(new PathToken(PathTokenType.UnionQuotedMemberSelector, members.ToArray()));
            return true;
        }

        /// <summary>
        /// Capture a token for "['store']".
        /// </summary>
        private bool TryConsumeStringIndex(ref int position, ITokenWriter tokens)
        {
            var pos = SkipPadding(position);
            if (pos + 3 >= _Last || !IsQuoted(_Path[pos]))
                return false;

            var field = CaptureMemberName(ref pos);
            if (string.IsNullOrEmpty(field))
                return false;

            tokens.Add(new PathToken(PathTokenType.DotSelector, field));
            position = pos + 1;
            return true;
        }

        /// <summary>
        /// Capture a token for "[*]".
        /// </summary>
        private bool TryConsumeIndexWildSelector(ref int position, ITokenWriter tokens)
        {
            var pos = SkipPadding(position);
            if (pos >= _Last || _Path[pos] != ANY)
                return false;

            pos = SkipPadding(pos + 1);
            if (pos > _Last || _Path[pos] != INDEX_CLOSE)
                return false;

            pos++;
            tokens.Add(new PathToken(PathTokenType.IndexWildSelector));
            position = pos;
            return true;
        }

        /// <summary>
        /// Capture a token for "[0]".
        /// </summary>
        private bool TryConsumeNumericIndex(ref int position, ITokenWriter tokens)
        {
            var pos = SkipPadding(position);
            if (!WhileNumeric(pos, out var end) || !int.TryParse(Substring(pos, end), out var index))
                return false;

            pos = SkipPadding(end);
            if (pos > _Last || _Path[pos] != INDEX_CLOSE)
                return false;

            tokens.Add(new PathToken(PathTokenType.IndexSelector, index));
            position = pos;
            return true;
        }

        private string? CaptureMemberName(ref int position)
        {
            return UntilQuote(ref position, out var value) || WhileMemberName(ref position, out value) ? value : null;
        }

        private bool TryBoolean(int position, out bool value)
        {
            value = default;
            if (IsSequence(position, bool.FalseString))
            {
                value = false;
                return true;
            }
            if (IsSequence(position, bool.TrueString))
            {
                value = true;
                return true;
            }
            return false;
        }

        private bool TryInteger(int position, out int value)
        {
            value = default;
            return WhileNumeric(position, out var end) && int.TryParse(Substring(position, end), out value);
        }

        private bool IsSequence(int position, string sequence)
        {
            return position + sequence.Length <= _Last && string.Compare(_Path, position, sequence, 0, sequence.Length, StringComparison.OrdinalIgnoreCase) == 0;
        }

        private bool IsMemberName(int position)
        {
            var p = Current(position);
            var p1 = Current(position + 1);
            return IsMemberNameCharacter(p) || (p == ROOTREF && IsMemberNameCharacter(p1));
        }

        /// <summary>
        /// Skip whitespace.
        /// </summary>
        private int SkipPadding(int pos)
        {
            while (pos < _Last && char.IsWhiteSpace(_Path[pos]))
                pos++;

            return pos;
        }

        private string? Substring(int pos, int end)
        {
            return pos == end ? null : _Path.Substring(pos, end - pos);
        }

        /// <summary>
        /// Continue while the character is a member name.
        /// </summary>
        private bool WhileMemberName(ref int position, out string value)
        {
            value = null;
            if (position >= _Last)
                return false;

            var end = _Path[position] == ROOTREF ? position + 1 : position;
            while (end <= _Last && IsMemberNameCharacter(_Path[end]) && (end != position || _Path[end] != DASH))
                end++;

            if (end > position && Current(end - 1) == DASH)
                end--;

            if (position == end)
                return false;

            value = Substring(position, end);
            position = end;
            return true;
        }

        /// <summary>
        /// Continue while the character is numeric.
        /// </summary>
        private bool WhileNumeric(int position, out int end)
        {
            end = position;
            if (position >= _Last)
                return false;

            var i = position;
            if (i <= _Last && _Path[i] == DASH)
                i++;

            while (i <= _Last && (char.IsDigit(_Path[i]) || (_Path[i] == DASH && i + 1 < _Last && char.IsDigit(_Path[i + 1]))))
                end = ++i;

            return end > position;
        }

        /// <summary>
        /// Find the end of the quote (').
        /// </summary>
        private bool UntilQuote(ref int position, out string value)
        {
            value = null;
            if (position >= _Last || !IsQuoted(_Path[position]))
                return false;

            var endQuote = _Path[position];
            var pos = position + 1;
            var end = pos;
            while (end <= _Last && _Path[end] != endQuote)
                end++;

            if (pos == end)
                return false;

            value = Substring(pos, end);
            position = end + 1;
            return true;
        }

        private bool AnyUntilIndexClose(int position, char c)
        {
            for (var i = position; i <= _Last && _Path[i] != INDEX_CLOSE; i++)
                if (_Path[i] == c)
                    return true;

            return false;
        }
    }

    /// <summary>
    /// Get path tokens for a specific object path expression.
    /// </summary>
    /// <param name="path">The object path expression.</param>
    /// <returns>One or more path tokens.</returns>
    internal static IPathToken[] Get(string path)
    {
        var stream = new PathStream(path);
        var tokens = new TokenStream();
        var position = 0;
        while (!stream.EOF(position))
        {
            if (!(stream.TryConsumeRef(ref position, tokens) ||
                stream.TryConsumeChild(ref position, tokens) ||
                stream.TryConsumeFilter(ref position, tokens)))
            {
                stream.Next(ref position);
            }
        }
        return tokens.ToArray();
    }
}
