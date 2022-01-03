---
author: BernieWhite
---

# Storing and naming rules

Rules are stored in one or more files and each file can contain one or many rules.
Additionally, rules can be grouped into a module and distributed.

!!! Abstract
    This topic covers recommendations for naming and storing rules.

## Using a standard file path

Rules can be standalone or packaged within a module.
Standalone rules are ideal for a single project such as an Infrastructure as Code (IaC) repository.
To reuse rules across multiple projects consider packaging these as a module.

The instructions for packaging rules in a module can be found here:

- [Packaging rules in a module](packaging-rules.md)

To store standalone rules we recommend that you:

- **Use .ps-rule/** &mdash; Create a sub-directory called `.ps-rule` in the root of your repository.
  Use all lower-case in the sub-directory name.
  Put any custom rules within this sub-directory.
- **Use files ending with .Rule.\*** &mdash; PSRule uses a file naming convention to discover rules.
  Use one of the following depending on the file format you are using:
  - YAML - `.Rule.yaml`.
  - JSON - `.Rule.jsonc` or `.Rule.json`.
  - PowerShell - `.Rule.ps1`.

!!! Note
    Build pipelines are often case-sensitive or run on Linux-based systems.
    Using the casing rule above reduces confusion latter when you configure continuous integration (CI).

## Naming rules

When running PSRule, rule names must be unique.
For example, [PSRule for Azure][1] uses the name prefix of `Azure.` for rules included in the module.

!!! Example
    The following names are examples of rules included within PSRule for Azure:

    - `Azure.AKS.Version`
    - `Azure.AKS.AuthorizedIPs`
    - `Azure.SQL.MinTLS`

When naming custom rules we recommend that you:

- **Use a standard prefix** &mdash; You can use the `Local.` or `Org.` prefix for standalone rules.
  - Alternatively choose a short prefix that identifies your organization.
- **Use dotted notation** &mdash; Use dots to separate rule name.
- **Use a maximum length of 35 characters** &mdash; The default view of `Invoke-PSRule` truncates longer names.
  PSRule supports longer rule names however if `Invoke-PSRule` is called directly consider using `Format-List`.

  [1]: https://aka.ms/ps-rule-azure
