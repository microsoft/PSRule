// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;

namespace PSRule.Runtime;

/// <summary>
/// Enable formatted log values in diagnostic messages.
/// </summary>
internal readonly struct FormattedLogValues : IReadOnlyList<KeyValuePair<string, object?>>
{
    private const string NullFormat = "[null]";

    private readonly object?[]? _Values;
    private readonly string _OriginalMessage;

    public FormattedLogValues(string? format, params object?[]? values)
    {
        _OriginalMessage = format ?? NullFormat;
        _Values = values;
    }

    public KeyValuePair<string, object?> this[int index]
    {
        get
        {
            if (index < 0 || index >= Count) throw new IndexOutOfRangeException(nameof(index));

            if (index == Count - 1)
            {
                return new KeyValuePair<string, object?>("{OriginalFormat}", _OriginalMessage);
            }

            return new KeyValuePair<string, object?>($"{index}", _Values?[index]);
        }
    }

    public int Count
    {
        get
        {
            return _Values == null ? 1 : _Values.Length + 1;
        }
    }

    public IEnumerator<KeyValuePair<string, object?>> GetEnumerator()
    {
        for (int i = 0; i < Count; ++i)
        {
            yield return this[i];
        }
    }

    public override string ToString()
    {
        return string.Format(Thread.CurrentThread.CurrentCulture, _OriginalMessage, _Values);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
