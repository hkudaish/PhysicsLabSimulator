# Phase 1 prototype delivery report

This report preserves the initial 0.1 prototype delivery. See [the visual-upgrade report](visual-upgrade.md) for the current 0.2 build, realistic rendering, graphics fallback and updated measurements.

**Phase:** Functional 3D Scientific Simulation Proof of Concept.

**Status:** Conditionally Passed for development-prototype delivery; Phase 1 acceptance remains open.

**Decision:** Do not start Phase 2 until independent science review, representative-school Windows qualification, the 60-minute stability test and owner acceptance are complete.

## Scope completed and deliverables

The Windows app implements ideal solution dilution and resistive DC/Ohm’s law in an interactive 3D bench. It includes procedural glassware/devices, lighting, selection, validated dragging, rotation, orbit/zoom, equipment placement/removal, wires, concentration/voltage/current instruments, goal feedback, reset and 20-step undo per experiment. Quantities drive liquid height/tint, meter readings and circuit visual state. Arabic/English layouts use bundled fonts and LTR isolates for quantities and equations. Low/high presets change antialiasing only.

SQLite atomically stores both benches, model/schema/client versions, tools, poses, topology, instrument settings, quantities, camera, action history and undo snapshots. Measurements/progress are deterministically recomputed from saved state using its model versions. Startup restores the draft; closing the actual Windows window waits for saving. Invalid or newer drafts do not replace current work.

- [Portable Windows ZIP](../../artifacts/phase1/VirtualSTEM-Lab-Phase1-Windows-x64.zip): 78,797,464 bytes (75.1 MiB).
- SHA-256: `b358f8aa9329ae9bafb8e030bb6f6a917f88c8be58956c0af61bc2606dde8683`.
- [Run/demonstration instructions](run-guide.md), [scientific validation](scientific-validation.md), [file checksum manifest](evidence/package-manifest.json).
- [Dilution screenshot](evidence/windows-chemistry-lab.png), [electrical screenshot](evidence/windows-electricity-lab.png).

## Files created or modified

`src/Scientific.Core/Dilution.cs` and `ResistiveDc.cs` implement the models. `src/Lab.Persistence` contains native SQLite drafts. `src/GodotClient` is the actual application, with pinned SDK/dependency locks and Windows export settings. `tests/Science.Reference` supplies independent analytical references and persistence validation. `tools/lab.py` and `check_lab_window_close.ps1` build, export, exercise and package it. README, RTM, roadmap, CI and this report reflect the implemented prototype. Earlier source documents and compatibility spikes remain preserved.

## Scientific, integration and persistence tests

| Check | Observed result | Evidence |
|---|---|---|
| Science/reference/native persistence on Linux | 452 assertions passed | [Linux reference](evidence/scientific-reference-linux.json) |
| Same harness on Windows .NET host | 452 assertions passed; build zero warnings/errors | [Windows reference](evidence/scientific-reference-windows.json) |
| Actual Godot Linux application | 44 integration checks passed | [Linux integration](evidence/linux-integration-result.json) |
| Actual exported Windows app, real OpenGL renderer | 52 checks passed, including capture and two graphics benchmarks | [Windows integration](evidence/windows-integration-result.json) |
| Actual window close with unsaved action | Owned test window received WM_CLOSE and exited successfully after saving | [Window-close evidence](evidence/windows-window-close-result.json) |
| Fresh standalone process after window-close autosave | 10 checks passed; exact saved workspace hash, topology, amounts and undo restored | [Fresh-process evidence](evidence/windows-resume-result.json) |
| ZIP integrity and separately extracted standalone application | CRC passed; 10 restart checks passed | [Package smoke](evidence/package-smoke-result.json) |
| Original-source/design regression | 19 checks passed | `python3 -m unittest discover -s tests/phase0 -v` |

The engine checks invoke real GUI controls and physics ray selection, including all six electrical terminals. They exercise live V/R controls, parallel voltmeter and series ammeter, open/short recovery, rejected transfers, probe proximity, 20 manipulation/undo cycles, language layouts, full save/reload, malformed/newer saves and graphics independence. All verification uses dedicated test storage. No human classroom usability or independent scientific acceptance is inferred from automated checks.

## Performance results

Development host: Windows 11 Pro 10.0.26300, Intel i5-14600KF (20 logical processors), 34,160,291,840 bytes RAM, NVIDIA RTX 4060 Ti, driver 32.0.16.1088. This is **not an approved school minimum**. [Hardware record](evidence/development-hardware.json), [raw measurements](evidence/windows-benchmark-result.json).

| Preset | Sampling duration | Mean FPS | 95th percentile frame time | Private memory start → end |
|---|---:|---:|---:|---:|
| Low, no MSAA | 15.003 s | 143.97 | 6.937 ms | 354.4 → 337.5 MiB |
| High, 4× MSAA | 15.006 s | 145.01 | 6.937 ms | 337.5 → 337.7 MiB |

A single low-preset frame took 129.036 ms in the final sample; the mean and percentile do not imply stall-free rendering. Each preset has a two-second warmup. Scene: three devices and a closed series-ammeter circuit; rendered 3D viewport 1040×768 within a 1440×960 UI. This is a short development-PC smoke, not a worst-case load, leak certification, school benchmark or hour-long stability result. A long run can be requested with `python3 tools/lab.py verify --windows --capture --benchmark-seconds 1800` (30 minutes per preset), but acceptance also requires approved hardware, interactive use and retained monitoring evidence.

## Security results and limitations

The prototype has no identities, credentials, remote requests, grade authority or cloud synchronization. Drafts use bound SQLite parameters, explicit UTF-8, WAL/full synchronization and version/size checks. Windows SQLite loads from System32. Tests cover quoted Arabic data and non-destructive rejection of corrupt/newer snapshots. The database is not encrypted or tamper-proof; sensitive student data and institutional features remain out of scope. This portable development build is unsigned. These checks are not a penetration test or privacy-compliance assessment.

## Known defects, unfinished requirements and dependencies

No blocking functional defect was observed in the covered automated flows. An earlier repeat benchmark detected a workspace change; automated modes now isolate external mouse/keyboard input, and the final rerun passed workspace invariance. The [rejected run](evidence/benchmark-rejected-run.json) is retained; its performance figures are not used. Remaining acceptance work:

1. A named scientific reviewer must approve model assumptions, fixture tolerances and proposed Saudi lesson mapping.
2. School IT must identify representative minimum Windows hardware and verify launch, controls and at least stable 30 FPS there.
3. Complete the 60-minute interactive stability/memory/save-recovery protocol on that hardware. The current 30-second sample cannot establish leak-free hour-long operation.
4. Obtain human Arabic editing/input/accessibility and classroom usability review. Equipment is schematic procedural geometry; production lab art and broader input-device qualification remain unfinished.

Current model limits include ideal additive volumes and one nonreactive solute; ideal static DC; no reaction/thermal/AC/semiconductor behavior. Floating and redundant ideal-source configurations can be unsolvable and receive explicit diagnostics. Local practice snapshots are not the Phase 2 student-submission journal. Accounts, grading, portals, licensing and synchronization have not been implemented during this phase.

**Recommended next steps:** run the delivered executable with a reviewer and school IT, retain hardware/hour-long evidence, remediate findings, then request the owner’s Phase 1 acceptance and permission for Phase 2. The present report does not authorize the next phase.
