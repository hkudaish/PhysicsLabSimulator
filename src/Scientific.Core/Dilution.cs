using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace VirtualStem.Science
{
    public static class Numbers
    {
        public static bool Finite(double value) => !double.IsNaN(value) && !double.IsInfinity(value);
    }

    public sealed class Vessel
    {
        public string Id { get; }
        public double CapacityL { get; }
        public double VolumeL { get; }
        public double Moles { get; }
        public string? SoluteId { get; }
        public double? ConcentrationMolL => VolumeL == 0 ? (double?)null : Moles / VolumeL;
        public Vessel(string id, double capacityL, double volumeL, double moles, string? soluteId = null)
        {
            if (string.IsNullOrWhiteSpace(id) || !Numbers.Finite(capacityL) || capacityL <= 0 ||
                !Numbers.Finite(volumeL) || volumeL < 0 || volumeL > capacityL ||
                !Numbers.Finite(moles) || moles < 0 || (volumeL == 0 && moles != 0) ||
                (volumeL > 0 && !Numbers.Finite(moles / volumeL)) ||
                (moles > 0 && string.IsNullOrWhiteSpace(soluteId)))
                throw new ArgumentException("Invalid vessel or quantities.");
            Id = id; CapacityL = capacityL; VolumeL = volumeL; Moles = moles; SoluteId = soluteId;
        }
    }

    public sealed class ChemistryState
    {
        public const string ModelVersion = "dilution-v1";
        public IReadOnlyDictionary<string, Vessel> Vessels { get; }
        public ChemistryState(IEnumerable<Vessel> vessels)
        {
            var copy = vessels.ToDictionary(v => v.Id, StringComparer.Ordinal);
            if (copy.Count > 32) throw new ArgumentException("Too many vessels for this model.");
            Vessels = new ReadOnlyDictionary<string, Vessel>(copy);
        }
    }

    public enum ChemistryStatus { Applied, NoChange, InvalidInput, InsufficientVolume, CapacityExceeded, UnsupportedModel }

    public sealed class ChemistryResult
    {
        public ChemistryState State { get; }
        public ChemistryStatus Status { get; }
        public bool Applied => Status == ChemistryStatus.Applied;
        public ChemistryResult(ChemistryState state, ChemistryStatus status) { State = state; Status = status; }
    }

    public static class Dilution
    {
        public const string IdealSolute = "ideal-solute";
        public static ChemistryResult Transfer(ChemistryState state, string sourceId, string targetId, double volumeL)
        {
            if (!Numbers.Finite(volumeL) || volumeL < 0 || sourceId == targetId ||
                !state.Vessels.TryGetValue(sourceId, out var source) || !state.Vessels.TryGetValue(targetId, out var target))
                return new ChemistryResult(state, ChemistryStatus.InvalidInput);
            if ((source.Moles > 0 && !SolutionProperties.Supports(source.SoluteId)) || (target.Moles > 0 && !SolutionProperties.Supports(target.SoluteId)))
                return new ChemistryResult(state, ChemistryStatus.UnsupportedModel);
            if(source.Moles>0&&target.Moles>0&&source.SoluteId!=target.SoluteId)
                return new ChemistryResult(state,ChemistryStatus.UnsupportedModel);
            if (volumeL == 0) return new ChemistryResult(state, ChemistryStatus.NoChange);
            if (volumeL > source.VolumeL) return new ChemistryResult(state, ChemistryStatus.InsufficientVolume);
            if (target.VolumeL + volumeL > target.CapacityL) return new ChemistryResult(state, ChemistryStatus.CapacityExceeded);
            double amount = source.Moles * (volumeL / source.VolumeL);
            double remainingVolume = source.VolumeL - volumeL;
            double remainingMoles = volumeL == source.VolumeL ? 0 : source.Moles - amount;
            double targetMoles = target.Moles + amount;
            if (!Numbers.Finite(targetMoles)) return new ChemistryResult(state, ChemistryStatus.InvalidInput);
            var changed = state.Vessels.Values.Select(v => v.Id == sourceId
                ? new Vessel(v.Id, v.CapacityL, remainingVolume, remainingMoles, remainingMoles == 0 ? null : source.SoluteId)
                : v.Id == targetId ? new Vessel(v.Id, v.CapacityL, v.VolumeL + volumeL, targetMoles,
                    targetMoles > 0 ? (source.Moles>0?source.SoluteId:target.SoluteId) : null) : v);
            return new ChemistryResult(new ChemistryState(changed), ChemistryStatus.Applied);
        }

        public static ChemistryResult AddWater(ChemistryState state, string targetId, double volumeL)
        {
            if (!Numbers.Finite(volumeL) || volumeL < 0 || !state.Vessels.TryGetValue(targetId, out var target))
                return new ChemistryResult(state, ChemistryStatus.InvalidInput);
            if (target.Moles > 0 && !SolutionProperties.Supports(target.SoluteId)) return new ChemistryResult(state, ChemistryStatus.UnsupportedModel);
            if (volumeL == 0) return new ChemistryResult(state, ChemistryStatus.NoChange);
            if (target.VolumeL + volumeL > target.CapacityL) return new ChemistryResult(state, ChemistryStatus.CapacityExceeded);
            return new ChemistryResult(new ChemistryState(state.Vessels.Values.Select(v => v.Id == targetId
                ? new Vessel(v.Id, v.CapacityL, v.VolumeL + volumeL, v.Moles, v.SoluteId) : v)), ChemistryStatus.Applied);
        }
    }
}
