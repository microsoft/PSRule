// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.


namespace PSRule.Pipeline;

#nullable enable

/// <summary>
/// A set of PSRule capabilities that can be queried.
/// </summary>
internal sealed class CapabilitySet
{
    private readonly Dictionary<string, CapabilityState> _Optional = [];

    public CapabilitySet()
    {
        AddIntrinsic(Engine.GetIntrinsicCapability());
    }

    /// <summary>
    /// Add an optional capability.
    /// </summary>
    /// <param name="capability">The name of the capability.</param>
    /// <param name="action">A delegate to query if the capability is currently enabled. The delegate returns <c>true</c> if the capability is enabled.</param>
    public void AddOptional(string capability, Func<bool> action)
    {
        var state = action() ? CapabilityState.Enabled : CapabilityState.Disabled;
        AddInternal(capability, state);
    }

    /// <summary>
    /// Check if a capability is available.
    /// </summary>
    public CapabilityState GetCapabilityState(string capability)
    {
        return _Optional.TryGetValue(capability, out var state) ? state : CapabilityState.Unknown;
    }

    /// <summary>
    /// Add intrinsic capabilities.
    /// </summary>
    private void AddIntrinsic(string[] capabilities)
    {
        for (var i = 0; i < capabilities.Length; i++)
        {
            AddInternal(capabilities[i], CapabilityState.Intrinsic);
        }
    }

    /// <summary>
    /// Add a capability.
    /// </summary>
    private void AddInternal(string capability, CapabilityState state)
    {
        if (string.IsNullOrWhiteSpace(capability) || state == CapabilityState.Unknown)
            return;

        _Optional.Add(capability, state);
    }
}

#nullable restore
