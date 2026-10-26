using DevAI.Core.Computer;

namespace DevAI.Computer;

/// <summary>Runs only explicitly supplied actions and requires approval for every non-read-only action.</summary>
public sealed class ComputerActionRunner(IActionApproval approval)
{
    private readonly IActionApproval _approval = approval ?? throw new ArgumentNullException(nameof(approval));

    public async Task<ComputerActionResult> RunAsync(IComputerAction action, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(action);
        if (action.Request.Risk != ComputerRisk.ReadOnly &&
            !await _approval.RequestApprovalAsync(action.Request, cancellationToken).ConfigureAwait(false))
            return new ComputerActionResult(false, "Action cancelled: approval was not granted.");

        cancellationToken.ThrowIfCancellationRequested();
        return await action.ExecuteAsync(cancellationToken).ConfigureAwait(false);
    }
}

/// <summary>Read-only file listing constrained to a configured root directory.</summary>
public sealed class ListFilesAction : IComputerAction
{
    private readonly string _root;
    private readonly string _relativePath;

    public ComputerActionRequest Request { get; } =
        new("files.list", "List files in the configured workspace (read-only)", ComputerRisk.ReadOnly);

    public ListFilesAction(string rootDirectory, string relativePath = ".")
    {
        _root = Path.GetFullPath(rootDirectory ?? throw new ArgumentNullException(nameof(rootDirectory)));
        _relativePath = relativePath ?? ".";
    }

    public Task<ComputerActionResult> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var target = Path.GetFullPath(Path.Combine(_root, _relativePath));
        var prefix = _root.EndsWith(Path.DirectorySeparatorChar) ? _root : _root + Path.DirectorySeparatorChar;
        if (!target.Equals(_root, StringComparison.OrdinalIgnoreCase) &&
            !target.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            return Task.FromResult(new ComputerActionResult(false, "Blocked: path is outside the configured workspace."));
        if (!Directory.Exists(target))
            return Task.FromResult(new ComputerActionResult(false, "Workspace directory does not exist."));
        var entries = Directory.EnumerateFileSystemEntries(target)
            .Take(200)
            .Select(Path.GetFileName)
            .Where(x => x is not null);
        return Task.FromResult(new ComputerActionResult(true, string.Join(Environment.NewLine, entries)));
    }
}
