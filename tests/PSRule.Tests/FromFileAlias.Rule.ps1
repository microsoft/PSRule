# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

# Synopsis: A rule with an alias.
Rule 'PS.RuleWithAlias1' -Ref 'PSRZZ.0001' -Alias 'PS.AlternativeName' {
    $Assert.HasField($TargetObject, 'name');
}
