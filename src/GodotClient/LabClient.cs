using Godot;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using VirtualStem.Client;
using VirtualStem.Persistence;
using VirtualStem.Science;

public partial class LabClient : Control
{
    private WorkspaceData workspace=new();
    private SessionData Current { get=>workspace.ActiveExperiment=="chemistry"?workspace.Chemistry:workspace.Electricity;
        set {if(workspace.ActiveExperiment=="chemistry")workspace.Chemistry=value;else workspace.Electricity=value;} }
    private List<SessionData> UndoStates=>workspace.ActiveExperiment=="chemistry"?workspace.ChemistryUndo:workspace.ElectricityUndo;
    private bool Chemical=>workspace.ActiveExperiment=="chemistry";
    private bool arabic=true,busy,highQuality,closing;
    private FontFile font=null!;
    private LocalSessionStore store=null!;
    private LabBench bench=null!;
    private SubViewport viewport=null!;
    private SubViewportContainer benchContainer=null!;
    private Label status=null!,readings=null!,feedback=null!,selectionLabel=null!;
    private VBoxContainer inspector=null!;
    private OptionButton targetChoice=null!,wireChoice=null!,qualityChoice=null!;
    private readonly List<string> targetIds=new(),wireIds=new();
    private SpinBox amount=null!,componentValue=null!;
    private Button saveButton=null!,resumeButton=null!;
    private CircuitResult? electrical;
    private string? wireStart,dragId;
    private Vector3 dragPosition;
    private bool orbiting,dragValid;
    private readonly Dictionary<string,Button> actions=new();

