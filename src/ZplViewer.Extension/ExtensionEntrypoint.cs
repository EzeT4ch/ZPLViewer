using Microsoft.VisualStudio.Extensibility;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;

namespace ZplViewer.Extension;

[VisualStudioContribution]
internal sealed class ExtensionEntrypoint : Microsoft.VisualStudio.Extensibility.Extension
{
    static ExtensionEntrypoint()
    {
        // ServiceHub does not probe our NuGet runtime folders for native DLLs.
        var context = AssemblyLoadContext.GetLoadContext(typeof(ExtensionEntrypoint).Assembly)!;
        context.ResolvingUnmanagedDll += ResolveRendererLibrary;
    }

    private static IntPtr ResolveRendererLibrary(Assembly assembly, string libraryName)
    {
        if (!((assembly.GetName().Name == "SkiaSharp" && libraryName == "libSkiaSharp")
            || (assembly.GetName().Name == "HarfBuzzSharp" && libraryName == "libHarfBuzzSharp")))
            return IntPtr.Zero;
        var architecture = RuntimeInformation.ProcessArchitecture switch
        {
            Architecture.X64 => "win-x64",
            Architecture.Arm64 => "win-arm64",
            _ => throw new PlatformNotSupportedException("El visor requiere un host Windows x64 o ARM64."),
        };
        var directory = Path.GetDirectoryName(typeof(ExtensionEntrypoint).Assembly.Location)!;
        return NativeLibrary.Load(Path.Combine(directory, "runtimes", architecture, "native", libraryName + ".dll"));
    }

    public override ExtensionConfiguration ExtensionConfiguration => new()
    {
        Metadata = new(
            id: "ZplViewer.54af8e6a-2269-4dc4-8aee-aa95554c3954",
            version: ExtensionAssemblyVersion,
            publisherName: "Ezequiel Benitez",
            displayName: "ZPL Viewer",
            description: "Visualiza y edita etiquetas ZPL localmente desde el depurador."),
    };
}
