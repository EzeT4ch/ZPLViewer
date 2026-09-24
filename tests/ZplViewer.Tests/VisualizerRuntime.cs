using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;

namespace ZplViewer.Tests;

internal static class VisualizerRuntime
{
    [ModuleInitializer]
    internal static void Initialize()
    {
        // VS supplies this implementation at debug time (NuGet only ships ref assemblies).
        // Preloading also permits the newer VS 2026 assembly to satisfy the SDK 17 reference.
        var runtimePath = typeof(VisualizerRuntime).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
            .Single(attribute => attribute.Key == "VisualizerRuntimePath").Value!;
        AssemblyLoadContext.Default.LoadFromAssemblyPath(runtimePath);
    }
}
