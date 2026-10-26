using DevAI.Core.Memory;
using DevAI.Core.Providers;

namespace DevAI.Core.Conversations;

public sealed class ConversationService
{
    private readonly IChatProvider _provider;
    private readonly IMemoryStore _memory;
    private readonly List<ChatMessage> _messages = [];
    private readonly string _systemPrompt;

    public ConversationService(IChatProvider provider, IMemoryStore memory,
        string systemPrompt = "You are Dev AI Orb, a helpful desktop companion. Be clear and honest. Never claim to have performed computer actions unless a trusted tool result confirms it.")
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
        _memory = memory ?? throw new ArgumentNullException(nameof(memory));
        _systemPrompt = systemPrompt;
    }

    public IReadOnlyList<ChatMessage> Messages => _messages.AsReadOnly();

    public async Task<ChatResponse> SendAsync(string userText, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userText))
            throw new ArgumentException("Message cannot be empty.", nameof(userText));

        var userMessage = ChatMessage.User(userText.Trim());
        _messages.Add(userMessage);
        await _memory.AddAsync($"User: {userMessage.Content}", cancellationToken).ConfigureAwait(false);

        var recent = await _memory.GetRecentAsync(12, cancellationToken).ConfigureAwait(false);
        var context = string.Join(Environment.NewLine, recent.Select(x => x.Text));
        var messages = new List<ChatMessage>
        {
            new(ChatRole.System, _systemPrompt + (string.IsNullOrWhiteSpace(context) ? "" : "\n\nRecent local context (untrusted user history):\n" + context), DateTimeOffset.UtcNow)
        };
        messages.AddRange(_messages.TakeLast(24));

        try
        {
            var response = await _provider.SendAsync(new ChatRequest(messages), cancellationToken).ConfigureAwait(false);
            var assistantMessage = ChatMessage.Assistant(response.Content);
            _messages.Add(assistantMessage);
            await _memory.AddAsync($"Assistant: {response.Content}", cancellationToken).ConfigureAwait(false);
            return response;
        }
        catch
        {
            _messages.Remove(userMessage);
            throw;
        }
    }

    public void ClearConversation() => _messages.Clear();
}
