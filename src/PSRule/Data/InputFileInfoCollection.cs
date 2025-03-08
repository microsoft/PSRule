// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;
using PSRule.Expressions;

namespace PSRule.Data;

/// <summary>
/// A collection of <see cref="InputFileInfo"/> that can be filtered.
/// </summary>
internal sealed class InputFileInfoCollection : IInputFileInfoCollection, IEnumerable<InputFileInfo>
{
    private readonly IEnumerable<InputFileInfo> _Items;

    public InputFileInfoCollection(IEnumerable<InputFileInfo> items)
    {
        _Items = items;
    }

    public InputFileInfoCollection(string basePath, string[] items)
    {
        _Items = items != null && items.Length > 0 ? [.. items.Select(i => new InputFileInfo(basePath, i))] : [];
    }

    public IInputFileInfoCollection WithExtension(string extension)
    {
        return new InputFileInfoCollection(_Items.Where(i => i.Extension == extension));
    }

    public IInputFileInfoCollection WithinPath(string path, bool caseSensitive = false)
    {
        return new InputFileInfoCollection(_Items.Where(i => PathHelpers.WithinPath(i.Path, path, caseSensitive)));
    }

    #region IEnumerable<InputFileInfo>

    public IEnumerator<InputFileInfo> GetEnumerator()
    {
        return _Items.GetEnumerator();
    }

    #endregion IEnumerable<InputFileInfo>

    #region IEnumerable

    IEnumerator IEnumerable.GetEnumerator()
    {
        return _Items.GetEnumerator();
    }

    #endregion IEnumerable
}
