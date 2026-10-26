using System.Globalization;
using System.Speech.Recognition;
using System.Speech.Synthesis;
using DevAI.Core.Voice;

namespace DevAI.Voice;

/// <summary>Uses Windows installed speech engines. Microphone capture begins only when ListenOnceAsync is called.</summary>
public sealed class SystemSpeechVoiceService : IVoiceService, IDisposable
{
    private readonly SpeechSynthesizer _synthesizer = new();
    private readonly object _sync = new();
    private SpeechRecognitionEngine? _recognizer;
    private TaskCompletionSource<string?>? _listenCompletion;
    private bool _disposed;

    public bool IsAvailable => OperatingSystem.IsWindows();

    public Task SpeakAsync(string text, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (string.IsNullOrWhiteSpace(text)) return Task.CompletedTask;
        cancellationToken.ThrowIfCancellationRequested();
        return Task.Run(() =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            _synthesizer.Speak(text);
        }, cancellationToken);
    }

    public async Task<string?> ListenOnceAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        cancellationToken.ThrowIfCancellationRequested();
        TaskCompletionSource<string?> completion;
        lock (_sync)
        {
            if (_listenCompletion is not null) throw new InvalidOperationException("Voice capture is already active.");
            completion = new TaskCompletionSource<string?>(TaskCreationOptions.RunContinuationsAsynchronously);
            _listenCompletion = completion;
            try
            {
                _recognizer = new SpeechRecognitionEngine(CultureInfo.CurrentUICulture);
                _recognizer.LoadGrammar(new DictationGrammar());
                _recognizer.SetInputToDefaultAudioDevice();
                _recognizer.SpeechRecognized += (_, e) => { if (e.Result.Confidence >= 0.55) completion.TrySetResult(e.Result.Text); };
                _recognizer.RecognizeCompleted += (_, e) => completion.TrySetResult(e.Result?.Text);
                _recognizer.RecognizeAsync(RecognizeMode.Single);
            }
            catch (Exception ex)
            {
                _recognizer?.Dispose();
                _recognizer = null;
                _listenCompletion = null;
                throw new InvalidOperationException("Windows speech recognition could not start. Check microphone permissions and installed speech languages.", ex);
            }
        }

        using var registration = cancellationToken.Register(() => completion.TrySetCanceled(cancellationToken));
        try { return await completion.Task.ConfigureAwait(false); }
        finally
        {
            lock (_sync)
            {
                try { _recognizer?.RecognizeAsyncCancel(); } catch (InvalidOperationException) { }
                _recognizer?.Dispose();
                _recognizer = null;
                _listenCompletion = null;
            }
        }
    }

    public void Cancel()
    {
        lock (_sync)
        {
            _listenCompletion?.TrySetCanceled();
            try { _recognizer?.RecognizeAsyncCancel(); } catch (InvalidOperationException) { }
            _synthesizer.SpeakAsyncCancelAll();
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        Cancel();
        _recognizer?.Dispose();
        _synthesizer.Dispose();
        _disposed = true;
    }
}
