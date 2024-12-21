---
title: About emitters
module: 3.0.0
---

# Emitters

Emitters allows complex structures and files types (formats) to be pre-processed and resulting objects extracted.
Once processed, the resulting objects can be evaluated by rules.

## Built-in emitters

PSRule ships with several built-in emitters for common formats, including:

- YAML (`yaml`)
- JSON (`json`)
- Markdown (`markdown`)
- PowerShell Data (`powershell_data`)

The following file extensions are configured by default for each format.

Name              | Default file extensions      | Configurable
----              | -----------------------      | :----------:
`yaml`            | `.yaml`, `.yml`              | Yes
`json`            | `.json`, `.jsonc`, `.sarif`  | Yes
`markdown`        | `.md`, `.markdown`           | Yes
`powershell_data` | `.psd1`                      | Yes

## Custom emitters

Custom emitters can be created by implementing the `PSRule.Emitters.IEmitter` interface available in `Microsoft.PSRule.Types`.
This custom type implementation will be loaded by PSRule and used to process the input object.

To use a custom emitter, it must be registered with PSRule as a service.
This can be done by a convention within the `-Initialize` script block.

## Configuring formats

The file or object types that each emitter processes is configurable by setting the [Format option](PSRule/en-US/about_PSRule_Options.md#format).
This allows custom types and file extensions to be easily added or removed to a compatible emitter.

For example, many configuration files use JSON but may end with a different file extension.
The extensions that will be processed can be overridden by setting the `format.json.types` key in `ps-rule.yaml`.
To change the file extension to be processed as JSON the following option can be set:

```yaml
format:
  json:
    types:
      - .json
      - .jsonc
      - .jsn
```

### Advanced configuration

Emitters may support additional options or feature flags for configuration.
Set these, by using the [Configuration](PSRule/en-US/about_PSRule_Options.md#configuration) option.

Currently there is no advanced configuration options for built-in emitters.
