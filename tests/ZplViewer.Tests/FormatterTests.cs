using Xunit;
using ZplViewer.Core;

namespace ZplViewer.Tests;

public sealed class FormatterTests
{
    [Theory]
    [InlineData("^XA^FO40,500^AqN,35,35^FDtexto ^AqN literal^FS^XZ", "^AqN,35,35")]
    [InlineData("^CC!!XA!FO40,500!AqN,35,35!FDtexto!FS!XZ", "!AqN,35,35")]
    public void LowercaseFontIdentifierIsASeparateLosslessSegment(string source, string font)
    {
        var segments = ZplFormatter.GetSegments(source);
        Assert.Contains(font, segments);
        Assert.Equal(source, string.Concat(segments));
        Assert.Contains(Environment.NewLine + font + Environment.NewLine, ZplFormatter.Format(source));
    }

    [Theory]
    [InlineData("")]
    [InlineData("^XA^FO10,20^FD  Café _5E ~texto\r\nsegunda línea  ^FS^XZ")]
    [InlineData("^XA^FH_^FD_5EXA_7EDG^FS^XZ")]
    [InlineData("~DGR:ICON.GRF,8,1,FF818181818181FF^XA^FO10,10^XGR:ICON.GRF,1,1^FS^XZ")]
    [InlineData("^XA^GFB,4,4,1,^XA~^FS^XZ")]
    [InlineData("^CC!!XA!FO10,10!FDTexto!FS!XZ")]
    [InlineData("^XA^FXcomment ^FO fake command^FS^XZ")]
    public void SegmentsPreserveEveryCharacter(string source)
        => Assert.Equal(source, string.Concat(ZplFormatter.GetSegments(source)));

    [Fact]
    public void FormattingDoesNotSplitFieldsOrChangeTheirWhitespace()
    {
        const string field = "^FD  texto ^XA literal\r\n_5E  ";
        var result = ZplFormatter.Format("^XA^FO10,10" + field + "^FS^XZ");
        Assert.Contains(field, result);
        Assert.Contains("^XA" + Environment.NewLine + "^FO10,10", result);
    }

    [Fact]
    public void BinaryStreamIsOpaque()
    {
        const string source = "^XA^GFB,4,4,1,^FO~^FS^XZ";
        Assert.Equal(source, ZplFormatter.Format(source));
    }
}
