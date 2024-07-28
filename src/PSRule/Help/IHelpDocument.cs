// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Definitions;

namespace PSRule.Help;

internal interface IHelpDocument
{
    string Name { get; }

    InfoString Synopsis { get; set; }

    InfoString Description { get; set; }

    Link[] Links { get; set; }
}
