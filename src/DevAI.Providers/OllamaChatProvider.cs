using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using DevAI.Core.Conversations;
using DevAI.Core.Providers;

namespace DevAI.Providers;

/// <summary>Connects to an explicitly configured local Ollama server. No cloud endpoint is used.</summary>
public sealed class OllamaChatProvider : IChatProvider, IDisposable
{
    private readonly HttpClient _http;
    private readonly Uri _endpoint;
    private readonly string _defaultModel;
    private readonly bool _ownsClient;

    public string Name => "Ollama (local)";

    public OllamaChatProvider(string endpoint = "http://127.0.0.1:11434/", string model = "llama3.2", HttpClient? httpClient = null)
    {
        if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var uri) || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            throw new ArgumentException("Endpoint must be an absolute HTTP(S) URI.", nameof(endpoint));
        if (uri.IsLoopback is false && !string.Equals(Environment.GetEnvironmentVariable("DEVAI_ALLOW_REMOTE_OLLAMA"), "1", StringComparison.Ordinal))
            throw new ArgumentException("Remote endpoints are disabled by default. Set DEVAI_ALLOW_REMOTE_OLLAMA=1 only if you intentionally trust that server.", nameof(endpoint));
        _endpoint = new Uri(uri, "api/chat");
        _defaultModel = string.IsNullOrWhiteSpace(model) ? throw new ArgumentException("Model is required.", nameof(model)) : model.Trim();
        _http = httpClient ?? new HttpClient { Timeout = TimeSpan.FromMinutes(3) };
        _ownsClient = httpClient is null;
    }

    public async Task<ChatResponse> SendAsync(ChatRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.Messages.Count == 0) throw new ArgumentException("At least one message is required.", nameof(request));
        var body = new
        {
            model = request.Model ?? _defaultModel,
            stream = false,
            messages = request.Messages.Select(m => new { role = m.Role switch { ChatRole.System => "system", ChatRole.User => "user", _ => "assistant" }, content = m.Content })
        };
        using var response = await _http.PostAsJsonAsync(_endpoint, body, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        using var document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken).ConfigureAwait(false);
        var content = document.RootElement.GetProperty("message").GetProperty("content").GetString();
        var model = document.RootElement.TryGetProperty("model", out var modelValue) ? modelValue.GetString() ?? request.Model ?? _defaultModel : request.Model ?? _defaultModel;
        if (string.IsNullOrWhiteSpace(content)) throw new InvalidOperationException("The local AI provider returned an empty response.");
        return new ChatResponse(content, Name, model);
    }

    public void Dispose() { if (_ownsClient) _http.Dispose(); }
}
