# Writing rule help

PSRule has built-in support for help.
Documentation can optionally be added for each rule to provide detailed information or remediation steps.

This scenario covers the following:

- Using inline help
- Writing markdown documentation
- Localizing documentation files

## Inline help with YAML and JSON

With authoring rules in YAML and JSON, PSRule provides the following syntax features:

- Synopsis resource comment.
- `metadata.displayName` property.
- `metadata.description` property.
- `metadata.link` property.
- `spec.recommend` property.

### Synopsis resource comment

Specify the synopsis of the rule with the `Synopsis` comment above the rule properties.

=== "YAML"

    ```yaml hl_lines="2"
    ---
    # Synopsis: An example rule to require TLS.
    apiVersion: github.com/microsoft/PSRule/2025-01-01
    kind: Rule
    metadata:
      name: 'Local.YAML.RequireTLS'
    spec:
      condition:
        field: 'configure.supportsHttpsTrafficOnly'
        equals: true
    ```

=== "JSON"

    ```json hl_lines="3"
    [
      {
        // Synopsis: An example rule to require TLS.
        "apiVersion": "github.com/microsoft/PSRule/2025-01-01",
        "kind": "Rule",
        "metadata": {
          "name": "Local.JSON.RequireTLS"
        },
        "spec": {
          "condition": {
            "field": "configure.supportsHttpsTrafficOnly",
            "equals": true
          }
        }
      }
    ]
    ```

