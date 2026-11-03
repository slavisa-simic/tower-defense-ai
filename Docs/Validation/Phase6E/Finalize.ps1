param([switch]$AfterFresh,[switch]$AfterObservation)
$ErrorActionPreference='Stop'
Set-Location (Split-Path (Split-Path (Split-Path $PSScriptRoot -Parent) -Parent) -Parent)
function Invoke-Editor([string[]]$CommandArgs) {
    $raw=(& unity command @CommandArgs --result-only) -join "`n"
    if($LASTEXITCODE -ne 0){throw $raw}
    $value=$raw|ConvertFrom-Json
    if($value -is [string] -and $value.TrimStart().StartsWith('{')){$value=$value|ConvertFrom-Json}
    if($value.PSObject.Properties.Name -contains 'success' -and !$value.success){throw ($value|ConvertTo-Json -Depth 8)}
    return [pscustomobject]@{raw=$raw;value=$value}
}
function Step([string]$name){@{step=$name;utc=[DateTime]::UtcNow.ToString('O')}|ConvertTo-Json|Set-Content -LiteralPath (Join-Path $PSScriptRoot 'finalization-status.json');Write-Output $name}
try {
    if($AfterObservation){
        Step 'Final strict scaled-viewport tooltip checks with explicit no-focus capture'
        $null=Invoke-Editor @('editor_stop')
        (Invoke-Editor @('console','--tail','150')).raw|Set-Content -LiteralPath (Join-Path $PSScriptRoot 'console-qa-ui-serialization.json')
        $null=Invoke-Editor @('clear_console')
        $null=Invoke-Editor @('editor_play')
        $observed=Invoke-Editor @('run_script','--file','Docs/Validation/Phase6E/Observe.cs','--entry','Phase6EObserve.EffectiveStats','--timeout_ms','120000','--timeout','120')
        $observed.raw|Set-Content -LiteralPath (Join-Path $PSScriptRoot 'effective-stats-final-envelope.json')
        $null=Invoke-Editor @('editor_stop')
    }else{
    if(!$AfterFresh){
    Step 'Waiting for final full PlayMode suite'
    do {$receipt=Invoke-Editor @('test_status');if($receipt.value.status -eq 'running'){Start-Sleep -Seconds 3}}while($receipt.value.status -eq 'running')
    $receipt.raw|Set-Content -LiteralPath (Join-Path $PSScriptRoot 'playmode-final.json')
    if($receipt.value.summary.total -ne 110 -or $receipt.value.summary.failed -ne 0){throw 'Full PlayMode acceptance failed; inspect playmode-final.json'}
    Step '110/110 PlayMode passed; archiving and clearing historical Console'
    (Invoke-Editor @('console','--tail','500')).raw|Set-Content -LiteralPath (Join-Path $PSScriptRoot 'console-before-final-walkthrough.json')
    $null=Invoke-Editor @('clear_console')
    Step 'Regenerating clean-profile product loop and per-wave CSV'
    $null=Invoke-Editor @('run_script','--file','Docs/Validation/Phase6E/Setup.cs','--entry','Phase6ESetup.Fresh')
    $null=Invoke-Editor @('editor_play')
    $fresh=Invoke-Editor @('run_script','--file','Docs/Validation/Phase6E/Simulation.cs','--entry','Phase6ESimulation.FreshLoop','--timeout_ms','360000','--timeout','360')
    $fresh.raw|Set-Content -LiteralPath (Join-Path $PSScriptRoot 'fresh-product-final-envelope.json')
    $null=Invoke-Editor @('editor_stop')
    Start-Sleep -Milliseconds 300
    $data=Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot 'fresh-product-loop.json')|ConvertFrom-Json
    $logs=Get-ChildItem -LiteralPath (Join-Path $data.directory 'BalanceLogs') -Filter '*.csv'
    foreach($file in $logs){Copy-Item -LiteralPath $file.FullName -Destination (Join-Path $PSScriptRoot ('fresh-'+$file.Name))}
    }else{
        Step 'Resuming after completed fresh product loop'
        $data=Get-Content -Raw -LiteralPath (Join-Path $PSScriptRoot 'fresh-product-loop.json')|ConvertFrom-Json
        $logs=Get-ChildItem -LiteralPath (Join-Path $data.directory 'BalanceLogs') -Filter '*.csv'
    }
    $runLog=$logs|Where-Object{@(Get-Content -LiteralPath $_.FullName).Count -gt 5}|Select-Object -First 1
    $rows=Import-Csv -LiteralPath $runLog.FullName
    $complete=@($rows|Where-Object{$_.result -eq 'WaveComplete'})
    if($complete.Count -ne 10 -or ($complete.wave -join ',') -ne '1,2,3,4,5,6,7,8,9,10'){throw 'Product CSV did not record ten distinct completed waves'}
    Step 'Reloading exact run and profile across Play Mode restart'
    $null=Invoke-Editor @('run_script','--file','Docs/Validation/Phase6E/Setup.cs','--entry','Phase6ESetup.Resume')
    $null=Invoke-Editor @('editor_play')
    $persist=Invoke-Editor @('run_script','--file','Docs/Validation/Phase6E/Simulation.cs','--entry','Phase6ESimulation.RestartPersistence','--timeout_ms','60000','--timeout','60')
    $persist.raw|Set-Content -LiteralPath (Join-Path $PSScriptRoot 'product-restart-final-envelope.json')
    $null=Invoke-Editor @('editor_stop')
    $null=Invoke-Editor @('run_script','--file','Docs/Validation/Phase6E/Setup.cs','--entry','Phase6ESetup.Finish')
    Step 'Observing final moving bosses, concurrent encounters and effective HUD stats'
    $null=Invoke-Editor @('editor_play')
    $observed=Invoke-Editor @('run_script','--file','Docs/Validation/Phase6E/Observe.cs','--entry','Phase6EObserve.FinalObservation','--timeout_ms','600000','--timeout','600')
    $observed.raw|Set-Content -LiteralPath (Join-Path $PSScriptRoot 'observation-final-envelope.json')
    $null=Invoke-Editor @('editor_stop')
    }
    Step 'Capturing final authored data and building Win64 release player'
    $null=Invoke-Editor @('run_script','--file','Docs/Validation/Phase6E/Snapshot.cs','--entry','Phase6ESnapshot.Final')
    $built=Invoke-Editor @('run_script','--file','Docs/Validation/Phase6E/Build.cs','--entry','Phase6EBuild.Run','--timeout_ms','1800000','--timeout','1800')
    $built.raw|Set-Content -LiteralPath (Join-Path $PSScriptRoot 'build-final-envelope.json')
    (Invoke-Editor @('console_status')).raw|Set-Content -LiteralPath (Join-Path $PSScriptRoot 'console-final-status.json')
    (Invoke-Editor @('console','--tail','300')).raw|Set-Content -LiteralPath (Join-Path $PSScriptRoot 'console-final.json')
    Step 'Final validation complete; review receipts and write acceptance report'
} catch {
    @{step='Failed';message=$_.Exception.Message;utc=[DateTime]::UtcNow.ToString('O')}|ConvertTo-Json -Depth 10|Set-Content -LiteralPath (Join-Path $PSScriptRoot 'finalization-status.json')
    throw
}
