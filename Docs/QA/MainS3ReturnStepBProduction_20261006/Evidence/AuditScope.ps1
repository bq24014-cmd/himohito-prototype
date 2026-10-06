$ErrorActionPreference='Stop'
$repo=$PWD.Path
$qa=Join-Path $repo 'Docs/QA/MainS3ReturnStepBProduction_20261006/Evidence'
$production=@('Assets/Scripts/MainS3MarkerVisual.cs','Assets/Scripts/MainS3MarkerVisual.cs.meta','Assets/Resources/MainS3Marker.shader','Assets/Resources/MainS3Marker.shader.meta')
$before=Get-Content -Raw -LiteralPath (Join-Path $qa 'ProtectionBefore.json')|ConvertFrom-Json
$src=Get-Content -Raw -LiteralPath (Join-Path $repo $production[0])
$shader=Get-Content -Raw -LiteralPath (Join-Path $repo $production[2])
$approved=Get-Content -Raw -LiteralPath (Join-Path $repo 'Docs/QA/MainS3ReturnStepBVisualProof_20261006/Evidence/Implementation/Assets/Resources/S3ReturnStepBMarkerProof.shader')
$shaderSame=$shader.Replace('HimoHito/MainS3Marker','HimoHito/S3ReturnStepBMarkerProof').Trim() -ceq $approved.Trim()
$guids=@('ba63bd7a9b6745a78a67def0a258e360','16171d69b6774f74b4d4402b48ce6524')
$metaChecks=@()
foreach($guid in $guids){
 $matches=@(Get-ChildItem -LiteralPath (Join-Path $repo 'Assets') -Recurse -File -Filter '*.meta'|Where-Object {(Get-Content -Raw -LiteralPath $_.FullName).Contains($guid)}|ForEach-Object {$_.FullName.Substring($repo.Length+1).Replace('\','/')})
 $metaChecks+=@{guid=$guid;matches=$matches;unique=($matches.Count -eq 1)}
}
$files=@($production|ForEach-Object { $f=Get-Item -LiteralPath (Join-Path $repo $_); @{path=$_;bytes=$f.Length;sha256=(Get-FileHash -LiteralPath $f.FullName).Hash} })
$sourceDiff=@(& git -c "safe.directory=$repo" diff --no-index --unified=0 -- 'Docs/QA/MainS3ReturnStepBVisualProof_20261006/Evidence/Implementation/Assets/Scripts/S3ReturnStepBMarkerProofVisual.cs' 'Assets/Scripts/MainS3MarkerVisual.cs' 2>$null)
$sourceDiff|Set-Content -LiteralPath (Join-Path $qa 'ApprovedToProductionSource.diff') -Encoding utf8
$shaderDiff=@(& git -c "safe.directory=$repo" diff --no-index --unified=0 -- 'Docs/QA/MainS3ReturnStepBVisualProof_20261006/Evidence/Implementation/Assets/Resources/S3ReturnStepBMarkerProof.shader' 'Assets/Resources/MainS3Marker.shader' 2>$null)
$shaderDiff|Set-Content -LiteralPath (Join-Path $qa 'ApprovedToProductionShader.diff') -Encoding utf8
$record=@{files=$files;target='Main Section 3 Return Step B';position=@(41,-10.775,0);scale=@(1.8,10.55,1);colliderSize=@(1,1);walkingY=-5.5;shaderOnlyNameChanged=$shaderSame;proofInputOrToggleInProduction=[bool]($src -match 'ShowCandidate|SetCandidate|RestoreLegacyForComparison|KeyCode|Input\.|UNITY_EDITOR|UnityEditor');mainStageOnly=[bool]($src.Contains('scene.name != "MainStage"'));targetIsStepBOnly=[bool]($src.Contains('TargetName = MainStageSectionThreeSetup.ReturnStepBName'));meta=$metaChecks;existingInputHashCount=$before.files.Count}
$pass=$shaderSame -and !$record.proofInputOrToggleInProduction -and $record.mainStageOnly -and $record.targetIsStepBOnly -and !(@($metaChecks|Where-Object {!$_.unique}).Count)
$record.status=if($pass){'PASS'}else{'FAIL'}
$record|ConvertTo-Json -Depth 8|Set-Content -LiteralPath (Join-Path $qa 'ProductionScope.json') -Encoding utf8
$record|ConvertTo-Json -Depth 8
if(!$pass){throw 'Production scope audit failed'}
