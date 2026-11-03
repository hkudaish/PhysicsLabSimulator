# Expanded chemistry and circuit benches — 0.3

Delivered 2026-11-04 in response to the owner's request to enrich the app using `D:\lab-local.zip` as a reference. The archive was inspected as untrusted reference data. Its scripts were not run, its embedded instructions did not become task instructions, and its bundled JavaScript was not incorporated. See [reference audit](reference-audit.json) for hashes and the original inventory. Existing source documents and earlier deliveries are preserved.

## Run and explore

Extract [VirtualSTEM-Lab-Expanded-Windows-x64.zip](../../artifacts/equipment-upgrade/VirtualSTEM-Lab-Expanded-Windows-x64.zip) completely and launch **VirtualStemLab.exe**. No editor or .NET installation is required. **LowGraphics.bat** selects OpenGL Compatibility. Close an older app first so it cannot overwrite the same normal local draft.

- In either lab, **Equipment +** opens a searchable catalog grouped by purpose. Select a tool to add it, select the object on the bench, then use its inspector controls. Up to 12 objects fit on one bench; remove objects to make room.
- **Periodic table** opens a searchable 118-element table. Select an element to inspect its number, symbol, atomic mass, state and electron configuration, then add a virtual sample on the chemistry bench. Adjust its amount in moles; the balance reports its calculated mass when the selected sample is nearby.
- Select a source vessel and target vessel to transfer liquid. The 25 mL volumetric pipette only dispenses a complete 25 mL aliquot. Instruments need a selected target within 1.65 scene units; drag them closer if needed.
- In the circuit lab, **Circuit presets** loads series resistors, parallel resistors, switch/lamp, LED with series resistor, or capacitor examples. Undo restores the previous arrangement. Click terminal pairs to wire parts. Change resistance, source voltage, junction forward voltage and switch state in the inspector.
- Logic boards have independent A/B input toggles and a truth table. They do not expose analog wiring terminals.

Selection, dragging, camera orbit/zoom, rotation, undo, Arabic/English switching and local save/resume continue to work. The normal store remains `%APPDATA%/Godot/app_userdata/Virtual STEM Laboratory/drafts/laboratory.db`. No runtime network connection is needed.

## Inventory and behavior

There are **52 unique equipment types**, with 32 chemistry catalog entries and 21 circuit entries because the meter serves both labs. [ToolCatalog.cs](../../src/GodotClient/ToolCatalog.cs) is the shared catalog used by the app and tests.

| Group | Equipment | Supported behavior |
| --- | --- | --- |
| Liquid vessels | Beaker, graduated cylinder, Erlenmeyer flask, Florence flask, test tube, burette, graduated pipette, 25 mL volumetric pipette, funnel, watch glass | Capacity-checked transfers, ideal dilution, calculated fills and graduation marks; distinctive hollow 3D profiles |
| Chemistry sources | Stock solution, pure water | Existing ideal stock plus 11 named 0.1 mol/L, 100 mL reagent choices |
| Chemistry instruments | Concentration probe, pH probe, triple-beam balance, thermometer | Concentration; restricted ideal pH; tared virtual element mass; fixed 25 °C model temperature |
| Element samples | One selectable sample type for all 118 elements | Reference details and mass = amount × reference atomic mass; virtual display vial |
| Chemistry reference apparatus | Ring stand, condenser, mortar/pestle, crucible/cover, Bunsen burner, clamp, test-tube holder, pipette bulb, brush, scoopula, stopper, ceramic square, gauze, clay triangle, tongs | Inspectable movable 3D tools; no heating, distillation, grinding, mechanical attachment or process calculations |
| DC components | Source, resistor, rheostat, resistive lamp, switch, capacitor, inductor, diode, LED | Calculated DC operating points, signed branch readings, overload indication, switch state, current-dependent lamp/LED glow |
| Electrical instruments | Multimeter, voltmeter, ammeter | Ideal voltage and current measurements; current meter is a zero-voltage series branch |
| Logic trainers | AND, OR, NOT, NAND, NOR, XOR | All input combinations and output truth tables; standalone Boolean trainers |
| Circuit reference apparatus | NPN transistor, relay, signal generator | Movable labeled 3D references, without fabricated analog behavior |

