# Configure an options file

- First, type `requires` and press Tab or Enter to complete the suggestion.
- Continue typing to replace `Module` with `PSRule` and press Tab or Enter to move to the version constraint.
- Replace `'>=1.0.0'` with the minimum version of PSRule you want to use, such as `'>=2.8.1'`.

## Configure additional modules

Additional modules can be added to the options file.
For example:

[Copy snippet](command:PSRule.walkthroughCopySnippet?%7B%22snippet%22%3A%2201_requiresModules%22%7D)

```yaml
requires:
  PSRule: '>=2.8.1'
  PSRule.Rules.Azure: '>=1.26.1'
```

### Configure other options

Rules often have additional configuration options that can be set.
For example, the `PSRule.Rules.Azure` module has the following options:

[Copy snippet](command:PSRule.walkthroughCopySnippet?%7B%22snippet%22%3A%2201_configureAzure%22%7D)

```yaml
configuration:
  # Enable expansion of Azure Template parameter files.
  AZURE_PARAMETER_FILE_EXPANSION: true

  # Enable expansion of Azure Bicep files.
  AZURE_BICEP_FILE_EXPANSION: true
```
