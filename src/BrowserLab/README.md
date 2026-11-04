# Browser laboratory

Current delivery: [guided workbench 0.5](../../docs/guided-lab/report.md), with contextual actions, collapsible drawers, drag/drop, 20-step undo/redo per lab and 18 lessons. Twelve are scientifically validated hands-on experiments; six unsupported process examples are explicitly conceptual. Choose **Guided Mode** for the catalog, or **Free Mode** for exploration. Pause locks editing of these steady-state models. Starting/resetting a guide is confirmed.

The browser client uses Three.js for the 3D workbench and calls a loopback ASP.NET Core host that directly links the existing C# scientific engine, session codec and equipment catalog. It is a working browser client, not a Godot Web export. No account, cloud service or production portal is involved.

From the repository root, using the pinned local development tools:

```sh
python3 tools/browser_lab.py
```

Open **http://localhost:5080**. Keep the terminal running; Ctrl+C stops the host. `python3 tools/browser_lab.py build` builds it separately, and `serve --no-build` starts an existing build. The helper verifies the bundled renderer, lighting and periodic-table checksums first.

On another machine with .NET SDK **10.0.402** installed, the standard command is:

```sh
dotnet run --project src/BrowserLab/BrowserLab.csproj --no-launch-profile
```

The repository's `global.json` pins the SDK. First build requires the appropriate SDK/reference packs; use [the development setup](../../docs/phase-0/10-development.md) for the existing portable toolchain. No Node installation is needed to build or run this client, and all browser assets are bundled locally. A WebGL 2 capable browser with hardware acceleration is recommended for the 3D view.

The host listens on **127.0.0.1:5080**, accepts localhost hostnames, and rejects foreign browser origins. In this workspace, Windows Edge was tested successfully against the WSL-hosted localhost address. It does not listen on the LAN.

The library provides the 52 equipment types from the desktop catalog, the 118-element reference table, 11 named stock solutions, six standalone logic trainers and five wired circuit presets. Select or drag tools in 3D; rotate them in the inspector. Orbit by dragging empty space/right dragging and zoom by scrolling. Terminal clicking and accessible dropdown controls both create real circuit wires. Instrument targets must be within 1.65 scene units. Numeric positioning is also available in the inspector. Undo retains 20 actions per lab.

Both benches, undo/redo, lesson progress/notes and separate unfinished-experiment bench checkpoints autosave in a single browser-local notebook (`virtual-stem-browser-notebook-v1`). **Export notebook** saves its versioned JSON envelope. Imports validate the scientific workspace before replacement; a checkpoint is validated again when restored. Legacy plain workspace JSON and older browser drafts remain readable. Unsupported model versions are rejected. The browser does not write the desktop SQLite database. `localhost` and `127.0.0.1` are different browser storage origins; keep the same address when resuming. These local learner records are not an institution grading system.

Chemistry, pH, piecewise diode/LED and DC steady-state limits are the same as [the expanded desktop models](../../docs/equipment-expansion/report.md). Reference apparatus is labeled. Rendering, geometry and colors are illustrative; there is no reaction, heating, AC or transient solver. The browser supports tool selection/pose, scientific state and undo persistence; camera position and graphics quality are session view controls and reset on reload.

## Verification

With the host running:

```sh
python3 -m unittest discover -s tests/browser -p 'test_*.py' -v
python3 tools/dev.py node --test tests/browser/guides.test.mjs
```

The optional UI/WebGL harness needs Node and Playwright. Install it into ignored QA state, without lifecycle scripts:

```sh
npm install --prefix artifacts/browser-qa --ignore-scripts --no-audit --no-fund playwright@1.56.1
node tests/browser/lab.smoke.cjs
node tests/browser/guided.smoke.cjs
```

On Windows, the harness uses installed Edge in an isolated temporary profile. On other systems, set `STEM_BROWSER_EXE` to a Chromium executable or install Playwright's Chromium separately. `PLAYWRIGHT_MODULE` may point to another Playwright installation. `STEM_BROWSER_URL` overrides the test address; the two `.cjs` UI harnesses also accept the URL as their first argument. For an isolated host, run `python3 tools/browser_lab.py serve --no-build --port 5081` and set that test URL. Tests do not touch the user's normal browser profile. UI reports/captures go to `artifacts/browser-qa` and `artifacts/guided-qa`; retained delivery evidence is in [the guided-lab report](../../docs/guided-lab/report.md).

Three.js 0.187.0 is vendored under MIT with npm integrity and per-file hashes in `wwwroot/vendor/provenance.json`. The existing CC0 Poly Haven lighting, Tajawal font notice and PubChem attribution are bundled in `wwwroot/assets`. Periodic-table JSON remains shared with the desktop source. No code from `lab-local.zip` was used.
