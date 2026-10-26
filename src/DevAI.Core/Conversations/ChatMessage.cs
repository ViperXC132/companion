namespace DevAI.Core.Conversations;

public enum ChatRole
{
    System,
    User,
    Assistant
}

public sealed record ChatMessage(ChatRole Role, string Content, DateTimeOffset CreatedAt)
{
    public static ChatMessage User(string content) =>
        new(ChatRole.User, content, DateTimeOffset.UtcNow);

    public static ChatMessage Assistant(string content) =>
        new(ChatRole.Assistant, content, DateTimeOffset.UtcNow);
}

public sealed record ChatRequest(IReadOnlyList<ChatMessage> Messages, string? Model = null);

public sealed record ChatResponse(string Content, string Provider, string Model);
