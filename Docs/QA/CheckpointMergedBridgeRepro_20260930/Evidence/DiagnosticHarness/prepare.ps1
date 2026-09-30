$ErrorActionPreference = 'Stop'
$root = 'C:\Users\田尻大翔\Documents\Codex\2026-08-07\new-chat\outputs\HimoHitoPrototype'
$qa = Join-Path $root 'Docs\QA\CheckpointMergedBridgeRepro_20260930\Evidence'
$copy = Join-Path $root '.codex_tmp\CheckpointMergedBridgeRepro_20260930\DiagnosticProject'
New-Item -ItemType Directory -Path $qa,$copy -Force | Out-Null
$files = @(foreach ($folder in @('Assets','Packages','ProjectSettings','output')) { Get-ChildItem -LiteralPath (Join-Path $root $folder) -File -Recurse })
$manifest = @($files | ForEach-Object { [pscustomobject]@{ path=$_.FullName.Substring($root.Length+1); sha256=(Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash } })
$manifest | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath (Join-Path $qa 'sha_start.json') -Encoding UTF8
if (-not (Test-Path -LiteralPath (Join-Path $copy 'Assets'))) {
 foreach ($folder in @('Assets','Packages','ProjectSettings')) { Copy-Item -LiteralPath (Join-Path $root $folder) -Destination $copy -Recurse }
}
# PNG encoder only; production manifest stays unchanged.
$reproManifestPath = Join-Path $copy 'Packages\manifest.json'
$reproManifest = [IO.File]::ReadAllText($reproManifestPath) | ConvertFrom-Json
$reproManifest.dependencies | Add-Member -NotePropertyName 'com.unity.modules.imageconversion' -NotePropertyValue '1.0.0' -Force
[IO.File]::WriteAllText($reproManifestPath, ($reproManifest | ConvertTo-Json -Depth 5), [Text.UTF8Encoding]::new($false))
# Mechanical, logging-only instrumentation in the isolated copy. No original source is written.
$edits = @{
 'RopePlatformBuilder.cs' = @(
 @('public PlatformState[] CapturePlatformStates()', 'CapturePlatformStates.enter'),
 @('public void RestorePlatformStates(IReadOnlyList<PlatformState> states)', 'RestorePlatformStates.enter'),
 @('public void ClearPlatforms()', 'ClearPlatforms.enter'),
 @('private void RestoreRemovedHooks()', 'RestoreRemovedHooks.enter')
 );
 'MainStageRespawnOnFall.cs' = @(
 @('private void CaptureCheckpointState()', 'CaptureCheckpointState.enter'),
 @('private void RestartFromCheckpoint()', 'RestartFromCheckpoint.enter')
 );
 'PrototypeRunController.cs' = @(
 @('private void CaptureCheckpointState()', 'CaptureCheckpointState.enter'),
 @('private void RestartFromCheckpoint()', 'RestartFromCheckpoint.enter')
 );
 'RopeController.cs' = @(@('public void DetachAndRefund(bool playReleaseSound = false)', 'DetachAndRefund.enter'),@('public void RestoreSelectedRopeLength(int length)', 'RestoreSelectedRopeLength.enter'));
 'RopeResource.cs' = @(,@('public void RestoreCurrentLength(float amount)', 'RestoreCurrentLength.enter'))
}
foreach ($file in $edits.Keys) {
 $path = Join-Path $copy ('Assets\Scripts\'+$file)
 $source = [IO.File]::ReadAllText((Join-Path $root ('Assets\Scripts\'+$file)))
 foreach ($edit in $edits[$file]) {
  $pattern = [regex]::Escape($edit[0])+'\s*\{'
  if (-not [regex]::IsMatch($source,$pattern)) { throw "Instrumentation target missing: $file $($edit[0])" }
  $source = [regex]::Replace($source,$pattern,('$0' + "`r`n            ReproTrace.Record(`"$($edit[1])`", gameObject);"),1)
 }
 if ($file -eq 'RopePlatformBuilder.cs') {
  $source = $source.Replace('removedHooks.Clear();', "removedHooks.Clear();`r`n            ReproTrace.Record(`"RestoreRemovedHooks.exit`", gameObject);")
  $source = $source.Replace('RopeBridgeBindings.Ensure(line);', "RopeBridgeBindings.Ensure(line);`r`n            ReproTrace.Record(`"CreatePlatform.exit`", gameObject);")
  $source = $source.Replace('hook.gameObject.SetActive(false);', "hook.gameObject.SetActive(false);`r`n            ReproTrace.Record(`"Merge.exit`", gameObject);")
  $source = $source.Replace('CreatePlatform(states[i].Start, states[i].End, states[i].RopeLength);', "CreatePlatform(states[i].Start, states[i].End, states[i].RopeLength);")
  $restoreEndPattern = '(CreatePlatform\(states\[i\]\.Start, states\[i\]\.End, states\[i\]\.RopeLength\);\s*\})(\s*\})'
  $source = [regex]::Replace($source, $restoreEndPattern, ('$1' + "`r`n            ReproTrace.Record(`"RestorePlatformStates.exit`", gameObject);" + '$2'), 1)
 }
 if ($file -eq 'MainStageRespawnOnFall.cs' -or $file -eq 'PrototypeRunController.cs') {
  $source = $source.Replace('checkpointPlatformStates = platformBuilder.CapturePlatformStates();', "checkpointPlatformStates = platformBuilder.CapturePlatformStates();`r`n            ReproTrace.Record(`"CaptureCheckpointState.exit`", gameObject);")
  $source = $source.Replace('RespawnWeaveVisual.Play(gameObject);', "RespawnWeaveVisual.Play(gameObject);`r`n            ReproTrace.Record(`"RestartFromCheckpoint.exit`", gameObject);")
  $source = $source.Replace('CurrentSection = Mathf.Clamp(sectionNumber, 1, 10);', "ReproTrace.Record(`"TryReachSection.accept`", gameObject);`r`n            CurrentSection = Mathf.Clamp(sectionNumber, 1, 10);")
  $source = $source.Replace('CurrentTutorialSection = sectionNumber;', "ReproTrace.Record(`"TryReachTutorialSection.accept`", gameObject);`r`n            CurrentTutorialSection = sectionNumber;")
 }
 [IO.File]::WriteAllText($path,$source,[Text.UTF8Encoding]::new($false))
}
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'ReproTrace.cs') -Destination (Join-Path $copy 'Assets\Scripts\ReproTrace.cs')
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'CheckpointMergedBridgeRepro.cs') -Destination (Join-Path $copy 'Assets\Editor\CheckpointMergedBridgeRepro.cs')
Write-Output "Prepared isolated project: $copy"
Write-Output "Protected files: $($manifest.Count)"
