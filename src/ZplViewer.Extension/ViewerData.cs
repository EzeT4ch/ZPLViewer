using System.Globalization;
using System.Runtime.Serialization;
using Microsoft.VisualStudio.Extensibility.DebuggerVisualizers;
using Microsoft.VisualStudio.Extensibility.UI;
using Microsoft.VisualStudio.RpcContracts.DebuggerVisualizers;
using ZplViewer.Core;

namespace ZplViewer.Extension;

[DataContract]
internal sealed class ViewerData : NotifyPropertyChangedObject, IDisposable
{
    private readonly VisualizerTarget target;
    private readonly EditSession session = new();
    private readonly ZplRenderer renderer = new();
    private readonly SemaphoreSlim operations = new(1, 1);
    private readonly CancellationTokenSource lifetime = new();
    private readonly string imageDirectory = Path.Combine(Path.GetTempPath(), "ZplViewer", Guid.NewGuid().ToString("N"));
    private List<string> images = [];
    private string status = "En espera del depurador…";
    private string formatted = "";
    private string width = "100", height = "150", density = "8";
    private double zoom = 100;
    private int pageIndex;
    private long targetEpoch;
    private long contextEpoch;
    private bool busy, rendered, disposed;
    private string? renderedDraft;
    private RenderSettings? renderedSettings;

    public ViewerData(VisualizerTarget target)
    {
        this.target = target;
        ShippingPreset = new AsyncCommand((_, _) =>
        {
            var preset = RenderSettings.Shipping4By8;
            WidthMm = preset.WidthMm.ToString(CultureInfo.InvariantCulture);
            HeightMm = preset.HeightMm.ToString(CultureInfo.InvariantCulture);
            DotsPerMm = preset.DotsPerMm.ToString(CultureInfo.InvariantCulture);
            return Task.CompletedTask;
        });
        Render = new AsyncCommand(async (parameter, token) => await RunAsync(() => RenderAsync(parameter as string ?? Draft, token), token));
        Apply = new AsyncCommand(async (parameter, token) => await RunAsync(() => ApplyAsync(parameter as string ?? Draft, token), token));
        Reload = new AsyncCommand(async (_, token) => await RunAsync(async () =>
        {
            if (!session.Snapshot.Available) return;
            var epoch = Interlocked.Read(ref targetEpoch);
            var text = await target.ObjectSource.RequestDataAsync<string>(null, token);
            if (epoch != Interlocked.Read(ref targetEpoch)) return;
            session.Observe(text ?? "", target.IsTargetReplaceable, reload: true);
            NotifySession();
            await RenderAsync(Draft, token);
        }, token));
        Previous = new AsyncCommand((_, _) => { pageIndex = Math.Max(0, pageIndex - 1); NotifyImage(); return Task.CompletedTask; });
        Next = new AsyncCommand((_, _) => { pageIndex = Math.Min(Math.Max(0, images.Count - 1), pageIndex + 1); NotifyImage(); return Task.CompletedTask; });
        target.Changed += OnTargetStateChangedAsync;
        // The pinned SDK marks expression notifications experimental. They are required to
        // invalidate drafts when a tool window switches to a different expression.
#pragma warning disable VSEXTPREVIEW_DEBUGGERVISUALIZERS_EXPRESSION
        target.VisualizedExpressionChanged += OnExpressionChangedAsync;
#pragma warning restore VSEXTPREVIEW_DEBUGGERVISUALIZERS_EXPRESSION
    }

