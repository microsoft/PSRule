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

In addition, names for rules and other resources must meet the following requirements:

- **Use between 3 and 128 characters** &mdash; This is the minimum and maximum length of a resource name.
- **Only use allowed characters** &mdash;
  To preserve consistency between file systems, some characters are not permitted.
  Dots, hyphens, and underscores are not permitted at the start and end of the name.
  Additionally some characters are restricted for future use.
  The following characters are not permitted:
  - `<` (less than)
  - `>` (greater than)
  - `:` (colon)
  - `/` (forward slash)
  - `\` (backslash)
  - `|` (vertical bar or pipe)
  - `?` (question mark)
  - `*` (asterisk)
  - `"` (double quote)
  - `'` (single quote)
  - `` ` `` (backtick)
  - `+` (plus)
  - `@` (at sign)
  - Integer value zero, sometimes referred to as the ASCII NUL character.
  - Characters whose integer representations are in the range from 1 through 31.

```text title="Regular expression for valid resource names"
^[^<>:/\\|?*"'`+@._\-\x00-\x1F][^<>:/\\|?*"'`+@\x00-\x1F]{1,126}[^<>:/\\|?*"'`+@._\-\x00-\x1F]$
```

When naming rules we recommend that you:

- **Use a standard prefix** &mdash; You can use the `Local.` or `Org.` prefix for standalone rules.
  - Alternatively choose a short prefix that identifies your organization.
- **Use dotted notation** &mdash; Use dots to separate rule name.
- **Use a maximum length of 35 characters** &mdash;
  The default view of `Invoke-PSRule` truncates longer names.
  PSRule supports longer rule names however if `Invoke-PSRule` is called directly consider using `Format-List`.
- **Avoid using special characters and punctuation** &mdash;
  Although these characters can be used in many cases, they may not be easy to use with all PSRule features.

  [1]: https://aka.ms/ps-rule-azure
