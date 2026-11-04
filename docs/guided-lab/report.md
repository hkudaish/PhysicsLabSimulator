# Guided experiments and workbench controls — 0.5

Applied 2026-11-04 from the owner's `Guided Experiments.md` and explicit instruction to apply it. [The source record](requirements-source.json) preserves the reference filename and SHA-256. The document was treated as requirements reference, not as independently executable instructions. This delivery updates the localhost browser app; preserved Godot executables and scientific model implementations remain unchanged.

## Run and use

```sh
python3 tools/browser_lab.py
```

Open **http://localhost:5080**. Keep the terminal running. If the host is already running an older build, stop that owned host and restart it, then refresh the page. `--no-build` starts an existing build and `--port 5081` selects a separate loopback validation port. See [the browser launch instructions](../../src/BrowserLab/README.md) for SDK setup.

The full bench is visible by default. **Equipment** and **Details** open drawers over the scene; close them to regain the entire workspace. Select an object for its anchored action bar. The chemistry and circuit benches share a visual language but use distinct green/blue accents.

- Drag equipment cards onto the bench. A snapped green footprint previews a valid position; a red footprint rejects overlap or out-of-bounds placement. Clicking a card still offers assisted placement. On a phone, tap a card and close the drawer; this remains available without native HTML drag support.
- Select then drag a tool, or use **Move** to drag any tool directly. Rotate, inspect or set exact coordinates from Details. Empty vessels do not offer a pour command; partial volumetric pipettes must be filled before their full aliquot can be delivered.
- **Fill** transfers a chosen quantity from an existing compatible source. **Empty to waste** explicitly discards the vessel contents and logs the removed volume/moles. **Duplicate empty vessel** preserves capacity and appearance while creating no extra solution. Components/instruments can also be duplicated; wires are not copied.
- Click two terminals or drag from one terminal to another. Valid destinations highlight and a lead preview appears before commitment. **Alt-drag** a connected terminal to move the first attached lead end. For a specific lead when several share a terminal, use its **A↗ / B↗** control in Details and click the replacement terminal. Escape cancels. **Disconnect** removes the selected component's leads.
- **Undo / Redo** work through toolbar buttons and **Ctrl+Z / Ctrl+Y / Ctrl+Shift+Z** outside text fields. Redo persists across reload and is cleared by a new scientific action on the same bench. Each lab retains 20 undo/redo states. Reset is confirmed and undoable; in an active guide it restarts that lesson after confirmation.
- **Pause** locks experiment editing while inspection/camera navigation remain available. The underlying scientific models are steady state: this is not a claimed thermal/transient time pause. **Resume** unlocks editing.
- **Save experiment** saves locally. **Export notebook** includes both benches, undo/redo, lessons, separate saved experiment benches and student notes. **Import draft** accepts the new notebook or legacy plain workspace JSON. Imports are scientifically validated before replacing the current workspace. Old browser drafts migrate automatically when first saved.

## Guided learning

Choose **Guided Mode** or **Guide** for the experiment catalog. Filter by lab, topic, difficulty, duration or type; search also matches equipment. Each lesson includes an objective, estimated duration, equipment/materials, safety/scope notes, procedure, observations, explanation/equation, common mistakes and student questions. English and Arabic versions are bundled locally.

**Use in Lab** starts the selected hands-on experiment after confirming replacement of its lab bench. The other lab is preserved. The guide highlights the next tool and bench position. **Use in Lab** within a step opens the relevant library, selects the required object or focuses its controls. Element-sample steps open the periodic table. Card clicks use the highlighted position; dragged tools must actually reach it.

The next step advances only when predicates on the real workspace and C#-calculated readings pass. This includes appropriate equipment/placement, substance identity, quantities, component values, wiring/polarity, instrument targets/proximity and expected current/voltage/concentration/pH/mass. Required steps have no skip action. Optional reflection may be skipped. Hints progress from general direction to specific guidance and a full solution, but cannot complete an incorrect setup. Measurement steps require a student entry consistent with the actual measurement and units.

At completion, the result sheet shows expected/model-observed values, difference, equation/explanation, verified steps, student questions and saved notes. The local ledger retains attempts, completion percentage, last step, completion count and best absolute model-result difference. If a completed bench is later changed, the guide explicitly distinguishes its historical recorded result from the current bench.

Switching to **Free Mode** hides instructions without changing the scientific state. Unfinished lessons save their actual bench separately. Resuming after another lesson replaced that lab restores the saved apparatus, quantities, values and connections through the same validated host adapter. Browser progress is local learner data, not tamper-proof grading or an institution record.

## Curriculum

| Runnable experiment | Result checked by the current model |
| --- | --- |
| Measuring liquid volume | 25 mL of water in a placed graduated cylinder |
| Dilution and concentration | 25 mL of 2 mol/L stock plus 75 mL water; targeted probe reads 0.5 mol/L |
| Acid, neutral and alkaline pH | Separate 0.1 mol/L HCl, water and 0.1 mol/L NaOH; targeted pH readings near 1, 7 and 13 at 25 °C |
| Amount and element sample mass | Targeted Cu balance; 0.020 mol × 63.55 g/mol = 1.271 g |
| Simple LED circuit | Correct polarity and 1 kΩ resistor; current = (9 − 2)/(1000 + 10) A |
| Ohm's law | Valid loop at 6 V and 9 V, unchanged 1 kΩ resistance and calculated V/I |
| Series resistors | Two 1 kΩ resistors, 4.5 mA and 4.5 V drops; equivalent 2 kΩ |
| Parallel resistors | Two 1 kΩ branches with 9 V each; 18 mA total source-current magnitude |
| Measuring voltage and current | Voltmeter across resistor, then current meter in series; 9 V and 9 mA |
| LED polarity | Physically reversed leads; negative junction voltage and modeled off leakage near 9 nA |
| Switch-controlled lamp | Actual open/closed topology; closed 100 Ω lamp at 9 V gives 0.81 W |
| Capacitor at DC steady state | Increase 100 μF to 200 μF at 9 V; final charge becomes 1800 μC with zero DC branch current |

