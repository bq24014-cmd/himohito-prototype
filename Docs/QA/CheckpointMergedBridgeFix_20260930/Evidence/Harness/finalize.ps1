$ErrorActionPreference='Stop'
$root='C:\Users\田尻大翔\Documents\Codex\2026-08-07\new-chat\outputs\HimoHitoPrototype'
$evidence=Join-Path $root 'Docs\QA\CheckpointMergedBridgeFix_20260930\Evidence'
$allowed=@('Assets\Scripts\RopePlatformBuilder.cs','Assets\Scripts\MainStageRespawnOnFall.cs','Assets\Scripts\PrototypeRunController.cs')
$start=Get-Content -LiteralPath (Join-Path $evidence 'sha_start.json') -Raw | ConvertFrom-Json
$comparison=@(foreach($entry in $start) {
 $path=Join-Path $root $entry.path
 $sha=if(Test-Path -LiteralPath $path) { (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash } else { 'MISSING' }
 [pscustomobject]@{ path=$entry.path; start=$entry.sha256; end=$sha; unchanged=($entry.sha256 -eq $sha); allowedSource=($allowed -contains $entry.path) }
})
$comparison | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath (Join-Path $evidence 'sha_end_comparison.json') -Encoding UTF8
$unexpected=@($comparison | Where-Object { !$_.unchanged -and !$_.allowedSource })
$expected=@($comparison | Where-Object { !$_.unchanged -and $_.allowedSource })
if($unexpected.Count -ne 0 -or $expected.Count -ne 3) { throw 'File protection audit failed' }
$scenes=@(Get-ChildItem -LiteralPath (Join-Path $root 'Assets\Scenes') -File -Recurse | ForEach-Object {
 $rel=$_.FullName.Substring($root.Length+1)
 [pscustomobject]@{ path=$rel; production=(Get-FileHash -LiteralPath $_.FullName).Hash; diagnostic=(Get-FileHash -LiteralPath (Join-Path $PSScriptRoot ('DiagnosticProject\'+$rel))).Hash }
})
if(@($scenes | Where-Object { $_.production -ne $_.diagnostic }).Count) { throw 'Diagnostic Scene mismatch' }
$scenes | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $evidence 'scene_copy_sha.json') -Encoding UTF8
$harness=Join-Path $evidence 'Harness'
New-Item -ItemType Directory -Path $harness -Force | Out-Null
foreach($name in @('FixTrace.cs','CheckpointMergedBridgeFix.cs','prepare.ps1','baseline.ps1','finalize.ps1')) {
 Copy-Item -LiteralPath (Join-Path $PSScriptRoot $name) -Destination $harness
}
foreach($name in @('RopePlatformBuilder.cs','MainStageRespawnOnFall.cs','PrototypeRunController.cs')) {
 $original=Join-Path $root ('Assets\Scripts\'+$name)
 $instrumented=Join-Path $PSScriptRoot ('DiagnosticProject\Assets\Scripts\'+$name)
 & git diff --no-index -- $original $instrumented | Set-Content -LiteralPath (Join-Path $harness ($name+'.instrumentation.diff')) -Encoding UTF8
}
& git diff -- Assets/Scripts/RopePlatformBuilder.cs Assets/Scripts/MainStageRespawnOnFall.cs Assets/Scripts/PrototypeRunController.cs | Set-Content -LiteralPath (Join-Path $evidence 'source.diff') -Encoding UTF8
& git diff --check -- Assets/Scripts/RopePlatformBuilder.cs Assets/Scripts/MainStageRespawnOnFall.cs Assets/Scripts/PrototypeRunController.cs
if($LASTEXITCODE -ne 0) { throw 'Source diff check failed' }
$fullCheck=@(& git diff --check 2>&1)
$fullExit=$LASTEXITCODE
$fullCheck | Set-Content -LiteralPath (Join-Path $evidence 'full_diff_check.txt') -Encoding UTF8
$cutoff=(Get-Item -LiteralPath (Join-Path $evidence 'sha_start.json')).LastWriteTime
$legacyTmp=@(Get-ChildItem -LiteralPath (Join-Path $root '.codex_tmp') -Recurse -File | Where-Object { !$_.FullName.StartsWith($PSScriptRoot+'\') -and !$_.FullName.Contains('\CheckpointMergedBridgeRepro_20260930\') } | ForEach-Object { [pscustomobject]@{ path=$_.FullName.Substring($root.Length+1); lastWrite=$_.LastWriteTime.ToString('o'); newerThanStart=($_.LastWriteTime -gt $cutoff); sha256=(Get-FileHash -LiteralPath $_.FullName).Hash } })
$legacyTmp | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath (Join-Path $evidence 'other_legacy_tmp_end_inventory.json') -Encoding UTF8
$audit=[pscustomobject]@{ baselineFiles=$comparison.Count; unchanged=@($comparison | Where-Object unchanged).Count; changedSource=$expected.path; unexpectedChanges=$unexpected.Count; diagnosticSceneMatch=$true; sourceDiffCheck='PASS'; fullDiffCheckExit=$fullExit; otherLegacyTmpFiles=$legacyTmp.Count; otherLegacyTmpNewerFiles=@($legacyTmp | Where-Object newerThanStart).Count; branch=(& git branch --show-current); head=(& git rev-parse HEAD); originMain=(& git rev-parse origin/main); staged=@(& git diff --cached --name-only); status=@(& git status --short) }
$audit | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $evidence 'git_and_protection_end.json') -Encoding UTF8
$issues=@(foreach($case in @('Merged','Unmerged','Normal','Tutorial')) {
 $log=Join-Path $evidence ($case+'_runtime.log')
 [pscustomobject]@{ test=$case; result=(Get-Content -LiteralPath (Join-Path $evidence ($case+'\result.txt')) -Raw).Trim(); exceptions=@(Select-String -LiteralPath $log -Pattern 'Exception:' | ForEach-Object { $_.Line }); compilerErrors=@(Select-String -LiteralPath $log -Pattern 'error CS' | ForEach-Object { $_.Line }); compilerWarnings=@(Select-String -LiteralPath $log -Pattern 'warning CS' | ForEach-Object { $_.Line }); nullReference=@(Select-String -LiteralPath $log -Pattern 'NullReferenceException' | ForEach-Object { $_.Line }); licensing=@(Select-String -LiteralPath $log -Pattern 'Access token is unavailable' | ForEach-Object { $_.Line }) }
})
$issues | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $evidence 'runtime_log_audit.json') -Encoding UTF8
$audit | ConvertTo-Json -Depth 5
