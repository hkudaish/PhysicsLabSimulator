using System.Text.Json;
using VirtualStem.Science;
using VirtualStem.Persistence;
using VirtualStem.Client;

int checks = 0;
void Check(bool ok, string name) { if (!ok) throw new Exception("FAIL: " + name); checks++; }
void Near(double actual, double expected, string name) => Check(Math.Abs(actual-expected) <= 1e-10 + 1e-9*Math.Abs(expected), name);
ChemistryState Stock(double concentration=1) => new(new[] { new Vessel("stock",0.5,0.1,concentration*0.1,Dilution.IdealSolute),new Vessel("beaker",0.25,0,0) });

var c1=Stock();
var diluted=Dilution.AddWater(c1,"stock",0.1);
Near(diluted.State.Vessels["stock"].ConcentrationMolL!.Value,0.5,"SCI-C01 dilution");
Near(c1.Vessels["stock"].VolumeL,0.1,"input immutable");
var transferred=Dilution.Transfer(Stock(2),"stock","beaker",0.025);
var c2=Dilution.AddWater(transferred.State,"beaker",0.075);
Near(c2.State.Vessels["beaker"].Moles,0.05,"SCI-C02 amount");
Near(c2.State.Vessels["beaker"].ConcentrationMolL!.Value,0.5,"SCI-C02 concentration");
var c3=Dilution.Transfer(c1,"stock","beaker",0.02);
Near(c3.State.Vessels["stock"].Moles,0.08,"SCI-C03 remaining amount");
Near(c3.State.Vessels["beaker"].VolumeL,0.02,"SCI-C03 transfer volume");
foreach(double value in new[]{-1,double.NaN,double.PositiveInfinity,0.101})
{
 var rejected=Dilution.Transfer(c1,"stock","beaker",value);
 Check(!rejected.Applied && ReferenceEquals(rejected.State,c1),"SCI-C04 rejected state untouched");
}
Check(Dilution.AddWater(c1,"beaker",0.3).Status==ChemistryStatus.CapacityExceeded,"capacity check");
Check(c1.Vessels["beaker"].ConcentrationMolL==null,"SCI-C05 empty reading undefined");
var unsupported=new ChemistryState(new[]{new Vessel("x",1,0.1,0.01,"unsupported"),new Vessel("y",1,0,0)});
Check(Dilution.Transfer(unsupported,"x","y",0.01).Status==ChemistryStatus.UnsupportedModel,"SCI-C05 unsupported solute");
var repeated=Stock(2);
for(int i=0;i<50;i++) repeated=Dilution.Transfer(repeated,"stock","beaker",0.001).State;
Near(repeated.Vessels.Values.Sum(v=>v.Moles),0.2,"SCI-C06 conservation after repeated transfers");
Near(repeated.Vessels.Values.Sum(v=>v.VolumeL),0.1,"SCI-C06 volume conservation");

