#if WINUI
using SharedImageSource = Microsoft.UI.Xaml.Media.ImageSource;
#else
using SharedImageSource = System.Windows.Media.ImageSource;
#endif
﻿using Cube.Desktop.Localization;
using System.IO;
using System.Windows.Input;
using Cube.Protocol;

namespace Cube.Desktop.ViewModels;

public sealed partial class EditorViewModel
{
    private readonly CabinetInventory inventory = new();
    private readonly EditHistory history = new();
    private readonly Dictionary<ParameterKey, long> revisions = [];
    private long revision;
    private object? gesture;
    private readonly Dictionary<(int Preset, int Stage), DialViewModel[]> dialGroups = [];
    private sealed class PendingMatch
    {
        public bool Started;
        public Dictionary<ParameterKey, (int Before, long Version)> Changes { get; } = [];
    }
    private readonly Dictionary<int, PendingMatch> pendingMatches = [];
    private VisualPreferences preferences = new();
    private string ImagePreferencesPath => Path.Combine(DataDirectory, "gear-images-v1.json");
    private int stage;
    public StageViewModel[] Stages { get; } =
    [new(0, "Amp", "Caráter e ganho", "#D5AD7D"), new(1, "Chorus", "Chorus", "#80B8A8"), new(2, "Phaser", "Phaser", "#D9A071"), new(3, "Delay", "Delay", "#82ACD2"), new(4, "Reverb", "Reverb", "#B49ACF"), new(5, "Cabinet", "Gabinete / IR", "#C5B77F")];
    public int SelectedStage { get => stage; set { if (value >= 0 && value < Stages.Length && Set(ref stage, value)) RefreshEditor(); } }
    public StageViewModel Stage => Stages[stage];
    public bool IsAmp => stage == 0;
    public bool IsModulation => stage is 1 or 2;
    public bool IsDelay => stage == 3;
    public bool IsReverb => stage == 4;
    public bool IsCabinet => stage == 5;
    public string EquipmentImageLabel => HasGallery ? GalleryLabel : "Personalizar imagem";
    public bool HasGallery => IsAmp || IsCabinet;
    public GearItem[] Amps { get; private set; } = [];
    public GearItem[] Cabinets { get; private set; } = [];
    public GearItem[] Effects { get; private set; } = [];
    public GearItem SelectedAmp => Amps[(int)Current.Parameters[0].Value];
    public GearItem SelectedCabinet => Cabinets.FirstOrDefault(c => c.Value == (int)Current.Parameters[9].Value) ?? Cabinets[0];
    public GearItem FocusedGear => stage switch { 0 => SelectedAmp, 1 => Effects[0], 2 => Effects[1], 3 => Effects[2], 4 => Effects[3], _ => SelectedCabinet };
    public DialViewModel[] EditorDials
    {
        get
        {
            var key = (SelectedIndex, stage);
            if (!dialGroups.TryGetValue(key, out var dials)) dialGroups[key] = dials = stage switch
            { 0 => [Current.Dials[1], Current.Dials[2]], 1 => [Current.Musical.ChorusDial], 2 => [Current.Musical.PhaserDial], 3 => [Current.Dials[6], Current.Dials[4], Current.Musical.DelayMixDial], 4 => [Current.Musical.ReverbDial], _ => [] };
            return dials;
        }
    }
    public string EffectHint => stage switch
    {
        0 => "Ganho e brilho do amplificador. Escolha um modelo para mudar o caráter do timbre.",
        1 or 2 => "Chorus e Phaser compartilham o mesmo bloco. Ligar um desliga o outro. Ajustar o efeito desligado prepara sua próxima ativação.",
        3 => !Current.Musical.DelayEnabled ? "Seção delay desligada. Seus ajustes são preservados." : Current.Parameters[7].Value == 0 ? "Sem sinal de delay: aumente a mistura para ouvir as repetições." : "Ajuste o espaço entre as repetições, a realimentação e a mistura.",
        4 => Current.Musical.ReverbEnabled ? "Misture a ambiência com o sinal direto." : "Desligado. Ajustar a mistura prepara a próxima ativação.",
        _ => Current.Parameters[10].Value == 0 ? "Seção IR desligada. Escolher um gabinete não liga a seção." : Current.Parameters[9].Value == 0 ? "Nenhum cabinet selecionado." : "O nome e a imagem acompanham o conteúdo identificado do IR."
    };
    public string InventoryStatus => inventory.Complete ? $"8 slots lidos · {inventory.Slots.Count(c => c.CabinetId != null)} IRs de packs reconhecidos" : "Inventário de IRs não verificado";
    public bool CanMatch => CanEdit && inventory.Complete;
    public bool AutomaticCabinetMatching
    {
        get => preferences.AutomaticCabinetMatching;
        set
        {
            if (preferences.AutomaticCabinetMatching == value) return;
            preferences.AutomaticCabinetMatching = value;
            Notify(nameof(AutomaticCabinetMatching));
            if (value && CanMatch) QueueAmpMatch(null);
        }
    }
    public string MatchDescription
    {
        get
        {
            if (!inventory.Complete) return "Conecte e identifique os IRs para combinar um cabinet.";
            preferences.Matches.TryGetValue((int)Current.Parameters[0].Value, out var reference);
            var match = inventory.Resolve(reference);
            return match != null ? $"Combinação: {match.Name} · slot {match.Slot + 1}" : reference == null ? "Sem associação. Configure uma combinação para este amp." : "Associação suspensa: o IR não foi encontrado. Atualize ou reconfigure.";
        }
    }
    public ICommand Undo { get; private set; } = null!;
    public ICommand MatchCabinet { get; private set; } = null!;
    public ICommand RefreshInventory { get; private set; } = null!;
    public ICommand ChangeImage { get; private set; } = null!;
    public ICommand ResetImage { get; private set; } = null!;
    private object? gestureToken;
    public void BeginGesture(object token) { if (!ReferenceEquals(gestureToken, token)) { gestureToken = token; gesture = new object(); } }
    public void RefreshThemeColors()
    {
        foreach (var item in Stages) item.RefreshColor();
        foreach (var item in Amps.Concat(Cabinets).Concat(Effects)) item.RefreshColor();
    }
    public void EndGesture(object token) { if (ReferenceEquals(gestureToken, token)) { gestureToken = null; gesture = null; } }

