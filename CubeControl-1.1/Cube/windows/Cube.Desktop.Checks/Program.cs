using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using System.Windows;
using System.Windows.Threading;
using Cube.Desktop.ViewModels;
using Cube.Protocol;
class Check {
 static readonly BindingFlags Flags=BindingFlags.Instance|BindingFlags.NonPublic;
 static void Assert(bool ok,string text){if(!ok)throw new Exception(text);Console.WriteLine("PASS: "+text);}
 static object Call(object o,string name,params object[] args)=>o.GetType().GetMethod(name,Flags)!.Invoke(o,args)!;
 static async Task<CabinetCapture> Match(IrLibraryEntry[] entries,byte[] sector,int? volume=80)=>await Task.Run(()=> (CabinetCapture)typeof(EditorViewModel).GetMethod("MatchCapture",BindingFlags.NonPublic|BindingFlags.Static)!.Invoke(null,new object[]{entries,sector,volume})!);
 [STAThread] static void Main(string[] args) { if (args.Contains("--theme")) ThemeMaterialChecks.Run(); else Run(); }
 static void Run(){var app=new Cube.Desktop.App();app.InitializeComponent();SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext());
 var task=Tests();var frame=new DispatcherFrame();task.ContinueWith(_=>app.Dispatcher.BeginInvoke(new Action(()=>frame.Continue=false)));Dispatcher.PushFrame(frame);task.GetAwaiter().GetResult();app.Shutdown();}
 static async Task Tests(){
 string data=Path.Combine(Path.GetTempPath(),"CubeControl-CabinetCapture-Checks",Guid.NewGuid().ToString("N"));
 var vm=new EditorViewModel(data);
 var entries=vm.IrLibrary.Entries.Where(e=>e.Metadata!=null).ToArray();Assert(entries.Length>0,"catalog loaded");
 var sample=entries.First(e=>e.Metadata.Brand=="Zilla" && e.Metadata.SpeakerLabel.Contains("WGS"));
 foreach(var entry in entries.DistinctBy(e=>e.Metadata.Brand).Take(5).Append(sample)){
 var audio=IrAudio.Decode(File.ReadAllBytes(entry.FullPath));var cap=await Match(new[]{entry},audio.Sector(80));
 Assert(cap!=null && cap.Title.Contains(entry.Metadata.Brand),"metadata match: "+entry.Metadata.Brand);
 var gear=new GearItem("test",1,"file","custom","cab","#C5B77F",capture:cap);Assert(gear.DefaultImage!=null && gear.HasCapture,"image and capture: "+entry.Metadata.Brand);
 }
 var bytes=IrAudio.Decode(File.ReadAllBytes(sample.FullPath));var sector=bytes.Sector(80);var capture=await Match(new[]{sample},sector);
 var conflict=sample with{Metadata=sample.Metadata with{CabinetModel="Conflicting model"}};
 Assert(await Match(new[]{sample,conflict},sector)==null,"conflicting candidates stay unidentified");
 Assert(await Match(new[]{sample with{Metadata=sample.Metadata with{Sha256=new string('0',64)}}},sector)==null,"changed source hash rejected");
 var sparse=new CabinetCapture(new IrLibraryMetadata(),"external.wav");Assert(sparse.Title=="" && sparse.Summary=="","missing metadata stays hidden");
 var probable=capture with{Metadata=capture.Metadata with{Identification="probable"}};Assert(probable.Title!=capture.Title,"probable label retained");
 var fake=new FakeConnection();var session=new CubeSession(fake);await session.Identify();typeof(EditorViewModel).GetField("session",Flags)!.SetValue(vm,session);
 await (Task)Call(vm,"WriteIrAsync",0,bytes,80,sample.Name,null,false,sample.FullPath);
 string hash=CabinetInventory.Hash(sector);
 Assert(vm.Cabinets[1].Name==capture.Title && vm.Cabinets[1].CaptureSummary==capture.Summary,"verified write updates cabinet");
 Assert(File.Exists(Path.Combine(data,"cabinet-captures-v1.json")),"capture persisted after verified write");
 // Exercise the same source snapshot through random and pack write entrypoints.
 fake.Bank[9]=1; await (Task)Call(vm,"LoadBank",false);
 typeof(IrLibraryViewModel).GetProperty("Entries")!.SetValue(vm.IrLibrary,new[]{sample});
 await (Task)Call(vm,"RandomizeCabinetIrAsync");
 Assert(vm.Cabinets[1].Name==capture.Title,"random write retains source metadata");
 var sectors=Enumerable.Range(0,8).Select(_=>Array.Empty<byte>()).ToArray();sectors[1]=sector;
 var pack=new IrPack("capture-check","Capture check",sectors){Definition=new IrPackDefinition{Entries=[new IrPackEntry(1,sample.RelativePath,sample.Metadata.Sha256,sample.Name,80)]}};
 vm.ChoosePackIrSlots=_=>Task.FromResult(new[]{1});vm.Ask=_=>Task.FromResult(true);
 await (Task)Call(vm,"ApplyPreparedIrPackAsync",pack,false);
 Assert(vm.Cabinets[2].Name==capture.Title,"pack write retains source metadata");
 vm.Ir.Load(sample.FullPath,bytes,sample.Collection);vm.Ir.Slot=2;
 vm.ConfirmIrWrite=_=>Task.FromResult((false,(string)null));int writes=fake.FlashWrites;
 await (Task)Call(vm,"ConfirmWriteIrAsync");Assert(fake.FlashWrites==writes,"cancelled confirmation performs no writes");
 var reloaded=new EditorViewModel(data);Call(reloaded,"UpdateCabinet",0,sector);Assert(reloaded.Cabinets[1].Name==capture.Title,"reconnection by content hash");
 Call(reloaded,"UpdateCabinet",0,new byte[4096]);Assert(!reloaded.Cabinets[1].HasCapture,"replacement clears stale details");
 Call(reloaded,"UpdateCabinet",0,sector);Assert(reloaded.Cabinets[1].Name==capture.Title,"restore restores capture");
 var failedData=data+"-failure";var failVm=new EditorViewModel(failedData);var bad=new FakeConnection{CorruptFlashReadback=true};var badSession=new CubeSession(bad);await badSession.Identify();typeof(EditorViewModel).GetField("session",Flags)!.SetValue(failVm,badSession);
 try{await (Task)Call(failVm,"WriteIrAsync",0,bytes,80,sample.Name,null,false,sample.FullPath);throw new Exception("Expected verification error");}catch(IOException){}
 Assert(!File.Exists(Path.Combine(failedData,"cabinet-captures-v1.json")),"failed write does not persist capture");
 var legacy=new EditorViewModel(data+"-legacy");await (Task)Call(legacy,"RecoverCaptureAsync",hash,sector,sample.Name);Call(legacy,"UpdateCabinet",0,sector);Assert(legacy.Cabinets[1].Name==capture.Title,"legacy identification from bytes without USB session");
 var source=System.Xml.Linq.XDocument.Load(Path.Combine(AppContext.BaseDirectory,"Source/MainWindow.xaml"));
 var root=source.Root!;var ns=System.Xml.Linq.XNamespace.Get("http://schemas.microsoft.com/winfx/2006/xaml/presentation");
 var textNodes=root.Descendants(ns+"TextBlock").Where(e=>new[]{"FocusedGear.Name","FocusedGear.Subtitle","FocusedGear.CaptureSummary"}.Any(p=>((string)e.Attribute("Text")??"").Contains(p))).ToArray();
 var panelXml=new System.Xml.Linq.XElement(ns+"StackPanel",root.Attributes().Where(a=>a.IsNamespaceDeclaration),textNodes.Select(e=>new System.Xml.Linq.XElement(e)));
 panelXml.SetAttributeValue(System.Xml.Linq.XNamespace.Xmlns+"loc","clr-namespace:Cube.Desktop.Localization;assembly=CubeControl");
 Application.Current.Resources["Visible"]=new System.Windows.Controls.BooleanToVisibilityConverter();
 foreach(double width in new[]{260d,640d}){
 var panel=(System.Windows.Controls.StackPanel)System.Windows.Markup.XamlReader.Parse(panelXml.ToString());
 panel.DataContext=new{FocusedGear=vm.Cabinets[2]};panel.Measure(new Size(width,double.PositiveInfinity));panel.Arrange(new Rect(0,0,width,panel.DesiredSize.Height));panel.UpdateLayout();
 var caption=panel.Children.OfType<System.Windows.Controls.TextBlock>().Last();
 Assert(caption.Text==capture.Summary && caption.TextAlignment==TextAlignment.Right && caption.ActualWidth<=width && caption.ActualHeight>0,"capture layout at width "+width);
 }
 vm.Dispose();reloaded.Dispose();failVm.Dispose();legacy.Dispose();
 }
}