CircuitComponent Source(double v=9)=>new("s",CircuitKind.VoltageSource,"p","g",v);
CircuitComponent R(string id,string a,string b,double value)=>new(id,CircuitKind.Resistor,a,b,value);
CircuitResult Solve(CircuitComponent[] components,CircuitWire[]? wires=null)=>ResistiveDc.Solve(components.SelectMany(c=>new[]{c.A,c.B}).Distinct(),components,wires??Array.Empty<CircuitWire>(),"g");
var e1=Solve(new[]{Source(),R("r","p","g",1000)});
Check(e1.Status==CircuitStatus.Solved,"SCI-E01 solved");Near(e1.Branches["r"].CurrentA,0.009,"SCI-E01 current");Near(e1.Branches["r"].PowerW,0.081,"SCI-E01 power");
var e2=Solve(new[]{Source(5),R("r","p","g",220)});Near(e2.Branches["r"].CurrentA,5.0/220,"SCI-E02 current");
var e3=Solve(new[]{Source(12),R("r1","p","n",1000),R("r2","n","g",2000)});
Near(e3.Branches["r1"].CurrentA,0.004,"SCI-E03 series current");Near(e3.NodeVoltages["n"],8,"SCI-E03 node voltage");
var e4=Solve(new[]{Source(6),R("r1","p","g",1000),R("r2","p","g",1000)});
Near(e4.Branches["r1"].CurrentA,0.006,"SCI-E04 parallel branch");Near(e4.Branches["s"].CurrentA,-0.012,"SCI-E04 source current");
var open=Solve(new[]{Source(),R("r","p","end",1000)});
Check(open.Status==CircuitStatus.OpenCircuit,"SCI-E05 open");Near(open.Branches["r"].CurrentA,0,"open current zero");
Check(Solve(new[]{Source(),R("r","p","g",1000)},new[]{new CircuitWire("p","g")}).Status==CircuitStatus.ShortCircuit,"SCI-E05 short");
Check(Solve(new[]{Source(),R("r","a","b",1000)}).Status==CircuitStatus.FloatingNetwork,"SCI-E05 floating");
Near(e1.VoltageBetween("g","p")!.Value,-9,"SCI-E05 reversed probes");
Check(Solve(new[]{Source(),R("r","p","g",0)}).Status==CircuitStatus.InvalidInput,"SCI-E06 zero resistance");
Check(Solve(new[]{Source(),R("r","p","g",-1)}).Status==CircuitStatus.InvalidInput,"SCI-E06 negative resistance");
Check(Solve(new[]{Source(),R("r","p","g",1000),R("r","p","g",2000)}).Status==CircuitStatus.InvalidInput,"SCI-E06 duplicate component IDs");
Check(Solve(new[]{Source(),R("r","p","g",double.NaN)}).Status==CircuitStatus.InvalidInput,"SCI-E06 nonfinite");
Check(Solve(new[]{Source(),new CircuitComponent("s2",CircuitKind.VoltageSource,"p","g",5)}).Status==CircuitStatus.InconsistentSources,"SCI-E06 inconsistent sources");
var meter=Solve(new[]{Source(),R("r","p","n",1000),new CircuitComponent("m",CircuitKind.Ammeter,"n","g",0)});
Near(meter.Branches["m"].CurrentA,0.009,"series ammeter");
Check(Solve(new[]{Source(),R("r","p","g",1000),new CircuitComponent("m",CircuitKind.Ammeter,"p","g",0)}).Status==CircuitStatus.ShortCircuit,"ammeter across source fault");
Check(Solve(new[]{Source(12),R("r","p","g",10)}).Branches["r"].Overloaded,"power overload warning");
var wireCircuit=ResistiveDc.Solve(new[]{"p","g","a","b"},new[]{Source(),R("r","a","b",1000)},new[]{new CircuitWire("p","a"),new CircuitWire("b","g")},"g");
Near(wireCircuit.Branches["r"].CurrentA,0.009,"wires merge terminal nodes");
var random=new Random(421);
for(int i=0;i<100;i++)
{
 double v=random.NextDouble()*12,r1=10+random.NextDouble()*9990,r2=10+random.NextDouble()*9990;
 var result=Solve(new[]{Source(v),R("r1","p","n",r1),R("r2","n","g",r2)});
 Near(result.Branches["r1"].CurrentA,v/(r1+r2),"random analytical series reference");
 Check(result.Residual<=1e-9,"KCL/MNA residual");
 var parallel=Solve(new[]{Source(v),R("r1","p","g",r1),R("r2","p","g",r2)});
 Near(-parallel.Branches["s"].CurrentA,v/r1+v/r2,"random parallel source current");
 Near(parallel.Branches.Values.Sum(b=>b.PowerW),0,"conservation of electrical power");
}

var workspace=new WorkspaceData();
var snapshot=Session.Encode(workspace);
Check(Session.Encode(Session.Decode(snapshot))==snapshot,"complete workspace serialization roundtrip");
bool Invalid(string json){try{Session.Decode(json);return false;}catch(Exception error)when(error is InvalidDataException or JsonException or ArgumentException){return true;}}
Check(Invalid(snapshot.Replace("\"SchemaVersion\":1","\"SchemaVersion\":2")),"unsupported schema rejected");
Check(Invalid(snapshot.Replace(ChemistryState.ModelVersion,"unknown-model")),"unsupported chemistry model rejected");
Check(Invalid(snapshot.Replace("\"CapacityL\":0.25","\"CapacityL\":-1")),"impossible vessel state rejected");
Check(Invalid(snapshot.Replace("\"Value\":1000","\"Value\":0")),"invalid resistance in saved draft rejected");
Check(Invalid(snapshot[..^1]+",\"Unexpected\":1}"),"unknown draft fields rejected");
workspace.ChemistryUndo.Add(Session.Clone(workspace.Chemistry));
workspace.Electricity.Wires.Add(new WireState{Id="w1",A="battery.a",B="resistor.a"});
var restored=Session.Decode(Session.Encode(workspace));
Check(restored.ChemistryUndo.Count==1&&restored.Electricity.Wires.Single().B=="resistor.a","topology and undo history roundtrip");
Check(!Session.CanPlace(workspace.Chemistry,"stock",0,-0.3),"tool overlap rejected");
Check(!Session.CanPlace(workspace.Chemistry,"stock",double.NaN,0),"nonfinite placement rejected");
Check(!Session.CanPlace(workspace.Chemistry,"stock",4,0),"off-table placement rejected");

string folder=Path.Combine(Path.GetTempPath(),"virtual-stem-save-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(folder);
try
{
 var store=new LocalSessionStore(Path.Combine(folder,"محفوظات.db"));
 Check(store.Load("chemistry","v1")==null,"missing draft");
 const string json="{\"label\":\"تجربة 'محفوظة'\",\"volume\":0.1}";
 store.Save("chemistry","v1",json);
 Check(new LocalSessionStore(Path.Combine(folder,"محفوظات.db")).Load("chemistry","v1")==json,"SQLite close/reopen Unicode JSON");
 bool rejected=false;try{store.Load("chemistry","v2");}catch(InvalidDataException){rejected=true;}
 Check(rejected,"incompatible version explicit");Check(store.Load("chemistry","v1")==json,"incompatible load preserves data");
 store.Save("electricity","v1","{}");Check(store.Load("chemistry","v1")==json,"experiment slots isolated");
}
finally {Directory.Delete(folder,true);}
Console.WriteLine(JsonSerializer.Serialize(new{passed=checks,scope="scientific reference and native persistence checks; no external certification"}));
