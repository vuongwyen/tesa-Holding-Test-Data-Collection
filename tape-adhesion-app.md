# Tape Adhesion Test App - Implementation Plan

## Overview
WinForms application to digitize a tape adhesion testing machine. Replaces manual recording and Kepware by communicating directly with a Siemens S7-200 PLC via Wi-Fi.

## Project Type
WINDOWS DESKTOP (Handled by `backend-specialist` for core logic and `frontend-specialist` for WinForms UI)

## Success Criteria
- [ ] PLC connection is stable with Auto-Reconnect on Wi-Fi drops.
- [ ] State Machine accurately infers states (IDLE, DEBOUNCE_START, RUNNING, DEBOUNCE_STOP, COMPLETED) without native flags.
- [ ] UI is responsive and updates real-time data without freezing.
- [ ] Test results are saved locally to SQLite to prevent data loss.
- [ ] Reports are successfully exported to a provided Excel template using ClosedXML.

## Tech Stack
- **Framework:** C# WinForms (.NET 10 LTS)
- **PLC:** `S7netplus` (direct port 102 to Siemens S7-200, V-Memory mapped to DB1)
- **Database:** SQLite (Entity Framework Core or Dapper)
- **Reporting:** `ClosedXML` (.xlsx export)
- **Architecture:** MVP (Model-View-Presenter) + Tick-based State Machine

## File Structure
```
/src
 ├── /Core
 │    ├── /Models       (PlcData, TestRecord, Constants)
 │    ├── /PLC          (IPlcService, PlcCommunicationService)
 │    ├── /State        (StateMachine, Enums)
 ├── /Data
 │    ├── /Database     (SQLiteDbContext, Repositories)
 │    ├── /Export       (ExcelReportService)
 ├── /UI
 │    ├── /Views        (MainForm, IMainView)
 │    ├── /Presenters   (MainPresenter)
```

## Task Breakdown

### Phase 1: Chuẩn bị và Thiết kế Kiến trúc
- **Task 1.1: Project Setup & Tag Mapping Constants** [x]
  - **Agent:** `backend-specialist` | **Skill:** `api-patterns`
  - **INPUT:** S7-200 V-Memory list
  - **OUTPUT:** `.NET 10 WinForms Project` + `PlcTags.cs` mapping V-Memory to DB1 (e.g., VD110 -> DB1.DBD110).
  - **VERIFY:** Solution compiles, static constants are strictly typed.
- **Task 1.2: MVP Interfaces Design** [x]
  - **Agent:** `backend-specialist` | **Skill:** `architecture`
  - **INPUT:** MVP Architecture requirements
  - **OUTPUT:** `IMainView.cs`, `IPlcService.cs`, `IStateMachine.cs` interfaces.
  - **VERIFY:** Interfaces decouple UI from Logic.

### Phase 2: Xây dựng Lõi kết nối PLC
- **Task 2.1: PlcCommunicationService & Auto-Reconnect** [x]
  - **Agent:** `backend-specialist` | **Skill:** `systematic-debugging`
  - **INPUT:** `IPlcService.cs`, S7netplus NuGet.
  - **OUTPUT:** Implemented `PlcCommunicationService.cs` with Connect/Read/Write and Auto-Reconnect logic in `try-catch`.
  - **VERIFY:** Can instantiate service and handle simulated disconnects without crashing.
- **Task 2.2: Tick-based State Machine** [x]
  - **Agent:** `backend-specialist` | **Skill:** `testing-patterns`
  - **INPUT:** `IStateMachine.cs`, `PlcData` object.
  - **OUTPUT:** `StateMachine.cs` that takes `Tick(PlcData)` every 500ms and processes Debounce_Start/Stop.
  - **VERIFY:** Pass mock `PlcData` and verify state transitions (IDLE -> RUNNING -> COMPLETED).

### Phase 3: Ráp nối Giao diện và Xử lý Nghiệp vụ
- **Task 3.1: MainForm UI Setup & Presenter** [x]
  - **Agent:** `frontend-specialist` | **Skill:** `clean-code`
  - **INPUT:** WinForms Designer, MVP Interfaces.
  - **OUTPUT:** `MainForm.cs` (implements `IMainView`) + `MainPresenter.cs`.
  - **VERIFY:** UI controls update when Presenter pushes mock data.
- **Task 3.2: Polling Loop & Data Sync** [x]
  - **Agent:** `backend-specialist` | **Skill:** `parallel-agents`
  - **INPUT:** `MainPresenter.cs`, `PlcCommunicationService`, `StateMachine`.
  - **OUTPUT:** A background polling loop (using Task/Timer) inside the Presenter/Main that ticks the StateMachine and updates View.
  - **VERIFY:** UI remains responsive while the background loop runs.

### Phase 4: Lưu trữ Cục bộ và Xuất Báo cáo
- **Task 4.1: SQLite Database Integration** [x]
  - **Agent:** `database-architect` | **Skill:** `database-design`
  - **INPUT:** `TestRecord` object.
  - **OUTPUT:** `SQLiteDbContext` and repository to save records when StateMachine reaches `COMPLETED` and Checkpoints every 5s during `RUNNING`.
  - **VERIFY:** `app.db` is created and records are inserted correctly.
- **Task 4.2: ClosedXML Excel Export** [x]
  - **Agent:** `backend-specialist` | **Skill:** `clean-code`
  - **INPUT:** SQLite Test Records, Company Excel Template.
  - **OUTPUT:** `ExcelReportService.cs` that writes data to specific cells in `.xlsx` and saves.
  - **VERIFY:** Generates a valid `.xlsx` file preserving template formatting.

## Phase X: Verification Checklist
- [x] **Build:** Solution builds without warnings.
- [x] **Tests:** Unit tests for `StateMachine` pass (transitions & debounce).
- [x] **Resilience:** App survives network disconnects and reconnects seamlessly.
- [x] **UI:** No cross-thread operation exceptions (InvokeRequired handled).
- [x] **Data:** SQLite file persists data across restarts.
