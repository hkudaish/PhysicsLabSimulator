using Godot;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using VirtualStem.Client;
using VirtualStem.Science;

public sealed partial class LabBench
{
    public SubViewport Viewport {get;}
    public Camera3D Camera {get;}
    private readonly Node3D world;
    private readonly Font font;
    private readonly Dictionary<string,Node3D> tools=new();
    private readonly Dictionary<string,string> signatures=new();
    private readonly Dictionary<string,Node3D> terminals=new();
    private readonly Dictionary<string,MeshInstance3D> rings=new();
    private Node3D wiresRoot=null!;
    private string wiresSignature="";
    private SessionData? lastData;
    private Vector3 cameraTarget;
    private bool cameraInitialized;
    private Tween? pourTween;
    private Node3D? pouringNode,stream;
    private Transform3D pourOriginal;
    public int ToolMeshBuildCount {get;private set;}
    public int CachedToolCount=>tools.Count;
    public LabBench(SubViewport viewport,Font font)
    {
        Viewport=viewport;this.font=font;world=new Node3D();viewport.AddChild(world);
        BuildRoom();Camera=new Camera3D{Current=true,Fov=48,Near=0.06f,Far=60};world.AddChild(Camera);
        ApplyQuality(SupportsAdvancedRendering);
    }
    public void Render(SessionData data,CircuitResult? result)
    {
        CancelPour();lastData=data;
        foreach(string id in tools.Keys.Where(id=>!data.Tools.Any(t=>t.Id==id)).ToArray())RemoveTool(id);
        foreach(var tool in data.Tools)
        {
            string reading=Reading(tool,data,result);
            string signature=$"{tool.Kind}:{tool.Value:R}:{tool.VolumeL:R}:{tool.CapacityL:R}:{tool.Moles:R}:{tool.Mode}:{reading}";
            if(!signatures.TryGetValue(tool.Id,out string? existing)||existing!=signature)
            {
                RemoveTool(tool.Id);
                var node=new Node3D();world.AddChild(node);tools[tool.Id]=node;signatures[tool.Id]=signature;ToolMeshBuildCount++;
                if(Session.Chemical(tool.Kind))Vessel(node,tool);
                else if(tool.Kind=="battery")PowerSupply(node,tool);
                else if(tool.Kind=="resistor")Resistor(node,tool);
                else Meter(node,tool,reading);
                if(data.Experiment=="electricity")
                {
                    var ports=tool.Kind=="meter"?new[]{new Vector3(-0.22f,0.23f,0.4f),new Vector3(0.22f,0.23f,0.4f)}:
                        tool.Kind=="resistor"?new[]{new Vector3(-0.43f,0.19f,0),new Vector3(0.43f,0.19f,0)}:
                        new[]{new Vector3(-0.4f,0.67f,0.14f),new Vector3(0.4f,0.67f,0.14f)};
                    Terminal(node,tool.Id+".a",ports[0],new Color(0.72f,0.035f,0.025f),"+");
                    Terminal(node,tool.Id+".b",ports[1],new Color(0.035f,0.04f,0.045f),"−");
                }
                var ring=Mesh(node,new TorusMesh{InnerRadius=0.51f,OuterRadius=0.523f,Rings=48,RingSegments=6},new Vector3(0,0.014f,0),Finish(new Color(0.06f,0.66f,0.6f),0.4f));rings[tool.Id]=ring;
            }
            tools[tool.Id].Position=new Vector3((float)tool.X,0,(float)tool.Z);tools[tool.Id].Rotation=new Vector3(0,(float)tool.Yaw,0);
            rings[tool.Id].Visible=tool.Id==data.SelectedId;
        }
        SyncWires(data);UpdateCamera(data);
    }
    private void RemoveTool(string id)
    {
        if(tools.Remove(id,out var old)){world.RemoveChild(old);old.QueueFree();}
        signatures.Remove(id);rings.Remove(id);terminals.Remove(id+".a");terminals.Remove(id+".b");
    }
    private void SyncWires(SessionData data,bool force=false)
    {
        string signature=string.Join("|",data.Wires.Select(w=>w.Id+":"+w.A+":"+w.B))+string.Join("|",data.Tools.Select(t=>$"{t.Id}:{tools[t.Id].Transform}"));
        if(!force&&signature==wiresSignature)return;
        wiresSignature=signature;
        if(wiresRoot!=null){world.RemoveChild(wiresRoot);wiresRoot.QueueFree();}
        wiresRoot=new Node3D();world.AddChild(wiresRoot);
        for(int index=0;index<data.Wires.Count;index++)
        {
            var wire=data.Wires[index];if(!terminals.TryGetValue(wire.A,out var a)||!terminals.TryGetValue(wire.B,out var b))continue;
            var start=a.GlobalPosition;var end=b.GlobalPosition;
            var c1=start+new Vector3(0,-0.35f,0.7f+index*0.08f);var c2=end+new Vector3(0,-0.35f,0.7f+index*0.08f);
            var path=new List<Vector3>();
            for(int i=0;i<=32;i++)
            {float t=i/32f,u=1-t;var p=u*u*u*start+3*u*u*t*c1+3*u*t*t*c2+t*t*t*end;p.Y=Math.Max(0.035f,p.Y);path.Add(p);}
            var color=wire.A.EndsWith(".a",StringComparison.Ordinal)?new Color(0.55f,0.025f,0.018f):new Color(0.025f,0.055f,0.08f);
            Mesh(wiresRoot,BenchGeometry.Tube(path,0.022f,10),Vector3.Zero,Finish(color,0.42f));
        }
    }
    private static string Reading(ToolState tool,SessionData data,CircuitResult? result)
    {
        if(tool.Kind!="meter")return "";
        if(data.Experiment=="chemistry")
        {
            var target=data.Tools.FirstOrDefault(t=>t.Id==tool.Target&&Session.Chemical(t.Kind));
            return target!=null&&Distance(tool,target)<=1.65&&target.Vessel().ConcentrationMolL is double c?F(c,"0.000")+" mol/L":"—";
        }
        if(result?.HasReadings!=true)return "—";
        if(tool.Mode=="voltage"&&result.VoltageBetween(tool.Id+".a",tool.Id+".b") is double voltage)return F(voltage,"0.000")+" V";
        return tool.Mode=="current"&&result.Branches.TryGetValue(tool.Id,out var branch)?F(branch.CurrentA,"0.000000")+" A":"—";
    }
    public void UpdateCamera(SessionData data,bool immediate=false)
    {
        float r=(float)data.CameraRadius,p=(float)data.CameraPitch,y=(float)data.CameraYaw;
        cameraTarget=new Vector3(r*Mathf.Cos(p)*Mathf.Sin(y),r*Mathf.Sin(p),r*Mathf.Cos(p)*Mathf.Cos(y));
        if(!cameraInitialized||immediate){Camera.Position=cameraTarget;Camera.LookAt(new Vector3(0,0.23f,0));cameraInitialized=true;}
    }
    public void Advance(double delta)
    {
        if(!cameraInitialized)return;
        Camera.Position=Camera.Position.Lerp(cameraTarget,1-Mathf.Exp(-12*(float)Math.Min(delta,0.1)));
        Camera.LookAt(new Vector3(0,0.23f,0));
    }
    public void SnapCamera(){if(cameraInitialized){Camera.Position=cameraTarget;Camera.LookAt(new Vector3(0,0.23f,0));}}
    public static double Distance(ToolState a,ToolState b)=>Math.Sqrt((a.X-b.X)*(a.X-b.X)+(a.Z-b.Z)*(a.Z-b.Z));
    public void PreviewPosition(string id,Vector3 position,double delta=1)
    {
        CancelPour();
        if(tools.TryGetValue(id,out var node))node.Position=node.Position.Lerp(position,1-Mathf.Exp(-28*(float)Math.Min(delta,1)));
        if(lastData?.Experiment=="electricity")SyncWires(lastData);
    }
    public void AnimatePour(string source,string target,bool pureWater)
    {
        CancelPour();if(!tools.TryGetValue(source,out var node)||!tools.TryGetValue(target,out var destination))return;
        pouringNode=node;pourOriginal=node.Transform;
        var cap=node.GetNodeOrNull<Node3D>("Cap");if(cap!=null)cap.Visible=false;
        var position=destination.Position+new Vector3(-0.55f,1.0f,0);
        pourTween=world.CreateTween();pourTween.SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.InOut);
        pourTween.TweenProperty(node,"position",position,0.3);
        pourTween.Parallel().TweenProperty(node,"rotation",new Vector3(0,0,-0.7f),0.3);
        pourTween.TweenCallback(Callable.From(()=>
        {
            stream=new Node3D();world.AddChild(stream);
            var path=new[]{destination.Position+new Vector3(-0.15f,1.42f,0),destination.Position+new Vector3(-0.05f,1.08f,0),destination.Position+new Vector3(0,0.44f,0)};
            var liquid=Finish(pureWater?new Color(0.58f,0.8f,0.85f,0.5f):new Color(0.16f,0.62f,0.7f,0.75f),0.08f);liquid.Transparency=BaseMaterial3D.TransparencyEnum.Alpha;
            Mesh(stream,BenchGeometry.Tube(path,0.028f),Vector3.Zero,liquid);
        }));
        pourTween.TweenInterval(0.35);
        pourTween.TweenCallback(Callable.From(()=>{stream?.QueueFree();stream=null;}));
        pourTween.TweenProperty(node,"transform",pourOriginal,0.3);
        pourTween.TweenCallback(Callable.From(()=>{if(cap!=null)cap.Visible=true;pouringNode=null;pourTween=null;}));
    }
    private void CancelPour()
    {
        pourTween?.Kill();pourTween=null;
        if(pouringNode!=null&&GodotObject.IsInstanceValid(pouringNode))
        {pouringNode.Transform=pourOriginal;var cap=pouringNode.GetNodeOrNull<Node3D>("Cap");if(cap!=null)cap.Visible=true;}
        pouringNode=null;stream?.QueueFree();stream=null;
    }
    public Vector3 ToolPosition(string id)=>tools[id].GlobalPosition;
    public Vector3 TerminalPosition(string id)=>terminals[id].GlobalPosition;
    public (string? tool,string? terminal) Pick(Vector2 point)
    {
        var from=Camera.ProjectRayOrigin(point);var query=PhysicsRayQueryParameters3D.Create(from,from+Camera.ProjectRayNormal(point)*100,1);
        var hit=Viewport.FindWorld3D().DirectSpaceState.IntersectRay(query);if(hit.Count==0)return(null,null);
        var collider=hit["collider"].AsGodotObject();return(collider.HasMeta("tool")?collider.GetMeta("tool").AsString():null,collider.HasMeta("terminal")?collider.GetMeta("terminal").AsString():null);
    }
    public Vector3? OnTable(Vector2 point)=>new Plane(Vector3.Up,0).IntersectsRay(Camera.ProjectRayOrigin(point),Camera.ProjectRayNormal(point));
    private void DeviceBody(Node3D node,string id,Vector3 size)
    {
        var body=new StaticBody3D();body.SetMeta("tool",id);node.AddChild(body);
        using var shape=new BoxShape3D{Size=size};
        body.AddChild(new CollisionShape3D{Shape=shape,Position=new Vector3(0,size.Y/2,0)});
    }
    private void Terminal(Node3D parent,string id,Vector3 position,Color color,string symbol)
    {
        var body=new StaticBody3D{Position=position};body.SetMeta("terminal",id);parent.AddChild(body);terminals[id]=body;
        Mesh(body,new CylinderMesh{TopRadius=0.07f,BottomRadius=0.08f,Height=0.08f,RadialSegments=24},new Vector3(0,-0.035f,0),metal);
        Mesh(body,new TorusMesh{InnerRadius=0.043f,OuterRadius=0.075f,Rings=32,RingSegments=10},Vector3.Zero,Finish(color,0.34f));
        Mesh(body,new CylinderMesh{TopRadius=0.036f,BottomRadius=0.036f,Height=0.014f,RadialSegments=24},new Vector3(0,0.004f,0),rubber);
        using var shape=new SphereShape3D{Radius=0.115f};
        body.AddChild(new CollisionShape3D{Shape=shape});
        Text3D(body,symbol,new Vector3(0,0.15f,0),19,new Color(0.12f,0.18f,0.19f),0.003f);
    }
    private static string F(double value,string format="0.###")=>value.ToString(format,CultureInfo.InvariantCulture);
}
