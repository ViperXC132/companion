namespace DevAI.Core.Voice;

public interface IVoiceService
{
    bool IsAvailable { get; }
    Task SpeakAsync(string text, CancellationToken cancellationToken = default);
    Task<string?> ListenOnceAsync(CancellationToken cancellationToken = default);
    void Cancel();
}
