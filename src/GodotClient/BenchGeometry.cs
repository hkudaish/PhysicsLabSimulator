using Godot;
using System;
using System.Collections.Generic;

// Reusable authored profiles: hollow vessels, rounded enclosures and continuous cables.
// These meshes are presentation assets and never determine scientific quantities.
public static class BenchGeometry
{
    private static readonly Dictionary<string,Mesh> Cache=new();
    public static void ReleaseCache()
    {foreach(var mesh in Cache.Values)mesh.Dispose();Cache.Clear();}
    public static Mesh Lathe(string key,Vector2[] profile,int segments=64,bool spout=false,bool cache=true)
    {
        if(cache&&Cache.TryGetValue(key,out var cached))return cached;
        using var surface=new SurfaceTool();surface.Begin(Mesh.PrimitiveType.Triangles);
        for(int j=0;j<profile.Length-1;j++)for(int i=0;i<segments;i++)
        {
            Vertex(j,i);Vertex(j,i+1);Vertex(j+1,i+1);
            Vertex(j,i);Vertex(j+1,i+1);Vertex(j+1,i);
        }
        surface.Index();var mesh=surface.Commit();if(cache)Cache[key]=mesh;return mesh;
        void Vertex(int row,int column)
        {
            float angle=column*Mathf.Tau/segments;
            var point=profile[row];var before=profile[Math.Max(0,row-1)];var after=profile[Math.Min(profile.Length-1,row+1)];
            var tangent=after-before;
            var normal=new Vector3(tangent.Y*Mathf.Cos(angle),-tangent.X,tangent.Y*Mathf.Sin(angle)).Normalized();
            if(normal.LengthSquared()<0.01f)normal=Vector3.Up;
            float lip=spout?Mathf.Pow(Mathf.Max(0,Mathf.Sin(angle)),24)*Mathf.SmoothStep(0.62f,0.79f,point.Y)*0.11f:0;
            surface.SetNormal(normal);surface.SetUV(new Vector2((float)column/segments,point.Y));
            surface.AddVertex(new Vector3(point.X*Mathf.Cos(angle),point.Y,point.X*Mathf.Sin(angle)+lip));
        }
    }
    public static Mesh RoundedBox(Vector3 size,float bevel=0.025f)
    {
        string key=$"box:{size}:{bevel}";if(Cache.TryGetValue(key,out var cached))return cached;
        float b=Mathf.Min(bevel,Mathf.Min(size.Y/3,Mathf.Min(size.X,size.Z)/4));
        int count=32;using var s=new SurfaceTool();s.Begin(Mesh.PrimitiveType.Triangles);
        for(int layer=0;layer<3;layer++)for(int i=0;i<count;i++)
        {
            V(layer,i);V(layer,i+1);V(layer+1,i+1);V(layer,i);V(layer+1,i+1);V(layer+1,i);
        }
        for(int i=0;i<count;i++)
        {
            Cap(0,i+1,i,Vector3.Down);Cap(3,i,i+1,Vector3.Up);
        }
        s.Index();s.GenerateTangents();var mesh=s.Commit();Cache[key]=mesh;return mesh;
        Vector3 Point(int layer,int index)
        {
            int k=index%count,corner=k/8;float a=(corner*90+(k%8)*90f/7)*Mathf.Pi/180;
            float inset=layer==0||layer==3?b:0;
            float x=size.X/2-inset,z=size.Z/2-inset,r=Mathf.Min(0.065f,Mathf.Min(x,z)*0.45f);
            float cx=(corner==0||corner==3?1:-1)*(x-r),cz=(corner<2?1:-1)*(z-r);
            float y=layer switch{0=>-size.Y/2,1=>-size.Y/2+b,2=>size.Y/2-b,_=>size.Y/2};
            return new Vector3(cx+r*Mathf.Cos(a),y,cz+r*Mathf.Sin(a));
        }
        void V(int layer,int index)
        {
            var p=Point(layer,index);float angle=((index%count)/8*90+((index%8)*90f/7))*Mathf.Pi/180;
            var horizontal=new Vector3(Mathf.Cos(angle),0,Mathf.Sin(angle));
            var n=layer==0?(horizontal+Vector3.Down).Normalized():layer==3?(horizontal+Vector3.Up).Normalized():horizontal;
            s.SetNormal(n);s.SetUV(new Vector2((float)index/count,p.Y));s.AddVertex(p);
        }
        void Cap(int layer,int first,int second,Vector3 n)
        {
            foreach(var p in new[]{new Vector3(0,Point(layer,0).Y,0),Point(layer,first),Point(layer,second)})
            {s.SetNormal(n);s.SetUV(new Vector2(p.X,p.Z));s.AddVertex(p);}
        }
    }
    public static Mesh Tube(IReadOnlyList<Vector3> path,float radius,int sides=8)
    {
        using var s=new SurfaceTool();s.Begin(Mesh.PrimitiveType.Triangles);
        for(int j=0;j<path.Count-1;j++)for(int i=0;i<sides;i++)
        {V(j,i);V(j,i+1);V(j+1,i+1);V(j,i);V(j+1,i+1);V(j+1,i);}
        s.Index();return s.Commit();
        void V(int row,int column)
        {
            var tangent=(path[Math.Min(path.Count-1,row+1)]-path[Math.Max(0,row-1)]).Normalized();
            var side=tangent.Cross(Math.Abs(tangent.Dot(Vector3.Up))>0.95?Vector3.Right:Vector3.Up).Normalized();
            var up=tangent.Cross(side).Normalized();float a=column*Mathf.Tau/sides;
            var n=side*Mathf.Cos(a)+up*Mathf.Sin(a);
            s.SetNormal(n);s.SetUV(new Vector2((float)column/sides,(float)row/path.Count));s.AddVertex(path[row]+n*radius);
        }
    }
}
