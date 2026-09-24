using Microsoft.VisualStudio.Extensibility;
using Microsoft.VisualStudio.Extensibility.DebuggerVisualizers;
using Microsoft.VisualStudio.RpcContracts.RemoteUI;
using ZplViewer.ObjectSource;

namespace ZplViewer.Extension;

[VisualStudioContribution]
internal sealed class ZplDebuggerVisualizerProvider : DebuggerVisualizerProvider
{
    public ZplDebuggerVisualizerProvider(ExtensionEntrypoint extension, VisualStudioExtensibility extensibility)
        : base(extension, extensibility) { }

    public override DebuggerVisualizerProviderConfiguration DebuggerVisualizerProviderConfiguration =>
        // Register the runtime implementation assemblies, not net8's System.Runtime reference facade.
        new(
            new VisualizerTargetType("%Visualizer.DisplayName%", "System.String, System.Private.CoreLib, Version=10.0.0.0, Culture=neutral, PublicKeyToken=7cec85d7bea7798e"),
            new VisualizerTargetType("%Visualizer.DisplayName%", "System.String, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"))
        {
            Style = VisualizerStyle.ToolWindow,
            VisualizerObjectSourceType = new(typeof(StringVisualizerObjectSource)),
        };

    public override Task<IRemoteUserControl> CreateVisualizerAsync(VisualizerTarget visualizerTarget, CancellationToken cancellationToken)
        => Task.FromResult<IRemoteUserControl>(new ViewerControl(visualizerTarget));
}
