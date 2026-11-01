# Scientific reference validation

The renderer-independent implementations are `src/Scientific.Core/Dilution.cs` and `ResistiveDc.cs`. The console harness and actual application use these same assemblies. Deterministic analytical expectations are independent of rendered labels.

## Dilution: dilution-v1

Canonical volume is L; amount is mol. Homogeneous transfer carries `n_transfer = n_source × V_transfer / V_source`. Source and destination receive equal opposite volume/amount changes. Pure water adds volume without solute. Concentration is `n/V` for nonempty vessels. Equation basis and assumptions: [OpenStax Chemistry 2e, Molarity](https://openstax.org/books/chemistry-2e/pages/3-3-molarity).

SCI-C01–06 cover 1 M diluted 1:1 to 0.5 M; 25 mL of 2 M plus 75 mL water yielding 0.050 mol/0.100 L; 20 mL transferred from 100 mL of 1 M leaving 0.080 mol; invalid/nonfinite/overflow quantities; empty/unsupported solutions; and repeated-transfer conservation. Rejections return the original immutable state. In the app, finite water-bottle transfers conserve whole-bench liquid volume and solute.

## Resistive DC: resistive-dc-v1

Wires merge terminal nodes. Modified nodal analysis stamps resistor conductances and ideal-source constraints, including zero-voltage ammeter branches. Partial pivoting solves the matrix. Node differences produce voltage readings; branches supply current, power and overload readings. Governing constraints: Kirchhoff’s laws and Ohm’s law, described in [OpenStax University Physics 2, Kirchhoff’s Rules](https://openstax.org/books/university-physics-volume-2/pages/10-3-kirchhoffs-rules).

SCI-E01–06 cover 9 V/1000 Ω (9 mA, 81 mW), 5 V/220 Ω, series division, parallel currents, open/short/floating/reversed probes and invalid/conflicting sources. Further tests cover series ammeters, meters across sources, overload, wire merging, 100 seeded random series and 100 parallel analytical networks. Power conservation and matrix residuals are checked. Undefined ideal-source current is never clipped to a finite reading.

## Evidence and limits

The reference harness passed **452 assertions on Linux and 452 on Windows**, including native SQLite Unicode/quoted JSON, version rejection, workspace roundtrips and invalid saved data. Numeric comparisons use absolute `1e-10` plus relative `1e-9`; for currents/voltages this is tighter than the proposed Phase 0 tolerance. Scaled MNA residual must be at most `1e-9`. Display rounding does not enter calculations.

The actual Windows app separately checks transfers/conservation, probe proximity, live controls, voltage/current meters, faults, physics ray picking, undo, language, graphics independence, damaged/newer saves and fresh-process restoration. See [the report](report.md) and raw evidence.

Automated results establish computational and integration evidence, pending independent science signoff and Saudi curricular review. Dilution supports one ideal nonreactive solute; arbitrary chemistry is refused. DC excludes transient/thermal effects. Redundant ideal constraints may have indeterminate branch currents and return a singular diagnostic. No chemical dataset, safety certification or approved real-laboratory procedure is claimed.