    [DataMember] public IAsyncCommand Render { get; }
    [DataMember] public IAsyncCommand ShippingPreset { get; }
    [DataMember] public IAsyncCommand Apply { get; }
    [DataMember] public IAsyncCommand Reload { get; }
    [DataMember] public IAsyncCommand Previous { get; }
    [DataMember] public IAsyncCommand Next { get; }
    [DataMember] public string Draft
    {
        get => session.Snapshot.Draft;
        set { session.Edit(value ?? ""); NotifySession(); }
    }
    [DataMember] public string Formatted { get => formatted; private set => SetProperty(ref formatted, value); }
    [DataMember] public string Status { get => status; private set => SetProperty(ref status, value); }
    [DataMember] public string WidthMm { get => width; set { SetProperty(ref width, value); NotifyPreview(); } }
    [DataMember] public string HeightMm { get => height; set { SetProperty(ref height, value); NotifyPreview(); } }
    [DataMember] public string DotsPerMm { get => density; set { SetProperty(ref density, value); NotifyPreview(); } }
    [DataMember] public double Zoom { get => zoom; set { SetProperty(ref zoom, Math.Clamp(value, 10, 400)); NotifyImage(); } }
    [DataMember] public double ImageWidth => (renderedSettings?.WidthMm ?? 100) / 25.4 * 96 * Zoom / 100;
    [DataMember] public double ImageHeight => (renderedSettings?.HeightMm ?? 150) / 25.4 * 96 * Zoom / 100;
    [DataMember] public string? ImageUri => images.Count == 0 ? null : new Uri(images[pageIndex]).AbsoluteUri;
    [DataMember] public string Page => images.Count == 0 ? "Sin imagen" : $"Etiqueta {pageIndex + 1} de {images.Count}";
    [DataMember] public bool CanApply => !busy && session.Snapshot.CanApply;
    [DataMember] public bool CanReload => !busy && session.Snapshot.Available;
    [DataMember] public bool CanRender => !busy;
    [DataMember] public string DebugStatus => !session.Snapshot.Available
        ? "Instantánea: el valor no está disponible. Escritura deshabilitada."
        : session.Snapshot.Conflict ? "El valor cambió. Recargá la variable antes de aplicar; el borrador se conserva."
        : !session.Snapshot.Replaceable ? "El depurador permite consultar esta expresión, pero no reemplazarla."
        : session.Snapshot.Dirty ? "Borrador modificado. Aplicar a variable escribe en el proceso depurado." : "Sin cambios pendientes.";
    [DataMember] public string PreviewStatus => !rendered ? "Vista previa pendiente."
        : renderedDraft != Draft || !TrySettings(out var settings) || settings != renderedSettings
            ? "Vista previa desactualizada. Pulsá Actualizar vista previa." : "Vista previa actualizada.";

    private Task OnExpressionChangedAsync(string? expression)
    {
        Interlocked.Increment(ref targetEpoch);
        Interlocked.Increment(ref contextEpoch);
        if (session.Snapshot.Dirty) session.RequireReload();
        NotifySession();
        return Task.CompletedTask;
    }

    private async Task OnTargetStateChangedAsync(VisualizerTargetStateNotification notification)
    {
        var epoch = Interlocked.Increment(ref targetEpoch);
        if (notification == VisualizerTargetStateNotification.Unavailable)
        {
            Interlocked.Increment(ref contextEpoch);
            session.Unavailable();
            NotifySession();
            return;
        }
        if (notification is not (VisualizerTargetStateNotification.Available or VisualizerTargetStateNotification.ValueUpdated)) return;
        // Notifications must finish reading before returning to the SDK. Do not acquire the
        // command semaphore here: replacement can itself trigger a value notification.
        try
        {
            var text = await target.ObjectSource.RequestDataAsync<string>(null, lifetime.Token);
            if (epoch != Interlocked.Read(ref targetEpoch)) return;
            var before = session.Snapshot;
            session.Observe(text ?? "", target.IsTargetReplaceable);
            NotifySession();
            if (!busy && (!rendered || (!before.Dirty && before.Source != text)))
                await RunAsync(() => RenderAsync(Draft, lifetime.Token), lifetime.Token);
        }
        catch (OperationCanceledException) { }
        catch (VisualizerTargetUnavailableException) { session.Unavailable(); NotifySession(); }
        catch (Exception exception) { if (!disposed) Status = $"No se pudo leer la variable: {exception.Message}"; }
    }

    private async Task ApplyAsync(string draft, CancellationToken token)
    {
        session.Edit(draft);
        var expected = session.Snapshot;
        if (!expected.CanApply) return;
        var epoch = Interlocked.Read(ref targetEpoch);
        var context = Interlocked.Read(ref contextEpoch);
        var actual = await target.ObjectSource.RequestDataAsync<string>(null, token);
        if (epoch != Interlocked.Read(ref targetEpoch) || session.Snapshot.Revision != expected.Revision
            || actual != expected.Source || !target.IsTargetReplaceable)
        {
            session.RequireReload();
            Status = "El destino o el borrador cambió antes de escribir. Recargá la variable.";
            return;
        }
        try
        {
            await target.ObjectSource.ReplaceTargetObjectAsync(expected.Draft, null, token);
            // A successful RPC is not enough: verify exactly what the debuggee now contains.
            actual = await target.ObjectSource.RequestDataAsync<string>(null, token);
        }
        catch
        {
            // The write may have succeeded even if its reply or verification failed.
            session.RequireReload();
            throw;
        }
        if (!session.Snapshot.Available || context != Interlocked.Read(ref contextEpoch) || actual != expected.Draft)
        {
            session.RequireReload();
            Status = "No se pudo confirmar el valor escrito. Recargá para comprobar el estado actual.";
            return;
        }
        session.Applied(actual);
        Status = "Variable actualizada y verificada.";
    }

