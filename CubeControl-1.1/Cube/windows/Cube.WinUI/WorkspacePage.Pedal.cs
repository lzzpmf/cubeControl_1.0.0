using Cube.WinUI.Controls;
using Cube.Desktop.ViewModels;
using Cube.Protocol;
using Cube.WinUI.Platform;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;

namespace Cube.WinUI;
public sealed partial class WorkspacePage
{
    private void BuildEffects()
    {
        void Effect(string title, object model, Func<Task> read, Func<Task> end, Func<Task> import, Func<Task> export, Action apply, params (string Path, string Label, double Min, double Max)[] fields)
        {
            async Task Load(Func<Task> operation)
            {
                double[] before = fields.Select(field => Convert.ToDouble(model.GetType().GetProperty(field.Path)!.GetValue(model))).ToArray();
                bool loaded = (bool?)model.GetType().GetProperty("Loaded")?.GetValue(model) ?? false;
                await operation();
                bool changed = fields.Where((field, i) => Math.Abs(Convert.ToDouble(model.GetType().GetProperty(field.Path)!.GetValue(model)) - before[i]) > 1e-10).Any();
                if (changed || (!loaded && (bool?)model.GetType().GetProperty("Loaded")?.GetValue(model) == true))
                    foreach (var field in fields) workspace.CaptureReference(model, field.Path);
            }
            var panel = Section(title, Buttons(Action("Ler e iniciar prévia", () => Load(read), E, "CanStart" + title), Action("Encerrar prévia", end, E, "CanRestore" + title), Action("Importar ajustes", () => Load(import), E, "CanUse" + title + "Files"), Action("Exportar ajustes", export, E, "CanUse" + title + "Files")), Text(model, "Message"));
            var edits = new SectionContent(); Bind(edits, IsEnabledProperty, E, "CanEdit" + title);
            var rack = Rack();
            foreach (var f in fields) rack.Children.Add(Instrument(model, f.Path, f.Label, f.Min, f.Max, f.Path is "Waveform" or "Sections" or "TimeSamples" or "Decay" or "Damping" ? 1 : .01));
            edits.Children.Add(rack);
            panel.Children.Add(edits); panel.Children.Add(Action("Aplicar ajustes", () => { apply(); return Task.CompletedTask; }, E, title == "Phaser" ? "CanRestorePhaser" : "CanApply" + title));
        }
        Effect("Chorus", E.Chorus, E.ReadChorusAsync, E.EndChorusPreviewAsync, E.ImportChorusAsync, E.ExportChorusAsync, E.ApplyChorusEdits,
            ("Depth", "Profundidade (%)", 0, 100), ("Speed", "Velocidade (Hz)", .384, 3.024), ("Mix", "Mix (%)", 0, 100), ("Feedback", "Feedback (%)", 0, 10), ("Waveform", "Forma de onda (0/1)", 0, 1), ("WetGain", "Ganho do efeito (%)", 50, 100), ("CombinedGain", "Ganho combinado (%)", 50, 100));
        Effect("Phaser", E.Phaser, E.ReadPhaserAsync, E.EndPhaserPreviewAsync, E.ImportPhaserAsync, E.ExportPhaserAsync, E.ApplyPhaserEdits,
            ("Speed", "Velocidade (Hz)", .48, 6.24), ("Lower", "Frequência mínima (Hz)", 300, 3000), ("Upper", "Frequência máxima (Hz)", 300, 3000), ("Level", "Nível (%)", 0, 100), ("Sections", "Estágios", 1, 3));
        Effect("Delay", E.Delay, E.ReadDelayAsync, E.EndDelayPreviewAsync, E.ImportDelayAsync, E.ExportDelayAsync, E.ApplyDelayEdits,
            ("TimeSamples", "Tempo (amostras)", 2400, 16350), ("Feedback", "Feedback (%)", 0, 99.21875), ("Depth", "Profundidade (%)", 0, 100), ("Rate", "Taxa (%)", 0, 100), ("Mix", "Mix (%)", 0, 92.1875));
        Effect("Reverb", E.Reverb, E.ReadReverbAsync, E.EndReverbPreviewAsync, E.ImportReverbAsync, E.ExportReverbAsync, E.ApplyReverbEdits,
            ("Dry", "Sinal direto (%)", 0, 100), ("Wet", "Efeito (%)", 0, 90), ("Decay", "Decaimento", 1, 15), ("ReturnLevel", "Retorno (%)", 0, 100), ("Damping", "Amortecimento", 0, 10));
    }
    private void BuildIrs()
    {
        var library = Library(E.IrLibrary, "VisibleEntries", "Selected", "Resposta de impulso", "Name");
        var details = new StackPanel { Spacing = 12 }; details.Children.Add(Text(E.IrLibrary, "SelectedDetails")); details.Children.Add(Command(E, "ChooseWav", "Abrir arquivo WAV"));
        var browser = new WorkbenchPanel { CellWidth = 260, MaximumColumns = 2, Gap = 16 }; browser.Children.Add(library); browser.Children.Add(details);
        Section("Biblioteca de IRs", Edit(E.IrLibrary, "Search", "Buscar IR"), browser);
        Section("Slot e prévia", Choice(E.Ir, "Slots", "Slot", "Slot do pedal", index: true), Number(E.Ir, "Distance", "Distância", 0, 99), Text(E.Ir, "Information"), Text(E.Ir, "State"), Text(E.Ir, "SlotInfo"), Buttons(Command(E, "ReadSlot", "Ler slot"), Command(E, "PreviewIr", "Ouvir no pedal"), Command(E, "EndPreview", "Encerrar prévia"), Command(E, "WriteIr", "Gravar IR no pedal"), Command(E, "RestoreIr", "Restaurar IR")));
        var a = E.IrAudition;
        Section("Audição com guitarra", Choice(a, "Tracks", "SelectedTrack", "Gravação", "Name"), Choice(a, "Outputs", "SelectedOutput", "Saída de áudio", "Name"), Number(a, "Volume", "Volume (%)", 0, 100), Check(a, "Loop", "Repetir"), Buttons(Command(a, "PlayPause", "Reproduzir / pausar"), Command(a, "Stop", "Parar"), Command(a, "RefreshOutputs", "Atualizar saídas")), Text(a, "ContextStatus"));
        var pack = E.PackEditor;
        var catalog = new ComboBox { Header = T("Pack"), DisplayMemberPath = "Name", HorizontalAlignment = HorizontalAlignment.Stretch };
        Bind(catalog, ItemsControl.ItemsSourceProperty, E, "IrPacks"); catalog.SelectedItem = pack.SelectedCatalogPack;
        bool selecting = false;
        catalog.SelectionChanged += async (_, _) => { if (selecting || catalog.SelectedItem is not IrPack) return; selecting = true; try { await pack.SelectAsync(catalog.SelectedItem as IrPack); catalog.SelectedItem = pack.SelectedCatalogPack; } finally { selecting = false; } };
        Observe(pack, "SelectedCatalogPack", () => { if (selecting) return; selecting = true; try { catalog.SelectedItem = pack.SelectedCatalogPack; } finally { selecting = false; } });
        var panel = Section("Packs de oito slots", catalog, Edit(pack, "Name", "Nome do pack"), Edit(pack, "Description", "Descrição"), Buttons(Command(pack, "NewPack", "Novo pack"), Command(pack, "DuplicatePack", "Duplicar pack"), Command(pack, "SavePack", "Salvar pack"), Command(pack, "DiscardChanges", "Descartar alterações"), Command(pack, "DeletePack", "Excluir pack"), Command(E, "ApplyIrPack", "Revisar e aplicar no pedal")), Buttons(Action("Importar pack", async () => { var d = new OpenFileDialog { Filter = "Pack CubeControl (*.zip)|*.zip" }; if (d.ShowDialog() == true && await pack.ConfirmDiscard()) pack.Import(d.FileName); }), Action("Exportar pack", () => { var d = new SaveFileDialog { Filter = "Pack CubeControl (*.zip)|*.zip", FileName = pack.Name + ".zip" }; if (d.ShowDialog() == true) pack.Export(d.FileName); })), Edit(pack, "Search", "Buscar IR para o pack"), Choice(pack, "Library", "SelectedIr", "IR para atribuir", "Name"), Text(pack, "Message"), Text(pack, "Problems"));
        foreach (var slot in pack.Slots)
        {
            panel.Children.Add(new TextBlock { Text = slot.Title, FontSize = 20 }); panel.Children.Add(Text(slot, "Name"));
            panel.Children.Add(Number(slot, "DistanceValue", "Distância do " + slot.Title, 0, 99));
            var actions = Buttons(Command(slot, "UseIr", "Usar IR selecionado"), Command(slot, "Remove", "Remover"), Command(slot, "MoveUp", "Mover acima"), Command(slot, "MoveDown", "Mover abaixo"));
            foreach (var action in actions.Children) AutomationProperties.SetName(action, AutomationProperties.GetName(action) + " · " + slot.Title);
            panel.Children.Add(actions);
        }
    }
    private void BuildAmps()
    {
        var bases = new ComboBox { Header = T("Modelo de partida"), ItemsSource = EquipmentCatalog.Amps.Select(a => a.Name).Concat(AmpEqPresets.All.Select(a => a.Description)).ToArray(), SelectedIndex = 0, HorizontalAlignment = HorizontalAlignment.Stretch };
        var load = Action("Carregar modelo local", async () =>
        {
            if (bases.SelectedIndex < 0 || !E.CanEditAmpLocally) return;
            if (E.Amp.HasLocalChanges && !await E.Ask("Carregar o modelo e descartar os ajustes locais do amp?")) return;
            int index = bases.SelectedIndex;
            if (index >= EquipmentCatalog.Amps.Count) { var preset = AmpEqPresets.All[index - EquipmentCatalog.Amps.Count]; E.Amp.Load(preset.Build(), preset.Name); }
            else
            {
                using var stream = typeof(AmpViewModel).Assembly.GetManifestResourceStream("Cube.FactoryAmps")!;
                stream.Position = index * AmpImage.Size; var bytes = new byte[AmpImage.Size]; stream.ReadExactly(bytes); E.Amp.Load(bytes, EquipmentCatalog.Amps[index].Name); E.Amp.Slot = index;
            }
        }, E, "CanEditAmpLocally");
        var operations = Buttons(Action("Ler do pedal", async () => { await E.ReadAmpAsync(); }, E, "CanUseAmp"), Action("Importar amp", async () => { await E.ImportAmpAsync(); }, E, "CanEditAmpLocally"), Action("Exportar amp", async () => { await E.ExportAmpAsync(); }, E, "CanAdjustAmp"), Action("Ouvir no pedal", async () => { await E.PreviewAmpAsync(); }, E, "CanSaveAmp"), Command(E, "EndAmpPreview", "Encerrar prévia"), Action("Gravar amp no pedal", async () => { await E.WriteAmpAsync(); }, E, "CanSaveAmp"));
        operations.MaximumColumns = 6; operations.CellWidth = 130;
        Section("Amplificador", Rack(bases, load, Choice(E.Amp, "Slots", "Slot", "Slot do amp", index: true)), Rack(Text(E.Amp, "Name"), Text(E.Amp, "LocalState"), Text(E, "AmpPreviewMessage")), operations);
        foreach (var (path, label) in new[] { ("MainControls", "Controles principais"), ("EqAControls", "Equalização A"), ("EqBControls", "Equalização B"), ("CurveControls", "Curva e acoplamento"), ("CoefficientAControls", "Coeficientes A"), ("CoefficientBControls", "Coeficientes B") })
        {
            var rack = new Views.AmplifierRackView(E.Amp, path, control =>
            {
                    var group = new StackPanel { Spacing = 6 };
                    var field = Instrument(control, "Position", control.Label, 0, 1000, 1, path.StartsWith("Eq"), control.Restore);
                    field.Instrument.FineAdjustment = true;
                    field.Instrument.WheelSpeedMultiplier = path.StartsWith("Eq") ? 11.5 : 10;
                    field.Editor.Visibility = Visibility.Collapsed;
                    group.Children.Add(field); group.Children.Add(Text(control, "RangeText"));
                    var number = new NumberBox { Minimum = control.Minimum, Maximum = control.Maximum, Header = T("Valor exato de ") + control.Label };
                    Bind(number, NumberBox.ValueProperty, control, "Number");
                    number.ValueChanged += (_, args) => { if (double.IsFinite(args.NewValue) && Math.Abs(args.NewValue - control.Number) > 1e-10) control.SetNumber(args.NewValue); };
                    group.Children.Add(number); group.Children.Add(Action("Restaurar " + control.Label, control.Restore));
                    return group;
            });
            var panel = Section(label, rack); Bind(panel, IsEnabledProperty, E, "CanAdjustAmp");
        }
        foreach (var (path, label) in new[] { ("Gain", "Tabela de ganho"), ("Compensation", "Tabela de compensação"), ("Curve", "Tabela da curva"), ("CoefficientsA", "Tabela de coeficientes A"), ("CoefficientsB", "Tabela de coeficientes B") })
        {
            var rows = new StackPanel { Spacing = 8 };
            var expander = new Expander { Header = label, HorizontalAlignment = HorizontalAlignment.Stretch, HorizontalContentAlignment = HorizontalAlignment.Stretch, Content = new ScrollViewer { Content = rows, MaxHeight = 320 } };
            Bind(expander, IsEnabledProperty, E, "CanAdjustAmp");
            void RenderRaw()
            {
                rows.Children.Clear(); if (!expander.IsExpanded) return;
                foreach (var value in (AmpValue[])typeof(AmpViewModel).GetProperty(path)!.GetValue(E.Amp)!)
                    rows.Children.Add(Edit(value, "Value", $"Linha {value.Index} · referência {value.Reference}"));
            }
            expander.Expanding += (_, _) => RenderRaw(); Observe(E.Amp, path, RenderRaw); Sections.Children.Add(expander);
        }
    }
}