!!! Note
    The resource comment is not localized.
    Use [markdown documentation](#writing-markdown-documentation) for a localized synopsis.

### Display name property

Specify the display name of the rule with the `metadata.displayName` property.

=== "YAML"

    ```yaml hl_lines="7"
    ---
    # Synopsis: An example rule to require TLS.
    apiVersion: github.com/microsoft/PSRule/2025-01-01
    kind: Rule
    metadata:
      name: 'Local.YAML.RequireTLS'
      displayName: Require TLS
    spec:
      condition:
        field: 'configure.supportsHttpsTrafficOnly'
        equals: true
    ```

=== "JSON"

    ```json hl_lines="8"
    [
      {
        // Synopsis: An example rule to require TLS.
        "apiVersion": "github.com/microsoft/PSRule/2025-01-01",
        "kind": "Rule",
        "metadata": {
          "name": "Local.JSON.RequireTLS",
          "displayName": "Require TLS"
        },
        "spec": {
          "condition": {
            "field": "configure.supportsHttpsTrafficOnly",
            "equals": true
          }
        }
      }
    ]
    ```

!!! Note
    This property is not localized.
    Use [markdown documentation](#writing-markdown-documentation) for a localized display name.

### Description property

Specify the description of the rule with the `metadata.description` property.

=== "YAML"

    ```yaml hl_lines="7"
    ---
    # Synopsis: An example rule to require TLS.
    apiVersion: github.com/microsoft/PSRule/2025-01-01
    kind: Rule
    metadata:
      name: 'Local.YAML.RequireTLS'
      description: The resource should only use TLS.
    spec:
      condition:
        field: 'configure.supportsHttpsTrafficOnly'
        equals: true
    ```

=== "JSON"

    ```json hl_lines="8"
    [
      {
        // Synopsis: An example rule to require TLS.
        "apiVersion": "github.com/microsoft/PSRule/2025-01-01",
        "kind": "Rule",
        "metadata": {
          "name": "Local.JSON.RequireTLS",
          "description": "The resource should only use TLS."
        },
        "spec": {
          "condition": {
            "field": "configure.supportsHttpsTrafficOnly",
            "equals": true
          }
        }
      }
    ]
    ```

!!! Note
    This property is not localized.
    Use [markdown documentation](#writing-markdown-documentation) for a localized description.

### Link property

Specify the online help URL of the rule with the `metadata.link` property.

=== "YAML"

    ```yaml hl_lines="7"
    ---
    # Synopsis: An example rule to require TLS.
    apiVersion: github.com/microsoft/PSRule/2025-01-01
    kind: Rule
    metadata:
      name: 'Local.YAML.RequireTLS'
      link: https://aka.ms/ps-rule
    spec:
      condition:
        field: 'configure.supportsHttpsTrafficOnly'
        equals: true
    ```

=== "JSON"

    ```json hl_lines="8"
    [
      {
        // Synopsis: An example rule to require TLS.
        "apiVersion": "github.com/microsoft/PSRule/2025-01-01",
        "kind": "Rule",
        "metadata": {
          "name": "Local.JSON.RequireTLS",
          "link": "https://aka.ms/ps-rule"
        },
        "spec": {
          "condition": {
            "field": "configure.supportsHttpsTrafficOnly",
            "equals": true
          }
        }
      }
    ]
    ```

!!! Note
    This property is not localized.
    Use [markdown documentation](#writing-markdown-documentation) for a localized online help URL.

### Recommend property

Specify the rule recommendation with the `spec.recommend` property.

=== "YAML"

    ```yaml hl_lines="8"
    ---
    # Synopsis: An example rule to require TLS.
    apiVersion: github.com/microsoft/PSRule/2025-01-01
    kind: Rule
    metadata:
      name: 'Local.YAML.RequireTLS'
    spec:
      recommend: The resource should only use TLS.
      condition:
        field: 'configure.supportsHttpsTrafficOnly'
        equals: true
    ```

=== "JSON"

    ```json hl_lines="10"
    [
      {
        // Synopsis: An example rule to require TLS.
        "apiVersion": "github.com/microsoft/PSRule/2025-01-01",
        "kind": "Rule",
        "metadata": {
          "name": "Local.JSON.RequireTLS"
        },
        "spec": {
          "recommend": "",
          "condition": {
            "field": "configure.supportsHttpsTrafficOnly",
            "equals": true
          }
        }
      }
    ]
    ```

!!! Note
    This property is not localized.
    Use [markdown documentation](#writing-markdown-documentation) for a localized recommendation.

## Inline help with PowerShell

When authoring rules in PowerShell, PSRule provides the following syntax features:

- Synopsis script comment.
- `Recommend` keyword.
- `Reason` keyword.

These features are each describe in detail in the following sections.

### Synopsis script comment

Comment metadata can be included directly above a rule block by using the syntax `# Synopsis: <text>`.
This is only supported for populating a rule synopsis.

For example:

```powershell title="PowerShell" hl_lines="1"
# Synopsis: Must have the app.kubernetes.io/name label
Rule 'metadata.Name' -Type 'Deployment', 'Service' {
    Exists "metadata.labels.'app.kubernetes.io/name'"
}
```

This example above would set the synopsis to `Must have the app.kubernetes.io/name label`.

Including comment metadata improves authoring by indicating the rules purpose.
Only a single line is supported.
A rule synopsis is displayed when using `Get-PSRule` and `Get-PSRuleHelp`.
The synopsis can not break over multiple lines.

The key limitation of _only_ using comment metadata is that it can not be localized for multiple languages.
Consider using comment metadata and also using markdown documentation for a multi-language experience.

!!! Note
    The script comment is not localized.
    Use [markdown documentation](#writing-markdown-documentation) for a localized synopsis.

### Recommend keyword

The `Recommend` keyword sets the recommendation for a rule.
Use the keyword with a text recommendation at the top of your rule body.

Using the `Recommend` keyword is recommended for rules that are not packaged in a module.
When packaging rules in a module consider using markdown help instead.

For example:

```powershell title="PowerShell" hl_lines="3"
# Synopsis: Must have the app.kubernetes.io/name label
Rule 'metadata.Name' -Type 'Deployment', 'Service' {
    Recommend 'Consider setting the recommended label ''app.kubernetes.io/name'' on deployment and service resources.'
    Exists "metadata.labels.'app.kubernetes.io/name'"
}
```

A rule recommendation is displayed when using `Invoke-PSRule` or `Get-PSRuleHelp`.

Only use the `Recommend` keyword once to set the recommendation text and avoid formatting with variables.
Recommendations are cached the first time they are used.
Supplying a unique recommendation within a rule based on conditions/ logic is not supported.
To return a custom unique reason for why the rule failed, use the `Reason` keyword.

Localized recommendations can set by using the `$LocalizedData`.

For example:

```powershell title="PowerShell" hl_lines="3"
# Synopsis: Must have the app.kubernetes.io/name label
Rule 'metadata.Name' -Type 'Deployment', 'Service' {
    Recommend $LocalizedData.RecommendNameLabel
    Exists "metadata.labels.'app.kubernetes.io/name'"
}
```

### Reason keyword

The `Reason` keyword sets the reason the rule failed when using `Invoke-PSRule` and `Assert-PSRule`.
The reason is only included in detailed output if the rule did not pass.
If the rule passed, then reason is empty it returned output.

Reasons are not included in the default view when using `Invoke-PSRule`.
Use `-OutputFormat Wide` to display reason messages.

To set a reason use the `Reason` keyword followed by the reason. For example:

```powershell title="PowerShell" hl_lines="6"
# Synopsis: Must have the app.kubernetes.io/name label
Rule 'metadata.Name' -Type 'Deployment', 'Service' {
    Recommend $LocalizedData.RecommendNameLabel
    Exists "metadata.labels.'app.kubernetes.io/name'"

    Reason 'The standard name label is not set.'
}
```

The `Reason` keyword can be used multiple times within conditional logic to return a list of reasons the rule failed.
Additionally the reason messages can be localized by using the `$LocalizedData` variable.

For example:

```powershell title="PowerShell" hl_lines="7"
# Synopsis: Must have the app.kubernetes.io/name label
Rule 'metadata.Name' -Type 'Deployment', 'Service' {
    Recommend $LocalizedData.RecommendNameLabel
    Exists "metadata.labels.'app.kubernetes.io/name'"

    # $LocalizedData.ReasonLabelMissing is set to 'The standard {0} label is not set.'.
    Reason ($LocalizedData.ReasonLabelMissing -f 'name')
}
```

## Writing markdown documentation

In addition to inline help, documentation can be written in markdown to provide online and offline help.
Extended documentation is generally easier to author using markdown.
Additionally markdown documentation is easily localized.

Markdown documentation is authored by creating one or more `.md` files, one for each rule.
PSRule uses a naming convention with a file name the same as the rule to match rule to markdown.

For example, `metadata.Name.md` would be used for a rule named `metadata.Name`.

We recommend matching the rule name case exactly when naming markdown files.
This is because some file systems are case-sensitive.
For example on Linux `Metadata.Name.md` would not match.

Within each markdown file a number of predefined sections are automatically interpreted by PSRule.
While it is possible to have additional sections, they will be ignored by the help system.

The basic structure of markdown help is as follows:

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

The PSRule [Visual Studio Code extension][extension] includes snippets for writing markdown documentation.

  [extension]: https://marketplace.visualstudio.com/items?itemName=bewhite.psrule-vscode-preview

### Annotations

The annotation front matter at the top of the markdown document, is a set of key value pairs.
Front matter follows YAML conventions and must start on the first line of the markdown document.

A `---` on a separate line indicates the start and end of the front matter block.
Within the front matter block, all key value pairs are treated as annotations by PSRule.

Annotations are optional metadata that are associated with the rule.
Any annotations associated with a rule are included in output.
Some examples of annotations include; `severity`, `category`, `author`.

Annotations differ from tags in two key ways:

- Annotations are localized, and can have a different value for different languages; tags are not.
- Tags are indexed and can be used to filter rules; annotations have no affect on rule filtering.

The following reserved annotation exists:

- `online version` - A URL to the online version of the document, used by `Get-PSRuleHelp -Online`.

```text
---
online version: https://github.com/microsoft/PSRule/blob/main/docs/scenarios/rule-docs/rule-docs.md
---
```

The front matter start and end `---` are not required and can be removed if no annotations are defined.

### Display name

The document title, indicated by a level one heading `#` is the display name of the rule.
The rule display name is shown when using `Get-PSRuleHelp` and is included in output.

Specify the display name on a single line.
Wrapping the display name across multiple lines is not supported.

For example:

```text
# Use recommended name label
```

### Synopsis section

The synopsis section is indicated by the heading `## SYNOPSIS`.
Any text following the heading is interpreted by PSRule and included in output.
The synopsis is displayed when using `Get-PSRule` and `Get-PSRuleHelp` cmdlets.

The _synopsis_ is intended to be a brief description of the rule, over a single line.
A good synopsis should convey the purpose of the rule.
A more verbose description can be included in the _description_ section.

For example:

```text
## SYNOPSIS

Deployments and services must use the app.kubernetes.io/name label.
```

### Description section

The description section is indicated by the heading `## DESCRIPTION`.
Any text following the heading is interpreted by PSRule and included in output.
The description is displayed when using the `Get-PSRuleHelp` cmdlet.

The _description_ is intended to be a verbose description of the rule.
If your rule documentation needs to include background information include it here.

PSRule supports semantic line breaks, and will automatically run together lines into a single paragraph.
Use a blank line to separate paragraphs.

For example:

```text
## DESCRIPTION

Kubernetes defines a common set of labels that are recommended for tool interoperability.
These labels should be used to consistently apply standard metadata.

The `app.kubernetes.io/name` label should be used to specify the name of the application.
```

### Recommendation section

The recommendation section is indicated by the heading `## RECOMMENDATION`.
Any text following the heading is interpreted by PSRule and included in output.
The recommendation is displayed when using the `Invoke-PSRule` and `Get-PSRuleHelp` cmdlets.

The _recommendation_ is intended to identify corrective actions that can be taken to address any failures.
Avoid using URLs within the recommendation.
Use the _links_ section to include references to external sources.

PSRule supports semantic line breaks, and will automatically run together lines into a single paragraph.
Use a blank line to separate paragraphs.

For example:

```text
## RECOMMENDATION

Consider setting the recommended label `app.kubernetes.io/name` on deployment and service resources.
```

### Notes section

The notes section is indicated by the heading `## NOTES`.
Any text following the heading is interpreted by PSRule and included in pipeline output.
Notes are excluded when formatting output as YAML and JSON.

To view any included notes use the `Get-PSRuleHelp` cmdlet with the `-Full` switch.

Use notes to include additional information such configuration options.

PSRule supports semantic line breaks, and will automatically run together lines into a single paragraph.
Use a blank line to separate paragraphs.

For example:

```text
## NOTES

The Kubernetes recommended labels include:

- `app.kubernetes.io/name`
- `app.kubernetes.io/instance`
- `app.kubernetes.io/version`
- `app.kubernetes.io/component`
- `app.kubernetes.io/part-of`
- `app.kubernetes.io/managed-by`
```

### Links section

The links section is indicated by the heading `## LINKS`.
Any markdown links following the heading are interpreted by PSRule and included in pipeline output.
Links are excluded when formatting output as YAML and JSON.

To view any included links use the `Get-PSRuleHelp` cmdlet with the `-Full` switch.

Use links to reference external sources with a URL.

To specify links, use the markdown syntax `[display name](url)`.
Include each link on a separate line.
To improve display in web rendered markdown, use a list of links by prefixing the line with `-`.

Additional text such as `See additional information:` is useful for web rendered views, but ignored by PSRule.

For example:

```text
## LINKS

- [Recommended Labels](https://kubernetes.io/docs/concepts/overview/working-with-objects/common-labels/)
```

## Localizing documentation files

When distributing rules, you may need to provide rule help in different languages.
PSRule builds on the culture system in PowerShell.

### Using cultures

A directory structure is used to identify the markdown documentation that should be used for each culture.

To get a list of cultures in PowerShell the use cmdlet `Get-Culture -ListAvailable`.

For example, store documentation targeted to the culture `en-US` in a directory named `en-US`.
Similarly, documentation for cultures such as `en-AU`, `en-GB` and `fr-FR` would be in separate directories.

If a directory for the exact culture `en-US` doesn't exist, PSRule will attempt to find the parent culture.
For example, documentation would be read from a directory named `en`.

When naming directories for their culture, use exact case.
This is because some file systems are case-sensitive.
For example on Linux `en-us` would not match.

### Culture directory search path

The path that PSRule looks for a culture directory in varies depending on how the rule is redistributed.
Rules can be redistributed individually (loose) or included in a module.

The following logic is used to locate the culture directory.

- If the rules are loose, PSRule will search for the culture directory in the same subdirectory as the `.Rule.ps1` file.
- When rules are included in a module, PSRule will search for the culture directory in the same subdirectory as the module manifest _.psd1_ file.

For example, loose file structure:

- .ps-rule/
  - en/
    - metadata.Name.md
  - en-US/
    - metadata.Name.md
  - fr-FR/
    - metadata.Name.md
  - kubernetes.Rule.ps1

Module file structure:

- Kubernetes.Rules/
  - en/
    - metadata.Name.md
  - en-US/
    - metadata.Name.md
  - fr-FR/
    - metadata.Name.md
  - rules/
    - kubernetes.Rule.ps1
  - Kubernetes.Rules.psd1

## More information

- [kubernetes.Rule.ps1](writing-rule-help/kubernetes.Rule.ps1) - An example rule for validating name label.
- [metadata.Name](writing-rule-help/en-US/metadata.Name.md) - An example markdown documentation file.
