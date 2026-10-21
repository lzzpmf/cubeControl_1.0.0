using System.Text.Json;
using System.Windows;
using System.Windows.Threading;

internal sealed record TestSuite(string Id, string Alias, string Description, string Requirement, bool Wpf, Func<Task> Run);
internal static class TestCatalog
{
    internal static string[] Arguments { get; private set; } = Environment.GetCommandLineArgs();
    internal static readonly TestSuite[] Suites =
    [
        new("drag-drop", "--drag-drop-only", "Shared drag adapters, motion and cancellation", "local", true, DragDropTests.Run),
        new("wide-layout", "--wide-layout-only", "Five sections at wide work areas and simulated DPI", "local", true, ResponsiveTests.Run),
        new("player-layout", "--player-layout-only", "PlayerLayout", "local", true, () => PlayerLayoutTests.Run()),
        new("overdriven", "--overdriven-only", "OverdrivenLibrary", "local", true, () => OverdrivenLibraryTests.Run()),
        new("documentation", "--documentation-only", "Documentation", "local", true, () => DocumentationTests.Run()),
        new("ir-audition", "--ir-audition-only", "IrAudition", "local", true, () => IrAuditionTests.Run()),
        new("image-picker", "--image-picker-only", "GearImagePicker", "local", true, () => GearImagePickerTests.Run()),
        new("assets", "--assets-only", "AssetLoading", "local", true, () => AssetLoadingTests.Run()),
        new("youtube-preview-live", "--youtube-preview-live", "YouTubePreview", "online", true, () => YouTubePreviewTests.RunLive()),
        new("youtube", "--youtube-only", "YouTube", "local", true, () => YouTubeTests.Run()),
        new("backing-profile", "--backing-profile", "BackingPerformance", "audio-device", true, () => BackingPerformanceTests.Run()),
        new("backing-preview", "--backing-preview", "BackingPerformance", "audio-device", true, () => BackingPerformanceTests.Run()),
        new("pack-workflow", "--pack-workflow-only", "PackWorkflow", "local", true, () => PackWorkflowTests.Run()),
        new("pack-review-ui", "--pack-review-ui-only", "PackReviewUi", "local", true, () => PackReviewUiTests.Run()),
        new("curated-packs", "--curated-packs-only", "CuratedPack", "local", true, () => CuratedPackTests.Run()),
        new("live-session", "--live-session-only", "LiveSession", "local", true, () => LiveSessionTests.Run()),
        new("factory-irs", "--factory-irs-only", "FactoryIrSelection", "local", true, () => FactoryIrSelectionTests.Run()),
        new("amp-eq-presets", "--amp-eq-presets-only", "Amp EQ variants", "local", true, AmpEqPresetTests.Run),
        new("amp-capacitors", "--amp-capacitors-only", "AmpCapacitor", "local", true, () => AmpCapacitorTests.Run()),
        new("hardware-effect-cards", "--hardware-effect-cards", "AdvancedHardware", "hardware", true, () => AdvancedHardwareTests.Run(cardFlow: true)),
        new("hardware-advanced-lifecycle", "--hardware-advanced-lifecycle", "AdvancedHardware", "hardware", true, () => AdvancedHardwareTests.Run()),
        new("advanced-lifecycle", "--advanced-lifecycle-only", "AdvancedLifecycle", "local", true, () => AdvancedLifecycleTests.Run()),
        new("advanced-interaction", "--advanced-interaction-only", "AdvancedInteraction", "local", true, () => AdvancedInteractionTests.Run()),
        new("backing-responsive", "--backing-responsive", "BackingResponsiveness", "audio-device", true, () => BackingResponsivenessTests.Run()),
        new("metronome", "--metronome-only", "Metronome", "audio-device", true, () => MetronomeTests.Run()),
        new("backing-practice", "--backing-practice-only", "BackingPractice", "local", true, () => BackingPracticeTests.Run()),
        new("backing", "--backing-only", "BackingTracks", "audio-device", true, () => BackingTracksTests.Run()),
        new("effects-firmware-b", "--effects-firmware-b-only", "EffectsFirmwareBDesktop", "local", true, () => EffectsFirmwareBDesktopTests.Run()),
        new("delay", "--delay-only", "DelayDesktop", "local", true, () => DelayDesktopTests.Run()),
        new("reverb", "--reverb-only", "ReverbDesktop", "local", true, () => ReverbDesktopTests.Run()),
        new("chorus", "--chorus-only", "ChorusDesktop", "local", true, () => ChorusDesktopTests.Run()),
        new("phaser", "--phaser-only", "PhaserDesktop", "local", true, () => PhaserDesktopTests.Run()),
        new("carousel", "--carousel-only", "GearCarousel", "local", true, () => GearCarouselTests.Run()),
        new("artwork", "--artwork-only", "GearArtwork", "local", true, () => GearArtworkTests.Run()),
        new("buttons", "--buttons-only", "ButtonSizing", "local", true, () => ButtonSizingTests.Run()),
        new("effects", "--effects-only", "EffectVisualization", "local", true, () => EffectVisualizationTests.Run()),
        new("chevrons", "--chevrons-only", "Chevron", "local", true, () => ChevronTests.Run()),
        new("scroll-profile", "--scroll-profile", "ScrollProfile", "local", true, () => ScrollProfileTests.Run()),
        new("snackbar", "--snackbar-only", "Snackbar", "local", true, () => SnackbarTests.Run()),
        new("auto-connect", "--auto-connect-only", "AutoConnection", "local", true, () => AutoConnectionTests.Run()),
        new("save-presets", "--save-presets-only", "SavePresets", "local", true, () => SavePresetsTests.Run()),
        new("motion", "--motion-only", "Motion", "local", true, () => MotionTests.Run()),
        new("ir-library-ui", "--ir-library-ui", "IrLibraryUi", "local", true, () => IrLibraryUiTests.Run()),
        new("ir-library", "--ir-library-only", "IrLibrary", "local", true, () => IrLibraryTests.Run()),
        new("ir-catalog", "--ir-catalog-only", "IrLibrary", "local", true, () => IrLibraryTests.Run()),
        new("progress", "--progress-only", "ProgressPresentation", "local", true, () => ProgressPresentationTests.Run()),
        new("options-menu", "--options-menu-only", "OptionsMenu", "local", true, () => OptionsMenuTests.Run()),
        new("ui-audit", "--ui-audit", "UiAudit", "local", true, () => UiAuditTests.Run()),
        new("microphone", "--microphone-only", "Microphone", "local", true, () => MicrophoneTests.Run()),
        new("knob-wheel", "--knob-wheel-only", "KnobWheel", "local", true, () => KnobWheelTests.Run()),
        new("density", "--density-only", "Density", "local", true, () => DensityTests.Run()),
        new("zoom-compact", "--zoom-compact-only", "Responsive", "local", true, () => ResponsiveTests.Run()),
        new("zoom-dialogs", "--zoom-dialogs-only", "Responsive", "local", true, () => ResponsiveTests.Run()),
        new("zoom", "--zoom-only", "Responsive", "local", true, () => ResponsiveTests.Run()),
        new("responsive", "--responsive-only", "Responsive", "local", true, () => ResponsiveTests.Run()),
        new("volume", "--volume-only", "Responsive", "local", true, () => ResponsiveTests.Run()),
        new("wheel", "--wheel-only", "Wheel", "local", true, () => WheelTests.Run()),
        new("chain", "--chain-only", "Chain", "local", true, () => ChainTests.Run()),
        new("presentation-render", "--presentation-render", "Presentation", "local", true, () => PresentationTests.RenderFixture()),
        new("theme", "--theme-only", "Theme", "local", true, () => ThemeTests.Run()),
        new("localization", "--localization-only", "Localization", "local", true, () => LocalizationTests.Run()),
        new("midi-ports", "--midi-ports-only", "MIDI port matching with synthetic port lists", "local", false, () => Task.Run(MidiPortSelectionTests.Run)),
        new("multitrack-capture", "--multitrack-capture", "Offline PCM conversion, capture routing and stop lifecycle", "local", false, MultitrackCaptureTests.Run),
        new("backing-audio-boundaries", "--backing-audio-boundaries", "Offline gain, pan, monitor overflow/underrun and meter boundaries", "local", false, BackingAudioBoundaryTests.Run),
        new("backing-clips", "--backing-clips", "Clip editing, fades, overdub, undo and v4 persistence", "local", true, BackingClipTests.Run),
        new("backing-daw", "--backing-daw", "Multitrack gain, takes and mixer rendering", "local", true, BackingDawTests.Run),
        new("backing-session", "--backing-session-only", "Offline multitrack mix, persistence and export recovery", "local", false, () => Task.Run(BackingSessionTests.Run)),
        new("backing-recording", "--backing-recording-only", "BackingRecording", "local", false, () => BackingRecordingTests.Run()),
        new("firmware-worker", "--firmware-worker-only", "FirmwareWorker", "elevation", false, () => FirmwareWorkerTests.Run()),
        new("editor", "--editor-only", "Editor integration with simulated protocol", "local", true, () => Program.Run()),
        new("default", "--default", "Legacy default integration suite", "local", true, () => Program.Run()),
        new("direct-pack-editing", "--direct-pack-editing", "direct-pack-editing", "local", true, () => DirectPackEditingTests.Run()),
        new("footswitch", "--footswitch", "footswitch", "local", true, () => FootswitchTests.Run()),
        new("pack-compact-ui", "--pack-compact-ui", "pack-compact-ui", "local", true, () => PackCompactUiTests.Run()),
        new("backing-responsive-offline", "--backing-responsive-offline", "Waveform interaction without device playback", "local", true, () => BackingResponsivenessTests.Run(false)),
        new("backing-offline", "--backing-offline", "backing-offline", "local", true, () => BackingTracksTests.Run(false)),
        new("metronome-offline", "--metronome-offline", "metronome-offline", "local", true, () => MetronomeTests.Run(false)),
        new("borderless", "--borderless", "borderless", "local", true, () => BorderlessTests.Run()),
        new("qa-desktop", "--qa-all", "All local desktop QA batteries", "local", false, QaDesktop.RunAll),
        new("qa-usability", "--usability-all", "All local usability QA batteries", "local", false, QaUsability.RunAll),
        new("catalog-contract", "--catalog-contract", "CLI discovery and invalid argument handling", "local", false, CatalogTests.Run),
    ];
    internal static TestSuite? Resolve(string[] args, out string[] normalized)
    {
        normalized = [];
        const string rootPrefix = "--documentation-root=";
        string[] options = args.Where(a => a.StartsWith(rootPrefix, StringComparison.Ordinal)).ToArray();
        if (options.Length > 1 || options.Any(a => string.IsNullOrWhiteSpace(a[rootPrefix.Length..]))) return null;
        string[] selectors = args.Where(a => !a.StartsWith(rootPrefix, StringComparison.Ordinal)).ToArray();
        TestSuite? suite = selectors.Length == 0 ? Suites.Single(s => s.Id == "default")
            : selectors.Length == 2 && selectors[0] == "--suite" ? Suites.SingleOrDefault(s => s.Id == selectors[1])
            : selectors.Length == 1 ? Suites.SingleOrDefault(s => s.Alias == selectors[0]) : null;
        if (suite == null || (options.Length > 0 && suite.Id != "documentation")) return null;
        normalized = [Environment.GetCommandLineArgs()[0], suite.Alias, .. options];
        return suite;
    }

    internal static int Execute(string[] args)
    {
        if (args.SequenceEqual(new[] { "--list" }))
        {
            Console.WriteLine(JsonSerializer.Serialize(Suites.Select(s => new { s.Id, s.Alias, s.Description, s.Requirement, s.Wpf }), new JsonSerializerOptions { WriteIndented = true }));
            return 0;
        }
        TestSuite? suite = Resolve(args, out var normalized);
        if (suite == null) { Console.Error.WriteLine("Unknown or incompatible arguments. Use --list or --suite <id>."); return 2; }
        Arguments = normalized;
        try
        {
            if (!suite.Wpf) { suite.Run().GetAwaiter().GetResult(); return 0; }
            var app = new Cube.Desktop.App { ShutdownMode = ShutdownMode.OnExplicitShutdown };
            app.InitializeComponent();
            SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext());
            Task task = suite.Run();
            task.ContinueWith(_ => app.Dispatcher.BeginInvokeShutdown(DispatcherPriority.Background), TaskScheduler.Default);
            Dispatcher.Run(); task.GetAwaiter().GetResult(); return 0;
        }
        catch (Exception ex) { Console.Error.WriteLine(ex); return 1; }
    }
}
