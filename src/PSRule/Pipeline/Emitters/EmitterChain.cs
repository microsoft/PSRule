// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using PSRule.Emitters;

namespace PSRule.Pipeline.Emitters;

/// <summary>
/// A chain of emitters.
/// </summary>
internal delegate bool EmitterChain(IEmitterContext context, object o, Type type);
