// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Collections;

namespace PSRule
{
    /// <summary>
    /// Defined a readonly hashtable.
    /// </summary>
    public sealed class ReadOnlyHashtable : Hashtable
    {
        internal ReadOnlyHashtable(IDictionary dictionary, IEqualityComparer equalityComparer)
            : base(dictionary, equalityComparer) { }


        /// <inheritdoc/>
        public override bool IsReadOnly => true;
    }
}
