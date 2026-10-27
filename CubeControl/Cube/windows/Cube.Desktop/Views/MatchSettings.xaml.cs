using System.Windows.Controls;
using Cube.Desktop.ViewModels;
namespace Cube.Desktop.Views;
public partial class MatchSettings : UserControl
{
    public MatchRow[] Rows { get; }
    public MatchSettings(MatchRow[] rows) { Rows = rows; InitializeComponent(); DataContext = rows; }
}
