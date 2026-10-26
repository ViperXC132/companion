namespace DevAI.Core.Computer;

public enum ComputerRisk
{
    ReadOnly,
    ChangesData,
    Destructive,
    ExternalSideEffect
}

public sealed record ComputerActionRequest(string Id, string Description, ComputerRisk Risk);

public sealed record ComputerActionResult(bool Succeeded, string Message);

public interface IComputerAction
{
    ComputerActionRequest Request { get; }
    Task<ComputerActionResult> ExecuteAsync(CancellationToken cancellationToken = default);
}

public interface IActionApproval
{
    Task<bool> RequestApprovalAsync(ComputerActionRequest request, CancellationToken cancellationToken = default);
}
