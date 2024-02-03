# Lock file

!!! Abstract
    PSRule v3 and later uses a lock file to define the modules and versions used to run analysis.
    This article describes the lock file and how to manage it.

An optional lock file can be used to define the modules and versions used to run analysis.
Using the lock file ensures that the same modules and versions are used across multiple machines, improving consistency.

- **Lock file is present** - PSRule will use the module versions defined in the lock file.
- **Lock file is not present** - PSRule will use the latest version of each module installed locally.

Name               | Supports lock file
----               | ------------------
PowerShell         | No
CLI                | Yes, v3 and later
GitHub Actions     | Yes, v3 and later
Azure Pipelines    | Yes, v3 task and later
Visual Studio Code | Yes, v3 and later

!!! Important
    The lock file only applies to PSRule outside of PowerShell.
    When using PSRule as a PowerShell module, the lock file is ignored.

## Restoring modules

When the lock file is present, PSRule will restore the modules and versions defined in the lock file.

<!-- Modules are automatically restored by PSRule when:

- Running analysis with  -->
