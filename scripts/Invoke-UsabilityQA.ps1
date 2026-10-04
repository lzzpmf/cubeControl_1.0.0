<# Real WPF windows, simulated devices. Run in an unlocked interactive Windows session. #>
[CmdletBinding()]
param(
    [switch]$List,
    [string[]]$Battery = @(),
    [switch]$NoBuild,
    [string]$OutputRoot = 'C:\CubeControl-UsabilityQA',
    [ValidateRange(10,7200)][int]$TimeoutSeconds = 180
)
$options = @{ Usability=$true; OutputRoot=$OutputRoot; TimeoutSeconds=$TimeoutSeconds; NoBuild=$NoBuild }
if ($List) { $options.List=$true }
elseif ($Battery.Count -gt 0) { $options.Battery=$Battery }
else { $options.AllOffline=$true }
& (Join-Path $PSScriptRoot 'Invoke-QA.ps1') @options
exit $LASTEXITCODE
