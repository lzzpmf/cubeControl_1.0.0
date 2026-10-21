using System.ComponentModel;
using Cube.Desktop.ViewModels;
using Cube.Desktop.Localization;
using Cube.WinUI.Appearance;
using Cube.WinUI.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Automation.Peers;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.UI.ViewManagement;

namespace Cube.WinUI.Views;

public sealed class SignalChainView : UserControl
{
    private readonly EditorViewModel editor;
    private readonly Func<ParameterViewModel, UIElement> parameter;
    private readonly WorkbenchPanel chain = new() { CellWidth = 140, MaximumColumns = 5, Gap = 8 };
    private readonly WorkbenchPanel detail = new() { CellWidth = 145, MaximumColumns = 6, Gap = 12 };
    private readonly ContentControl detailHost = new() { HorizontalContentAlignment = HorizontalAlignment.Stretch, IsTabStop = false };
    private readonly List<(Image Image, string Kind)> artwork = [];
    private int selected;
    private PresetViewModel? current;
    public SignalChainView(EditorViewModel editor, Func<ParameterViewModel, UIElement> parameter)
    {
        App.Trace("SignalChain ctor");
        this.editor = editor; this.parameter = parameter; IsTabStop = false;
        selected = editor.SelectedStage switch { 0 => 0, 1 or 2 => 1, 3 => 2, 4 => 3, _ => 4 };
        var panel = new StackPanel { Spacing = 16 };
        detailHost.Content = detail;
        panel.Children.Add(chain); panel.Children.Add(detailHost); Content = panel;
        App.Trace("SignalChain content");
        detailHost.IsEnabled = editor.CanEdit;
        App.Trace("SignalChain bound");
        Loaded += (_, _) => { editor.PropertyChanged += EditorChanged; InstrumentAppearance.Changed += ArtworkChanged; detailHost.IsEnabled = editor.CanEdit; if (!ReferenceEquals(current, editor.Current)) Render(); else RefreshArtwork(); };
        Unloaded += (_, _) => { editor.PropertyChanged -= EditorChanged; InstrumentAppearance.Changed -= ArtworkChanged; };
        ActualThemeChanged += (_, _) => RefreshArtwork();
        Render();
    }
    private void EditorChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == "CanEdit") detailHost.IsEnabled = editor.CanEdit;
        if (e.PropertyName == "Current" && !ReferenceEquals(current, editor.Current)) Render();
    }
    private void Render()
    {
        App.Trace("SignalChain render");
        current = editor.Current; chain.Children.Clear(); artwork.Clear();
        for (int i = 0; i < current.Modules.Length; i++)
        {
            int index = i; var module = current.Modules[i];
            var content = new StackPanel { Spacing = 4, HorizontalAlignment = HorizontalAlignment.Center };
            content.Children.Add(new TextBlock { Text = module.Number + " / " + LanguageService.T(module.Name), FontWeight = Microsoft.UI.Text.FontWeights.SemiBold });
            var image = new Image { Width = 38, Height = 38, Stretch = Stretch.Uniform };
            AutomationProperties.SetAccessibilityView(image, AccessibilityView.Raw); artwork.Add((image, "footswitch")); content.Children.Add(image);
            App.Trace("SignalChain stage " + i);
            var selector = new RadioButton { Content = content, GroupName = "SignalChain", Style = (Style)Application.Current.Resources["SignalStageStyle"], IsChecked = i == selected };
            AutomationProperties.SetName(selector, LanguageService.T(module.Name));
            AutomationProperties.SetHelpText(selector, LanguageService.T(module.Description));
            selector.Checked += (_, _) => Show(index); chain.Children.Add(selector);
        }
        App.Trace("SignalChain artwork"); RefreshArtwork(); Show(selected);
    }
    private void Show(int index)
    {
        App.Trace("SignalChain show " + index);
        selected = index; detail.Children.Clear();
        editor.SelectedStage = index switch { 0 => 0, 1 => editor.Current.Parameters[8].Value > 8 ? 2 : 1, 2 => 3, 3 => 4, _ => 5 };
        foreach (var field in editor.Current.Modules[index].Parameters) detail.Children.Add(parameter(field));
        detail.Children.Add(parameter(editor.Current.Volume));
        StudioMotion.Enter(detailHost);
    }
    private void ArtworkChanged(object? sender, EventArgs args) => RefreshArtwork();
    private void RefreshArtwork()
    {
        bool contrast = new AccessibilitySettings().HighContrast;
        foreach (var entry in artwork)
        {
            entry.Image.Visibility = contrast ? Visibility.Collapsed : Visibility.Visible;
            entry.Image.Source = InstrumentAppearance.Face(entry.Kind, ActualTheme);
        }
    }
}
