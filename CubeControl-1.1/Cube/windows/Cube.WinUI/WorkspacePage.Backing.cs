using Cube.Desktop.ViewModels;
using Cube.WinUI.Platform;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;

namespace Cube.WinUI;
public sealed partial class WorkspacePage
{
    private void BuildBacking()
    {
        Section("Linha do tempo", new TextBlock { Text = T("Clique para posicionar; Shift + arraste para selecionar A–B."), TextWrapping = TextWrapping.Wrap }, new SessionTimeline(B), Buttons(Command(B, "ZoomIn", "Ampliar"), Command(B, "ZoomOut", "Reduzir"), Command(B, "FitWaveform", "Exibir tudo")), Number(B, "ViewportStart", "Início da visualização (s)", 0, 86400));
        Section("Sessões", Edit(B, "Search", "Buscar sessão"), Library(B, "FilteredTracks", "SelectedTrack", "Sessão", "Title"), Buttons(Command(B, "AddFiles", "Adicionar áudio"), Command(B, "RemoveTrack", "Remover sessão"), Command(B, "MoveUp", "Mover acima"), Command(B, "MoveDown", "Mover abaixo"), Command(B, "LocateFile", "Localizar arquivo ausente"), Command(B, "ShowFile", "Mostrar arquivo")), Text(B, "SaveState"));
        var seek = new Slider { Minimum = 0 }; Bind(seek, RangeBase.MaximumProperty, B, "Duration"); Bind(seek, RangeBase.ValueProperty, B, "Position", true); Bind(seek, IsEnabledProperty, B, "CanSeek"); Microsoft.UI.Xaml.Automation.AutomationProperties.SetName(seek, "Posição na sessão");
        Section("Transporte", Buttons(Command(B, "PlayPause", "Reproduzir / pausar"), Command(B, "Stop", "Parar"), Command(B, "Previous", "Sessão anterior"), Command(B, "Next", "Próxima sessão"), Command(B, "SkipBack", "Retroceder"), Command(B, "SkipForward", "Avançar")), Text(B, "TimeLabel"), seek, Number(B, "Speed", "Velocidade (%)", 50, 150), Number(B, "Pitch", "Afinação (semitons)", -6, 6), Buttons(Command(B, "ResetSpeed", "Restaurar velocidade"), Command(B, "ResetPitch", "Restaurar afinação")));
        Section("Trecho e estudo", Buttons(Command(B, "MarkStart", "Marcar início A"), Command(B, "MarkEnd", "Marcar fim B"), Command(B, "GoToA", "Ir ao início A"), Command(B, "ResetRange", "Limpar trecho")), Text(B, "RangeLabel"), Check(B, "IsLooping", "Repetir trecho A–B"), Number(B, "Bpm", "BPM original", 20, 300), Choice(B, "Keys", "OriginalKey", "Tom original"), Edit(B, "Tuning", "Afinação da guitarra"), Choice(B, "PresetOptions", "LinkedPreset", "Preset associado", index: true), Edit(B, "SectionName", "Nome do marcador"), Choice(B, "Sections", "SelectedSection", "Marcadores", "Label"), Buttons(Command(B, "AddSection", "Adicionar marcador"), Command(B, "RemoveSection", "Remover marcador"), Command(B, "GoToSection", "Ir ao marcador")));
        Section("Dispositivos e mixagem", Choice(B, "Outputs", "SelectedOutput", "Saída de áudio", "Name"), Choice(B, "Inputs", "SelectedInput", "Entrada de gravação", "Name"), Buttons(Command(B, "RefreshOutputs", "Atualizar saídas"), Command(B, "RefreshInputs", "Atualizar entradas")), Rack(Instrument(B, "MasterDb", "Ganho master (dB)", -60, 12, .1, true), Instrument(B, "Volume", "Acompanhamento (%)", 0, 200, 1, true), Instrument(B, "ClickDb", "Metrônomo (dB)", -60, 12, .1, true)));
        var stems = Section("Pistas e gravação", Choice(B, "SessionStems", "SelectedStem", "Pista", "Name"), Buttons(Command(B, "AddEmptyStem", "Nova pista"), Command(B, "ImportStem", "Importar áudio na pista"), Command(B, "DuplicateStem", "Duplicar pista"), Command(B, "RemoveStem", "Remover pista"), Command(B, "StemUp", "Mover pista acima"), Command(B, "StemDown", "Mover pista abaixo"), Command(B, "LocateStem", "Localizar áudio da pista")));
        var stemFields = new StackPanel { Spacing = 8 }; stems.Children.Add(stemFields);
        void RenderStem()
        {
            stemFields.Children.Clear(); if (B.SelectedStem is not { } s) return;
            stemFields.Children.Add(Edit(s, "Name", "Nome da pista"));
            var switches = Rack(); stemFields.Children.Add(switches);
            foreach (var (path, label) in new[] { ("IsArmed", "Armar gravação"), ("IsMonitoring", "Monitorar entrada"), ("IsMuted", "Silenciar pista"), ("IsSolo", "Solo") }) switches.Children.Add(Check(s, path, label));
            var input = new ComboBox { Header = T("Canais de entrada"), DisplayMemberPath = "Label", HorizontalAlignment = HorizontalAlignment.Stretch }; Bind(input, ItemsControl.ItemsSourceProperty, B, "InputChoices"); Bind(input, Selector.SelectedItemProperty, s, "InputChoice", true); stemFields.Children.Add(input);
            stemFields.Children.Add(Rack(Instrument(s, "GainDb", "Ganho da pista (dB)", -60, 12, .1, true), Instrument(s, "Pan", "Panorama (-1 esquerda, 1 direita)", -1, 1, .1)));
            stemFields.Children.Add(Choice(s, "Takes", "ActiveTake", "Take da pista", "Title"));
        }
        RenderStem(); Observe(B, "SelectedStem", RenderStem);
        var record = Command(B, "ToggleRecording", "Gravar"); Bind(record, ContentControl.ContentProperty, B, "RecordingLabel"); Bind(record, Microsoft.UI.Xaml.Automation.AutomationProperties.NameProperty, B, "RecordingLabel"); stems.Children.Add(record); stems.Children.Add(Text(B, "RecordingState")); stems.Children.Add(Text(B, "RecordingTime")); stems.Children.Add(Number(B, "LatencyCompensationMs", "Compensação de latência (ms)", -1000, 1000));

        BuildClips();
        var metronome = Section("Metrônomo", Command(B, "ToggleMetronome", "Ligar / desligar metrônomo"), Text(B, "MetronomeStatus"), Check(B, "FollowTrack", "Seguir sessão"), Number(B, "MetronomeBpm", "BPM do metrônomo", 20, 300), Choice(B, "MetronomeMeters", "MetronomeMeter", "Compasso"), Choice(B, "MetronomeCountIns", "CountInBars", "Compassos de contagem inicial"), Check(B, "MetronomeAccent", "Acentuar primeiro tempo"), Buttons(Command(B, "TapMetronome", "Tap tempo"), Command(B, "MarkFirstBeat", "Marcar primeiro tempo")));
        var sounds = new ComboBox { Header = T("Som"), ItemsSource = B.BuiltInMetronomeSounds, SelectedItem = B.MetronomeSound }; sounds.SelectionChanged += (_, _) => { if (sounds.SelectedItem is string sound) B.SelectMetronomeSound(sound); }; metronome.Children.Add(sounds);
        metronome.Children.Add(Action("Importar som do metrônomo", async () => { var d = new OpenFileDialog { Filter = "Áudio WAV (*.wav)|*.wav" }; if (d.ShowDialog() == true) await B.ImportMetronomeSound(d.FileName); }));
        Section("Exportação WAV", Check(B, "ExportMix", "Exportar todas as pistas (desmarque para exportar a selecionada)"), Check(B, "ExportInterval", "Exportar somente o intervalo A–B"), Buttons(Command(B, "ExportRange", "Exportar mixagem WAV"), Command(B, "CancelExport", "Cancelar exportação")), Text(B, "Status"));
        BuildLibraries();
    }
    private void BuildClips()
    {
        var panel = Section("Clipes", Check(B, "SnapEnabled", "Ajustar à grade"), Choice(B, "SnapDivisions", "SnapDivision", "Divisão da grade"), Buttons(Action("Desfazer", () => B.UndoEdit()), Action("Refazer", () => B.UndoEdit(true)), Action("Dividir no cursor", B.SplitClips), Action("Excluir clipes", B.DeleteClips), Action("Copiar", B.CopyClips), Action("Colar no cursor", () => B.PasteClips()), Action("Duplicar", () => B.PasteClips(true)), Action("Crossfade dos selecionados", B.CrossfadeClips)));
        var list = new ListView { SelectionMode = ListViewSelectionMode.Multiple, MaxHeight = 250, DisplayMemberPath = "Label" }; panel.Children.Add(list);
        bool updating = false;
        void Render()
        {
            updating = true;
            list.ItemsSource = (B.SessionStems ?? []).SelectMany(s => s.Clips.Select(c => new ClipRow(s, c))).ToArray();
            foreach (ClipRow row in list.Items) if (B.SelectedClips.Contains(row.Clip.Id)) list.SelectedItems.Add(row);
            updating = false;
        }
        list.SelectionChanged += (_, _) => { if (updating) return; var rows = list.SelectedItems.Cast<ClipRow>().ToArray(); B.SelectedClips.Clear(); foreach (var row in rows) B.SelectedClips.Add(row.Clip.Id); if (rows.LastOrDefault() is { } last) B.SelectedStem = last.Stem; };
        Render(); Observe(B, "ClipRevision", Render); Observe(B, "SelectedTrack", Render);
        panel.Children.Add(Action("Mover clipes selecionados", async () =>
        {
            if (!B.CanEditSession || !B.SelectedClipItems().Any()) return;
            var offset = new NumberBox { Header = T("Deslocamento (s)"), Value = 0 };
            var destination = new ComboBox { Header = T("Pista de destino"), ItemsSource = B.SessionStems, DisplayMemberPath = "Name", SelectedItem = B.SelectedStem };
            var fields = new StackPanel { Spacing = 8 }; fields.Children.Add(offset); fields.Children.Add(destination);
            if (await Dialogs.Show(new ContentDialog { Title = T("Mover clipes"), Content = fields, PrimaryButtonText = "Mover", CloseButtonText = "Cancelar" }) != ContentDialogResult.Primary || !double.IsFinite(offset.Value)) return;
            B.BeginClipGesture(); try { B.MoveClips(offset.Value, destination.SelectedItem as BackingStem); B.EndClipGesture(); } catch { B.CancelClipGesture(); throw; }
        }));
        panel.Children.Add(Action("Localizar arquivo do clipe", async () =>
        {
            if (list.SelectedItem is not ClipRow row || !B.CanEditSession) return;
            var sources = new ComboBox { Header = T("Arquivo a substituir"), ItemsSource = row.Clip.Takes.SelectMany(t => t.Layers).Select(l => l.Source).Distinct().ToArray(), SelectedIndex = 0, MaxWidth = 450 };
            if (await Dialogs.Show(new ContentDialog { Title = T("Localizar arquivo do clipe"), Content = sources, PrimaryButtonText = "Escolher arquivo", CloseButtonText = "Cancelar" }) != ContentDialogResult.Primary || sources.SelectedItem is not string oldPath) return;
            var file = new OpenFileDialog { Filter = "Áudio WAV/MP3|*.wav;*.mp3" };
            if (file.ShowDialog() == true) await B.RelinkClipSource(row.Stem, oldPath, file.FileName);
        }));
        panel.Children.Add(Action("Editar clipe selecionado", async () =>
        {
            if (list.SelectedItem is not ClipRow row || !B.CanEditSession) return;
            var c = row.Clip;
            NumberBox Field(string label, double value, double min = 0, double max = double.MaxValue) => new() { Header = label, Value = value, Minimum = min, Maximum = max };
            var position = Field("Posição (s)", c.Position); var duration = Field("Duração (s)", c.Duration, .001); var fadeIn = Field("Fade in (s)", c.FadeIn, 0, c.Duration); var fadeOut = Field("Fade out (s)", c.FadeOut, 0, c.Duration);
            var trimStart = Field(T("Recortar início (s)"), 0, 0, Math.Max(0, c.Duration - .001));
            var powerIn = new CheckBox { Content = T("Fade in de potência constante"), IsChecked = c.EqualPowerIn };
            var powerOut = new CheckBox { Content = T("Fade out de potência constante"), IsChecked = c.EqualPowerOut };
            var takes = new ComboBox { Header = T("Take"), ItemsSource = c.Takes, DisplayMemberPath = "Name", SelectedItem = c.ActiveTake };
            var fields = new StackPanel { Spacing = 8 }; foreach (var field in new UIElement[] { position, trimStart, duration, fadeIn, fadeOut, powerIn, powerOut, takes }) fields.Children.Add(field);
            if (await Dialogs.Show(new ContentDialog { Title = "Editar clipe", Content = fields, PrimaryButtonText = "Aplicar", CloseButtonText = "Cancelar" }) != ContentDialogResult.Primary) return;
            var changed = c.Trim(c.Position + trimStart.Value, c.Position + trimStart.Value + duration.Value) with { Position = position.Value, FadeIn = fadeIn.Value, FadeOut = fadeOut.Value, EqualPowerIn = powerIn.IsChecked == true, EqualPowerOut = powerOut.IsChecked == true, ActiveTakeId = (takes.SelectedItem as BackingClipTake)?.Id ?? c.ActiveTakeId };
            changed.Validate(); B.BeginClipGesture(); try { B.ReplaceClip(row.Stem, changed); B.EndClipGesture(); } catch { B.CancelClipGesture(); throw; }
        }));
    }
    private sealed record ClipRow(BackingStem Stem, BackingClip Clip)
    { public string Label => $"{Stem.Name} · {Clip.Position:0.00}–{Clip.End:0.00} s · {Clip.ActiveTake?.Name}"; }
    private void BuildLibraries()
    {
        var local = new ListView { DisplayMemberPath = "Name", MaxHeight = 220 }; Bind(local, ItemsControl.ItemsSourceProperty, B, "LocalFiles");
        Section("Biblioteca local", Command(B, "AddLibraryFolder", "Adicionar pasta"), Edit(B, "LocalSearch", "Buscar áudio local"), Choice(B, "LocalCategories", "LocalCategory", "Categoria"), local, Buttons(Action("Ouvir arquivo", async () => { if (local.SelectedItem is LocalAudioFile f) await B.PreviewLocalAudio(f); }), Action("Parar prévia", B.StopLocalPreview), Action("Inserir na sessão", async () => { if (local.SelectedItem is LocalAudioFile f) await B.InsertLocalAudio(f.Path); })));
        var results = new ListView { DisplayMemberPath = "Title", MaxHeight = 250 }; Bind(results, ItemsControl.ItemsSourceProperty, B, "PresentedYouTubeResults");
        Section("YouTube", Edit(B, "YouTubeQuery", "Buscar no YouTube"), Check(B, "AppendBackingTrack", "Buscar backing tracks"), Text(B, "YouTubeStatus"), results, Buttons(Action("Baixar áudio selecionado", async () => { if (results.SelectedItem is Cube.Desktop.Services.YouTubeResult result) await B.DownloadYouTube(result); }), Command(B, "CancelYouTubeDownload", "Cancelar download")), Text(B, "YouTubeDownloadStatus"), Buttons(Action("Ouvir prévia selecionada", async () => { if (results.SelectedItem is Cube.Desktop.Services.YouTubeResult result) await B.PreviewYouTube(result, true); }), Action("Parar prévia online", () => B.StopYouTubePreview()), Command(B, "ChangeYouTubeDownloadFolder", "Pasta dos downloads")), Text(B, "YouTubeDownloadFolderLabel"), Text(B, "YouTubePreviewStatus"));
    }
}
