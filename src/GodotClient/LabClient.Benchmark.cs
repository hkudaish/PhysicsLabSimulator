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
            throw new ArgumentException("Benchmark duration must be 5–3600 seconds per preset.");
        Require(DisplayServer.GetName()!="headless","performance sampling uses real renderer");
        var samples=new List<object>();string original=Session.Encode(workspace);
        using var process=Process.GetCurrentProcess();
        foreach(bool high in new[]{false,true})
        {
            SetQuality(high);var clock=Stopwatch.StartNew();
            while(clock.Elapsed.TotalSeconds<2)await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);
            process.Refresh();long initialPrivate=process.PrivateMemorySize64;
            var frames=new List<double>();clock.Restart();double previous=0;
            while(clock.Elapsed.TotalSeconds<duration)
            {
                await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);
                double now=clock.Elapsed.TotalSeconds;frames.Add((now-previous)*1000);previous=now;
            }
            process.Refresh();var sorted=frames.OrderBy(t=>t).ToArray();
            samples.Add(new{preset=high?"high-4x-msaa":"low-no-msaa",seconds=clock.Elapsed.TotalSeconds,frames=frames.Count,
                averageFps=frames.Count/clock.Elapsed.TotalSeconds,p95FrameMs=sorted[(int)((sorted.Length-1)*0.95)],
                maximumFrameMs=sorted[^1],initialPrivateBytes=initialPrivate,finalPrivateBytes=process.PrivateMemorySize64,
                workingSetBytes=process.WorkingSet64});
        }
        SetQuality(false);Require(Session.Encode(workspace)==original,"benchmark presets preserve full scientific workspace");
        File.WriteAllText(Path.Combine(ResultsFolder(),"benchmark-result.json"),JsonSerializer.Serialize(new
        {samples,renderer=RenderingServer.GetCurrentRenderingMethod(),gpu=RenderingServer.GetVideoAdapterName(),
            viewport=new{width=viewport.Size.X,height=viewport.Size.Y},scene="three-tool closed DC loop with series ammeter",
            schoolHardwareApproved=false,fullHourAcceptance=false,scope="Short development-PC smoke; no leak or school-hardware qualification claim"},new JsonSerializerOptions{WriteIndented=true}));
    }
}
