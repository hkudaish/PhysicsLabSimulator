param([Parameter(Mandatory=$true)][string]$Executable)
$ErrorActionPreference = 'Stop'
$results = $env:STEM_LAB_RESULTS
$ready = Join-Path $results 'close-ready.txt'
if (Test-Path $ready) { Remove-Item $ready }
$lab = Start-Process -FilePath $Executable -ArgumentList @('--','--close-check','--expect-export') -PassThru `
    -RedirectStandardOutput (Join-Path $results 'close-stdout.log') `
    -RedirectStandardError (Join-Path $results 'close-stderr.log')
$null = $lab.Handle
try {
    $deadline = [DateTime]::UtcNow.AddSeconds(30)
    while (!(Test-Path $ready)) {
        if ($lab.HasExited) { throw 'Prototype exited before preparing the close test.' }
        if ([DateTime]::UtcNow -gt $deadline) { throw 'Window-close preparation timed out.' }
        Start-Sleep -Milliseconds 100
        $lab.Refresh()
    }
    $lab.Refresh()
    if (!$lab.CloseMainWindow()) { throw 'The owned prototype window rejected WM_CLOSE.' }
    if (!$lab.WaitForExit(20000)) { throw 'Save-before-close did not finish within 20 seconds.' }
    if ($lab.ExitCode -ne 0) { throw "Prototype failed with exit code $($lab.ExitCode)." }
    @{success=$true;trigger='WM_CLOSE sent to the process created by this test';freshProcessResumeRequired=$true} |
        ConvertTo-Json | Set-Content -Encoding UTF8 (Join-Path $results 'window-close-result.json')
} finally {
    $lab.Refresh()
    if (!$lab.HasExited) { Stop-Process -Id $lab.Id -Force }
}
