<#
Offline QA launcher. No selection means list only. Reports certify automated cases, never entire batteries.
Requires Windows PowerShell 5.1+ and .NET 10; desktop build also requires the existing MSVC toolchain.
#>
[CmdletBinding()]
param(
    [string[]]$Battery = @(),
    [switch]$List,
    [switch]$AllOffline,
    [switch]$Usability,
    [switch]$IncludeDependencies,
    [switch]$NoBuild,
    [ValidateSet('Both','Protocol','Desktop')][string]$HostKind = 'Both',
    [string]$OutputRoot = 'C:\CubeControl-QA',
    [ValidateRange(10,7200)][int]$TimeoutSeconds = 900,
    [switch]$AnalyzeAudio,
    [string]$Python = 'python'
)
$ErrorActionPreference = 'Stop'
$repo = [IO.Path]::GetFullPath((Split-Path -Parent $PSScriptRoot))
$windows = Join-Path $repo 'CubeControl-1.1\Cube\windows'
$plan = Get-Content -Raw -Encoding UTF8 -LiteralPath (Join-Path $repo 'QA_TEST_PLAN.md')
$catalog = @{}
foreach ($line in ($plan -split '\r?\n')) {
    if ($line -match '^\| \[ \] \| (QA-[A-Z]+-\d{3}) \|') {
        $cells = $line.Split('|') | ForEach-Object { $_.Trim() }
        $catalog[$cells[2]] = [pscustomobject]@{
            Id=$cells[2]; Name=$cells[3]; Priority=$cells[5]; Dependencies=@($cells[8] -split ', ' | Where-Object { $_ -match '^QA-' });
            Availability=$cells[9]; Cases=@(); Coverage='MANUAL_OR_NOT_IMPLEMENTED'
        }
    }
}
if ($catalog.Count -eq 0) { throw 'QA_TEST_PLAN index not found.' }
$hosts = @(
    [pscustomobject]@{Name='Protocol'; Project='Cube.Tests'; Source='QaProtocol.cs'},
    [pscustomobject]@{Name='Desktop'; Project='Cube.Desktop.Tests'; Source='QaDesktop.cs'}
)
if ($Usability) {
    if ($HostKind -eq 'Protocol') { throw 'Usability requires the Desktop host.' }
    $HostKind = 'Desktop'
    $hosts = @([pscustomobject]@{Name='Desktop'; Project='Cube.Desktop.Tests'; Source='QaUsability.cs'})
}
# Registrations are deliberately declarative, one Test(id,name,batteries,...) per case.
foreach ($hostSpec in $hosts) {
    $source = Get-Content -Raw -Encoding UTF8 -LiteralPath (Join-Path $windows ($hostSpec.Project + '\' + $hostSpec.Source))
    foreach ($match in [regex]::Matches($source, 'Test\("([a-z0-9-]+)", "([^"]+)", "(QA-[A-Z0-9,-]+)"')) {
        foreach ($id in $match.Groups[3].Value.Split(',')) {
            if (-not $catalog.ContainsKey($id)) { throw "Unknown registered battery: $id" }
            $catalog[$id].Cases += ($hostSpec.Name + ':' + $match.Groups[1].Value)
            $catalog[$id].Coverage = 'AUTOMATED_SUBSET'
        }
    }
}
if ($List -or (-not $AllOffline -and $Battery.Count -eq 0)) {
    $catalog.Values | Sort-Object Id | Select-Object Id,Priority,Coverage,@{n='Cases';e={$_.Cases.Count}},Name | Format-Table -AutoSize
    return
}
if ($AllOffline -and $Battery.Count -gt 0) { throw 'Use -Battery or -AllOffline, not both.' }
$selected = New-Object 'System.Collections.Generic.HashSet[string]' ([StringComparer]::Ordinal)
if ($AllOffline) { foreach ($item in $catalog.Values) { if ($item.Cases.Count -gt 0) { [void]$selected.Add($item.Id) } } }
else { foreach ($value in $Battery) { foreach ($id in $value.Split(',')) { if (-not $catalog.ContainsKey($id.Trim())) { throw "Unknown battery: $id" }; [void]$selected.Add($id.Trim()) } } }
if ($IncludeDependencies) {
    do { $changed=$false; foreach ($id in @($selected)) { foreach ($dep in $catalog[$id].Dependencies) { if ($selected.Add($dep)) { $changed=$true } } } } while ($changed)
}
$OutputRoot = [IO.Path]::GetFullPath($OutputRoot).TrimEnd('\','/')
if ($OutputRoot -eq $repo -or $OutputRoot.StartsWith($repo + '\', [StringComparison]::OrdinalIgnoreCase)) { throw 'Use an output directory outside the repository.' }
foreach ($path in @($OutputRoot,$windows)) { if ($path -match '["\r\n]') { throw 'Quotes/newlines in paths are unsupported.' } }
$runRoot = Join-Path $OutputRoot ('runs\' + (Get-Date -Format 'yyyyMMdd-HHmmss') + '-' + [Guid]::NewGuid().ToString('N').Substring(0,8))
[void][IO.Directory]::CreateDirectory($runRoot)
$buildRoot = Join-Path $OutputRoot 'build'
$runHosts = @($hosts | Where-Object { $HostKind -eq 'Both' -or $_.Name -eq $HostKind })
$hostResults = @(); $failed=$false; $incomplete=$false

function Run-Process([string]$Executable, [string[]]$Arguments, [string]$LogBase) {
    foreach ($argument in $Arguments) { if ($argument -match '["\r\n]' -or $argument.EndsWith('\')) { throw 'Unsupported process argument.' } }
    $quoted = ($Arguments | ForEach-Object { '"' + $_ + '"' }) -join ' '
    try { $process = Start-Process -FilePath $Executable -ArgumentList $quoted -WindowStyle Hidden -PassThru -RedirectStandardOutput ($LogBase + '.stdout.log') -RedirectStandardError ($LogBase + '.stderr.log') }
    catch { $_.ToString() | Set-Content -Encoding UTF8 -LiteralPath ($LogBase + '.launch-error.log'); return 127 }
    $processHandle = $process.Handle # Keep the process handle alive so ExitCode survives process termination.
    if (-not $process.WaitForExit($TimeoutSeconds * 1000)) {
        # The test host has no physical device handles; terminate its process tree on a hang.
        & taskkill.exe /PID $process.Id /T /F | Out-Null
        return 124
    }
    $process.WaitForExit(); $process.Refresh()
    return $process.ExitCode
}

foreach ($hostSpec in $runHosts) {
    $ids = @($selected | Where-Object { @($catalog[$_].Cases | Where-Object { $_.StartsWith($hostSpec.Name + ':') }).Count -gt 0 } | Sort-Object)
    if ($ids.Count -eq 0) { continue }
    $code = 0
    if (-not $NoBuild) {
        $project = Join-Path $windows ($hostSpec.Project + '\' + $hostSpec.Project + '.csproj')
        Write-Host ('Building ' + $hostSpec.Project)
        $code = Run-Process 'dotnet' @('build',$project,('-p:ArtifactsPath=' + $buildRoot),'--nologo','-v:minimal') (Join-Path $runRoot ($hostSpec.Name + '-build'))
    }
    $dll = Join-Path $buildRoot ('bin\' + $hostSpec.Project + '\debug\' + $hostSpec.Project + '.dll')
    $resultPath = Join-Path $runRoot $hostSpec.Name
    if ($code -eq 0 -and -not (Test-Path -LiteralPath $dll)) { $code=2 }
    if ($code -eq 0) {
        Write-Host ('Running offline cases: ' + $hostSpec.Name + ' (' + ($ids -join ', ') + ')')
        $testArgs = @($dll,'--qa','--batteries',($ids -join ','),'--results',$resultPath)
        if ($Usability) { $testArgs += '--usability' }
        $code = Run-Process 'dotnet' $testArgs (Join-Path $runRoot $hostSpec.Name)
    }
    if ($code -ne 0) { $failed=$true }
    $hostResults += [pscustomobject]@{Host=$hostSpec.Name;ExitCode=$code;Results=$resultPath}
}
$allCases = @()
foreach ($hostResult in $hostResults) {
    $jsonPath = Join-Path $hostResult.Results 'results.json'
    if (Test-Path -LiteralPath $jsonPath) {
        $report = Get-Content -Raw -Encoding UTF8 -LiteralPath $jsonPath | ConvertFrom-Json
        foreach ($case in $report.Cases) { $case | Add-Member NoteProperty Host $hostResult.Host; $allCases += $case }
    }
}
$batteryResults = @()
foreach ($id in ($selected | Sort-Object)) {
    $cases = @($allCases | Where-Object { $_.Batteries -contains $id })
    $expected = @($catalog[$id].Cases | Where-Object { $prefix=$_.Split(':')[0]; $HostKind -eq 'Both' -or $prefix -eq $HostKind })
    $status = if ($cases.Status -contains 'FAIL') { 'AUTOMATED_SUBSET_FAILED' }
        elseif ($expected.Count -eq 0) { 'NOT_RUN_MANUAL_OR_UNIMPLEMENTED' }
        elseif ($cases.Count -lt $expected.Count) { 'AUTOMATED_SUBSET_INCOMPLETE' }
        else { 'AUTOMATED_SUBSET_PASSED' }
    if ($status -in 'NOT_RUN_MANUAL_OR_UNIMPLEMENTED','AUTOMATED_SUBSET_INCOMPLETE') { $incomplete=$true }
    $batteryResults += [pscustomobject]@{Id=$id;Status=$status;ExpectedCases=$expected.Count;ExecutedCases=$cases.Count;
        Dependencies=$catalog[$id].Dependencies;UnselectedDependencies=@($catalog[$id].Dependencies | Where-Object { -not $selected.Contains($_) });
        Remaining='Manual, hardware and unimplemented scenarios from QA_TEST_PLAN remain unverified.'}
}
$audioCode = $null
if ($AnalyzeAudio) {
    $manifest = Join-Path $runRoot 'Desktop\native-dsp\audio-manifest.json'
    if (Test-Path -LiteralPath $manifest) {
        $audioCode = Run-Process $Python @((Join-Path $PSScriptRoot 'qa\analyze_audio.py'),'--manifest',$manifest,'--output',(Join-Path $runRoot 'audio-analysis.json')) (Join-Path $runRoot 'audio-analysis')
        if ($audioCode -ne 0) { $failed=$true }
    } else { $incomplete=$true; Write-Warning 'Audio analysis requires the desktop QA-DSP-003 cases.' }
}
$commit = (& git -C $repo rev-parse HEAD 2>$null) -join ''
$summary = [pscustomobject]@{Schema=1;CreatedUtc=[DateTimeOffset]::UtcNow.ToString('o');Commit=$commit;HardwareAccess=$false;
    Scope='Automated subsets; no whole-battery PASS/FAIL';Usability=[bool]$Usability;HostKind=$HostKind;Hosts=$hostResults;Batteries=$batteryResults;AudioAnalysisExitCode=$audioCode}
$summary | ConvertTo-Json -Depth 10 | Set-Content -Encoding UTF8 -LiteralPath (Join-Path $runRoot 'summary.json')
$lines = @('# QA execution report','','Automated subsets only. Hardware and human validation remain pending.','','| Battery | Result | Cases |','| --- | --- | --- |')
foreach ($item in $batteryResults) { $lines += ('| ' + $item.Id + ' | ' + $item.Status + ' | ' + $item.ExecutedCases + '/' + $item.ExpectedCases + ' |') }
$lines += @('', '## Execution', '')
foreach ($item in $hostResults) { $lines += ('- ' + $item.Host + ' process exit code: ' + $item.ExitCode) }
if ($AnalyzeAudio) { $lines += ('- Independent audio analysis exit code: ' + $audioCode + ' (see audio-analysis.json and audio-analysis logs).') }
$lines += @('', 'Dependency selection does not certify prerequisites or impose battery execution order. Cases use independent fixtures.', 'NoBuild uses existing binaries; source-hashes.json describes this checkout and does not prove binary/source equivalence.')
$lines | Set-Content -Encoding UTF8 -LiteralPath (Join-Path $runRoot 'summary.md')
# Source hashes identify the dirty checkout, not just HEAD. Never record personal application data.
$sourceHashes = Get-ChildItem -LiteralPath $windows -Recurse -File | Where-Object { $_.FullName -notmatch '\\(obj|bin)\\' -and $_.Extension -in '.cs','.xaml','.csproj','.cpp','.h','.props','.targets' } | Get-FileHash -Algorithm SHA256 | Select-Object Path,Hash
$sourceHashes | ConvertTo-Json | Set-Content -Encoding UTF8 -LiteralPath (Join-Path $runRoot 'source-hashes.json')
Write-Host ('Report: ' + (Join-Path $runRoot 'summary.md'))
if ($failed) { exit 1 }; if ($incomplete) { exit 2 }; exit 0

