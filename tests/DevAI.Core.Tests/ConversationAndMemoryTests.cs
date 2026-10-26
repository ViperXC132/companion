using DevAI.Core.Conversations;
using DevAI.Core.Memory;
using DevAI.Core.Providers;
using DevAI.Storage;
using Xunit;

namespace DevAI.Core.Tests;

public sealed class ConversationAndMemoryTests
{
    [Fact]
    public async Task JsonMemoryStore_PersistsAndDeletesEntries()
    {
        var directory = Path.Combine(Path.GetTempPath(), "DevAIOrbTests", Guid.NewGuid().ToString("N"));
        var path = Path.Combine(directory, "memory.json");
        try
        {
            var store = new JsonMemoryStore(path);
            await store.AddAsync("first");
            await store.AddAsync("second");
            var recent = await store.GetRecentAsync(1);
            Assert.Single(recent);
            Assert.Equal("second", recent[0].Text);

            await store.DeleteAsync(recent[0].Id);
            var remaining = await store.GetRecentAsync();
            Assert.Single(remaining);
            Assert.Equal("first", remaining[0].Text);

            await store.ClearAsync();
            Assert.Empty(await store.GetRecentAsync());
        }
        finally
        {
            if (Directory.Exists(directory)) Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public async Task ConversationService_UsesProviderAndStoresAssistantReply()
    {
        var memory = new InMemoryMemoryStore();
        var provider = new FakeChatProvider("Hello from test");
        var conversation = new ConversationService(provider, memory);

        var response = await conversation.SendAsync("hello");

        Assert.Equal("Hello from test", response.Content);
        Assert.Equal(2, conversation.Messages.Count);
        Assert.Equal(ChatRole.User, conversation.Messages[0].Role);
        Assert.Equal(ChatRole.Assistant, conversation.Messages[1].Role);
        Assert.Equal(2, (await memory.GetRecentAsync()).Count);
        Assert.Equal("hello", provider.LastRequest!.Messages.Last().Content);
    }

    [Fact]
    public async Task ConversationService_RejectsBlankInput()
    {
        var conversation = new ConversationService(new FakeChatProvider("unused"), new InMemoryMemoryStore());
        await Assert.ThrowsAsync<ArgumentException>(() => conversation.SendAsync("  "));
    }

    private sealed class FakeChatProvider(string reply) : IChatProvider
    {
        public string Name => "test";
        public ChatRequest? LastRequest { get; private set; }

        public Task<ChatResponse> SendAsync(ChatRequest request, CancellationToken cancellationToken = default)
        {
            LastRequest = request;
            return Task.FromResult(new ChatResponse(reply, Name, "fake"));
        }
    }

    private sealed class InMemoryMemoryStore : IMemoryStore
    {
        private readonly List<MemoryEntry> _entries = [];

        public Task<IReadOnlyList<MemoryEntry>> GetRecentAsync(int limit = 50, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<MemoryEntry>>(_entries.TakeLast(limit).ToArray());

        public Task AddAsync(string text, CancellationToken cancellationToken = default)
        {
            _entries.Add(new MemoryEntry(Guid.NewGuid(), text, DateTimeOffset.UtcNow));
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            _entries.RemoveAll(entry => entry.Id == id);
            return Task.CompletedTask;
        }

        public Task ClearAsync(CancellationToken cancellationToken = default)
        {
            _entries.Clear();
            return Task.CompletedTask;
        }
    }
}