    private void EditSwitch(Action edit)
    {
        var previous = gesture; gesture = new object();
        try { edit(); } finally { gesture = previous; }
    }

    private void InitializeVisual()
    {
        Stages[0].BindToggle(() => Current.Parameters[12].Enabled, v => Current.Parameters[12].Enabled = v, () => CanEdit);
        Stages[1].BindToggle(() => Current.Musical.ChorusEnabled, v => EditSwitch(() => Current.Musical.ChorusEnabled = v), () => CanEdit);
        Stages[2].BindToggle(() => Current.Musical.PhaserEnabled, v => EditSwitch(() => Current.Musical.PhaserEnabled = v), () => CanEdit);
        Stages[3].BindToggle(() => Current.Musical.DelayEnabled, v => EditSwitch(() => Current.Musical.DelayEnabled = v), () => CanEdit);
        Stages[4].BindToggle(() => Current.Musical.ReverbEnabled, v => Current.Musical.ReverbEnabled = v, () => CanEdit);
        Stages[5].BindToggle(() => Current.Parameters[10].Enabled, v => Current.Parameters[10].Enabled = v, () => CanEdit);
        try { preferences.Images = GearImagePreferences.Load(ImagePreferencesPath, Path.Combine(DataDirectory, "visual-preferences-v1.json")); }
        catch (Exception ex) { ErrorMessage = Status = "Preferências não carregadas: " + ex.Message; }
        Amps = EquipmentCatalog.Amps.Select((reference, i) => WithImage(new GearItem($"amp-{i}", i,
            reference.Name, reference.Category, "amp", "#D5AD7D", reference.Description, reference.Alias))).ToArray();
        Effects = [WithImage(new("chorus", 0, "Chorus", "Modulação", "chorus", "#80B8A8")), WithImage(new("phaser", 1, "Phaser", "Modulação", "phaser", "#D9A071")), WithImage(new("delay", 2, "Delay", "Repetições", "delay", "#82ACD2")), WithImage(new("reverb", 3, "Reverb", "Ambiência", "reverb", "#B49ACF"))];
        RebuildCabinets();
        Undo = Command(UndoAsync, () => CanEdit && history.CanUndo, changesEffects: true);
        MatchCabinet = new UiCommand(() => QueueAmpMatch(null), () => CanMatch);
        RefreshInventory = Command(RefreshInventoryAsync, () => CanOperate);
        ChangeImage = new UiCommand(() => ImageSelectionRequested?.Invoke(FocusedGear), () => !Busy && (!IsCabinet || inventory.Complete));
        ResetImage = new UiCommand(() => { preferences.Images.Remove(FocusedGear.Id); FocusedGear.Image = FocusedGear.DefaultImage; SaveImages(); }, () => !Busy);
        foreach (var preset in Presets)
        {
            preset.Musical.PropertyChanged += (_, _) => RefreshEditor();
            foreach (var parameter in preset.Parameters) parameter.PropertyChanged += (_, e) => { if (e.PropertyName == nameof(ParameterViewModel.Value)) RefreshEditor(); };
        }
    }
    private GearItem WithImage(GearItem item)
    {
        if (preferences.Images.TryGetValue(item.Id, out var path)) item.Image = GearItem.ReadImage(Path.Combine(DataDirectory, "Images", Path.GetFileName(path)));
        item.Image ??= item.DefaultImage;
        return item;
    }
    private bool SaveImages()
    {
        try
        {
            GearImagePreferences.Save(ImagePreferencesPath, preferences.Images);
            if (ErrorMessage.StartsWith("Não foi possível salvar as preferências locais:", StringComparison.Ordinal)) ErrorMessage = "";
            return true;
        }
        catch (Exception ex) { ErrorMessage = Status = "Não foi possível salvar as preferências locais: " + ex.Message; NotifyFailure(Status); return false; }
    }
    private void RefreshEditor()
    {
        Notify(nameof(RandomIrDestination));
        RefreshEffectCard();
        if (Amps.Length == 0 || Cabinets.Length == 0 || Presets == null) return;
        Notify(nameof(DelayGraphState)); Notify(nameof(ChorusGraphState));
        Notify(nameof(PhaserGraphState)); Notify(nameof(ReverbGraphState));
        Notify(nameof(GalleryLabel)); Notify(nameof(EquipmentImageLabel));
        Notify(nameof(ModulationLabel)); Notify(nameof(DelayLabel)); Notify(nameof(ReverbLabel)); Notify(nameof(CabinetLabel)); Notify(nameof(ToneLabel));
        foreach (string name in new[] { nameof(Stage), nameof(IsAmp), nameof(IsModulation), nameof(IsDelay), nameof(IsReverb), nameof(IsCabinet), nameof(HasGallery), nameof(CanOpenGearGallery), nameof(FocusedGear), nameof(SelectedAmp), nameof(SelectedCabinet), nameof(EditorDials), nameof(EffectHint), nameof(MatchDescription), nameof(InventoryStatus) }) Notify(name);
        Stages[0].State = SelectedAmp.Name;
        Stages[1].State = Current.Musical.ChorusEnabled ? "Ligado" : "Desligado";
        Stages[2].State = Current.Musical.PhaserEnabled ? "Ligado" : "Desligado";
        Stages[3].State = !Current.Musical.DelayEnabled ? "Desligado" : Current.Parameters[7].Value == 0 ? "Sem sinal de efeito" : "Ligado";
        Stages[4].State = Current.Musical.ReverbEnabled ? "Ligado" : "Desligado";
        Stages[5].State = (Current.Parameters[10].Enabled ? "" : "Desligado · ") + SelectedCabinet.Name;
        if (!HasBank) foreach (var item in Stages) item.State = "Não carregado";
        foreach (var item in Stages) item.RefreshToggle();
        CommandManager.InvalidateRequerySuggested();
    }
    private async Task RefreshInventoryAsync()
    {
        inventory.Clear(); RebuildCabinets(); Status = "Lendo os oito slots de IR…";
        InventoryReadCount = 0; ReadingInventory = true;
        BusyDetail = "Presets carregados. Lendo cabinets: 0/8. Aguarde para editar.";
        bool acceptingProgress = true;
        var progress = new Progress<TransferProgress>(p =>
        {
            if (!acceptingProgress) return;
            InventoryReadCount = p.Done;
            BusyDetail = Status = $"Presets carregados. Lendo cabinets: {p.Done}/{p.Total}. Aguarde para editar.";
        });
        try
        {
            var sectors = await session!.ReadIrInventory(progress);
            BusyDetail = "Identificando os cabinets lidos…";
            for (int i = 0; i < 8; i++) UpdateCabinet(i, sectors[i]);
            InventoryReadCount = 8;
        }
        finally { acceptingProgress = false; ReadingInventory = false; }
        Status = "Inventário lido. Associações disponíveis para os conteúdos reconhecidos."; Refresh();
    }
    private void UpdateCabinet(int slot, byte[] bytes)
    {
        string hash = CabinetInventory.Hash(bytes);
        if (inventory.Slots.FirstOrDefault(c => c.Slot == slot) is { } previous && previous.Hash != hash) soundContentRevision++;
        preferences.IrNames.TryGetValue(hash, out var name);
        if (!string.IsNullOrWhiteSpace(name))
        {
            string clean = name.EndsWith(".wav", StringComparison.OrdinalIgnoreCase) ? Path.GetFileNameWithoutExtension(name) : name;
            if (!resolvedIrNames.TryGetValue(hash, out var resolved))
            {
                var matches = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var entry in IrLibrary.Entries.Where(e => e.Name.Equals(clean, StringComparison.OrdinalIgnoreCase)))
                {
                    try
                    {
                        var audio = IrAudio.Decode(File.ReadAllBytes(entry.FullPath));
                        for (int distance = 0; distance < 100; distance++)
                            if (audio.Sector(distance).AsSpan().SequenceEqual(bytes))
                            { matches.Add(IrViewModel.DisplayName(entry.FullPath, entry.Collection)); break; }
                    }
                    catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or FormatException) { }
                }
                resolved = matches.Count == 1 ? matches.Single() : clean;
                resolvedIrNames[hash] = resolved;
            }
            if (name != resolved)
            {
                name = resolved; preferences.IrNames[hash] = name;
            }
        }
        inventory.Update(slot, bytes, name); RebuildCabinets();
    }
    private readonly Dictionary<string, string> resolvedIrNames = [];
    private void RebuildCabinets()
    {
        var items = new List<GearItem> { WithImage(new("cab-off", 0, "Sem cabinet", "Desligado", "cab", "#C5B77F")) };
        for (int i = 0; i < 8; i++)
        {
            var cab = inventory.Slots.FirstOrDefault(x => x.Slot == i);
            var reference = EquipmentCatalog.FindCabinet(cab?.CabinetId);
            items.Add(WithImage(new(cab?.Hash ?? $"unverified-{i}", i + 1, cab?.Name ?? $"IR personalizado — slot {i + 1}", cab?.Category ?? "Não verificado", "cab", "#C5B77F",
                reference?.Description ?? (cab?.CabinetId?.StartsWith("pack-bfin-", StringComparison.Ordinal) == true
                    ? "Pack BFIN reconhecido pelo conteúdo. O modelo do gabinete ainda não foi confirmado; a imagem é genérica."
                    : "Conteúdo sem referência de equipamento identificada. Nome e imagem personalizados são metadados locais."), reference?.Alias ?? "", cab?.CabinetId)));
        }
        Cabinets = items.ToArray();
        Ir.UpdateSlots(Enumerable.Range(0, 8).Select(i =>
        {
            var cab = inventory.Slots.FirstOrDefault(c => c.Slot == i);
            string name = cab != null && (cab.CabinetId != null || preferences.IrNames.ContainsKey(cab.Hash)) ? cab.Name : "IR não identificado";
            return $"Slot {i + 1} — {name}";
        }));
        Notify(nameof(Cabinets)); Notify(nameof(CanMatch)); RefreshEditor();
    }
    public void ChooseGear(GearItem item)
    {
        if (!CanEdit) return;
        if (item.Kind == "amp") QueueAmpMatch(item.Value);
        else if (item.Kind == "cab") Current.Parameters[9].Value = item.Value;
        else if (item.Kind == "chorus") Current.Musical.ChooseChorus.Execute(null);
        else if (item.Kind == "phaser") Current.Musical.ChoosePhaser.Execute(null);
    }
    private void QueueAmpMatch(int? amp)
    {
        if (!CanEdit) return;
        int slot = SelectedIndex, ampValue = amp ?? (int)Current.Parameters[0].Value;
        bool wantsMatch = !amp.HasValue || AutomaticCabinetMatching;
        preferences.Matches.TryGetValue(ampValue, out var reference);
        var cabinet = wantsMatch ? inventory.Resolve(reference) : null;
        if (!amp.HasValue && cabinet == null) { Status = MatchDescription; return; }
        if (pendingMatches.TryGetValue(slot, out var previous) && !previous.Started)
        {
            if (!amp.HasValue && previous.Changes.ContainsKey(new ParameterKey(slot, 0))) amp = ampValue;
            foreach (var old in previous.Changes)
                if (revisions.GetValueOrDefault(old.Key) == old.Value.Version)
                { edited![slot * 16 + old.Key.Offset] = (byte)old.Value.Before; Presets[slot].Parameters[old.Key.Offset].Load(old.Value.Before); }
        }
        var changes = new List<(ParameterKey Key, int Value)>();
        if (amp.HasValue) changes.Add((new(slot, 0), ampValue));
        if (cabinet != null) changes.Add((new(slot, 9), cabinet.Slot + 1));
        var group = new object(); bankOperationGroup = group; var versions = new Dictionary<ParameterKey, long>(); var intent = new PendingMatch(); pendingMatches[slot] = intent;
        bankOperationDescription = $"Preset {(char)('A' + slot)} · {Amps[ampValue].Name}" + (cabinet == null ? "" : " · " + cabinet.Name);
        foreach (var change in changes)
        {
            int index = change.Key.Slot * 16 + change.Key.Offset;
            history.Record(change.Key, edited![index], group);
            int before = edited[index];
            edited[index] = (byte)change.Value; Presets[slot].Parameters[change.Key.Offset].Load(change.Value);
            versions[change.Key] = revisions[change.Key] = ++revision;
            intent.Changes[change.Key] = (before, revision);
        }
        Presets[slot].State = "Enviando…";
        queue!.EnqueueAction($"amp-match:{slot}", async () =>
        {
            intent.Started = true;
            bool valid = true;
            if (cabinet != null)
            {
                var current = await session!.ReadIrSlot(cabinet.Slot);
                valid = CabinetInventory.Hash(current) == cabinet.Hash;
                if (!valid)
                {
                    UpdateCabinet(cabinet.Slot, current);
                    var key = new ParameterKey(slot, 9);
                    if (revisions.GetValueOrDefault(key) == versions[key])
                    { edited![slot * 16 + 9] = confirmed![slot * 16 + 9]; Presets[slot].Parameters[9].Load(edited[slot * 16 + 9]); }
                    ErrorMessage = Status = "Match suspenso: o conteúdo do slot mudou. Atualize ou reconfigure a associação.";
                }
            }
            int applied = 0;
            try
            {
                foreach (var change in changes)
                {
                    if (change.Key.Offset == 9 && (!valid || revisions.GetValueOrDefault(change.Key) != versions[change.Key])) continue;
                    confirmed = await session!.WriteLiveChecked(slot, change.Key.Offset, change.Value, confirmed); applied++;
                }
            }
            catch (Exception ex) { throw new IOException((applied > 0 ? "Ação aplicada parcialmente. " : "Ação interrompida; aplicação parcial possível. ") + ex.Message, ex); }
            UpdateStates();
            if (valid && cabinet != null) Status = $"Cabinet combinado: {cabinet.Name}." + (Presets[slot].Parameters[10].Value == 0 ? " A seção IR permanece desligada." : "");
            else if (wantsMatch && cabinet == null) Status = "Amp aplicado. Sem associação válida; cabinet preservado.";
        });
        Refresh();
    }
    private async Task UndoAsync()
    {
        foreach (var change in history.Pop())
        {
            edited![change.Key.Slot * 16 + change.Key.Offset] = (byte)change.Value;
            Presets[change.Key.Slot].Parameters[change.Key.Offset].Load(change.Value);
            queue!.Enqueue(change.Key, change.Value);
        }
        await queue!.Flush(); Status = "Ajuste desfeito. Salvar continua sendo uma ação separada.";
        bankOperationGroup = new object(); RecordBankOperation(Status); Refresh();
    }
    public MatchRow[] CreateMatchRows() => Amps.Select(amp =>
    {
        preferences.Matches.TryGetValue(amp.Value, out var reference); var cab = inventory.Resolve(reference);
        return new MatchRow(amp, Cabinets, Cabinets.FirstOrDefault(c => c.Value == (cab?.Slot + 1 ?? 0)), reference != null && cab == null);
    }).ToArray();
    public void SaveMatches(IEnumerable<MatchRow> rows)
    {
        foreach (var row in rows)
        {
            if (row.Selected?.Value == -1) continue;
            if (row.Selected == null || row.Selected.Value == 0) preferences.Matches.Remove(row.Amp.Value);
            else { var cab = inventory.Slots.FirstOrDefault(c => c.Slot == row.Selected.Value - 1); if (cab != null) preferences.Matches[row.Amp.Value] = cab.Hash; }
        }
        RefreshEditor();
        Status = "Associações mantidas nesta sessão. Nenhum comando enviado."; Completed(Status);
    }
    public event Action<GearItem>? ImageSelectionRequested;

    public bool ApplyGearImage(GearItem target, string path)
    {
        string? destination = null;
        try
        {
            var image = ValidateGearImage(path);
            string name = Guid.NewGuid().ToString("N") + Path.GetExtension(path).ToLowerInvariant();
            Directory.CreateDirectory(Path.Combine(DataDirectory, "Images"));
            destination = Path.Combine(DataDirectory, "Images", name);
            File.Copy(path, destination);
            var updated = new Dictionary<string, string>(preferences.Images) { [target.Id] = name };
            GearImagePreferences.Save(ImagePreferencesPath, updated);
            preferences.Images = updated;
            target.Image = image;
            if (ErrorMessage.StartsWith("Não foi possível importar a imagem:", StringComparison.Ordinal)) ErrorMessage = "";
            Status = "Imagem local atualizada."; Completed(Status);
            return true;
        }
        catch (Exception ex)
        {
            if (destination != null) { try { File.Delete(destination); } catch (IOException) { } catch (UnauthorizedAccessException) { } }
            ErrorMessage = Status = "Não foi possível importar a imagem: " + ex.Message; NotifyFailure(Status);
            return false;
        }
    }

    public static SharedImageSource ValidateGearImage(string path)
    {
        if (Path.GetExtension(path).ToLowerInvariant() is not (".png" or ".jpg" or ".jpeg"))
            throw new FormatException("Imagem inválida. Use PNG ou JPEG.");
        if (new FileInfo(path).Length > 20 * 1024 * 1024) throw new FormatException("Imagem maior que 20 MB.");
        return GearItem.ReadImage(path) ?? throw new FormatException("Imagem inválida. Use PNG ou JPEG.");
    }
}