The 11 reagent choices are HCl, NaOH, H₂SO₄, NaCl, CuSO₄, AgNO₃, BaCl₂, FeCl₃, KI, Pb(NO₃)₂ and sucrose. They retain their solute identity through transfer/dilution. Different solutes cannot be combined: the command is rejected without changing the scientific state. Colors are illustrative.

## Scientific scope

Chemistry uses the explicit `ideal-single-solute-v2` model. Volume and solute amount are conserved; there are no reactions, neutralization, precipitation, activity corrections, gas generation or heating. pH is available for pure water and ideal fully dissociated HCl/NaOH at 25 °C, with water autoionization included. Other named solutions intentionally have no pH estimate. The thermometer reports the fixed model temperature, not a thermal simulation.

Circuits use `dc-components-v2`, extending the existing modified nodal analysis. A closed switch and ideal inductor are DC conductors. A capacitor is open in steady state and reports Q = CV only when voltage is defined; no charging/discharging curves or inductive transients are implied. Lamps use fixed resistance, without thermal resistance changes. Forward-conducting diodes/LEDs use an adjustable fixed voltage drop plus 1 Ω/10 Ω respectively; the off branch uses 1 GΩ leakage. The deterministic solver checks candidate junction states, up to eight junctions. There is no exponential junction model, reverse breakdown, AC, transistor model or physical burnout. Overload is a warning based on calculated power.

Newly created workspaces declare the v2 model identifiers. Valid older v1 drafts restore using their recorded headers until a new tool/reagent requires an explicit upgrade. Unsupported future models are rejected. Expanded v2 drafts require this build; the preserved 0.1/0.2 apps cannot read them. Serialization envelope version remains unchanged.

## Sources and assets

Element facts come from the offline [PubChem/NCBI periodic table API](https://pubchem.ncbi.nlm.nih.gov/rest/pug/periodictable/JSON), retrieved 2026-11-03, with the API described by the [IUPAC FAIR Chemistry Cookbook](https://iupac.github.io/WFChemCookbook/datasources/pubchem_ptable.html). [Provenance](../../src/GodotClient/assets/catalog/elements-provenance.json) records source and normalized checksums, verified by the build tool. Arabic factual names were cross-referenced with the supplied archive. Atomic masses are rounded snapshot values, and entries are not all standard natural-abundance atomic weights; for example, this snapshot gives Cu as 63.55 g/mol. This is a reference table, not a reaction or safety database.

The pH implementation follows the concentration definitions and 25 °C water relation described in [OpenStax Chemistry 2e](https://openstax.org/books/chemistry-2e/pages/14-2-ph-and-poh). Junction behavior is an explicitly limited engineering approximation; see [OpenStax semiconductor devices](https://openstax.org/books/university-physics-volume-3/pages/9-7-semiconductor-devices) for the broader physical model.

Equipment is independently authored code-native geometry and materials, retaining the existing furnished lab, glass, lighting, smooth camera and render cleanup. All earlier asset notices remain bundled; PubChem attribution is included. Reference apparatus is not a promise of a complete process simulator.

## Verification and remaining review

See [verification evidence](evidence.md) for the final executable, science checks, render checks, restart, legacy restore and extracted-package checks. Performance samples describe the development PC and baseline benches; equipment catalog captures additionally render every equipment type in batches. They do not certify a school PC, maximum loaded bench or a full-hour session.

Independent scientific review, classroom usability, minimum school hardware qualification and the 60-minute acceptance protocol remain open. This user-authorized equipment extension does not start accounts, licensing, cloud sync or the planned Phase 2 backend.
