using Godot;
using System;
using System.Linq;
using VirtualStem.Client;
using VirtualStem.Science;

public partial class LabClient
{
    private void BuildUi()
    {
        foreach(Node child in GetChildren()){RemoveChild(child);child.QueueFree();}
        actions.Clear();LayoutDirection=arabic?LayoutDirectionEnum.Rtl:LayoutDirectionEnum.Ltr;
        var background=new ColorRect{Color=new Color(0.035f,0.055f,0.085f),MouseFilter=MouseFilterEnum.Ignore};background.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);AddChild(background);
        var margin=new MarginContainer();margin.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);foreach(string side in new[]{"left","right","top","bottom"})margin.AddThemeConstantOverride("margin_"+side,18);AddChild(margin);
        var main=new VBoxContainer();main.AddThemeConstantOverride("separation",12);margin.AddChild(main);
        var header=new HBoxContainer();main.AddChild(header);
        var title=new Label{Text=T("مختبر العلوم الافتراضي","Virtual STEM Laboratory"),SizeFlagsHorizontal=SizeFlags.ExpandFill};title.AddThemeFontSizeOverride("font_size",30);header.AddChild(title);
        Button(header,"language","العربية / English",()=>{arabic=!arabic;BuildUi();Refresh(true);});
        saveButton=AsyncButton(header,"save",T("حفظ محلي","Save locally"),async()=>{await SaveWorkspace();});
        resumeButton=AsyncButton(header,"resume",T("استئناف","Resume"),async()=>{await LoadWorkspace();});
        var tabs=new HBoxContainer();main.AddChild(tabs);
        Button(tabs,"chemistry",T("تخفيف المحاليل","Solution dilution"),()=>SwitchExperiment("chemistry"));
        Button(tabs,"electricity",T("قانون أوم","Ohm’s law"),()=>SwitchExperiment("electricity"));
        Button(tabs,"undo",T("تراجع","Undo"),Undo);Button(tabs,"reset",T("إعادة التجربة","Reset experiment"),ResetExperiment);
        Button(tabs,"camera",T("إعادة الكاميرا","Reset camera"),()=>{Current.CameraYaw=0.1;Current.CameraPitch=0.85;Current.CameraRadius=8.5;bench.UpdateCamera(Current);});
        Button(tabs,"help",T("تعليمات","Help"),Help);
        var quality=new OptionButton{TooltipText=T("جودة الرسم؛ الحسابات العلمية ثابتة.","Rendering quality; scientific calculations stay the same.")};
        quality.AddItem(T("اقتصادي","Low graphics"));quality.AddItem(T("محسّن","High graphics"));quality.Select(highQuality?1:0);
        tabs.AddChild(quality);quality.ItemSelected+=index=>SetQuality(index==1);
        var layout=new HBoxContainer{SizeFlagsVertical=SizeFlags.ExpandFill};layout.AddThemeConstantOverride("separation",14);main.AddChild(layout);
        var panel=new PanelContainer{CustomMinimumSize=new Vector2(350,0)};layout.AddChild(panel);
        var scroll=new ScrollContainer{CustomMinimumSize=new Vector2(350,0)};panel.AddChild(scroll);
        inspector=new VBoxContainer{SizeFlagsHorizontal=SizeFlags.ExpandFill,CustomMinimumSize=new Vector2(325,0)};inspector.AddThemeConstantOverride("separation",10);scroll.AddChild(inspector);
        var work=new VBoxContainer{SizeFlagsHorizontal=SizeFlags.ExpandFill};layout.AddChild(work);
        var note=new Label{Text=T("اسحب الأدوات • زر الفأرة الأيمن للدوران • عجلة الفأرة للتقريب • Q/E للتدوير","Drag tools • Right-drag to orbit • Wheel to zoom • Q/E rotate"),AutowrapMode=TextServer.AutowrapMode.WordSmart};note.AddThemeFontSizeOverride("font_size",16);work.AddChild(note);
        benchContainer=new SubViewportContainer{Stretch=true,SizeFlagsHorizontal=SizeFlags.ExpandFill,SizeFlagsVertical=SizeFlags.ExpandFill,CustomMinimumSize=new Vector2(500,400),FocusMode=FocusModeEnum.All};work.AddChild(benchContainer);
        viewport=new SubViewport{Size=new Vector2I(1000,750),OwnWorld3D=true,RenderTargetUpdateMode=SubViewport.UpdateMode.Always};benchContainer.AddChild(viewport);
        bench=new LabBench(viewport,font);benchContainer.GuiInput+=BenchInput;
        status=new Label{Text=T("تجربة محلية دون اتصال — نموذج أولي","Offline local experiment — prototype"),AutowrapMode=TextServer.AutowrapMode.WordSmart};status.AddThemeFontSizeOverride("font_size",17);main.AddChild(status);
        SetQuality(highQuality);RebuildInspector();
    }
    private Button Button(Container parent,string id,string text,Action action)
    {
        var button=new Button{Text=text,TooltipText=text,CustomMinimumSize=new Vector2(0,38)};button.Pressed+=action;parent.AddChild(button);actions[id]=button;return button;
    }
    private Button AsyncButton(Container parent,string id,string text,Func<System.Threading.Tasks.Task> action)
    {var button=new Button{Text=text,TooltipText=text,CustomMinimumSize=new Vector2(0,38)};button.Pressed+=async()=>{try{await action();}catch(Exception error){Say(error.Message);}};parent.AddChild(button);actions[id]=button;return button;}
    private Label Text(Container parent,string text,int size=19)
    {var label=new Label{Text=text,AutowrapMode=TextServer.AutowrapMode.WordSmart};label.AddThemeFontSizeOverride("font_size",size);parent.AddChild(label);return label;}
    private string ToolName(ToolState tool)=>tool.Kind switch
    {"stock"=>T("محلول قياسي","Stock solution"),"beaker"=>T("كأس ","Beaker ")+Ltr("250 mL"),"cylinder"=>T("مخبار مدرج","Graduated cylinder"),"water"=>T("ماء نقي","Pure water"),"battery"=>T("مصدر DC","DC source"),"resistor"=>T("مقاومة","Resistor"),_=>Chemical?T("مجس التركيز","Concentration probe"):T("ملتيميتر","Multimeter")};
    private void RebuildInspector()
    {
        foreach(Node child in inspector.GetChildren()){inspector.RemoveChild(child);child.QueueFree();}
        Text(inspector,Chemical?T("الهدف: ","Goal: ")+Ltr("25 mL")+T(" من محلول "," of ")+Ltr("2 mol/L")+T(" ثم "," then ")+Ltr("75 mL")+T(" ماء."," water."):
            T("الهدف: دائرة مغلقة ","Goal: closed loop ")+Ltr("9 V / 1000 Ω")+T(". التيار المتوقع ",". Expected current ")+Ltr("9 mA"),18);
        feedback=Text(inspector,"",17);feedback.AddThemeColorOverride("font_color",new Color(0.35f,0.9f,0.8f));
        Text(inspector,T("خزانة الأدوات","Equipment"),22);
        var library=new HFlowContainer();inspector.AddChild(library);
        foreach(string kind in Chemical?new[]{"beaker","cylinder","stock","water","meter"}:new[]{"battery","resistor","meter"})
        {string chosen=kind;var dummy=new ToolState{Kind=kind};Button(library,"add-"+kind,ToolName(dummy),()=>AddTool(chosen));}
        var select=new OptionButton{CustomMinimumSize=new Vector2(0,38)};inspector.AddChild(select);
        for(int i=0;i<Current.Tools.Count;i++){select.AddItem(ToolName(Current.Tools[i])+" "+(i+1));if(Current.Tools[i].Id==Current.SelectedId)select.Select(i);}
        select.ItemSelected+=index=>{if(index>=0&&index<Current.Tools.Count)SelectTool(Current.Tools[(int)index].Id);};
        selectionLabel=Text(inspector,Selected==null?T("اختر أداة من المنضدة.","Select a tool on the bench."):ToolName(Selected),22);
        readings=Text(inspector,"",20);
        var tool=Selected;
        if(Chemical)
        {
            Text(inspector,T("الكمية المنقولة (mL)","Transfer quantity (mL)"),17);
            amount=new SpinBox{MinValue=1,MaxValue=250,Step=1,Value=25,CustomMinimumSize=new Vector2(0,38),LayoutDirection=LayoutDirectionEnum.Ltr};inspector.AddChild(amount);
            targetChoice=new OptionButton{CustomMinimumSize=new Vector2(0,38)};targetIds.Clear();inspector.AddChild(targetChoice);
            foreach(var target in Current.Tools.Where(t=>Session.Chemical(t.Kind)&&t.Id!=Current.SelectedId))
            {targetIds.Add(target.Id);targetChoice.AddItem(ToolName(target)+" "+(Current.Tools.IndexOf(target)+1));}
            Button(inspector,"pour",T("نقل المحلول إلى الوعاء المحدد","Transfer solution to chosen vessel"),()=>Pour(false));
            Button(inspector,"water",T("إضافة ماء إلى الوعاء الحالي","Add water to selected vessel"),()=>Pour(true));
            if(tool?.Kind=="meter")Button(inspector,"probe",T("قياس الوعاء المحدد في القائمة","Measure vessel chosen above"),()=>
            {
                if(targetChoice.Selected<0||targetChoice.Selected>=targetIds.Count)return;
                string target=targetIds[targetChoice.Selected];Change("Set concentration probe",data=>{data.Tools.First(t=>t.Id==data.SelectedId).Target=target;return true;});
            });
            Text(inspector,T("لون السائل توضيحي للتركيز. الحجم وكمية المذاب هما أساس الحساب.","Liquid color illustrates concentration. Calculations use volume and solute amount."),15);
        }
        else
        {
            if(tool?.Kind=="battery"||tool?.Kind=="resistor")
            {
                Text(inspector,tool.Kind=="battery"?T("الجهد V","Voltage V"):T("المقاومة Ω","Resistance Ω"),18);
                componentValue=new SpinBox{MinValue=tool.Kind=="battery"?0:10,MaxValue=tool.Kind=="battery"?12:10000,
                    Step=tool.Kind=="battery"?0.5:10,Value=tool.Value,CustomMinimumSize=new Vector2(0,38),LayoutDirection=LayoutDirectionEnum.Ltr};inspector.AddChild(componentValue);
                componentValue.ValueChanged+=SetComponentValue;
            }
            if(tool?.Kind=="meter")
            {
                var mode=new OptionButton();mode.AddItem(T("جهد — توصيل على التوازي","Voltage — parallel connection"));mode.AddItem(T("تيار — توصيل على التوالي","Current — series connection"));mode.Select(tool.Mode=="current"?1:0);inspector.AddChild(mode);
                mode.ItemSelected+=index=>Change("Meter mode",data=>{data.Tools.First(t=>t.Id==data.SelectedId).Mode=index==0?"voltage":"current";return true;});
            }
            Text(inspector,T("انقر طرفاً أحمر أو أسود ثم طرفاً آخر لإنشاء سلك.","Click a red/black terminal, then another terminal to create a wire."),17);
            wireChoice=new OptionButton{CustomMinimumSize=new Vector2(0,38)};wireIds.Clear();inspector.AddChild(wireChoice);
            for(int i=0;i<Current.Wires.Count;i++){wireIds.Add(Current.Wires[i].Id);wireChoice.AddItem(T("سلك ","Wire ")+(i+1));}
            Button(inspector,"disconnect",T("فصل السلك المحدد","Disconnect selected wire"),Disconnect);
        }
        var manipulation=new HBoxContainer();inspector.AddChild(manipulation);
        Button(manipulation,"rotate-left","↶",()=>RotateSelected(-Mathf.Pi/8));Button(manipulation,"rotate-right","↷",()=>RotateSelected(Mathf.Pi/8));
        Button(manipulation,"remove",T("إزالة الأداة","Remove tool"),RemoveSelected);
    }
    private void Refresh(bool rebuild=false)
    {
        electrical=Chemical?null:Session.Electricity(Current);bench.Render(Current,electrical);
        if(rebuild)RebuildInspector();
        var tool=Selected;
        if(Chemical)
        {
            if(tool!=null&&Session.Chemical(tool.Kind))
            {
                var vessel=tool.Vessel();readings.Text=T("الحجم: ","Volume: ")+Ltr(N(vessel.VolumeL*1000,"0.#")+" mL")+"\n"+
                    T("المذاب: ","Solute: ")+Ltr(N(vessel.Moles,"0.######")+" mol")+"\n"+
                    T("التركيز: ","Concentration: ")+Ltr(vessel.ConcentrationMolL is double c?N(c,"0.000")+" mol/L":"—");
            }
            else if(tool?.Kind=="meter")
            {
                var target=Current.Tools.FirstOrDefault(t=>t.Id==tool.Target&&Session.Chemical(t.Kind));
                readings.Text=target!=null&&LabBench.Distance(tool,target)<=1.65&&target.Vessel().ConcentrationMolL is double c?
                    Ltr(N(c,"0.000")+" mol/L"):T("قرّب المجس من وعاء ممتلئ واختره للقياس.","Move the probe near a filled target and select it for measurement.");
            }
            else readings.Text="—";
            var goal=Current.Tools.FirstOrDefault(t=>t.Kind=="beaker"&&Math.Abs(t.VolumeL-0.1)<1e-9&&Math.Abs(t.Moles-0.05)<1e-9);
            feedback.Text=goal!=null?T("اكتملت التجربة: ","Goal achieved: ")+Ltr("0.050 mol / 0.100 L = 0.500 mol/L"):
                T("حافظ على كمية المذاب أثناء التخفيف. ","Dilution preserves solute amount. ")+Ltr("C = n / V");
        }
        else
        {
            readings.Text="—";
            if(electrical?.HasReadings==true&&tool!=null)
            {
                if(tool.Kind=="meter")
                {
                    if(tool.Mode=="voltage")readings.Text=electrical.VoltageBetween(tool.Id+".a",tool.Id+".b") is double v?Ltr(N(v,"0.000")+" V"):"—";
                    else if(electrical.Branches.TryGetValue(tool.Id,out var m))readings.Text=Ltr(N(m.CurrentA,"0.000000")+" A");
                }
                else if(electrical.Branches.TryGetValue(tool.Id,out var branch))readings.Text=Ltr(N(branch.VoltageV,"0.000")+" V")+"\n"+
                    T("تيار محسوب: ","Calculated current: ")+Ltr(N(Math.Abs(branch.CurrentA),"0.000000")+" A")+"\n"+
                    Ltr(N(Math.Abs(branch.PowerW),"0.000")+" W")+(branch.Overloaded?"\n"+T("تحذير: تجاوز حد قدرة المقاومة.","Warning: resistor power rating exceeded."):"");
            }
            feedback.Text=electrical==null?"":CircuitFeedback(electrical.Status);
            if(electrical?.HasReadings==true&&Current.Tools.Any(t=>t.Kind=="resistor"&&electrical.Branches.TryGetValue(t.Id,out var b)&&Math.Abs(b.CurrentA-0.009)<1e-9&&Math.Abs(b.VoltageV-9)<1e-9&&t.Value==1000))
                feedback.Text=T("قانون أوم تحقق: ","Ohm's law verified: ")+Ltr("I = 9 V / 1000 Ω = 0.009 A");
        }
    }
    private string CircuitFeedback(CircuitStatus status)=>status switch
    {
        CircuitStatus.Solved=>T("الدائرة محسوبة. غيّر الجهد أو المقاومة وراقب القياس.","Circuit solved. Change voltage or resistance and observe readings."),
        CircuitStatus.OpenCircuit=>T("دائرة مفتوحة: لا يمر تيار في المسار.","Open circuit: no current through the path."),
        CircuitStatus.FloatingNetwork=>T("أطراف عائمة: أكمل التوصيلات قبل قراءة القياس.","Floating terminals: complete the connections before measuring."),
        CircuitStatus.ShortCircuit=>T("قصر كهربائي: افصل المسار المباشر. التيار غير معرّف في المصدر المثالي.","Short circuit: remove the bypass. Ideal-source current is undefined."),
        CircuitStatus.InconsistentSources=>T("مصادر جهد متعارضة: راجع التوصيلات.","Conflicting voltage sources: check wiring."),
        CircuitStatus.EmptyCircuit=>T("ضع مصدر جهد ومقاومة ثم صلهما.","Place a source and resistor, then connect them."),
        _=>T("توصيل غير صالح أو غير قابل للحل. افحص الدائرة.","Invalid or singular circuit. Inspect the connections.")
    };
    private void Help()
    {
        var dialog=new AcceptDialog{Title=T("دليل المختبر","Laboratory guide"),DialogText=T(
            "التخفيف: اختر المحلول القياسي وانقل 25 mL إلى الكأس، ثم أضف 75 mL ماء.\nالمجس يقيس وعاءً قريباً منه. اسحب الأدوات لتغيير موضعها.\nالكهرباء: صل بطارية ومقاومة بسلكين. الملتيميتر على التوازي للجهد وعلى التوالي للتيار.\nزر الفأرة الأيمن وعجلة الفأرة للكاميرا. Q/E لتدوير الأداة وCtrl+Z للتراجع.\nالحفظ محلي على هذا الجهاز. نموذج التخفيف مثالي، ولا يحاكي التفاعلات أو الحرارة. الدائرة DC مثالية؛ التيار في القصر غير معرّف.\nهذا نموذج أولي؛ تقييم الأجهزة المدرسية والمراجعة العلمية المستقلة لم يكتمل بعد.",
            "Dilution: transfer 25 mL of stock into the beaker, then add 75 mL water.\nMove the probe near its target. Drag tools to reposition them.\nElectricity: close the battery/resistor loop with two wires. Connect the multimeter in parallel for voltage or in series for current.\nRight-drag/wheel control the camera. Q/E rotate, Ctrl+Z undoes.\nSaves are local to this device. Dilution uses an ideal nonreactive solution; DC components are ideal. Short-circuit current is undefined.\nPrototype: school-hardware qualification and independent scientific review remain pending.")};AddChild(dialog);dialog.PopupCentered(new Vector2I(820,550));
    }
}
