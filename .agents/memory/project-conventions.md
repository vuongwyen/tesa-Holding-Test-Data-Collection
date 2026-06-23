---
type: project
created: 2026-05-25
updated: 2026-05-25
---

# Project Conventions

## Git Workflow
- Always create a new dedicated branch for major code changes.
- Branch name format should follow: `feature/[task-slug]` or `fix/[bug-slug]`.

## C# WinForms Development
- Principles: Apply SOLID principles. Clean C# code.
- Architecture: Separate UI, State Machine, PLC communication, and Database layers. Start with `StateMachine` and `PlcService` (with debounce/checkpoint) before UI.
- Error Handling: Always wrap network/database calls in `try-catch` blocks.
- Logging: Write local text logs for errors since deployment is offline.