    public override async void _Ready()
    {
        try
        {
            GetTree().AutoAcceptQuit=false;
            GetViewport().GuiEmbedSubwindows=true;
            Elements.Load(Godot.FileAccess.GetFileAsString("res://assets/catalog/elements.json"));
            highQuality=!OS.GetCmdlineUserArgs().Contains("--low-graphics")&&RenderingServer.GetCurrentRenderingMethod()=="forward_plus";
            font=ResourceLoader.Load<FontFile>("res://assets/Tajawal-Regular.ttf");
            var fallback=ResourceLoader.Load<FontFile>("res://assets/DejaVuSans.ttf");
            font.AllowSystemFallback=false;fallback.AllowSystemFallback=false;font.Fallbacks=new Godot.Collections.Array<Font>{fallback};
            ApplyLabTheme();
            string? testPath=System.Environment.GetEnvironmentVariable("STEM_LAB_TEST_STORAGE");
            if((OS.GetCmdlineUserArgs().Contains("--verify")||OS.GetCmdlineUserArgs().Contains("--resume-check")||OS.GetCmdlineUserArgs().Contains("--close-check"))&&string.IsNullOrWhiteSpace(testPath))
                throw new InvalidOperationException("Verification requires isolated storage.");
            string folder=string.IsNullOrWhiteSpace(testPath)?ProjectSettings.GlobalizePath("user://drafts"):testPath;
            Directory.CreateDirectory(folder);store=new LocalSessionStore(Path.Combine(folder,"laboratory.db"));
            BuildUi();Refresh();
            var args=OS.GetCmdlineUserArgs();
            if(args.Contains("--verify")||args.Contains("--resume-check"))
            {
                GetViewport().GuiDisableInput=true;SetProcessInput(false);SetProcessUnhandledKeyInput(false);
                await VerifyPrototype(args.Contains("--resume-check"));
            }
            else if(args.Contains("--close-check"))
            {
                Require(OS.HasFeature("template")&&!OS.HasFeature("editor"),"window-close check uses standalone export");
                Require(await LoadWorkspace(),"window-close check loads saved workspace");
                Current.Actions.Add("Window-close autosave check");Current.Revision++;
                File.WriteAllText(Path.Combine(ResultsFolder(),"saved-workspace.sha256"),Hash(Session.Encode(workspace)));
                File.WriteAllText(Path.Combine(ResultsFolder(),"close-ready.txt"),"unsaved action awaits actual window close");
            }
            else await LoadWorkspace(true);
        }
        catch(Exception error){GD.PushError(error.ToString());if(OS.GetCmdlineUserArgs().Contains("--verify")||OS.GetCmdlineUserArgs().Contains("--resume-check")||OS.GetCmdlineUserArgs().Contains("--close-check")){WriteFailure(error);GetTree().Quit(1);}else Say(T("تعذر بدء المختبر: ","Unable to start: ")+error.Message);}
    }
    private string T(string ar,string en)=>arabic?ar:en;
    private static string N(double value,string format="0.###")=>value.ToString(format,CultureInfo.InvariantCulture);
    private static string Ltr(string text)=>"\u2066"+text+"\u2069";
    private ToolState? Selected=>Current.Tools.FirstOrDefault(t=>t.Id==Current.SelectedId);
    private void Say(string text){if(status!=null)status.Text=text;}
    private void SwitchExperiment(string experiment)
    {
        if(busy)return;
        workspace.ActiveExperiment=experiment;wireStart=null;dragId=null;orbiting=false;
        RebuildInspector();Refresh();Say(Chemical?T("اختر المحلول ثم انقله إلى الكأس.","Select stock solution and transfer it into the beaker."):
            T("انقر طرفين لإنشاء سلك. ضع مقياس التيار على التوالي.","Click two terminals to add a wire. Connect an ammeter in series."));
    }
    private bool Change(string action,Func<SessionData,bool> apply,bool rebuild=false)
    {
        if(busy)return false;
        var before=Session.Clone(Current);var candidate=Session.Clone(Current);
        if(!apply(candidate))return false;
        candidate.Revision=Current.Revision+1;candidate.Actions.Add(action);if(candidate.Actions.Count>100)candidate.Actions.RemoveAt(0);
        UndoStates.Add(before);if(UndoStates.Count>20)UndoStates.RemoveAt(0);Current=candidate;Session.TrackModels(workspace);Refresh(rebuild);return true;
    }
    private void SetQuality(bool high)
    {
        highQuality=high&&bench.SupportsAdvancedRendering;bench.ApplyQuality(highQuality);
        Engine.MaxFps=highQuality?120:60;
        if(qualityChoice!=null)qualityChoice.Select(highQuality?1:0);
    }
    private void SelectTool(string id)
    {
        if(busy)return;Current.SelectedId=id;Refresh(true);
    }
    private void Undo()
    {
        if(busy||UndoStates.Count==0){Say(T("لا توجد خطوة للتراجع عنها.","Nothing to undo."));return;}
        long revision=Current.Revision;Current=Session.Clone(UndoStates[^1]);UndoStates.RemoveAt(UndoStates.Count-1);Current.Revision=revision+1;
        wireStart=null;Refresh(true);Say(T("تم التراجع عن آخر إجراء.","Last action undone."));
    }
    private void ResetExperiment()
    {
        if(busy)return;var previous=Session.Clone(Current);long revision=Current.Revision;
        UndoStates.Add(previous);if(UndoStates.Count>20)UndoStates.RemoveAt(0);
        Current=Session.New(workspace.ActiveExperiment);Current.Revision=revision+1;wireStart=null;Refresh(true);Say(T("بدأت تجربة جديدة. يمكنك التراجع.","Experiment reset. Undo can restore the previous state."));
    }
    private bool AddTool(string kind,string? content=null)
    {
        return Change("Add "+kind,data=>
        {
            if(data.Tools.Count>=12){Say(T("الحد الأقصى 12 أداة على المنضدة.","Maximum 12 tools on the bench."));return false;}
            (double x,double z)? placement=null;double best=-1;
            for(double z=-1.5;z<=1.5;z+=0.25)for(double x=-3;x<=3;x+=0.25)
                if(Session.CanPlace(data,"new",x,z))
                {
                    double distance=data.Tools.Count==0?0:data.Tools.Min(t=>(t.X-x)*(t.X-x)+(t.Z-z)*(t.Z-z));
                    if(distance>best){best=distance;placement=(x,z);}
                }
            if(placement==null){Say(T("لا توجد مساحة؛ أزل أداة أو أعد ترتيب المنضدة.","No free space; remove or rearrange equipment."));return false;}
            var tool=ToolCatalog.Create(kind,workspace.ActiveExperiment);tool.X=placement.Value.x;tool.Z=placement.Value.z;
            if(kind=="element"&&content!=null)tool.Mode=content;
            if(kind=="stock"&&content!=null){tool.SoluteId=content;tool.VolumeL=0.1;tool.Moles=0.01;}
            if(kind is "meter" or "phmeter" or "thermometer")tool.Target=data.Tools.FirstOrDefault(t=>t.Kind=="beaker")?.Id;
            if(kind=="balance")tool.Target=data.Tools.FirstOrDefault(t=>t.Kind=="element")?.Id;
            data.Tools.Add(tool);data.SelectedId=tool.Id;return true;
        },true);
    }
    private void RemoveSelected()
    {
        string id=Current.SelectedId;
        Change("Remove "+id,data=>
        {
            if(!data.Tools.Any(t=>t.Id==id))return false;
            data.Tools.RemoveAll(t=>t.Id==id);data.Wires.RemoveAll(w=>w.A.StartsWith(id+".",StringComparison.Ordinal)||w.B.StartsWith(id+".",StringComparison.Ordinal));
            foreach(var t in data.Tools.Where(t=>t.Target==id))t.Target=null;
            data.SelectedId=data.Tools.FirstOrDefault()?.Id??"";return true;
        },true);wireStart=null;
    }
    private void Pour(bool water)
    {
        string source=water?Current.Tools.FirstOrDefault(t=>t.Kind=="water"&&t.Moles==0)?.Id??"":Current.SelectedId;
        string target=water?Current.SelectedId:targetChoice.Selected>=0&&targetChoice.Selected<targetIds.Count?targetIds[targetChoice.Selected]:"";
        double volume=amount.Value/1000;
        bool pureWater=Current.Tools.FirstOrDefault(t=>t.Id==source)?.Moles==0;
        bool applied=Change("Transfer "+N(volume)+" L "+source+" → "+target,data=>
        {
            if(!data.Tools.Any(t=>t.Id==source&&Session.Chemical(t.Kind))||!data.Tools.Any(t=>t.Id==target&&Session.Chemical(t.Kind)))
            {Say(T("اختر وعاءً ومصدراً صالحاً أولاً.","Choose a valid vessel and source first."));return false;}
            if(data.Tools.First(t=>t.Id==source).Kind=="volpipette"&&Math.Abs(volume-0.025)>1e-10)
            {Say(T("الماصة الحجمية تسلّم 25 mL كاملة؛ املأها أولاً.","Volumetric pipette delivers its full 25 mL; fill it first."));return false;}
            var result=Dilution.Transfer(Session.Chemistry(data),source,target,volume);
            if(!result.Applied){Say(ChemistryError(result.Status));return false;}
            Session.ApplyChemistry(data,result.State);data.SelectedId=target;return true;
        },true);
        if(applied){bench.AnimatePour(source,target,pureWater);Say(T("تم النقل: ","Transferred: ")+Ltr(N(volume*1000)+" mL"));}
    }
    private string ChemistryError(ChemistryStatus status)=>status switch
    {
        ChemistryStatus.CapacityExceeded=>T("الكمية تتجاوز سعة الوعاء. لم يتغير المحلول.","Vessel capacity exceeded; solution unchanged."),
        ChemistryStatus.InsufficientVolume=>T("الكمية المطلوبة أكبر من الكمية المتاحة.","Requested volume exceeds available solution."),
        ChemistryStatus.UnsupportedModel=>T("خلط مذابين مختلفين أو التفاعلات خارج نموذج التخفيف.","Different solutes/reactions are outside the dilution model."),
        _=>T("تعذر تنفيذ النقل. راجع المصدر والوجهة والكمية.","Transfer rejected. Check source, target and quantity.")
    };
    private void SetComponentValue(double value)
    {
        string id=Current.SelectedId;
        Change("Set value "+N(value),data=>
        {
            var tool=data.Tools.FirstOrDefault(t=>t.Id==id);if(tool==null)return false;
            if(!Numbers.Finite(value)||(tool.Kind=="battery"&&(value<0||value>12))||
                ((tool.Kind is "resistor" or "rheostat" or "bulb")&&(value<10||value>10000))||
                ((tool.Kind is "led" or "diode")&&(value<0.1||value>5))||
                (tool.Kind=="element"&&(value<0.0001||value>1))||
                ((tool.Kind is "capacitor" or "inductor")&&value<=0))return false;
            if(tool.Value==value)return false;tool.Value=value;return true;
        });
    }
    private void TerminalClicked(string terminal)
    {
        if(busy||Chemical)return;
        if(wireStart==null){wireStart=terminal;Say(T("اختر الطرف الثاني للسلك. Esc للإلغاء.","Select the other terminal. Esc cancels."));return;}
        string a=wireStart;wireStart=null;
        Change("Connect "+a+" → "+terminal,data=>
        {
            if(a==terminal||!Session.TerminalExists(data,a)||!Session.TerminalExists(data,terminal)||
                data.Wires.Any(w=>(w.A==a&&w.B==terminal)||(w.A==terminal&&w.B==a)))
            {Say(T("اختر طرفين مختلفين غير متصلين بالفعل.","Choose distinct terminals that are not already connected."));return false;}
            if(data.Wires.Count>=48)return false;
            data.Wires.Add(new WireState{Id=Guid.NewGuid().ToString("N"),A=a,B=terminal});return true;
        },true);
    }
    private void Disconnect()
    {
        if(wireChoice.Selected<0||wireChoice.Selected>=wireIds.Count)return;
        string id=wireIds[wireChoice.Selected];Change("Disconnect wire",data=>data.Wires.RemoveAll(w=>w.Id==id)>0,true);
    }
    private bool MoveTool(string id,double x,double z)
    {
        bool changed=Change("Move tool",data=>
        {
            var tool=data.Tools.FirstOrDefault(t=>t.Id==id);if(tool==null||!Session.CanPlace(data,id,x,z))return false;
            if(Math.Abs(tool.X-x)<0.001&&Math.Abs(tool.Z-z)<0.001)return false;tool.X=x;tool.Z=z;return true;
        });
        if(!changed)Refresh();return changed;
    }
    private void RotateSelected(float radians)=>Change("Rotate tool",data=>
    {var tool=data.Tools.FirstOrDefault(t=>t.Id==data.SelectedId);if(tool==null)return false;tool.Yaw+=radians;return true;});
    private void BenchInput(InputEvent input)
    {
        if(busy)return;
        if(input is InputEventMouseButton button)
        {
            if(button.ButtonIndex==MouseButton.WheelUp&&button.Pressed){Current.CameraRadius=Math.Max(5,Current.CameraRadius-0.5);bench.UpdateCamera(Current);}
            if(button.ButtonIndex==MouseButton.WheelDown&&button.Pressed){Current.CameraRadius=Math.Min(14,Current.CameraRadius+0.5);bench.UpdateCamera(Current);}
            if(button.ButtonIndex==MouseButton.Right&&button.Pressed)orbiting=true;
            if(button.ButtonIndex==MouseButton.Left&&button.Pressed)
            {
                benchContainer.GrabFocus();var hit=bench.Pick(button.Position);
                if(hit.terminal!=null){TerminalClicked(hit.terminal);return;}
                if(hit.tool!=null){SelectTool(hit.tool);dragId=hit.tool;var tool=Selected!;dragPosition=new Vector3((float)tool.X,0,(float)tool.Z);dragValid=true;}
            }
        }
    }
    public override void _Process(double delta)
    {
        if(bench==null)return;
        bench.Advance(delta);
        if(dragId!=null)bench.PreviewPosition(dragId,dragPosition,delta);
    }
    public override void _Input(InputEvent input)
    {
        if(busy)return;
        if(input is InputEventMouseMotion motion)
        {
            if(orbiting)
            {Current.CameraYaw-=motion.Relative.X*0.006;Current.CameraPitch=Math.Clamp(Current.CameraPitch+motion.Relative.Y*0.005,0.3,1.4);bench.UpdateCamera(Current);}
            if(dragId!=null&&bench.OnTable(benchContainer.GetLocalMousePosition()) is Vector3 point)
            {
                dragPosition=new Vector3(Mathf.Snapped(point.X,0.1f),0,Mathf.Snapped(point.Z,0.1f));
                dragValid=Session.CanPlace(Current,dragId,dragPosition.X,dragPosition.Z);
                if(!dragValid)Say(T("موضع غير صالح: ابق داخل المنضدة وبعيداً عن الأدوات الأخرى.","Invalid placement: stay on the bench and avoid other tools."));
            }
        }
        if(input is InputEventMouseButton button&&!button.Pressed)
        {
            if(button.ButtonIndex==MouseButton.Right)orbiting=false;
            if(button.ButtonIndex==MouseButton.Left&&dragId!=null)
            {string id=dragId;dragId=null;if(dragValid)MoveTool(id,dragPosition.X,dragPosition.Z);else Refresh();}
        }
    }
    public override void _UnhandledKeyInput(InputEvent input)
    {
        if(busy||input is not InputEventKey key||!key.Pressed||key.Echo)return;
        if(key.Keycode==Key.Escape){dragId=null;wireStart=null;orbiting=false;Refresh();}
        if(key.Keycode==Key.Q)RotateSelected(-Mathf.Pi/8);
        if(key.Keycode==Key.E)RotateSelected(Mathf.Pi/8);
        if(key.Keycode==Key.Z&&key.CtrlPressed)Undo();
    }
    private async Task<bool> SaveWorkspace()
    {
        if(busy)return false;busy=true;saveButton.Disabled=resumeButton.Disabled=true;
        try
        {
            string json=Session.Encode(workspace);
            await Task.Run(()=>store.Save("workspace",Session.WorkspaceVersion,json));
            Say(T("تم حفظ التجربتين محلياً. يمكن إغلاق التطبيق واستئناف العمل.","Both experiments saved locally. You can close and resume."));return true;
        }
        catch(Exception error){Say(T("فشل الحفظ؛ لم نحذف العمل: ","Save failed; work was not discarded: ")+error.Message);return false;}
        finally{busy=false;saveButton.Disabled=resumeButton.Disabled=false;}
    }
    private async Task<bool> LoadWorkspace(bool startup=false)
    {
        if(busy)return false;busy=true;saveButton.Disabled=resumeButton.Disabled=true;
        try
        {
            string? json=await Task.Run(()=>store.Load("workspace",Session.WorkspaceVersion));
            if(json==null){if(!startup)Say(T("لا يوجد حفظ سابق على هذا الجهاز.","No saved workspace on this device."));return false;}
            var restored=Session.Decode(json);workspace=restored;wireStart=null;dragId=null;
            Refresh(true);Say(T("استعيدت الأدوات والقياسات والتوصيلات وسجل التراجع.","Tools, measurements, wires and undo history restored."));return true;
        }
        catch(Exception error){Say(T("تعذر استعادة الحفظ؛ العمل الحالي محفوظ في الذاكرة: ","Cannot load; current workspace unchanged: ")+error.Message);return false;}
        finally{busy=false;saveButton.Disabled=resumeButton.Disabled=false;}
    }
    public override async void _Notification(int what)
    {
        if(what==NotificationWMCloseRequest&&!closing&&store!=null)
        {
            closing=true;
            while(busy)await ToSignal(GetTree(),SceneTree.SignalName.ProcessFrame);
            if(await SaveWorkspace())await FinishAndQuit(0);else closing=false;
        }
    }
    private async Task FinishAndQuit(int code)
    {
        var tree=GetTree();busy=true;
        bench?.Release();
        foreach(Node child in GetChildren()){RemoveChild(child);child.QueueFree();}
        bench=null!;viewport=null!;font=null!;Theme=null;
        await ToSignal(tree,SceneTree.SignalName.ProcessFrame);
        await ToSignal(tree,SceneTree.SignalName.ProcessFrame);
        BenchGeometry.ReleaseCache();
        // Release managed resource wrappers while the rendering device still exists.
        // This is a shutdown boundary, never a per-frame garbage collection policy.
        GC.Collect();GC.WaitForPendingFinalizers();GC.Collect();
        await ToSignal(tree,SceneTree.SignalName.PhysicsFrame);
        await ToSignal(tree,SceneTree.SignalName.PhysicsFrame);
        await ToSignal(tree,SceneTree.SignalName.ProcessFrame);
        RenderingServer.ForceSync();
        tree.Quit(code);
    }
}
