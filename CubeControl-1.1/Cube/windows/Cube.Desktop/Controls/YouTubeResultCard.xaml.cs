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
    public bool CanDownload => Model?.YouTubeDownloading == false;
    public string PreviewLabel => IsPreviewActive ? "Parar prévia" : "Pré-ouvir";
    public SymbolRegular PreviewIcon => IsPreviewActive ? SymbolRegular.Stop24 : SymbolRegular.Play24;
    public string StatusText => Model?.YouTubeDownloadId == Video?.Id && Model?.YouTubeDownloading == true ? Model.YouTubeDownloadStatus
        : Model?.YouTubePreviewId == Video?.Id ? Model?.YouTubePreviewStatus ?? "" : "Passe o mouse para pré-ouvir";
    public event PropertyChangedEventHandler? PropertyChanged;

    public YouTubeResultCard()
    {
        InitializeComponent();
        DataContextChanged += (_, e) =>
        {
            if (e.OldValue is YouTubeResult old) Model?.StopYouTubePreview(old.Id);
            Thumbnail.Visibility = Visibility.Visible; Refresh();
        };
        TouchDown += (_, _) => { PreviewButton.Focus(); };
        LostKeyboardFocus += (_, _) => { if (!IsKeyboardFocusWithin && !IsMouseOver) Stop(); };
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
        foreach (string name in new[] { nameof(IsPreviewActive), nameof(CanDownload), nameof(PreviewLabel), nameof(PreviewIcon), nameof(StatusText) })
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
    { if (Video is { } video && Model != null) await Model.DownloadYouTube(video); e.Handled = true; }
    private void ThumbnailFailed(object sender, ExceptionRoutedEventArgs e) => Thumbnail.Visibility = Visibility.Hidden;
    private void CardContentSizeChanged(object sender, SizeChangedEventArgs e)
        => ((FrameworkElement)sender).Clip = new System.Windows.Media.RectangleGeometry(new Rect(e.NewSize), 10, 10);
}
