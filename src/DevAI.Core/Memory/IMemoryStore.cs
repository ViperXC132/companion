namespace DevAI.Core.Memory;

public sealed record MemoryEntry(Guid Id, string Text, DateTimeOffset CreatedAt);

public interface IMemoryStore
{
    Task<IReadOnlyList<MemoryEntry>> GetRecentAsync(int limit = 50, CancellationToken cancellationToken = default);
    Task AddAsync(string text, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task ClearAsync(CancellationToken cancellationToken = default);
}
