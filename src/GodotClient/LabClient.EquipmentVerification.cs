using Godot;
using System;
using System.Linq;
using System.Threading.Tasks;
using VirtualStem.Client;
using VirtualStem.Science;

public partial class LabClient
{
    private async Task VerifyExpandedEquipment()
    {
        string saved=Session.Encode(workspace);
        Require(Elements.All.Length==118,"all 118 elements available offline");
        OpenElements();await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);
        Require(GetChildren().OfType<Window>().Any(d=>d.Visible),"periodic browser opens as a real dialog");
        if(OS.GetCmdlineUserArgs().Contains("--capture"))await Capture("periodic-table");
        foreach(var d in GetChildren().OfType<Window>().ToArray()){RemoveChild(d);d.QueueFree();}
        OpenEquipment();await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);
        Require(GetChildren().OfType<Window>().Any(d=>d.Visible),"searchable equipment catalog opens");
        if(OS.GetCmdlineUserArgs().Contains("--capture"))await Capture("equipment-catalog");
        foreach(var d in GetChildren().OfType<Window>().ToArray()){RemoveChild(d);d.QueueFree();}
        workspace.ActiveExperiment="chemistry";workspace.Chemistry=Session.New("chemistry");Refresh(true);
        Require(AddTool("erlenmeyer"),"place Erlenmeyer flask from catalog");string flask=Current.SelectedId;
        SelectTool("stock");targetChoice.Select(targetIds.IndexOf(flask));amount.Value=25;actions["pour"].EmitSignal(Godot.Button.SignalName.Pressed);
        Near(Selected!.Moles,.05,"new flask receives conserved solute");
        amount.Value=75;actions["water"].EmitSignal(Godot.Button.SignalName.Pressed);Near(Selected!.Vessel().ConcentrationMolL!.Value,.5,"new flask dilution calculated");
        Undo();Near(Selected!.VolumeL,.025,"undo restores expanded vessel contents");
        // Keep a single known bench while testing precision metering and model restrictions.
        workspace.Chemistry=new SessionData{Experiment="chemistry",SelectedId="stock",Tools=new()
        {
            new(){Id="stock",Kind="stock",X=-1.8,Z=-.3,VolumeL=.1,Moles=.01,SoluteId="HCl"},
            new(){Id="pipette",Kind="volpipette",X=0,Z=-.3,CapacityL=.025},
            new(){Id="flask",Kind="erlenmeyer",X=1.8,Z=-.3},
            new(){Id="ph",Kind="phmeter",X=1.8,Z=1.1,Target="flask"}
        }};Refresh(true);
        targetChoice.Select(targetIds.IndexOf("pipette"));amount.Value=25;Pour(false);
        Require(Current.SelectedId=="pipette","precision pipette fills");
        targetChoice.Select(targetIds.IndexOf("flask"));amount.Value=10;string before=Session.Encode(workspace);Pour(false);
        Require(Session.Encode(workspace)==before,"volumetric pipette rejects partial delivery without mutation");
        amount.Value=25;Pour(false);Near(Current.Tools.First(t=>t.Id=="flask").Moles,.0025,"precision pipette delivers complete aliquot");
        SelectTool("ph");Require(readings.Text.Contains("1.000",StringComparison.Ordinal),"pH instrument reads supported acid calculation");
        workspace.Chemistry=new SessionData{Experiment="chemistry",SelectedId="sample",Tools=new()
        {new(){Id="sample",Kind="element",Mode="Cu",Value=.01,X=0,Z=0},new(){Id="balance",Kind="balance",Target="sample",X=1.2,Z=0}}};
        Refresh(true);SelectTool("balance");Require(readings.Text.Contains("0.6355",StringComparison.Ordinal),"virtual tared balance uses authoritative reference mass");
        workspace.ActiveExperiment="electricity";workspace.Electricity=Session.New("electricity");Refresh(true);
        LoadCircuitPreset("switch-lamp");Require(electrical!.Status==CircuitStatus.Solved,"switch and lamp preset solves");
        actions["toggle-switch"].EmitSignal(Godot.Button.SignalName.Pressed);Require(electrical!.Status==CircuitStatus.OpenCircuit,"switch UI opens circuit");
        actions["undo"].EmitSignal(Godot.Button.SignalName.Pressed);Require(electrical!.Status==CircuitStatus.Solved,"undo restores closed switch circuit");
        LoadCircuitPreset("led");Near(electrical!.Branches["extra"].CurrentA,7.0/1010,"LED preset uses piecewise solver");
        await ToSignal(GetTree(),SceneTree.SignalName.PhysicsFrame);await ToSignal(GetTree(),SceneTree.SignalName.PhysicsFrame);bench.SnapCamera();
        foreach(string terminal in new[]{"extra.a","extra.b"})Require(bench.Pick(bench.Camera.UnprojectPosition(bench.TerminalPosition(terminal))).terminal==terminal,"new component terminal can be clicked "+terminal);
        LoadCircuitPreset("parallel");Near(-electrical!.Branches["battery"].CurrentA,.018,"parallel preset branch sum");
        LoadCircuitPreset("capacitor");Require(electrical!.Status==CircuitStatus.OpenCircuit,"capacitor DC steady-state preset opens path");
        if(OS.GetCmdlineUserArgs().Contains("--capture"))await Capture("expanded-capacitor");
        Require(AddTool("and"),"logic trainer placed");actions["logic-0"].EmitSignal(Godot.Button.SignalName.Pressed);actions["logic-1"].EmitSignal(Godot.Button.SignalName.Pressed);
        Require(readings.Text.Contains("Y = 1",StringComparison.Ordinal),"logic input controls evaluate truth table");
        Require(await SaveWorkspace(),"save expanded catalog tools and modes");string expanded=Session.Encode(workspace);
        Require(await LoadWorkspace()&&Session.Encode(workspace)==expanded,"expanded tool state and controls survive SQLite reload");
        store.Save("expanded-verification",Session.WorkspaceVersion,expanded);
        // Render every catalog model in bounded batches. This catches missing meshes/resources without filling one bench.
        foreach(string experiment in new[]{"chemistry","electricity"})
        {
            var defs=ToolCatalog.All.Where(d=>experiment=="chemistry"?ToolCatalog.Chemistry(d.Kind):ToolCatalog.Circuit(d.Kind)).ToArray();
            for(int start=0;start<defs.Length;start+=8)
            {
                var state=new SessionData{Experiment=experiment};
                foreach(var def in defs.Skip(start).Take(8))
                {
                    var tool=ToolCatalog.Create(def.Kind,experiment);int index=state.Tools.Count;tool.X=-2.4+(index%4)*1.6;tool.Z=index<4?-.8:.8;state.Tools.Add(tool);
                }
                state.SelectedId=state.Tools[0].Id;workspace.ActiveExperiment=experiment;Current=state;Refresh(true);
                await ToSignal(GetTree(),SceneTree.SignalName.PhysicsFrame);await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);
                Require(bench.CachedToolCount==state.Tools.Count,experiment+" catalog batch "+start+" creates all models");
                if(OS.GetCmdlineUserArgs().Contains("--capture"))await Capture(experiment+"-catalog-"+start);
            }
        }
        workspace=Session.Decode(saved);Refresh(true);Require(await SaveWorkspace(),"restore original integration workspace after expanded checks");
    }
}
