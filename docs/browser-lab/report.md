# Localhost browser laboratory — 0.4

Delivered 2026-11-04 following the owner's explicit choice to build a browser version. The browser client lives in [src/BrowserLab](../../src/BrowserLab/README.md); the Godot desktop delivery remains preserved.

## Run

```sh
python3 tools/browser_lab.py
```

Open **http://localhost:5080** and leave the host terminal running. The pinned portable SDK is used when available. A standard .NET SDK 10.0.402 installation can instead run `dotnet run --project src/BrowserLab/BrowserLab.csproj --no-launch-profile`. Browser assets have no runtime CDN dependency; the renderer, controls, HDR lighting, font and element reference data are local. The app requires its local C# host; opening `index.html` directly or uploading it to a static host alone will not run the scientific API.

The server only binds 127.0.0.1. Host/origin checks keep the workspace API local. Windows Edge accessed the WSL-hosted address successfully. This task creates no public website or hosted backend.

## What is implemented

- Furnished, shaded WebGL 2 workbench with clear glassware, calculated liquid fills, instruments, components, leads and distinct models for every equipment type. The camera orbits with damping; tools can be selected, dragged, numerically positioned and rotated. Lower graphics quality reduces pixel density and disables shadows.
- Searchable, categorized equipment library using the shared 52-type catalog; 12 objects per bench. The shared meter is available in both labs, so the two catalog views collectively contain 53 entries. Reference apparatus is explicitly labeled.
- Searchable periodic table with all 118 elements, atomic number/mass, electron configuration, state and virtual samples. The sample's calculated mass uses the same attributed PubChem snapshot as the desktop app.
- Eleven named 0.1 mol/L stock solutions, volume/capacity checks, transfer and dilution, full-aliquot volumetric pipette behavior, concentration, restricted ideal pH, fixed model temperature and a virtual tared sample balance.
- Interactive terminal picking and keyboard-accessible wire selectors, signed DC readings, voltage/current instrument modes, open-circuit and short-circuit reporting, switch controls, lamp/LED brightness, component values, steady-state capacitor charge and six standalone logic trainers.
- Five working circuit presets: series, parallel, switch/lamp, LED and capacitor. Both labs retain 20 undo states.
- English/Arabic RTL interface, narrow-screen layout, browser-local autosave, restart/resume and validated JSON export/import for both benches. Browser drafts are separate from the desktop SQLite store. View controls reset on reload; scientific equipment poses and states persist.

The local host directly links [Scientific.Core](../../src/Scientific.Core), [SessionModel.cs](../../src/GodotClient/SessionModel.cs) and [ToolCatalog.cs](../../src/GodotClient/ToolCatalog.cs). Calculations are not reimplemented in JavaScript. Every command validates the incoming workspace, applies the requested operation to a private copy, updates undo/model declarations and validates the result. Invalid operations return an error; the browser retains its previous state. The server holds no shared mutable draft, and localStorage failures produce an export reminder.

The scientific limits remain those of [the expanded models](../equipment-expansion/report.md): ideal single-solute dilution, pH only for water/HCl/NaOH at 25 °C, piecewise diodes/LEDs with at most eight junctions, fixed-resistance lamps and DC steady state. There is no reaction, thermal, AC or transient model. Reference apparatus does not simulate a process. Graphics are illustrative. Independent science review and target-school/full-hour qualification remain open.

## Verification

| Check | Result |
| --- | --- |
| Browser host build | Zero warnings, zero errors |
| Shared science and native persistence harness | 829 assertions passed |
| Actual localhost HTTP API suites | 9 passed, covering every equipment type and all logic inputs |
| Real Edge UI/WebGL smoke | 33 checks passed |
| Graphics | Edge 156.0.4314.58, ANGLE / NVIDIA GeForce RTX 4060 Ti / Direct3D 11 |
| Rendering coverage | All 52 unique equipment types, with the meter exercised in each lab |
| UI coverage | Real 3D dragging/terminal picking, transfers, dilution, circuit presets, meter short, logic, periodic table, sample mass, search/add, undo, export/import and restart |
| Layout coverage | 1512 × 982 desktop, 390 × 844 mobile, English and Arabic RTL |
| Browser errors | No unexpected JavaScript/console errors or failed asset requests |

The HTTP tests additionally check HCl/NaOH pH, probe proximity, sample balance mass, rejected mixed-solute transfers, rejected partial volumetric aliquots, power conservation, capacitor charge, value/placement bounds, history limits, unknown model rejection and foreign Host/Origin rejection. Browser tests use an isolated temporary profile. An intentional HTTP 400 for an unsupported imported model is expected and excluded from unexpected-console-error counts. These are development smoke tests rather than long-session performance certification.

[Raw browser results](evidence/browser-result.json), [API tests](evidence/api-tests.log), [science results](evidence/science-reference.json) and [build output](evidence/build.log) are retained.

## Captures

These are screenshots from the running browser app:

- [Chemistry workbench](evidence/chemistry-browser.png)
- [Wired LED circuit and calculated readings](evidence/circuit-browser.png)
- [Periodic table](evidence/periodic-browser.png)
- [Arabic interface](evidence/arabic-browser.png)
- [Mobile Arabic interface](evidence/mobile-arabic-browser.png)
- [Mobile periodic table](evidence/mobile-periodic-browser.png)

The additional equipment-batch captures remain in `artifacts/browser-qa` and are reproducible with [the UI harness](../../tests/browser/lab.smoke.cjs).

## Source and provenance

Three.js 0.187.0 is a local, MIT-licensed dependency, downloaded from the [npm registry's pinned package](https://registry.npmjs.org/three/0.187.0), checked against npm's SHA-512 integrity, and recorded with per-file SHA-256 hashes. The launcher checks those hashes along with the element and lighting checksums. The source snapshot and license are tracked so no npm/CDN request is needed to use the lab. Existing font, CC0 HDRI and PubChem notices accompany their local assets.

The user-supplied reference archive remains reference data only; it provided no executable code or task instructions. The browser source is prepared for the owner's repository at [hkudaish/PhysicsLabSimulator](https://github.com/hkudaish/PhysicsLabSimulator). No accounts, cloud sync or Phase 2 portal workflows are introduced.
