using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using VirtualStem.Client;
using VirtualStem.Science;

public partial class LabClient
{
    private readonly List<string> verified=new();
    private void Require(bool condition,string name)
    {if(!condition)throw new InvalidOperationException("Verification failed: "+name);verified.Add(name);File.WriteAllText(Path.Combine(ResultsFolder(),"progress.txt"),name);}
    private void Near(double actual,double expected,string name)=>Require(Math.Abs(actual-expected)<=1e-10+Math.Abs(expected)*1e-9,name);
    private string ResultsFolder()
    {
        string directory=System.Environment.GetEnvironmentVariable("STEM_LAB_RESULTS")??ProjectSettings.GlobalizePath("user://verification");
        Directory.CreateDirectory(directory);return directory;
    }
    private static string Hash(string text)=>Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text))).ToLowerInvariant();
    private async Task VerifyPrototype(bool resume)
    {
        if(string.IsNullOrWhiteSpace(System.Environment.GetEnvironmentVariable("STEM_LAB_TEST_STORAGE")))
            throw new InvalidOperationException("Verification requires an isolated STEM_LAB_TEST_STORAGE directory.");
        verified.Clear();
        if(OS.GetCmdlineUserArgs().Contains("--expect-export"))Require(OS.HasFeature("template")&&!OS.HasFeature("editor"),"standalone exported runtime");
        if(resume)
        {
            Require(await LoadWorkspace(),"load saved workspace in fresh process");
            Near(workspace.Chemistry.Tools.First(t=>t.Id=="beaker").VolumeL,0.1,"restored dilution volume");
            Near(workspace.Chemistry.Tools.First(t=>t.Id=="beaker").Moles,0.05,"restored dilution solute");
            Require(workspace.Electricity.Wires.Count==3,"restored wire topology");
            Require(workspace.Electricity.Tools.First(t=>t.Id=="multimeter").Mode=="current","restored instrument mode");
            Near(Session.Electricity(workspace.Electricity).Branches["multimeter"].CurrentA,0.009,"restored calculated current");
            Require(workspace.ChemistryUndo.Count>0&&workspace.ElectricityUndo.Count>0,"undo history survives process restart");
            Require(Hash(Session.Encode(workspace))==File.ReadAllText(Path.Combine(ResultsFolder(),"saved-workspace.sha256")),"full workspace snapshot restored exactly");
            if(OS.GetCmdlineUserArgs().Contains("--expect-expanded"))
            {
                string? draft=store.Load("expanded-verification",Session.WorkspaceVersion);Require(draft!=null,"expanded verification draft survives process restart");
                var expanded=Session.Decode(draft!);
                Require(expanded.Chemistry.Tools.Any(t=>t.Kind=="element"&&t.Mode=="Cu")&&expanded.Electricity.Tools.Any(t=>t.Kind=="and"&&t.Mode=="11"),"new sample and logic modes restore in fresh process");
            }
        }
        else
        {
            string original=Session.Encode(workspace);
            amount.Value=200;actions["pour"].EmitSignal(Godot.Button.SignalName.Pressed);
            Require(Session.Encode(workspace)==original,"invalid transfer leaves entire workspace unchanged");
            amount.Value=25;actions["pour"].EmitSignal(Godot.Button.SignalName.Pressed);
            Near(Current.Tools.First(t=>t.Id=="beaker").Moles,0.05,"UI transfer commits calculated solute");
            amount.Value=75;actions["water"].EmitSignal(Godot.Button.SignalName.Pressed);
            Near(Selected!.Vessel().ConcentrationMolL!.Value,0.5,"UI dilution concentration");
            Require(feedback.Text.Contains("0.500",StringComparison.Ordinal),"scientific goal feedback uses computed state");
            Near(Current.Tools.Where(t=>Session.Chemical(t.Kind)).Sum(t=>t.Moles),0.2,"whole-bench solute conserved");
            Near(Current.Tools.Where(t=>Session.Chemical(t.Kind)).Sum(t=>t.VolumeL),0.6,"whole-bench liquid conserved");
            actions["undo"].EmitSignal(Godot.Button.SignalName.Pressed);
            Near(Selected!.VolumeL,0.025,"undo restores pre-water volume");
            amount.Value=75;actions["water"].EmitSignal(Godot.Button.SignalName.Pressed);
            original=Session.Encode(workspace);
            Require(!MoveTool("beaker",99,0)&&Session.Encode(workspace)==original,"out-of-bounds placement rejected");
            var stock=Current.Tools.First(t=>t.Id=="stock");
            Require(!MoveTool("beaker",stock.X,stock.Z),"overlap placement rejected");
            Require(MoveTool("beaker",-1.3,1.1),"tool moves to valid placement");
            Near(Selected!.Moles,0.05,"placement cannot change chemistry");
            Undo();Near(Selected!.X,0,"undo restores equipment position");
            RotateSelected(Mathf.Pi/8);Require(Math.Abs(Selected!.Yaw)>0.3,"rotation changes pose");Undo();
            await ToSignal(GetTree(),SceneTree.SignalName.PhysicsFrame);
            await ToSignal(GetTree(),SceneTree.SignalName.PhysicsFrame);
            Vector2 pixel=bench.Camera.UnprojectPosition(bench.ToolPosition("beaker")+Vector3.Up*0.32f);
            Require(bench.Pick(pixel).tool=="beaker","3D physics ray selects actual glassware");
            SelectTool("sensor");Require(readings.Text.Contains("0.500",StringComparison.Ordinal),"concentration probe shows calculated measurement");
            Require(MoveTool("sensor",2.8,1.4)&&!readings.Text.Contains("0.500",StringComparison.Ordinal),"probe must be near its target vessel");
            Undo();
            SelectTool("beaker");
            int meshBuilds=bench.ToolMeshBuildCount;
            SelectTool("stock");SelectTool("beaker");
            Require(bench.ToolMeshBuildCount==meshBuilds,"selection retains equipment meshes and physics bodies");
            var beforeDrag=bench.ToolPosition("beaker");original=Session.Encode(workspace);
            bench.PreviewPosition("beaker",beforeDrag+Vector3.Right,0.016);
            Require(bench.ToolPosition("beaker").X>beforeDrag.X&&bench.ToolPosition("beaker").X<beforeDrag.X+1,"drag preview interpolates toward pointer target");
            Require(Session.Encode(workspace)==original,"drag preview cannot commit scientific or saved placement state");
            Refresh();
            for(int i=0;i<20;i++){RotateSelected(Mathf.Pi/8);Undo();}
            Require(Current.Tools.Count==4&&Current.Tools.Select(t=>t.Id).Distinct().Count()==4,"twenty manipulation and undo cycles retain equipment");
            actions["language"].EmitSignal(Godot.Button.SignalName.Pressed);
            Require(LayoutDirection==LayoutDirectionEnum.Ltr&&feedback.Text.Contains("Goal achieved",StringComparison.Ordinal),"English interface switches layout and lesson text");
            actions["language"].EmitSignal(Godot.Button.SignalName.Pressed);
            Require(LayoutDirection==LayoutDirectionEnum.Rtl,"Arabic interface restores RTL layout");
            var cameraPose=bench.Camera.Position;var cameraState=Session.Clone(Current);cameraState.CameraYaw+=0.3;
            bench.UpdateCamera(cameraState);
            Require(bench.Camera.Position.IsEqualApprox(cameraPose),"camera input sets a smooth target without snapping");
            bench.Advance(0.05);Require(!bench.Camera.Position.IsEqualApprox(cameraPose),"camera advances toward orbit target");
            bench.UpdateCamera(Current,true);
            original=Session.Encode(workspace);bench.AnimatePour("stock","beaker",false);
            var animationClock=System.Diagnostics.Stopwatch.StartNew();
            while(animationClock.Elapsed.TotalSeconds<1.1)await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);
            Require(Session.Encode(workspace)==original,"pour animation cannot change quantities pose or scientific state");
            SwitchExperiment("electricity");
            await ToSignal(GetTree(),SceneTree.SignalName.PhysicsFrame);await ToSignal(GetTree(),SceneTree.SignalName.PhysicsFrame);
            foreach(var tool in Current.Tools)foreach(string port in new[]{".a",".b"})
            {
                string terminal=tool.Id+port;
                Require(bench.Pick(bench.Camera.UnprojectPosition(bench.TerminalPosition(terminal))).terminal==terminal,"3D ray selects terminal "+terminal);
            }
            TerminalClicked("battery.a");TerminalClicked("resistor.a");TerminalClicked("resistor.b");TerminalClicked("battery.b");
            TerminalClicked("multimeter.a");TerminalClicked("resistor.a");TerminalClicked("multimeter.b");TerminalClicked("resistor.b");
            Require(electrical!.Status==CircuitStatus.Solved,"wired 3D workbench solves circuit");
            Near(electrical.Branches["resistor"].CurrentA,0.009,"UI Ohm law current");
            SelectTool("multimeter");Require(readings.Text.Contains("9.000",StringComparison.Ordinal),"voltmeter connected in parallel");
            SelectTool("resistor");componentValue.Value=2000;
            Near(electrical!.Branches["resistor"].CurrentA,0.0045,"resistance control recalculates current");componentValue.Value=1000;
            SelectTool("battery");componentValue.Value=12;Near(electrical!.Branches["resistor"].CurrentA,0.012,"voltage control recalculates current");componentValue.Value=9;
            wireChoice.Select(0);actions["disconnect"].EmitSignal(Godot.Button.SignalName.Pressed);
            Require(electrical!.Status==CircuitStatus.OpenCircuit,"disconnect produces open circuit");
            TerminalClicked("battery.a");TerminalClicked("resistor.a");
            TerminalClicked("battery.a");TerminalClicked("battery.b");Require(electrical!.Status==CircuitStatus.ShortCircuit&&!electrical.HasReadings,"short circuit refuses fabricated current");
            wireChoice.Select(wireIds.Count-1);actions["disconnect"].EmitSignal(Godot.Button.SignalName.Pressed);
            Require(electrical!.Status==CircuitStatus.Solved,"removing short restores valid state");
            Change("Series ammeter setup",data=>
            {
                data.Tools.First(t=>t.Id=="multimeter").Mode="current";
                data.Wires.RemoveAll(w=>w.A.StartsWith("multimeter.",StringComparison.Ordinal)||w.B.StartsWith("multimeter.",StringComparison.Ordinal)||
                    (w.A=="resistor.b"&&w.B=="battery.b")||(w.B=="resistor.b"&&w.A=="battery.b"));return true;
            },true);
            TerminalClicked("resistor.b");TerminalClicked("multimeter.a");TerminalClicked("multimeter.b");TerminalClicked("battery.b");
            SelectTool("multimeter");Near(electrical!.Branches["multimeter"].CurrentA,0.009,"series ammeter reads solver branch");
            Require(readings.Text.Contains("0.009000",StringComparison.Ordinal),"ammeter measurement display");
            string scienceBefore=Session.Encode(workspace);SetQuality(true);
            Require(bench.HighQuality==bench.SupportsAdvancedRendering,"realistic profile only enables supported rendering effects");
            Near(Session.Electricity(Current).Branches["resistor"].CurrentA,0.009,"graphics quality cannot alter scientific results");
            Require(scienceBefore==Session.Encode(workspace),"renderer does not mutate scientific state");SetQuality(false);
            Require(await SaveWorkspace(),"save both experiments through SQLite");
            string saved=Session.Encode(workspace);
            workspace.Chemistry.Tools.First(t=>t.Id=="beaker").VolumeL=0.05;
            Require(await LoadWorkspace(),"resume workspace through SQLite");
            Require(Session.Encode(workspace)==saved,"save/resume retains tools topology quantities and history");
            store.Save("workspace",Session.WorkspaceVersion,"{broken-json");
            Require(!await LoadWorkspace()&&Session.Encode(workspace)==saved,"corrupt saved draft cannot replace current work");
            store.Save("workspace","future-model-version",saved);
            Require(!await LoadWorkspace()&&Session.Encode(workspace)==saved,"newer saved model cannot replace current work");
            Require(await SaveWorkspace(),"valid draft can be saved after rejected loads");
        }
        if(!resume)await VerifyExpandedEquipment();
        if(OS.GetCmdlineUserArgs().Contains("--capture"))
        {
            Require(DisplayServer.GetName()!="headless","capture uses real graphics driver");
            SetQuality(true);
            SwitchExperiment("chemistry");SelectTool("beaker");await Capture("chemistry");
            SwitchExperiment("electricity");SelectTool("multimeter");await Capture("electricity");
            SetQuality(false);await Capture("electricity-low");SetQuality(true);
        }
        await BenchmarkIfRequested();
        Require(await SaveWorkspace(),"final saved workspace");
        File.WriteAllText(Path.Combine(ResultsFolder(),"saved-workspace.sha256"),Hash(Session.Encode(workspace)));
        var report=new {success=true,passed=verified,engine=Engine.GetVersionInfo()["string"].AsString(),platform=OS.GetName(),
            exported=OS.HasFeature("template"),renderer=RenderingServer.GetCurrentRenderingMethod(),gpu=RenderingServer.GetVideoAdapterName(),
            chemistryConcentration=workspace.Chemistry.Tools.First(t=>t.Id=="beaker").Vessel().ConcentrationMolL,
            currentA=Session.Electricity(workspace.Electricity).Branches["multimeter"].CurrentA,
            visualVersion="0.3.0",elementCount=Elements.All.Length,equipmentCount=ToolCatalog.All.Length,advancedRenderingSupported=bench.SupportsAdvancedRendering,
            schoolHardwareApproved=false,independentScientificReview="pending",scope="Phase 1 visual upgrade integration checks; no release acceptance claimed"};
        File.WriteAllText(Path.Combine(ResultsFolder(),resume?"resume-result.json":"integration-result.json"),JsonSerializer.Serialize(report,new JsonSerializerOptions{WriteIndented=true}));
        GD.Print("Lab integration checks passed: "+verified.Count);await FinishAndQuit(0);
    }
    private async Task Capture(string name)
    {
        bench.SnapCamera();
        await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);
        RenderingServer.ForceDraw(false);RenderingServer.ForceSync();
        var image=viewport.GetTexture().GetImage();var colors=new HashSet<Color>();
        for(int x=0;x<image.GetWidth();x+=32)for(int y=0;y<image.GetHeight();y+=32)colors.Add(image.GetPixel(x,y));
        Require(colors.Count>8,name+" 3D viewport renders equipment");
        Require(GetViewport().GetTexture().GetImage().SavePng(Path.Combine(ResultsFolder(),name+"-lab.png"))==Error.Ok,name+" screenshot");
    }
    private void WriteFailure(Exception error)
    {File.WriteAllText(Path.Combine(ResultsFolder(),"failure.json"),JsonSerializer.Serialize(new{success=false,error=error.ToString(),passed=verified},new JsonSerializerOptions{WriteIndented=true}));}
}
