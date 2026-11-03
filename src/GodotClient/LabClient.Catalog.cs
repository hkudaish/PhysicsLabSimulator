using Godot;
using System;
using System.Linq;
using VirtualStem.Client;
using VirtualStem.Science;

public partial class LabClient
{
    private void OpenEquipment()
    {
        var dialog=new AcceptDialog{Title=T("خزانة المختبر","Laboratory equipment"),MinSize=new Vector2I(760,540)};AddChild(dialog);
        var box=new VBoxContainer();dialog.AddChild(box);
        var search=new LineEdit{PlaceholderText=T("ابحث بالعربية أو الإنجليزية...","Search Arabic / English...")};box.AddChild(search);
        var category=new OptionButton();box.AddChild(category);
        string[] groups=Chemical?new[]{"all","vessels","instruments","apparatus","reagents"}:new[]{"all","sources","passive","semiconductors","electrical-meters","logic","circuit-reference"};
        foreach(string group in groups)category.AddItem(group switch{"all"=>T("الكل","All"),"vessels"=>T("أوعية وقياس حجم","Vessels / volumetry"),"instruments"=>T("أجهزة القياس","Instruments"),"apparatus"=>T("أدوات مرجعية","Apparatus reference"),"reagents"=>T("محاليل","Solutions"),"sources"=>T("مصادر","Sources"),"passive"=>T("عناصر سلبية","Passive components"),"semiconductors"=>T("ثنائيات و LED","Diodes / LEDs"),"electrical-meters"=>T("أجهزة كهربائية","Electrical instruments"),"logic"=>T("مدرب المنطق الرقمي","Digital logic trainer"),_=>T("مرجع الدارات","Circuit reference")});
        var scroll=new ScrollContainer{CustomMinimumSize=new Vector2(730,360),SizeFlagsVertical=SizeFlags.ExpandFill};box.AddChild(scroll);
        var list=new VBoxContainer{SizeFlagsHorizontal=SizeFlags.ExpandFill};scroll.AddChild(list);
        void Populate()
        {
            foreach(Node child in list.GetChildren()){list.RemoveChild(child);child.QueueFree();}
            string query=search.Text.Trim(),group=groups[category.Selected];
            foreach(var def in ToolCatalog.All.Where(d=>Chemical?ToolCatalog.Chemistry(d.Kind)&&d.Kind!="element":ToolCatalog.Circuit(d.Kind)))
            {
                if(group!="all"&&(def.Kind=="meter"&&!Chemical?"electrical-meters":def.Category)!=group)continue;
                if(query.Length>0&&!def.Arabic.Contains(query,StringComparison.OrdinalIgnoreCase)&&!def.English.Contains(query,StringComparison.OrdinalIgnoreCase))continue;
                string kind=def.Kind;string note=def.Behavior switch{"reference"=>T(" — مرجع ثلاثي الأبعاد"," — 3D reference"),"steady"=>T(" — DC مستقر فقط"," — DC steady state"),"logic"=>T(" — مدرب جدول الحقيقة"," — truth-table trainer"),_=>""};
                var button=new Button{Text=(def.Kind=="meter"&&!Chemical?T("ملتيميتر","Multimeter"):arabic?def.Arabic:def.English)+note,CustomMinimumSize=new Vector2(0,40)};
                list.AddChild(button);button.Pressed+=()=>{if(AddTool(kind))dialog.QueueFree();};
            }
            if(Chemical&&(group=="all"||group=="reagents"))
                foreach(string formula in new[]{"HCl","NaOH","NaCl","CuSO4","AgNO3","BaCl2","FeCl3","KI","Pb(NO3)2","H2SO4","C12H22O11"})
                {
                    if(query.Length>0&&!formula.Contains(query,StringComparison.OrdinalIgnoreCase))continue;
                    string reagent=formula;var button=new Button{Text=Ltr(formula+"  •  0.1 mol/L / 100 mL"),CustomMinimumSize=new Vector2(0,38)};
                    list.AddChild(button);button.Pressed+=()=>{if(AddTool("stock",reagent))dialog.QueueFree();};
                }
        }
        search.TextChanged+=_=>Populate();category.ItemSelected+=_=>Populate();Populate();
        dialog.Confirmed+=()=>dialog.QueueFree();dialog.Canceled+=()=>dialog.QueueFree();dialog.PopupCentered(new Vector2I(800,570));
    }
    private void OpenElements()
    {
        var dialog=new Window{Title=T("الجدول الدوري — 118 عنصراً","Periodic table — 118 elements"),MinSize=new Vector2I(1050,720),Transient=true,Exclusive=true,WrapControls=false};AddChild(dialog);
        var panel=new PanelContainer();panel.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);dialog.AddChild(panel);
        var box=new VBoxContainer();panel.AddChild(box);
        var search=new LineEdit{PlaceholderText=T("ابحث بالرمز أو الاسم أو العدد الذري","Search symbol, name or atomic number")};box.AddChild(search);
        var tableScroll=new ScrollContainer{CustomMinimumSize=new Vector2(1020,460)};box.AddChild(tableScroll);
        var grid=new GridContainer{Columns=18,LayoutDirection=LayoutDirectionEnum.Ltr};tableScroll.AddChild(grid);
        var details=new Label{AutowrapMode=TextServer.AutowrapMode.WordSmart,CustomMinimumSize=new Vector2(0,90)};box.AddChild(details);
        ElementInfo selected=Elements.Find("Cu")!;
        void Select(ElementInfo e)
        {
            selected=e;details.Text=(arabic?e.Arabic:e.Name)+" / "+Ltr(e.Symbol+" • Z="+e.Number)+"\n"+
                T("الكتلة الذرية المرجعية: ","Reference atomic mass: ")+Ltr(e.AtomicMass)+" • "+e.State+" • "+e.Category+"\n"+Ltr(e.Configuration)+"\n"+
                T("عينة افتراضية للفحص وحساب الكتلة؛ لا نفترض تفاعلاً كيميائياً.","Virtual sample for inspection and calculated mass; no chemical reaction is assumed.");
        }
        for(int row=1;row<=10;row++)for(int col=1;col<=18;col++)
        {
            var element=Elements.All.FirstOrDefault(e=>Elements.Position(e.Number)==(row,col));
            if(element==null){grid.AddChild(new Control{CustomMinimumSize=new Vector2(48,26)});continue;}
            var e=element;var button=new Button{Text=e.Number+"\n"+e.Symbol,TooltipText=e.Arabic+" / "+e.Name,CustomMinimumSize=new Vector2(48,42)};
            button.AddThemeFontSizeOverride("font_size",14);
            Color group=e.Category switch{"Alkali metal"=>new Color(.5f,.20f,.15f),"Alkaline earth metal"=>new Color(.42f,.31f,.12f),"Transition metal"=>new Color(.35f,.33f,.12f),"Post-transition metal"=>new Color(.15f,.34f,.23f),"Metalloid"=>new Color(.11f,.33f,.34f),"Nonmetal"=>new Color(.12f,.31f,.43f),"Halogen"=>new Color(.31f,.20f,.43f),"Noble gas"=>new Color(.43f,.17f,.33f),"Lanthanide"=>new Color(.28f,.35f,.14f),_=>new Color(.28f,.28f,.19f)};
            button.AddThemeStyleboxOverride("normal",Card(group,group.Lightened(.15f),5,2));grid.AddChild(button);button.Pressed+=()=>Select(e);
            search.TextChanged+=text=>button.Modulate=string.IsNullOrWhiteSpace(text)||e.Symbol.Contains(text,StringComparison.OrdinalIgnoreCase)||e.Name.Contains(text,StringComparison.OrdinalIgnoreCase)||e.Arabic.Contains(text,StringComparison.OrdinalIgnoreCase)||e.Number.ToString()==text?Colors.White:new Color(0.3f,0.3f,0.3f);
        }
        var place=new Button{Text=T("إضافة عينة العنصر المحدد إلى المنضدة","Place selected virtual element sample"),CustomMinimumSize=new Vector2(0,42)};box.AddChild(place);
        place.Pressed+=()=>{if(AddTool("element",selected.Symbol))dialog.QueueFree();};Select(selected);
        dialog.CloseRequested+=()=>dialog.QueueFree();dialog.PopupCentered(new Vector2I(1100,800));
    }
    private string ToolDescription(ToolState tool)
    {
        var def=ToolCatalog.Find(tool.Kind);
        if(def?.Behavior=="reference")return T("نموذج مرجعي قابل للفحص والتحريك؛ لا تتم محاكاة عمليته حالياً.","Inspectable, movable 3D reference; its process is not simulated.");
        if(ToolCatalog.Logic(tool.Kind))return T("مدرب منطق مستقل: بدّل المدخلات وشاهد جدول الحقيقة. لا يتصل بدائرة DC.","Standalone logic trainer: toggle inputs and inspect the truth table. Not coupled to DC wiring.");
        return tool.Kind switch
        {
            "capacitor"=>T("حالة DC مستقرة: دائرة مفتوحة، I=0 و Q=C·V. لا توجد محاكاة شحن زمنية.","DC steady state: open branch, I=0 and Q=C·V. No charging transient."),
            "inductor"=>T("حالة DC مستقرة: سلك مثالي. لا يوجد سلوك حثي زمني.","DC steady state: ideal wire. No inductive transient."),
            "led" or "diode"=>T("نموذج خطي مقطعي: جهد أمامي + مقاومة تشغيل، ومقاومة إيقاف 1 GΩ؛ لا انهيار عكسي.","Piecewise model: forward drop plus on resistance; 1 GΩ off resistance, no reverse breakdown."),
            "bulb"=>T("مصباح بمقاومة ثابتة؛ السطوع يتبع القدرة المحسوبة، دون نموذج حرارة الفتيلة.","Fixed-resistance lamp; brightness follows calculated power, without filament heating dynamics."),
            "thermometer"=>T("درجة ثابتة 25 °C ضمن افتراض نموذج المحلول؛ لا يوجد نموذج تسخين.","Fixed 25 °C solution-model assumption; no heating model."),
            "phmeter"=>T("pH مثالي للماء و HCl أو NaOH فقط عند 25 °C؛ المحاليل الأخرى غير متاحة.","Ideal pH for water, HCl or NaOH only at 25 °C; other solutions are unavailable."),
            "balance"=>T("ميزان افتراضي مصفّر: كتلة عينة العنصر = كمية المادة × الكتلة الذرية المرجعية.","Virtual tared balance: element sample mass = amount × reference atomic mass."),
            "volpipette"=>T("تسليم كامل 25 mL بعد ملء الماصة؛ لا تسلّم أحجاماً جزئية.","Delivers the full 25 mL after filling; partial deliveries are rejected."),
            _=>""
        };
    }
    private void BuildExtraControls(ToolState? tool)
    {
        if(tool==null)return;string description=ToolDescription(tool);if(description.Length>0)Text(inspector,description,16);
        if(tool.Kind=="switch")Button(inspector,"toggle-switch",tool.Mode=="closed"?T("فتح المفتاح","Open switch"):T("إغلاق المفتاح","Close switch"),()=>
            Change("Toggle switch",data=>{var t=data.Tools.First(t=>t.Id==data.SelectedId);t.Mode=t.Mode=="closed"?"open":"closed";return true;},true));
        if(ToolCatalog.Logic(tool.Kind))
        {
            foreach(int input in tool.Kind=="not"?new[]{0}:new[]{0,1})
            {int bit=input;Button(inspector,"logic-"+bit,(bit==0?"A = ":"B = ")+tool.Mode[bit],()=>Change("Toggle logic input",data=>{var t=data.Tools.First(t=>t.Id==data.SelectedId);var bits=t.Mode.ToCharArray();bits[bit]=bits[bit]=='0'?'1':'0';t.Mode=new string(bits);return true;},true));}
            Text(inspector,"A  B  →  Y\n"+string.Join("\n",new[]{"00","01","10","11"}.Select(bits=>bits[0]+"   "+bits[1]+"   →   "+(ToolCatalog.Output(tool.Kind,bits[0]=='1',bits[1]=='1')?"1":"0"))),18).LayoutDirection=LayoutDirectionEnum.Ltr;
        }
        if(tool.Kind=="element"||tool.Kind=="diode"||tool.Kind=="led"||tool.Kind=="capacitor"||tool.Kind=="inductor")
        {
            Text(inspector,tool.Kind=="element"?T("كمية المادة (mol)","Amount (mol)"):tool.Kind=="capacitor"?"C (F)":tool.Kind=="inductor"?"L (H)":T("الجهد الأمامي (V)","Forward drop (V)"),17);
            componentValue=new SpinBox{MinValue=tool.Kind=="element"?0.0001:tool.Kind is "led" or "diode"?0.1:0.000001,MaxValue=tool.Kind=="element"?1:tool.Kind=="inductor"?10:tool.Kind=="capacitor"?1:5,
                Step=tool.Kind=="element"?0.001:tool.Kind=="capacitor"?0.00001:0.1,Value=tool.Value,LayoutDirection=LayoutDirectionEnum.Ltr};inspector.AddChild(componentValue);componentValue.ValueChanged+=SetComponentValue;
        }
        if(Chemical&&tool.Kind is "balance" or "phmeter" or "thermometer")
        {
            var choice=new OptionButton();inspector.AddChild(choice);var targets=Current.Tools.Where(t=>tool.Kind=="balance"?t.Kind=="element":Session.Chemical(t.Kind)).ToArray();
            for(int i=0;i<targets.Length;i++){choice.AddItem(ToolName(targets[i]));if(targets[i].Id==tool.Target)choice.Select(i);}
            choice.ItemSelected+=i=>{if(i>=0&&i<targets.Length){string id=targets[(int)i].Id;Change("Set measurement target",data=>{data.Tools.First(t=>t.Id==data.SelectedId).Target=id;return true;});}};
        }
    }
}
