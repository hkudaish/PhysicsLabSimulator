using System.Text.Json;
using VirtualStem.Client;
using VirtualStem.Science;

namespace VirtualStem.Browser;

public sealed record Command(string Action,JsonElement Workspace,string? Tool=null,string? Kind=null,string? Target=null,
    double? Value=null,double? X=null,double? Z=null,string? Content=null,string? A=null,string? B=null,SessionData? Snapshot=null);

public static class Bench
{
    public static readonly string[] Reagents={"HCl","NaOH","H2SO4","NaCl","CuSO4","AgNO3","BaCl2","FeCl3","KI","Pb(NO3)2","C12H22O11"};
    public static object Execute(Command command)
    {
        var workspace=Session.Decode(command.Workspace.GetRawText());
        if(command.Action=="view")return View(workspace);
        var current=workspace.ActiveExperiment=="chemistry"?workspace.Chemistry:workspace.Electricity;
        var history=workspace.ActiveExperiment=="chemistry"?workspace.ChemistryUndo:workspace.ElectricityUndo;
        var before=Session.Clone(current);
        ToolState Tool()=>current.Tools.FirstOrDefault(t=>t.Id==command.Tool)??throw new ArgumentException("Select an existing tool.");
        double Value()=>command.Value is double value&&double.IsFinite(value)?value:throw new ArgumentException("Enter a finite value.");
        void Place(ToolState tool)
        {
            if(current.Tools.Count>=12)throw new ArgumentException("Maximum 12 tools on one bench. Remove a tool first.");
            if(command.X!=null||command.Z!=null)
            {
                double x=command.X??double.NaN,z=command.Z??double.NaN;
                if(!Session.CanPlace(current,"new",x,z))throw new ArgumentException("That placement overlaps equipment or leaves the workbench.");
                tool.X=x;tool.Z=z;return;
            }
            var spots=from x in Enumerable.Range(0,25) from z in Enumerable.Range(0,13) let px=-3+x*0.25 let pz=-1.5+z*0.25
                      where Session.CanPlace(current,"new",px,pz) orderby current.Tools.Count==0?0:current.Tools.Min(t=>Math.Pow(t.X-px,2)+Math.Pow(t.Z-pz,2)) descending select (px,pz);
            if(!spots.Any())throw new ArgumentException("No free placement area. Rearrange or remove tools.");
            (tool.X,tool.Z)=spots.First();
        }
        switch(command.Action)
        {
            case "undo":
                if(history.Count==0)throw new ArgumentException("Nothing to undo.");
                current=history[^1];history.RemoveAt(history.Count-1);break;
            case "redo":
            case "restore":
                if(command.Snapshot==null||command.Snapshot.Experiment!=current.Experiment)throw new ArgumentException("No valid redo state for this laboratory.");
                Session.Validate(new WorkspaceData
                {
                    Chemistry=current.Experiment=="chemistry"?command.Snapshot:Session.New("chemistry"),
                    Electricity=current.Experiment=="electricity"?command.Snapshot:Session.New("electricity")
                });
                current=Session.Clone(command.Snapshot);break;
            case "reset":current=Session.New(current.Experiment);break;
            case "clear":current=Session.New(current.Experiment);current.Tools.Clear();current.Wires.Clear();current.SelectedId="";break;
            case "add":
            {
                string kind=command.Kind??"";
                if(current.Experiment=="chemistry"?!ToolCatalog.Chemistry(kind):!ToolCatalog.Circuit(kind))throw new ArgumentException("Tool belongs to the other lab.");
                var tool=ToolCatalog.Create(kind,current.Experiment);
                Place(tool);
                if(kind=="stock"&&command.Content!=null)
                {
                    if(!Reagents.Contains(command.Content))throw new ArgumentException("Unknown reagent.");
                    tool.VolumeL=0.1;tool.Moles=0.01;tool.SoluteId=command.Content;
                }
                if(kind=="element"&&command.Content!=null)tool.Mode=Elements.Find(command.Content)?.Symbol??throw new ArgumentException("Unknown element.");
                if(kind is "meter" or "phmeter" or "thermometer")tool.Target=current.Tools.FirstOrDefault(t=>Session.Chemical(t.Kind)&&t.Kind!="stock"&&t.Kind!="water")?.Id;
                if(kind=="balance")tool.Target=current.Tools.FirstOrDefault(t=>t.Kind=="element")?.Id;
                current.Tools.Add(tool);current.SelectedId=tool.Id;break;
            }
            case "duplicate":
            {
                var original=Tool();
                var copy=Session.Clone(new SessionData{Tools=new(){original}}).Tools[0];
                copy.Id=Guid.NewGuid().ToString("N");Place(copy);
                // A duplicated container is empty; duplicating a view never manufactures its solution.
                if(Session.Chemical(copy.Kind)){copy.VolumeL=0;copy.Moles=0;copy.SoluteId=null;}
                current.Tools.Add(copy);current.SelectedId=copy.Id;break;
            }
            case "empty":
            {
                var tool=Tool();
                if(current.Experiment!="chemistry"||!Session.Chemical(tool.Kind)||tool.VolumeL<=0)throw new ArgumentException("Select a nonempty vessel to discard its contents.");
                current.Actions.Add(FormattableString.Invariant($"Discard to waste: {tool.VolumeL:G17} L; {tool.Moles:G17} mol; {tool.SoluteId??"water"}"));
                tool.VolumeL=0;tool.Moles=0;tool.SoluteId=null;break;
            }
            case "disconnect":
            {
                var tool=Tool();
                if(current.Experiment!="electricity"||!ToolCatalog.Electrical(tool.Kind))throw new ArgumentException("Select a wired circuit component.");
                if(current.Wires.RemoveAll(w=>w.A.StartsWith(tool.Id+".",StringComparison.Ordinal)||w.B.StartsWith(tool.Id+".",StringComparison.Ordinal))==0)
                    throw new ArgumentException("This component has no leads to disconnect.");
                break;
            }
            case "remove":
            {
                var tool=Tool();current.Tools.Remove(tool);current.Wires.RemoveAll(w=>w.A.StartsWith(tool.Id+".",StringComparison.Ordinal)||w.B.StartsWith(tool.Id+".",StringComparison.Ordinal));
                foreach(var t in current.Tools.Where(t=>t.Target==tool.Id))t.Target=null;
                current.SelectedId=current.Tools.FirstOrDefault()?.Id??"";break;
            }
            case "move":
            {
                var tool=Tool();double x=command.X??double.NaN,z=command.Z??double.NaN;
                if(!Session.CanPlace(current,tool.Id,x,z))throw new ArgumentException("Keep equipment on the bench and clear of other objects.");
                tool.X=x;tool.Z=z;break;
            }
            case "rotate":Tool().Yaw+=Math.PI/6;break;
            case "value":
            {
                var tool=Tool();double value=Value();
                bool valid=tool.Kind switch
                {
                    "battery"=>value>=0&&value<=12,
                    "resistor" or "rheostat" or "bulb"=>value>=10&&value<=10000,
                    "diode" or "led"=>value>=0.1&&value<=5,
                    "element"=>value>=0.0001&&value<=1,
                    "capacitor"=>value>=0.000001&&value<=1,
                    "inductor"=>value>=0.000001&&value<=10,
                    _=>false
                };
                if(!valid)throw new ArgumentException("Value is outside this component's range.");
                tool.Value=value;break;
            }
            case "mode":
            {
                var tool=Tool();string mode=command.Content??"";
                if(tool.Kind=="switch"&&(mode=="open"||mode=="closed")||ToolCatalog.Logic(tool.Kind)&&(mode=="00"||mode=="01"||mode=="10"||mode=="11")||
                    current.Experiment=="electricity"&&tool.Kind=="meter"&&(mode=="voltage"||mode=="current"))tool.Mode=mode;
                else throw new ArgumentException("Unsupported instrument mode.");break;
            }
            case "target":
            {
                var tool=Tool();var target=current.Tools.FirstOrDefault(t=>t.Id==command.Target)??throw new ArgumentException("Choose a target.");
                if(current.Experiment!="chemistry"||!(tool.Kind=="balance"?target.Kind=="element":tool.Kind is "meter" or "phmeter" or "thermometer"&&Session.Chemical(target.Kind)))throw new ArgumentException("Unsupported instrument target.");
                tool.Target=target.Id;break;
            }
            case "transfer":
            {
                var tool=Tool();double volume=Value()/1000;
                if(current.Experiment!="chemistry"||!Session.Chemical(tool.Kind))throw new ArgumentException("Select a source vessel.");
                if(tool.Kind=="volpipette"&&Math.Abs(volume-0.025)>1e-10)throw new ArgumentException("The volumetric pipette delivers its complete 25 mL aliquot.");
                var result=Dilution.Transfer(Session.Chemistry(current),tool.Id,command.Target??"",volume);
                if(!result.Applied)throw new ArgumentException("Transfer rejected: "+result.Status+". Different solutes cannot be mixed.");
                Session.ApplyChemistry(current,result.State);break;
            }
            case "wire":
            {
                string a=command.A??"",b=command.B??"";
                if(current.Experiment!="electricity"||a==b||!Session.TerminalExists(current,a)||!Session.TerminalExists(current,b))throw new ArgumentException("Choose two different circuit terminals.");
                if(current.Wires.Any(w=>w.A==a&&w.B==b||w.A==b&&w.B==a))throw new ArgumentException("These terminals are already connected.");
                if(current.Wires.Count>=64)throw new ArgumentException("Maximum 64 leads.");
                current.Wires.Add(new WireState{Id=Guid.NewGuid().ToString("N"),A=a,B=b});break;
            }
            case "unwire":
                if(current.Wires.RemoveAll(w=>w.Id==command.Content)==0)throw new ArgumentException("Choose a lead to disconnect.");break;
            case "rewire":
            {
                var wire=current.Wires.FirstOrDefault(w=>w.Id==command.Content)??throw new ArgumentException("Select an existing lead.");
                string a=command.A??wire.A,b=command.B??wire.B;
                if(current.Experiment!="electricity"||a==b||!Session.TerminalExists(current,a)||!Session.TerminalExists(current,b))throw new ArgumentException("Choose valid, different endpoints.");
                if(current.Wires.Any(w=>w.Id!=wire.Id&&(w.A==a&&w.B==b||w.A==b&&w.B==a)))throw new ArgumentException("These terminals are already connected.");
                wire.A=a;wire.B=b;break;
            }
            case "preset":
                if(current.Experiment!="electricity")throw new ArgumentException("Open the circuit lab first.");
                current=Preset(command.Content??"");break;
            default:throw new ArgumentException("Unknown command.");
        }
        if(command.Action!="undo")
        {
            history.Add(before);if(history.Count>20)history.RemoveAt(0);
            current.Revision=before.Revision+1;current.Actions.Add(command.Action);while(current.Actions.Count>100)current.Actions.RemoveAt(0);
        }
        if(workspace.ActiveExperiment=="chemistry")workspace.Chemistry=current;else workspace.Electricity=current;
        Session.TrackModels(workspace);workspace.ClientVersion="0.5.0-browser";Session.Validate(workspace);
        return View(workspace);
    }
    public static object View(WorkspaceData workspace)
    {
        Session.Validate(workspace);
        var current=workspace.ActiveExperiment=="chemistry"?workspace.Chemistry:workspace.Electricity;
        bool chemical=workspace.ActiveExperiment=="chemistry";
        var circuit=chemical?null:Session.Electricity(current);
        var readings=new Dictionary<string,object>();
        foreach(var tool in current.Tools)
        {
            var target=current.Tools.FirstOrDefault(t=>t.Id==tool.Target);
            bool nearby=target!=null&&Math.Pow(tool.X-target.X,2)+Math.Pow(tool.Z-target.Z,2)<=1.65*1.65;
            object? instrument=null;
            if(chemical&&tool.Kind is "meter" or "phmeter" or "thermometer" or "balance")
            {
                double? value=nearby&&target!=null?(tool.Kind switch
                {
                    "meter" when Session.Chemical(target.Kind)=>target.Vessel().ConcentrationMolL,
                    "phmeter" when Session.Chemical(target.Kind)=>SolutionProperties.Ph(target.Vessel()),
                    "thermometer" when Session.Chemical(target.Kind)=>25,
                    "balance" when target.Kind=="element"=>target.Value*Elements.Find(target.Mode)!.Mass,
                    _=>null
                }):null;
                instrument=new {value,unit=tool.Kind switch{"meter"=>"mol/L","phmeter"=>"pH","balance"=>"g",_=>"°C"},nearby};
            }
            if(!chemical&&tool.Kind is "meter" or "voltmeter" or "ammeter")
            {
                bool currentMode=tool.Kind=="ammeter"||tool.Mode=="current";
                double? value=currentMode?circuit!.Branches.GetValueOrDefault(tool.Id)?.CurrentA:circuit!.VoltageBetween(tool.Id+".a",tool.Id+".b");
                instrument=new {value,unit=currentMode?"A":"V",nearby=true};
            }
            var branch=circuit?.Branches.GetValueOrDefault(tool.Id);
            readings[tool.Id]=new
            {
                concentration=Session.Chemical(tool.Kind)?tool.Vessel().ConcentrationMolL:null,
                ph=Session.Chemical(tool.Kind)?SolutionProperties.Ph(tool.Vessel()):null,
                mass=tool.Kind=="element"?(double?)(tool.Value*Elements.Find(tool.Mode)!.Mass):null,
                output=ToolCatalog.Logic(tool.Kind)?(bool?)ToolCatalog.Output(tool.Kind,tool.Mode[0]=='1',tool.Mode[1]=='1'):null,
                voltage=circuit?.VoltageBetween(tool.Id+".a",tool.Id+".b"),
                charge=tool.Kind=="capacitor"?tool.Value*circuit?.VoltageBetween(tool.Id+".a",tool.Id+".b"):null,
                branch,instrument
            };
        }
        return new {workspace,readings,circuit= circuit==null?null:new {status=circuit.Status.ToString(),circuit.Residual,hasReadings=circuit.HasReadings}};
    }
    public static SessionData Preset(string preset)
    {
        if(!new[]{"series","parallel","switch-lamp","led","capacitor"}.Contains(preset))throw new ArgumentException("Unknown circuit preset.");
        var state=Session.New("electricity");state.Tools.Clear();state.Wires.Clear();
        var source=ToolCatalog.Create("battery","electricity");source.Id="battery";source.X=-1.8;source.Z=-0.4;
        var resistor=ToolCatalog.Create(preset=="switch-lamp"?"bulb":"resistor","electricity");resistor.Id="resistor";resistor.Z=-0.4;
        var extra=ToolCatalog.Create(preset switch{"led"=>"led","switch-lamp"=>"switch","capacitor"=>"capacitor",_=>"resistor"},"electricity");extra.Id="extra";extra.X=1.8;extra.Z=-0.4;
        if(extra.Kind=="switch")extra.Mode="closed";
        var meter=ToolCatalog.Create("meter","electricity");meter.Id="multimeter";meter.Z=1.1;
        state.Tools.AddRange(new[]{source,resistor,extra,meter});state.SelectedId="extra";
        void Wire(string a,string b)=>state.Wires.Add(new WireState{Id=Guid.NewGuid().ToString("N"),A=a,B=b});
        Wire("battery.a","resistor.a");
        if(preset=="parallel"){Wire("resistor.b","battery.b");Wire("battery.a","extra.a");Wire("extra.b","battery.b");}
        else{Wire("resistor.b","extra.a");Wire("extra.b","battery.b");}
        Wire("multimeter.a","resistor.a");Wire("multimeter.b","resistor.b");return state;
    }
}
