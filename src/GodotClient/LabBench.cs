using Godot;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using VirtualStem.Client;
using VirtualStem.Science;

public sealed class LabBench
{
    public SubViewport Viewport { get; }
    public Camera3D Camera { get; }
    private Node3D toolsRoot = null!;
    private readonly Node3D world;
    private readonly Font font;
    private readonly Dictionary<string,Node3D> tools=new();
    private readonly Dictionary<string,Node3D> terminals=new();
    public LabBench(SubViewport viewport,Font font)
    {
        Viewport=viewport;this.font=font;
        world=new Node3D();viewport.AddChild(world);
        var environment=new WorldEnvironment{Environment=new Godot.Environment
        {
            BackgroundMode=Godot.Environment.BGMode.Color,BackgroundColor=new Color(0.055f,0.08f,0.12f),
            AmbientLightSource=Godot.Environment.AmbientSource.Color,AmbientLightColor=new Color(0.7f,0.8f,1),AmbientLightEnergy=0.65f,
            ReflectedLightSource=Godot.Environment.ReflectionSource.Disabled
        }};
        world.AddChild(environment);
        world.AddChild(new DirectionalLight3D{RotationDegrees=new Vector3(-55,-30,0),LightEnergy=1.15f});
        Camera=new Camera3D{Current=true,Fov=48};world.AddChild(Camera);
        Box(world,new Vector3(7.2f,0.22f,4.2f),new Vector3(0,-0.12f,0),new Color(0.19f,0.25f,0.29f));
        Box(world,new Vector3(7.3f,0.08f,4.3f),new Vector3(0,-0.26f,0),new Color(0.06f,0.11f,0.14f));
        foreach(float x in new[]{-3f,3f})foreach(float z in new[]{-1.5f,1.5f})
            Box(world,new Vector3(0.12f,1.1f,0.12f),new Vector3(x,-0.78f,z),new Color(0.1f,0.13f,0.16f));
        Box(world,new Vector3(30,0.1f,30),new Vector3(0,-1.4f,0),new Color(0.045f,0.065f,0.09f));
        for(int i=-3;i<=3;i++)Box(world,new Vector3(0.008f,0.004f,3.8f),new Vector3(i,0.003f,0),new Color(0.24f,0.32f,0.35f));
    }
    public void Render(SessionData data,CircuitResult? result)
    {
        if(toolsRoot!=null){world.RemoveChild(toolsRoot);toolsRoot.QueueFree();}
        toolsRoot=new Node3D();world.AddChild(toolsRoot);tools.Clear();terminals.Clear();
        foreach(var tool in data.Tools)
        {
            var node=new Node3D{Position=new Vector3((float)tool.X,0,(float)tool.Z),Rotation=new Vector3(0,(float)tool.Yaw,0)};
            toolsRoot.AddChild(node);tools[tool.Id]=node;
            if(Session.Chemical(tool.Kind))Container(node,tool);
            else if(tool.Kind=="battery")
            {
                Box(node,new Vector3(0.8f,0.48f,0.55f),new Vector3(0,0.25f,0),new Color(0.11f,0.2f,0.24f));
                Box(node,new Vector3(0.82f,0.09f,0.57f),new Vector3(0,0.5f,0),new Color(0.8f,0.34f,0.16f));
                Label(node,F(tool.Value)+" V",new Vector3(0,0.78f,0),23);
                DeviceBody(node,tool.Id,0.55f);
            }
            else if(tool.Kind=="resistor")
            {
                Box(node,new Vector3(0.62f,0.25f,0.3f),new Vector3(0,0.22f,0),new Color(0.76f,0.62f,0.36f));
                for(int i=-1;i<=1;i++)Box(node,new Vector3(0.035f,0.26f,0.31f),new Vector3(i*0.14f,0.22f,0),new Color(0.3f,0.12f,0.06f));
                Label(node,F(tool.Value)+" Ω",new Vector3(0,0.65f,0),21);
                DeviceBody(node,tool.Id,0.4f);
            }
            else
            {
                Box(node,new Vector3(0.78f,0.3f,0.55f),new Vector3(0,0.2f,0),new Color(0.88f,0.63f,0.18f));
                Box(node,new Vector3(0.63f,0.035f,0.35f),new Vector3(0,0.37f,0),new Color(0.06f,0.15f,0.12f));
                string reading="—";
                if(data.Experiment=="chemistry")
                {
                    var target=data.Tools.FirstOrDefault(t=>t.Id==tool.Target&&Session.Chemical(t.Kind));
                    if(target!=null&&Distance(tool,target)<=1.65&&target.Vessel().ConcentrationMolL is double concentration)
                        reading=F(concentration,"0.000")+" mol/L";
                }
                else if(result?.HasReadings==true)
                {
                    if(tool.Mode=="voltage"&&result.VoltageBetween(tool.Id+".a",tool.Id+".b") is double voltage)reading=F(voltage,"0.000")+" V";
                    if(tool.Mode=="current"&&result.Branches.TryGetValue(tool.Id,out var branch))reading=F(branch.CurrentA,"0.000000")+" A";
                }
                Label(node,reading,new Vector3(0,0.62f,0),20,new Color(0.45f,1,0.75f));
                DeviceBody(node,tool.Id,0.45f);
            }
            if(data.Experiment=="electricity")
            {
                Terminal(node,tool.Id+".a",new Vector3(-0.43f,0.65f,0),new Color(0.95f,0.2f,0.17f),"+");
                Terminal(node,tool.Id+".b",new Vector3(0.43f,0.65f,0),new Color(0.18f,0.22f,0.29f),"−");
            }
            if(tool.Id==data.SelectedId)
                AddMesh(node,new TorusMesh{InnerRadius=0.48f,OuterRadius=0.51f,Rings=32,RingSegments=8},
                    new Vector3(0,0.015f,0),Material(new Color(0.1f,0.8f,0.9f),true));
        }
        if(data.Experiment=="electricity")foreach(var wire in data.Wires)
        {
            if(!terminals.TryGetValue(wire.A,out var start)||!terminals.TryGetValue(wire.B,out var end))continue;
            bool active=result?.HasReadings==true&&result.Branches.Values.Any(r=>Math.Abs(r.CurrentA)>1e-8);
            var color=wire.A.EndsWith(".a",StringComparison.Ordinal)?new Color(0.9f,0.2f,0.16f):new Color(0.16f,0.4f,0.68f);
            Vector3 previous=start.GlobalPosition;
            for(int i=1;i<=12;i++)
            {
                float t=i/12f;Vector3 next=start.GlobalPosition.Lerp(end.GlobalPosition,t)+Vector3.Up*(0.18f*Mathf.Sin(t*Mathf.Pi));
                var segment=AddMesh(toolsRoot,new CylinderMesh{TopRadius=0.018f,BottomRadius=0.018f,Height=previous.DistanceTo(next),RadialSegments=8},
                    (previous+next)*0.5f,Material(active?color.Lightened(0.15f):color,active));
                if(previous.DistanceTo(next)>0.001f){segment.LookAt(next,Math.Abs((next-previous).Normalized().Dot(Vector3.Up))>0.99?Vector3.Forward:Vector3.Up);segment.RotateObjectLocal(Vector3.Right,Mathf.Pi/2);}
                previous=next;
            }
        }
        UpdateCamera(data);
    }
    public void UpdateCamera(SessionData data)
    {
        float radius=(float)data.CameraRadius,pitch=(float)data.CameraPitch,yaw=(float)data.CameraYaw;
        Camera.Position=new Vector3(radius*Mathf.Cos(pitch)*Mathf.Sin(yaw),radius*Mathf.Sin(pitch),radius*Mathf.Cos(pitch)*Mathf.Cos(yaw));
        Camera.LookAt(new Vector3(0,0.2f,0));
    }
    public static double Distance(ToolState a,ToolState b)=>Math.Sqrt((a.X-b.X)*(a.X-b.X)+(a.Z-b.Z)*(a.Z-b.Z));
    public void PreviewPosition(string id,Vector3 position){if(tools.TryGetValue(id,out var node))node.Position=position;}
    public Vector3 ToolPosition(string id)=>tools[id].GlobalPosition;
    public Vector3 TerminalPosition(string id)=>terminals[id].GlobalPosition;
    public (string? tool,string? terminal) Pick(Vector2 point)
    {
        var from=Camera.ProjectRayOrigin(point);var direction=Camera.ProjectRayNormal(point);
        var query=PhysicsRayQueryParameters3D.Create(from,from+direction*100,1);
        var hit=Viewport.FindWorld3D().DirectSpaceState.IntersectRay(query);
        if(hit.Count==0)return(null,null);
        var collider=hit["collider"].AsGodotObject();
        return(collider.HasMeta("tool")?collider.GetMeta("tool").AsString():null,
            collider.HasMeta("terminal")?collider.GetMeta("terminal").AsString():null);
    }
    public Vector3? OnTable(Vector2 point)=>new Plane(Vector3.Up,0).IntersectsRay(Camera.ProjectRayOrigin(point),Camera.ProjectRayNormal(point));
    private void Container(Node3D node,ToolState tool)
    {
        float height=tool.Kind=="water"?0.9f:tool.Kind=="cylinder"?0.9f:0.65f;
        float radius=tool.Kind=="cylinder"?0.17f:0.28f;
        var glass=Material(new Color(0.66f,0.89f,1,0.22f));glass.Transparency=BaseMaterial3D.TransparencyEnum.Alpha;glass.CullMode=BaseMaterial3D.CullModeEnum.Disabled;glass.Roughness=0.1f;
        AddMesh(node,new CylinderMesh{TopRadius=radius,BottomRadius=radius*0.92f,Height=height,RadialSegments=32},new Vector3(0,height/2+0.025f,0),glass);
        AddMesh(node,new TorusMesh{InnerRadius=radius-0.018f,OuterRadius=radius+0.018f,Rings=32,RingSegments=8},new Vector3(0,height+0.025f,0),Material(new Color(0.65f,0.85f,0.91f)));
        if(tool.VolumeL>0)
        {
            float level=(float)(tool.VolumeL/tool.CapacityL)*(height-0.07f);
            float concentration=(float)(tool.Vessel().ConcentrationMolL??0);
            var liquid=Material(new Color(0.06f,0.4f+0.2f*(1-Math.Min(1,concentration/2)),0.8f,0.8f));liquid.Transparency=BaseMaterial3D.TransparencyEnum.Alpha;
            AddMesh(node,new CylinderMesh{TopRadius=radius-0.035f,BottomRadius=radius-0.04f,Height=level,RadialSegments=32},new Vector3(0,0.045f+level/2,0),liquid);
        }
        Label(node,F(tool.VolumeL*1000,"0.#")+" mL",new Vector3(0,height+0.25f,0),22);
        for(int i=1;i<=4;i++)Label(node,F(tool.CapacityL*1000*i/4,"0"),new Vector3(radius+0.03f,height*i/4,-0.03f),12,new Color(0.9f,0.96f,1));
        var body=new StaticBody3D();body.SetMeta("tool",tool.Id);node.AddChild(body);
        body.AddChild(new CollisionShape3D{Shape=new CylinderShape3D{Radius=radius+0.035f,Height=height},Position=new Vector3(0,height/2,0)});
    }
    private void DeviceBody(Node3D node,string id,float height)
    {
        var body=new StaticBody3D();body.SetMeta("tool",id);node.AddChild(body);
        body.AddChild(new CollisionShape3D{Shape=new BoxShape3D{Size=new Vector3(0.78f,height,0.55f)},Position=new Vector3(0,height/2,0)});
    }
    private void Terminal(Node3D parent,string id,Vector3 position,Color color,string symbol)
    {
        var body=new StaticBody3D{Position=position};body.SetMeta("terminal",id);parent.AddChild(body);terminals[id]=body;
        AddMesh(body,new SphereMesh{Radius=0.09f,Height=0.18f,RadialSegments=16,Rings=8},Vector3.Zero,Material(color));
        body.AddChild(new CollisionShape3D{Shape=new SphereShape3D{Radius=0.13f}});
        Label(body,symbol,new Vector3(0,0.18f,0),15);
    }
    private void Label(Node3D parent,string text,Vector3 position,int size,Color? color=null)
    {
        parent.AddChild(new Label3D{Text=text,Font=font,FontSize=size,PixelSize=0.006f,Position=position,
            Billboard=BaseMaterial3D.BillboardModeEnum.Enabled,Modulate=color??Colors.White,NoDepthTest=false,OutlineSize=3});
    }
    private static string F(double value,string format="0.###")=>value.ToString(format,CultureInfo.InvariantCulture);
    private static StandardMaterial3D Material(Color color,bool glow=false)=>new(){AlbedoColor=color,Roughness=0.65f,EmissionEnabled=glow,Emission=color,EmissionEnergyMultiplier=glow?0.3f:0};
    private static MeshInstance3D AddMesh(Node3D parent,Mesh mesh,Vector3 position,Material material)
    {var node=new MeshInstance3D{Mesh=mesh,Position=position,MaterialOverride=material};parent.AddChild(node);return node;}
    private static void Box(Node3D parent,Vector3 size,Vector3 position,Color color)=>AddMesh(parent,new BoxMesh{Size=size},position,Material(color));
}
