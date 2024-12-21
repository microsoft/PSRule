// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.


// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Emitters;

/// <summary>
/// A chain of emitters.
/// </summary>
internal delegate bool EmitterChain(IEmitterContext context, object o, Type type);
