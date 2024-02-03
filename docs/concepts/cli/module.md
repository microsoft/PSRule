# ps-rule module

!!! Abstract
    Use the `module` command to manage or restore modules tracked by the module lock file and configured options. (`ps-rule.lock.json`).
    The module lock file, provides consistent module versions across multiple machines and environments.
    For more information, see [Lock file](../lockfile.md).

To use the `module` command, choose one of the available subcommands:

- [module init](#module-init)
- [module list](#module-list)
- [module add](#module-add)
- [module remove](#module-remove)
- [module restore](#module-restore)
- [module upgrade](#module-upgrade)

## `module init`

Initialize a new lock file based on existing options.
Using this command, the module lock file is created or updated.

If `ps-rule.yaml` option are configured to included module (`Include.Modules`), these a automatically added to the lock file.
Any required version constraints set by the `Requires` option are taken into consideration.

Optional parameters:

- `--force` - Force the creation of a new lock file, even if one already exists.

For example:

```bash title="PSRule CLI command-line"
ps-rule module init
```

For example, force the creation of a new lock file, even if one already exists:

```bash title="PSRule CLI command-line"
ps-rule module init --force
```

## `module list`

List any module and the installed versions from the lock file.

## `module add`

Add one or more modules to the module lock file.
If the lock file does not exist, it is created.

By default, the latest stable version of the module is added.
Any required version constraints set by the `Requires` option are taken into consideration.

To use a specific module version, use the `--version` argument.

Optional parameters:

- `--version` - Specifies a specific version of the module to add.
  By default, the latest stable version of the module is added.
  Any required version constraints set by the `Requires` option are taken into consideration.

For example:

```bash title="PSRule CLI command-line"
ps-rule module add PSRule.Rules.Azure
```

For example, a specific version of the module is added:

```bash title="PSRule CLI command-line"
ps-rule module add PSRule.Rules.Azure --version 1.32.1
```

## `module remove`

Remove one or more modules from the lock file.

For example:

```bash title="PSRule CLI command-line"
ps-rule module remove PSRule.Rules.Azure
```

## `module restore`

Restore modules from the module lock file (`ps-rule.lock.json`) and configured options.

Optional parameters:

- `--force` - Restore modules even when an existing version that meets constraints is already installed locally.

For example:

```bash title="PSRule CLI command-line"
ps-rule module restore
```

For example, force restore of all modules:

```bash title="PSRule CLI command-line"
ps-rule module restore --force
```

## `module upgrade`

Upgrade to the latest versions any modules within the lock file.

For example:

```bash title="PSRule CLI command-line"
ps-rule module upgrade
```

## Next steps

For more information on the module lock file, see [Lock file](../lockfile.md).

To find out more about the commands available with the PSRule CLI, see [PSRule CLI](./index.md).
