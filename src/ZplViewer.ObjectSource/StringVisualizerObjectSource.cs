using Microsoft.VisualStudio.DebuggerVisualizers;

namespace ZplViewer.ObjectSource;

public sealed class StringVisualizerObjectSource : VisualizerObjectSource
{
    public override void GetData(object target, Stream outgoingData)
    {
        if (target is not string text)
            throw new ArgumentException("ZPL Viewer only supports strings.", nameof(target));

        SerializeAsJson(outgoingData, text);
    }

    public override object CreateReplacementObject(object target, Stream incomingData)
    {
        if (target is not string)
            throw new ArgumentException("ZPL Viewer only supports strings.", nameof(target));

        return DeserializeFromJson(incomingData, typeof(string)) as string
            ?? throw new InvalidDataException("The replacement must be a string, not null.");
    }
}
