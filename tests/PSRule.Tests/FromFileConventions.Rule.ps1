# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

#
# Conventions for pesters tests
#

# Synopsis: A convention for unit testing
Export-PSRuleConvention 'Convention1' {
    Write-Debug -Message 'Process convention1'
    $PSRule.Data.count += 1;
} -Begin {
    Write-Debug -Message 'Begin convention1'
    $PSRule.Data.count += 1;
    $PSRule.Data['Order'] += 'Convention1|'
    $PSRule.Data['BeginConfiguration'] = $Configuration.BeginConfiguration

    # Add the initialize value to the result data.
    $PSRule.Data['InitializeConfiguration'] = $PSRule.GetService('Config').InitializeConfiguration
} -End {
    Write-Debug -Message 'End convention1'
} -Initialize {

    # Data isn't available in initialize so store the value and load it in begin.
    $PSRule.AddService('Config', [PSCustomObject]@{
        InitializeConfiguration = $Configuration.InitializeConfiguration
    })
}

# Synopsis: A convention for unit testing
Export-PSRuleConvention 'Convention2' {
    Write-Debug -Message 'Process convention2'
    $PSRule.Data.count *= 10;
} -Begin {
    Write-Debug -Message 'Begin convention2'
    $PSRule.Data.count *= 10;
} -End {
    Write-Debug -Message 'End convention2'
}

# Synopsis: A convention for unit testing
Export-PSRuleConvention 'Convention3' -If { $Assert.HasFieldValue($TargetObject, 'IfTest', 1) } {
    Write-Debug 'Convention3::Process'
    $PSRule.Data.count += 1000;
}

# Synopsis: A convention for unit testing
Export-PSRuleConvention 'Convention.Expansion' -Initialize {
    $newObject = [PSCustomObject]@{
        Name = 'TestObject3'
    };
    $PSRule.ImportWithType('ExpandCustomObject', @($newObject));
} -Begin {
    if ($TargetObject.Name -eq 'TestObject1') {
        $newObject = [PSCustomObject]@{
            Name = 'TestObject2'
        };
        $PSRule.Import($newObject);
        $PSRule.Import(@($newObject, $newObject));
    }
}

# Synopsis: A convention for unit testing
Export-PSRuleConvention 'Convention.WithException' -Begin {
    throw 'Some exception';
}

# Synopsis: A convention for unit testing
Export-PSRuleConvention 'Convention.Init' -Initialize {
    $PSRule.AddService('Store', [PSCustomObject]@{
        Name = 'TestObject1'
    });
}

# Synopsis: A rule for testing conventions
Rule 'ConventionTest' {
    $Assert.HasFieldValue($PSRule.Data, 'count', 1);

    $store = $PSRule.GetService('Store');
    $Assert.HasFieldValue($store, 'Name', 'TestObject1');
}

# Synopsis: A convention for unit testing localized data.
Export-PSRuleConvention 'Convention.WithLocalizedData' -Initialize {
    Write-Information -MessageData ($LocalizedData.WithLocalizedDataMessage -f 'Initialize')
} -Begin {
    Write-Information -MessageData ($LocalizedData.WithLocalizedDataMessage -f 'Begin')
} -Process {
    Write-Information -MessageData ($LocalizedData.WithLocalizedDataMessage -f 'Process')
} -End {
    Write-Information -MessageData ($LocalizedData.WithLocalizedDataMessage -f 'End')
}

# Synopsis: Test localized data in pre-condition.
Rule 'WithLocalizedDataPrecondition' -If { Write-Information -MessageData ($LocalizedData.WithLocalizedDataMessage -f 'Precondition'); $True; } {
    $Assert.Pass();
}