    private async Task RenderAsync(string draft, CancellationToken token)
    {
        session.Edit(draft);
        if (!TrySettings(out var settings)) throw new ArgumentException("Ingresá dimensiones numéricas y una densidad válida.");
        Formatted = draft.Length <= ZplRenderer.MaximumSourceLength ? ZplFormatter.Format(draft) : "Texto demasiado grande para formatear.";
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(token, lifetime.Token);
        var result = await Task.Run(() => renderer.Render(draft, settings, linked.Token), linked.Token);
        linked.Token.ThrowIfCancellationRequested();
        Directory.CreateDirectory(imageDirectory);
        var nextImages = new List<string>();
        foreach (var png in result.Pages)
        {
            var path = Path.Combine(imageDirectory, Guid.NewGuid().ToString("N") + ".png");
            await File.WriteAllBytesAsync(path, png, linked.Token);
            nextImages.Add(path);
        }
        var previousImages = images;
        images = nextImages;
        pageIndex = 0;
        renderedDraft = draft;
        renderedSettings = settings;
        rendered = true;
        NotifyImage();
        NotifyPreview();
        foreach (var path in previousImages) TryDelete(path);
        Status = string.Join(Environment.NewLine, result.Warnings);
    }

    private bool TrySettings(out RenderSettings settings)
    {
        settings = new();
        if (!TryNumber(width, out var w) || !TryNumber(height, out var h) || !int.TryParse(density, out var d)) return false;
        settings = new(w, h, d);
        return true;
    }

    private static bool TryNumber(string value, out double number) =>
        double.TryParse(value.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out number);

    private async Task RunAsync(Func<Task> operation, CancellationToken token)
    {
        if (disposed) return;
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(token, lifetime.Token);
        var entered = false;
        try
        {
            if (!await operations.WaitAsync(0, linked.Token)) return;
            entered = true;
            if (disposed) return;
            busy = true;
            NotifySession();
            await operation();
        }
        catch (OperationCanceledException) { if (!disposed) Status = "Operación cancelada."; }
        catch (VisualizerTargetUnavailableException) { session.Unavailable(); Status = "El depurador ya no tiene disponible este valor."; }
        catch (Exception exception) { if (!disposed) Status = $"No se pudo completar la operación: {exception.Message}"; }
        finally
        {
            if (entered)
            {
                busy = false;
                operations.Release();
                if (!disposed) NotifySession();
                else CleanupImages();
            }
        }
    }

    private void NotifySession()
    {
        foreach (var property in new[] { nameof(Draft), nameof(CanApply), nameof(CanReload), nameof(CanRender), nameof(DebugStatus), nameof(PreviewStatus) })
            RaiseNotifyPropertyChangedEvent(property);
    }
    private void NotifyPreview() => RaiseNotifyPropertyChangedEvent(nameof(PreviewStatus));
    private void NotifyImage()
    {
        foreach (var property in new[] { nameof(ImageUri), nameof(Page), nameof(ImageWidth), nameof(ImageHeight) })
            RaiseNotifyPropertyChangedEvent(property);
    }
    private static void TryDelete(string path)
    {
        try { File.Delete(path); }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
    }
    public void Dispose()
    {
        if (disposed) return;
        disposed = true;
        target.Changed -= OnTargetStateChangedAsync;
#pragma warning disable VSEXTPREVIEW_DEBUGGERVISUALIZERS_EXPRESSION
        target.VisualizedExpressionChanged -= OnExpressionChangedAsync;
#pragma warning restore VSEXTPREVIEW_DEBUGGERVISUALIZERS_EXPRESSION
        lifetime.Cancel();
        target.Dispose();
        CleanupImages();
    }

    private void CleanupImages()
    {
        // Only remove files from this instance's private, randomly named directory.
        try
        {
            if (!Directory.Exists(imageDirectory)) return;
            foreach (var path in Directory.EnumerateFiles(imageDirectory, "*.png")) TryDelete(path);
            Directory.Delete(imageDirectory);
        }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
    }
}
