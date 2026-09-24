namespace ZplViewer.Core;

public sealed record EditSnapshot(string Source, string Draft, bool Available, bool Replaceable, bool Conflict, long Revision)
{
    public bool Dirty => Draft != Source;
    public bool CanApply => Available && Replaceable && !Conflict && Dirty;
}

/// <summary>Keeps debugger observations separate from the user's working copy.</summary>
public sealed class EditSession
{
    private readonly object sync = new();
    private EditSnapshot snapshot = new("", "", false, false, false, 0);
    private bool initialized;

    public EditSnapshot Snapshot { get { lock (sync) return snapshot; } }

    public void Edit(string draft)
    {
        lock (sync)
            if (snapshot.Draft != draft)
                snapshot = snapshot with { Draft = draft, Revision = snapshot.Revision + 1 };
    }

    public void Observe(string value, bool replaceable, bool reload = false)
    {
        lock (sync)
        {
            var conflict = !reload && initialized && (snapshot.Conflict || (snapshot.Dirty && value != snapshot.Source));
            var draft = reload || !initialized || (!snapshot.Dirty && !snapshot.Conflict) ? value : snapshot.Draft;
            snapshot = new(value, draft, true, replaceable, conflict, snapshot.Revision + 1);
            initialized = true;
        }
    }

    public void Unavailable()
    {
        lock (sync) snapshot = snapshot with { Available = false, Revision = snapshot.Revision + 1 };
    }

    public void RequireReload()
    {
        lock (sync) snapshot = snapshot with { Conflict = true, Revision = snapshot.Revision + 1 };
    }

    public void Applied(string written)
    {
        lock (sync)
            snapshot = snapshot with { Source = written, Conflict = false, Revision = snapshot.Revision + 1 };
    }
}
