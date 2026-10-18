using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Cube.Desktop.Localization;
using Cube.Desktop.ViewModels;

namespace Cube.Desktop;

public partial class MainWindow
{
    private void BackingNumberKey(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && sender is TextBox field)
        { field.GetBindingExpression(TextBox.TextProperty)?.UpdateSource(); e.Handled = true; }
    }
    private void BackingBpmClick(object sender, RoutedEventArgs e) => EditMetronomeNumber(false);
    private int backingBpmWheelDelta;
    private int backingSpeedWheelDelta;
    private int backingPitchWheelDelta;
    private void BackingTempoLeave(object sender, MouseEventArgs e)
    {
        if (sender == BackingSpeedOptions) backingSpeedWheelDelta = 0;
        else backingPitchWheelDelta = 0;
    }
    private void BackingTempoWheel(object sender, MouseWheelEventArgs e)
    {
        e.Handled = true;
        bool speed = sender == BackingSpeedOptions;
        ref int remainder = ref (speed ? ref backingSpeedWheelDelta : ref backingPitchWheelDelta);
        if (BackingTracks.SelectedTrack == null) { remainder = 0; return; }
        remainder += e.Delta;
        int steps = remainder / 120;
        remainder %= 120;
        if (steps == 0) return;
        if (speed) BackingTracks.Speed += steps;
        else BackingTracks.Pitch += steps;
    }
    private void BackingBpmLeave(object sender, MouseEventArgs e) => backingBpmWheelDelta = 0;
    private void BackingBpmWheel(object sender, MouseWheelEventArgs e)
    {
        e.Handled = true;
        backingBpmWheelDelta += e.Delta;
        int steps = backingBpmWheelDelta / 120;
        backingBpmWheelDelta %= 120;
        if (steps != 0) BackingTracks.MetronomeBpm += steps;
    }
    private void BackingMixerClick(object sender, RoutedEventArgs e)
    {
        BackingVolumesPanel.Visibility = BackingVolumesPanel.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
    }
    private void BackingMixerResetClick(object sender, RoutedEventArgs e) => BackingTracks.ResetMixerVolumes();
    private void BackingMixerMuteClick(object sender, RoutedEventArgs e)
    {
        switch ((sender as FrameworkElement)?.Tag as string)
        {
            case "track": BackingTracks.IsMuted = !BackingTracks.IsMuted; break;
            case "guitar": BackingTracks.MonitorMuted = !BackingTracks.MonitorMuted; break;
            case "click": BackingTracks.MetronomeMuted = !BackingTracks.MetronomeMuted; break;
        }
    }
    private void BackingMixerFaderReset(object sender, MouseButtonEventArgs e)
    {
        if (e.ClickCount != 2 || sender is not Controls.BackingFader fader) return;
        fader.SetCurrentValue(System.Windows.Controls.Primitives.RangeBase.ValueProperty, (fader.Tag as string) switch { "guitar" => 0d, "click" => 100d, _ => 30d });
        e.Handled = true;
    }
    private bool backingDialogOpen;
    private ContextMenu? backingOpenMenu;
    private static DependencyObject? BackingParent(DependencyObject node) => node is Visual ? VisualTreeHelper.GetParent(node) : LogicalTreeHelper.GetParent(node);
    private static ButtonBase? BackingButtonAncestor(DependencyObject? node)
    {
        for (; node != null; node = BackingParent(node)) if (node is ButtonBase button) return button;
        return null;
    }
    private void BackingMuteClick(object sender, RoutedEventArgs e) => BackingTracks.IsMuted = !BackingTracks.IsMuted;
    private void BackingMenuClick(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement element && element.Tag is string kind)
            OpenBackingMenu(element, kind, element.DataContext as BackingTrack, element.DataContext as BackingSection);
        e.Handled = true;
    }
    // Handle the press before ListBoxItem sees it: opening options must not select/play another track.
    private void BackingTrackMenuPress(object sender, MouseButtonEventArgs e) { BackingMenuClick(sender, e); backingDragTrack = null; }
    private void BackingTrackContext(object sender, MouseButtonEventArgs e)
    {
        if (e.OriginalSource is DependencyObject source && ItemsControl.ContainerFromElement(BackingTrackList, source) is ListBoxItem { DataContext: BackingTrack track } item)
        { OpenBackingMenu(item, "track", track, atMouse: true); e.Handled = true; }
    }
    private void BackingContextOpening(object sender, ContextMenuEventArgs e)
    {
        if (e.Handled || sender is not FrameworkElement element || element.Tag is not string kind) return;
        e.Handled = true;
        BackingTrack? track = element.DataContext as BackingTrack;
        BackingSection? section = element.DataContext as BackingSection;
        if (element == BackingTrackList)
        {
            if (e.OriginalSource is DependencyObject source && ItemsControl.ContainerFromElement(BackingTrackList, source) is ListBoxItem item)
            { track = item.DataContext as BackingTrack; element = item; }
            else track = BackingTrackList.SelectedItem as BackingTrack;
            if (track == null) return;
        }
        OpenBackingMenu(element, kind, track, section, atMouse: e.CursorLeft >= 0);
    }
    private void OpenBackingMenu(FrameworkElement owner, string kind, BackingTrack? track = null, BackingSection? section = null, double? time = null, bool atMouse = false)
    {
        if (backingOpenMenu != null) backingOpenMenu.IsOpen = false;
        var model = BackingTracks;
        var menu = new ContextMenu { PlacementTarget = owner, Placement = atMouse || time != null ? PlacementMode.MousePoint : PlacementMode.Bottom };
        void Command(string title, ICommand command, bool external = false) => menu.Items.Add(new MenuItem { Header = LanguageService.T(title), Command = command, Icon = external ? Cube.Desktop.Appearance.AppIcons.Create(Wpf.Ui.Controls.SymbolRegular.Open24) : null });
        void Action(string title, Action execute, Func<bool>? enabled = null, bool external = false) => Command(title, new UiCommand(execute, enabled), external);
        void Separator() => menu.Items.Add(new Separator());
        switch (kind)
        {
            case "metronome":
                void Choice(string title, bool selected, Action action, bool enabled = true)
                {
                    var item = new MenuItem { Header = LanguageService.T(title), IsCheckable = true, IsChecked = selected, IsEnabled = enabled };
                    item.Click += (_, _) => action(); menu.Items.Add(item);
                }
                Choice("Acompanhar faixa", model.FollowTrack, () => model.FollowTrack = !model.FollowTrack);
                Choice("Acentuar primeiro tempo", model.MetronomeAccent, () => model.MetronomeAccent = !model.MetronomeAccent);
                Action("Editar BPM", () => EditMetronomeNumber(false));
                var meters = new MenuItem { Header = LanguageService.T("Compasso") };
                foreach (string meter in model.MetronomeMeters)
                {
                    var item = new MenuItem { Header = meter, IsCheckable = true, IsChecked = meter == model.MetronomeMeter };
                    item.Click += (_, _) => model.MetronomeMeter = meter; meters.Items.Add(item);
                }
                menu.Items.Add(meters);
                var countIn = new MenuItem { Header = LanguageService.T("Contagem (compassos)"), IsEnabled = model.FollowTrack };
                foreach (int bars in model.MetronomeCountIns)
                {
                    var item = new MenuItem { Header = bars == 0 ? LanguageService.T("Desligado") : bars.ToString(), IsCheckable = true, IsChecked = bars == model.CountInBars };
                    item.Click += (_, _) => model.CountInBars = bars; countIn.Items.Add(item);
                }
                menu.Items.Add(countIn);
                Command("Marcar primeiro tempo aqui", model.MarkFirstBeat);
                Action("Primeiro tempo (s)", () => EditMetronomeNumber(true), () => model.CanSeek && model.FollowTrack);
                Separator();
                var sounds = new MenuItem { Header = LanguageService.T("Som do metrônomo") };
                foreach (string sound in model.BuiltInMetronomeSounds.Concat(model.CustomMetronomeFiles))
                {
                    var item = new MenuItem { Header = model.BuiltInMetronomeSounds.Contains(sound) ? LanguageService.T(sound) : System.IO.Path.GetFileNameWithoutExtension(sound), IsCheckable = true, IsChecked = sound == model.MetronomeSound };
                    item.Click += (_, _) => model.SelectMetronomeSound(sound); sounds.Items.Add(item);
                }
                menu.Items.Add(sounds);
                Action("Importar som WAV", () => ImportMetronomeWav());
                break;
            case "track":
                track ??= model.SelectedTrack;
                if (track == null) return;
                var target = track;
                bool Exists() => model.Tracks.Contains(target);
                Action("Reproduzir", () => { model.SelectedTrack = target; model.PlaySelected(); }, () => Exists() && model.SelectedOutput != null && !model.IsLoading);
                Action("Corrigir dados da faixa", () => EditBackingTrack(target), Exists);
                Action("Analisar novamente", () => model.ReanalyzeMusic(target), Exists);
                Separator();
                Action("Mover faixa para cima", () => model.MoveTrack(target, model.Tracks.IndexOf(target) - 1), () => Exists() && model.Tracks.IndexOf(target) > 0);
                Action("Mover faixa para baixo", () => model.MoveTrack(target, model.Tracks.IndexOf(target) + 1), () => Exists() && model.Tracks.IndexOf(target) < model.Tracks.Count - 1);
                Separator();
                Action("Mostrar no Explorer", () => model.ShowTrackFile(target), Exists, external: true);
                Action("Localizar novamente", () => model.LocateTrackFile(target), Exists);
                Action("Informações", () => ShowBackingTrackDetails(target), Exists);
                Separator();
                Action("Remover da lista", () => model.RemoveTrackItem(target), Exists);
                break;
            case "wave":
                var current = model.SelectedTrack;
                double point = time ?? model.Position;
                bool Current() => current != null && current == model.SelectedTrack && model.CanSeek;
                Action("Definir A", () => model.RangeStart = point, () => Current() && model.Duration >= .1);
                Action("Definir B", () => model.RangeEnd = point, () => Current() && model.Duration >= .1);
                Action("Ir para A", () => model.GoToA.Execute(null), () => Current() && model.GoToA.CanExecute(null));
                Action("Limpar A", () => model.ClearStart.Execute(null), () => Current() && double.IsFinite(model.RangeStart));
                Action("Limpar B", () => model.ClearEnd.Execute(null), () => Current() && double.IsFinite(model.RangeEnd));
                Action("Limpar A/B", () => model.ResetRange.Execute(null), () => Current() && model.ResetRange.CanExecute(null));
                Separator();
                Action("Adicionar seção", () => AddBackingSection(current!, point), Current);
                Action(model.ExportLabel, () => model.ExportRange.Execute(null), () => Current() && model.ExportRange.CanExecute(null));
                break;
            case "section":
                if (section == null || model.SelectedTrack == null) return;
                var sectionTrack = model.SelectedTrack; var marker = section;
                bool HasSection() => model.SelectedTrack == sectionTrack && sectionTrack.Sections.Contains(marker);
                Action("Ir para seção", () => model.Position = marker.Position, HasSection);
                Action("Remover seção", () => { sectionTrack.Sections.Remove(marker); if (model.SelectedSection == marker) model.SelectedSection = null; }, HasSection);
                break;
            case "speed":
                var speedEditor = new TextBox { Width = 96, IsEnabled = model.HasTracks };
                speedEditor.SetBinding(TextBox.TextProperty, new System.Windows.Data.Binding(nameof(model.Speed))
                { Source = model, Mode = System.Windows.Data.BindingMode.TwoWay, UpdateSourceTrigger = System.Windows.Data.UpdateSourceTrigger.LostFocus, StringFormat = "0" });
                speedEditor.KeyDown += BackingNumberKey;
                System.Windows.Automation.AutomationProperties.SetName(speedEditor, LanguageService.T("Velocidade"));
                var speedFields = new StackPanel { Orientation = Orientation.Horizontal };
                speedFields.Children.Add(speedEditor);
                speedFields.Children.Add(new TextBlock { Text = "%", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(8, 0, 0, 0) });
                menu.Items.Add(new MenuItem { Header = speedFields, StaysOpenOnClick = true });
                Separator();
                foreach (int rate in new[] { 50, 75, 90, 100 })
                {
                    var item = new MenuItem { Header = rate + "%", IsCheckable = true, IsChecked = model.Speed == rate, Command = new UiCommand(() => model.Speed = rate, () => model.SelectedTrack != null) };
                    menu.Items.Add(item);
                }
                Separator(); Command("Restaurar velocidade", model.ResetSpeed);
                break;
            case "audio":
                Command("Atualizar saídas", model.RefreshOutputs); Command("Atualizar entradas", model.RefreshInputs);
                Command("Bluetooth", model.BluetoothSettings, external: true); break;
            case "pitch":
                Command("Descer um semitom", model.LowerPitch);
                ((MenuItem)menu.Items[^1]).Icon = Cube.Desktop.Appearance.AppIcons.Create(Wpf.Ui.Controls.SymbolRegular.Subtract24);
                ((MenuItem)menu.Items[^1]).StaysOpenOnClick = true;
                Command("Subir um semitom", model.RaisePitch);
                ((MenuItem)menu.Items[^1]).Icon = Cube.Desktop.Appearance.AppIcons.Create(Wpf.Ui.Controls.SymbolRegular.Add24);
                ((MenuItem)menu.Items[^1]).StaysOpenOnClick = true;
                Separator();
                Command("Restaurar tom", model.ResetPitch);
                break;
            case "player":
                var sequence = new MenuItem { Header = LanguageService.T("Continuar sequência"), IsCheckable = true, IsChecked = model.ContinueSequence };
                sequence.Click += (_, _) => model.ContinueSequence = sequence.IsChecked; menu.Items.Add(sequence);
                Action("Atalhos do player", ShowBackingShortcuts);
                break;
            default: return;
        }
        backingOpenMenu = menu;
        menu.Closed += (_, _) => { if (backingOpenMenu == menu) backingOpenMenu = null; };
        menu.IsOpen = true;
    }
    private bool OpenBackingKeyboardMenu(DependencyObject? source)
    {
        var original = source as FrameworkElement;
        for (var node = source; node != null; node = BackingParent(node))
        {
            if (node is Controls.BackingWaveform wave) { OpenBackingMenu(wave, "wave"); return true; }
            if (node is FrameworkElement owner && owner.Tag is string kind && kind is "track" or "wave" or "section" or "speed" or "pitch" or "player" or "audio" or "metronome")
            {
                var item = source == null ? null : ItemsControl.ContainerFromElement(BackingTrackList, source) as ListBoxItem;
                OpenBackingMenu(owner, kind, item?.DataContext as BackingTrack ?? original?.DataContext as BackingTrack,
                    original?.DataContext as BackingSection ?? owner.DataContext as BackingSection);
                return true;
            }
        }
        return false;
    }
    private async void EditBackingTrack(BackingTrack track)
    {
        if (backingDialogOpen) return;
        backingDialogOpen = true;
        try
        {
            var fields = new StackPanel { MinWidth = 240, MaxWidth = 440 };
            fields.Children.Add(new TextBlock { Text = track.Title, ToolTip = track.Title, TextTrimming = TextTrimming.CharacterEllipsis });
            void Field(string label, FrameworkElement control)
            {
                fields.Children.Add(new TextBlock { Text = LanguageService.T(label), Margin = new Thickness(0, 12, 0, 4) }); fields.Children.Add(control);
            }
            var bpm = new TextBox { Text = track.Bpm > 0 ? track.Bpm.ToString("0.#", CultureInfo.CurrentCulture) : "" };
            var key = new ComboBox { ItemsSource = BackingTracks.Keys.Select(k => new KeyValuePair<string, string>(k, LanguageService.T(k))).ToArray(), DisplayMemberPath = "Value", SelectedValuePath = "Key", SelectedValue = track.OriginalKey };
            var tuning = new Wpf.Ui.Controls.TextBox { Text = track.Tuning, MaxLength = 40,
                PlaceholderText = LanguageService.T("Sem afinação associada"),
                ToolTip = LanguageService.T("Deixe vazio para não associar uma afinação.") };
            var preset = new ComboBox { ItemsSource = BackingTracks.PresetOptions.Select(LanguageService.T).ToArray(), SelectedIndex = track.Preset };
            System.Windows.Automation.AutomationProperties.SetName(bpm, LanguageService.T("BPM original"));
            System.Windows.Automation.AutomationProperties.SetName(key, LanguageService.T("Tom original"));
            System.Windows.Automation.AutomationProperties.SetName(tuning, LanguageService.T("Afinação"));
            System.Windows.Automation.AutomationProperties.SetName(preset, LanguageService.T("Preset associado"));
            Field("BPM original (opcional, 20–300)", bpm); Field("Tom original", key); Field("Afinação", tuning); Field("Preset associado", preset);
            fields.Children.Add(new TextBlock { Text = LanguageService.T("Ao tocar, o app lembra o preset associado. A troca física A/B/C deve ser feita no footswitch."), TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 12, 0, 0) });
            var dialog = new Wpf.Ui.Controls.ContentDialog(DialogHost) { Title = LanguageService.T("Corrigir dados da faixa"), Content = fields, PrimaryButtonText = LanguageService.T("Salvar"), CloseButtonText = LanguageService.T("Cancelar") };
            dialog.Loaded += (_, _) => dialog.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background,
                new Action(() => (dialog.Template.FindName("PART_ContentScroll", dialog) as ScrollViewer)?.ScrollToTop()));
            bool Valid() => string.IsNullOrWhiteSpace(bpm.Text) || double.TryParse(bpm.Text, NumberStyles.Float, CultureInfo.CurrentCulture, out var value) && double.IsFinite(value) && value >= 20 && value <= 300;
            bpm.TextChanged += (_, _) => dialog.IsPrimaryButtonEnabled = Valid(); dialog.IsPrimaryButtonEnabled = Valid();
            if (await dialog.ShowLocalizedAsync() == Wpf.Ui.Controls.ContentDialogResult.Primary && Valid() && key.SelectedValue is string chosen)
                BackingTracks.UpdateTrackMetadata(track, string.IsNullOrWhiteSpace(bpm.Text) ? 0 : double.Parse(bpm.Text, CultureInfo.CurrentCulture), chosen, tuning.Text, preset.SelectedIndex);
        }
        finally { backingDialogOpen = false; }
    }
    private async void AddBackingSection(BackingTrack track, double position)
    {
        if (backingDialogOpen) return;
        backingDialogOpen = true;
        try
        {
            var name = new TextBox { Text = "Intro", MaxLength = 40, MinWidth = 240 };
            System.Windows.Automation.AutomationProperties.SetName(name, LanguageService.T("Nome da seção"));
            var dialog = new Wpf.Ui.Controls.ContentDialog(DialogHost) { Title = LanguageService.T("Adicionar seção") + " · " + BackingTracksViewModel.FormatTime(position, true), Content = name, PrimaryButtonText = LanguageService.T("Adicionar"), CloseButtonText = LanguageService.T("Cancelar") };
            name.TextChanged += (_, _) => dialog.IsPrimaryButtonEnabled = !string.IsNullOrWhiteSpace(name.Text);
            if (await dialog.ShowLocalizedAsync() == Wpf.Ui.Controls.ContentDialogResult.Primary) BackingTracks.AddSectionAt(track, position, name.Text);
        }
        finally { backingDialogOpen = false; }
    }
    private async void ShowBackingTrackDetails(BackingTrack track)
    {
        if (backingDialogOpen) return;
        backingDialogOpen = true;
        try
        {
            string message;
            try { message = await Task.Run(() => { using var decoder = new Services.BackingDecoder(track.Source); return $"{track.Title}\n{System.IO.Path.GetExtension(track.Source).TrimStart('.').ToUpperInvariant()} · {decoder.Format.SampleRate} Hz · {decoder.Format.Channels} ch\n{BackingTracksViewModel.FormatTime(decoder.Duration)}\n{track.Source}"; }); }
            catch { message = LanguageService.T("Não foi possível ler o arquivo. Use Localizar novamente."); }
            await new Wpf.Ui.Controls.ContentDialog(DialogHost) { Title = LanguageService.T("Informações"), Content = new TextBlock { Text = message, TextWrapping = TextWrapping.Wrap, MaxWidth = 520 }, CloseButtonText = LanguageService.T("Fechar") }.ShowLocalizedAsync();
        }
        finally { backingDialogOpen = false; }
    }
    private async void ImportMetronomeWav()
    {
        var picker = new Microsoft.Win32.OpenFileDialog { Filter = "WAV (*.wav)|*.wav", Title = LanguageService.T("Importar som WAV") };
        if (picker.ShowDialog(this) == true) await BackingTracks.ImportMetronomeSound(picker.FileName);
    }
    private void BackingMetronomeBpmKey(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter) return;
        e.Handled = true; EditMetronomeNumber(false);
    }
    private async void EditMetronomeNumber(bool offset)
    {
        if (backingDialogOpen) return;
        backingDialogOpen = true;
        try
        {
            string title = offset ? "Primeiro tempo (s)" : "BPM do metrônomo";
            var input = new TextBox { Text = (offset ? BackingTracks.FirstBeatOffset : BackingTracks.MetronomeBpm).ToString("0.###", CultureInfo.CurrentCulture), MinWidth = 240 };
            System.Windows.Automation.AutomationProperties.SetName(input, LanguageService.T(title));
            var dialog = new Wpf.Ui.Controls.ContentDialog(DialogHost) { Title = LanguageService.T(title), Content = input, PrimaryButtonText = LanguageService.T("Salvar"), CloseButtonText = LanguageService.T("Cancelar") };
            bool Valid() => double.TryParse(input.Text, NumberStyles.Float, CultureInfo.CurrentCulture, out double value) && double.IsFinite(value) && value >= (offset ? 0 : 20) && value <= (offset ? BackingTracks.Duration : 300);
            input.TextChanged += (_, _) => dialog.IsPrimaryButtonEnabled = Valid(); dialog.IsPrimaryButtonEnabled = Valid();
            dialog.Loaded += (_, _) => { input.Focus(); input.SelectAll(); };
            if (await dialog.ShowLocalizedAsync() == Wpf.Ui.Controls.ContentDialogResult.Primary && Valid())
            {
                double value = double.Parse(input.Text, CultureInfo.CurrentCulture);
                if (offset) BackingTracks.FirstBeatOffset = value; else BackingTracks.MetronomeBpm = value;
            }
        }
        finally { backingDialogOpen = false; }
    }
    private async void ShowBackingShortcuts()
    {
        if (backingDialogOpen) return;
        backingDialogOpen = true;
        try
        {
            await new Wpf.Ui.Controls.ContentDialog(DialogHost)
            {
                Title = LanguageService.T("Atalhos do player"),
                Content = new TextBlock { Text = LanguageService.T("Espaço: reproduzir/pausar\n← / →: voltar/avançar 5 s (Shift: 10 s)\nBotão direito na waveform: definir A/B · L: loop\n↑ / ↓: velocidade · Home: início ou A\nRoda na waveform: ±5 s (Shift: 1 s) · Faders: ±1%\nPlaylist: Enter reproduz · Delete remove · Alt+↑/↓ reordena\nShift+F10 ou tecla de menu: opções do elemento"), TextWrapping = TextWrapping.Wrap, MaxWidth = 480 },
                CloseButtonText = LanguageService.T("Fechar")
            }.ShowLocalizedAsync();
        }
        finally { backingDialogOpen = false; }
    }
}
