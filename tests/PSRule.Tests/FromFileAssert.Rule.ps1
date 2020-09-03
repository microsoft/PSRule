# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

#
# Pester unit test rules for the assertion helper
#

Add-Member -InputObject $Assert -MemberType ScriptMethod -Name '_ExtensionIsValue' -Value {
    param ($value, $s)
    return $this.Create(("MethodValue$value" -eq $s))
}

# Synopsis: Test for $Assert extension with Add-Member
Rule 'Assert.AddMember' {
    $Assert._ExtensionIsValue(1, 'MethodValue1')
    $Assert._ExtensionIsValue(2, 'MethodValue2')
}

# Synopsis: Test for $Assert in script pre-conditions
Rule 'Assert.Precondition' -If { $Assert.StartsWith($TargetObject, 'Name', 'TestObject1') } {
    $True;
}

# Synopsis: Test for $Assert with self field
Rule 'Assert.Self' {
    $Assert.HasFieldValue($TargetObject.Name, '.', 'TestObject1')
    $Assert.Greater(3, '.', 2)
    $Assert.EndsWith('Name', '.', 'ame')
}

# Synopsis: Test for $Assert.Complete
Rule 'Assert.Complete' {
    $Assert.HasField($TargetObject, 'Name').Complete() -and
        $Assert.HasField($TargetObject, 'Type').Complete() -and
        $Assert.HasField($TargetObject, 'OtherField').Complete()
}

# Synopsis: Test for $Assert.Fail
Rule 'Assert.Fail' {
    $Assert.Fail('Reason 1')
    $Assert.Fail('Reason {0}', 2)
    $Assert.Fail('Reason {0}', 3).Reason('Reason 4').Reason('Reason {0}', '5')
}

# Synopsis: Test for $Assert.Contains
Rule 'Assert.Contains' {
    $Assert.Contains($TargetObject, 'OtherField', @('abc', 'th'))
}

# Synopsis: Test for $Assert.EndsWith
Rule 'Assert.EndsWith' {
    $Assert.EndsWith($TargetObject, 'Name', '1')
}

# Synopsis: Test for $Assert.FileHeader
Rule 'Assert.FileHeader' {
    $Assert.FileHeader($TargetObject, 'Path', @(
        'Copyright (c) Microsoft Corporation.'
        'Licensed under the MIT License.'
    ))
}

# Synopsis: Test for $Assert.FilePath
Rule 'Assert.FilePath' {
    $Assert.FilePath($TargetObject, 'Path')
    $Assert.FilePath($TargetObject, 'ParentPath', @('PSRule.Assert.Tests.ps1'))
}

# Synopsis: Test for $Assert.Greater
Rule 'Assert.Greater' {
    $Assert.Greater($TargetObject, 'CompareNumeric', 2)
    $Assert.Greater($TargetObject, 'CompareArray', 2)
    $Assert.Greater($TargetObject, 'CompareString', 2)
}

# Synopsis: Test for $Assert.GreaterOrEqual
Rule 'Assert.GreaterOrEqual' {
    $Assert.GreaterOrEqual($TargetObject, 'CompareNumeric', 3)
    $Assert.GreaterOrEqual($TargetObject, 'CompareArray', 3)
    $Assert.GreaterOrEqual($TargetObject, 'CompareString', 3)
}

# Synopsis: Test for $Assert.HasField
Rule 'Assert.HasField' {
    $Assert.HasField($TargetObject, 'Type')
}

# Synopsis: Test for $Assert.HasFieldValue
Rule 'Assert.HasFieldValue' {
    AnyOf {
        $Assert.HasFieldValue($TargetObject, 'Type')
        $Assert.HasFieldValue($TargetObject, 'Value')
        $Assert.HasFieldValue($TargetObject, 'String')
        $Assert.HasFieldValue($TargetObject, 'Array')
        $Assert.HasFieldValue($TargetObject, 'Name', 'TestObject1')
        $Assert.HasFieldValue($TargetObject, 'Int', 1)
        $Assert.HasFieldValue($TargetObject, 'Bool', $True)
    }
}

