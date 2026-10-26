using System.Text.Json;
using DevAI.Core.Memory;

namespace DevAI.Storage;

/// <summary>Stores memory in the user's local application-data folder; never uploads it.</summary>
public sealed class JsonMemoryStore : IMemoryStore
{
    private readonly string _path;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public JsonMemoryStore(string? path = null)
    {
        _path = path ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DevAIOrb", "memory.json");
    }

    public async Task<IReadOnlyList<MemoryEntry>> GetRecentAsync(int limit = 50, CancellationToken cancellationToken = default)
    {
        if (limit < 1) throw new ArgumentOutOfRangeException(nameof(limit));
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try { return (await LoadAsync(cancellationToken).ConfigureAwait(false)).TakeLast(limit).ToArray(); }
        finally { _gate.Release(); }
    }

    public async Task AddAsync(string text, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text)) throw new ArgumentException("Memory text cannot be empty.", nameof(text));
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var items = await LoadAsync(cancellationToken).ConfigureAwait(false);
            items.Add(new MemoryEntry(Guid.NewGuid(), text.Trim(), DateTimeOffset.UtcNow));
            if (items.Count > 2000) items.RemoveRange(0, items.Count - 2000);
            await SaveAsync(items, cancellationToken).ConfigureAwait(false);
        }
        finally { _gate.Release(); }
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var items = await LoadAsync(cancellationToken).ConfigureAwait(false);
            items.RemoveAll(x => x.Id == id);
            await SaveAsync(items, cancellationToken).ConfigureAwait(false);
        }
        finally { _gate.Release(); }
    }

    public async Task ClearAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try { await SaveAsync([], cancellationToken).ConfigureAwait(false); }
        finally { _gate.Release(); }
    }

    private async Task<List<MemoryEntry>> LoadAsync(CancellationToken ct)
    {
        if (!File.Exists(_path)) return [];
        await using var stream = new FileStream(_path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);
        return await JsonSerializer.DeserializeAsync<List<MemoryEntry>>(stream, JsonOptions, ct).ConfigureAwait(false) ?? [];
    }

    private async Task SaveAsync(List<MemoryEntry> items, CancellationToken ct)
    {
        var directory = Path.GetDirectoryName(_path);
        if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
        var tempPath = _path + ".tmp";
        await using (var stream = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true))
            await JsonSerializer.SerializeAsync(stream, items, JsonOptions, ct).ConfigureAwait(false);
        File.Move(tempPath, _path, overwrite: true);
    }
}
