// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.


namespace PSRule.Emitters;

#nullable enable

internal sealed class ReplaceTextReader(TextReader textReader, KeyValuePair<string, string>[] replacementTokens) : TextReader
{
    private readonly TextReader _TextReader = textReader;
    private readonly KeyValuePair<string, string>[] _ReplacementTokens = replacementTokens;

    private bool _Disposed;
    private StringReader? _StringReader;

    public override void Close()
    {
        _TextReader.Close();
    }

    public override int Read()
    {
        _StringReader ??= GetStringReader();
        return _StringReader.Read();
    }

    public override int Read(char[] buffer, int index, int count)
    {
        _StringReader ??= GetStringReader();
        return _StringReader.Read(buffer, index, count);
    }

    public override int Peek()
    {
        _StringReader ??= GetStringReader();
        return _StringReader.Peek();
    }

    public override string ReadLine()
    {
        _StringReader ??= GetStringReader();
        return _StringReader.ReadLine();
    }

    public override string ReadToEnd()
    {
        _StringReader ??= GetStringReader();
        return _StringReader.ReadToEnd();
    }

    public override Task<int> ReadAsync(char[] buffer, int index, int count)
    {
        throw new NotImplementedException();
    }

    public override Task<string> ReadLineAsync()
    {
        throw new NotImplementedException();
    }

    public override int ReadBlock(char[] buffer, int index, int count)
    {
        throw new NotImplementedException();
    }

    public override Task<int> ReadBlockAsync(char[] buffer, int index, int count)
    {
        throw new NotImplementedException();
    }

    public override Task<string> ReadToEndAsync()
    {
        throw new NotImplementedException();
    }

    private StringReader GetStringReader()
    {
        var s = _TextReader.ReadToEnd();
        foreach (var token in _ReplacementTokens)
        {
            s = s.Replace(token.Key, token.Value);
        }

        return new StringReader(s);
    }

    #region IDisposable

    protected override void Dispose(bool disposing)
    {
        if (!_Disposed)
        {
            if (disposing)
            {
                _TextReader.Dispose();
            }
            _Disposed = true;
        }
    }

    #endregion IDisposable
}

#nullable restore
