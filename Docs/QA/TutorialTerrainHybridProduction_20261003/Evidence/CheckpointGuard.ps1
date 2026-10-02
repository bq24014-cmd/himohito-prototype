param([ValidateSet('Prepare','Audit','End')][string]$Mode='Prepare')
$ErrorActionPreference='Stop'
$env:GIT_OPTIONAL_LOCKS='0'
$root=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../../..'))
$qaRel='Docs/QA/TutorialTerrainHybridProduction_20261003'
$qa=Join-Path $root $qaRel
$baseline='c21463ce60f63ccbff78486a98210eac3aa21b1f'
function Git([string[]]$arguments){$value=& git.exe -c "safe.directory=$root" -c core.quotepath=false -C $root @arguments;if($LASTEXITCODE-ne 0){throw ('Git failed: '+($arguments-join' '))};return ($value-join"`n")}
function NewJson($name,$value){$path=Join-Path $PSScriptRoot $name;if(Test-Path -LiteralPath $path){throw "Refuse overwrite: $name"};[IO.File]::WriteAllText($path,($value|ConvertTo-Json -Depth 15),[Text.UTF8Encoding]::new($false))}
function ApprovedProduction {
 $end=Get-Content -LiteralPath (Join-Path $PSScriptRoot 'ProtectionEnd.json') -Raw|ConvertFrom-Json -AsHashtable
 $changes=@()
 foreach($rel in $end.hashes.Keys){$file=Join-Path $root $rel;if(-not(Test-Path -LiteralPath $file)-or(Get-FileHash -LiteralPath $file).Hash-ne$end.hashes[$rel]){$changes+=$rel}}
 if($changes.Count){throw ('Approved file changed: '+($changes-join', '))}
 return $end.hashes.Count
}
function PreserveArtifacts {
 $before=Get-Content -LiteralPath (Join-Path $PSScriptRoot 'ProtectionStart.json') -Raw|ConvertFrom-Json -AsHashtable
 $changes=@()
 foreach($rel in $before.protected_artifact_metadata.Keys){
  $old=$before.protected_artifact_metadata[$rel];$path=Join-Path $root $rel
  if(-not(Test-Path -LiteralPath $path)){$changes+=$rel;continue}
  $now=Get-Item -LiteralPath $path
  if($now.Length-ne$old.bytes-or$now.LastWriteTimeUtc.Ticks-ne$old.modified_utc_ticks){$changes+=$rel}
 }
 if($changes.Count){throw ('Prior artifact changed: '+($changes-join', '))}
 return $before.protected_artifact_metadata.Count
}
function ProtectedDirty {
 $map=[ordered]@{}
 foreach($rel in @('Assets/Scenes/MainStage.unity','README.md','Docs/LEARNING_LOG.md','Docs/QA/MainStageCleanReproduction_20261001/Evidence/DeterministicSceneSync/ExeA.log','Docs/QA/MainStageCleanReproduction_20261001/Evidence/DeterministicSceneSync/ExeB.log')){
  $map[$rel]=(Get-FileHash -LiteralPath (Join-Path $root $rel)).Hash
 }
 return $map
}
function Plan {return (Get-Content -LiteralPath (Join-Path $PSScriptRoot 'CheckpointStagePlan.json') -Raw|ConvertFrom-Json)}
if($Mode-eq'Prepare'){
 if((Git @('branch','--show-current'))-ne'main'-or(Git @('rev-parse','HEAD'))-ne$baseline-or(Git @('rev-parse','origin/main'))-ne$baseline){throw 'Unexpected Git baseline'}
 if((Git @('diff','--cached','--name-only'))-ne''){throw 'Index must start empty'}
 $remote=Git @('remote','get-url','origin')
 if($remote-notmatch '^(https://github\.com/|git@github\.com:)bq24014-cmd/himohito-prototype(?:\.git)?$'){throw 'Unexpected remote repository'}
 $productionCount=ApprovedProduction;$artifactCount=PreserveArtifacts
 $bgm='Assets/Resources/Audio/YasashiiOdori.mp3'
 if((Git @('ls-files','--',$bgm))-ne''){throw 'BGM unexpectedly tracked'}
 $bgmIgnore=Git @('check-ignore','-v','--',$bgm)
 $build=Join-Path $root 'output/TutorialTerrainHybridProduction_20261003/Windows'
 $audit=Get-Content -LiteralPath (Join-Path $PSScriptRoot 'PlayerAssemblyAudit02/PLAYER_ASSEMBLY_AUDIT.json') -Raw|ConvertFrom-Json
 $assembly=Join-Path $build 'HimoHitoTutorialTerrainHybridProduction_Data/Managed/Assembly-CSharp.dll'
 if(-not$audit.passed-or(Get-FileHash -LiteralPath $assembly).Hash-ne$audit.sha256){throw 'Built DLL differs from approved audit'}
 NewJson 'CheckpointStart.json' ([ordered]@{utc=[DateTimeOffset]::UtcNow.ToString('o');branch='main';HEAD=$baseline;origin_main=$baseline;remote=$remote;approved_production_hashes_matched=$productionCount;prior_artifacts_metadata_matched=$artifactCount;dirty_hashes=(ProtectedDirty);tracked_status=(Git @('status','--short','-uno'));staged='';bgm_sha256=(Get-FileHash -LiteralPath (Join-Path $root $bgm)).Hash;bgm_tracked=$false;bgm_ignore_rule=$bgmIgnore;human_approval='問題なし・採用してよい';gameplay_source_modified=$false})
 NewJson 'BuildValidationSummary.json' ([ordered]@{build_result_original=(Get-Content -LiteralPath (Join-Path $build 'BUILD_RESULT.txt') -Raw);initial_audit_failure='QA Reflection AmbiguousMatchException on Cecil DeclaringType; Build itself Succeeded';reaudit_pass=$audit.passed;reaudit_evidence='PlayerAssemblyAudit02/PLAYER_ASSEMBLY_AUDIT.json';unchanged_dll_sha256=$audit.sha256;exe_sha256=(Get-FileHash -LiteralPath (Join-Path $build 'HimoHitoTutorialTerrainHybridProduction.exe')).Hash;production_source_sha256=(Get-FileHash -LiteralPath (Join-Path $root 'Assets/Scripts/TutorialTerrainHybridVisual.cs')).Hash;build_location='output/TutorialTerrainHybridProduction_20261003/Windows';human_review='PASS / APPROVED';raw_unity_logs='LOCAL ONLY; retain original files; do not publish device/licensing metadata';no_rebuild_or_binary_replacement=$true})
 $paths=@('Assets/Scripts/TutorialTerrainHybridVisual.cs','Assets/Scripts/TutorialTerrainHybridVisual.cs.meta','Assets/Resources/Art/TutorialTerrainHybrid.meta')
 foreach($name in @('Start','Landing1','BridgeBank','MergeBank','Goal')){$paths+='Assets/Resources/Art/TutorialTerrainHybrid/'+$name+'.png';$paths+='Assets/Resources/Art/TutorialTerrainHybrid/'+$name+'.png.meta'}
 foreach($file in @('00_README.md','01_ProductionIntegration.md','02_ProductionDiff.md','03_GameplayRegression.md','04_VisualRegression.md','05_HumanReviewPacket.md','06_FinalDecision.md','07_HumanApprovalAndCheckpoint.md','LaunchReview.ps1')){$paths+=$qaRel+'/'+$file}
 foreach($file in @('FinalSourceIdentity.json','FinalSourceIdentity.ps1','ImageComparison_Runtime01.json','Launch_20261003_002502.json','LaunchCheck_20261003_002534.json','PackageProductionComparisons.py','ProductionProtection.ps1','ProtectionEnd.json','RecordFinalState.ps1','TerrainProductionEntry.cs','TerrainProductionQA.cs','TerrainProductionQaFacade.cs','VerificationCopyIdentity.json','CheckpointGuard.ps1','CheckpointStart.json','BuildValidationSummary.json','CheckpointStagePlan.json','PlayerAssemblyAudit02/PLAYER_ASSEMBLY_AUDIT.json','Runtime01/aim_resolution.csv','Runtime01/captures.csv','Runtime01/checks.txt','Runtime01/editor_search_errors.txt','Runtime01/intro.csv','Runtime01/result.txt','Runtime01/route.csv','Runtime01/terrain_projection.csv','Runtime01/terrain_runtime_audit.txt')){$paths+=$qaRel+'/Evidence/'+$file}
 foreach($file in @('AllFiveTerrain_Comparison.png','Overview_Comparison.png','Comparison_S1_Standing.png','Comparison_S2_Standing.png','Comparison_S3_Standing.png','Comparison_S4_Standing.png','Comparison_S4_WideStanding.png','Comparison_S4_Merged.png','Comparison_S4_Clearing.png','Comparison_S4_Clear.png','Comparison_Intro_Middle.png','Comparison_Intro_NearEnd.png')){$paths+=$qaRel+'/Screenshots/Final/'+$file}
 $records=@()
 foreach($rel in $paths){
  if($rel-eq$qaRel+'/Evidence/CheckpointStagePlan.json'){$records+=@{path=$rel;sha256=$null;bytes=$null;self_manifest=$true};continue}
  $file=Get-Item -LiteralPath (Join-Path $root $rel)
  if($file.Length-ge 50MB){throw ('Large candidate: '+$rel)}
  $records+=@{path=$rel;sha256=(Get-FileHash -LiteralPath $file.FullName).Hash;bytes=$file.Length;self_manifest=$false}
 }
 NewJson 'CheckpointStagePlan.json' ([ordered]@{scope='13 Production files + selected necessary QA; no Build/BGM/cache/raw PPM/other Proof';base_head=$baseline;file_count=$records.Count;files=$records;self_manifest_hash='Not embedded to avoid self reference; index/worktree equality checked before commit';local_only='Raw PPM, unselected PNG, raw Unity/player logs, ProtectionStart metadata, FinalState/full status, duplicate candidate Source, retired helper, all prior QA/Builds remain on disk';estimated_bytes_without_self=($records|Measure-Object bytes -Sum).Sum})
 "Ready: $($records.Count) explicit paths; $productionCount approved file hashes and $artifactCount prior artifacts preserved."
}
if($Mode-eq'Audit'){
 $plan=Plan;$expected=@($plan.files.path|Sort-Object);$actual=@((Git @('diff','--cached','--name-only')).Split("`n")|Where-Object{$_}|Sort-Object)
 if(@(Compare-Object $expected $actual).Count){throw 'Staged file set differs from explicit plan'}
 if((Git @('rev-parse','HEAD'))-ne$baseline-or(Git @('rev-parse','origin/main'))-ne$baseline){throw 'Git baseline changed'}
 $mismatch=@()
 foreach($item in $plan.files){
  $full=Join-Path $root $item.path;$hash=(Get-FileHash -LiteralPath $full).Hash
  if(-not$item.self_manifest-and$hash-ne$item.sha256){$mismatch+=$item.path}
  $workBlob=Git @('hash-object','--path',$item.path,'--',$item.path);$indexBlob=Git @('rev-parse',(':'+$item.path))
  if($workBlob-ne$indexBlob){$mismatch+=$item.path}
 }
 if($mismatch.Count){throw ('Staged content mismatch: '+($mismatch-join', '))}
 $rawCheck=& git.exe -c "safe.directory=$root" -C $root diff --cached --check
 $rawCheckExit=$LASTEXITCODE
 # Preserve approved Unity meta bytes and the original diagnostic stack text.
 # Only these known warnings are accepted; Source/docs/other evidence stay strict.
 $whitespaceExceptions=@($expected|Where-Object{$_-like 'Assets/Resources/Art/TutorialTerrainHybrid*.meta'})
 $errorEvidence=$qaRel+'/Evidence/Runtime01/editor_search_errors.txt'
 foreach($line in $rawCheck){
  if($line.StartsWith('+')){continue}
  if($line-match '^(.+):(\d+): (.+)$'){
   $file=$Matches[1];$message=$Matches[3]
   if(($file-in$whitespaceExceptions-and$message-eq'trailing whitespace.')-or($file-eq$errorEvidence-and$message-eq'new blank line at EOF.')){continue}
  }
  throw ('Unexpected whitespace diagnostic: '+$line)
 }
 $strictPaths=@($expected|Where-Object{$_-notin$whitespaceExceptions-and$_-ne$errorEvidence})
 $check=& git.exe -c "safe.directory=$root" -C $root diff --cached --check -- @strictPaths
 if($LASTEXITCODE-ne 0){throw ('Cached Source/docs diff check failed: '+($check-join"`n"))}
 $productionCount=ApprovedProduction;$artifactCount=PreserveArtifacts
 NewJson 'CheckpointStageAudit.json' (@{passed=$true;file_count=$actual.Count;staged_paths=$actual;stat=(Git @('diff','--cached','--stat'));status=(Git @('status','--short'));production_hashes_matched=$productionCount;prior_artifacts_metadata_matched=$artifactCount;stage_content_mismatch=@();cached_diff_check='Source/docs strict PASS; approved meta and verbatim diagnostic whitespace retained';raw_cached_diff_check_exit=$rawCheckExit;raw_cached_diff_check=($rawCheck-join"`n");note='Audit record local-only; does not recursively add itself to approved stage set'})
 "Stage audit PASS: $($actual.Count) exact paths; Source/docs whitespace PASS; known approved-meta/verbatim-evidence whitespace retained; all protections held."
}
if($Mode-eq'End'){
 $start=Get-Content -LiteralPath (Join-Path $PSScriptRoot 'CheckpointStart.json') -Raw|ConvertFrom-Json -AsHashtable
 $plan=Plan;$head=Git @('rev-parse','HEAD');$remote=Git @('rev-parse','origin/main')
 if($head-ne$remote){throw 'HEAD/origin mismatch'}
 if((Git @('diff','--cached','--name-only'))-ne''){throw 'Stage remains'}
 if((Git @('status','--short','-uno'))-ne$start.tracked_status){throw 'Original tracked dirty status changed'}
 $now=ProtectedDirty;foreach($rel in $start.dirty_hashes.Keys){if($now[$rel]-ne$start.dirty_hashes[$rel]){throw ('Protected dirty content changed: '+$rel)}}
 $productionCount=ApprovedProduction;$artifactCount=PreserveArtifacts
 foreach($item in $plan.files){
  if((Git @('rev-parse',('HEAD:'+$item.path)))-ne(Git @('hash-object','--path',$item.path,'--',$item.path))){throw ('Committed file not identical: '+$item.path)}
 }
 if((Git @('ls-files','--','Assets/Resources/Audio/YasashiiOdori.mp3'))-ne''){throw 'BGM unexpectedly tracked'}
 NewJson 'CheckpointPushVerification.json' (@{passed=$true;HEAD=$head;origin_main=$remote;branch=(Git @('branch','--show-current'));staged='';committed_files_matched=$plan.file_count;production_hashes_matched=$productionCount;prior_artifacts_metadata_matched=$artifactCount;protected_dirty_hashes_unchanged=$true;bgm_untracked_ignored=$true;status=(Git @('status','--short'));human_review='TP1 APPROVED';cleanup_performed=$false})
 "Push verification PASS: HEAD == origin/main == $head; $($plan.file_count) committed files checked; all protections held."
}
