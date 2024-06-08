// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Runtime.ObjectPath;

internal sealed class TokenReader : ITokenReader
{
    private readonly IPathToken[] _Tokens;
    private readonly int _Last;

    private int _Index;

    public TokenReader(IPathToken[] tokens)
    {
        _Tokens = tokens;
        _Last = tokens.Length - 1;
        _Index = -1;
    }

    public IPathToken Current { get; private set; }

    public bool Consume(PathTokenType type)
    {
        return Peak(out var token) && token.Type == type && Next();
    }

    public bool Next(out IPathToken token)
    {
        token = null;
        if (!Next())
            return false;

        token = Current;
        return true;
    }

    private bool Next()
    {
        Current = _Index < _Last ? _Tokens[++_Index] : null;
        return Current != null;
    }

    public bool Peak(out IPathToken token)
    {
        token = _Index < _Last ? _Tokens[_Index + 1] : null;
        return token != null;
    }
}