# Synopsis: Test for $Assert.HasDefaultValue
Rule 'Assert.HasDefaultValue' {
    $Assert.HasDefaultValue($TargetObject, 'OtherField', 'Other')
    $Assert.HasDefaultValue($TargetObject, 'NotBool', $True)
    $Assert.HasDefaultValue($TargetObject, 'Bool', $True)
    $Assert.HasDefaultValue($TargetObject, 'OtherBool', $True)
    $Assert.HasDefaultValue($TargetObject, 'OtherInt', 1)
}

# Synopsis: Test for $Assert.HasJsonSchema
Rule 'Assert.HasJsonSchema' {
    $schemas = @(
        "http://json-schema.org/draft-04/schema`#"
        "http://json-schema.org/draft-07/schema`#"
    )
    $Assert.HasJsonSchema($TargetObject)
    $Assert.HasJsonSchema($TargetObject, $schemas[1])
    $Assert.HasJsonSchema($TargetObject, $schemas)
}

# Synopsis: Test for $Assert.JsonSchema
Rule 'Assert.JsonSchema' {
    $Assert.JsonSchema($TargetObject, 'tests/PSRule.Tests/FromFile.Json.schema.json')
}

# Synopsis: Test for $Assert.In
Rule 'Assert.In' {
    $Assert.In($TargetObject, 'Name', @('TestObject2', 'TestObject3'))
    $Assert.In($TargetObject, 'Int', @(2, 3))
    $Assert.In($TargetObject, 'InArray', @('Item2'))
    $Assert.In($TargetObject, 'InArray', @('item1'), $True)
    $Assert.In($TargetObject, 'InArrayPSObject', @('item2'), $True)
}

# Synopsis: Test for $Assert.Less
Rule 'Assert.Less' {
    $Assert.Less($TargetObject, 'CompareNumeric', 2)
    $Assert.Less($TargetObject, 'CompareArray', 2)
    $Assert.Less($TargetObject, 'CompareString', 2)
}

# Synopsis: Test for $Assert.LessOrEqual
Rule 'Assert.LessOrEqual' {
    $Assert.LessOrEqual($TargetObject, 'CompareNumeric', 0)
    $Assert.LessOrEqual($TargetObject, 'CompareArray', 0)
    $Assert.LessOrEqual($TargetObject, 'CompareString', 0)
}

# Synopsis: Test for $Assert.Match
Rule 'Assert.Match' {
    $Assert.Match($TargetObject, 'Name', '^Test\w*2$')
    $Assert.Match($TargetObject, 'CompareString', '^(|ABC)$', $True)
}

# Synopsis: Test for $Assert.NotIn
Rule 'Assert.NotIn' {
    $Assert.NotIn($TargetObject, 'Name', @('TestObject1', 'TestObject3'))
    $Assert.NotIn($TargetObject, 'Int', @(1, 3))
    $Assert.NotIn($TargetObject, 'Type', @('TestType'))
}

# Synopsis: Test for $Assert.NotMatch
Rule 'Assert.NotMatch' {
    $Assert.NotMatch($TargetObject, 'Name', '^Test\w*1$')
    $Assert.NotMatch($TargetObject, 'Type', '.*')
}

# Synopsis: Test for $Assert.HasEmptyField
Rule 'Assert.NullOrEmpty' {
    $Assert.NullOrEmpty($TargetObject, 'Type')
    $Assert.NullOrEmpty($TargetObject, 'Value')
    $Assert.NullOrEmpty($TargetObject, 'String')
    $Assert.NullOrEmpty($TargetObject, 'Array')
}

# Synopsis: Test for $Assert.StartsWith
Rule 'Assert.StartsWith' {
    $Assert.StartsWith($TargetObject, 'Version', '2.0')
}

# Synopsis: Test for $Assert.Version
Rule 'Assert.Version' {
    $Assert.Version($TargetObject, 'Version', '>1.0.0')
}
