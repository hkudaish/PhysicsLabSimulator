# Godot compatibility experiment

Phase 0 alternative-engine evaluation, **not an approved engine migration or playable science laboratory**. Pinned Godot 4.7.2 .NET, .NET 8 target, Compatibility/OpenGL renderer. The experiment uses the existing Scientific.Core contract and the same native SQLite diagnostic source; it does not implement scientific calculations.

It exercises native Arabic shaping and RTL glyph order, LTR-isolated scientific notation, glyph fallback, logical editing, content scaling, simple lit 3D geometry and SQLite through the engine's C# runtime. Actual keyboard/caret usability, exact curriculum terminology, target-school performance and scientific acceptance still require review.

## Reproduce

From the project root, with the existing .NET tool bootstrap completed:

```sh
python3 tools/bootstrap_godot_spike.py
python3 tools/build_godot_spike.py
python3 tools/run_godot_spike.py --import-only
python3 tools/run_godot_spike.py
python3 tools/build_godot_spike.py --windows
python3 tools/run_godot_spike.py --windows --capture
python3 tools/export_godot_spike.py
python3 tools/run_godot_spike.py --exported --capture
```

Network restore and Windows execution may require authorized execution outside the sandbox. Linux uses isolated .NET SDK 10.0.402; Windows build uses installed SDK 8.0.427. The spike's SDK roll-forward policy is scoped by its own global.json; the platform backend pin remains unchanged. The Godot SDK/packages are version-pinned and restored from publisher-supplied packages; the package graph has a lockfile. `toolchain.json` records archive hashes and the verified Windows editor publisher signature, including the limit that published release checksums could not be retrieved.

Builds for Linux and Windows share the Debug output folder; run them sequentially. Debug and ExportRelease have distinct lockfiles because editor packages and runtime identifiers differ. Windows command environments use WSLENV path translation so profiles/results stay inside artifacts. The export tool checks logs and required managed output because exit code alone can miss a managed export failure. After verifying the export, run `python3 tools/package_godot_spike.py` to create the review ZIP and checksum manifest.

## Preview and evidence

The actual export is `artifacts/godot-spike/windows-export/VirtualStemCompatibility.exe`, with its adjacent PCK, managed/runtime data and licenses. Keep the full folder together. The exported runtime was run from that folder without a source-project path; its own checks assert export-template mode and absence of editor mode. This is a development preview, not a signed commercial release or a school installation qualification.

Results, logs and captured viewports are under `artifacts/godot-spike`. A reviewed evidence snapshot is linked from `docs/phase-0/13-godot-evaluation.md`. Images show the real engine viewport. Simple geometry is a rendering diagnostic, not an experiment. The UI does not report human Arabic approval automatically.

Engine MIT and third-party attribution are generated from the actual engine by `WriteLicenses.gd`; font notices are bundled. Fonts and SQL are copies of the prior diagnostic assets and must stay consistent with their canonical versions.
