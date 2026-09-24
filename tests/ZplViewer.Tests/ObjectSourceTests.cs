using System.Text;
using System.IO;
using Newtonsoft.Json;
using Xunit;
using ZplViewer.ObjectSource;

namespace ZplViewer.Tests;

public sealed class ObjectSourceTests
{
    [Theory]
    [InlineData("")]
    [InlineData("^XA\r\n^FD  Café \\\" _5E  ^FS\n^XZ")]
    public void ReadAndReplacementPreserveExactString(string text)
    {
        var source = new StringVisualizerObjectSource();
        using var output = new MemoryStream();
        source.GetData(text, output);
        Assert.Equal(text, JsonConvert.DeserializeObject<string>(Encoding.UTF8.GetString(output.ToArray())));
        using var input = new MemoryStream(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(text)));
        Assert.Equal(text, source.CreateReplacementObject("original", input));
    }

    [Fact]
    public void RejectsNullReplacement()
    {
        using var input = new MemoryStream(Encoding.UTF8.GetBytes("null"));
        Assert.Throws<InvalidDataException>(() => new StringVisualizerObjectSource().CreateReplacementObject("original", input));
    }
}
