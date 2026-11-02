using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using VirtualStem.Client;

public partial class LabClient
{
    private async Task BenchmarkIfRequested()
    {
        var args=OS.GetCmdlineUserArgs();int option=Array.IndexOf(args,"--benchmark-seconds");
        if(option<0)return;
        if(option+1>=args.Length||!double.TryParse(args[option+1],out double duration)||duration<5||duration>3600)
            throw new ArgumentException("Benchmark duration must be 5–3600 seconds per experiment/preset.");
        Require(DisplayServer.GetName()!="headless","performance sampling uses real renderer");
        var samples=new List<object>();string original=Session.Encode(workspace),active=workspace.ActiveExperiment;
        bool previousQuality=highQuality;
        using var process=Process.GetCurrentProcess();
        foreach(string experiment in new[]{"chemistry","electricity"})foreach(bool high in new[]{false,true})
        {
            // Compatibility has one supported profile; do not invent a second high-quality sample.
            if(high&&!bench.SupportsAdvancedRendering)continue;
            SwitchExperiment(experiment);SetQuality(high);bench.SnapCamera();
            var camera=Session.Clone(Current);var clock=Stopwatch.StartNew();
            while(clock.Elapsed.TotalSeconds<2)await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);
            process.Refresh();long initialPrivate=process.PrivateMemorySize64;
            var frames=new List<double>();clock.Restart();double previous=0;
            while(clock.Elapsed.TotalSeconds<duration)
            {
                camera.CameraYaw=Current.CameraYaw+0.15*Math.Sin(clock.Elapsed.TotalSeconds*0.7);
                bench.UpdateCamera(camera);
                await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);
                double now=clock.Elapsed.TotalSeconds;frames.Add((now-previous)*1000);previous=now;
            }
            process.Refresh();var sorted=frames.OrderBy(t=>t).ToArray();
            samples.Add(new{experiment,preset=high?"realistic-ssao-ssr-4x-msaa":"low-no-screen-space-effects",fpsLimit=Engine.MaxFps,seconds=clock.Elapsed.TotalSeconds,frames=frames.Count,
                averageFps=frames.Count/clock.Elapsed.TotalSeconds,p95FrameMs=sorted[(int)((sorted.Length-1)*0.95)],
                maximumFrameMs=sorted[^1],initialPrivateBytes=initialPrivate,finalPrivateBytes=process.PrivateMemorySize64,
                workingSetBytes=process.WorkingSet64});
            bench.UpdateCamera(Current,true);
        }
        SwitchExperiment(active);SetQuality(previousQuality);bench.UpdateCamera(Current,true);
        Require(Session.Encode(workspace)==original,"benchmark profiles and camera motion preserve full scientific workspace");
        File.WriteAllText(Path.Combine(ResultsFolder(),"benchmark-result.json"),JsonSerializer.Serialize(new
        {samples,renderer=RenderingServer.GetCurrentRenderingMethod(),gpu=RenderingServer.GetVideoAdapterName(),
            viewport=new{width=viewport.Size.X,height=viewport.Size.Y},scene="furnished room; both scientific benches; scripted smooth camera orbit",
            schoolHardwareApproved=false,fullHourAcceptance=false,scope="Development-PC visual/performance smoke; no school-hardware or leak qualification"},new JsonSerializerOptions{WriteIndented=true}));
    }
}
