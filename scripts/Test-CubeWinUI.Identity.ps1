param(
    [string]$Executable = 'C:\CubeControl-WinUI-Build\publish\CubeControl.WinUI\CubeControl.WinUI.exe',
    [string]$EvidenceDirectory = 'C:\CubeControl-WinUI-Build\evidence\identity'
)
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes
Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing
Add-Type @'
using System;
using System.Runtime.InteropServices;
public static class CubeIdentityWindow {
    [StructLayout(LayoutKind.Sequential)] public struct Rect { public int Left, Top, Right, Bottom; }
    [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr hwnd, out Rect rect);
    [DllImport("user32.dll")] public static extern bool PrintWindow(IntPtr hwnd, IntPtr dc, uint flags);
    [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr hwnd);
    [DllImport("user32.dll")] public static extern bool SetCursorPos(int x, int y);
    [DllImport("user32.dll")] public static extern void mouse_event(uint flags, uint x, uint y, int data, UIntPtr extra);
    [DllImport("dwmapi.dll")] public static extern int DwmFlush();
    [DllImport("user32.dll")] public static extern uint GetDpiForWindow(IntPtr hwnd);
    public static void Move(int x, int y) { mouse_event(1, unchecked((uint)x), unchecked((uint)y), 0, UIntPtr.Zero); }
}
'@
$EvidenceDirectory = Join-Path $EvidenceDirectory ([Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Force -Path $EvidenceDirectory | Out-Null
$data = Join-Path $EvidenceDirectory ('data-' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Force -Path $data | Out-Null
# Load an existing WPF-compatible preference and ensure subsequent edits retain it.
'{"Version":1,"Mode":"Dark","Finish":"BlackIvory","FutureField":"preserve"}' | Set-Content -LiteralPath (Join-Path $data 'appearance-v1.json') -Encoding UTF8
$app = Start-Process -FilePath $Executable -ArgumentList @('--offline', '--diagnostics', '--data-directory', ('"' + $data + '"')) -WorkingDirectory (Split-Path $Executable) -PassThru -WindowStyle Hidden
try {
    $until = [DateTime]::UtcNow.AddSeconds(25)
    do {
        $app.Refresh()
        if ($app.HasExited) { throw "Host exited: $($app.ExitCode)" }
        if ($app.MainWindowHandle -ne 0) { break }
        Start-Sleep -Milliseconds 200
    } while ([DateTime]::UtcNow -lt $until)
    $window = [System.Windows.Automation.AutomationElement]::FromHandle($app.MainWindowHandle)
    [ordered]@{ Executable = $Executable; Windows = [Environment]::OSVersion.VersionString; Dpi = [CubeIdentityWindow]::GetDpiForWindow($app.MainWindowHandle); StartedUtc = [DateTime]::UtcNow.ToString('o') } |
        ConvertTo-Json | Set-Content -LiteralPath (Join-Path $EvidenceDirectory 'environment.json') -Encoding UTF8
    function Find($property, $value) {
        $window.FindFirst([System.Windows.Automation.TreeScope]::Descendants, [System.Windows.Automation.PropertyCondition]::new($property, $value))
    }
    function Wait-Control($property, $value) {
        $until = [DateTime]::UtcNow.AddSeconds(12)
        do { $found = Find $property $value; if ($found) { return $found }; Start-Sleep -Milliseconds 150 } while ([DateTime]::UtcNow -lt $until)
        throw "Missing control: $value"
    }
    function Name($value) { Wait-Control ([System.Windows.Automation.AutomationElement]::NameProperty) $value }
    function Id($value) { Wait-Control ([System.Windows.Automation.AutomationElement]::AutomationIdProperty) $value }
    function Wait-Value($range, $value, $context) {
        $until = [DateTime]::UtcNow.AddSeconds(4)
        do { if ([Math]::Abs($range.Current.Value - $value) -lt .00001) { return }; Start-Sleep -Milliseconds 100 } while ([DateTime]::UtcNow -lt $until)
        $focused = [System.Windows.Automation.AutomationElement]::FocusedElement
        throw "$context expected $value, received $($range.Current.Value); focus=$($focused.Current.Name)"
    }
    function Route($value) {
        (Name $value).GetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern).Select()
        $until = [DateTime]::UtcNow.AddSeconds(8)
        do { $heading = Id 'Heading'; if ($heading.Current.Name -eq $value) { return }; Start-Sleep -Milliseconds 100 } while ([DateTime]::UtcNow -lt $until)
        throw "Route did not open: $value"
    }
    function Capture($name) {
        $null = [CubeIdentityWindow]::SetForegroundWindow($app.MainWindowHandle)
        Start-Sleep -Milliseconds 800
        $null = [CubeIdentityWindow]::DwmFlush()
        $rect = [CubeIdentityWindow+Rect]::new()
        $null = [CubeIdentityWindow]::GetWindowRect($app.MainWindowHandle, [ref]$rect)
        $bitmap = [System.Drawing.Bitmap]::new($rect.Right - $rect.Left, $rect.Bottom - $rect.Top)
        $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
        $dc = $graphics.GetHdc()
        try { if (![CubeIdentityWindow]::PrintWindow($app.MainWindowHandle, $dc, 2)) { throw 'PrintWindow failed' } }
        finally { $graphics.ReleaseHdc($dc) }
        try { $bitmap.Save((Join-Path $EvidenceDirectory ($name + '.png')), [System.Drawing.Imaging.ImageFormat]::Png) }
        finally { $graphics.Dispose(); $bitmap.Dispose() }
        $cache = [System.Windows.Automation.CacheRequest]::new()
        foreach ($property in @([System.Windows.Automation.AutomationElement]::NameProperty, [System.Windows.Automation.AutomationElement]::AutomationIdProperty, [System.Windows.Automation.AutomationElement]::ControlTypeProperty, [System.Windows.Automation.AutomationElement]::IsEnabledProperty)) { $cache.Add($property) }
        $scope = $cache.Activate()
        try {
            $elements = $window.FindAll([System.Windows.Automation.TreeScope]::Descendants, [System.Windows.Automation.Condition]::TrueCondition)
            $inventory = @($elements | ForEach-Object {
                $item = $_.Cached
                if ($item.ControlType -in @([System.Windows.Automation.ControlType]::Button, [System.Windows.Automation.ControlType]::Slider, [System.Windows.Automation.ControlType]::ComboBox, [System.Windows.Automation.ControlType]::CheckBox, [System.Windows.Automation.ControlType]::RadioButton, [System.Windows.Automation.ControlType]::Edit)) {
                    [ordered]@{ Name = $item.Name; Id = $item.AutomationId; Type = $item.ControlType.ProgrammaticName; Enabled = $item.IsEnabled }
                }
            })
            ConvertTo-Json -InputObject $inventory -Depth 3 | Set-Content -LiteralPath (Join-Path $EvidenceDirectory ($name + '.controls.json')) -Encoding UTF8
        } finally { $scope.Dispose() }
    }
    $null = Id 'Heading'
    Capture 'pedal-dark'
    Route 'Dispositivo e ajuda'
    $knob = Name 'Knob'
    $fader = Name 'Fader'
    foreach ($control in @($knob, $fader)) {
        $range = $control.GetCurrentPattern([System.Windows.Automation.RangeValuePattern]::Pattern)
        $range.SetValue(0); if ($range.Current.Value -ne 0) { throw 'Minimum not applied' }
        $range.SetValue(100); if ($range.Current.Value -ne 100) { throw 'Maximum not applied' }
        $range.SetValue(37); if ($range.Current.Value -ne 37) { throw 'Range input did not update' }
        $invalidAccepted = $false
        try { $range.SetValue(101); $invalidAccepted = $true } catch { }
        if ($invalidAccepted) { throw 'Out-of-range input accepted' }
        $null = [CubeIdentityWindow]::SetForegroundWindow($app.MainWindowHandle)
        $control.SetFocus()
        Start-Sleep -Milliseconds 200
        [System.Windows.Forms.SendKeys]::SendWait('{HOME}{RIGHT}')
        Wait-Value $range 1 ("Keyboard " + $control.Current.Name)
        [System.Windows.Forms.SendKeys]::SendWait('{BACKSPACE}')
        Wait-Value $range 50 ("Restore " + $control.Current.Name)
    }
    $exact = Name 'Valor exato de Knob'
    $exact.GetCurrentPattern([System.Windows.Automation.RangeValuePattern]::Pattern).SetValue(64)
    if ($knob.GetCurrentPattern([System.Windows.Automation.RangeValuePattern]::Pattern).Current.Value -ne 64) { throw 'Exact edit did not repaint knob' }
    Write-Host 'PASS: knob/fader RangeValue, bounds, keyboard, restore and exact editing.'
    $range = $knob.GetCurrentPattern([System.Windows.Automation.RangeValuePattern]::Pattern)
    $range.SetValue(50)
    $knob.GetCurrentPattern([System.Windows.Automation.ScrollItemPattern]::Pattern).ScrollIntoView()
    $knob.SetFocus(); Start-Sleep -Milliseconds 300
    $bounds = $knob.Current.BoundingRectangle
    $x = [int]($bounds.Left + $bounds.Width / 2); $y = [int]($bounds.Top + $bounds.Height / 2)
    $null = [CubeIdentityWindow]::SetCursorPos($x, $y)
    [CubeIdentityWindow]::mouse_event(0x800, 0, 0, 60, [UIntPtr]::Zero)
    Start-Sleep -Milliseconds 150
    if ($range.Current.Value -ne 50) { throw 'High-resolution wheel lost fractional delta' }
    [CubeIdentityWindow]::mouse_event(0x800, 0, 0, 60, [UIntPtr]::Zero)
    Start-Sleep -Milliseconds 150
    if ($range.Current.Value -ne 51) { throw 'Wheel deltas did not accumulate' }
    [CubeIdentityWindow]::mouse_event(2, 0, 0, 0, [UIntPtr]::Zero)
    Start-Sleep -Milliseconds 150
    foreach ($offset in @(4, 8, 12, 16)) {
        [CubeIdentityWindow]::Move(4, -4)
        Start-Sleep -Milliseconds 70
    }
    [CubeIdentityWindow]::mouse_event(4, 0, 0, 0, [UIntPtr]::Zero)
    if ($range.Current.Value -le 51) { throw 'Pointer drag did not edit the knob' }
    $range.SetValue(70)
    $null = [CubeIdentityWindow]::SetCursorPos($x, $y)
    foreach ($click in 1..2) {
        [CubeIdentityWindow]::mouse_event(2, 0, 0, 0, [UIntPtr]::Zero)
        [CubeIdentityWindow]::mouse_event(4, 0, 0, 0, [UIntPtr]::Zero)
        Start-Sleep -Milliseconds 80
    }
    Start-Sleep -Milliseconds 200
    if ($range.Current.Value -ne 50) { throw 'Double-click restore failed' }
    Write-Host 'PASS: real pointer drag, fractional wheel deltas and double-click restore.'
    Capture 'controls-dark'
    # Each finish must have the complete assets used by the presentation layer.
    $assets = Join-Path (Split-Path $Executable) 'Assets\Images\Controls'
    $finishes = @(Get-ChildItem -LiteralPath (Join-Path $assets 'Finishes') -Directory)
    foreach ($finish in $finishes) {
        foreach ($face in @('knob.png', 'fader-vertical.png', 'footswitch.png', 'switch-on.png', 'switch-off.png', 'player-face.png')) {
            if (!(Test-Path -LiteralPath (Join-Path $finish.FullName $face))) { throw "Missing artwork: $($finish.Name)/$face" }
        }
    }
    Route 'Criador de amps'
    (Name 'Carregar modelo local').GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern).Invoke()
    $gain = Name 'Ganho'
    $range = $gain.GetCurrentPattern([System.Windows.Automation.RangeValuePattern]::Pattern)
    $initial = $range.Current.Value
    $range.SetValue([Math]::Min(1000, $initial + 10))
    $null = Name 'Modificado localmente'
    $range.SetValue(500)
    $gain.SetFocus(); Start-Sleep -Milliseconds 200
    [System.Windows.Forms.SendKeys]::SendWait('+{RIGHT}')
    Wait-Value $range 500.1 'Amp fine adjustment'
    $gain.SetFocus(); [System.Windows.Forms.SendKeys]::SendWait('{BACKSPACE}'); Start-Sleep -Milliseconds 250
    if ([Math]::Abs($range.Current.Value - $initial) -gt .001) { throw 'Amp restore did not return to loaded reference' }
    Capture 'amp-dark'
    Route 'IRs'; Capture 'ir-dark'
    Route 'Backing tracks'; Capture 'backing-dark'
    $transform = $window.GetCurrentPattern([System.Windows.Automation.TransformPattern]::Pattern)
    $transform.Resize(760, 800); Capture 'backing-narrow'
    $transform.Resize(480, 800); Capture 'backing-compact'
    $windowBounds = $window.Current.BoundingRectangle
    foreach ($controlId in @('ActivePreset', 'SessionConnect', 'SessionDisconnect', 'PlaybackPlay', 'PlaybackStop')) {
        $control = Id $controlId; $bounds = $control.Current.BoundingRectangle
        if ($bounds.Width -le 0 -or $bounds.Left -lt $windowBounds.Left -or $bounds.Right -gt $windowBounds.Right -or $bounds.Bottom -gt $windowBounds.Bottom) { throw "Compact chrome clipped $controlId" }
    }
    $transform.Resize(1280, 800)
    Route 'Dispositivo e ajuda'
    $finishCombo = Name 'Acabamento'
    $finishCombo.GetCurrentPattern([System.Windows.Automation.ExpandCollapsePattern]::Pattern).Expand()
    (Name 'Grafite Cobre').GetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern).Select()
    $combo = Name 'Tema'
    $combo.GetCurrentPattern([System.Windows.Automation.ExpandCollapsePattern]::Pattern).Expand()
    (Name 'Claro').GetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern).Select()
    (Name 'Aplicar tema').GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern).Invoke()
    Capture 'controls-light'
    $saved = Get-Content -LiteralPath (Join-Path $data 'appearance-v1.json') -Raw | ConvertFrom-Json
    if ($saved.Mode -ne 'Light' -or $saved.Finish -ne 'GraphiteCopper' -or $saved.FutureField -ne 'preserve') { throw 'Appearance write lost existing preferences' }
    $window.GetCurrentPattern([System.Windows.Automation.WindowPattern]::Pattern).Close()
    if (!$app.WaitForExit(5000)) { throw 'Host did not close for preference restart' }
    $app = Start-Process -FilePath $Executable -ArgumentList @('--offline', '--data-directory', ('"' + $data + '"')) -WorkingDirectory (Split-Path $Executable) -PassThru -WindowStyle Hidden
    $until = [DateTime]::UtcNow.AddSeconds(20)
    do { $app.Refresh(); if ($app.MainWindowHandle -ne 0) { break }; Start-Sleep -Milliseconds 200 } while ([DateTime]::UtcNow -lt $until)
    $window = [System.Windows.Automation.AutomationElement]::FromHandle($app.MainWindowHandle)
    Route 'Dispositivo e ajuda'
    $selected = (Name 'Acabamento').GetCurrentPattern([System.Windows.Automation.SelectionPattern]::Pattern).Current.GetSelection()
    if ($selected.Count -ne 1 -or $selected[0].Current.Name -ne 'Grafite Cobre') { throw 'Restart did not restore finish' }
    Write-Host "PASS: WPF preference, $($finishes.Count) finish asset sets, amp editing/restore, 760/480 px layouts, light theme and preference restart."
    Write-Host "Evidence: $EvidenceDirectory"
} finally {
    if (!$app.HasExited) { $null = $app.CloseMainWindow(); $null = $app.WaitForExit(5000) }
}
