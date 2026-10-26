namespace DevAI.Core.Status;

public enum CompanionState
{
    Idle,
    Listening,
    Thinking,
    Speaking,
    NeedsPermission,
    Error
}

public sealed record ProjectStatus(
    string Name,
    string Version,
    bool IsProductionReady)
{
    public static ProjectStatus Current { get; } =
        new("Dev AI Orb", "0.1.0-dev", false);
}
