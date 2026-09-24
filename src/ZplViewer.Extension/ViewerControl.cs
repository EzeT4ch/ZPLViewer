using Microsoft.VisualStudio.Extensibility.DebuggerVisualizers;
using Microsoft.VisualStudio.Extensibility.UI;

namespace ZplViewer.Extension;

internal sealed class ViewerControl : RemoteUserControl
{
    private readonly ViewerData data;

    public ViewerControl(VisualizerTarget target) : this(new ViewerData(target)) { }

    private ViewerControl(ViewerData data) : base(data) => this.data = data;

    protected override void Dispose(bool disposing)
    {
        if (disposing) data.Dispose();
        base.Dispose(disposing);
    }
}
