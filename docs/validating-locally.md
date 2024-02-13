---
author: BernieWhite
---

# Validating locally

PSRule can be installed locally on MacOS, Linux, and Windows for local validation.
This allows you to test Infrastructure as Code (IaC) artifacts before pushing changes to a repository.

!!! Tip
    If you haven't already, follow the instructions on [installing locally][1] before continuing.

  [1]: install.md#with-powershell

## With Visual Studio Code

[:octicons-download-24: Extension][2]

An extension for Visual Studio Code is available for an integrated experience using PSRule.
The Visual Studio Code extension includes a built-in task _PSRule: Run analysis_ task.

<p align="center">
  <img src="https://raw.githubusercontent.com/microsoft/PSRule-vscode/main/docs/images/tasks-provider.png" alt="Built-in tasks shown in task list" />
</p>

!!! Info
    To learn about tasks in Visual Studio Code see [Integrate with External Tools via Tasks][3].

### Customizing the task

The _PSRule: Run analysis_ task will be available automatically after you install the PSRule extension.
You can customize the defaults of the task by editing or inserting the task into `.vscode/tasks.json` within your workspace.

```json title="JSON"
{
    "type": "PSRule",
    "problemMatcher": [
        "$PSRule"
    ],
    "label": "PSRule: Run analysis",
    "modules": [
        "PSRule.Rules.Azure"
    ],
    "presentation": {
        "clear": true,
        "panel": "dedicated"
    }
}
```

!!! Example
    A complete `.vscode/tasks.json` might look like the following:

    ```json title=".vscode/tasks.json"
    {
        "version": "2.0.0",
        "tasks": [
            {
                "type": "PSRule",
                "problemMatcher": [
                    "$PSRule"
                ],
                "label": "PSRule: Run analysis",
                "modules": [
                    "PSRule.Rules.Azure"
                ],
                "presentation": {
                    "clear": true,
                    "panel": "dedicated"
                }
            }
        ]
    }
    ```

  [2]: https://marketplace.visualstudio.com/items?itemName=bewhite.psrule-vscode
  [3]: https://code.visualstudio.com/docs/editor/tasks
