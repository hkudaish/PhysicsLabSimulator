# Verification — expanded 0.3 build

Final checks completed 2026-11-04. All automated runs used isolated draft stores; they did not replace the user's normal local drafts.

| Check | Result |
| --- | --- |
| Scientific reference and native persistence harness | 829 assertions on Linux and 829 on Windows |
| Phase 0 artifact/source integrity tests | 19 passed |
| Managed build | Zero warnings, zero errors |
| Linux source headless integration | 83 checks |
| Windows Forward+ real-renderer integration and captures | 113 checks |
| Windows OpenGL Compatibility integration and captures | 113 checks |
| Preserved original-version draft restore | 10 checks |
| Real window-close test | Owned process accepted WM_CLOSE, saved and exited successfully |
| Fresh Windows process after close | 12 checks, including the separately saved expanded workspace |
| ZIP extraction and execution | CRC validation and 12 restart checks from the extracted package |

The science harness includes ideal dilution conservation and rejected mixed solutes, HCl/NaOH pH including water autoionization, all 118 element records, all six logic truth tables, analytical LED cases and two-junction circuits, power conservation, version declarations and native SQLite transactions. Runtime checks exercise the real tool inspector commands, pH probe, full-aliquot pipette restriction, switch/undo, LED wiring and terminal picking, capacitor steady state, all equipment meshes, modal catalog/table controls and expanded save/reload. [Summary](evidence/verification-summary.json), individual JSON reports and successful runtime logs are preserved in [evidence](evidence/).

The export was checked for managed build errors even when Godot's exit code was zero. Final integration logs contain no engine errors or native shutdown leak warnings. Source/model changes and scientific tests are available for independent review; these checks are not scientific accreditation.

## Rendered previews

- [Periodic table](evidence/periodic-table-lab.png) and [searchable equipment catalog](evidence/equipment-catalog-lab.png).
- [Glassware](evidence/chemistry-catalog-0-lab.png), [chemistry instruments and apparatus](evidence/chemistry-catalog-8-lab.png), [more apparatus](evidence/chemistry-catalog-16-lab.png), [remaining apparatus](evidence/chemistry-catalog-24-lab.png).
- [DC sources and passive components](evidence/electricity-catalog-0-lab.png), [meters, junctions and logic](evidence/electricity-catalog-8-lab.png), [remaining circuit tools](evidence/electricity-catalog-16-lab.png), [wired capacitor example](evidence/expanded-capacitor-lab.png).

These are captures from the Windows executable. Every catalog type is rendered in batches; the screenshots are not design mockups.

## Performance smoke

NVIDIA GeForce RTX 4060 Ti; Vulkan Forward+ and OpenGL 3.3 Compatibility. The scripted benchmark uses each baseline scientific bench with camera orbit, 10 seconds per experiment/profile, a 1012 × 768 3D viewport inside the 1440 × 960 app window.

| Profile | Chemistry / circuits average FPS | p95 frame time |
| --- | --- | --- |
| Forward+ realistic, 4× MSAA, SSAO/SSR, 120 FPS cap | 120.1 / 120.1 | 8.37 ms each |
| Forward+ low, 60 FPS cap | 60.1 / 60.1 | 16.68 ms each |
| OpenGL Compatibility low, 60 FPS cap | 60.1 / 60.1 | 16.68 ms each |

See [Forward+ samples](evidence/windows-results-benchmark-result.json) and [OpenGL samples](evidence/windows-compat-results-benchmark-result.json), including process memory. These short baseline measurements do not establish maximum-equipment performance, minimum school hardware, long-session stability or a 60-minute acceptance pass.

## Package

[VirtualSTEM-Lab-Expanded-Windows-x64.zip](../../artifacts/equipment-upgrade/VirtualSTEM-Lab-Expanded-Windows-x64.zip): 80,104,967 bytes. SHA-256:

```text
ebf9ac949fb7092a7d29de4ae73e61b618f42243994e7c68da6f9a3a63443b2e
```

[Package manifest](evidence/package-manifest.json) records the packaged files and their hashes. The ZIP contains the native Windows executable, PCK, self-contained .NET runtime, low-graphics launcher, usage notes and asset/license notices including PubChem provenance. The previous visual and original deliveries remain intact.
