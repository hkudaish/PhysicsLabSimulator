using System;
using System.Collections.Generic;

namespace VirtualStem.Science
{
    public static class SolutionProperties
    {
        public const string ModelVersion="ideal-single-solute-v2";
        private static readonly HashSet<string> Supported=new HashSet<string>(StringComparer.Ordinal)
        {Dilution.IdealSolute,"HCl","NaOH","H2SO4","NaCl","CuSO4","AgNO3","BaCl2","FeCl3","KI","Pb(NO3)2","C12H22O11"};
        public static bool Supports(string? solute)=>solute!=null&&Supported.Contains(solute);
        // Ideal, fully dissociated monoprotic acid/base in water at 25 C. Includes water autoionization.
        public static double? Ph(Vessel vessel)
        {
            if(vessel.VolumeL==0)return null;
            if(vessel.Moles==0)return 7;
            double c=vessel.ConcentrationMolL!.Value;
            if(c>10)return null;
            if(vessel.SoluteId!="HCl"&&vessel.SoluteId!="NaOH")return null;
            double dominant=(c+Math.Sqrt(c*c+4e-14))/2;
            return vessel.SoluteId=="HCl"?-Math.Log10(dominant):14+Math.Log10(dominant);
        }
    }
}
