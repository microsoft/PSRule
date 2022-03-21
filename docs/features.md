---
author: BernieWhite
---

# Features

## DevOps

PSRule allows you to quickly plug-in Infrastructure as Code (IaC) controls into your DevOps pipeline.

- **Shift-left** &mdash; Identify configuration issues and provide fast feedback in PRs.
- **Quality gates** &mdash; Implement quality gates between environments such as dev, test, and prod.
- **Monitor continuously** &mdash; Perform ongoing checks for configuration optimization opportunities.

Run on MacOS, Linux, and Windows or anywhere PowerShell is supported.
Native support for popular continuous integration (CI) systems includes:

- **GitHub Actions** - Trigger tests for GitHub repositories using workflows.
- **Azure Pipelines** - Use tasks to run tests in Azure DevOps YAML or Classic pipelines and releases.

## Extensible

Import pre-built rules or define your own using YAML, JSON, or PowerShell format.
Regardless of the format you choose, any combination of YAML, JSON, or PowerShell rules can be used together.

- **YAML** &mdash; Use a popular, easy to read, and learn IaC format.
  With YAML, you can quickly build out common rules with minimal effort and no scripting experience.
- **JSON** &mdash; Is ubiquitous used by many tools.
  While this format is typically harder to read then YAML it is easy to automate.
  You may prefer to use this format if you are generating rules with automation.
- **PowerShell** &mdash; Is a flexible scripting language.
  If you or your team already can write a basic PowerShell script, you can already define a rule.
  PowerShell allows you to tap into a large world-wide community of PowerShell users.
  Use existing cmdlets to help you build out rules quickly.

Rules can be authored using any text editor, but we provide a native extension for Visual Studio Code.
Use the extension to quickly author rules or run tests locally before you commit your IaC.

## Reusable

Typically unit tests in traditional testing frameworks are written for a specific case.
This makes it hard invest in tests that are not easily reusable between projects.
Several features of PSRule make it easier to reuse and share rules across teams or organizations.

The following built-in features improve portability:

- [Modular][1] &mdash; Rules can be packages up into a standard PowerShell module then distributed.
  - **Private** &mdash; Modules can be published privately on a network share or NuGet feed.
  - **Public** &mdash; Distribute rules globally using the PowerShell Gallery.
- [Configuration][2] &mdash; PSRule and rules can be configured.
- [Baselines][3] &mdash; An artifact containing rules and configuration for a scenario.
- [Suppression][4] &mdash; Allows you to handle and keep exceptions auditable in git history.
  - **Approval** &mdash; Use [code owners][6] and [branch policy][7] concepts to control changes.
- [Documentation][5] &mdash; Provide guidance on how to resolve detected issues.
  - **Quick** - Use a one liner to quickly add a hint or reference on rules you build.
  - **Detailed** - Support for markdown allows you to provide detailed detailed guidance to resolve issues.

  [1]: authoring/packaging-rules.md
  [2]: concepts/PSRule/en-US/about_PSRule_Options.md
  [3]: concepts/PSRule/en-US/about_PSRule_Baseline.md
  [4]: concepts/PSRule/en-US/about_PSRule_Options.md#suppression
  [5]: authoring/writing-rule-help.md
  [6]: https://docs.github.com/repositories/managing-your-repositorys-settings-and-features/customizing-your-repository/about-code-owners
  [7]: https://docs.microsoft.com/azure/devops/repos/git/branch-policies?view=azure-devops&tabs=browser#automatically-include-code-reviewers

*[IaC]: Infrastructure as Code
*[CI]: Continuous Integration
*[PRs]: Pull Requests
