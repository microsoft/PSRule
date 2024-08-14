# Emitter specification

A key component of PSRule design is the ability to read objects from file or PowerShell pipeline.
PSRule provided built-in support for a limited number of file formats and object types.
Conventions could be used to provide support for additional file formats and object types.

Current challenges:

- **Performance**: When processing a high volume of objects, reading objects from disk across multiple times can be slow.
  Many repositories have a large number of files that need to be read and processed.
  CI workflows often executed in container environments where disk access is slow or limited resources are available.
- **Confusing output**: Files processed as objects often end up reporting as unprocessed, generating a default warning.
  This is because they are processed by each available rule, but no rules are defined to process them.
- **Expansion**: Handover of objects to conventions can be complex and does not expose strongly typed objects.
  Expansion is a pattern used by PSRule for Azure to expand a file into multiple objects.
  This is helpful when PSRule does not support a file format or object type natively.

Goals with emitters:

- **Single touch**: If a file needs to be read from disk, it should be read once to reduce IO.
- **Extensible**: A interface should be provided to allow for custom emitters that can be used to read objects from any source.
- **Multi-threaded**: Emitters should be thread safe and support parallel processing.
- **Carried as needed**: Emitter should be only emit objects to be processed by rules.
  File objects are not processed by default.

The goal of emitters is to provide a high performance and extensible way to emit custom objects to the input stream.

Emitters define an `IEmitter` interface for emitting objects to the input stream.
The implementation of an emitter must be thread safe, as emitters can be run in parallel.

## Logging

An emitter may expose diagnostic logs by using the `PSRule.Runtime.ILogger<T>` interface.

## Dependency injection

PSRule uses dependency injection to create each emitter instance.
The following interfaces can optionally be specified in a emitter constructor to have references injected to the instance.

- `PSRule.Runtime.ILogger<T>`
