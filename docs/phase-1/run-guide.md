# Windows prototype instructions

Extract the entire `VirtualSTEM-Lab-Phase1-Windows-x64.zip` archive. Open `VirtualSTEM-Lab/VirtualStemLab.exe` on Windows x64. Keep the executable, PCK, managed data folder and licenses together. No editor, SDK, installer or .NET setup is required. The actual exported app was tested directly, without a source-project path.

The app starts in Arabic; the language button switches to English. Help explains controls and model boundaries. Equipment uses schematic procedural geometry, before production art and classroom usability qualification.

## Solution dilution

1. Select the stock solution, initially 100 mL at 2 mol/L.
2. Choose the empty 250 mL beaker as destination. Set the quantity to 25 mL and transfer.
3. The beaker becomes selected. Set the quantity to 75 mL and add water.
4. Read 100 mL, 0.050 mol and 0.500 mol/L. The stock and water bottle lose the corresponding volumes.
5. Select the concentration probe. It measures its chosen vessel within 1.65 scene metres. Move it closer or choose another vessel with the measurement button.

Water comes from a finite bottle. To transfer other liquids, select the source and choose its destination. Insufficient liquid or overflow leaves both vessels unchanged. Empty-vessel concentration is unavailable. Liquid height follows volume; tint illustrates concentration without predicting chemical color.

## Ohm’s law

1. Switch experiments. A 9 V source, 1000 Ω resistor and multimeter are available. More equipment can be placed from the library.
2. Click the red source terminal, then the red resistor terminal. Click the black resistor terminal, then the black source terminal. This closes the circuit with two wires.
3. Select the resistor to read 0.009 A and 0.081 W. Change voltage or resistance in the inspector to recalculate.
4. In voltage mode, connect the meter across the resistor. For current mode, remove the resistor’s return wire and insert the meter in series between resistor and source return. The default loop reads 0.009000 A.
5. Select a wire and disconnect it to open the loop. Directly connecting source terminals, including through an ammeter, creates an ideal-source short with no fabricated current reading.

Red/black terminals define measurement orientation; reversed probes change the sign. Floating spare components may make the bench unsolvable: remove them or finish their connections. Indeterminate ideal-source loops return a diagnostic. Resistors have a 0.25 W power rating; overload produces a warning without a thermal burnout animation.

## Manipulation and drafts

- Click equipment to select; drag it to an unoccupied table position. Invalid placement is rejected.
- Q/E and arrow buttons rotate. Right-drag orbits; the wheel zooms. Reset camera restores the initial view.
- Undo or Ctrl+Z restores up to 20 snapshots per experiment. Reset starts a fresh experiment and can itself be undone.
- Low/high graphics changes antialiasing without changing scientific quantities.
- Save locally stores both benches, quantities, wires, instrument modes, poses, camera, actions and undo history. Measurements are recalculated using saved model versions.
- Closing the window waits for an atomic save. A save failure leaves the window open. Startup and Resume restore the last saved draft.

Normal draft location: `%APPDATA%/Godot/app_userdata/Virtual STEM Laboratory/drafts/laboratory.db`. Back up after closing the app. Test tools use separate `artifacts/phase1/*-test-storage` directories. Incompatible drafts are preserved; do not delete one to fix a version error.

These are anonymous local practice drafts, without accounts, trusted grades or cloud sync. Ideal dilution excludes reactions, heat and pH; ideal DC excludes AC and semiconductor dynamics. Independent science review, school-PC qualification and the hour-long acceptance run remain open.
