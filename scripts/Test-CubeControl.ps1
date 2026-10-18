param(
    [string]$OutputRoot = 'C:\CubeControl-Validation\Tests',
    [string[]]$Suite = @(),
    [int]$TimeoutSeconds = 300,
    [switch]$SkipBuild
)
$ErrorActionPreference = 'Stop'
$repo = [IO.Path]::GetFullPath((Split-Path -Parent $PSScriptRoot))
$workspace = Join-Path $repo 'CubeControl-1.1\Cube\windows'
if (-not [IO.Path]::IsPathRooted($OutputRoot)) { throw 'OutputRoot must be absolute.' }
$OutputRoot = [IO.Path]::GetFullPath($OutputRoot).TrimEnd('\')
if ($OutputRoot -eq $repo -or $OutputRoot.StartsWith($repo + '\', [StringComparison]::OrdinalIgnoreCase)) { throw 'OutputRoot must be outside the repository.' }
if ($TimeoutSeconds -lt 1) { throw 'TimeoutSeconds must be positive.' }
$artifacts = Join-Path $OutputRoot 'artifacts'
$report = Join-Path $OutputRoot ('runs\' + [DateTime]::UtcNow.ToString('yyyyMMdd-HHmmss') + '-' + [guid]::NewGuid().ToString('N').Substring(0,8))
[IO.Directory]::CreateDirectory($report) | Out-Null
$lock = [IO.File]::Open((Join-Path $OutputRoot '.test-run.lock'), [IO.FileMode]::OpenOrCreate, [IO.FileAccess]::ReadWrite, [IO.FileShare]::None)
try {
if (-not $SkipBuild) {
    foreach ($project in 'Cube.Tests', 'Cube.Desktop.Tests') {
        & dotnet build (Join-Path $workspace "$project\$project.csproj") -c Release --artifacts-path $artifacts
        if ($LASTEXITCODE -ne 0) { throw "Compilation failed: $project" }
    }
}
$desktop = Join-Path $artifacts 'bin\Cube.Desktop.Tests\release\Cube.Desktop.Tests.exe'
$protocol = Join-Path $artifacts 'bin\Cube.Tests\release\Cube.Tests.exe'
if (-not (Test-Path -LiteralPath $desktop) -or -not (Test-Path -LiteralPath $protocol)) { throw 'Test executables are missing; run without SkipBuild.' }
$catalogText = & $desktop --list
if ($LASTEXITCODE -ne 0) { throw 'Could not list desktop suites.' }
$catalog = ($catalogText -join "`n") | ConvertFrom-Json
$catalog | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath (Join-Path $report 'catalog.json') -Encoding UTF8
foreach ($id in $Suite) { if ($id -notin @('protocol','qa-protocol','doc-links','doc-catalog') -and $id -notin $catalog.Id) { throw "Unknown suite: $id" } }
$results = [Collections.Generic.List[object]]::new()
$oldData = $env:CUBECONTROL_TEST_DATA
$oldTemp = $env:TEMP
$oldTmp = $env:TMP
try {
    $cases = @([pscustomobject]@{ Id = 'protocol'; Requirement = 'local' }, [pscustomobject]@{ Id = 'qa-protocol'; Requirement = 'local' }, [pscustomobject]@{ Id = 'doc-links'; Requirement = 'local' }, [pscustomobject]@{ Id = 'doc-catalog'; Requirement = 'local' }) + @($catalog)
    foreach ($case in $cases) {
        if ($Suite.Count -gt 0 -and $case.Id -notin $Suite) { continue }
        if ($case.Requirement -ne 'local') {
            $results.Add([pscustomobject]@{ Suite=$case.Id; Status='excluded'; Reason=$case.Requirement; Seconds=0; ExitCode=$null })
            continue
        }
        $sandbox = Join-Path $report ($case.Id + '-data')
        $temp = Join-Path $sandbox 'temp'
        [IO.Directory]::CreateDirectory($temp) | Out-Null
        $env:CUBECONTROL_TEST_DATA = $sandbox
        $env:TEMP = $temp; $env:TMP = $temp
        $log = Join-Path $report ($case.Id + '.log')
        $errors = Join-Path $report ($case.Id + '.errors.log')
        $exe = if ($case.Id -in @('protocol','qa-protocol')) { $protocol } else { $desktop }
        if ($case.Id -eq 'doc-links') { $exe = (Get-Command python -ErrorAction Stop).Source }
        if ($case.Id -eq 'doc-catalog') { $exe = (Get-Command powershell -ErrorAction Stop).Source }
        $parameters = @{ FilePath=$exe; WorkingDirectory=$repo; WindowStyle='Hidden'; PassThru=$true; RedirectStandardOutput=$log; RedirectStandardError=$errors }
        if ($case.Id -eq 'doc-links') { $parameters.ArgumentList = @(('"' + (Join-Path $repo 'scripts/qa/validate_document_links.py') + '"')) }
        elseif ($case.Id -eq 'doc-catalog') { $parameters.ArgumentList = @('-NoProfile', '-ExecutionPolicy', 'Bypass', '-File', ('"' + (Join-Path $repo 'scripts/Validate-Documentation.ps1') + '"')) }
        elseif ($case.Id -eq 'qa-protocol') { $parameters.ArgumentList = @('--qa', '--all', ('"' + (Join-Path $sandbox 'qa-results') + '"')) }
        elseif ($case.Id -ne 'protocol') { $parameters.ArgumentList = @('--suite', $case.Id) }
        $watch = [Diagnostics.Stopwatch]::StartNew()
        $process = Start-Process @parameters
        $null = $process.Handle
        $finished = $process.WaitForExit($TimeoutSeconds * 1000)
        if (-not $finished) { Stop-Process -Id $process.Id -Force; $status='timeout'; $code=$null }
        else { $process.WaitForExit(); $code=$process.ExitCode; $status=if ($code -eq 0) { 'passed' } else { 'failed' } }
        $watch.Stop()
        $results.Add([pscustomobject]@{ Suite=$case.Id; Status=$status; Reason=''; Seconds=[Math]::Round($watch.Elapsed.TotalSeconds,2); ExitCode=$code; Log=$log; Errors=$errors })
        $results | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath (Join-Path $report 'results.json') -Encoding UTF8
        Write-Host ($case.Id + ': ' + $status + ' (' + $watch.Elapsed.TotalSeconds.ToString('0.0') + 's)')
    }
} finally {
    $env:CUBECONTROL_TEST_DATA=$oldData; $env:TEMP=$oldTemp; $env:TMP=$oldTmp
    $results | ConvertTo-Json -Depth 6 | Set-Content -LiteralPath (Join-Path $report 'results.json') -Encoding UTF8
    Write-Host "Report: $report"
}
if (@($results | Where-Object { $_.Status -in 'failed','timeout' }).Count) { exit 1 }
exit 0

} finally { $lock.Dispose() }
