# PSRule_Badges

## about_PSRule_Badges

## SHORT DESCRIPTION

Describes using the badge API with PSRule.

## LONG DESCRIPTION

PSRule executes rules to validate an object from input.
When processing input it may be necessary to perform custom actions before or after rules execute.
Conventions provide an extensibility point that can be shipped with or external to standard rules.
The badge API can be used to create badges within a convention.

### Using the API

PSRule provides the `$PSRule` built-in variable that exposes the badge API.
By using the `$PSRule.Badges.Create` method you can create a standard or custom badge.

The create method provides the following overloads:

```csharp
// Create a badge for the worst case of an analyzed object.
IBadge Create(InvokeResult result);

// Create a badge for the worst case of all analyzed objects.
IBadge Create(IEnumerable<InvokeResult> result);

// Create a custom badge.
IBadge Create(string title, BadgeType type, string label);
```

A badge once created can be read as a string or written to disk with the following methods:

```csharp
// Get the badge as SVG text content.
string ToSvg();

// Write the SVG badge content directly to disk.
void ToFile(string path);
```

### Defining conventions

To define a convention, add a `Export-PSRuleConvention` block within a `.Rule.ps1` file.
The `.Rule.ps1` must be in an included path or module with `-Path` or `-Module`.

The `Export-PSRuleConvention` block works similar to the `Rule` block.
Each convention must have a unique name.
Currently the badge API support creating badges in the `-End` block.

For example:

```powershell
# Synopsis: A convention that generates a badge for an aggregate result.
Export-PSRuleConvention 'Local.Aggregate' -End {
    $PSRule.Badges.Create($PSRule.Output).ToFile('out/badges/aggregate.svg');
}
```

```powershell
# Synopsis: A convention that generates a custom badge.
Export-PSRuleConvention 'Local.CustomBadge' -End {
    $PSRule.Badges.Create('PSRule', [PSRule.Badges.BadgeType]::Success, 'OK').ToFile('out/badges/custom.svg');
}
```

### Using conventions

A convention can be included by using the `-Convention` parameter when executing a PSRule cmdlet.
Alternatively, conventions can be included with options.
To use a convention specify the name of the convention by name.
For example:

```powershell
Invoke-PSRule -Convention 'Local.Aggregate';
```

## NOTE

An online version of this document is available at https://microsoft.github.io/PSRule/v1/concepts/PSRule/en-US/about_PSRule_Badges/.

## SEE ALSO

- [Invoke-PSRule](https://microsoft.github.io/PSRule/v1/commands/PSRule/en-US/Invoke-PSRule/)

## KEYWORDS

- Badges
- Conventions
- PSRule
