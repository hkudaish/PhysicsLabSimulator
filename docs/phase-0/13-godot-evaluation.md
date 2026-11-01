# Godot alternative-engine evaluation

Date: 2026-11-01. Scope authorized: a Phase 0 compatibility spike after the owner asked for a Unity alternative. This is an engine evaluation, not Phase 1 laboratory implementation. Unity remains the documented baseline until the owner accepts a revision.

## Recommendation

**Adopt Godot 4.7.2 .NET as the proposed Windows-client engine**, subject to owner acceptance of this architecture change and the remaining school/scientific reviews. This removes the unavailable Unity editor/activation dependency for the evaluated path. Godot permits commercial use under its MIT terms; engine/font notices are included in the preview. [Godot licensing](https://docs.godotengine.org/en/stable/about/complying_with_licenses.html)

The evaluated application builds with C#, loads the existing engine-independent Scientific.Core contract, performs native Arabic shaping and mixed-direction text processing, renders lit 3D geometry, executes SQLite through its C# runtime, and exports to a standalone Windows application. This supports the initial desktop scope. Godot C# web export is not currently supported by its documented stable workflow; any future browser product needs a separate design. [C# support and limitations](https://docs.godotengine.org/en/stable/tutorials/scripting/c_sharp/c_sharp_basics.html)

## Deliverables

* Source: [GodotCompatibility](../../spikes/GodotCompatibility/README.md).
* Standalone preview: `artifacts/godot-spike/VirtualSTEM-Godot-Compatibility-Windows-x64.zip` after packaging. Extract the full folder and run `VirtualStemCompatibility.exe`; keep its PCK, runtime data and licenses together.
* Results and screenshot: `docs/phase-0/godot-evidence/` contains a preserved review snapshot; live logs/results remain under artifacts.
* Reproduction: portable-tool bootstrap, platform build/run, export and packaging tools under `tools/`.

No account activation, paid engine plugin, source-project access or editor process is needed to run the exported preview. Its managed runtime files are packaged. A clean school computer has not yet been qualified.

## Evidence and limits

| Check | Observed result | What it does not establish |
|---|---|---|
| C# builds | Linux SDK 10.0.402 and native Windows SDK 8.0.427, zero warnings/errors | Main backend remains SDK 10; Unity scripts not validated |
| Linux engine run | 14 automated compatibility checks passed | Headless run is not a rendered UI test |
| Windows engine run | 16 checks with a captured real viewport | Full keyboard/caret/usability or linguistic review |
| Standalone Windows export | 17 checks passed from the output folder, export-template mode true and editor mode false | Signed production installer or clean-machine rollout |
| Arabic shaping | Contextual glyph substitutions and descending visual cluster positions | All Arabic typography or future text inputs |
| Scientific formula | LTR visual cluster order preserved inside an isolated RTL paragraph | All mathematical expression layouts |
| Font independence | Bundled Tajawal plus DejaVu fallback, system fallback disabled | Font rendering on every graphics stack |
| SQLite inside Godot | 10 shared native checks passed per runtime | Production persistence/encryption/assessment trust |
| 3D output | Lit diagnostic geometry rendered, nonuniform viewport pixels captured | Laboratory asset fidelity or school-PC frame budget |
| Resources | Shared font/SQL copies and configuration-specific package locks checked | Full content packaging/update governance |

The Windows render used OpenGL 3.3 compatibility on an NVIDIA GeForce RTX 4060 Ti host. This is **not** an approved minimum school PC or performance benchmark. Arabic text joining, right-aligned layout, formula order and fallback glyphs were visually inspected in the actual viewport capture. Human language/usability signoff remains pending and is not auto-approved by the application.

The native SQLite spike source is shared with the earlier standalone tests. The engine currently loads that source by project reference/link for evaluation; a production persistence module has not been designed or approved. Scientific.Core currently contains a contract, not implemented chemistry/electrical solvers.

## Reproducibility decisions

Godot 4.7.2 .NET editors and matching export templates are pinned in `spikes/GodotCompatibility/toolchain.json`. Official HTTPS downloads were used; the Windows editor's Authenticode signature verified to Prehensile Tales B.V. Published release checksum retrieval was unavailable, so the recorded SHA-256 values are local reproducibility pins, not independent publisher-digest verification.

Debug requires GodotSharpEditor while ExportRelease does not and targets win-x64. These configurations therefore have separate lockfiles. Locked rebuilds pass. A first export exposed a generated WSL/Windows profile-path artifact inside the project; explicit WSLENV path translation fixed the profile path and the artifact was moved out of source. Export validation checks both compiler logs and managed outputs because editor exit code alone was insufficient for a failed managed publish. The standalone runtime was then actually launched and verified.

The native Windows SDK first-run log emitted a development-certificate creation notice; no certificate trust command was run. Future first-run/telemetry/certificate settings are explicitly carried across WSL. No Unity install, license change, remote deployment, institutional data or outbound messages occurred.

## Proposed architecture revision

Keep C# scientific contracts, PostgreSQL/SQLite separation, ASP.NET backend, React portals, tenant isolation and phase gates. Replace Unity/URP presentation adapters with Godot nodes/scenes, its text server and the Compatibility renderer for the first proof. Keep Windows-first offline delivery. Preserve the Unity scaffold and evidence as a historical alternative until an engine decision is accepted; do not migrate published data or rewrite server contracts for this change.

Before Phase 1: owner engine selection; school hardware baseline; scientific reviewer/tolerance acceptance; precise Saudi secondary curricular mapping; human Arabic/input review; and local-data protection/provider decisions. Once accepted, update the requirements and phase gate around Godot rather than retaining Unity/IL2CPP acceptance criteria for a Godot client.

Reference downloads: [Godot .NET for Windows](https://godotengine.org/download/windows/), [Windows export](https://docs.godotengine.org/en/stable/tutorials/export/exporting_for_windows.html). The observed results are limited to the committed spike and evidence snapshot.
