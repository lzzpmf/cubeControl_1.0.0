using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Data;
using Cube.Desktop.Appearance;
using Cube.Desktop.Localization;

namespace Cube.Desktop.Controls;

// Coordinates stay in the scene's space, including when the Viewbox scales it.
public sealed class MicrophoneDistance : Viewbox
{
    public static readonly DependencyProperty DistanceProperty = DependencyProperty.Register(
        nameof(Distance), typeof(double), typeof(MicrophoneDistance),
        new FrameworkPropertyMetadata(50d, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
            (d, e) =>
            {
                var control = (MicrophoneDistance)d;
                control.PositionMicrophone();
                if (control.microphone != null && UIElementAutomationPeer.FromElement(control.microphone) is MicrophonePeer peer)
                    peer.RaisePropertyChangedEvent(RangeValuePatternIdentifiers.ValueProperty, e.OldValue, e.NewValue);
            },
            (_, value) => Math.Clamp(Math.Round((double)value), 0, 99)));
    public double Distance { get => (double)GetValue(DistanceProperty); set => SetCurrentValue(DistanceProperty, value); }
    public static readonly DependencyProperty ApplyCommandProperty = DependencyProperty.Register(nameof(ApplyCommand), typeof(ICommand), typeof(MicrophoneDistance));
    public ICommand? ApplyCommand { get => (ICommand?)GetValue(ApplyCommandProperty); set => SetValue(ApplyCommandProperty, value); }
    private void Apply() { if (IsEnabled && ApplyCommand?.CanExecute(null) == true) ApplyCommand.Execute(null); }
    private readonly Canvas scene = new() { Width = 480, Height = 250, Background = Brushes.Transparent };
    private readonly Thumb microphone;
    private double grabOffset;
    private const double Near = 210, Travel = 160;

    public MicrophoneDistance()
    {
        Stretch = Stretch.Uniform;
        StretchDirection = StretchDirection.DownOnly;
        HorizontalAlignment = HorizontalAlignment.Stretch;
        var cabinet = new Image { Source = Load("gabineteGenerico.png"), Width = 250, Height = 215, Stretch = Stretch.Uniform, IsHitTestVisible = false, RenderTransformOrigin = new Point(.5, .5), RenderTransform = new ScaleTransform(-1, 1) };
        Canvas.SetLeft(cabinet, 40); Canvas.SetTop(cabinet, 2); scene.Children.Add(cabinet);

        var picture = new FrameworkElementFactory(typeof(Image));
        picture.SetValue(Image.SourceProperty, Load("micGenerico.png"));
        picture.SetValue(Image.StretchProperty, Stretch.Uniform);
        picture.SetValue(RenderTransformOriginProperty, new Point(.5, .5));
        picture.SetValue(RenderTransformProperty, new ScaleTransform(-1, 1));
        var frame = new FrameworkElementFactory(typeof(Border));
        frame.SetValue(Border.BorderThicknessProperty, new Thickness(0));
        frame.SetValue(Border.CornerRadiusProperty, new CornerRadius(4));
        frame.SetValue(Border.BorderBrushProperty, Brushes.Transparent);
        frame.SetValue(Border.BackgroundProperty, Brushes.Transparent);
        frame.SetValue(IsHitTestVisibleProperty, false);
        frame.Name = "FocusFrame";
        var content = new FrameworkElementFactory(typeof(Grid));
        content.SetValue(Panel.BackgroundProperty, Brushes.Transparent);
        content.AppendChild(picture);
        content.AppendChild(frame);
        var template = new ControlTemplate(typeof(Thumb)) { VisualTree = content };
        microphone = new MicrophoneThumb(this)
        {
            Width = 100, Height = 91, Cursor = GrabCursors.OpenHand, Focusable = true,
            FocusVisualStyle = null, Template = template
        };
        microphone.SetBinding(AutomationProperties.NameProperty, new Binding("[Distância do microfone]") { Source = LanguageService.Instance });
        microphone.SetBinding(AutomationProperties.HelpTextProperty, new Binding("[Use as setas para ajustar; solte a tecla para ouvir. Home e End selecionam os limites.]") { Source = LanguageService.Instance });
        scene.Children.Add(microphone);
        microphone.DragStarted += (_, _) =>
        {
            grabOffset = Mouse.GetPosition(scene).X - Canvas.GetLeft(microphone);
            microphone.Cursor = GrabCursors.ClosedHand;
        };
        microphone.DragDelta += (_, _) => Distance = (Mouse.GetPosition(scene).X - grabOffset - Near) / Travel * 99;
        microphone.DragCompleted += (_, e) =>
        {
            microphone.Cursor = GrabCursors.OpenHand;
            if (!e.Canceled) Apply();
        };
        microphone.KeyDown += (_, e) =>
        {
            if (e.Key is not (Key.Left or Key.Right or Key.Home or Key.End)) return;
            Distance = e.Key == Key.Home ? 0 : e.Key == Key.End ? 99 : Distance + (e.Key == Key.Right ? 1 : -1);
            e.Handled = true;
        };
        microphone.KeyUp += (_, e) => { if (e.Key is Key.Left or Key.Right or Key.Home or Key.End) { Apply(); e.Handled = true; } };
        void RefreshAppearance() => Opacity = IsEnabled || ThemeService.Instance.IsHighContrast ? 1 : .5;
        IsEnabledChanged += (_, _) => RefreshAppearance();
        Loaded += (_, _) => { ThemeService.Instance.Changed += RefreshAppearance; RefreshAppearance(); };
        Unloaded += (_, _) => ThemeService.Instance.Changed -= RefreshAppearance;
        Child = scene;
        PositionMicrophone();
    }

    private sealed class MicrophoneThumb(MicrophoneDistance scene) : Thumb
    {
        protected override AutomationPeer OnCreateAutomationPeer() => new MicrophonePeer(this, scene);
    }

    private sealed class MicrophonePeer(Thumb thumb, MicrophoneDistance scene) : FrameworkElementAutomationPeer(thumb), IRangeValueProvider
    {
        protected override string GetClassNameCore() => nameof(MicrophoneDistance);
        protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Slider;
        public override object? GetPattern(PatternInterface pattern) => pattern == PatternInterface.RangeValue ? this : base.GetPattern(pattern);
        public bool IsReadOnly => !scene.IsEnabled;
        public double Minimum => 0;
        public double Maximum => 99;
        public double SmallChange => 1;
        public double LargeChange => 10;
        public double Value => scene.Distance;
        public void SetValue(double value)
        {
            if (IsReadOnly) throw new ElementNotEnabledException();
            if (!double.IsFinite(value) || value < Minimum || value > Maximum) throw new ArgumentOutOfRangeException(nameof(value));
            scene.Distance = value;
            scene.Apply(); // One accessibility edit is a completed gesture, like key release.
        }
    }

    private void PositionMicrophone()
    {
        if (microphone == null) return;
        // Keep a horizontal capsule path; the full range needs a longer, less sensitive drag.
        Canvas.SetLeft(microphone, Near + Travel * Distance / 99);
        Canvas.SetTop(microphone, 92);
    }

    private static BitmapImage Load(string name)
    {
        var image = new BitmapImage();
        image.BeginInit();
        image.CacheOption = BitmapCacheOption.OnLoad;
        image.UriSource = new Uri(AssetPaths.Image(name));
        image.EndInit(); image.Freeze();
        return image;
    }
}
