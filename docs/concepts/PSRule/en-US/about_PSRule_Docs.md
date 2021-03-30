# PSRule_Docs

## about_PSRule_Docs

## SHORT DESCRIPTION

Describes usage of documentation within PSRule.

## LONG DESCRIPTION

PSRule includes a built-in documentation system that provide culture specific help and metadata for rules.

Rule documentation is composed of markdown files that can be optionally shipped with a module.

### Getting documentation

To get documentation for a rule use the `Get-PSRuleHelp` cmdlet.

For example:

```powershell
Get-PSRuleHelp <rule-name>
```

Each rule can include the following documentation:

- Annotations - Additional metadata included in results.
- Synopsis - A brief description on the intended purpose of the rule.
- Description - A detailed description on the intended purpose of the rule.
- Recommendation - A detailed explanation of the requirements to pass the rule.
- Notes - Any additional information or configuration options.
- Links - Any links to external references.

See cmdlet help for detailed information on the `Get-PSRuleHelp` cmdlet.

### Online help

Rule documentation may optionally include a link to an online version.
When included, the `-Online` parameter can be used to open the online version in the default web browser.

For example:

```powershell
Get-PSRuleHelp <rule-name> -Online
```

### Creating documentation

Rule documentation is composed of markdown files, one per rule.
When creating rules for more then one culture, a separate markdown file is created per rule per culture.

The markdown files for each rule is automatically discovered based on naming convention.

Markdown is saved in a file with the same filename as the rule name with the `.md` extension.
The file name should match the same case exactly, with a lower case extension.

As an example, the `storageAccounts.UseHttps.md` markdown file would be created.

```powershell
# Synopsis: Configure storage accounts to only accept encrypted traffic i.e. HTTPS/SMB
Rule 'storageAccounts.UseHttps' -If { ResourceType 'Microsoft.Storage/storageAccounts' } {
    Recommend 'Storage accounts should only allow secure traffic'

    $TargetObject.Properties.supportsHttpsTrafficOnly
}
```

This directory PSRule will look for these markdown files depends on how the rules are packaged:

- If the rules are loose (not part of a module), PSRule will search for documentation in the `.\<culture>\` subdirectory relative to where the rule script _.ps1_ file is located.
- When the rules are shipped as part of a module, PSRule will search for documentation in the `.\<culture>\` subdirectory relative to where the module manifest _.psd1_ file is located.

The `<culture>` subdirectory will be the current culture that PowerShell is executed under (the same as `(Get-Culture).Name`).
Alternatively, the culture can set by using the `-Culture` parameter of PSRule cmdlets.

The markdown of each file uses following structure.

```text
---
{{ Annotations }}
---

# {{ Name of rule }}

## SYNOPSIS

{{ A brief summary of the rule }}

## DESCRIPTION

{{ A detailed description of the rule }}

## RECOMMENDATION

{{ A detailed explanation of the steps required to pass the rule }}

## NOTES

{{ Additional information or configuration options }}

## LINKS

{{ Links to external references }}

```

Optionally, one or more annotations formatted as YAML key value pairs can be included.
i.e. `severity: Critical`

## NOTE

An online version of this document is available at https://github.com/Microsoft/PSRule/blob/main/docs/concepts/PSRule/en-US/about_PSRule_Docs.md.

## SEE ALSO

- [Get-PSRuleHelp](https://github.com/Microsoft/PSRule/blob/main/docs/commands/PSRule/en-US/Get-PSRuleHelp.md)

## KEYWORDS

- Help
- Rule
