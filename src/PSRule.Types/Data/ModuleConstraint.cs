// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Data
{
    public sealed class ModuleConstraint
    {
        public ModuleConstraint(string module, IVersionConstraint constraint)
        {
            Module = module;
            Constraint = constraint;
        }

        public string Module { get; }

        public IVersionConstraint Constraint { get; }
    }
}
