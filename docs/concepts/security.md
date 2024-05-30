# Security guidance

!!! Abstract
    The following is information provides consolidated guidance for customers on security when using PSRule.

## PowerShell usage guidance

PSRule supports and recommends using PowerShell security features to secure your environment.
Additionally from PSRule v3.0.0, supports:

- **Disabling PowerShell rules** &mdash; However this will impact rules modules that implement PowerShell rules.
  For details on disabling PowerShell rules see [Execution.RestrictScriptSource][2].

Continue reading [PowerShell security features][1] to learn more about how to secure your PowerShell environment.

  [1]: https://learn.microsoft.com/powershell/scripting/security/security-features?view=powershell-7.4
  [2]: PSRule/en-US/about_PSRule_Options.md#executionrestrictscriptsource

## Software Bill of Materials (SBOM)

Beginning with v2.1.0, PSRule contains a Software Bill of Materials (SBOM).
The SBOM can be found at `_manifest/spdx_2.2/manifest.spdx.json` within the module root.

Things to note:

- When installing the module using `Install-Module` or `Update-Module`,
  PowerShell creates a metadata file `PSGetModuleInfo.xml` in the module root.
  This file is used to keep track of when and where the module was installed from.
  As a result, this file is not included in the SBOM.
  The `PSGetModuleInfo.xml` file is not required for the module to function.

For more information about this initiative,
see the blog post [Generating Software Bills of Materials (SBOMs) with SPDX at Microsoft][3].

  [3]: https://devblogs.microsoft.com/engineering-at-microsoft/generating-software-bills-of-materials-sboms-with-spdx-at-microsoft/

## Reporting security issues

If you have a security issue to report please see our [security policy][4].

  [4]: https://github.com/microsoft/PSRule/security/policy
