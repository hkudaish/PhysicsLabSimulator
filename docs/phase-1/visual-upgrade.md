# Visual upgrade — Godot 0.2 prototype

Historical delivery: the current build and its additional scientific models are documented in [the equipment expansion report](../equipment-expansion/report.md). This report and its evidence describe the preserved 0.2 build.

Date: 2026-11-02. This is the owner-authorized visual and interaction refinement of Phase 1. Accounts, licensing, cloud sync and Phase 2 work have not started. Phase 1 acceptance remains open for independent science review, classroom usability, approved minimum hardware and the full 60-minute stability protocol.

## Run

Extract [VirtualSTEM-Lab-Visual-Windows-x64.zip](../../artifacts/visual-upgrade/VirtualSTEM-Lab-Visual-Windows-x64.zip) completely and start **VirtualStemLab.exe**. Keep its PCK, managed runtime data and licenses together. No editor or .NET installation is required. **LowGraphics.bat** launches the same app using OpenGL Compatibility for older graphics hardware. Both launches use the same normal local draft location and scientific models.

The [original 0.1 ZIP](../../artifacts/phase1/VirtualSTEM-Lab-Phase1-Windows-x64.zip), source checkpoint and original evidence remain preserved. Existing experiment steps are in [the run guide](run-guide.md).

## What changed

- Furnished laboratory with epoxy worktop, cabinetry, hardware, window and background storage.
- Authored reusable mesh profiles for hollow beakers, graduated cylinders, reagent/water bottles, rounded instrument housings and continuous curved leads. Meshes are code-native assets, not generated bitmap mockups.
- Clear-glass Fresnel highlights, liquid surfaces/meniscus, etched graduation marks, metal/rubber finishes and procedurally shaded surface microtexture. Liquid height and readings follow calculated volume and concentration; color remains illustrative.
- Bench DC supply with live voltage display, multimeter LCD/dial/jacks, and resistor bands derived from its selected value. Probe proximity, voltage/current orientation, open circuits and shorts retain the existing scientific behavior.
- Forward+ realistic profile with HDRI illumination, soft shadows, ambient occlusion, screen-space reflections and 4× MSAA. Low turns off screen-space effects/MSAA. The separate Compatibility launcher uses direct/ambient lighting without HDRI radiance, avoiding the Forward+ hardware requirement and a radiance-texture teardown problem observed on OpenGL.
- Camera orbit/zoom and dragging interpolate smoothly. Equipment meshes/physics bodies persist across selection and pose changes; transient liquid meshes are not accumulated in the shared shape cache. Pour animation cannot write scientific state, and another action can cancel the visual animation safely.
- Rounded UI controls, consistent panel spacing and the existing Arabic/English layout. Scientific notation remains isolated LTR.
- Explicit ownership/release of room textures/materials and private scene lifetimes, plus orderly graphics teardown. An additional reflection probe produced a native shutdown warning in testing; the final pipeline omits it and retains HDRI plus screen-space reflections. Collision shapes also release their managed references after native ownership is established; shutdown waits for physics cleanup. Final runtime logs are retained for inspection.

The math libraries and native SQLite store are unchanged. Save schema/model identifiers remain compatible; the new runtime separately tests restoration of an actual 0.1 draft. Fresh sessions use a lower camera angle; loading an existing draft preserves its camera pose. Reset camera selects the new default view.

## Assets and reproducibility

