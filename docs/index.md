# PSRule

PSRule is an open-source, general-purpose rules engine built on top of PowerShell and maintained on GitHub.

PSRule provides an easy way to:

- Define reusable business rules like scripts.
- Validate PowerShell objects and infrastructure code.

Features of PSRule include:

- [Extensible](features.md#extensible) - Use PowerShell, a flexible scripting language.
- [Cross-platform](features.md#cross-platform) - Run on MacOS, Linux, and Windows.
- [Reusable](features.md#reusable) - Share rules across teams or organizations.
- [Recommendations](features.md#recommendations) - Include detailed instructions to remediate issues.

## Installing the module

You can download and install the PSRule module from the PowerShell Gallery.

Module | Description | Downloads / instructions
------ | ----------- | ------------------------
PSRule | Validate objects using PowerShell rules. | [latest][module] / [instructions][install]

![module-ci-badge] ![module-version-badge] ![module-downloads-badge]

## Visual Studio Code extension

You can download and install the companion extension for Visual Studio Code from the Visual Studio Marketplace.

Extension | Description | Downloads / instructions
--------- | ----------- | ------------------------
PSRule    | An extension for IT Pros using the PSRule PowerShell module. | [latest][extension-vscode] / [instructions][install]

![extension-vscode-ci-badge] ![extension-vscode-version-badge] ![extension-vscode-installs-badge]

## Azure DevOps extension

You can download and install the companion extension for Azure Pipelines from the Visual Studio Marketplace.

Extension | Description | Downloads / instructions
--------- | ----------- | ------------------------
PSRule    | An Azure DevOps extension for using PSRule within Azure Pipelines. | [latest][extension-pipelines] / [instructions][install]

![extension-pipelines-ci-badge] ![extension-pipelines-version-badge]

## GitHub action

You can use PSRule with in a workflow with GitHub Actions.

Action | Description | Downloads / instructions
------ | ----------- | ------------------------
PSRule | Validate infrastructure as code (IaC) and DevOps repositories using GitHub Actions. | [latest][extension-github] / [instructions][install]

![extension-github-ci-badge] ![extension-github-version-badge]

## Additional modules

You can optionally download and install the following modules from the PowerShell Gallery.

Module                  | Description | Version / downloads
------                  | ----------- | -------------------
PSRule.Rules.Azure      | A suite of rules to validate Azure resources using PSRule. | [![rules-azure-version-badge]][rules-azure-version-module] [![rules-azure-downloads-badge]][rules-azure-version-module]
PSRule.Rules.Kubernetes | A suite of rules to validate Kubernetes resources using PSRule. | [![rules-kubernetes-version-badge]][rules-kubernetes-version-module] [![rules-kubernetes-downloads-badge]][rules-kubernetes-version-module]

## Support

This project is open source and **not a supported product**.

If you are experiencing problems, have a feature request, or a question, please check for an [issue] on GitHub.
If you do not see your problem captured, please file a new issue, and follow the provided template.

[issue]: https://github.com/Microsoft/PSRule/issues
[install]: install-instructions.md
[module]: https://www.powershellgallery.com/packages/PSRule
[module-ci-badge]: https://dev.azure.com/bewhite/PSRule/_apis/build/status/PSRule-CI?branchName=main
[module-version-badge]: https://img.shields.io/powershellgallery/v/PSRule.svg?label=PowerShell%20Gallery&color=brightgreen
[module-downloads-badge]: https://img.shields.io/powershellgallery/dt/PSRule.svg?color=brightgreen
[extension-vscode]: https://marketplace.visualstudio.com/items?itemName=bewhite.psrule-vscode-preview
[extension-vscode-ci-badge]: https://dev.azure.com/bewhite/PSRule-vscode/_apis/build/status/PSRule-vscode-CI?branchName=main
[extension-vscode-version-badge]: https://vsmarketplacebadge.apphb.com/version/bewhite.psrule-vscode-preview.svg
[extension-vscode-installs-badge]: https://vsmarketplacebadge.apphb.com/installs-short/bewhite.psrule-vscode-preview.svg
[extension-pipelines]: https://marketplace.visualstudio.com/items?itemName=bewhite.ps-rule
[extension-pipelines-ci-badge]: https://dev.azure.com/bewhite/PSRule-pipelines/_apis/build/status/PSRule-pipelines-CI?branchName=main
[extension-pipelines-version-badge]: https://vsmarketplacebadge.apphb.com/version/bewhite.ps-rule.svg
[extension-github]: https://github.com/marketplace/actions/psrule
[extension-github-ci-badge]: https://img.shields.io/github/workflow/status/microsoft/ps-rule/Build?label=GitHub%20Actions&color=brightgreen
[extension-github-version-badge]: https://img.shields.io/github/v/release/microsoft/ps-rule?sort=semver&label=release&color=brightgreen
[rules-azure-version-badge]: https://img.shields.io/powershellgallery/v/PSRule.Rules.Azure.svg?label=PowerShell%20Gallery&color=brightgreen
[rules-azure-downloads-badge]: https://img.shields.io/powershellgallery/dt/PSRule.Rules.Azure.svg?color=brightgreen
[rules-azure-version-module]: https://www.powershellgallery.com/packages/PSRule.Rules.Azure
[rules-kubernetes-version-badge]: https://img.shields.io/powershellgallery/v/PSRule.Rules.Kubernetes.svg?label=PowerShell%20Gallery&color=brightgreen
[rules-kubernetes-downloads-badge]: https://img.shields.io/powershellgallery/dt/PSRule.Rules.Kubernetes.svg?color=brightgreen
[rules-kubernetes-version-module]: https://www.powershellgallery.com/packages/PSRule.Rules.Kubernetes
