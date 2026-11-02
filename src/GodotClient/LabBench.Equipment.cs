using Godot;
using System;
using System.Collections.Generic;
using VirtualStem.Client;

public sealed partial class LabBench
{
    private void Vessel(Node3D node,ToolState tool)
    {
        bool cylinder=tool.Kind=="cylinder",bottle=tool.Kind=="water"||tool.Kind=="stock";
        float height=cylinder?1.08f:bottle?1.02f:0.8f,radius=cylinder?0.17f:0.33f;
        Vector2[] profile;
        if(bottle)
            profile=new[]{new Vector2(0,0.025f),new Vector2(radius-0.02f,0.025f),new Vector2(radius,0.06f),new Vector2(radius,0.74f),
                new Vector2(radius-0.03f,0.79f),new Vector2(0.135f,0.9f),new Vector2(0.13f,1.0f),new Vector2(0.11f,1.0f),new Vector2(0.11f,0.89f),
                new Vector2(radius-0.05f,0.77f),new Vector2(radius-0.022f,0.72f),new Vector2(radius-0.022f,0.065f),new Vector2(0,0.065f)};
        else
            profile=new[]{new Vector2(0,0.02f),new Vector2(radius-0.025f,0.02f),new Vector2(radius,0.045f),new Vector2(radius,height-0.025f),
                new Vector2(radius+0.012f,height),new Vector2(radius-0.012f,height),new Vector2(radius-0.02f,height-0.025f),
                new Vector2(radius-0.02f,0.06f),new Vector2(0,0.06f)};
        Mesh(node,BenchGeometry.Lathe("vessel-"+tool.Kind,profile,72,!bottle&&!cylinder),Vector3.Zero,glass);
        if(cylinder)
            Mesh(node,new CylinderMesh{TopRadius=0.28f,BottomRadius=0.29f,Height=0.035f,RadialSegments=48},new Vector3(0,0.025f,0),ceramic);
        if(tool.VolumeL>0)
        {
            float maxFill=bottle?0.68f:height-0.11f;
            float level=(float)(tool.VolumeL/tool.CapacityL)*maxFill;
            float concentration=(float)(tool.Vessel().ConcentrationMolL??0);
            var liquid=Finish(concentration>0?new Color(0.08f,0.42f+0.12f*(1-Math.Min(1,concentration/2)),0.59f,0.7f):new Color(0.58f,0.8f,0.84f,0.38f),0.055f);
            liquid.Transparency=BaseMaterial3D.TransparencyEnum.Alpha;liquid.MetallicSpecular=0.8f;
            float r=radius-0.025f;
            // The meniscus is a visual lip; displayed values always come from the model.
            var liquidProfile=new[]{new Vector2(0,0.062f),new Vector2(r,0.062f),new Vector2(r,level+0.068f),new Vector2(r-0.012f,level+0.057f),new Vector2(0,level+0.057f)};
            Mesh(node,BenchGeometry.Lathe($"liquid:{tool.Kind}:{level:R}",liquidProfile,64,cache:false),Vector3.Zero,liquid);
        }
        if(bottle)
        {
            var cap=new Node3D{Name="Cap"};node.AddChild(cap);
            var lid=tool.Kind=="water"?Finish(new Color(0.045f,0.24f,0.33f),0.48f):rubber;
            Mesh(cap,new CylinderMesh{TopRadius=0.145f,BottomRadius=0.145f,Height=0.105f,RadialSegments=48},new Vector3(0,1.03f,0),lid);
            for(int i=0;i<24;i++)
            {
                float a=i*Mathf.Tau/24;
                var ridge=Solid(cap,new Vector3(0.014f,0.08f,0.012f),new Vector3(0.145f*Mathf.Cos(a),1.03f,0.145f*Mathf.Sin(a)),lid,0.004f);ridge.Rotation=new Vector3(0,-a,0);
            }
            Solid(node,new Vector3(0.42f,0.3f,0.014f),new Vector3(0,0.47f,radius+0.003f),Finish(new Color(0.9f,0.92f,0.86f),0.83f),0.008f);
            Text3D(node,tool.Kind=="water"?"H₂O":"STOCK",new Vector3(0,0.51f,radius+0.015f),30,new Color(0.06f,0.2f,0.22f),0.004f,false);
            Text3D(node,F(tool.VolumeL*1000,"0.#")+" mL",new Vector3(0,0.39f,radius+0.017f),23,new Color(0.18f,0.29f,0.3f),0.0036f,false);
        }
        else
        {
            var ink=Finish(new Color(0.78f,0.89f,0.9f),0.7f);
            for(int i=1;i<=10;i++)
            {
                float y=0.06f+(height-0.11f)*i/10;
                var path=new List<Vector3>();
                float width=i%2==0?0.15f:0.095f;
                for(int j=0;j<=8;j++)
                {float x=-width+width*j/8;path.Add(new Vector3(x,y,Mathf.Sqrt(radius*radius-x*x)+0.005f));}
                Mesh(node,BenchGeometry.Tube(path,0.004f,5),Vector3.Zero,ink);
                if(i%2==0)Text3D(node,F(tool.CapacityL*1000*i/10,"0"),new Vector3(0.08f,y,radius+0.01f),17,new Color(0.83f,0.94f,0.94f),0.0028f,false);
            }
            Text3D(node,"mL",new Vector3(0.07f,height+0.01f,radius),17,new Color(0.79f,0.9f,0.9f),0.003f,false);
        }
        // Small hanging readout is separate from the etched graduation scale.
        Text3D(node,F(tool.VolumeL*1000,"0.#")+" mL",new Vector3(0,height+0.24f,0),23,new Color(0.05f,0.16f,0.18f),0.004f);
        var body=new StaticBody3D();body.SetMeta("tool",tool.Id);node.AddChild(body);
        using var shape=new CylinderShape3D{Radius=radius+0.025f,Height=height};
        body.AddChild(new CollisionShape3D{Shape=shape,Position=new Vector3(0,height/2,0)});
    }
    private void PowerSupply(Node3D node,ToolState tool)
    {
        var chassis=Finish(new Color(0.72f,0.77f,0.77f),0.36f,0.1f);
        Solid(node,new Vector3(1.0f,0.54f,0.7f),new Vector3(0,0.32f,0),chassis,0.035f);
        Solid(node,new Vector3(0.96f,0.42f,0.018f),new Vector3(0,0.32f,0.36f),Finish(new Color(0.1f,0.17f,0.19f),0.5f));
        Solid(node,new Vector3(0.55f,0.20f,0.018f),new Vector3(-0.12f,0.39f,0.378f),rubber,0.01f);
        Text3D(node,F(tool.Value,"0.00")+" V",new Vector3(-0.12f,0.40f,0.39f),30,new Color(0.4f,0.91f,0.74f),0.004f,false);
        Text3D(node,"DC  POWER SUPPLY",new Vector3(-0.04f,0.19f,0.38f),17,new Color(0.76f,0.87f,0.87f),0.003f,false);
        var knob=Mesh(node,new CylinderMesh{TopRadius=0.09f,BottomRadius=0.09f,Height=0.06f,RadialSegments=32},new Vector3(0.34f,0.35f,0.40f),rubber);knob.RotationDegrees=new Vector3(90,0,0);
        Solid(node,new Vector3(0.01f,0.06f,0.005f),new Vector3(0.34f,0.37f,0.435f),ceramic,0.002f);
        for(int i=0;i<9;i++)Solid(node,new Vector3(0.05f,0.013f,0.3f),new Vector3(-0.25f+i*0.06f,0.594f,-0.08f),rubber,0.004f);
        foreach(float x in new[]{-0.37f,0.37f})foreach(float z in new[]{-0.23f,0.23f})
            Mesh(node,new CylinderMesh{TopRadius=0.07f,BottomRadius=0.07f,Height=0.065f,RadialSegments=16},new Vector3(x,0.04f,z),rubber);
        DeviceBody(node,tool.Id,new Vector3(1.0f,0.6f,0.72f));
    }
    private void Resistor(Node3D node,ToolState tool)
    {
        Solid(node,new Vector3(1.05f,0.045f,0.48f),new Vector3(0,0.035f,0),ceramic,0.025f);
        var body=new Node3D{Position=new Vector3(0,0.22f,0),Rotation=new Vector3(0,0,Mathf.Pi/2)};node.AddChild(body);
        var profile=new[]{new Vector2(0,-0.3f),new Vector2(0.075f,-0.3f),new Vector2(0.11f,-0.25f),new Vector2(0.11f,0.25f),new Vector2(0.075f,0.3f),new Vector2(0,0.3f)};
        Mesh(body,BenchGeometry.Lathe("resistor-body",profile,40),Vector3.Zero,Finish(new Color(0.64f,0.51f,0.32f),0.42f));
        Color[] palette={new(0.02f,0.02f,0.02f),new(0.18f,0.07f,0.02f),new(0.7f,0.035f,0.02f),new(0.92f,0.32f,0.02f),new(0.88f,0.68f,0.02f),new(0.03f,0.35f,0.1f),new(0.02f,0.2f,0.65f),new(0.4f,0.06f,0.53f),new(0.4f,0.42f,0.43f),new(0.88f,0.88f,0.86f)};
        int exponent=(int)Math.Floor(Math.Log10(tool.Value))-2,significant=(int)Math.Round(tool.Value/Math.Pow(10,exponent));
        var multiplier=exponent<0?new Color(0.58f,0.43f,0.1f):palette[Math.Min(9,exponent)];
        Color[] colors={palette[significant/100%10],palette[significant/10%10],palette[significant%10],multiplier,new Color(0.58f,0.43f,0.1f)};
        for(int i=0;i<5;i++)
            Mesh(body,new CylinderMesh{TopRadius=0.112f,BottomRadius=0.112f,Height=0.027f,RadialSegments=40},new Vector3(0,-0.18f+i*0.09f,0),Finish(colors[i],0.5f));
        foreach(float sign in new[]{-1f,1f})
            Mesh(node,BenchGeometry.Tube(new[]{new Vector3(sign*0.27f,0.22f,0),new Vector3(sign*0.36f,0.22f,0),new Vector3(sign*0.43f,0.15f,0)},0.018f),Vector3.Zero,metal);
        Text3D(node,F(tool.Value)+" Ω",new Vector3(0,0.5f,0),23,new Color(0.08f,0.18f,0.18f),0.004f);
        DeviceBody(node,tool.Id,new Vector3(0.65f,0.36f,0.42f));
    }
    private void Meter(Node3D node,ToolState tool,string reading)
    {
        Solid(node,new Vector3(0.72f,0.18f,1.0f),new Vector3(0,0.12f,0),rubber,0.04f);
        Solid(node,new Vector3(0.65f,0.13f,0.94f),new Vector3(0,0.20f,0),Finish(new Color(0.78f,0.51f,0.065f),0.5f),0.025f);
        Solid(node,new Vector3(0.52f,0.015f,0.3f),new Vector3(0,0.275f,-0.22f),rubber,0.015f);
        Solid(node,new Vector3(0.46f,0.012f,0.25f),new Vector3(0,0.286f,-0.22f),Finish(new Color(0.43f,0.57f,0.46f),0.24f),0.01f);
        var digits=Text3D(node,reading,new Vector3(0,0.299f,-0.22f),28,new Color(0.02f,0.10f,0.08f),0.0025f,false);digits.Rotation=new Vector3(-Mathf.Pi/2,0,0);
        Mesh(node,new CylinderMesh{TopRadius=0.15f,BottomRadius=0.16f,Height=0.04f,RadialSegments=48},new Vector3(0,0.30f,0.13f),rubber);
        var dial=Solid(node,new Vector3(0.055f,0.035f,0.24f),new Vector3(0,0.335f,0.13f),Finish(new Color(0.1f,0.14f,0.15f),0.65f),0.012f);
        dial.Rotation=new Vector3(0,tool.Mode=="current"?-0.7f:0.65f,0);
        for(int i=0;i<10;i++)
        {
            float a=i*Mathf.Tau/10;
            var tick=Solid(node,new Vector3(0.01f,0.005f,0.035f),new Vector3(0.2f*Mathf.Sin(a),0.27f,0.13f+0.2f*Mathf.Cos(a)),rubber,0.002f);tick.Rotation=new Vector3(0,a,0);
        }
        Text3D(node,reading,new Vector3(0,0.63f,0),23,new Color(0.05f,0.19f,0.18f),0.0038f);
        DeviceBody(node,tool.Id,new Vector3(0.72f,0.29f,1.0f));
    }
}
