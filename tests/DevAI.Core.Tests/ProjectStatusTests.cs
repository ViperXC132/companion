using DevAI.Core.Status;
using Xunit;

namespace DevAI.Core.Tests;

public sealed class ProjectStatusTests
{
    [Fact]
    public void CurrentStatus_IdentifiesAppAsNotProductionReady()
    {
        Assert.Equal("Dev AI Orb", ProjectStatus.Current.Name);
        Assert.False(ProjectStatus.Current.IsProductionReady);
    }

    [Fact]
    public void CompanionState_ContainsExplicitInteractionStates()
    {
        Assert.Contains(CompanionState.Idle, Enum.GetValues<CompanionState>());
        Assert.Contains(CompanionState.Listening, Enum.GetValues<CompanionState>());
        Assert.Contains(CompanionState.Thinking, Enum.GetValues<CompanionState>());
        Assert.Contains(CompanionState.Speaking, Enum.GetValues<CompanionState>());
        Assert.Contains(CompanionState.NeedsPermission, Enum.GetValues<CompanionState>());
    }
}
