using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Globalization;

namespace VirtualStem.Client
{
    public sealed record ToolDefinition(string Kind,string Arabic,string English,string Category,string Behavior,double Capacity=0.25,double Initial=0);
    public static class ToolCatalog
    {
        public static readonly ToolDefinition[] All={
            new("beaker","كأس زجاجي","Beaker","vessels","fluid"),new("cylinder","مخبار مدرج","Graduated cylinder","vessels","fluid"),
            new("erlenmeyer","دورق مخروطي","Erlenmeyer flask","vessels","fluid"),new("florence","دورق فلورنس","Florence flask","vessels","fluid"),
            new("testtube","أنبوب اختبار","Test tube","vessels","fluid",0.025),new("burette","سحاحة","Burette","vessels","fluid",0.05),
            new("pipette","ماصة مدرجة","Graduated pipette","vessels","fluid",0.01),new("volpipette","ماصة حجمية 25 mL","25 mL volumetric pipette","vessels","fluid",0.025),
            new("funnel","قمع","Funnel","vessels","fluid",0.05),new("watchglass","زجاجة ساعة","Watch glass","vessels","fluid",0.01),
            new("stock","محلول قياسي","Stock solution","reagents","fluid",0.25),new("water","ماء نقي","Pure water","reagents","fluid",0.5),
            new("meter","مجس التركيز","Concentration probe","instruments","concentration"),new("phmeter","مجس pH مثالي","Ideal pH probe","instruments","ph"),
            new("balance","ميزان ثلاثي الذراع","Triple-beam balance","instruments","mass"),new("thermometer","مقياس حرارة","Thermometer","instruments","temperature"),
            new("stand","حامل حلقي","Ring stand","apparatus","reference"),new("condenser","مكثف زجاجي","Condenser","apparatus","reference"),
            new("mortar","هاون ومدقة","Mortar and pestle","apparatus","reference"),new("crucible","بوتقة مع غطاء","Crucible and cover","apparatus","reference"),
            new("burner","موقد بنزن","Bunsen burner","apparatus","reference"),new("clamp","مشبك تثبيت","Clamp","apparatus","reference"),
            new("holder","ماسك أنابيب","Test tube holder","apparatus","reference"),new("bulb-pipette","ماصة مطاطية","Pipette bulb","apparatus","reference"),
            new("brush","فرشاة أنابيب","Test tube brush","apparatus","reference"),new("scoopula","مغرفة مختبر","Scoopula","apparatus","reference"),
            new("stopper","سدادة مطاطية","Rubber stopper","apparatus","reference"),new("ceramic","مربع سيراميك","Ceramic square","apparatus","reference"),
            new("gauze","شبكة سلكية","Wire gauze","apparatus","reference"),new("triangle","مثلث طيني","Clay triangle","apparatus","reference"),
            new("tongs","ملقط بوتقة","Crucible tongs","apparatus","reference"),new("element","عينة عنصر افتراضية","Virtual element sample","elements","sample"),
            new("battery","مصدر DC","DC source","sources","source",Initial:9),new("resistor","مقاومة","Resistor","passive","resistance",Initial:1000),
            new("rheostat","مقاومة متغيرة","Rheostat","passive","resistance",Initial:1000),new("bulb","مصباح مقاومي","Resistive lamp","passive","resistance",Initial:100),
            new("switch","مفتاح","Switch","passive","switch"),new("capacitor","مكثف كهربائي","Capacitor","passive","steady",Initial:0.0001),
            new("inductor","ملف حثي","Inductor","passive","steady",Initial:0.1),new("diode","ثنائي","Diode","semiconductors","junction",Initial:0.7),
            new("led","صمام LED","LED","semiconductors","junction",Initial:2),new("voltmeter","فولتميتر","Voltmeter","electrical-meters","voltage"),
            new("ammeter","أميتر","Ammeter","electrical-meters","current"),
            new("and","بوابة AND","AND gate","logic","logic"),new("or","بوابة OR","OR gate","logic","logic"),new("not","بوابة NOT","NOT gate","logic","logic"),
            new("nand","بوابة NAND","NAND gate","logic","logic"),new("nor","بوابة NOR","NOR gate","logic","logic"),new("xor","بوابة XOR","XOR gate","logic","logic"),
            new("transistor","ترانزستور NPN","NPN transistor","circuit-reference","reference"),new("relay","مرحل","Relay","circuit-reference","reference"),
            new("generator","مولد إشارة","Signal generator","circuit-reference","reference")
        };
        public static ToolDefinition? Find(string kind)=>All.FirstOrDefault(d=>d.Kind==kind);
        public static bool Fluid(string kind)=>Find(kind)?.Behavior=="fluid";
        public static bool Logic(string kind)=>Find(kind)?.Behavior=="logic";
        public static bool Electrical(string kind)=>kind=="meter"||Find(kind)?.Category is "sources" or "passive" or "semiconductors" or "electrical-meters";
        public static bool Chemistry(string kind)=>Find(kind)?.Category is "vessels" or "reagents" or "instruments" or "apparatus" or "elements";
        public static bool Circuit(string kind)=>Electrical(kind)||Logic(kind)||Find(kind)?.Category=="circuit-reference";
        public static bool Output(string kind,bool a,bool b)=>kind switch{"and"=>a&&b,"or"=>a||b,"not"=>!a,"nand"=>!(a&&b),"nor"=>!(a||b),"xor"=>a!=b,_=>false};
        public static ToolState Create(string kind,string experiment)
        {
            var def=Find(kind)??throw new ArgumentException("Unknown tool.");
            var tool=new ToolState{Id=Guid.NewGuid().ToString("N"),Kind=kind,CapacityL=def.Capacity,Value=def.Initial};
            if(kind=="stock"){tool.VolumeL=0.1;tool.Moles=0.2;tool.SoluteId=VirtualStem.Science.Dilution.IdealSolute;}
            if(kind=="water")tool.VolumeL=0.5;
            if(kind=="meter")tool.Mode=experiment=="chemistry"?"concentration":"voltage";
            if(kind=="phmeter")tool.Mode="ph";
            if(kind=="switch")tool.Mode="open";
            if(Logic(kind))tool.Mode="00";
            if(kind=="element"){tool.Mode="Cu";tool.Value=0.01;}
            if(kind=="ammeter")tool.Mode="current";
            return tool;
        }
    }
    public sealed class ElementInfo
    {
        public int Number{get;set;} public string Symbol{get;set;}="";public string Name{get;set;}="";public string Arabic{get;set;}="";
        public string AtomicMass{get;set;}="";public string State{get;set;}="";public string Category{get;set;}="";public string Color{get;set;}="888888";
        public string Configuration{get;set;}="";
        public double Mass=>double.Parse(AtomicMass.Trim('[',']'),CultureInfo.InvariantCulture);
    }
    public static class Elements
    {
        public static ElementInfo[] All{get;private set;}=Array.Empty<ElementInfo>();
        public static void Load(string json)
        {
            var data=JsonSerializer.Deserialize<ElementInfo[]>(json,new JsonSerializerOptions{PropertyNameCaseInsensitive=true})??throw new ArgumentException("Missing element data.");
            if(data.Length!=118||data.Select(e=>e.Number).Distinct().Count()!=118||data.Select(e=>e.Symbol).Distinct().Count()!=118||
                data.Where((e,i)=>e.Number!=i+1||!double.IsFinite(e.Mass)||e.Mass<=0).Any())throw new ArgumentException("Invalid element reference.");
            All=data;
        }
        public static ElementInfo? Find(string symbol)=>All.FirstOrDefault(e=>e.Symbol==symbol);
        public static (int row,int column) Position(int z)
        {
            if(z<=2)return(1,z==1?1:18);
            if(z<=10)return(2,z<=4?z-2:z+8);
            if(z<=18)return(3,z<=12?z-10:z);
            if(z<=36)return(4,z-18);
            if(z<=54)return(5,z-36);
            if(z<=57)return(6,z-54);
            if(z<=71)return(9,z-54);
            if(z<=86)return(6,z-68);
            if(z<=89)return(7,z-86);
            if(z<=103)return(10,z-86);
            return(7,z-100);
        }
    }
}
