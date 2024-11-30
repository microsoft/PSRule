# PSRule

Validate infrastructure as code (IaC) and DevOps repositories using the PSRule.
PSRule is powerful, feature rich, and highly customizable to meet your needs.

## Features

### CodeLens

<p align="center">
  <img src="./docs/images/codelens-doc-link.png" alt="CodeLens showing link to create documentation" width="640px" />
</p>

- CodeLens shows links to create or edit markdown documentation from rules in YAML, JSON, or PowerShell.
  - **Open documentation** &mdash; Opens rule markdown documentation in the editor.
    - The location for storing documentation is configurable in the extension settings.
    - By default, a locale specific folder is created in the same directory as the rule file.
  - **Create documentation** &mdash; Creates a new markdown file for the rule based on a snippet.
    - New markdown documentation is created with the built-in _Rule Doc_ snippet.
    - An alternative snippet can be specified by configuring extension settings.

### IntelliSense

<p align="center">
  <img src="./docs/images/options-schema-flyout.png" alt="Options suggestion context menu" width="640px" />
</p>

- Adds IntelliSense and validation support for configuring options and resources.
  - **Workspace options** &mdash; use IntelliSense to configure options for the workspace.
    - Type or trigger IntelliSense with `Ctrl+Space` from `ps-rule.yaml`.
  - **Create resources** &mdash; define _baselines_ and _selectors_ by using pre-built snippets and IntelliSense.

<p align="center">
  <img src="./docs/images/snippet-rule-type.png" alt="Rule definition snippet" width="520px" />
</p>

- Adds snippets for defining new rules.
  - **Define rules** with snippets and IntelliSense support.
    - Trigger IntelliSense by typing `rule` in a `.Rule.ps1`, `.Rule.yaml`, or `.Rule.jsonc` file.
    IntelliSense can also be triggered by using the shortcut `Ctrl+Space`.

<p align="center">
  <img src="./docs/images/snippet-markdown.png" alt="Rule markdown documentation snippet" width="640px" />
</p>

- Adds snippets for creating markdown documentation.
  - **Quick documentation** &mdash; create rule documentation to provide rule recommendations and examples.
    - Trigger IntelliSense by typing `rule` in a `.md` file.
    IntelliSense can also be triggered by using the shortcut `Ctrl+Space`.

### Quick tasks

<p align="center">
  <img src="./docs/images/tasks-provider.png" alt="Built-in tasks shown in task list" width="640px" />
</p>

- Adds quick tasks for analysis directly from Visual Studio Code.
  - **Run analysis** &mdash; runs rules against files in the current workspace.
    - _Input path_, _Baseline_, _Modules_, and _Outcome_ options can be configured per task.
    - _Output as_, and showing a _Not processed warning_ options can be configured by workspace or user.
    - Rule stored in `.ps-rule/` are automatically used by default.
    - Use the built-in analysis task by running or configuring the task from the _Terminal_ menu.

## Configuration

In addition to configuring the [ps-rule.yaml] options file, the following settings are available.

Name                                            | Description
----                                            | -----------
`PSRule.codeLens.ruleDocumentationLinks`        | Enables Code Lens that displays links to rule documentation. This is an experimental feature that requires experimental features to be enabled.
`PSRule.documentation.path`                     | The path to look for rule documentation. When not set, the path containing rules will be used.
`PSRule.documentation.localePath`               | The locale path to use for locating rule documentation. The VS Code locale will be used by default.
`PSRule.documentation.customSnippetPath`        | The path to a file containing a rule documentation snippet. When not set, built-in PSRule snippets will be used.
`PSRule.documentation.snippet`                  | The name of a snippet to use when creating new rule documentation. By default, the built-in `Rule Doc` snippet will be used.
`PSRule.execution.notProcessedWarning`          | Warn when objects are not processed by any rule. This option is deprecated and replaced by `PSRule.execution.unprocessedObject`.
`PSRule.execution.ruleExcluded`                 | Determines how to handle excluded rules. When set to `None`, PSRule will use the default (`Ignore`), unless set by PSRule options.
`PSRule.execution.ruleSuppressed`               | Determines how to handle suppressed rules. When set to `None`, PSRule will use the default (`Warn`), unless set by PSRule options.
`PSRule.execution.unprocessedObject`            | Determines how to report objects that are not processed by any rule. When set to `None`, PSRule will use the default (`Warn`), unless set by PSRule options.
`PSRule.experimental.enabled`                   | Enables experimental features in the PSRule extension.
`PSRule.notifications.showChannelUpgrade`       | Determines if a notification to switch to the stable channel is shown on start up.
`PSRule.notifications.showPowerShellExtension`  | Determines if a notification to install the PowerShell extension is shown on start up.
`PSRule.options.path`                           | The path specifying a PSRule option file. When not set, the default `ps-rule.yaml` will be used from the current workspace.
`PSRule.output.as`                              | Configures the output of analysis tasks, either summary or detailed.
`PSRule.rule.baseline`                          | The name of the default baseline to use for executing rules. This setting can be overridden on individual PSRule tasks.

[ps-rule.yaml]: https://aka.ms/ps-rule/options
