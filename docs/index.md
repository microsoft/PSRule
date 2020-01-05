# PSRule

PSRule is an open-source, general-purpose rules engine built on top of PowerShell and maintained on GitHub.

PSRule provides an easy way to:

- Define reusable business rules like scripts.
- Validate PowerShell objects with rules by piping them to PSRule.

Because PSRule is based on PowerShell:

- It builds on existing PowerShell skills.
- Works with other PowerShell modules and .NET classes.
- Works great with source control and DevOps pipelines.

## Installing the module

You can download and install the PSRule module from the PowerShell Gallery.

Module | Description | Downloads / instructions
------ | ----------- | ------------------------
PSRule | Validate objects using PowerShell rules. | [latest][module] / [instructions][install]

![module-ci-badge] ![module-version-badge] ![module-downloads-badge]

## Installing the extension

You can download and install the companion extension for Visual Studio Code from the Visual Studio Marketplace.

Extension | Description | Downloads / instructions
--------- | ----------- | ------------------------
PSRule    | An extension for IT Pros using the PSRule PowerShell module. | [latest][extension] / [instructions][install]

![extension-ci-badge] ![extension-version-badge] ![extension-installs-badge]

## Additional modules

You can optionally download and install the following modules from the PowerShell Gallery.

Module                  | Description | Version / downloads
------                  | ----------- | -------------------
PSRule.Rules.Azure      | A suite of rules to validate Azure resources using PSRule. | [![rules-azure-version-badge]][rules-azure-version-module] [![rules-azure-downloads-badge]][rules-azure-version-module]
PSRule.Rules.Kubernetes | A suite of rules to validate Kubernetes resources using PSRule. | [![rules-kubernetes-version-badge]][rules-kubernetes-version-module] [![rules-kubernetes-downloads-badge]][rules-kubernetes-version-module]

## Support

This project is to be considered a **proof-of-concept** and **not a supported product**.

If you have any problems please check our GitHub [issues](https://github.com/Microsoft/PSRule/issues) page.
If you do not see your problem captured, please file a new issue and follow the provided template.

[install]: scenarios/install-instructions.md
[module]: https://www.powershellgallery.com/packages/PSRule
[extension]: https://marketplace.visualstudio.com/items?itemName=bewhite.psrule-vscode-preview
[module-ci-badge]: https://dev.azure.com/bewhite/PSRule/_apis/build/status/PSRule-CI?branchName=master
[module-version-badge]: https://img.shields.io/powershellgallery/v/PSRule.svg?label=PowerShell%20Gallery&color=brightgreen
[module-downloads-badge]: https://img.shields.io/powershellgallery/dt/PSRule.svg?color=brightgreen
[extension-ci-badge]: https://dev.azure.com/bewhite/PSRule-vscode/_apis/build/status/PSRule-vscode-CI?branchName=master
[extension-version-badge]: https://vsmarketplacebadge.apphb.com/version/bewhite.psrule-vscode-preview.svg
[extension-installs-badge]: https://vsmarketplacebadge.apphb.com/installs-short/bewhite.psrule-vscode-preview.svg
[rules-azure-version-badge]: https://img.shields.io/powershellgallery/v/PSRule.Rules.Azure.svg?label=PowerShell%20Gallery&color=brightgreen
[rules-azure-downloads-badge]: https://img.shields.io/powershellgallery/dt/PSRule.Rules.Azure.svg?color=brightgreen
[rules-azure-version-module]: https://www.powershellgallery.com/packages/PSRule.Rules.Azure
[rules-kubernetes-version-badge]: https://img.shields.io/powershellgallery/v/PSRule.Rules.Kubernetes.svg?label=PowerShell%20Gallery&color=brightgreen
[rules-kubernetes-downloads-badge]: https://img.shields.io/powershellgallery/dt/PSRule.Rules.Kubernetes.svg?color=brightgreen
[rules-kubernetes-version-module]: https://www.powershellgallery.com/packages/PSRule.Rules.Kubernetes
