using BinaryKits.Zpl.Viewer;
using System.IO;
using SkiaSharp;
using Xunit;
using ZplViewer.Core;

namespace ZplViewer.Tests;

public sealed class FontRenderingTests
{
    [Theory]
    [InlineData("CANALIZADOR:", 35, 220)]
    [InlineData("TIPO DE CONT:", 28, 160)]
    [InlineData("TOTAL ETIQUETAS:", 30, 210)]
    [InlineData("UNIDADES ETIQUETA:", 28, 230)]
    [InlineData("UNIDADES PEDIDO:", 28, 230)]
    [InlineData("CAJAS ETIQUETA:", 28, 230)]
    public void FixedCaptionFitsBeforeFollowingField(string text, int size, int available)
    {
        using var bitmap = Render($"^XA^FO0,0^AqN,{size},{size}^FD{text}^FS^XZ");
        var bounds = InkBounds(bitmap);
        Assert.True(bounds.Right < available, $"{text}: right edge {bounds.Right}, available {available} dots");
    }

    [Fact]
    public void LowercaseQUsesBundledFontButUppercaseQRetainsBinaryKitsBehavior()
    {
        const string source = "^XA^FO20,20^AQN,28,28^FDTEST^FS^XZ";
        var storage = new PrinterStorage();
        var label = new ZplAnalyzer(storage).Analyze(source).LabelInfos.Single();
        var expected = new ZplElementDrawer(storage).Draw(label.ZplElements, 100, 150, 8);
        var actual = new ZplRenderer().Render(source, new());
        Assert.Equal(expected, actual.Pages.Single());
        Assert.DoesNotContain(actual.Warnings, warning => warning.StartsWith("Fuente q"));
        var q = new ZplRenderer().Render(source.Replace("^AQ", "^Aq"), new());
        Assert.Contains(q.Warnings, warning => warning.StartsWith("Fuente q"));
        Assert.False(expected.SequenceEqual(q.Pages.Single()));
    }

    [Fact]
    public void WidthAndHeightAreIndependentAndLongFieldsRemainLong()
    {
        using var normal = Render("^XA^FO20,20^AqN,30,30^FDABCDEFGHIJ^FS^XZ");
        using var wide = Render("^XA^FO20,20^AqN,30,60^FDABCDEFGHIJ^FS^XZ");
        using var tall = Render("^XA^FO20,20^AqN,60,30^FDABCDEFGHIJ^FS^XZ");
        var a = InkBounds(normal); var b = InkBounds(wide); var c = InkBounds(tall);
        Assert.InRange(b.Width / (double)a.Width, 1.9, 2.1);
        Assert.InRange(c.Height / (double)a.Height, 1.8, 2.2);
        using var longField = Render("^XA^FO0,0^AqN,30,30^FD" + new string('W', 40) + "^FS^XZ");
        Assert.True(InkBounds(longField).Right > 230);
    }

    [Fact]
    public void FieldOriginAndTypesetKeepTheirCoordinateSemantics()
    {
        using var origin = Render("^XA^FO40,100^AqN,30,30^FDCafé año^FS^XZ");
        using var baseline = Render("^XA^FT40,100^AqN,30,30^FDCafé año^FS^XZ");
        var a = InkBounds(origin); var b = InkBounds(baseline);
        Assert.Equal(a.Left, b.Left);
        Assert.Equal(a.Width, b.Width);
        Assert.True(a.Top > b.Top);
        Assert.InRange(b.Bottom, 95, 110);
    }

    [Theory]
    [InlineData("shipping-template.zpl")]
    [InlineData("shipping-example.zpl")]
    public void ShippingFixturePreservesSourceAndNonTextGraphics(string file)
    {
        var source = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Labels", file));
        Assert.Equal(source, string.Concat(ZplFormatter.GetSegments(source)));
        var result = new ZplRenderer().Render(source, RenderSettings.Shipping4By8);
        using var actual = SKBitmap.Decode(result.Pages.Single());
        var storage = new PrinterStorage();
        var label = new ZplAnalyzer(storage).Analyze(source).LabelInfos.Single();
        using var before = SKBitmap.Decode(new ZplElementDrawer(storage).Draw(label.ZplElements, 101.6, 203.2, 8));
        Assert.Equal(before.Width, actual.Width);
        Assert.Equal(before.Height, actual.Height);
        foreach (var region in new[] { new SKRectI(18, 137, 770, 143), new SKRectI(40, 530, 400, 580), new SKRectI(110, 1435, 700, 1545) })
            for (var y = region.Top; y < region.Bottom; y++)
                for (var x = region.Left; x < region.Right; x++)
                    Assert.Equal(before.GetPixel(x, y), actual.GetPixel(x, y));
    }

    private static SKBitmap Render(string source) => SKBitmap.Decode(new ZplRenderer().Render(source, new()).Pages.Single());

    private static SKRectI InkBounds(SKBitmap bitmap)
    {
        var left = bitmap.Width; var top = bitmap.Height; var right = -1; var bottom = -1;
        for (var y = 0; y < bitmap.Height; y++)
            for (var x = 0; x < bitmap.Width; x++)
            {
                var p = bitmap.GetPixel(x, y);
                if (p.Alpha < 100 || p.Red > 100) continue;
                left = Math.Min(left, x); top = Math.Min(top, y); right = Math.Max(right, x); bottom = Math.Max(bottom, y);
            }
        Assert.True(right >= left);
        return new(left, top, right + 1, bottom + 1);
    }
}
