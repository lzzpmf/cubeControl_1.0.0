param([string]$Executable = 'C:\CubeControl-WinUI-Build\publish\CubeControl.WinUI\CubeControl.WinUI.exe')
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes
Add-Type -AssemblyName System.Windows.Forms
$data = Join-Path ([IO.Path]::GetTempPath()) ('CubeWinUI-test-' + [Guid]::NewGuid().ToString('N'))
$app = Start-Process -FilePath $Executable -ArgumentList @('--offline', '--data-directory', ('"' + $data + '"')) -WorkingDirectory (Split-Path $Executable) -PassThru -WindowStyle Hidden
try {
    $deadline = [DateTime]::UtcNow.AddSeconds(20)
    do {
        $app.Refresh()
        if ($app.HasExited) { throw "Host encerrou antes de abrir: $($app.ExitCode)" }
        if ($app.MainWindowHandle -ne 0) { break }
        Start-Sleep -Milliseconds 200
    } while ([DateTime]::UtcNow -lt $deadline)
    if ($app.MainWindowHandle -eq 0) { throw 'Janela nao encontrada.' }
    $window = [System.Windows.Automation.AutomationElement]::FromHandle($app.MainWindowHandle)
    function Find-Element($property, $value) {
        $condition = New-Object System.Windows.Automation.PropertyCondition($property, $value)
        $window.FindFirst([System.Windows.Automation.TreeScope]::Descendants, $condition)
    }
    function Wait-Element($property, $value) {
        $until = [DateTime]::UtcNow.AddSeconds(10)
        do {
            $element = Find-Element $property $value
            if ($null -ne $element) { return $element }
            Start-Sleep -Milliseconds 150
        } while ([DateTime]::UtcNow -lt $until)
        throw "Controle nao encontrado: $value"
    }
    foreach ($name in @('Pedaleira', 'IRs', 'Backing tracks', 'Criador de amps', 'Dispositivo e ajuda')) {
        $item = Find-Element ([System.Windows.Automation.AutomationElement]::NameProperty) $name
        if ($null -eq $item) { throw "Navegacao ausente: $name" }
        $pattern = $item.GetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern)
        $timer = [Diagnostics.Stopwatch]::StartNew()
        $pattern.Select()
        $deadline = [DateTime]::UtcNow.AddSeconds(5)
        do {
            Start-Sleep -Milliseconds 100
            $heading = Find-Element ([System.Windows.Automation.AutomationElement]::AutomationIdProperty) 'Heading'
            if ($null -ne $heading -and $heading.Current.Name -eq $name) { break }
        } while ([DateTime]::UtcNow -lt $deadline)
        if ($null -eq $heading -or $heading.Current.Name -ne $name) { throw "Pagina nao abriu: $name" }
        Write-Host ("{0}: {1:N0} ms (UIA, polling 100 ms)" -f $name, $timer.Elapsed.TotalMilliseconds)
    }
    $app.Refresh()
    Write-Host ("PrivateMB={0:N1}; CPUms={1:N0}" -f ($app.PrivateMemorySize64 / 1MB), $app.TotalProcessorTime.TotalMilliseconds)
} finally {
    if (!$app.HasExited) { $null = $app.CloseMainWindow(); $null = $app.WaitForExit(10000) }
}
