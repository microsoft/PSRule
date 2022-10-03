// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PSRule.Data
{
    public interface IInputFileInfoCollection : IEnumerable<InputFileInfo>
    {
        IInputFileInfoCollection WithExtension(string extension);
    }

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

        //public IInputFileInfoCollection WithPath()
        //{
        //    return new InputFileInfoCollection(_Items.Where(i => i.));
        //}

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
