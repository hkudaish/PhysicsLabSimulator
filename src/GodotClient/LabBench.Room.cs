using Godot;
using System;
using System.Collections.Generic;

public sealed partial class LabBench
{
    private Godot.Environment environment=null!;
    private DirectionalLight3D keyLight=null!;
    private StandardMaterial3D metal=null!,rubber=null!,ceramic=null!;
    private Material glass=null!,worktop=null!;
    private readonly List<Resource> roomResources=new();
    private bool buildingRoom=true,released;
    private T Own<T>(T resource) where T:Resource {roomResources.Add(resource);return resource;}
    public bool SupportsAdvancedRendering=>RenderingServer.GetCurrentRenderingMethod()=="forward_plus";
    public bool HighQuality {get;private set;}
    private void BuildRoom()
    {
        metal=Finish(new Color(0.48f,0.53f,0.55f),0.24f,0.85f);
        rubber=Finish(new Color(0.022f,0.029f,0.035f),0.86f);
        ceramic=Finish(new Color(0.81f,0.83f,0.8f),0.32f);
        glass=Own(new ShaderMaterial{Shader=ResourceLoader.Load<Shader>("res://assets/clear_glass.gdshader")});
        worktop=Grain(new Color(0.14f,0.185f,0.2f),0.4f);
        Sky? sky=null;
        if(SupportsAdvancedRendering)
        {
            var panorama=Own(ResourceLoader.Load<Texture2D>("res://assets/lighting/studio_small_09_1k.hdr",cacheMode:ResourceLoader.CacheMode.Ignore));
            var skyMaterial=Own(new PanoramaSkyMaterial{Panorama=panorama});
            sky=Own(new Sky{SkyMaterial=skyMaterial});
        }
        environment=Own(new Godot.Environment
        {
            BackgroundMode=Godot.Environment.BGMode.Color,BackgroundColor=new Color(0.68f,0.74f,0.76f),
            Sky=sky,
            AmbientLightSource=sky!=null?Godot.Environment.AmbientSource.Sky:Godot.Environment.AmbientSource.Color,
            AmbientLightColor=new Color(0.7f,0.79f,0.8f),AmbientLightEnergy=sky!=null?0.38f:0.6f,
            ReflectedLightSource=sky!=null?Godot.Environment.ReflectionSource.Sky:Godot.Environment.ReflectionSource.Disabled,TonemapMode=Godot.Environment.ToneMapper.Filmic,
            TonemapExposure=0.9f,TonemapWhite=6,SsaoRadius=0.42f,SsaoIntensity=1.5f,SsrMaxSteps=32
        });
        world.AddChild(new WorldEnvironment{Environment=environment});
        keyLight=new DirectionalLight3D{RotationDegrees=new Vector3(-55,-28,0),LightEnergy=1.15f,LightColor=new Color(1,0.94f,0.86f),ShadowEnabled=true};world.AddChild(keyLight);
        world.AddChild(new DirectionalLight3D{RotationDegrees=new Vector3(-28,125,0),LightEnergy=0.32f,LightColor=new Color(0.71f,0.83f,1)});
        // Main epoxy bench, pale cabinetry, slim metal hardware and toe kick.
        Solid(world,new Vector3(7.3f,0.15f,4.2f),new Vector3(0,-0.09f,0),worktop,0.045f);
        Solid(world,new Vector3(7.18f,0.045f,4.08f),new Vector3(0,-0.19f,0),metal);
        Solid(world,new Vector3(6.9f,1.08f,3.7f),new Vector3(0,-0.76f,0),ceramic,0.045f);
        Solid(world,new Vector3(6.7f,0.12f,3.5f),new Vector3(0,-1.32f,0),rubber);
        for(int i=0;i<6;i++)
        {
            float x=-2.85f+i*1.14f;
            Solid(world,new Vector3(1.10f,0.74f,0.035f),new Vector3(x,-0.84f,1.87f),Finish(new Color(0.73f,0.77f,0.76f),0.5f));
            Solid(world,new Vector3(1.10f,0.2f,0.04f),new Vector3(x,-0.34f,1.87f),ceramic);
            Solid(world,new Vector3(0.27f,0.028f,0.07f),new Vector3(x,-0.36f,1.94f),metal,0.012f);
            Solid(world,new Vector3(0.25f,0.028f,0.07f),new Vector3(x,-0.6f,1.94f),metal,0.012f);
        }
        var floor=Grain(new Color(0.48f,0.53f,0.55f),0.72f);
        Solid(world,new Vector3(26,0.12f,26),new Vector3(0,-1.48f,0),floor);
        var wall=Finish(new Color(0.79f,0.83f,0.81f),0.94f);
        Solid(world,new Vector3(22,8,0.2f),new Vector3(0,2.5f,-4.9f),wall);
        Solid(world,new Vector3(0.2f,8,17),new Vector3(-7.2f,2.5f,0),wall);
        Solid(world,new Vector3(22,0.52f,0.035f),new Vector3(0,0.98f,-4.77f),Finish(new Color(0.12f,0.33f,0.34f),0.6f));
        // A daylight window gives the room visual depth without a network dependency.
        var window=Finish(new Color(0.64f,0.8f,0.85f),0.16f);window.EmissionEnabled=true;window.Emission=new Color(0.62f,0.78f,0.83f);window.EmissionEnergyMultiplier=0.7f;
        Solid(world,new Vector3(3.4f,2.25f,0.12f),new Vector3(-3.8f,2.75f,-4.72f),window);
        foreach(float x in new[]{-5.55f,-3.8f,-2.05f})Solid(world,new Vector3(0.055f,2.4f,0.18f),new Vector3(x,2.75f,-4.59f),ceramic);
        foreach(float y in new[]{1.57f,2.75f,3.93f})Solid(world,new Vector3(3.6f,0.055f,0.18f),new Vector3(-3.8f,y,-4.59f),ceramic);
        // Back counter and storage remain outside the interactive placement area.
        Solid(world,new Vector3(10,0.12f,0.95f),new Vector3(0,-0.05f,-4),worktop);
        Solid(world,new Vector3(9.9f,1.18f,0.86f),new Vector3(0,-0.72f,-4),ceramic);
        for(int i=0;i<9;i++)Solid(world,new Vector3(0.015f,1,0.025f),new Vector3(-4.5f+i*1.1f,-0.7f,-3.54f),rubber);
        for(int i=0;i<3;i++)
        {
            Solid(world,new Vector3(1.05f,1.1f,0.43f),new Vector3(0.5f+i*1.15f,2.5f,-4.48f),ceramic);
            Solid(world,new Vector3(0.025f,0.25f,0.07f),new Vector3(0.92f+i*1.15f,2.45f,-4.22f),metal,0.01f);
        }
        for(int i=0;i<5;i++)
        {
            float x=-0.8f+i*0.44f;
            var bottle=new Node3D{Position=new Vector3(x,0.02f,-4)};world.AddChild(bottle);
            Mesh(bottle,new CylinderMesh{TopRadius=0.10f,BottomRadius=0.11f,Height=0.3f,RadialSegments=24},new Vector3(0,0.16f,0),Finish(new Color(0.28f,0.18f,0.08f),0.2f));
            Mesh(bottle,new CylinderMesh{TopRadius=0.065f,BottomRadius=0.065f,Height=0.08f,RadialSegments=24},new Vector3(0,0.35f,0),rubber);
            Solid(bottle,new Vector3(0.15f,0.12f,0.012f),new Vector3(0,0.18f,0.1f),ceramic,0.005f);
        }
        Text3D(world,"S T E M   /   L A B  0 1",new Vector3(0.9f,1.3f,-4.68f),28,new Color(0.83f,0.95f,0.94f),0.009f,false);
        MarkRoomLayers(world);
        buildingRoom=false;
    }
    private static void MarkRoomLayers(Node root)
    {
        if(root is VisualInstance3D visual)visual.Layers=2;
        foreach(Node child in root.GetChildren())MarkRoomLayers(child);
    }
    public void ApplyQuality(bool high)
    {
        HighQuality=high&&SupportsAdvancedRendering;
        environment.SsaoEnabled=HighQuality;environment.SsrEnabled=HighQuality;
        keyLight.ShadowEnabled=true;
        keyLight.LightAngularDistance=HighQuality?2.0f:0;
        keyLight.ShadowBlur=HighQuality?1.5f:1;
        Viewport.Msaa3D=HighQuality?Godot.Viewport.Msaa.Msaa4X:Godot.Viewport.Msaa.Disabled;
    }
    public void Release()
    {
        if(released)return;released=true;CancelPour();
        if(GodotObject.IsInstanceValid(world))world.QueueFree();
        for(int i=roomResources.Count-1;i>=0;i--)roomResources[i].Dispose();
        roomResources.Clear();tools.Clear();terminals.Clear();rings.Clear();signatures.Clear();
    }
    private ShaderMaterial Grain(Color color,float roughness)
    {
        var material=Own(new ShaderMaterial{Shader=ResourceLoader.Load<Shader>("res://assets/grain_surface.gdshader")});
        material.SetShaderParameter("surface_color",color);material.SetShaderParameter("surface_roughness",roughness);
        return material;
    }
    private StandardMaterial3D Finish(Color color,float roughness=0.5f,float metallic=0)
    {
        var material=new StandardMaterial3D{AlbedoColor=color,Roughness=roughness,Metallic=metallic};
        return buildingRoom?Own(material):material;
    }
    private static MeshInstance3D Mesh(Node3D parent,Godot.Mesh mesh,Vector3 position,Material material)
    {var node=new MeshInstance3D{Mesh=mesh,Position=position,MaterialOverride=material};parent.AddChild(node);return node;}
    private static MeshInstance3D Solid(Node3D parent,Vector3 size,Vector3 position,Material material,float bevel=0.02f)=>
        Mesh(parent,BenchGeometry.RoundedBox(size,bevel),position,material);
    private Label3D Text3D(Node3D parent,string text,Vector3 position,int size,Color color,float pixel=0.004f,bool billboard=true)
    {
        var label=new Label3D{Text=text,Font=font,FontSize=size,PixelSize=pixel,Position=position,Modulate=color,
            Billboard=billboard?BaseMaterial3D.BillboardModeEnum.Enabled:BaseMaterial3D.BillboardModeEnum.Disabled,OutlineSize=billboard?2:0};parent.AddChild(label);return label;
    }
}
