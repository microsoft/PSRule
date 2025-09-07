// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Configuration;

/// <summary>
/// Used by custom binding functions.
/// </summary>
public delegate string BindTargetName(object targetObject);

public delegate string? BindTargetMethod(string[] propertyNames, bool caseSensitive, object targetObject, out string? path);
