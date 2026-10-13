using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Cube.Desktop.Services;
using Cube.Desktop.ViewModels;
using Wpf.Ui.Controls;

namespace Cube.Desktop.Controls;

public partial class YouTubeResultCard : UserControl, INotifyPropertyChanged
{
    public static readonly DependencyProperty ModelProperty = DependencyProperty.Register(nameof(Model), typeof(BackingTracksViewModel), typeof(YouTubeResultCard), new PropertyMetadata(null, ModelChanged));
    public BackingTracksViewModel? Model { get => (BackingTracksViewModel?)GetValue(ModelProperty); set => SetValue(ModelProperty, value); }
    private YouTubeResult? Video => DataContext as YouTubeResult;
    private BackingTracksViewModel? subscribed;
    public bool IsPreviewActive => Model?.YouTubePreviewId == Video?.Id && Model?.YouTubePreviewActive == true;
    public bool IsDownloading => Model?.YouTubeDownloading == true && Model.YouTubeDownloadId == Video?.Id;
    public bool HasDownloadMessage => Video != null && Model?.YouTubeDownloadMessageId == Video.Id && !string.IsNullOrEmpty(Model.YouTubeDownloadStatus);
    public bool CanDownload => IsDownloading || Model?.YouTubeDownloading == false;
    public bool IsDownloaded => Video != null && Model?.IsYouTubeDownloaded(Video.Id) == true;
    public string DownloadLabel => IsDownloading ? "Cancelar download" : IsDownloaded ? "Download realizado — baixar novamente" : "Baixar MP3";
    public SymbolRegular DownloadIcon => IsDownloading ? SymbolRegular.Dismiss24 : IsDownloaded ? SymbolRegular.CheckmarkCircle24 : SymbolRegular.ArrowDownload24;
    public Visibility DownloadProgressVisibility => IsDownloading ? Visibility.Visible : Visibility.Collapsed;
    public Visibility WaveVisibility => !HasDownloadMessage && PreviewPeaks.Length > 0 ? Visibility.Visible : Visibility.Collapsed;
    public Visibility DetailVisibility => WaveVisibility == Visibility.Visible || StatusText.Length > 0 || IsDownloading ? Visibility.Visible : Visibility.Collapsed;
    public string PreviewLabel => IsPreviewActive ? "Parar prévia" : "Pré-ouvir";
    public SymbolRegular PreviewIcon => IsPreviewActive ? SymbolRegular.Stop24 : SymbolRegular.Play24;
    public string StatusText => HasDownloadMessage ? Model!.YouTubeDownloadStatus
        : Model?.YouTubePreviewId != Video?.Id ? ""
        : Model?.YouTubePreviewStatus is "Pré-escuta · até 30 s" or "Prévia concluída" ? "" : Model?.YouTubePreviewStatus ?? "";
    private YouTubePreviewFrame? Wave => Model?.GetYouTubeWave(Video?.Id);
    public float[] PreviewPeaks => Wave?.Peaks ?? [];
    public double PreviewDuration => Wave?.Duration ?? (Video?.Duration > 0 ? Math.Min(30, Video.Duration) : 30);
    public double PreviewPosition => Model?.YouTubePreviewId == Video?.Id ? Wave?.Position ?? 0 : 0;
    public event PropertyChangedEventHandler? PropertyChanged;

    public YouTubeResultCard()
    {
        InitializeComponent();
        DataContextChanged += (_, e) =>
        {
            if (e.OldValue is YouTubeResult old) Model?.StopYouTubePreview(old.Id);
            Refresh();
        };
        TouchDown += (_, _) => { PreviewButton.Focus(); };
        LostKeyboardFocus += (_, _) => { if (!IsKeyboardFocusWithin && !IsMouseOver) Stop(); };
    }
    private void CardSizeChanged(object sender, SizeChangedEventArgs e)
    {
        bool compact = e.NewSize.Width < 480;
        ResultTitle.TextWrapping = compact ? TextWrapping.Wrap : TextWrapping.NoWrap;
        Grid.SetRow(PreviewDetails, compact ? 1 : 0);
        Grid.SetColumn(PreviewDetails, compact ? 1 : 2);
        Grid.SetColumnSpan(PreviewDetails, compact ? 2 : 1);
        PreviewDetails.Width = compact ? double.NaN : 120;
        PreviewDetails.Margin = compact ? new Thickness(12, 4, 12, 0) : new Thickness(0, 0, 12, 0);
        PreviewDetails.VerticalAlignment = VerticalAlignment.Center;
    }
    private static void ModelChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e) => ((YouTubeResultCard)sender).Subscribe();
    private void Subscribe()
    {
        if (subscribed != null) subscribed.PropertyChanged -= ModelPropertyChanged;
        subscribed = IsLoaded ? Model : null;
        if (subscribed != null) subscribed.PropertyChanged += ModelPropertyChanged;
        Refresh();
    }
    private void Refresh()
    {
        foreach (string name in new[] { nameof(IsPreviewActive), nameof(CanDownload), nameof(IsDownloading), nameof(DownloadLabel), nameof(DownloadIcon), nameof(DownloadProgressVisibility), nameof(WaveVisibility), nameof(DetailVisibility), nameof(PreviewLabel), nameof(PreviewIcon), nameof(StatusText), nameof(PreviewPeaks), nameof(PreviewDuration), nameof(PreviewPosition) })
            PropertyChanged?.Invoke(this, new(name));
    }
    private void ModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    { if (e.PropertyName?.StartsWith("YouTube", StringComparison.Ordinal) == true) Refresh(); }
    private void CardLoaded(object sender, RoutedEventArgs e) => Subscribe();
    private void CardUnloaded(object sender, RoutedEventArgs e)
    {
        Stop();
        if (subscribed != null) subscribed.PropertyChanged -= ModelPropertyChanged;
        subscribed = null;
    }
    private void CardVisibilityChanged(object sender, DependencyPropertyChangedEventArgs e) { if (!IsVisible) Stop(); }
    private void Stop() { if (Video is { } video) Model?.StopYouTubePreview(video.Id); }
    private async void CardMouseEnter(object sender, MouseEventArgs e)
    { if (e.StylusDevice == null && Window.GetWindow(this)?.IsActive == true && Video is { } video && Model != null) await Model.PreviewYouTube(video); }
    private void CardMouseLeave(object sender, MouseEventArgs e) => Stop();
    private async void PreviewClick(object sender, RoutedEventArgs e)
    {
        if (IsPreviewActive) Stop();
        else if (Video is { } video && Model != null) await Model.PreviewYouTube(video, immediate: true);
        e.Handled = true;
    }
    private async void DownloadClick(object sender, RoutedEventArgs e)
    {
        e.Handled = true;
        if (IsDownloading) Model?.CancelYouTubeDownload.Execute(null);
        else if (Video is { } video && Model != null) await Model.DownloadYouTube(video);
    }
}
