using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using VirtualStem.Science;

namespace VirtualStem.Client
{
    public sealed class ToolState
    {
        public string Id { get; set; } = "";
        public string Kind { get; set; } = "";
        public double X { get; set; }
        public double Z { get; set; }
        public double Yaw { get; set; }
        public double Value { get; set; }
        public double CapacityL { get; set; } = 0.25;
        public double VolumeL { get; set; }
        public double Moles { get; set; }
        public string? SoluteId { get; set; }
        public string Mode { get; set; } = "voltage";
        public string? Target { get; set; }
        public Vessel Vessel() => new Vessel(Id, CapacityL, VolumeL, Moles, SoluteId);
    }
    public sealed class WireState
    {
        public string Id { get; set; } = "";
        public string A { get; set; } = "";
        public string B { get; set; } = "";
    }
    public sealed class SessionData
    {
        public string Experiment { get; set; } = "chemistry";
        public long Revision { get; set; }
        public List<ToolState> Tools { get; set; } = new();
        public List<WireState> Wires { get; set; } = new();
        public double CameraYaw { get; set; } = 0.1;
        public double CameraPitch { get; set; } = 0.62;
        public double CameraRadius { get; set; } = 8.5;
        public string SelectedId { get; set; } = "";
        public List<string> Actions { get; set; } = new();
    }
    public sealed class WorkspaceData
    {
        public int SchemaVersion { get; set; } = 1;
        public string ChemistryModel { get; set; } = ChemistryState.ModelVersion;
        public string ElectricalModel { get; set; } = ResistiveDc.ModelVersion;
        public string ClientVersion { get; set; } = "0.2.0";
        public string ActiveExperiment { get; set; } = "chemistry";
        public SessionData Chemistry { get; set; } = Session.New("chemistry");
        public SessionData Electricity { get; set; } = Session.New("electricity");
        public List<SessionData> ChemistryUndo { get; set; } = new();
        public List<SessionData> ElectricityUndo { get; set; } = new();
    }

