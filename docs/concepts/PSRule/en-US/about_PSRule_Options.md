# PSRule_Options

## about_PSRule_Options

## SHORT DESCRIPTION

Describes additional options that can be used during rule execution.

## LONG DESCRIPTION

PSRule lets you use options when calling `Invoke-PSRule` to change how rules are executed. This topic describes what options are available, when to and how to use them.

Options can be used by:

- Using the `-Option` parameter of `Invoke-PSRule` with an object created with `New-PSRuleOption`
- Using the `-Option` parameter of `Invoke-PSRule` with a hash table
- Using the `-Option` parameter of `Invoke-PSRule` with a YAML file
- Configuring the default options file `psrule.yml`

As mentioned above, a options object can be created with `New-PSRuleOption` see cmdlet help for syntax and examples.

When using a hash table, `@{}`, one or more options can be specified with the `-Option` parameter using a dotted notation.

For example:

```powershell
$option = @{ 'execution.languageMode' = 'ConstrainedLanguage' };
Invoke-PSRule -Path . -Option $option;
```

`execution.languageMode` is an example of an option that can be used. Please see the following sections for other options can be used.

Another option is to use an external file, formatted as YAML, instead of having to create an options object manually each time. This YAML file can be used with `Invoke-PSRule` to quickly execute rules in a repeatable way.

YAML properties are specified using lower camel case, for example:

```yaml
execution:
  languageMode: ConstrainedLanguage
```

By default PSRule will automatically look for a file named `psrule.yml` in the current working directory. Alternatively, you can specify a YAML file in the `-Option` parameter.

For example:

```powershell
Invoke-PSRule -Path . -Option '.\myconfig.yml'.
```

### Language mode

Unless PowerShell has been constrained, full language features of PowerShell are available to use within rule definitions. In locked down environments, a reduced set of language features may be desired.

When PSRule is executed in an environment configured for Device Guard, only constrained language features are available.

The following language modes are available for use in PSRule:

- FullLanguage
- ConstrainedLanguage

This option can be specified using:

```powershell
# PowerShell: Using the Execution.LanguageMode hash table key
$option = New-PSRuleOption -Option @{ 'Execution.LanguageMode' = 'ConstrainedLanguage' }
```

```yaml
# psrule.yml: Using the execution/languageMode YAML property
execution:
  languageMode: ConstrainedLanguage
```

## EXAMPLES

### Example PSRule.yml

```yaml
# Set execution options
execution:
  languageMode: ConstrainedLanguage
```

### Default PSRule.yml

```yaml
# These are the default options.
# Only properties that differ from the default values need to be specified.
execution:
  languageMode: FullLanguage
```

## NOTE

An online version of this document is available at https://github.com/BernieWhite/PSRule/blob/master/docs/concepts/PSRule/en-US/about_PSRule_Options.md.

## SEE ALSO

- [Invoke-PSRule](https://github.com/BernieWhite/PSRule/blob/master/docs/commands/PSRule/en-US/Invoke-PSRule.md)
- [New-PSRuleOption](https://github.com/BernieWhite/PSRule/blob/master/docs/commands/PSRule/en-US/New-PSRuleOption.md)

## KEYWORDS

- Options
- PSRule
