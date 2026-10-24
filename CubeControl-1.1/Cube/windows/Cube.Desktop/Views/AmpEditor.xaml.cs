using System.Windows.Controls;
using Cube.Protocol;
using System.Windows;
using Cube.Desktop.Localization;
using Cube.Desktop.ViewModels;
using System.Windows.Input;

namespace Cube.Desktop.Views;

public partial class AmpEditor : UserControl
{
    private readonly EditorViewModel model;
    public static readonly DependencyProperty IsCompactLayoutProperty = DependencyProperty.Register(nameof(IsCompactLayout), typeof(bool), typeof(AmpEditor), new PropertyMetadata(false));
    public bool IsCompactLayout { get => (bool)GetValue(IsCompactLayoutProperty); set => SetValue(IsCompactLayoutProperty, value); }
    public AmpEditor(EditorViewModel model)
    {
        this.model = model; InitializeComponent(); DataContext = model;
        AddHandler(Mouse.PreviewMouseDownEvent, new MouseButtonEventHandler((_, _) => model.BeginGesture(this)), true);
        AddHandler(Mouse.MouseUpEvent, new MouseButtonEventHandler((_, _) => model.EndGesture(this)), true);
        Unloaded += (_, _) => model.EndGesture(this);
        PreviewMouseWheel += (_, e) =>
        {
            if (!model.CanAdjustAmp || !EqGraph.IsVisible || EqGraph.FilterAt(e.GetPosition(EqGraph)) < 0) return;
            model.BeginGesture(this);
            try { e.Handled = EqGraph.AdjustQ(e.GetPosition(EqGraph), e.Delta); }
            finally { model.EndGesture(this); }
        };
        BaseSelector.ItemsSource = EquipmentCatalog.Amps.Select(a => a.Name)
            .Concat(AmpEqPresets.All.Select(a => a.Description)).ToArray();
        Loaded += (_, _) => UpdateLayoutMode();
        SizeChanged += (_, _) => UpdateLayoutMode();
    }
    private void UpdateLayoutMode()
    {
        bool headerWide = ActualWidth >= 800, eqWide = ActualWidth >= 960;
        AmpHeader.ColumnDefinitions[1].Width = headerWide ? GridLength.Auto : new GridLength(0);
        Grid.SetColumn(GlobalControls, headerWide ? 1 : 0); Grid.SetRow(GlobalControls, headerWide ? 1 : 2);
        GlobalControls.Margin = new Thickness(0, headerWide ? 8 : 16, 0, 0);
        ModernEq.ColumnDefinitions[1].Width = new GridLength(eqWide ? 240 : 0);
        Grid.SetColumn(EqControlsPanel, eqWide ? 1 : 0); Grid.SetRow(EqControlsPanel, eqWide ? 0 : 1);
        EqControlsPanel.Margin = eqWide ? new Thickness(12, 0, 0, 0) : new Thickness(0, 8, 0, 0);
        double width = Math.Max(152, Math.Floor((ActualWidth - 24) / 152) * 152);
        AFirstGroup.Width = Math.Min(760, width); ASecondGroup.Width = Math.Min(456, width);
        BFirstGroup.Width = BSecondGroup.Width = Math.Min(456, width); BThirdGroup.Width = Math.Min(304, width);
    }
    private void EditValueClick(object sender, RoutedEventArgs e)
    {
        if (!model.CanAdjustAmp || sender is not FrameworkElement { DataContext: AmpControl control, Parent: Grid grid } button) return;
        var editor = grid.Children.OfType<TextBox>().Single();
        editor.Text = control.EditText; editor.Tag = editor.Text;
        button.Visibility = Visibility.Collapsed; editor.Visibility = Visibility.Visible;
        editor.Focus(); editor.SelectAll();
    }
    private static void CloseValueEditor(TextBox editor)
    {
        if (editor.Parent is not Grid grid) return;
        editor.Visibility = Visibility.Collapsed;
        grid.Children.OfType<Wpf.Ui.Controls.Button>().Single().Visibility = Visibility.Visible;
        if (grid.Parent is StackPanel panel) ((TextBlock)panel.Children[1]).Visibility = Visibility.Collapsed;
    }
    private void ValueEditorKeyDown(object sender, KeyEventArgs e)
    {
        if (sender is not TextBox { DataContext: AmpControl control } editor || e.Key is not (Key.Enter or Key.Escape)) return;
        e.Handled = true;
        if (e.Key == Key.Enter && model.CanAdjustAmp && editor.Text != editor.Tag as string && !control.TryEdit(editor.Text))
        {
            if (editor.Parent is Grid { Parent: StackPanel panel }) ((TextBlock)panel.Children[1]).Visibility = Visibility.Visible;
            return;
        }
        CloseValueEditor(editor);
        if (editor.Parent is Grid grid) grid.Children.OfType<Wpf.Ui.Controls.Button>().Single().Focus();
    }
    private void ValueEditorLostFocus(object sender, KeyboardFocusChangedEventArgs e) { if (sender is TextBox editor) CloseValueEditor(editor); }
    private void ResetBandClick(object sender, RoutedEventArgs e)
    {
        if (!model.CanAdjustAmp || sender is not FrameworkElement button) return;
        var controls = Convert.ToInt32(button.Tag) == 0 ? model.Amp.EqAControls : model.Amp.EqBControls;
        if (controls.Length == 3) controls[0].Restore();
    }
    private int selectedBase = -1;
    private bool selectingBase;
    private async void BaseSelected(object sender, SelectionChangedEventArgs e)
    {
        if (!IsLoaded || selectingBase || BaseSelector.SelectedIndex < 0) return;
        int next = BaseSelector.SelectedIndex;
        selectingBase = true;
        try
        {
            if (!model.CanEditAmpLocally) { BaseSelector.SelectedIndex = selectedBase; return; }
            var dialog = new Wpf.Ui.Controls.MessageBox
            {
                Owner = Window.GetWindow(this), Title = LanguageService.T("Carregar amp"),
                Content = LanguageService.T("Carregar o amp selecionado? Os ajustes atuais serão perdidos."),
                PrimaryButtonText = LanguageService.T("Carregar"), CloseButtonText = LanguageService.T("Cancelar")
            };
            if (await dialog.ShowDialogAsync() != Wpf.Ui.Controls.MessageBoxResult.Primary)
            {
                BaseSelector.SelectedIndex = selectedBase; return;
            }
            bool loaded;
            if (next >= EquipmentCatalog.Amps.Count)
            {
                var preset = AmpEqPresets.All[next - EquipmentCatalog.Amps.Count];
                model.Amp.Load(preset.Build(), preset.Name);
                loaded = true;
            }
            else loaded = await model.ReadAmpAsync(next);
            if (loaded)
            {
                selectedBase = next;
                if (next < EquipmentCatalog.Amps.Count) model.Amp.Slot = next;
                await ApplyControls();
            }
            else
            {
                BaseSelector.SelectedIndex = selectedBase;
            }
        }
        finally { selectingBase = false; }
    }
    private async void ImportClick(object sender, RoutedEventArgs e)
    {
        var imported = await model.ImportAmpAsync();
        if (imported == true) await ApplyControls();
    }
    private async void ExportClick(object sender, RoutedEventArgs e) => await model.ExportAmpAsync();
    private async Task ApplyControls() => await model.PreviewAmpAsync();
    private void RestoreControlRequested(object sender, RoutedEventArgs e)
    {
        if (!model.CanAdjustAmp || sender is not FrameworkElement { DataContext: AmpControl control }) return;
        control.Restore();
    }
    private void RestoreCapacitorRequested(object sender, RoutedEventArgs e)
    {
        if (!model.CanAdjustAmp || sender is not FrameworkElement { DataContext: CapacitorBank bank }) return;
        if (bank.RestoreOriginal.CanExecute(null)) bank.RestoreOriginal.Execute(null);
        e.Handled = true;
    }
    private async void RestoreClick(object sender, RoutedEventArgs e) => await model.RestoreAmpAsync();
    private async void WriteClick(object sender, RoutedEventArgs e) => await model.WriteAmpAsync(async message =>
    {
        var dialog = new Wpf.Ui.Controls.MessageBox
        {
            Owner = Window.GetWindow(this), Title = LanguageService.T("Salvar no pedal"), Content = LanguageService.T(message),
            PrimaryButtonText = LanguageService.T("Confirmar"), CloseButtonText = LanguageService.T("Cancelar")
        };
        return await dialog.ShowDialogAsync() == Wpf.Ui.Controls.MessageBoxResult.Primary;
    });
}
