// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System;
using System.Runtime.Serialization;
using System.Security.Permissions;
using PSRule.Pipeline;

namespace PSRule.Runtime.ObjectPath
{
    /// <summary>
    /// An exception thrown by PSRule when evaluating an object path.
    /// </summary>
    [Serializable]
    public sealed class ObjectPathEvaluateException : PipelineException
    {
        /// <inheritdoc/>
        public ObjectPathEvaluateException()
        {
        }

        /// <inheritdoc/>
        public ObjectPathEvaluateException(string message) : base(message)
        {
        }

        /// <inheritdoc/>
        public ObjectPathEvaluateException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <inheritdoc/>
        private ObjectPathEvaluateException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        /// <inheritdoc/>
        [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            base.GetObjectData(info, context);
        }
    }
}
