using Godot;
using System;
using System.Linq;
using VirtualStem.Client;
using VirtualStem.Science;

public partial class LabClient
{
    private string ExtraReading(ToolState tool)
    {
        if(tool.Kind=="element")
        {
            var e=Elements.Find(tool.Mode);return e==null?"—":Ltr(e.Symbol+" • Z="+e.Number)+"\n"+Ltr(N(tool.Value,"0.####")+" mol")+"\n"+Ltr(N(tool.Value*e.Mass,"0.0000")+" g")+"\n"+e.State;
        }
        if(tool.Kind=="switch")return tool.Mode=="closed"?T("مغلق — موصل مثالي","Closed — ideal conductor"):T("مفتوح — لا توصيل","Open — no conduction");
        if(ToolCatalog.Logic(tool.Kind))return Ltr("Y = "+(ToolCatalog.Output(tool.Kind,tool.Mode[0]=='1',tool.Mode[1]=='1')?"1":"0"));
        if(tool.Kind is "balance" or "phmeter" or "thermometer")
        {
            var target=Current.Tools.FirstOrDefault(t=>t.Id==tool.Target);
            if(target==null||LabBench.Distance(tool,target)>1.65)return T("اختر عينة قريبة للقياس (≤ 1.65).","Choose a nearby target (≤ 1.65 scene units).");
            if(tool.Kind=="balance")return target.Kind=="element"&&Elements.Find(target.Mode) is ElementInfo e?Ltr(N(target.Value*e.Mass,"0.0000")+" g"):"—";
            if(!Session.Chemical(target.Kind))return "—";
            if(tool.Kind=="thermometer")return Ltr("25.0 °C")+"\n"+T("درجة نموذج ثابتة","Fixed model temperature");
            return SolutionProperties.Ph(target.Vessel()) is double ph?Ltr("pH = "+N(ph,"0.000")):T("غير متاح لهذا المحلول","Unavailable for this solution");
        }
        if(tool.Kind=="capacitor")
        {
            var voltage=electrical?.VoltageBetween(tool.Id+".a",tool.Id+".b");
            return Ltr("C = "+N(tool.Value,"0.######")+" F")+"\n"+Ltr("I = 0 A (DC)")+"\n"+(voltage is double v?Ltr("Q = "+N(tool.Value*v,"0.######")+" C"):"—");
        }
        if(tool.Kind=="inductor")return Ltr("L = "+N(tool.Value)+" H")+"\n"+T("حالة مستقرة: موصل مثالي","Steady state: ideal conductor");
        return ToolCatalog.Find(tool.Kind)?.Behavior=="reference"?T("مرجع ثلاثي الأبعاد","3D reference"):"—";
    }
    private void OpenCircuitPresets()
    {
        var dialog=new AcceptDialog{Title=T("دارات جاهزة","Circuit presets")};AddChild(dialog);var box=new VBoxContainer();dialog.AddChild(box);
        foreach(string preset in new[]{"series","parallel","switch-lamp","led","capacitor"})
        {
            string chosen=preset;var button=new Button{Text=preset switch{"series"=>T("مقاومتان على التوالي","Series resistors"),"parallel"=>T("مقاومتان على التوازي","Parallel resistors"),"switch-lamp"=>T("مصباح ومفتاح","Switch and lamp"),"led"=>T("LED مع مقاومة حماية","LED with series resistor"),_=>T("مكثف — حالة DC مستقرة","Capacitor — DC steady state")},CustomMinimumSize=new Vector2(350,42)};box.AddChild(button);
            button.Pressed+=()=>{LoadCircuitPreset(chosen);dialog.QueueFree();};
        }
        dialog.Confirmed+=()=>dialog.QueueFree();dialog.Canceled+=()=>dialog.QueueFree();dialog.PopupCentered(new Vector2I(420,360));
    }
    private void LoadCircuitPreset(string preset)
    {
        if(Chemical||busy)return;
        Change("Circuit preset "+preset,data=>
        {
            var source=ToolCatalog.Create("battery","electricity");source.Id="battery";source.X=-1.8;source.Z=-0.4;
            var r=ToolCatalog.Create(preset=="switch-lamp"?"bulb":"resistor","electricity");r.Id="resistor";r.X=0;r.Z=-0.4;
            var part=ToolCatalog.Create(preset switch{"led"=>"led","switch-lamp"=>"switch","capacitor"=>"capacitor",_=>"resistor"},"electricity");part.Id="extra";part.X=1.8;part.Z=-0.4;
            if(part.Kind=="switch")part.Mode="closed";
            var meter=ToolCatalog.Create("meter","electricity");meter.Id="multimeter";meter.X=0;meter.Z=1.1;
            data.Tools=new(){source,r,part,meter};data.Wires.Clear();
            void Wire(string a,string b)=>data.Wires.Add(new WireState{Id=Guid.NewGuid().ToString("N"),A=a,B=b});
            Wire("battery.a","resistor.a");
            if(preset=="parallel"){Wire("resistor.b","battery.b");Wire("battery.a","extra.a");Wire("extra.b","battery.b");}
            else {Wire("resistor.b","extra.a");Wire("extra.b","battery.b");}
            Wire("multimeter.a","resistor.a");Wire("multimeter.b","resistor.b");
            data.SelectedId="extra";return true;
        },true);
    }
}
