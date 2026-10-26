# Security principles

- Do not commit API keys, tokens, credentials, or local configuration secrets.
- Keep memory local by default; do not silently upload it.
- Make provider/network use explicit in settings.
- Ask before destructive actions, sending messages, changing system settings, or running commands.
- Limit computer actions to explicit capabilities and validate all paths and arguments.
- Log action outcomes without logging secrets or unnecessary private content.
- Provide a clear way to cancel active work and disable microphone access.
- Treat model output and retrieved content as untrusted input, not as permission to execute actions.
