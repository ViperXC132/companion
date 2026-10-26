# Development roadmap

## Phase 1 — Foundation
- [x] Create the solution and initial project structure.
- [x] Add shared build defaults and ignore rules.
- [x] Add the first WPF desktop shell.
- [x] Add a framework-independent core library and initial unit tests.
- [ ] Add CI build/test workflow and verify on a Windows runner.
- [ ] Review the complete scaffold and resolve any build/test failures.

## Phase 2 — Orb and desktop lifecycle
- Transparent, hollow white orb window with subtle state transitions.
- Drag-to-position and always-on-top behavior.
- Tray menu, startup preferences, and reliable exit path.

## Phase 3 — AI and conversation
- Provider interfaces and explicit configuration.
- Local provider support first; cloud providers opt-in.
- Conversation orchestration, cancellation, and safe error handling.

## Phase 4 — Persistent memory
- Local storage, inspectable entries, and delete/export controls.
- No hidden upload of local memory.

## Phase 5 — Voice
- Voice capture and speech output behind replaceable interfaces.
- Clear listening/speaking indicators and microphone failure handling.

## Phase 6 — Permissioned computer actions
- Explicit capability model and per-action confirmation for sensitive changes.
- Allow-listed operations, audit log, and cancellation.
- No arbitrary command execution without clear user approval.

## Phase 7 — Polish and packaging
- Accessibility, responsiveness, tests, documentation, and release packaging.

A phase is complete only when its acceptance criteria are implemented and the relevant checks have passed.
