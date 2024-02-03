// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

using System.Management.Automation;

namespace PSRule;

public sealed class TargetInfoTests
{
    [Fact]
    public void TargetSourceInfo()
    {
        var source = new PSObject();
        source.Properties.Add(new PSNoteProperty("file", "file.json"));
        source.Properties.Add(new PSNoteProperty("line", 100));
        source.Properties.Add(new PSNoteProperty("position", 1000));
        source.Properties.Add(new PSNoteProperty("Type", "Origin"));
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
        Assert.Equal("Origin", actual[0].Type);
    }

    [Fact]
    public void TargetIssueInfo()
    {
        var issue = new PSObject();
        issue.Properties.Add(new PSNoteProperty("Type", "CustomIssue"));
        issue.Properties.Add(new PSNoteProperty("name", "Issue.1"));
        issue.Properties.Add(new PSNoteProperty("message", "Some issue"));
        var info = new PSObject();
        info.Properties.Add(new PSNoteProperty("issue", new PSObject[] { issue }));
        var o = new PSObject();
        o.Properties.Add(new PSNoteProperty("_PSRule", info));
        o.ConvertTargetInfoProperty();

        var actual = o.GetIssueInfo();
        Assert.NotNull(actual);
        Assert.Equal("CustomIssue", actual[0].Type);
        Assert.Equal("Issue.1", actual[0].Name);
        Assert.Equal("Some issue", actual[0].Message);
    }

    [Fact]
    public void TargetPath()
    {
        var info = new PSObject();
        info.Properties.Add(new PSNoteProperty("path", "resources[0]"));
        var o = new PSObject();
        o.Properties.Add(new PSNoteProperty("_PSRule", info));
        o.ConvertTargetInfoProperty();

        var actual = o.GetTargetPath();
        Assert.Equal("resources[0]", actual);
    }
}
