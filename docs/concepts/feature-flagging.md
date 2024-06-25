# Feature flagging

!!! Abstract
    Feature flags are a way to enable or disable functionality.
    Rule and module authors can use feature flags to toggle functionality on or off.

## Using feature flags in emitters

When an emitter is executed `IEmitterContext` is passed into each call.
This context includes a `Configuration` property that exposes `IConfiguration`.
