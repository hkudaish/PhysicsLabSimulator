# Virtual STEM Laboratory

The Windows prototype runs two offline experiments: solution dilution and Ohm’s law. It includes calculated instruments, Arabic/English UI, movable equipment, wiring, undo and SQLite save/resume.

Use [the visual-upgrade Windows ZIP](artifacts/visual-upgrade/VirtualSTEM-Lab-Visual-Windows-x64.zip) for the current build. Extract everything and run **VirtualStemLab.exe**. Forward+ provides the realistic profile; **LowGraphics.bat** launches the lighter OpenGL path. No editor or .NET installation is required. See [the visual-upgrade report](docs/phase-1/visual-upgrade.md).

The [earlier prototype ZIP](artifacts/phase1/VirtualSTEM-Lab-Phase1-Windows-x64.zip), [original instructions](docs/phase-1/run-guide.md) and [original delivery report](docs/phase-1/report.md) remain preserved.

Phase 0 architecture passed and the owner authorized this prototype after reviewing the Godot evaluation. Phase 1 acceptance remains open for independent science review, school-PC qualification and the 60-minute stability test. Phase 2 has not started. Saudi secondary school is the proposed curriculum baseline.

## Development

```sh
python3 tools/dev.py dotnet run --project tests/Science.Reference/Science.Reference.csproj --configuration Release
python3 tools/lab.py build
python3 tools/lab.py import
python3 tools/lab.py verify
python3 tools/lab.py export
python3 tools/lab.py verify --windows --capture --benchmark-seconds 15
python3 tools/lab.py close-check --windows
python3 tools/lab.py verify --windows --resume
python3 tools/lab.py package
python3 tools/lab.py package-check
python3 tools/lab.py verify --windows --compatibility --capture --benchmark-seconds 15
python3 tools/lab.py legacy-check
```

The tooling uses portable Godot 4.7.2 .NET, matching Windows templates and SDK 10.0.402 established in [development setup](docs/phase-0/10-development.md) and [engine evaluation](docs/phase-0/13-godot-evaluation.md). `python3 tools/bootstrap_godot_spike.py --offline` reuses the verified local archives. The client targets .NET 10 and exports its runtime; the console harness also supports Windows SDK 8.0.427 from its own directory.

`src/Scientific.Core` has no renderer dependency; `src/Lab.Persistence` implements atomic native SQLite drafts; `src/GodotClient` is the actual app. Earlier Unity and Godot compatibility spikes remain preserved. Backend and portal directories remain Phase 0 scaffolds.

Original documents are unchanged; [the manifest](docs/phase-0/source-manifest.json) records their hashes. See [the audit](docs/phase-0/01-audit.md), [architecture](docs/phase-0/03-architecture.md), and [gate decision](docs/phase-0/14-gate-decision.md) for source authority and the engine change.

A recoverable repository uses separate metadata because the environment protects `.git`. Use `bash tools/git.sh status` and `bash tools/git.sh log --oneline`. Preserve `artifacts/source-control.git` and `checkpoints/phase0-source.bundle` before clearing artifacts. No remote is configured.