Six additional examples are **concept lessons, not simulated experiments**: heating water to boiling, neutralization, precipitation, evaporation/crystallization, liquid-density measurement and capacitor charging/discharging. They provide the requested teaching structure and scientific explanations, but no fake heating controls, precipitates, liquid-mass readings or measured time curves. Reading them does not earn simulated-experiment completion. The separate DC-capacitor activity is runnable and explicitly distinguishes final charge from a transient.

This follows the document's instruction to preserve the existing science. Thermal/reaction/solubility/liquid-density/transient models are not added. Reference burners, condensers, holders, transistors and generators remain inspectable references. AC, frequency/duty-cycle controls, direct ohmmeter mode, source current limiting, functional motors/oscilloscopes, breadboard-hole snapping and mechanical stand/burner attachment are not exposed as working features because the current model has no such behavior. Capacity-changing resizing and pouring-rate dynamics are likewise not implied.

## Requirements coverage

| Document section | Applied behavior |
| --- | --- |
| 1–2: design and workspace | Compact hierarchy, contextual states, lab accents, full-width bench, independent overlay drawers |
| 3–4: tool/context controls | Type-specific toolbar, compatible fill/pour, explicit discard, duplicate-empty, measurements, values, rotation, connect/disconnect |
| 5: drag/drop | Real library drops, snap/footprint preview, collision checks; terminal snapping and editable lead endpoints |
| 6–8: quick access/history/state | Compact Select/Move/Measure/Pause/Guide controls, persistent redo, shortcuts, confirmed reset, manual/automatic save and import |
| 9: full guide structure | Objectives, levels/time, materials, scope/safety, procedures, observations/results, explanations, mistakes and questions |
| 10–14: guided/free/scientific validation | Highlighted next tool/position, live predicates, progressive hints, blocked required steps, optional reflection skip and result comparisons |
| 15–18: learning progress/discovery/integration | Local attempts/progress/best result/notes, per-lesson bench checkpoints, filters/search, direct links to the equipment and controls |
| 19–20: combined experience | Large interactive workspace, contextual actions and an integrated compact step/measurement guide |

## Implementation and verification

[experiments.mjs](../../src/BrowserLab/wwwroot/experiments.mjs) contains bilingual lesson metadata and checks against the actual API response. [guide.mjs](../../src/BrowserLab/wwwroot/guide.mjs) handles progress, hints, records, results and resuming. The [browser adapter](../../src/BrowserLab/Bench.cs) adds placement, duplicate-empty, discard, redo/checkpoint restore, disconnect and endpoint edit commands. Every restored snapshot is validated before it can affect the workspace. Discard logs trim correctly at the bounded action-history limit. The scientific model identifiers and implementations stay unchanged.

Validation uses isolated profiles and the separate 5081 host. Final evidence records:

- **829** shared science/native-persistence assertions.
- **11** HTTP API suites, including empty/duplicate conservation, discard/undo/redo, malformed redo rejection, checkpoint restore and real reverse-LED rewiring.
- **16** curriculum/workflow tests: every runnable experiment against the C# host; metadata completeness; required-step blocking; concept/result separation; bounded saved learning state.
- **51** guided UI/WebGL checks in Edge, including actual library dragging, collision rejection, student-answer validation, hints, pause, shortcuts, notebook import/export, restoring an older attempt's real saved bench, lead dragging/endpoint editing, Arabic filters and mobile layout.
- The existing **33** browser/all-equipment regression checks and a warning/error-free host build, recorded with the final handoff evidence.

See [verification summary](evidence/verification-summary.json), [guided browser results](evidence/guided-browser-result.json) and [free-lab regression results](evidence/browser-result.json). These are development checks, not independent science review, school-hardware qualification or a full-hour acceptance run.

## Captures

- [Full-width free workspace](evidence/free-workspace.png)
- [Integrated experiment catalog and procedure](evidence/experiments-guide.png)
- [Validated guided step](evidence/validated-volume.png)
- [Expected/observed results, explanation and questions](evidence/experiment-result.png)
- [Arabic experiment filters](evidence/arabic-guide.png)
- [Mobile workbench](evidence/mobile-workspace.png)
- [Mobile guide](evidence/mobile-guide.png)

Concept references are linked directly in their lessons: [OpenStax Chemistry 2e phase transitions](https://openstax.org/books/chemistry-2e/pages/10-3-phase-transitions), [reaction classification](https://openstax.org/books/chemistry-2e/pages/4-2-classifying-chemical-reactions), [solubility](https://openstax.org/books/chemistry-2e/pages/11-3-solubility), [physical properties](https://openstax.org/books/chemistry-2e/pages/1-3-physical-and-chemical-properties), and [University Physics RC circuits](https://openstax.org/books/university-physics-volume-2/pages/10-5-rc-circuits). Element provenance, renderer integrity and all existing asset notices are retained.
