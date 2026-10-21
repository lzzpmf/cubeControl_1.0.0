using System.ComponentModel;
using Cube.Desktop.ViewModels;
using Cube.WinUI.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Cube.WinUI.Views;

public sealed class AmplifierRackView : UserControl
{
    private readonly AmpViewModel model;
    private readonly string property;
    private readonly Func<AmpControl, UIElement> create;
    private readonly WorkbenchPanel rack = new() { CellWidth = 170, MaximumColumns = 6, Gap = 12 };
    private object? controls;
    public AmplifierRackView(AmpViewModel model, string property, Func<AmpControl, UIElement> create)
    {
        this.model = model; this.property = property; this.create = create;
        Content = rack; IsTabStop = false; HorizontalContentAlignment = HorizontalAlignment.Stretch;
        Loaded += (_, _) => { model.PropertyChanged += Changed; Refresh(); };
        Unloaded += (_, _) => model.PropertyChanged -= Changed;
        Refresh();
    }
    private void Changed(object? sender, PropertyChangedEventArgs e) { if (e.PropertyName == property) Refresh(); }
    private void Refresh()
    {
        var current = (AmpControl[])typeof(AmpViewModel).GetProperty(property)!.GetValue(model)!;
        if (ReferenceEquals(current, controls)) return;
        controls = current; rack.Children.Clear();
        foreach (var control in current) rack.Children.Add(create(control));
    }
}
