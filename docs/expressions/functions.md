# Expression functions

!!! Abstract
    Functions are an advanced lanaguage feature specific to YAML and JSON resources.
    That extend the language to allow for more complex use cases with expressions.

!!! Experimental
    Functions are a work in progress and subject to change.
    We hope to add more functions, broader support, and more detailed documentation in the future.
    [Join or start a disucssion][1] to let us know how we can improve this feature going forward.

  [1]: https://github.com/microsoft/PSRule/discussions

## Using functions

It may be necessary to perform minor transformation before evaluating a condition.

- `boolean` - Convert a value to a boolean.
- `string` - Convert a value to a string.
- `integer` - Convert a value to an integer.
- `concat` - Concatenate multiple values.
- `substring` - Extract a substring from a string.
- `configuration` - Get a configuration value.
- `path` - Get a value from an object path.

## Supported conditions

Currently functions are only supported on a subset of conditions.
The conditions that are supported are:

- `equals`
- `notEquals`
- `count`
- `less`
- `lessOrEquals`
- `greater`
- `greaterOrEquals`

## Examples

```yaml
---
# Synopsis: An expression function example.
apiVersion: github.com/microsoft/PSRule/v1
kind: Selector
metadata:
  name: Yaml.Fn.Example1
spec:
  if:
    value:
      $:
        substring:
          path: name
        length: 7
    equals: TestObj

---
# Synopsis: An expression function example.
apiVersion: github.com/microsoft/PSRule/v1
kind: Selector
metadata:
  name: Yaml.Fn.Example2
spec:
  if:
    value:
      $:
        configuration: 'ConfigArray'
    count: 5

---
# Synopsis: An expression function example.
apiVersion: github.com/microsoft/PSRule/v1
kind: Selector
metadata:
  name: Yaml.Fn.Example3
spec:
  if:
    value:
      $:
        boolean: true
    equals: true

---
# Synopsis: An expression function example.
apiVersion: github.com/microsoft/PSRule/v1
kind: Selector
metadata:
  name: Yaml.Fn.Example4
spec:
  if:
    value:
      $:
        concat:
        - path: name
        - string: '-'
        - path: name
    equals: TestObject1-TestObject1

---
# Synopsis: An expression function example.
apiVersion: github.com/microsoft/PSRule/v1
kind: Selector
metadata:
  name: Yaml.Fn.Example5
spec:
  if:
    value:
      $:
        integer: 6
    greater: 5

---
# Synopsis: An expression function example.
apiVersion: github.com/microsoft/PSRule/v1
kind: Selector
metadata:
  name: Yaml.Fn.Example6
spec:
  if:
    value: TestObject1-TestObject1
    equals:
      $:
        concat:
        - path: name
        - string: '-'
        - path: name
```
