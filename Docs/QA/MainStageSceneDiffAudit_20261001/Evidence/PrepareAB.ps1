$ErrorActionPreference='Stop'
$taskRoot=(Get-Location).Path
$taskEvidence=$PSScriptRoot
$taskProject=Join-Path $taskRoot '.codex_tmp/MainStageSceneDiffAudit_20261001/Project'
if(Test-Path -LiteralPath $taskProject){throw 'New isolated destination required.'}
New-Item -ItemType Directory -Path $taskProject | Out-Null
foreach($folder in @('Assets','Packages','ProjectSettings')){Copy-Item -LiteralPath (Join-Path $taskRoot $folder) -Destination $taskProject -Recurse}
$taskManifest=@()
foreach($folder in @('Assets','Packages','ProjectSettings')){
 foreach($file in Get-ChildItem -LiteralPath (Join-Path $taskProject $folder) -File -Recurse){
  $rel=$file.FullName.Substring($taskProject.Length+1)
  $taskManifest+=[pscustomobject]@{path=$rel;sha256=(Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash}
 }
}
$taskManifest | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath (Join-Path $taskEvidence 'isolated_input_manifest.json') -Encoding UTF8
Copy-Item -LiteralPath (Join-Path $taskEvidence 'SceneAuditEntry.cs') -Destination (Join-Path $taskProject 'Assets/Editor/SceneAuditEntry.cs')
Copy-Item -LiteralPath (Join-Path $taskEvidence 'HEAD_MainStage.unity') -Destination (Join-Path $taskProject 'Assets/Scenes/MainStage.unity')
Write-Output ('ISOLATED_PROJECT_READY '+$taskProject)
