using DevAI.Core.Conversations;

namespace DevAI.Core.Providers;

public interface IChatProvider
{
    string Name { get; }
    Task<ChatResponse> SendAsync(ChatRequest request, CancellationToken cancellationToken = default);
}
