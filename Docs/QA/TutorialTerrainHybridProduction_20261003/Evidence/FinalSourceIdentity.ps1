$ErrorActionPreference='Stop'
$env:GIT_OPTIONAL_LOCKS='0'
$root=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../../..'))
$project=Join-Path $root '.codex_tmp/TutorialTerrainHybridProduction_20261003/Project'
$candidate=Join-Path $root 'Docs/QA/TutorialTerrainHybridAll_20261002/Evidence/ProductionCandidate'
$start=Get-Content -LiteralPath (Join-Path $PSScriptRoot 'ProtectionStart.json') -Raw|ConvertFrom-Json -AsHashtable
$path=Join-Path $PSScriptRoot 'FinalSourceIdentity.json'
if(Test-Path -LiteralPath $path){throw 'Refuse existing identity record'}
$isolatedDifferent=@();$isolatedMatch=0;$originalChanges=@();$approvedNonCode=@();$newFiles=@()
foreach($area in @('Assets','Packages','ProjectSettings')){
 foreach($file in Get-ChildItem -LiteralPath (Join-Path $root $area) -Recurse -File){
  $rel=$file.FullName.Substring($root.Length+1).Replace('\','/')
  $hash=(Get-FileHash -LiteralPath $file.FullName).Hash
  $other=Join-Path $project $rel
  $otherHash=if(Test-Path -LiteralPath $other){(Get-FileHash -LiteralPath $other).Hash}else{'MISSING'}
  if($hash-ne$otherHash){$isolatedDifferent+=@{path=$rel;production=$hash;isolated=$otherHash}}else{$isolatedMatch++}
  if($start.hashes.ContainsKey($rel)){
   if($start.hashes[$rel]-ne$hash){$originalChanges+=$rel}
  }else{
   $newFiles+=@{path=$rel;sha256=$hash;bytes=$file.Length}
   if($rel-ne'Assets/Scripts/TutorialTerrainHybridVisual.cs'){
    $approved=Join-Path $candidate $rel
    $approvedHash=if(Test-Path -LiteralPath $approved){(Get-FileHash -LiteralPath $approved).Hash}else{'MISSING'}
    $approvedNonCode+=@{path=$rel;production_sha256=$hash;approved_sha256=$approvedHash;equal=($hash-eq$approvedHash)}
   }
  }
 }
}
$unexpectedDiff=@($isolatedDifferent|Where-Object{$_.path-ne'ProjectSettings/ProjectSettings.asset'})
$source=Get-Content -LiteralPath (Join-Path $root 'Assets/Scripts/TutorialTerrainHybridVisual.cs') -Raw
$forbiddenTokens=@('KeyCode.F7','Input.','UnityEditor','OnGUI','SetHybrid','GetTarget(','GetVisualBounds','public static TutorialTerrainHybridVisual Current','public bool Ready','TextureCropInfo')|Where-Object{$source.Contains($_)}
$metas=@(Get-ChildItem -LiteralPath (Join-Path $root 'Assets') -Recurse -Filter '*.meta' -File)
$allGuids=@{}
foreach($file in $metas){$match=[regex]::Match((Get-Content -LiteralPath $file.FullName -Raw),'(?m)^guid:\s*(\w+)');if($match.Success){$guid=$match.Groups[1].Value;$allGuids[$guid]=1+$allGuids[$guid]}}
$newGuids=@()
foreach($file in $newFiles|Where-Object{$_.path.EndsWith('.meta')}){
 $match=[regex]::Match((Get-Content -LiteralPath (Join-Path $root $file.path) -Raw),'(?m)^guid:\s*(\w+)')
 $guid=$match.Groups[1].Value;$newGuids+=@{path=$file.path;guid=$guid;count=$allGuids[$guid]}
}
$record=[ordered]@{
 utc=[DateTimeOffset]::UtcNow.ToString('o');original_production_changes=$originalChanges;new_production_files=$newFiles
 approved_non_code=$approvedNonCode;new_meta_guids=$newGuids;forbidden_source_tokens=$forbiddenTokens
 isolated_matching=$isolatedMatch;isolated_differences=$isolatedDifferent;unexpected_isolated_differences=$unexpectedDiff
 isolated_settings_reason='Review-only companyName/productName identification. Source/Scene/Packages/other settings must remain byte-identical.'
 source_sha256=(Get-FileHash -LiteralPath (Join-Path $root 'Assets/Scripts/TutorialTerrainHybridVisual.cs')).Hash
 passed=($originalChanges.Count-eq 0-and$newFiles.Count-eq 13-and@($approvedNonCode|Where-Object{-not$_.equal}).Count-eq 0-and@($newGuids|Where-Object{$_.count-ne 1}).Count-eq 0-and$forbiddenTokens.Count-eq 0-and$unexpectedDiff.Count-eq 0)
}
[IO.File]::WriteAllText($path,($record|ConvertTo-Json -Depth 12),[Text.UTF8Encoding]::new($false))
$record|Select-Object passed,isolated_matching,isolated_differences,original_production_changes,source_sha256|ConvertTo-Json -Depth 5