    public static class Session
    {
        public const string WorkspaceVersion = "phase1-workspace-v1";
        private static readonly JsonSerializerOptions Options = new()
        {
            UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
            MaxDepth = 32
        };
        public static bool Chemical(string kind) => kind == "stock" || kind == "beaker" || kind == "cylinder" || kind == "water";
        public static SessionData New(string experiment)
        {
            if (experiment == "chemistry") return new SessionData
            {
                Experiment = experiment, SelectedId = "stock",
                Tools = new List<ToolState>
                {
                    new() { Id="stock",Kind="stock",X=-1.8,Z=-0.3,CapacityL=0.25,VolumeL=0.1,Moles=0.2,SoluteId=Dilution.IdealSolute,Value=2 },
                    new() { Id="beaker",Kind="beaker",X=0,Z=-0.3,CapacityL=0.25 },
                    new() { Id="water",Kind="water",X=1.8,Z=-0.3,CapacityL=0.5,VolumeL=0.5 },
                    new() { Id="sensor",Kind="meter",X=0,Z=1.1,Mode="concentration",Target="beaker" }
                }
            };
            return new SessionData
            {
                Experiment="electricity",SelectedId="battery",
                Tools=new List<ToolState>
                {
                    new(){Id="battery",Kind="battery",X=-1.8,Z=-0.3,Value=9},
                    new(){Id="resistor",Kind="resistor",X=0,Z=-0.3,Value=1000},
                    new(){Id="multimeter",Kind="meter",X=1.8,Z=0.5,Mode="voltage"}
                }
            };
        }
        public static ChemistryState Chemistry(SessionData data) => new(data.Tools.Where(t=>Chemical(t.Kind)).Select(t=>t.Vessel()));
        public static void ApplyChemistry(SessionData data, ChemistryState state)
        {
            foreach(var tool in data.Tools.Where(t=>Chemical(t.Kind)))
            { var vessel=state.Vessels[tool.Id];tool.VolumeL=vessel.VolumeL;tool.Moles=vessel.Moles;tool.SoluteId=vessel.SoluteId; }
        }
        public static CircuitResult Electricity(SessionData data)
        {
            var terminals=data.Tools.SelectMany(t=>new[]{t.Id+".a",t.Id+".b"}).ToArray();
            var components=data.Tools.Where(t=>t.Kind=="battery"||t.Kind=="resistor"||(t.Kind=="meter"&&t.Mode=="current"))
                .Select(t=>new CircuitComponent(t.Id,t.Kind=="battery"?CircuitKind.VoltageSource:t.Kind=="resistor"?CircuitKind.Resistor:CircuitKind.Ammeter,
                    t.Id+".a",t.Id+".b",t.Kind=="meter"?0:t.Value)).ToArray();
            string ground=(data.Tools.FirstOrDefault(t=>t.Kind=="battery")?.Id??"missing")+".b";
            return ResistiveDc.Solve(terminals,components,data.Wires.Select(w=>new CircuitWire(w.A,w.B)),ground);
        }
        public static SessionData Clone(SessionData data) => JsonSerializer.Deserialize<SessionData>(JsonSerializer.Serialize(data,Options),Options)!;
        public static string Encode(WorkspaceData data) { Validate(data);return JsonSerializer.Serialize(data,Options); }
        public static WorkspaceData Decode(string json)
        {
            if(json.Length>4*1024*1024)throw new InvalidDataException("Draft too large.");
            var data=JsonSerializer.Deserialize<WorkspaceData>(json,Options)??throw new InvalidDataException("Empty draft.");
            Validate(data);return data;
        }
        public static void Validate(WorkspaceData workspace)
        {
            if(workspace.SchemaVersion!=1||workspace.ChemistryModel!=ChemistryState.ModelVersion||workspace.ElectricalModel!=ResistiveDc.ModelVersion||
                (workspace.ActiveExperiment!="chemistry"&&workspace.ActiveExperiment!="electricity")||workspace.Chemistry==null||workspace.Electricity==null||
                workspace.ChemistryUndo==null||workspace.ElectricityUndo==null||workspace.ChemistryUndo.Count>20||workspace.ElectricityUndo.Count>20)
                throw new InvalidDataException("Unsupported or invalid draft version; current work preserved.");
            ValidateState(workspace.Chemistry,"chemistry");ValidateState(workspace.Electricity,"electricity");
            foreach(var state in workspace.ChemistryUndo)ValidateState(state,"chemistry");
            foreach(var state in workspace.ElectricityUndo)ValidateState(state,"electricity");
        }
        private static void ValidateState(SessionData data,string expected)
        {
            if(data==null||data.Experiment!=expected||data.Revision<0||data.Tools==null||data.Wires==null||data.Tools.Count>12||data.Wires.Count>48||
                data.Actions==null||data.Actions.Count>100||data.Actions.Any(a=>a==null||a.Length>500)||
                !Numbers.Finite(data.CameraYaw)||!Numbers.Finite(data.CameraPitch)||!Numbers.Finite(data.CameraRadius)||
                data.CameraPitch<0.3||data.CameraPitch>1.4||data.CameraRadius<5||data.CameraRadius>14)
                throw new InvalidDataException("Invalid experiment draft.");
            var ids=new HashSet<string>(StringComparer.Ordinal);
            foreach(var tool in data.Tools)
            {
                if(tool==null||!ValidId(tool.Id)||!ids.Add(tool.Id)||!Numbers.Finite(tool.X)||!Numbers.Finite(tool.Z)||Math.Abs(tool.X)>3||Math.Abs(tool.Z)>1.5||
                    !Numbers.Finite(tool.Yaw)||!Numbers.Finite(tool.Value)||
                    (expected=="chemistry"&&!(Chemical(tool.Kind)||tool.Kind=="meter"))||
                    (expected=="electricity"&&!(tool.Kind=="battery"||tool.Kind=="resistor"||tool.Kind=="meter"))||
                    (tool.Kind=="battery"&&(tool.Value<0||tool.Value>12))||
                    (tool.Kind=="resistor"&&(tool.Value<10||tool.Value>10000))||
                    (tool.Kind=="meter"&&(expected=="chemistry"?tool.Mode!="concentration":tool.Mode!="voltage"&&tool.Mode!="current")))
                    throw new InvalidDataException("Invalid tool data.");
                if(Chemical(tool.Kind))
                {
                    var vessel=tool.Vessel();
                    if(vessel.Moles>0&&vessel.SoluteId!=Dilution.IdealSolute)throw new InvalidDataException("Unsupported solution.");
                }
            }
            var wireIds=new HashSet<string>();var pairs=new HashSet<string>();
            foreach(var wire in data.Wires)
            {
                if(wire==null||!ValidId(wire.Id)||!wireIds.Add(wire.Id)||wire.A==wire.B||!TerminalExists(data,wire.A)||!TerminalExists(data,wire.B))
                    throw new InvalidDataException("Invalid wire endpoints.");
                string pair=string.CompareOrdinal(wire.A,wire.B)<0?wire.A+"|"+wire.B:wire.B+"|"+wire.A;
                if(!pairs.Add(pair))throw new InvalidDataException("Duplicate connection.");
            }
            if(expected=="chemistry"&&data.Wires.Count!=0)throw new InvalidDataException("Unexpected chemistry wiring.");
        }
        public static bool TerminalExists(SessionData data,string terminal) => data.Tools.Any(t=>terminal==t.Id+".a"||terminal==t.Id+".b");
        public static bool ValidId(string id) => !string.IsNullOrWhiteSpace(id)&&id.Length<=64&&id.All(c=>char.IsAsciiLetterOrDigit(c)||c=='-'||c=='_');
        public static bool CanPlace(SessionData data,string id,double x,double z) => Numbers.Finite(x)&&Numbers.Finite(z)&&Math.Abs(x)<=3&&Math.Abs(z)<=1.5&&
            data.Tools.Where(t=>t.Id!=id).All(t=>(t.X-x)*(t.X-x)+(t.Z-z)*(t.Z-z)>=1.15*1.15);
    }
}
