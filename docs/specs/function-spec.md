# PSRule function expressions spec (draft)

This is a spec for implementing function expressions in PSRule v2.

## Synopsis

Functions are available to handle complex conditions within YAML and JSON expressions.

## Schema driven

While functions allow handing for complex use cases, they should still remain schema driven.
A schema driven design allows auto-completion and validation during authoring in a broad set of tools.

## Syntax

Functions can be used within YAML and JSON expressions by using the `$` object property.
For example:

```yaml
---
# Synopsis: An expression function example.
apiVersion: github.com/microsoft/PSRule/2025-01-01
kind: Selector
metadata:
  name: Yaml.Fn.Example1
spec:
  if:
    value:
      $:
        substring:
          path: name
        length: 3
    equals: abc
```
