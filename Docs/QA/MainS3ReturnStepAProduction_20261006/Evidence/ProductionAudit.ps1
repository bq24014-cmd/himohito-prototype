$ErrorActionPreference='Stop'
$repo='C:/Users/田尻大翔/Documents/Codex/2026-08-07/new-chat/outputs/HimoHitoPrototype'
$project=Join-Path $repo '.codex_tmp/MainStageVisualAudit_20261003/Project'
$qa=Join-Path $repo 'Docs/QA/MainS3ReturnStepAProduction_20261006'
$paths=@('Assets/Scripts/MainS3PencilVisual.cs','Assets/Scripts/MainS3PencilVisual.cs.meta','Assets/Resources/MainS3Pencil.shader','Assets/Resources/MainS3Pencil.shader.meta')
$manifest=@()
foreach($relative in $paths) {
  $source=Join-Path $repo $relative
  $tested=Join-Path $project $relative
  $hash=(Get-FileHash -LiteralPath $source -Algorithm SHA256).Hash
  $manifest += [pscustomobject]@{path=$relative;sha256=$hash;bytes=(Get-Item -LiteralPath $source).Length;testedByteIdentical=($hash -eq (Get-FileHash -LiteralPath $tested -Algorithm SHA256).Hash)}
}
if(@($manifest|Where-Object {-not $_.testedByteIdentical}).Count) {throw 'Tested inputs differ from Production'}
$guids=@()
foreach($relative in $paths | Where-Object {$_.EndsWith('.meta')}) {
  $raw=Get-Content -LiteralPath (Join-Path $repo $relative) -Raw
  if($raw -notmatch '(?m)^guid: ([0-9a-f]{32})\r?$') {throw "Invalid GUID: $relative"}
  $guid=$Matches[1]
  $hits=@(Get-ChildItem -LiteralPath (Join-Path $repo 'Assets') -Recurse -File -Filter '*.meta' | Select-String -SimpleMatch "guid: $guid")
  if($hits.Count -ne 1) {throw "Duplicate GUID: $relative"}
  $guids += [pscustomobject]@{path=$relative;guid=$guid;matches=$hits.Count}
}
$sourceText=Get-Content -LiteralPath (Join-Path $repo 'Assets/Scripts/MainS3PencilVisual.cs') -Raw
$forbidden=@('Proof','F7','F8','OnGUI','Input.','SetCandidate','RestoreLegacyForComparison','UnityEditor','-s3Review')
$found=@($forbidden | Where-Object {$sourceText.Contains($_)})
if($found.Count) {throw 'Production contains proof/runtime diagnostic features'}
$approvedShader=Get-Content -LiteralPath (Join-Path $repo 'Docs/QA/MainS3ReturnStepAVisualProof_20261006/Evidence/Implementation/Assets/Resources/S3ReturnStepAPencilProof.shader') -Raw
$productionShader=Get-Content -LiteralPath (Join-Path $repo 'Assets/Resources/MainS3Pencil.shader') -Raw
$shaderExact=$approvedShader.Replace('Shader "HimoHito/Proof/S3ReturnStepAPencil"','Shader "HimoHito/MainS3Pencil"').Replace("`r`n","`n").TrimEnd() -eq $productionShader.Replace("`r`n","`n").TrimEnd()
if(-not $shaderExact) {throw 'Approved shader math changed'}
$record=[ordered]@{time=(Get-Date -Format o);manifest=$manifest;guidAudit=$guids;proofFeatureMatches=$found;approvedShaderMathIdenticalExceptName=$shaderExact;productionNewFiles=$paths;windowsBuildPerformed=$false;realKeyIntegratedRetest='UNCONFIRMED'}
$record | ConvertTo-Json -Depth 10 | Set-Content -LiteralPath (Join-Path $qa 'Evidence/ProductionManifest.json') -Encoding utf8
$record | ConvertTo-Json -Depth 8
