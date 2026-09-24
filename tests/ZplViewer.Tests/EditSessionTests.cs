using Xunit;
using ZplViewer.Core;

namespace ZplViewer.Tests;

public sealed class EditSessionTests
{
    [Fact]
    public void ObservingAndEditingDoNotWriteTheSource()
    {
        var session = new EditSession();
        session.Observe("original", true);
        Assert.False(session.Snapshot.CanApply);
        session.Edit("edited");
        Assert.Equal("original", session.Snapshot.Source);
        Assert.True(session.Snapshot.CanApply);
    }

    [Fact]
    public void ResumingPreservesDraftAndDisablesApply()
    {
        var session = new EditSession();
        session.Observe("original", true);
        session.Edit("draft");
        session.Unavailable();
        Assert.Equal("draft", session.Snapshot.Draft);
        Assert.False(session.Snapshot.CanApply);
        session.Observe("original", true);
        Assert.True(session.Snapshot.CanApply);
    }

    [Fact]
    public void ChangedTargetRequiresExplicitReloadEvenAfterFurtherNotifications()
    {
        var session = new EditSession();
        session.Observe("original", true);
        session.Edit("draft");
        session.Observe("other", true);
        session.Observe("other", true);
        Assert.Equal("draft", session.Snapshot.Draft);
        Assert.True(session.Snapshot.Conflict);
        Assert.False(session.Snapshot.CanApply);
        session.Observe("other", true, reload: true);
        Assert.Equal("other", session.Snapshot.Draft);
        Assert.False(session.Snapshot.Conflict);
    }

    [Fact]
    public void ReadOnlyExpressionNeverEnablesApply()
    {
        var session = new EditSession();
        session.Observe("original", false);
        session.Edit("draft");
        Assert.False(session.Snapshot.CanApply);
    }

    [Fact]
    public void VerificationPreservesEditsMadeWhileWriting()
    {
        var session = new EditSession();
        session.Observe("original", true);
        session.Edit("write this");
        session.Edit("newer draft");
        session.Applied("write this");
        Assert.Equal("newer draft", session.Snapshot.Draft);
        Assert.Equal("write this", session.Snapshot.Source);
        Assert.True(session.Snapshot.Dirty);
    }
}
