# Roadmap

## Phase 1 — Foundation
- [x] .NET 8 WPF solution, core project, CI build/test workflow, and security notes.
- [ ] Continue improving diagnostics and test coverage.

## Phase 2 — Desktop orb
- [x] Transparent hollow orb, idle animation, drag support, context menu, and basic settings.
- [ ] System-tray integration and persisted UI preferences.
- [ ] Verify on a real Windows desktop and polish accessibility.

## Phase 3 — AI and memory
- [x] Conversation domain models and provider interface.
- [x] Local Ollama provider with no cloud fallback and remote-endpoint opt-in.
- [x] Local JSON memory with recent-item limit, deletion, and clear support.
- [x] Chat interface and bounded in-session history.
- [ ] Add provider health/model selection UI, streaming responses, and deeper memory controls.
- [ ] Expand automated tests for provider errors and persistence.

## Phase 4 — Voice
- [x] Windows speech synthesis and one-shot speech recognition service.
- [x] Explicit Listen button and transcription review before sending.
- [x] Optional spoken assistant responses.
- [ ] Add UI cancellation/status refinements and test across Windows speech-language configurations.

## Phase 5 — Computer operation
- [x] Risk classification and approval contract for computer actions.
- [x] Approval-gated runner for non-read-only actions.
- [x] Read-only listing for a user-selected workspace folder.
- [ ] Add narrowly scoped actions with explicit confirmation and path validation.
- [ ] Add integration tests for approval refusal, cancellation, and path boundaries.
- [ ] Do not add unrestricted command execution or autonomous destructive actions.

## Release readiness
- [ ] All CI checks pass on the latest commit.
- [ ] Manual Windows smoke test of orb, chat, memory, microphone, and speech output.
- [ ] Document installation and troubleshooting; package a testable build.
