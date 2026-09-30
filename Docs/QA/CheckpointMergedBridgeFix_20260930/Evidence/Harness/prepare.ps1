$ErrorActionPreference='Stop'
$root='C:\Users\田尻大翔\Documents\Codex\2026-08-07\new-chat\outputs\HimoHitoPrototype'
$copy=Join-Path $PSScriptRoot 'DiagnosticProject'
if(Test-Path -LiteralPath $copy) { throw 'Fresh diagnostic directory already exists' }
New-Item -ItemType Directory -Path $copy | Out-Null
foreach($folder in @('Assets','Packages','ProjectSettings')) { Copy-Item -LiteralPath (Join-Path $root $folder) -Destination $copy -Recurse }
$manifestPath=Join-Path $copy 'Packages\manifest.json'
$manifest=[IO.File]::ReadAllText($manifestPath) | ConvertFrom-Json
$manifest.dependencies | Add-Member -NotePropertyName 'com.unity.modules.imageconversion' -NotePropertyValue '1.0.0'
[IO.File]::WriteAllText($manifestPath,($manifest | ConvertTo-Json -Depth 5),[Text.UTF8Encoding]::new($false))
foreach($file in @('MainStageRespawnOnFall.cs','PrototypeRunController.cs')) {
 $path=Join-Path $copy ('Assets\Scripts\'+$file)
 $source=[IO.File]::ReadAllText($path)
 foreach($method in @('CaptureCheckpointState','RestartFromCheckpoint')) {
  $source=[regex]::Replace($source,('private void '+$method+'\(\)\s*\{'),('$0'+"`r`n            FixTrace.Record(`"$method.enter`", gameObject);"),1)
 }
 $line='checkpointRemovedHooks = platformBuilder.CaptureRemovedHooks();'
 $source=$source.Replace($line,$line+"`r`n            FixTrace.Record(`"CaptureCheckpointState.exit`", gameObject);")
 $line='platformBuilder.RestorePlatformStates(checkpointPlatformStates, checkpointRemovedHooks);'
 $source=$source.Replace($line,$line+"`r`n            FixTrace.Record(`"RestorePlatformStates.exit`", gameObject);")
 $source=$source.Replace('RespawnWeaveVisual.Play(gameObject);',"RespawnWeaveVisual.Play(gameObject);`r`n            FixTrace.Record(`"RestartFromCheckpoint.exit`", gameObject);")
 [IO.File]::WriteAllText($path,$source,[Text.UTF8Encoding]::new($false))
}
$path=Join-Path $copy 'Assets\Scripts\RopePlatformBuilder.cs'
$source=[IO.File]::ReadAllText($path)
foreach($method in @('ClearPlatforms','RestoreRemovedHooks')) {
 $source=[regex]::Replace($source, ('(public|private) void '+$method+'\(\)\s*\{'), ('$0'+"`r`n            FixTrace.Record(`"$method.enter`", gameObject);"),1)
}
$source=$source.Replace('removedHooks.Clear();',"removedHooks.Clear();`r`n            FixTrace.Record(`"RestoreRemovedHooks.exit`", gameObject);")
[IO.File]::WriteAllText($path,$source,[Text.UTF8Encoding]::new($false))
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'FixTrace.cs') -Destination (Join-Path $copy 'Assets\Scripts\FixTrace.cs')
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'CheckpointMergedBridgeFix.cs') -Destination (Join-Path $copy 'Assets\Editor\CheckpointMergedBridgeFix.cs')
Write-Output $copy
