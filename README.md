# Dev AI Orb

A Windows desktop AI companion built with C# and .NET 8 WPF.

> Early development software. Review the security notes and verify functionality on your own Windows machine before relying on it.

## Current capabilities

- Minimal hollow white desktop orb with subtle idle animation, drag support, and context menu.
- Chat UI connected to a local Ollama endpoint by default (`http://127.0.0.1:11434`).
- JSON-backed persistent memory stored under the current user's Local Application Data directory.
- Optional Windows speech input and output. The microphone is accessed only after pressing **Listen**; recognized text is shown for review before sending.
- Read-only workspace listing, constrained to the folder selected by the user.
- A permission-gated action runner for future state-changing computer operations.

## Download the Windows app

Every push to `main` and every manual run of the **Build and package Dev AI Orb** workflow restores, builds, tests, and publishes a self-contained Windows x64 application. The packaged output is uploaded as a GitHub Actions artifact named **DevAI-Orb-win-x64** (retained for 30 days).

1. Open the repository's **Actions** tab.
2. Select **Build and package Dev AI Orb**.
3. Open the latest successful run.
4. Under **Artifacts**, download `DevAI-Orb-win-x64` and extract the ZIP.
5. Run `DevAI.Orb.exe` on Windows 10/11.

The artifact is a self-contained .NET publish, so the .NET runtime does not need to be installed separately. **Ollama is still a separate dependency** for AI chat and must be installed/running with a model such as `llama3.2`. Voice features depend on Windows speech support and microphone permissions.

## Requirements for developing from source

- Windows 10/11
- .NET 8 SDK
- [Ollama](https://ollama.com/) installed and running for AI chat; pull a model such as `llama3.2` first.
- Windows speech recognition language/engine and microphone permission for voice input.

## Build and test locally

```powershell
dotnet restore DevAI.sln
dotnet build DevAI.sln --configuration Release
dotnet test DevAI.sln --configuration Release
dotnet publish src/DevAI.App/DevAI.App.csproj --configuration Release --runtime win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
```

Open `DevAI.sln` in Visual Studio 2022 or use the commands above on Windows.

## Privacy and safety

- AI requests go to the configured Ollama server. By default the provider accepts loopback endpoints only and has no cloud fallback.
- Conversation and memory content is stored locally. Use **Clear memory** in the chat window to delete stored memory.
- Voice capture is opt-in per press of **Listen**. Transcription is not sent until the user presses **Send**.
- Workspace inspection is read-only. No arbitrary shell command execution is exposed.
- Future state-changing or external actions must go through the approval interface. Model output is not itself permission to act.
- Do not commit API keys or secrets.

See [Architecture](docs/ARCHITECTURE.md), [Roadmap](docs/ROADMAP.md), and [Security](docs/SECURITY.md).
