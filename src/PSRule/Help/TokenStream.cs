// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using System.Diagnostics;

namespace PSRule.Help;

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

    public MarkdownToken? Current => (_Token.Count <= _Position) ? null : _Token[_Position];

    public int Position => _Position;

    public int Count => _Token.Count;

    #endregion Properties

    public bool IsTokenType(params MarkdownTokenType[] tokenType)
    {
        return Current != null && tokenType != null &&
            (tokenType.Length == 1 ? tokenType[0] == Current.Type : tokenType.Contains(Current.Type));
    }

    public MarkdownTokenType PeakTokenType(int offset = 1)
    {
        var p = _Position + offset;
        return p < 0 || p >= _Token.Count ? MarkdownTokenType.None : _Token[p].Type;
    }

    public MarkdownToken? Peak(int offset = 1)
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
        // first reference definition takes precedence when multiple definitions use the same link label
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

    public MarkdownToken? Pop()
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

    public MarkdownToken? ResolveLinkTarget(string name)
    {
        return !_LinkTargetIndex.ContainsKey(name) ? null : _LinkTargetIndex[name];
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
