using System;
using System.Collections.Generic;
using System.Linq;

namespace VirtualStem.Science
{
    public sealed class Junction
    {
        public string Id {get;} public string A {get;} public string B {get;}
        public double ForwardV {get;} public double OnResistance {get;} public double RatedPower {get;}
        public Junction(string id,string a,string b,double forwardV,double onResistance,double ratedPower)
        {Id=id;A=a;B=b;ForwardV=forwardV;OnResistance=onResistance;RatedPower=ratedPower;}
    }
    public static class PiecewiseDc
    {
        public const string ModelVersion="dc-components-v2";
        public const double OffResistance=1e9;
        public static CircuitResult Solve(IEnumerable<string> terminalInput,IEnumerable<CircuitComponent> componentInput,
            IEnumerable<CircuitWire> wireInput,string ground,IEnumerable<Junction> junctionInput)
        {
            var terminals=terminalInput.ToArray();var components=componentInput.ToArray();var wires=wireInput.ToArray();var junctions=junctionInput.ToArray();
            if(!components.Any(c=>c.Kind==CircuitKind.VoltageSource))return new CircuitResult(CircuitStatus.EmptyCircuit);
            if(junctions.Length==0)return ResistiveDc.Solve(terminals,components,wires,ground);
            if(junctions.Length>8||junctions.Any(d=>!Numbers.Finite(d.ForwardV)||d.ForwardV<=0||!Numbers.Finite(d.OnResistance)||d.OnResistance<=0||
                !Numbers.Finite(d.RatedPower)||d.RatedPower<=0||!terminals.Contains(d.A)||!terminals.Contains(d.B))||
                junctions.Select(d=>d.Id).Concat(components.Select(c=>c.Id)).Distinct().Count()!=junctions.Length+components.Length)
                return new CircuitResult(CircuitStatus.InvalidInput);
            CircuitResult? failure=null;
            for(int mask=0;mask<(1<<junctions.Length);mask++)
            {
                var allTerminals=terminals.ToList();var branches=components.ToList();
                for(int i=0;i<junctions.Length;i++)
                {
                    var d=junctions[i];bool on=(mask&(1<<i))!=0;
                    if(on)
                    {
                        string node=d.Id+".__junction";allTerminals.Add(node);
                        branches.Add(new CircuitComponent(d.Id+".__drop",CircuitKind.VoltageSource,d.A,node,d.ForwardV));
                        branches.Add(new CircuitComponent(d.Id,CircuitKind.Resistor,node,d.B,d.OnResistance,d.RatedPower));
                    }
                    else branches.Add(new CircuitComponent(d.Id,CircuitKind.Resistor,d.A,d.B,OffResistance,d.RatedPower));
                }
                var result=ResistiveDc.Solve(allTerminals,branches,wires,ground);
                if(!result.HasReadings){failure=result;continue;}
                bool valid=true;
                for(int i=0;i<junctions.Length;i++)
                {
                    var d=junctions[i];double v=result.NodeVoltages[d.A]-result.NodeVoltages[d.B];
                    if((mask&(1<<i))!=0)valid &=result.Branches[d.Id].CurrentA>=-1e-10&&v>=d.ForwardV-1e-8;
                    else valid &=v<=d.ForwardV+1e-8;
                }
                if(!valid)continue;
                var readings=result.Branches.Where(p=>!p.Key.EndsWith(".__drop",StringComparison.Ordinal)).ToDictionary(p=>p.Key,p=>p.Value);
                foreach(var d in junctions)
                {
                    double v=result.NodeVoltages[d.A]-result.NodeVoltages[d.B],current=result.Branches[d.Id].CurrentA;
                    readings[d.Id]=new BranchReading(v,current,v*current,v*current>d.RatedPower);
                }
                var nodes=result.NodeVoltages.Where(p=>terminals.Contains(p.Key)).ToDictionary(p=>p.Key,p=>p.Value);
                return new CircuitResult(result.Status,nodes,readings,result.Residual);
            }
            return failure??new CircuitResult(CircuitStatus.NumericallySingular);
        }
    }
}
