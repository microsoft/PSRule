// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Benchmark
{
    public sealed class TargetObject
    {
        public TargetObject(string name, string message, string value)
        {
            Name = name;
            Message = message;
            Value = value;
        }

        public string Name { get; private set; }

        public string Message { get; private set; }

        public string Value { get; private set; }
    }
}
