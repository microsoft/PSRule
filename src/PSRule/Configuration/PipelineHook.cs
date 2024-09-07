// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Configuration;

/// <summary>
/// Used by custom binding functions.
/// </summary>
public delegate string BindTargetName(object targetObject);

internal delegate string BindTargetMethod(string[] propertyNames, bool caseSensitive, bool preferTargetInfo, object targetObject, out string path);
internal delegate string BindTargetFunc(string[] propertyNames, bool caseSensitive, bool preferTargetInfo, object targetObject, BindTargetMethod next, out string path);
