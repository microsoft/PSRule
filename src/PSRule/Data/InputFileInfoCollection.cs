// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PSRule.Data
{
    /// <summary>
    /// A collection of <see cref="InputFileInfo"/>.
    /// </summary>
    public interface IInputFileInfoCollection : IEnumerable<InputFileInfo>
    {
        /// <summary>
        /// Filters the collection to only include <see cref="InputFileInfo"/> with a specific file extension.
        /// </summary>
        /// <param name="extension">A file extension to filter the collection to.</param>
        /// <returns>A filtered collection.</returns>
        IInputFileInfoCollection WithExtension(string extension);
    }

    /// <summary>
    /// A collection of <see cref="InputFileInfo"/>.
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
            _Items = items != null && items.Length > 0 ? items.Select(i => new InputFileInfo(basePath, i)).ToArray() : Array.Empty<InputFileInfo>();
        }

        public IInputFileInfoCollection WithExtension(string extension)
        {
            return new InputFileInfoCollection(_Items.Where(i => i.Extension == extension));
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
}
