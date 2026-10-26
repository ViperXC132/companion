using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using DevAI.Core.Conversations;
using DevAI.Core.Providers;

namespace DevAI.Providers;

/// <summary>Supports providers exposing the OpenAI Chat Completions-compatible API.</summary>
public sealed class OpenAiCompatibleChatProvider : IChatProvider, IDisposable
{
    private readonly HttpClient _http;
    private readonly string _model;
    private readonly string _providerName;
    private readonly bool _ownsClient;
    private readonly Uri _endpoint;
    public string Name => _providerName;

    public OpenAiCompatibleChatProvider(string providerName, string endpoint, string model, string apiKey, HttpClient? httpClient = null)
    {
        if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var uri) || (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp))
            throw new ArgumentException("API endpoint must be an absolute HTTP(S) URL.", nameof(endpoint));
        if (string.IsNullOrWhiteSpace(model)) throw new ArgumentException("Model is required.", nameof(model));
        if (string.IsNullOrWhiteSpace(apiKey)) throw new ArgumentException("API key is required.", nameof(apiKey));
        _providerName = string.IsNullOrWhiteSpace(providerName) ? "Custom API" : providerName.Trim();
        _model = model.Trim();
        _http = httpClient ?? new HttpClient { Timeout = TimeSpan.FromMinutes(3) };
        _ownsClient = httpClient is null;
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey.Trim());
        _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _endpoint = uri.AbsoluteUri.EndsWith("/", StringComparison.Ordinal) ? new Uri(uri, "chat/completions") : new Uri(uri.AbsoluteUri + "/chat/completions");
    }

    public async Task<ChatResponse> SendAsync(ChatRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var body = new
        {
            model = request.Model ?? _model,
            messages = request.Messages.Select(m => new { role = m.Role switch { ChatRole.System => "system", ChatRole.User => "user", _ => "assistant" }, content = m.Content }),
            stream = false
        };
        using var response = await _http.PostAsJsonAsync(_endpoint, body, cancellationToken).ConfigureAwait(false);
        var raw = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"{_providerName} returned {(int)response.StatusCode} ({response.ReasonPhrase}). Check the API key, endpoint, model, and account limits.");
        using var doc = JsonDocument.Parse(raw);
        var content = doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
        if (string.IsNullOrWhiteSpace(content)) throw new InvalidOperationException($"{_providerName} returned an empty response.");
        return new ChatResponse(content, _providerName, request.Model ?? _model);
    }

    public void Dispose() { if (_ownsClient) _http.Dispose(); }
}
