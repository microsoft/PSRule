// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Tool.Models;

/// <summary>
/// A record of a module within the lock file.
/// </summary>
/// <param name="Name">The name of the module.</param>
/// <param name="Version">The version of the module.</param>
/// <param name="Installed">Is the version is installed.</param>
/// <param name="Locked">Is the module tracked.</param>
internal sealed record ModuleRecord(string Name, string Version, bool Installed, bool Locked);
