namespace Cube.Desktop.ViewModels;

public sealed partial class EditorViewModel
{
    private readonly bool[] advancedCards = new bool[5];
    public bool HasAdvancedCard => SelectedStage is >= 1 and <= 4;
    public bool IsAdvancedCard => HasAdvancedCard && advancedCards[SelectedStage];
    public bool IsSimpleCard => !IsAdvancedCard;
    public bool IsAdvancedChorusCard => SelectedStage == 1 && IsAdvancedCard;
    public bool IsAdvancedPhaserCard => SelectedStage == 2 && IsAdvancedCard;
    public bool IsAdvancedDelayCard => SelectedStage == 3 && IsAdvancedCard;
    public bool IsAdvancedReverbCard => SelectedStage == 4 && IsAdvancedCard;
    public string EffectCardAction => IsAdvancedCard ? "Voltar à edição simples" : "Opções avançadas";
    public string EffectCardError => HasAdvancedCard ? advancedErrors[SelectedStage] : "";

    private void RefreshEffectCard()
    {
        foreach (string property in new[] { nameof(HasAdvancedCard), nameof(IsAdvancedCard), nameof(IsSimpleCard),
            nameof(IsAdvancedChorusCard), nameof(IsAdvancedPhaserCard), nameof(IsAdvancedDelayCard),
            nameof(IsAdvancedReverbCard), nameof(EffectCardAction), nameof(EffectCardError) }) Notify(property);
    }

    public async Task ToggleEffectCardAsync()
    {
        if (!HasAdvancedCard) return;
        int effect = SelectedStage;
        advancedCards[effect] = !advancedCards[effect];
        RefreshEffectCard();
        // Returning to simple controls is presentation only. The preview and its
        // recovery remain alive until explicitly restored or the session ends.
        if (!advancedCards[effect]) return;
        switch (effect)
        {
            case 1: await ReadChorusAsync(); break;
            case 2: await ReadPhaserAsync(); break;
            case 3: await ReadDelayAsync(); break;
            case 4: await ReadReverbAsync(); break;
        }
    }
}
