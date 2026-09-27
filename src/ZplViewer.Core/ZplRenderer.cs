using BinaryKits.Zpl.Viewer;
using BinaryKits.Zpl.Viewer.ElementDrawers;
using SkiaSharp;

namespace ZplViewer.Core;

public sealed record RenderSettings(double WidthMm = 100, double HeightMm = 150, int DotsPerMm = 8)
{
    public static RenderSettings Shipping4By8 { get; } = new(101.6, 203.2, 8);
    public void Validate()
    {
        if (!double.IsFinite(WidthMm) || !double.IsFinite(HeightMm) || WidthMm <= 0 || HeightMm <= 0
            || WidthMm > 300 || HeightMm > 600 || DotsPerMm is not (6 or 8 or 12 or 24)
            || WidthMm * HeightMm * DotsPerMm * DotsPerMm > 32_000_000)
            throw new ArgumentException("Tamaño inválido: hasta 300 × 600 mm, 6/8/12/24 puntos/mm y 32 megapíxeles.");
    }
}

public sealed record RenderResult(IReadOnlyList<byte[]> Pages, IReadOnlyList<string> Warnings);

public sealed class ZplRenderer
{
    public const int MaximumSourceLength = 2_000_000;
    public const int MaximumPages = 50;

    // The embedded font is immutable and identical for every render; parsing it from the
    // resource stream on every call was pure overhead.
    private static readonly Lazy<SKTypeface> EmbeddedTypeface = new(() =>
    {
        using var fontStream = typeof(ZplRenderer).Assembly.GetManifestResourceStream("ZplViewer.Core.Fonts.RobotoCondensed-Bold.ttf")
            ?? throw new InvalidOperationException("No se encontró la fuente incluida en el visor.");
        return SKTypeface.FromStream(fontStream)
            ?? throw new InvalidOperationException("No se pudo cargar Roboto Condensed Bold.");
    }, LazyThreadSafetyMode.ExecutionAndPublication);

    public RenderResult Render(string source, RenderSettings settings, CancellationToken cancellationToken = default, IReadOnlyList<string>? segments = null)
    {
        settings.Validate();
        if (string.IsNullOrWhiteSpace(source))
            throw new ArgumentException("La cadena está vacía.");
        if (source.Length > MaximumSourceLength)
            throw new ArgumentException("La vista previa admite hasta 2 millones de caracteres.");
        segments ??= ZplFormatter.GetSegments(source);
        var starts = segments.Count(s => s.StartsWith("^XA", StringComparison.Ordinal));
        var ends = segments.Count(s => s.StartsWith("^XZ", StringComparison.Ordinal));
        if (starts == 0 || starts != ends)
            throw new ArgumentException("Se requieren etiquetas completas delimitadas por ^XA y ^XZ.");
        if (starts > MaximumPages)
            throw new ArgumentException($"La vista previa admite hasta {MaximumPages} etiquetas.");
        var insideLabel = false;
        foreach (var segment in segments)
        {
            if (segment.StartsWith("^XA", StringComparison.Ordinal))
            {
                if (insideLabel) throw new ArgumentException("Se encontró ^XA dentro de una etiqueta sin cerrar.");
                insideLabel = true;
            }
            else if (segment.StartsWith("^XZ", StringComparison.Ordinal))
            {
                if (!insideLabel) throw new ArgumentException("Se encontró ^XZ sin una etiqueta abierta.");
                insideLabel = false;
            }
        }
        if (starts * settings.WidthMm * settings.HeightMm * settings.DotsPerMm * settings.DotsPerMm > 64_000_000)
            throw new ArgumentException("El documento supera el límite de 64 megapíxeles. Reducí tamaño, densidad o cantidad de etiquetas.");

        cancellationToken.ThrowIfCancellationRequested();
        // Storage belongs to this document: downloads never leak between debugged variables.
        var storage = new PrinterStorage();
        var analysis = new ZplAnalyzer(storage).Analyze(source);
        var typeface = EmbeddedTypeface.Value;
        var options = new DrawerOptions { ReplaceDashWithEnDash = false, ReplaceUnderscoreWithEnSpace = false };
        var fallback = options.FontManager.FontLoader;
        var substitutedQ = false;
        options.FontManager.FontLoader = name =>
        {
            if (name == "q") substitutedQ = true;
            return name is "0" or "q" ? typeface : fallback(name);
        };
        var drawer = new ZplElementDrawer(storage, options);
        var pages = new List<byte[]>();
        foreach (var label in analysis.LabelInfos)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (pages.Count == MaximumPages)
                throw new ArgumentException("Demasiadas etiquetas para la vista previa.");
            pages.Add(drawer.Draw(label.ZplElements, settings.WidthMm, settings.HeightMm, settings.DotsPerMm));
        }
        if (pages.Count == 0)
            throw new ArgumentException("No se encontraron etiquetas renderizables.");
        var warnings = new List<string> { "Vista aproximada de BinaryKits: comandos y fuentes no compatibles pueden omitirse. No reproduce recursos almacenados en una impresora física." };
        if (substitutedQ)
            warnings.Add("Fuente q sustituida por Roboto Condensed Bold incluida en el visor; aproximación local, no equivalente a Q ni a una fuente descargada en la impresora.");
        return new(pages, warnings);
    }
}
