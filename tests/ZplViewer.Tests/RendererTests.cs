using SkiaSharp;
using Xunit;
using ZplViewer.Core;

namespace ZplViewer.Tests;

public sealed class RendererTests
{
    public static TheoryData<string> Labels => new()
    {
        "^XA^FO20,20^A0N,30,30^FDHello^FS^XZ",
        "^XA^CI28^FO20,20^A0N,30,30^FDCafé año^FS^XZ",
        "^XA^FO20,20^BY2^BCN,80,Y,N,N^FD12345678^FS^XZ",
        "^XA^FO20,20^BQN,2,5^FDLA,ZPL Viewer^FS^XZ",
        "^XA^FO20,20^GFA,8,8,1,FF818181818181FF^FS^XZ",
        "~DGR:ICON.GRF,8,1,FF818181818181FF^XA^FO20,20^XGR:ICON.GRF,3,3^FS^XZ",
    };

    [Theory]
    [MemberData(nameof(Labels))]
    public void ProducesNonblankPngWithRequestedDimensions(string source)
    {
        var result = new ZplRenderer().Render(source, new(100, 150, 8));
        using var bitmap = SKBitmap.Decode(Assert.Single(result.Pages));
        Assert.Equal(800, bitmap.Width);
        Assert.Equal(1200, bitmap.Height);
        Assert.Contains(bitmap.Pixels, color => color.Alpha > 0 && color.Red < 100 && color.Green < 100 && color.Blue < 100);
    }

    [Fact]
    public void RendersMultiplePages()
    {
        var result = new ZplRenderer().Render("^XA^FO10,10^GB20,20,2^FS^XZ^XA^FO10,10^GB40,40,2^FS^XZ", new());
        Assert.Equal(2, result.Pages.Count);
        Assert.False(result.Pages[0].SequenceEqual(result.Pages[1]));
    }

    [Theory]
    [InlineData("")]
    [InlineData("not ZPL")]
    [InlineData("^XA^FDunterminated")]
    [InlineData("^XA^XA^XZ^XZ")]
    [InlineData("^XZ^XA")]
    public void RejectsInvalidInput(string source)
        => Assert.Throws<ArgumentException>(() => new ZplRenderer().Render(source, new()));

    [Fact]
    public void RejectsExcessiveRasterSizeBeforeRendering()
        => Assert.Throws<ArgumentException>(() => new ZplRenderer().Render("^XA^XZ", new(300, 600, 24)));

    [Fact]
    public void HonorsCancellationBeforeRendering()
        => Assert.Throws<OperationCanceledException>(() => new ZplRenderer().Render("^XA^XZ", new(), new CancellationToken(true)));

    [Fact]
    public void PassingPrecomputedSegmentsProducesTheSameResultAsComputingThemInternally()
    {
        const string source = "^XA^FO20,20^A0N,30,30^FDHello^FS^XZ";
        var segments = ZplFormatter.GetSegments(source);
        var withSegments = new ZplRenderer().Render(source, new(100, 150, 8), segments: segments);
        var withoutSegments = new ZplRenderer().Render(source, new(100, 150, 8));
        Assert.True(Assert.Single(withSegments.Pages).SequenceEqual(Assert.Single(withoutSegments.Pages)));
    }
}
