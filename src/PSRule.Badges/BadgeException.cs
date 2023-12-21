// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Runtime.Serialization;

namespace PSRule.Badges;

/// <summary>
/// A generic exception for handling of badges.
/// </summary>
[Serializable]
internal class BadgeException : Exception
{
    public BadgeException()
    {
    }

    public BadgeException(string message) : base(message)
    {
    }

    public BadgeException(string message, Exception innerException) : base(message, innerException)
    {
    }

    protected BadgeException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
