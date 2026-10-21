param([string]$Executable = 'C:\CubeControl-WinUI-Build\publish\CubeControl.WinUI\CubeControl.WinUI.exe')
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName UIAutomationClient
Add-Type -AssemblyName UIAutomationTypes
Add-Type -AssemblyName System.Windows.Forms
$data = Join-Path ([IO.Path]::GetTempPath()) ('CubeWinUI-test-' + [Guid]::NewGuid().ToString('N'))
$app = Start-Process -FilePath $Executable -ArgumentList @('--offline', '--diagnostics', '--data-directory', ('"' + $data + '"')) -WorkingDirectory (Split-Path $Executable) -PassThru -WindowStyle Hidden
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
        $pattern.Select()
        $deadline = [DateTime]::UtcNow.AddSeconds(5)
        do {
            Start-Sleep -Milliseconds 100
            $heading = Find-Element ([System.Windows.Automation.AutomationElement]::AutomationIdProperty) 'Heading'
            if ($null -ne $heading -and $heading.Current.Name -eq $name) { break }
        } while ([DateTime]::UtcNow -lt $deadline)
        if ($null -eq $heading -or $heading.Current.Name -ne $name) { throw "Pagina nao abriu: $name" }
        $lists = $window.FindAll([System.Windows.Automation.TreeScope]::Descendants, (New-Object System.Windows.Automation.PropertyCondition([System.Windows.Automation.AutomationElement]::ControlTypeProperty, [System.Windows.Automation.ControlType]::List)))
        foreach ($list in $lists) {
            if ([string]::IsNullOrWhiteSpace($list.Current.Name)) { throw "Lista sem nome acessivel em $name" }
        }
        Write-Host "PASS: $name e nomes acessiveis das listas"
    }
    $refresh = Find-Element ([System.Windows.Automation.AutomationElement]::AutomationIdProperty) 'RefreshButton'
    $refresh.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern).Invoke()
    Start-Sleep -Seconds 4
    if (!$refresh.Current.IsEnabled) { throw 'Consulta MIDI nao finalizou.' }
    $status = Find-Element ([System.Windows.Automation.AutomationElement]::AutomationIdProperty) 'Status'
    if ([string]::IsNullOrWhiteSpace($status.Current.Name)) { throw 'Consulta sem resultado acessivel.' }
    Write-Host "PASS: consulta MIDI - $($status.Current.Name)"
    # Exercise the actual keyboard handler, not just programmatic focus.
    $device = Find-Element ([System.Windows.Automation.AutomationElement]::NameProperty) 'Dispositivo e ajuda'
    $device.SetFocus()
    [System.Windows.Forms.SendKeys]::SendWait('{F6}')
    Start-Sleep -Milliseconds 250
    $contentFocus = [System.Windows.Automation.AutomationElement]::FocusedElement
    if ($contentFocus.Current.Name -eq 'Dispositivo e ajuda') { throw 'F6 nao entrou no conteudo.' }
    [System.Windows.Forms.SendKeys]::SendWait('{F6}')
    Start-Sleep -Milliseconds 250
    $transportFocus = [System.Windows.Automation.AutomationElement]::FocusedElement
    if ($transportFocus.Current.AutomationId -notin @('PlaybackPlay', 'PlaybackStop')) { throw "F6 nao entrou no transporte: $($transportFocus.Current.Name)" }
    [System.Windows.Forms.SendKeys]::SendWait('+{F6}')
    Start-Sleep -Milliseconds 250
    if ([System.Windows.Automation.AutomationElement]::FocusedElement.Current.AutomationId -in @('PlaybackPlay', 'PlaybackStop')) { throw 'Shift+F6 nao retornou ao conteudo.' }
    Write-Host 'PASS: F6 e Shift+F6 entre regioes da janela.' 
    $backing = Find-Element ([System.Windows.Automation.AutomationElement]::NameProperty) 'Backing tracks'
    $backing.GetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern).Select()
    Start-Sleep -Milliseconds 400
    $add = Wait-Element ([System.Windows.Automation.AutomationElement]::AutomationIdProperty) 'AddEmptyStem'
    $add.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern).Invoke()
    Start-Sleep -Milliseconds 500
    $name = Wait-Element ([System.Windows.Automation.AutomationElement]::NameProperty) 'Nome da pista'
    if ($null -eq $name) { throw 'Nova pista nao apresentou seus controles.' }
    $name.GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern).SetValue('Pista WinUI persistida')
    $tuning = Find-Element ([System.Windows.Automation.AutomationElement]::NameProperty) 'Afinação da guitarra'
    $tuning.GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern).SetValue('Drop D')
    $bpm = Wait-Element ([System.Windows.Automation.AutomationElement]::NameProperty) 'BPM original'
    $bpm.GetCurrentPattern([System.Windows.Automation.RangeValuePattern]::Pattern).SetValue(123)
    Start-Sleep -Seconds 2
    $saved = Get-ChildItem -LiteralPath $data -Recurse -Filter '*.json' | Where-Object { (Get-Content -LiteralPath $_.FullName -Raw) -match 'Pista WinUI persistida' }
    if (!$saved) { throw 'Edicao nativa nao chegou ao arquivo persistido.' }
    $json = Get-Content -LiteralPath @($saved)[0].FullName -Raw | ConvertFrom-Json
    if ($json.Version -ne 5 -or $json.Sessions[0].Tuning -ne 'Drop D' -or $json.Sessions[0].Bpm -ne 123) { throw 'Metadados da sessao nao foram preservados.' }
    Write-Host 'PASS: criar pista, editar nome/afinacao por UI Automation e salvar sessao v5.'
    $amps = Find-Element ([System.Windows.Automation.AutomationElement]::NameProperty) 'Criador de amps'
    $amps.GetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern).Select()
    Start-Sleep -Milliseconds 400
    $load = Wait-Element ([System.Windows.Automation.AutomationElement]::NameProperty) 'Carregar modelo local'
    $load.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern).Invoke()
    Start-Sleep -Seconds 2
    $export = Wait-Element ([System.Windows.Automation.AutomationElement]::NameProperty) 'Exportar amp'
    if (!$export.Current.IsEnabled) { throw 'Modelo local nao habilitou a edicao/exportacao de amp.' }
    $gain = Wait-Element ([System.Windows.Automation.AutomationElement]::NameProperty) 'Ganho'
    $range = $gain.GetCurrentPattern([System.Windows.Automation.RangeValuePattern]::Pattern)
    $range.SetValue([Math]::Min($range.Current.Maximum, $range.Current.Value + 10))
    $null = Wait-Element ([System.Windows.Automation.AutomationElement]::NameProperty) 'Modificado localmente'
    Write-Host 'PASS: carregar modelo de amp local e habilitar exportacao.'
    $irs = Find-Element ([System.Windows.Automation.AutomationElement]::NameProperty) 'IRs'
    $irs.GetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern).Select()
    $newPack = Wait-Element ([System.Windows.Automation.AutomationElement]::AutomationIdProperty) 'NewPack'
    $newPack.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern).Invoke()
    $packName = Wait-Element ([System.Windows.Automation.AutomationElement]::NameProperty) 'Nome do pack'
    $packName.GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern).SetValue('Rascunho WinUI')
    $backing.GetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern).Select()
    $null = Wait-Element ([System.Windows.Automation.AutomationElement]::AutomationIdProperty) 'AddEmptyStem'
    $irs.GetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern).Select()
    $packName = Wait-Element ([System.Windows.Automation.AutomationElement]::NameProperty) 'Nome do pack'
    if ($packName.GetCurrentPattern([System.Windows.Automation.ValuePattern]::Pattern).Current.Value -ne 'Rascunho WinUI') { throw 'Navegar perdeu o rascunho do pack.' }
    $packName.SetFocus()
    $window.GetCurrentPattern([System.Windows.Automation.WindowPattern]::Pattern).Close()
    $cancel = Wait-Element ([System.Windows.Automation.AutomationElement]::NameProperty) 'Cancelar'
    $cancel.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern).Invoke()
    Start-Sleep -Seconds 1
    $app.Refresh()
    if ($app.HasExited) { throw 'Cancelar fechamento descartou a janela.' }
    if ([System.Windows.Automation.AutomationElement]::FocusedElement.Current.Name -ne 'Nome do pack') { throw 'Cancelar dialogo nao restaurou o foco ao campo do pack.' }
    Write-Host 'PASS: retorno de foco apos cancelar dialogo.'
    $window.GetCurrentPattern([System.Windows.Automation.WindowPattern]::Pattern).Close()
    $discard = Wait-Element ([System.Windows.Automation.AutomationElement]::NameProperty) 'Descartar'
    $discard.GetCurrentPattern([System.Windows.Automation.InvokePattern]::Pattern).Invoke()
    if (!$app.WaitForExit(5000)) { throw 'Host nao encerrou normalmente.' }
    if ($app.ExitCode -ne 0) { throw "Host encerrou com erro: $($app.ExitCode)" }
    Write-Host 'PASS: rascunho preservado ao navegar, cancelar fechamento e descartar explicitamente.'
    Write-Host 'PASS: inicializacao, cinco rotas, MIDI, sessao persistida, modelo de amp e encerramento.'
} finally {
    if (!$app.HasExited) { $null = $app.CloseMainWindow() }
}
