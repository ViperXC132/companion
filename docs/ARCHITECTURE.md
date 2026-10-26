# Architecture

## Projects

- **DevAI.App** — WPF presentation, chat interface, desktop orb lifecycle, and user-driven composition.
- **DevAI.Core** — framework-independent conversation models, provider and memory interfaces, voice contracts, and computer-action approval contracts.
- **DevAI.Providers** — Ollama HTTP chat provider. Loopback is enforced by default; remote endpoints require explicit opt-in through an environment variable.
- **DevAI.Storage** — JSON memory store in the user's local application-data directory.
- **DevAI.Voice** — Windows System.Speech input and output.
- **DevAI.Computer** — approval-gated action runner and read-only workspace listing.
- **DevAI.Core.Tests** — unit tests for domain behavior.

## Data flow

1. The user submits a message in the WPF window.
2. `ConversationService` builds a bounded conversation context and calls `IChatProvider`.
3. `OllamaChatProvider` sends a non-streaming request to the configured local Ollama endpoint.
4. User and assistant text is persisted by `IMemoryStore` locally.
5. Speech output is optional. Listening starts only when the user presses the Listen button, and transcription is shown for review before submission.
6. Workspace listing is read-only and uses the folder explicitly selected by the user. Non-read-only computer actions require an `IActionApproval` implementation.

## Boundaries and limitations

- No cloud AI provider or automatic cloud fallback is configured.
- Ollama must be installed, running, and have the configured model available.
- Speech recognition depends on Windows language packs, installed engines, and microphone permissions.
- The current computer integration does not run arbitrary shell commands or perform autonomous file edits.
- Memory is local but not encrypted; do not store highly sensitive secrets in it.
- Interfaces are kept replaceable so providers and operating-system integrations can be tested independently.
