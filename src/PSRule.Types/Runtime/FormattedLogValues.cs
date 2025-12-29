// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;

namespace PSRule.Runtime;

/// <summary>
/// Enable formatted log values in diagnostic messages.
/// </summary>
internal readonly struct FormattedLogValues(string? format, params object?[]? values) : IReadOnlyList<KeyValuePair<string, object?>>
{
    private const string NullFormat = "[null]";

    private readonly object?[]? _Values = values;
    private readonly string _OriginalMessage = format ?? NullFormat;

    public KeyValuePair<string, object?> this[int index]
    {
        get
        {
            if (index < 0 || index >= Count) throw new IndexOutOfRangeException(nameof(index));

            return index == Count - 1
                ? new KeyValuePair<string, object?>("{OriginalFormat}", _OriginalMessage)
                : new KeyValuePair<string, object?>($"{index}", _Values?[index]);
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
        for (var i = 0; i < Count; ++i)
        {
            yield return this[i];
        }
    }

    public override string ToString()
    {
        return _Values == null || _Values.Length == 0
            ? _OriginalMessage
            : string.Format(Thread.CurrentThread.CurrentCulture, _OriginalMessage, _Values);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
