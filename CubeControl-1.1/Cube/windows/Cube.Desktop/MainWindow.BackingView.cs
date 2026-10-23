using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
namespace Cube.Desktop;
public partial class MainWindow
{
    private Grid BackingPage => BackingView.BackingPage;
    private ColumnDefinition BackingSideColumn => BackingView.BackingSideColumn;
    private Border BackingTransportHost => BackingView.BackingTransportHost;
    private ToolBar BackingTransportButtons => BackingView.BackingTransportButtons;
    private Wpf.Ui.Controls.Button BackingPlayButton => BackingView.BackingPlayButton;
    private CheckBox BackingLoopButton => BackingView.BackingLoopButton;
    private StackPanel BackingMetronomePanel => BackingView.BackingMetronomePanel;
    private StackPanel BackingTempoGroup => BackingView.BackingTempoGroup;
    private Wpf.Ui.Controls.Button BackingBpmButton => BackingView.BackingBpmButton;
    private ComboBox BackingSpeedOptions => BackingView.BackingSpeedOptions;
    private Border BackingPitchOptions => BackingView.BackingPitchOptions;
    private ScrollViewer BackingTracksScroll => BackingView.BackingTracksScroll;
    private Grid BackingWorkspace => BackingView.BackingWorkspace;
    private Grid BackingArrangement => BackingView.BackingArrangement;
    private Controls.BackingWaveform BackingMainWave => BackingView.BackingMainWave;
    private ListBox BackingStemList => BackingView.BackingStemList;
    private Border BackingVolumesPanel => BackingView.BackingVolumesPanel;
    private Border BackingLibraryLayout => BackingView.BackingLibraryLayout;
    private Border BackingYouTubePanel => BackingView.BackingYouTubePanel;
    private Wpf.Ui.Controls.AutoSuggestBox BackingYouTubeSearch => BackingView.BackingYouTubeSearch;
    private ToggleButton BackingTrackSearchToggle => BackingView.BackingTrackSearchToggle;
    private ScrollViewer BackingYouTubeScroll => BackingView.BackingYouTubeScroll;
    private ItemsControl YouTubeMosaic => BackingView.YouTubeMosaic;
    private Border BackingDropOverlay => BackingView.BackingDropOverlay;
    private Popup BackingSessionsPopup => BackingView.BackingSessionsPopup;
    private ListBox BackingTrackList => BackingView.BackingTrackList;
    internal bool BackingDialogOpen { get; set; }
    private bool backingDialogOpen { get => BackingDialogOpen; set => BackingDialogOpen = value; }
    private static ButtonBase? BackingButtonAncestor(DependencyObject? source) => Views.BackingTracksView.BackingButtonAncestor(source);
    private bool OpenBackingKeyboardMenu(DependencyObject? source) => BackingView.OpenBackingKeyboardMenu(source);
    private void OpenBackingMenu(FrameworkElement owner, string kind, ViewModels.BackingTrack? track = null, ViewModels.BackingSection? section = null, double? time = null, bool atMouse = false)
        => BackingView.OpenBackingMenu(owner, kind, track, section, time, atMouse);
    private void ConfigureBackingTransport(double width, double height) => BackingView?.ConfigureBackingTransport(width, height);
}
