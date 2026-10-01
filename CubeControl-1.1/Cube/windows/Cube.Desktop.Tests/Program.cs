using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Cube.Desktop.Controls;
using Cube.Desktop.ViewModels;
using Cube.Protocol;

internal static class Program
{
    [STAThread]
    static int Main()
    {
        var app = new Cube.Desktop.App { ShutdownMode = ShutdownMode.OnExplicitShutdown }; app.InitializeComponent();
        SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext());
        Task run = Environment.GetCommandLineArgs().Contains("--delay-only") ? DelayDesktopTests.Run() : Environment.GetCommandLineArgs().Contains("--reverb-only") ? ReverbDesktopTests.Run() : Environment.GetCommandLineArgs().Contains("--chorus-only") ? ChorusDesktopTests.Run() : Environment.GetCommandLineArgs().Contains("--phaser-only") ? PhaserDesktopTests.Run() : Environment.GetCommandLineArgs().Contains("--carousel-only") ? GearCarouselTests.Run() : Environment.GetCommandLineArgs().Contains("--artwork-only") ? GearArtworkTests.Run() : Environment.GetCommandLineArgs().Contains("--buttons-only") ? ButtonSizingTests.Run() : Environment.GetCommandLineArgs().Contains("--effects-only") ? EffectVisualizationTests.Run() : Environment.GetCommandLineArgs().Contains("--chevrons-only") ? ChevronTests.Run() : Environment.GetCommandLineArgs().Contains("--scroll-profile") ? ScrollProfileTests.Run() : Environment.GetCommandLineArgs().Contains("--snackbar-only") ? SnackbarTests.Run() : Environment.GetCommandLineArgs().Contains("--auto-connect-only") ? AutoConnectionTests.Run() : Environment.GetCommandLineArgs().Contains("--save-presets-only") ? SavePresetsTests.Run() : Environment.GetCommandLineArgs().Contains("--motion-only") ? MotionTests.Run() : Environment.GetCommandLineArgs().Contains("--ir-library-ui") ? IrLibraryUiTests.Run() : (Environment.GetCommandLineArgs().Contains("--ir-library-only") || Environment.GetCommandLineArgs().Contains("--ir-catalog-only")) ? IrLibraryTests.Run() : Environment.GetCommandLineArgs().Contains("--progress-only") ? ProgressPresentationTests.Run() : Environment.GetCommandLineArgs().Contains("--options-menu-only") ? OptionsMenuTests.Run() : Environment.GetCommandLineArgs().Contains("--ui-audit") ? UiAuditTests.Run() : Environment.GetCommandLineArgs().Contains("--microphone-only") ? MicrophoneTests.Run() : Environment.GetCommandLineArgs().Contains("--knob-wheel-only") ? KnobWheelTests.Run() : Environment.GetCommandLineArgs().Contains("--density-only") ? DensityTests.Run() : (Environment.GetCommandLineArgs().Contains("--zoom-compact-only") || Environment.GetCommandLineArgs().Contains("--zoom-dialogs-only") || Environment.GetCommandLineArgs().Contains("--zoom-only") || Environment.GetCommandLineArgs().Contains("--responsive-only") || Environment.GetCommandLineArgs().Contains("--volume-only")) ? ResponsiveTests.Run() : Environment.GetCommandLineArgs().Contains("--wheel-only") ? WheelTests.Run() : Environment.GetCommandLineArgs().Contains("--chain-only") ? ChainTests.Run() : Environment.GetCommandLineArgs().Contains("--presentation-render") ? PresentationTests.RenderFixture() : Environment.GetCommandLineArgs().Contains("--theme-only") ? ThemeTests.Run() : Environment.GetCommandLineArgs().Contains("--localization-only") ? LocalizationTests.Run() : Run(); run.ContinueWith(_ => app.Dispatcher.BeginInvokeShutdown(DispatcherPriority.Background), TaskScheduler.Default);
        Dispatcher.Run();
        try { run.GetAwaiter().GetResult(); return 0; } catch (Exception ex) { Console.Error.WriteLine(ex); return 1; }
    }
    static async Task Run()
    {
        if (!Environment.GetCommandLineArgs().Contains("--editor-only"))
        {
            await IrLibraryTests.Run();
            await PresentationTests.Run();
        }
        int passed = 0;
        void Check(bool condition, string name) { if (!condition) throw new Exception(name); Console.WriteLine("PASS " + name); passed++; }
        var knob = new Knob { Maximum = 7, Value = 3, Width = 100, Height = 100 };
        var panel = new StackPanel(); panel.Children.Add(knob); int bubbled = 0, starts = 0, ends = 0;
        panel.AddHandler(Mouse.MouseWheelEvent, new MouseWheelEventHandler((_, _) => bubbled++));
        panel.AddHandler(Knob.GestureStartedEvent, new RoutedEventHandler((_, _) => starts++));
        panel.AddHandler(Knob.GestureEndedEvent, new RoutedEventHandler((_, _) => ends++));
        void Wheel(int delta) => knob.RaiseEvent(new MouseWheelEventArgs(Mouse.PrimaryDevice, 0, delta) { RoutedEvent = Mouse.MouseWheelEvent });
        Wheel(120); Check(!knob.IsKeyboardFocused && knob.Value == 4 && bubbled == 0, "wheel adjusts without focus and prevents page scrolling");
        Wheel(30); Wheel(30); Wheel(30); Check(knob.Value == 4, "fractional wheel waits for complete step"); Wheel(30);
        Check(knob.Value == 5 && starts == 1, "wheel burst remains one gesture");
        await Task.Delay(500);
        await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
        Check(ends == 1, "wheel idle finishes undo gesture");
        Wheel(1200); Check(knob.Value == 7, "wheel respects upper bound");
        knob.IsEnabled = false; Wheel(-120); Check(knob.Value == 7 && bubbled == 1, "disabled knob does not edit or consume scrolling");
        panel.RaiseEvent(new MouseWheelEventArgs(Mouse.PrimaryDevice, 0, 120) { RoutedEvent = Mouse.MouseWheelEvent });
        Check(bubbled == 2, "scroll outside knob propagates normally");
        var events = new List<(ParameterKey Key, int Value)>(); var p = new PresetViewModel(1, (k, v) => events.Add((k, v)));
        var bank = new byte[48]; bank[24] = 7; p.Load(bank);
        Check(events.Count == 0 && p.Parameters[8].Value == 7, "load and render preserve off 7 without writes");
        p.Musical.ChoosePhaser.Execute(null); p.Musical.ModulationDial.Value = 6;
        Check(events.Count == 0, "disabled phaser selection and adjustment stay local");
        p.Musical.ModulationEnabled = true; Check(events.Count == 2 && events[0] == (new ParameterKey(1, 8), 14) && events[1] == (new ParameterKey(1, 11), 1), "semantic phaser enable sets amount and shared gate on original preset");
        p.Musical.ReverbDial.Value = 12; Check(events.Count == 2, "disabled reverb edit stays local");
        p.Musical.ReverbEnabled = true; Check(events[^1] == (new ParameterKey(1, 3), 12), "reverb enable restores prepared amount");

        string folder = Path.Combine(Path.GetTempPath(), "cube-desktop-tests-" + Guid.NewGuid().ToString("N")); Directory.CreateDirectory(folder);
        try
        {
            File.WriteAllText(Path.Combine(folder, "bad.png"), "not an image");
            Check(GearItem.ReadImage(Path.Combine(folder, "missing.png")) == null && GearItem.ReadImage(Path.Combine(folder, "bad.png")) == null, "missing and invalid images use generic illustration");
            string png = Path.Combine(folder, "valid.png");
            var bitmap = BitmapSource.Create(1, 1, 96, 96, System.Windows.Media.PixelFormats.Bgra32, null, new byte[] { 1, 2, 3, 255 }, 4);
            var encoder = new PngBitmapEncoder(); encoder.Frames.Add(BitmapFrame.Create(bitmap)); using (var output = File.Create(png)) encoder.Save(output);
            Check(GearItem.ReadImage(png) != null, "custom PNG decodes independently of original file handle");
            Check(Enumerable.Range(0, 9).All(i => new GearItem($"amp-{i}", i, "", "", "amp", "").DefaultImage != null), "all nine bundled amp photos are available in output");
            Check(Enumerable.Range(0, 8).All(i => new GearItem("content-hash", 8, "", "", "cab", "", factoryId: $"factory-cab-{i}").DefaultImage != null), "eight factory cabinet photos follow identity even in another slot");
            Check(new GearItem("custom-content", 1, "", "", "cab", "").DefaultImage == null && new GearItem("unlisted-effect", 1, "", "", "phaser", "").DefaultImage == null, "unknown content and equipment without photos retain generic artwork");
            using var fake = new FakeConnection();
            for (int i = 0; i < 8; i++) fake.Flash[i * 4096] = (byte)(i + 1);
            fake.Bank[8] = 8; fake.Bank[10] = 0;
            string legacyHash = CabinetInventory.Hash(fake.Flash[..4096]); Directory.CreateDirectory(Path.Combine(folder, "IrNames")); File.WriteAllText(Path.Combine(folder, "IrNames", legacyHash + ".txt"), "Meu cabinet antigo");
            using var session = new CubeSession(fake); await session.Identify();
            Directory.CreateDirectory(Path.Combine(folder, "Images")); File.Copy(png, Path.Combine(folder, "Images", "custom.png"));
            var photoPreferences = new VisualPreferences(); photoPreferences.Images["amp-0"] = "custom.png"; photoPreferences.Images["amp-1"] = "missing.png";
            photoPreferences.Save(Path.Combine(folder, "visual-preferences-v1.json"));
            using var model = new EditorViewModel(folder);
            Check(model.Amps[0].Image != null && model.Amps[0].Image!.Height == model.Amps[0].Image!.Width, "custom square photo takes precedence over bundled amp photograph");
            Check(model.Amps[1].Image != null, "missing custom photograph falls back to bundled photograph");
            model.ResetImage.Execute(null);
            Check(!VisualPreferences.Load(Path.Combine(folder, "visual-preferences-v1.json")).Images.ContainsKey("amp-0") && model.Amps[0].Image != null, "reset removes custom preference and restores bundled photograph");
            var loadingCounts = new List<int>();
            bool loadedPresetsWhileReading = false;
            model.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(model.InventoryReadCount)) loadingCounts.Add(model.InventoryReadCount);
                if (e.PropertyName == nameof(model.BusyDetail) && model.ReadingInventory)
                    loadedPresetsWhileReading |= model.HasBank && model.Busy && !model.CanEdit && model.BusyDetail.Contains("Presets carregados");
            };
            await model.Execute(() => model.AttachSession(session));
            Check(loadedPresetsWhileReading && loadingCounts.SequenceEqual(Enumerable.Range(1, 8)), "loaded presets remain distinct from real eight-slot inventory progress");
            Check(!model.Busy && !model.ReadingInventory && model.CanEdit, "connection finishes and releases editing after inventory completes");
            Check(fake.Writes == 0 && model.CanMatch, "connect inventories IRs without any writes");
            Check(model.ConnectionSummary == "Conectado por USB" && !model.HasEditNotice, "connection and editor availability reflect confirmed session");
            Check(model.InventoryStatus.Contains("8 slots lidos") && model.InventoryStatus.Contains("0 referências"), "read inventory does not imply known factory equipment");
            Check(model.IrAvailability == "Selecione um WAV para ouvir ou gravar.", "IR actions explain the missing file prerequisite");
            Check(model.Cabinets[1].Name == "Meu cabinet antigo", "legacy content-keyed IR name migrates");
            var rows = model.CreateMatchRows(); rows[0].Selected = rows[0].Choices.First(c => c.Value == 3); rows[2].Selected = rows[2].Choices.First(c => c.Value == 5); model.SaveMatches(rows);
            Check(fake.Writes == 0, "association table sends no commands");
            model.ChooseGear(model.Amps[0]); model.MatchCabinet.Execute(null); await model.Flush();
            Check(fake.Bank[0] == 0 && fake.Bank[9] == 3 && fake.Bank[10] == 0, "manual match selects associated content without enabling IR");
            model.ChooseGear(model.Cabinets[2]); await model.Flush();
            Check(fake.Bank[9] == 2, "manual cabinet selection prevails");
            model.SelectedIndex = 2; model.ChooseGear(model.Amps[0]); model.MatchCabinet.Execute(null); model.SelectedIndex = 0; await model.Flush();
            Check(fake.Bank[41] == 3 && fake.Bank[9] == 2, "queued match remains bound to preset C after selection changes");
            object group = new(); model.BeginGesture(group); model.Current.Parameters[1].Value = 2; model.Current.Parameters[1].Value = 4; model.EndGesture(group); await model.Flush();
            model.Undo.Execute(null); while (model.Busy) await Task.Delay(10);
            Check(fake.Bank[1] == 0, "undo restores full knob gesture through same queue");
            model.ChooseGear(model.Amps[2]); model.MatchCabinet.Execute(null); await model.Flush();
            Check(fake.Bank[0] == 2 && fake.Bank[9] == 5, "compound amp plus match applied");
            model.Undo.Execute(null); while (model.Busy) await Task.Delay(10);
            Check(fake.Bank[0] == 0 && fake.Bank[9] == 2, "one undo restores amp and cabinet together");
            model.ChooseGear(model.Amps[2]); model.MatchCabinet.Execute(null); model.ChooseGear(model.Amps[3]); model.MatchCabinet.Execute(null); await model.Flush();
            Check(fake.Bank[0] == 3 && fake.Bank[9] == 2 && model.Current.Parameters[9].Value == 2, "superseded pending match leaves no phantom cabinet edit");
            model.ChooseGear(model.Amps[0]); model.MatchCabinet.Execute(null); fake.Flash[2 * 4096] = 88; await model.Flush();
            Check(fake.Bank[0] == 0 && fake.Bank[9] == 2 && model.Status.Contains("suspenso"), "changed slot is rechecked and suspended before cabinet write");
            int before = fake.Writes;
            model.SelectedStage = 2; model.SelectedIndex = 1; model.SelectedIndex = 0; await model.Flush();
            Check(fake.Writes == before, "module and preset navigation never trigger automatic match");
            model.ChooseGear(model.Amps[2]); await model.Flush();
            Check(fake.Bank[0] == 2 && fake.Bank[9] == 2, "amp selection preserves cabinet without automatic matching");
            model.MatchCabinet.Execute(null); await model.Flush();
            Check(fake.Bank[9] == 5 && fake.Bank[10] == 0, "manual Match button applies the saved association");
            model.ChooseGear(model.Amps[3]); await model.Flush();
            model.ChooseGear(model.Amps[2]); model.MatchCabinet.Execute(null); await model.Flush();
            Check(fake.Bank[0] == 2 && fake.Bank[9] == 5, "manual match preserves the pending explicit amp selection");
            model.ChooseGear(model.Cabinets[2]); await model.Flush();
            var saved = VisualPreferences.Load(Path.Combine(folder, "visual-preferences-v1.json"));
            Check(saved.IrNames[legacyHash] == "Meu cabinet antigo", "preferences and migrated names persist");
            Check(fake.FlashWrites == 0 && fake.Erases == 0, "visual and matching workflows never rewrite IR sectors");
            fake.Observe = packet =>
            {
                if (packet.Command == 0x22 && packet.Payload[0] == 5 && System.Buffers.Binary.BinaryPrimitives.ReadUInt32LittleEndian(packet.Payload.AsSpan(1)) == 0x80000009) throw new IOException("Disconnected between amp and cabinet");
            };
            model.ChooseGear(model.Amps[2]); model.MatchCabinet.Execute(null); model.Current.Parameters[1].Value = 7;
            bool interrupted = false; try { await model.Flush(); } catch (IOException) { interrupted = true; }
            Check(interrupted && !model.CanEdit && model.Status.Contains("parcial") && fake.Bank[1] == 0 && model.Current.Parameters[1].Value == 7, "partial compound failure stops pending sends and preserves local editing");
            Check(model.HasError && model.ErrorMessage.Contains("parcial") && model.EditAvailability.Contains("suspensa"), "partial failure remains visible with recovery guidance");
        }
        finally
        {
            string resolved = Path.GetFullPath(folder);
            if (!resolved.StartsWith(Path.GetFullPath(Path.GetTempPath()), StringComparison.OrdinalIgnoreCase) || !Path.GetFileName(resolved).StartsWith("cube-desktop-tests-")) throw new IOException("Unexpected test directory.");
            Directory.Delete(resolved, true);
        }
        Console.WriteLine($"{passed} desktop checks passed.");
    }
}
