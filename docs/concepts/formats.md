---
title: About formats and emitters
module: 3.0.0
---

# Formats and emitters

PSRule ships with built-in support for several common file formats.
Each supported format is processed by an _emitter_ that extracts complex objects from the file content.
Once processed, the resulting objects can be evaluated by rules.

Previously in PSRule v2 and earlier, the number of supported input formats and their configuration was not configurable.
In PSRule v3, support has been redesigned to allow extensibility and customization with emitters.

## Built-in formats

PSRule ships with support for the following common formats, each with a corresponding emitter:

- YAML (`yaml`)
- JSON (`json`)
- Markdown (`markdown`)
- PowerShell Data (`powershell_data`)

The following file extensions are configured by default for each format.

Name              | Default file extensions
----              | -----------------------
`yaml`            | `.yaml`, `.yml`
`json`            | `.json`, `.jsonc`, `.sarif`
`markdown`        | `.md`, `.markdown`
`powershell_data` | `.psd1`

## Custom emitters

Custom emitters can be created by implementing the `PSRule.Emitters.IEmitter` interface available in `Microsoft.PSRule.Types`.
This custom type implementation will be loaded by PSRule and used to process the input object.

To use a custom emitter, it must be registered with PSRule as a service.
This can be done by registering a runtime factory.

## Configuring formats

Each format has a set of common properties that can be configured which includes:

- `enabled` &mdash; Enable or disable the format. All formats are disabled by default.
- `type` &mdash; The file extensions that will be processed.
- `replace` &mdash; A set of key-value pairs to replace in the file content.

Although the properties are common, not all properties may be supported by all custom emitters.

### Enabling a format

By default, all built-in formats/ emitters are disabled.
To enable a format, set it's `enabled` property to `true` in the `ps-rule.yaml` options file.
Multiple formats can be enabled at the same time.

For example:

```yaml
format:
  yaml:
    enabled: true
  json:
    enabled: true
```

Alternatively this can be set using an environment variable, which overrides the options file when both are set:

```bash
export PSRULE_FORMAT_YAML_ENABLED=true
export PSRULE_FORMAT_JSON_ENABLED=true
```

Additionally, formats can be enabled on the command-line or CI/CD pipeline by using the _formats_ parameter/ input.

For example, to run the CLI with YAML and JSON formats:

```bash
ps-rule run -f . --formats yaml json
```

Or in PowerShell:

```powershell
Invoke-PSRule -InputPath . -Formats yaml,json
```

Or in GitHub Actions:

```yaml
- name: Analyze with PSRule
  uses: microsoft/ps-rule@v3.0.0
  with:
    formats: yaml,json
```

Or in Azure Pipelines:

```yaml
- task: PSRule@3
  displayName: Analyze with PSRule
  inputs:
    formats: yaml,json
```

### Configuring file extensions

The file or object types that each emitter processes is configurable by setting `type` property of the the [Format option](PSRule/en-US/about_PSRule_Options.md#format).
This allows custom types and file extensions to be easily added or removed to a compatible emitter.

Many configuration files use JSON but may end with a different file extension.
The extensions that will be processed can be overridden by setting the `type` property in `ps-rule.yaml`.
For example, to process `.json`, `.jsonc`, and `.jsn` files:

```yaml
format:
  json:
    type:
      - .json
      - .jsonc
      - .jsn
```

### Configuring replacement

Commonly, Infrastructure as Code files may contain placeholders that need to be replaced before processing.
The `replace` property allows you to specify a set of literal key-value pairs to replace in the file content.
The replacement happens in-memory during processing and does not modify the original file.

For example, to replace `{{environment}}` with `production` and `{{region}}` with `eastus`:

```yaml
format:
  json:
    replace:
      '{{environment}}': production
      '{{region}}': eastus
```

### Advanced configuration

Emitters may support additional options or feature flags for configuration.
Set these, by using the [Configuration](PSRule/en-US/about_PSRule_Options.md#configuration) option.

Currently there is no advanced configuration options for built-in emitters.
