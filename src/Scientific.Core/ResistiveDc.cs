using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace VirtualStem.Science
{
    public enum CircuitKind { Resistor, VoltageSource, Ammeter }
    public enum CircuitStatus { Solved, OpenCircuit, FloatingNetwork, ShortCircuit, InconsistentSources, InvalidInput, NumericallySingular, EmptyCircuit }

    public sealed class CircuitComponent
    {
        public string Id { get; }
        public CircuitKind Kind { get; }
        public string A { get; }
        public string B { get; }
        public double Value { get; }
        public double RatedPowerW { get; }
        public CircuitComponent(string id, CircuitKind kind, string a, string b, double value, double ratedPowerW = 0.25)
        { Id = id; Kind = kind; A = a; B = b; Value = value; RatedPowerW = ratedPowerW; }
    }
    public sealed class CircuitWire
    {
        public string A { get; }
        public string B { get; }
        public CircuitWire(string a, string b) { A = a; B = b; }
    }
    public sealed class BranchReading
    {
        public double VoltageV { get; }
        public double CurrentA { get; }
        public double PowerW { get; }
        public bool Overloaded { get; }
        public BranchReading(double voltage, double current, double power, bool overloaded)
        { VoltageV = voltage; CurrentA = current; PowerW = power; Overloaded = overloaded; }
    }
    public sealed class CircuitResult
    {
        public CircuitStatus Status { get; }
        public IReadOnlyDictionary<string, double> NodeVoltages { get; }
        public IReadOnlyDictionary<string, BranchReading> Branches { get; }
        public double Residual { get; }
        public bool HasReadings => Status == CircuitStatus.Solved || Status == CircuitStatus.OpenCircuit;
        internal CircuitResult(CircuitStatus status, Dictionary<string, double>? nodes = null,
            Dictionary<string, BranchReading>? branches = null, double residual = 0)
        {
            Status = status; Residual = residual;
            NodeVoltages = new ReadOnlyDictionary<string, double>(nodes ?? new Dictionary<string, double>());
            Branches = new ReadOnlyDictionary<string, BranchReading>(branches ?? new Dictionary<string, BranchReading>());
        }
        public double? VoltageBetween(string a, string b) => NodeVoltages.TryGetValue(a, out double va) &&
            NodeVoltages.TryGetValue(b, out double vb) ? va - vb : (double?)null;
    }

    public static class ResistiveDc
    {
        public const string ModelVersion = "resistive-dc-v1";
        private sealed class Union
        {
            private readonly Dictionary<string, string> parents;
            public Union(IEnumerable<string> values) { parents = values.ToDictionary(v => v, v => v, StringComparer.Ordinal); }
            public string Root(string value) { if (parents[value] != value) parents[value] = Root(parents[value]); return parents[value]; }
            public void Join(string a, string b)
            {
                string ra = Root(a), rb = Root(b);
                if (string.CompareOrdinal(ra, rb) < 0) parents[rb] = ra; else parents[ra] = rb;
            }
        }

        public static CircuitResult Solve(IEnumerable<string> terminalInput, IEnumerable<CircuitComponent> componentInput,
            IEnumerable<CircuitWire> wireInput, string groundTerminal)
        {
            string[] terminals = terminalInput.ToArray();
            CircuitComponent[] components = componentInput.OrderBy(c => c.Id, StringComparer.Ordinal).ToArray();
            CircuitWire[] wires = wireInput.ToArray();
            if (terminals.Length == 0 && components.Length == 0) return new CircuitResult(CircuitStatus.EmptyCircuit);
            var unique = new HashSet<string>(terminals, StringComparer.Ordinal);
            if (terminals.Length > 128 || components.Length > 32 || wires.Length > 256 || unique.Count != terminals.Length ||
                terminals.Any(string.IsNullOrWhiteSpace) || !unique.Contains(groundTerminal) ||
                components.Select(c => c.Id).Distinct(StringComparer.Ordinal).Count() != components.Length ||
                components.Any(c => string.IsNullOrWhiteSpace(c.Id) || !unique.Contains(c.A) || !unique.Contains(c.B) || c.A == c.B ||
                    !Enum.IsDefined(typeof(CircuitKind), c.Kind) || !Numbers.Finite(c.Value) ||
                    (c.Kind == CircuitKind.Resistor && c.Value <= 0) || (c.Kind == CircuitKind.Ammeter && c.Value != 0) ||
                    !Numbers.Finite(c.RatedPowerW) || c.RatedPowerW <= 0) ||
                wires.Any(w => !unique.Contains(w.A) || !unique.Contains(w.B) || w.A == w.B))
                return new CircuitResult(CircuitStatus.InvalidInput);
            var sources = components.Where(c => c.Kind != CircuitKind.Resistor).ToArray();
            var powerSources = components.Where(c => c.Kind == CircuitKind.VoltageSource).ToArray();
            if (powerSources.Length == 0) return new CircuitResult(CircuitStatus.EmptyCircuit);
            var union = new Union(terminals);
            foreach (var wire in wires) union.Join(wire.A, wire.B);
            var zeroDrop = new Union(terminals);
            foreach (var wire in wires) zeroDrop.Join(wire.A, wire.B);
            foreach (var source in sources.Where(s => s.Kind == CircuitKind.Ammeter || s.Value == 0)) zeroDrop.Join(source.A, source.B);
            if (powerSources.Any(s => s.Value != 0 && zeroDrop.Root(s.A) == zeroDrop.Root(s.B)))
                return new CircuitResult(CircuitStatus.ShortCircuit);
            var imposed = new Dictionary<string, double>(StringComparer.Ordinal);
            foreach (var source in sources)
            {
                string a = union.Root(source.A), b = union.Root(source.B);
                if (a == b) return new CircuitResult(CircuitStatus.NumericallySingular);
                bool order = string.CompareOrdinal(a, b) < 0;
                string key = (order ? a : b) + "\0" + (order ? b : a);
                double voltage = source.Kind == CircuitKind.Ammeter ? 0 : source.Value;
                if (!order) voltage = -voltage;
                if (imposed.TryGetValue(key, out double other) && Math.Abs(voltage - other) > 1e-10)
                    return new CircuitResult(CircuitStatus.InconsistentSources);
                imposed[key] = voltage;
            }
            string ground = union.Root(groundTerminal);
            string[] roots = components.SelectMany(c => new[] { union.Root(c.A), union.Root(c.B) }).Distinct()
                .OrderBy(v => v, StringComparer.Ordinal).ToArray();
            var reached = new HashSet<string>(StringComparer.Ordinal) { ground };
            bool added;
            do
            {
                added = false;
                foreach (var component in components)
                {
                    string a = union.Root(component.A), b = union.Root(component.B);
                    if (reached.Contains(a)) added |= reached.Add(b);
                    if (reached.Contains(b)) added |= reached.Add(a);
                }
            } while (added);
            if (roots.Any(r => !reached.Contains(r))) return new CircuitResult(CircuitStatus.FloatingNetwork);
            string[] nodes = roots.Where(r => r != ground).ToArray();
            var index = nodes.Select((n, i) => new { n, i }).ToDictionary(v => v.n, v => v.i);
            int size = nodes.Length + sources.Length;
            var matrix = new double[size, size]; var rhs = new double[size];
            int Node(string terminal) => union.Root(terminal) == ground ? -1 : index[union.Root(terminal)];
            foreach (var component in components.Where(c => c.Kind == CircuitKind.Resistor))
            {
                int a = Node(component.A), b = Node(component.B); double g = 1 / component.Value;
                if (a >= 0) matrix[a, a] += g;
                if (b >= 0) matrix[b, b] += g;
                if (a >= 0 && b >= 0) { matrix[a, b] -= g; matrix[b, a] -= g; }
            }
            for (int i = 0; i < sources.Length; i++)
            {
                int a = Node(sources[i].A), b = Node(sources[i].B), s = nodes.Length + i;
                if (a >= 0) { matrix[a, s] = 1; matrix[s, a] = 1; }
                if (b >= 0) { matrix[b, s] = -1; matrix[s, b] = -1; }
                rhs[s] = sources[i].Kind == CircuitKind.Ammeter ? 0 : sources[i].Value;
            }
            double[]? solution = LinearSolve(matrix, rhs, out bool inconsistent);
            if (solution == null) return new CircuitResult(inconsistent ? CircuitStatus.InconsistentSources : CircuitStatus.NumericallySingular);
            double residual = 0;
            for (int row = 0; row < size; row++)
            {
                double value = 0, scale = Math.Abs(rhs[row]);
                for (int column = 0; column < size; column++) { double term = matrix[row, column] * solution[column]; value += term; scale += Math.Abs(term); }
                residual = Math.Max(residual, Math.Abs(value - rhs[row]) / Math.Max(1e-12, scale));
            }
            if (!Numbers.Finite(residual) || residual > 1e-9) return new CircuitResult(CircuitStatus.NumericallySingular);
            var voltages = new Dictionary<string, double>(StringComparer.Ordinal);
            foreach (string terminal in terminals)
            {
                string root = union.Root(terminal);
                if (root == ground) voltages[terminal] = 0;
                else if (index.TryGetValue(root, out int node)) voltages[terminal] = solution[node];
            }
            var readings = new Dictionary<string, BranchReading>(StringComparer.Ordinal);
            foreach (var component in components)
            {
                double voltage = voltages[component.A] - voltages[component.B];
                double current = component.Kind == CircuitKind.Resistor ? voltage / component.Value :
                    solution[nodes.Length + Array.FindIndex(sources, s => s.Id == component.Id)];
                double power = voltage * current;
                if (!Numbers.Finite(voltage) || !Numbers.Finite(current) || !Numbers.Finite(power)) return new CircuitResult(CircuitStatus.NumericallySingular);
                readings[component.Id] = new BranchReading(voltage, current, power,
                    component.Kind == CircuitKind.Resistor && power > component.RatedPowerW);
            }
            bool open = sources.All(s => Math.Abs(readings[s.Id].CurrentA) < 1e-12) &&
                !HasPassivePath(union.Root(powerSources[0].A), union.Root(powerSources[0].B), components, union);
            return new CircuitResult(open ? CircuitStatus.OpenCircuit : CircuitStatus.Solved, voltages, readings, residual);
        }

        private static bool HasPassivePath(string start, string end, CircuitComponent[] components, Union union)
        {
            var reached = new HashSet<string> { start }; bool changed;
            do
            {
                changed = false;
                foreach (var c in components.Where(c => c.Kind != CircuitKind.VoltageSource))
                {
                    string a = union.Root(c.A), b = union.Root(c.B);
                    if (reached.Contains(a)) changed |= reached.Add(b);
                    if (reached.Contains(b)) changed |= reached.Add(a);
                }
            } while (changed);
            return reached.Contains(end);
        }

        private static double[]? LinearSolve(double[,] original, double[] originalRhs, out bool inconsistent)
        {
            inconsistent = false; int n = originalRhs.Length;
            var a = (double[,])original.Clone(); var b = (double[])originalRhs.Clone();
            for (int column = 0; column < n; column++)
            {
                int pivot = column;
                for (int row = column + 1; row < n; row++) if (Math.Abs(a[row, column]) > Math.Abs(a[pivot, column])) pivot = row;
                if (Math.Abs(a[pivot, column]) < 1e-14)
                {
                    for (int row = column; row < n; row++)
                    {
                        double norm = 0; for (int j = column; j < n; j++) norm = Math.Max(norm, Math.Abs(a[row, j]));
                        if (norm < 1e-14 && Math.Abs(b[row]) > 1e-10) inconsistent = true;
                    }
                    return null;
                }
                for (int j = column; j < n; j++) { double temp = a[column, j]; a[column, j] = a[pivot, j]; a[pivot, j] = temp; }
                double swap = b[column]; b[column] = b[pivot]; b[pivot] = swap;
                for (int row = column + 1; row < n; row++)
                {
                    double factor = a[row, column] / a[column, column];
                    for (int j = column; j < n; j++) a[row, j] -= factor * a[column, j];
                    b[row] -= factor * b[column];
                }
            }
            var x = new double[n];
            for (int row = n - 1; row >= 0; row--)
            { double value = b[row]; for (int j = row + 1; j < n; j++) value -= a[row, j] * x[j]; x[row] = value / a[row, row]; }
            return x.All(Numbers.Finite) ? x : null;
        }
    }
}
