---
external help file: PSRule-help.xml
Module Name: PSRule
online version: https://microsoft.github.io/PSRule/commands/PSRule/en-US/Get-PSRule.html
schema: 2.0.0
---

# Get-PSRule

## SYNOPSIS

Get a list of rule definitions.

## SYNTAX

```text
Get-PSRule [-Module <String[]>] [-ListAvailable] [-OutputFormat <OutputFormat>] [-Baseline <BaselineOption>]
 [[-Path] <String[]>] [-Name <String[]>] [-Tag <Hashtable>] [-Option <PSRuleOption>] [-Culture <String>]
 [-IncludeDependencies] [<CommonParameters>]
```

## DESCRIPTION

Get a list of matching rule definitions within the search path.

## EXAMPLES

### Example 1

```powershell
Get-PSRule;
```

```text
RuleName                            ModuleName                 Synopsis
--------                            ----------                 --------
isFruit                                                        An example rule
```

Get a list of rule definitions from the current working path.

### Example 2

```powershell
Get-PSRule -Module PSRule.Rules.Azure;
```

```text
RuleName                            ModuleName                 Synopsis
--------                            ----------                 --------
Azure.ACR.AdminUser                 PSRule.Rules.Azure         Use Azure AD accounts instead of using the registry adm…
Azure.ACR.MinSku                    PSRule.Rules.Azure         ACR should use the Premium or Standard SKU for producti…
Azure.AKS.MinNodeCount              PSRule.Rules.Azure         AKS clusters should have minimum number of nodes for fa…
Azure.AKS.Version                   PSRule.Rules.Azure         AKS clusters should meet the minimum version.
Azure.AKS.UseRBAC                   PSRule.Rules.Azure         AKS cluster should use role-based access control (RBAC).
```

Get a list of rule definitions included in the module `PSRule.Rules.Azure`.

### Example 3

```powershell
Get-PSRule -Module PSRule.Rules.Azure -OutputFormat Wide;
```

```text
RuleName                            ModuleName                 Synopsis                     Tag
--------                            ----------                 --------                     ---
Azure.ACR.AdminUser                 PSRule.Rules.Azure         Use Azure AD accounts        severity='Critical'
                                                               instead of using the         category='Security
                                                               registry admin user.         configuration'
Azure.ACR.MinSku                    PSRule.Rules.Azure         ACR should use the Premium   severity='Important'
                                                               or Standard SKU for          category='Performance'
                                                               production deployments.
Azure.AKS.MinNodeCount              PSRule.Rules.Azure         AKS clusters should have     severity='Important'
                                                               minimum number of nodes for  category='Reliability'
                                                               failover and updates.
Azure.AKS.Version                   PSRule.Rules.Azure         AKS clusters should meet     severity='Important'
                                                               the minimum version.         category='Operations
                                                                                            management'
Azure.AKS.UseRBAC                   PSRule.Rules.Azure         AKS cluster should use       severity='Important'
                                                               role-based access control    category='Security
                                                               (RBAC).                      configuration'
```

Get a list of rule definitions included in the module `PSRule.Rules.Azure` including tags with line wrapping.

## PARAMETERS

### -Name

The name of a specific rule to list. If this parameter is not specified all rules in search paths will be listed.

```yaml
Type: String[]
Parameter Sets: (All)
Aliases: n

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Path

One or more paths to search for rule definitions within.

If this parameter is not specified the current working path will be used, unless the `-Module` parameter is used.

If the `-Module` parameter is used, rule definitions from the currently working path will not be included by default.

```yaml
Type: String[]
Parameter Sets: (All)
Aliases: p

Required: False
Position: 0
Default value: $PWD
Accept pipeline input: False
Accept wildcard characters: False
```

### -Tag

Only get rules with the specified tags set.
If this parameter is not specified all rules in search paths will be returned.

When more than one tag is used, all tags must match. Tags are not case sensitive.
A tag value of `*` may be used to filter rules to any rule with the tag set, regardless of tag value.

An array of tag values can be used to match a rule with either value.
i.e. `severity = important, critical` matches rules with a category of either `important` or `critical`.

```yaml
Type: Hashtable
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Option

Additional options that configure execution.
A `PSRuleOption` can be created by using the `New-PSRuleOption` cmdlet.
Alternatively a hashtable or path to YAML file can be specified with options.

For more information on PSRule options see about_PSRule_Options.

```yaml
Type: PSRuleOption
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ListAvailable

Look for modules containing rule definitions including modules that are currently not imported.

This switch is used with the `-Module` parameter.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Module

Search for rule definitions within a module.
When specified without the `-Path` parameter, only rule definitions in the module will be discovered.

When both `-Path` and `-Module` are specified, rule definitions from both are discovered.

```yaml
Type: String[]
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Culture

Specifies the culture to use for documentation and messages. By default, the culture of PowerShell is used.

This option does not affect the culture used for the PSRule engine, which always uses the culture of PowerShell.

The PowerShell cmdlet `Get-Culture` shows the current culture of PowerShell.

```yaml
Type: String
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -OutputFormat

Configures the format that output is presented in.

The following format options are available:

- None - Output is presented as an object using PowerShell defaults. This is the default.
- Wide - Output is presented using the wide table format, which includes tags and wraps columns.

```yaml
Type: OutputFormat
Parameter Sets: (All)
Aliases:
Accepted values: None, Wide

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -IncludeDependencies

When this switch is specified, dependencies of the rules that meet the `-Name` and `-Tag` filters are included even if they would normally be excluded.

This switch has no affect when getting an unfiltered list of rules.

```yaml
Type: SwitchParameter
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Baseline

When specified, rules are filtered so that only rules that are included in the baselines are returned.

```yaml
Type: BaselineOption
Parameter Sets: (All)
Aliases:

Required: False
Position: Named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters

This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

### None

## OUTPUTS

### PSRule.Rules.Rule

## NOTES

## RELATED LINKS

[Invoke-PSRule](Invoke-PSRule.md)
