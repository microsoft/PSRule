// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.

namespace PSRule.Pipeline.Output;

/// <summary>
/// Unit tests for <see cref="HostPipelineWriter"/>.
/// </summary>
public sealed class HostPipelineWriterTests : OutputWriterBaseTests
{
    [Fact]
    public void WriteObject_WithSimpleObject_ShouldInvokeHostContextForObject()
    {
        var hostContext = new Mock<IHostContext>();
        var writer = new HostPipelineWriter(hostContext.Object, GetOption(), null);
        var expected = "TestObject";

        writer.WriteObject(expected, false);

        hostContext.Verify(h => h.WriteObject(expected, false), Times.Once);
    }

    [Fact]
    public void WriteResult_WithResult_ShouldInvokeHostContextForEachRecord()
    {
        var hostContext = new Mock<IHostContext>();
        var writer = new HostPipelineWriter(hostContext.Object, GetOption(), null);
        var expected = new InvokeResult(GetRun());
        var pass = GetPass();
        var fail = GetFail();
        expected.Add(pass);
        expected.Add(fail);

        writer.WriteResult(expected);

        hostContext.Verify(h => h.WriteObject(pass, false), Times.Once);
        hostContext.Verify(h => h.WriteObject(fail, false), Times.Once);
    }
}
