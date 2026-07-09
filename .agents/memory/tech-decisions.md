---
type: project
created: 2026-06-23
updated: 2026-06-23
---

# Technical Decisions

## Project Overview
- App: WinForms application to digitize tape adhesion testing machine.
- Goal: Replace manual recording and Kepware.
- Environment: Windows 10/11 offline (no internet), single laptop connected to PLC Wi-Fi.
- Deployment: Self-Contained or Single File (.exe) without Visual Studio requirement.

## Technology Stack
- Framework: C# WinForms on .NET 10 LTS.
- PLC Communication: `S7netplus` (S7.Net), direct port 102.
- Database: SQLite (Entity Framework Core or Dapper).
- Reporting: `ClosedXML` for Excel export (modifying existing template).

## Architecture & Core Requirements
- PLC Mapping: S7-200 mapping rule in S7.Net (V-Memory = DB1).
- State Inference: Infer state from real-time changes (VD110 -> DB1.DBD110, VD368 -> DB1.DBD368). No native Start/Stop/Done flags.
- State Machine: `IDLE` -> `DEBOUNCE_START` -> `RUNNING` -> `DEBOUNCE_STOP` -> `COMPLETED`.
- Crash Recovery: Checkpoint every 5 seconds (async) during `RUNNING`. Restore `IN_PROGRESS` session on restart.
- Data Processing: Combine PLC test info + UI input (BATCH, NART via barcode) + Timestamp into SQLite when `COMPLETED`.
- Code Architecture: Clear separation of layers: UI, State Machine, PLC Service, Database.

## Implemented Features (Progress)
- **Sanitizer:** `PlcValueSanitizer` handles ghost jumps and double-traps (5s debounce).
- **Architecture:** Broker pattern via `PlcManager` distributes 1 connection to multiple clients.
- **Hook State Logic:** Adaptive State Machine handles IDLE -> RUNNING -> COMPLETED with `IsGood` flag check.
- **Multi-Rack Scaling:** Dynamic UI scaling for up to 3 racks via `PlcConnectionCard` and `RackDashboardView`.
- **Infrastructure:** SQLite WAL mode enabled, native FileLogger added, DGV DoubleBuffered fixed.
