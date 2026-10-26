# Architecture

## Projects

- **DevAI.App** — Windows-only WPF presentation and desktop lifecycle.
- **DevAI.Core** — framework-independent domain types and orchestration contracts.
- **DevAI.Core.Tests** — fast unit tests for core behavior.

Additional projects for providers, voice, storage, and computer actions will be introduced when their boundaries are implemented, rather than adding empty placeholder projects.

## Principles

1. Keep UI code separate from AI, storage, and operating-system integrations.
2. Use interfaces at external boundaries so implementations can be replaced and tested.
3. Keep provider choice and network behavior visible to the user.
4. Make cancellation and failures first-class outcomes.
5. Never represent a planned or stubbed feature as implemented.
