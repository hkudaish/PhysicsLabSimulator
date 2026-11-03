using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using VirtualStem.Client;
using VirtualStem.Science;

public sealed partial class LabBench
{
    private void SpecialVessel(Node3D node,ToolState tool)
    {
        Vector2[] outer=tool.Kind switch
        {
            "erlenmeyer"=>new[]{new Vector2(.30f,.03f),new(.34f,.08f),new(.34f,.20f),new(.12f,.73f),new(.12f,.96f)},
            "florence"=>new[]{new Vector2(.12f,.03f),new(.28f,.13f),new(.35f,.35f),new(.28f,.57f),new(.13f,.72f),new(.13f,1.04f)},
            "testtube"=>new[]{new Vector2(.035f,.025f),new(.10f,.06f),new(.13f,.14f),new(.13f,.90f)},
            "burette"=>new[]{new Vector2(.025f,.10f),new(.045f,.35f),new(.075f,.43f),new(.075f,1.65f)},
            "pipette"=>new[]{new Vector2(.02f,.03f),new(.055f,.18f),new(.055f,1.3f)},
            "volpipette"=>new[]{new Vector2(.02f,.03f),new(.045f,.20f),new(.045f,.45f),new(.14f,.55f),new(.14f,.77f),new(.045f,.87f),new(.045f,1.35f)},
            "funnel"=>new[]{new Vector2(.045f,.03f),new(.045f,.27f),new(.32f,.78f)},
            _=>new[]{new Vector2(.045f,.025f),new(.20f,.05f),new(.34f,.14f)}
        };
        float bottom=outer[0].Y+.025f,top=outer[^1].Y-.03f,maxR=outer.Max(v=>v.X);
        var profile=new List<Vector2>{new(0,outer[0].Y)};profile.AddRange(outer);
        for(int i=outer.Length-1;i>=0;i--)profile.Add(new Vector2(Math.Max(.008f,outer[i].X-.014f),Math.Max(bottom,outer[i].Y)));
        profile.Add(new Vector2(0,bottom));
        Mesh(node,BenchGeometry.Lathe("special-"+tool.Kind,profile.ToArray(),56),Vector3.Zero,glass);
        float Radius(float y)
        {
            for(int i=1;i<outer.Length;i++)if(y<=outer[i].Y)
                return Math.Max(.006f,Mathf.Lerp(outer[i-1].X,outer[i].X,(y-outer[i-1].Y)/(outer[i].Y-outer[i-1].Y))-.019f);
            return maxR-.019f;
        }
        const int bins=80;float dy=(top-bottom)/bins;
        var volumes=new float[bins];float total=0;
        for(int i=0;i<bins;i++){float r=Radius(bottom+(i+.5f)*dy);volumes[i]=Mathf.Pi*r*r*dy;total+=volumes[i];}
        float Level(double fraction)
        {
            float required=(float)fraction*total,acc=0;
            for(int i=0;i<bins;i++){if(acc+volumes[i]>=required)return bottom+i*dy+(required-acc)/volumes[i]*dy;acc+=volumes[i];}
            return top;
        }
        if(tool.VolumeL>0)
        {
            float y=Level(tool.VolumeL/tool.CapacityL);var fill=new List<Vector2>{new(0,bottom)};
            for(int i=0;i<=bins;i++){float h=bottom+(y-bottom)*i/bins;fill.Add(new Vector2(Radius(h),h));}
            fill.Add(new(0,y));var liquid=Finish(new Color(.08f,.53f,.65f,.65f),.07f);liquid.Transparency=BaseMaterial3D.TransparencyEnum.Alpha;
            Mesh(node,BenchGeometry.Lathe("fluid",fill.ToArray(),48,cache:false),Vector3.Zero,liquid);
        }
        var ink=Finish(new Color(.8f,.91f,.92f),.8f);
        for(int i=2;i<=10;i+=2)
        {
            float y=Level(i/10.0),r=Radius(y)+.022f;
            Solid(node,new Vector3(.07f,.007f,.006f),new Vector3(0,y,r),ink,.002f);
            Text3D(node,F(tool.CapacityL*1000*i/10,"0"),new Vector3(.08f,y,r),15,new Color(.82f,.94f,.95f),.0025f,false);
        }
        if(tool.Kind=="burette")
        {
            Solid(node,new Vector3(.22f,.04f,.08f),new Vector3(.07f,.36f,0),Finish(new Color(.65f,.06f,.04f),.5f));
            Solid(node,new Vector3(.5f,.035f,.45f),new Vector3(0,.018f,0),metal);
            Mesh(node,new CylinderMesh{TopRadius=.018f,BottomRadius=.018f,Height=1.7f},new Vector3(-.25f,.87f,0),metal);
        }
        else if(tool.Kind is "pipette" or "volpipette" or "testtube")
            Solid(node,new Vector3(.4f,.04f,.36f),new Vector3(0,.02f,0),ceramic);
        Text3D(node,F(tool.VolumeL*1000,"0.#")+" mL",new Vector3(0,outer[^1].Y+.22f,0),22,new Color(.08f,.19f,.2f),.004f);
        var body=new StaticBody3D();body.SetMeta("tool",tool.Id);node.AddChild(body);
        using var shape=new CylinderShape3D{Radius=Math.Max(.1f,maxR),Height=outer[^1].Y};body.AddChild(new CollisionShape3D{Shape=shape,Position=new Vector3(0,outer[^1].Y/2,0)});
    }
    private void ExtraDevice(Node3D node,ToolState tool,SessionData data,CircuitResult? result)
    {
        var blue=Finish(new Color(.055f,.28f,.34f),.46f);var red=Finish(new Color(.7f,.025f,.015f),.35f);
        var beige=Finish(new Color(.76f,.71f,.56f),.6f);
        void Cylinder(float radius,float height,Vector3 at,Material material)=>Mesh(node,new CylinderMesh{TopRadius=radius,BottomRadius=radius,Height=height,RadialSegments=32},at,material);
        void Line(Vector3 a,Vector3 b,float radius,Material material)=>Mesh(node,BenchGeometry.Tube(new[]{a,b},radius),Vector3.Zero,material);
        if(tool.Kind is "meter" or "voltmeter" or "ammeter") {Meter(node,tool,Reading(tool,data,result));return;}
        if(tool.Kind=="rheostat")
        {
            Solid(node,new Vector3(.95f,.12f,.5f),new Vector3(0,.08f,0),ceramic);
            for(int i=0;i<16;i++)Mesh(node,new TorusMesh{InnerRadius=.11f,OuterRadius=.15f,Rings=24,RingSegments=6},new Vector3(-.32f+i*.043f,.25f,0),metal).Rotation=new Vector3(0,0,Mathf.Pi/2);
            Solid(node,new Vector3(.12f,.1f,.25f),new Vector3((float)(tool.Value/10000-.5)*.65f,.37f,0),blue);
        }
        else if(tool.Kind=="switch")
        {
            Solid(node,new Vector3(.95f,.08f,.5f),new Vector3(0,.05f,0),beige);
            foreach(float x in new[]{-.35f,.35f})Cylinder(.07f,.16f,new Vector3(x,.16f,0),metal);
            var lever=Solid(node,new Vector3(.7f,.045f,.09f),new Vector3(0,tool.Mode=="closed"?.25f:.43f,0),metal,.01f);
            lever.Rotation=new Vector3(0,0,tool.Mode=="closed"?0:.55f);Cylinder(.065f,.09f,new Vector3(-.3f,.53f,0),rubber);
        }
        else if(tool.Kind=="bulb")
        {
            Cylinder(.32f,.09f,new Vector3(0,.06f,0),ceramic);Cylinder(.13f,.22f,new Vector3(0,.20f,0),metal);
            Mesh(node,new SphereMesh{Radius=.27f,Height=.54f,RadialSegments=40,Rings=20},new Vector3(0,.58f,0),glass);
            double power=result?.HasReadings==true&&result.Branches.TryGetValue(tool.Id,out var reading)?reading.PowerW:0;
            var filament=Finish(new Color(.75f,.4f,.07f),.6f);filament.EmissionEnabled=power>0;filament.Emission=new Color(1,.45f,.08f);filament.EmissionEnergyMultiplier=(float)Math.Min(4,power);
            Line(new Vector3(-.08f,.34f,0),new Vector3(-.06f,.6f,0),.008f,metal);Line(new Vector3(.08f,.34f,0),new Vector3(.06f,.6f,0),.008f,metal);
            Mesh(node,BenchGeometry.Tube(new[]{new Vector3(-.06f,.6f,0),new Vector3(0,.64f,0),new Vector3(.06f,.6f,0)},.012f),Vector3.Zero,filament);
        }
        else if(tool.Kind is "diode" or "led")
        {
            Solid(node,new Vector3(.85f,.035f,.45f),new Vector3(0,.025f,0),ceramic);
            if(tool.Kind=="led")
            {
                var tint=Finish(new Color(.6f,.025f,.015f),.15f);double current=result?.HasReadings==true&&result.Branches.TryGetValue(tool.Id,out var br)?br.CurrentA:0;
                tint.EmissionEnabled=current>0;tint.Emission=new Color(1,.015f,.005f);tint.EmissionEnergyMultiplier=(float)Math.Clamp(current/.01,0,3);
                Cylinder(.13f,.2f,new Vector3(0,.29f,0),tint);Mesh(node,new SphereMesh{Radius=.13f,Height=.26f},new Vector3(0,.39f,0),tint);
            }
            else
            {
                Mesh(node,new CylinderMesh{TopRadius=.1f,BottomRadius=.1f,Height=.4f,RadialSegments=32},new Vector3(0,.23f,0),rubber).Rotation=new Vector3(0,0,Mathf.Pi/2);
                Mesh(node,new CylinderMesh{TopRadius=.103f,BottomRadius=.103f,Height=.035f},new Vector3(.13f,.23f,0),ceramic).Rotation=new Vector3(0,0,Mathf.Pi/2);
            }
            foreach(float sign in new[]{-1f,1f})Line(new Vector3(sign*.15f,.23f,0),new Vector3(sign*.4f,.15f,0),.015f,metal);
        }
        else if(tool.Kind=="capacitor")
        {
            Cylinder(.19f,.6f,new Vector3(0,.36f,0),blue);Cylinder(.17f,.012f,new Vector3(0,.67f,0),metal);
            Solid(node,new Vector3(.014f,.5f,.006f),new Vector3(.11f,.36f,.155f),ceramic,.002f);
            Text3D(node,F(tool.Value*1e6)+" µF",new Vector3(0,.9f,0),22,new Color(.06f,.18f,.19f),.004f);
        }
        else if(tool.Kind=="inductor")
        {
            Solid(node,new Vector3(.9f,.06f,.5f),new Vector3(0,.04f,0),ceramic);
            for(int i=0;i<14;i++)Mesh(node,new TorusMesh{InnerRadius=.12f,OuterRadius=.17f,Rings=32,RingSegments=8},new Vector3(-.3f+i*.045f,.26f,0),Finish(new Color(.65f,.29f,.07f),.25f,.75f)).Rotation=new Vector3(0,0,Mathf.Pi/2);
        }
        else if(ToolCatalog.Logic(tool.Kind))
        {
            Solid(node,new Vector3(.9f,.045f,.65f),new Vector3(0,.04f,0),Finish(new Color(.02f,.25f,.13f),.5f));
            Solid(node,new Vector3(.45f,.12f,.32f),new Vector3(0,.14f,0),rubber);
            Text3D(node,tool.Kind.ToUpperInvariant(),new Vector3(0,.35f,0),24,new Color(.06f,.16f,.17f),.004f);
            bool output=ToolCatalog.Output(tool.Kind,tool.Mode[0]=='1',tool.Mode[1]=='1');Cylinder(.06f,.06f,new Vector3(.3f,.12f,.18f),output?red:rubber);
            Text3D(node,"Y="+(output?1:0),new Vector3(0,.48f,0),20,new Color(.06f,.18f,.2f),.004f);
        }
        else if(tool.Kind=="element")
        {
            var e=Elements.Find(tool.Mode);Color tint=e==null?new Color(.3f,.4f,.5f):new Color(e.Color.Length==6?"#"+e.Color:"#7799aa");
            Cylinder(.24f,.04f,new Vector3(0,.03f,0),ceramic);
            Mesh(node,new CylinderMesh{TopRadius=.2f,BottomRadius=.2f,Height=.6f,RadialSegments=40},new Vector3(0,.36f,0),glass);
            Mesh(node,new SphereMesh{Radius=.13f,Height=.26f},new Vector3(0,.22f,0),Finish(tint,.3f,.25f));
            Cylinder(.21f,.08f,new Vector3(0,.7f,0),blue);Text3D(node,tool.Mode,new Vector3(0,.91f,0),30,new Color(.08f,.2f,.2f),.005f);
        }
        else if(tool.Kind=="balance")
        {
            Solid(node,new Vector3(.98f,.1f,.7f),new Vector3(0,.07f,0),ceramic);Cylinder(.09f,.45f,new Vector3(-.18f,.3f,0),metal);
            for(int i=0;i<3;i++)Solid(node,new Vector3(.78f,.035f,.055f),new Vector3(.12f,.48f,i*.08f),metal,.008f);
            Cylinder(.25f,.025f,new Vector3(-.25f,.6f,0),metal);Text3D(node,Reading(tool,data,result),new Vector3(0,.82f,0),25,new Color(.08f,.2f,.2f),.004f);
        }
        else if(tool.Kind is "phmeter" or "thermometer")
        {
            Solid(node,new Vector3(.6f,.12f,.7f),new Vector3(0,.09f,0),tool.Kind=="phmeter"?blue:ceramic);
            Cylinder(.035f,.85f,new Vector3(.3f,.5f,0),glass);Line(new Vector3(.3f,.12f,0),new Vector3(.3f,.65f,0),.012f,red);
            Text3D(node,Reading(tool,data,result),new Vector3(0,.4f,0),25,new Color(.08f,.2f,.2f),.004f);
        }
        else if(tool.Kind is "stand" or "clamp" or "holder" or "condenser")
        {
            Solid(node,new Vector3(.65f,.045f,.6f),new Vector3(0,.03f,0),rubber);Cylinder(.025f,1.3f,new Vector3(-.22f,.68f,0),metal);
            Line(new Vector3(-.22f,.95f,0),new Vector3(.2f,.95f,0),.023f,metal);
            Mesh(node,new TorusMesh{InnerRadius=.2f,OuterRadius=.22f,Rings=32,RingSegments=8},new Vector3(.1f,.95f,0),metal);
            if(tool.Kind=="condenser")
            {Cylinder(.12f,1.1f,new Vector3(.13f,.65f,0),glass);Cylinder(.045f,1.3f,new Vector3(.13f,.65f,0),glass);}
        }
        else if(tool.Kind is "mortar" or "crucible")
        {
            Mesh(node,BenchGeometry.Lathe("bowl-"+tool.Kind,new[]{new Vector2(0,.03f),new(.2f,.03f),new(.32f,.28f),new(.30f,.34f),new(.25f,.3f),new(.1f,.12f),new(0,.12f)},48),Vector3.Zero,ceramic);
            if(tool.Kind=="mortar")Mesh(node,new CylinderMesh{TopRadius=.05f,BottomRadius=.07f,Height=.55f},new Vector3(.12f,.38f,0),ceramic).Rotation=new Vector3(0,0,.6f);
            else Cylinder(.33f,.035f,new Vector3(.05f,.36f,0),ceramic);
        }
        else if(tool.Kind=="burner")
        {Cylinder(.28f,.06f,new Vector3(0,.04f,0),rubber);Cylinder(.055f,.65f,new Vector3(0,.37f,0),metal);Line(new Vector3(0,.12f,0),new Vector3(.4f,.12f,0),.035f,blue);}
        else if(tool.Kind is "ceramic" or "gauze" or "triangle")
        {
            Solid(node,new Vector3(.7f,.035f,.65f),new Vector3(0,.04f,0),tool.Kind=="ceramic"?ceramic:metal);
            if(tool.Kind=="gauze")for(int i=0;i<10;i++){Line(new Vector3(-.32f+i*.07f,.07f,-.32f),new Vector3(-.32f+i*.07f,.07f,.32f),.004f,rubber);}
            if(tool.Kind=="triangle")Mesh(node,BenchGeometry.Tube(new[]{new Vector3(-.28f,.09f,.2f),new Vector3(.28f,.09f,.2f),new Vector3(0,.09f,-.27f),new Vector3(-.28f,.09f,.2f)},.025f),Vector3.Zero,ceramic);
        }
        else if(tool.Kind=="bulb-pipette"||tool.Kind=="stopper")
        {Mesh(node,new SphereMesh{Radius=.2f,Height=.4f},new Vector3(0,.23f,0),tool.Kind=="stopper"?rubber:red);Cylinder(.06f,.2f,new Vector3(0,.45f,0),rubber);}
        else if(tool.Kind is "tongs" or "scoopula" or "brush")
        {
            foreach(float z in tool.Kind=="tongs"?new[]{-.07f,.07f}:new[]{0f})Line(new Vector3(-.4f,.09f,z),new Vector3(.4f,.09f,z),.018f,metal);
            if(tool.Kind=="brush")for(int i=0;i<12;i++)Cylinder(.07f,.018f,new Vector3(.18f+i*.015f,.1f,0),beige);
            if(tool.Kind=="scoopula")Solid(node,new Vector3(.24f,.035f,.14f),new Vector3(.32f,.09f,0),metal);
        }
        else
        {
            Solid(node,new Vector3(.8f,.4f,.55f),new Vector3(0,.23f,0),tool.Kind=="generator"?blue:rubber);
            if(tool.Kind=="transistor")foreach(float x in new[]{-.18f,0f,.18f})Line(new Vector3(x,.05f,.2f),new Vector3(x,.3f,.2f),.014f,metal);
            Text3D(node,tool.Kind.ToUpperInvariant(),new Vector3(0,.62f,0),19,new Color(.07f,.2f,.2f),.0034f);
        }
        DeviceBody(node,tool.Id,ToolCatalog.Electrical(tool.Kind)?new Vector3(.65f,.92f,.55f):new Vector3(.95f,.95f,.85f));
    }
}
