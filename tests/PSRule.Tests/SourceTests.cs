// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Management.Automation;
using Xunit;

namespace PSRule
{
    public sealed class SourceTests
    {
        [Fact]
        public void TargetSourceInfo()
        {
            var source = new PSObject();
            source.Properties.Add(new PSNoteProperty("file", "file.json"));
            source.Properties.Add(new PSNoteProperty("line", 100));
            source.Properties.Add(new PSNoteProperty("position", 1000));
            var info = new PSObject();
            info.Properties.Add(new PSNoteProperty("source", new PSObject[] { source }));
            var o = new PSObject();
            o.Properties.Add(new PSNoteProperty("_PSRule", info));
            o.ConvertTargetInfoProperty();

            var actual = o.GetSourceInfo();
            Assert.NotNull(actual);
            Assert.Equal("file.json", actual[0].File);
            Assert.Equal(100, actual[0].Line);
            Assert.Equal(1000, actual[0].Position);
        }
    }
}