[Studio Small 09](https://polyhaven.com/a/studio_small_09), by Sergej Majboroda / Poly Haven, is bundled as an offline 1K HDRI under [CC0](https://polyhaven.com/license). The repository records its source, download URL, byte count and locally calculated SHA-256 in `src/GodotClient/assets/lighting/provenance.json`; the build tool verifies this checksum. Notices are included with the executable. No asset download or remote service is required while using the app.

Build and export use the existing pinned Godot 4.7.2 .NET engine/templates and .NET SDK 10.0.402. Run:

```sh
python3 tools/lab.py build
python3 tools/lab.py import
python3 tools/lab.py verify
python3 tools/lab.py export
python3 tools/lab.py verify --windows --capture --benchmark-seconds 15
python3 tools/lab.py verify --windows --compatibility --capture --benchmark-seconds 15
python3 tools/lab.py legacy-check
python3 tools/lab.py close-check --windows
python3 tools/lab.py verify --windows --resume
python3 tools/lab.py package
python3 tools/lab.py package-check
```

`legacy-check` uses the preserved initial prototype test draft, copies it into separate test storage and leaves its source untouched. All automated runs require isolated draft storage. Runtime errors and native resource-leak warnings fail the wrapper. The packaged ZIP is extracted to a separate directory and that executable is tested without a source-project path.

## Evidence and performance

Measurements below are development-PC smoke results, not an approved school benchmark. Each experiment/profile sample has a two-second warmup and a scripted gentle camera orbit. Sampling duration is per experiment/profile: 15 seconds gives 60 seconds total on Forward+ and 30 seconds on Compatibility. This is not the required full-hour interactive test.

Package: **80,072,748 bytes** (76.4 MiB). SHA-256: `e61133c58985f8b23bb283dea37259dccfeec10fcf3d1b044fca950f73c27710`. The [manifest](visual-evidence/package-manifest.json) records every packaged file. ZIP CRC and separately extracted standalone execution passed.

| Validation | Result |
|---|---|
| Rebuilt independent science/native SQLite harness | 452 assertions on Linux and 452 on Windows |
| Source headless integration | 51 checks |
| Actual Windows Forward+ with capture and benchmark | 61 checks; clean shutdown log |
| Actual Windows OpenGL with capture and benchmark | 61 checks; clean shutdown log |
| Preserved 0.1 draft in new standalone runtime | 10 checks; full snapshot hash unchanged on load |
| Owned-window WM_CLOSE plus fresh restart | Passed; 10 restart checks |
| Separately extracted packaged executable | 10 restart checks; no source-project path |
| Original-source/design regression | 19 checks |

See [Forward+ checks](visual-evidence/forward-integration-result.json), [OpenGL checks](visual-evidence/compatibility-integration-result.json), [legacy recovery](visual-evidence/legacy-restore.json), [package recovery](visual-evidence/package-restore.json), and the raw runtime logs alongside them.

| Renderer | Experiment | Profile | FPS limit | Mean loop FPS | p95 interval | Worst interval | End working set |
|---|---|---|---:|---:|---:|---:|---:|
| Forward+ | chemistry | Low | 60 | 60.05 | 16.683 ms | 17.473 ms | 685.9 MiB |
| Forward+ | chemistry | Realistic | 120 | 120.06 | 8.361 ms | 10.330 ms | 723.8 MiB |
| Forward+ | electricity | Low | 60 | 60.03 | 16.680 ms | 16.725 ms | 724.3 MiB |
| Forward+ | electricity | Realistic | 120 | 120.05 | 8.362 ms | 10.122 ms | 724.5 MiB |
| OpenGL | chemistry | Low | 60 | 60.05 | 16.683 ms | 17.517 ms | 313.8 MiB |
| OpenGL | electricity | Low | 60 | 60.03 | 16.683 ms | 16.984 ms | 314.8 MiB |

The realistic profile is limited to 120 FPS; low profiles are limited to 60 FPS to avoid unnecessary GPU work. These are engine process-frame intervals rather than measurements of displayed refresh rate. Different pacing limits and renderer features prevent interpreting this table as a relative-throughput ranking. Working set is the resident-memory snapshot at each sample’s end, not peak memory or GPU VRAM.

Development hardware: Intel Core i5-14600KF, 32 GiB nominal RAM, NVIDIA RTX 4060 Ti, Windows 11 Pro. See [hardware metadata](visual-evidence/development-hardware.json), [Forward+ samples](visual-evidence/forward-benchmark-result.json) and [OpenGL samples](visual-evidence/compatibility-benchmark-result.json). This is not approved minimum school hardware.

[Realistic chemistry preview](visual-evidence/forward-chemistry-lab.png) · [Realistic electrical preview](visual-evidence/forward-electricity-lab.png) · [Compatibility preview](visual-evidence/compatibility-electricity-lab.png)

## Limits and remaining acceptance

The equipment is enlarged for convenient interaction; rendered geometry is not a metrically calibrated digital twin. Glass/liquid shaders and the pour stream are visual approximations, without fluid dynamics, refraction spectroscopy or reaction/thermal physics. The room is procedurally authored, not a photogrammetric production asset set. Independent visual/accessibility and Arabic classroom review remain necessary.

Frame-time percentiles, mean FPS and memory snapshots from a short development run cannot establish worst-case school performance or absence of long-session leaks. The approved minimum PC and the hour-long save/recovery/memory protocol remain open. The prototype still stores anonymous local drafts, with no trusted grades, accounts or encrypted learner database. Production signing/installation remain later-phase work.
