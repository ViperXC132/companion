using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using DevAI.Computer;
using DevAI.Core.Conversations;
using DevAI.Core.Memory;
using DevAI.Core.Providers;
using DevAI.Core.Voice;
using DevAI.Providers;
using DevAI.Storage;
using DevAI.Voice;
using Microsoft.Win32;

namespace DevAI.App;

public sealed record ConversationLine(string RoleLabel, string Content);

public partial class MainWindow : Window
{
    private readonly IMemoryStore _memory;
    private readonly ConversationService _conversation;
    private readonly IVoiceService _voice;
    private readonly IChatProvider _provider;
    private bool _isBusy;

    public ObservableCollection<ConversationLine> ConversationLines { get; } = [];

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;
        _memory = new JsonMemoryStore();
        _provider = new OllamaChatProvider();
        _conversation = new ConversationService(_provider, _memory);
        _voice = new SystemSpeechVoiceService();
        ConversationLines.Add(new ConversationLine("DEV AI ORB", "Hi. I'm ready when you are. This build uses Ollama on your own computer; install/start Ollama and pull a model such as llama3.2 to chat."));
    }

    private async void Send_Click(object sender, RoutedEventArgs e) => await SendMessageAsync();

    private async void MessageInput_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && Keyboard.Modifiers != ModifierKeys.Shift)
        {
            e.Handled = true;
            await SendMessageAsync();
        }
    }

    private async Task SendMessageAsync()
    {
        if (_isBusy) return;
        var text = MessageInput.Text.Trim();
        if (text.Length == 0) return;

        MessageInput.Clear();
        ConversationLines.Add(new ConversationLine("YOU", text));
        ConversationList.ScrollIntoView(ConversationLines[^1]);
        SetBusy(true, "Thinking · local AI");
        try
        {
            var response = await _conversation.SendAsync(text);
            ConversationLines.Add(new ConversationLine($"{response.Provider} · {response.Model}", response.Content));
            ConversationList.ScrollIntoView(ConversationLines[^1]);
            StatusText.Text = "Ready · Response received";
            if (SpeakResponsesCheckBox.IsChecked == true)
                await _voice.SpeakAsync(response.Content);
        }
        catch (Exception ex)
        {
            ConversationLines.Add(new ConversationLine("NOTICE", FriendlyError(ex)));
            ConversationList.ScrollIntoView(ConversationLines[^1]);
            StatusText.Text = "Needs attention";
        }
        finally { SetBusy(false, StatusText.Text); }
    }

    private async void Listen_Click(object sender, RoutedEventArgs e)
    {
        if (_isBusy) return;
        SetBusy(true, "Listening · microphone active");
        try
        {
            var transcript = await _voice.ListenOnceAsync();
            if (!string.IsNullOrWhiteSpace(transcript))
            {
                MessageInput.Text = transcript;
                MessageInput.Focus();
                MessageInput.CaretIndex = MessageInput.Text.Length;
                StatusText.Text = "Speech transcribed · review before sending";
            }
            else StatusText.Text = "No speech recognized";
        }
        catch (Exception ex)
        {
            ConversationLines.Add(new ConversationLine("VOICE NOTICE", ex.Message));
            StatusText.Text = "Voice unavailable";
        }
        finally { SetBusy(false, StatusText.Text); }
    }

    private async void ClearMemory_Click(object sender, RoutedEventArgs e)
    {
        var answer = MessageBox.Show(this,
            "Permanently delete all locally stored memory for Dev AI Orb? This cannot be undone.",
            "Clear local memory", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (answer != MessageBoxResult.Yes) return;
        try
        {
            await _memory.ClearAsync();
            _conversation.ClearConversation();
            ConversationLines.Clear();
            ConversationLines.Add(new ConversationLine("DEV AI ORB", "Local memory cleared."));
            StatusText.Text = "Local memory cleared";
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Could not clear memory", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void Workspace_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog { Title = "Choose a workspace to inspect", Multiselect = false };
        if (dialog.ShowDialog(this) != true) return;

        try
        {
            var action = new ListFilesAction(dialog.FolderName);
            var result = await action.ExecuteAsync();
            ConversationLines.Add(new ConversationLine("WORKSPACE · READ ONLY",
                result.Succeeded ? (string.IsNullOrWhiteSpace(result.Message) ? "(Folder is empty)" : result.Message) : result.Message));
            ConversationList.ScrollIntoView(ConversationLines[^1]);
        }
        catch (Exception ex)
        {
            ConversationLines.Add(new ConversationLine("WORKSPACE NOTICE", ex.Message));
        }
    }

    private void SetBusy(bool busy, string status)
    {
        _isBusy = busy;
        MessageInput.IsEnabled = !busy;
        StatusText.Text = status;
    }

    private static string FriendlyError(Exception ex)
    {
        if (ex is HttpRequestException)
            return "Couldn't reach the local AI service. Make sure Ollama is installed and running at http://127.0.0.1:11434, then pull your configured model (default: llama3.2). No cloud fallback is enabled.";
        return ex.Message;
    }

    protected override void OnClosed(EventArgs e)
    {
        if (_provider is IDisposable disposableProvider) disposableProvider.Dispose();
        if (_voice is IDisposable disposableVoice) disposableVoice.Dispose();
        base.OnClosed(e);
    }
}
