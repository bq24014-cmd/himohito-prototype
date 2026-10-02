param([ValidateSet('Start','CopyPng','CreateVerificationCopy','End')][string]$Mode='Start')
$ErrorActionPreference='Stop';$env:GIT_OPTIONAL_LOCKS='0'
$root=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../../..'))
$qa=Join-Path $root 'Docs/QA/TutorialTerrainHybridProduction_20261003'
$approved=Join-Path $root 'Docs/QA/TutorialTerrainHybridAll_20261002/Evidence/ProductionCandidate'
$project=Join-Path $root '.codex_tmp/TutorialTerrainHybridProduction_20261003/Project'
$build=Join-Path $root 'output/TutorialTerrainHybridProduction_20261003'
function Git([string[]]$a){$r=& git.exe -c "safe.directory=$root" -c core.quotepath=false -C $root @a;if($LASTEXITCODE-ne 0){throw 'Git failure'};return ($r-join"`n")}
function NewJson($p,$v){if(Test-Path -LiteralPath $p){throw "Refuse overwrite: $p"};[IO.File]::WriteAllText($p,($v|ConvertTo-Json -Depth 14),[Text.UTF8Encoding]::new($false))}
function Hashes {
 $h=[ordered]@{}
 foreach($area in @('Assets','Packages','ProjectSettings')){foreach($f in Get-ChildItem -LiteralPath (Join-Path $root $area) -Recurse -File|Sort-Object FullName){
 if($f.Attributes-band[IO.FileAttributes]::ReparsePoint){throw 'Reparse point not permitted'}
 $h[$f.FullName.Substring($root.Length+1).Replace('\','/')]=(Get-FileHash -LiteralPath $f.FullName).Hash
 }}
 foreach($rel in @('README.md','Docs/LEARNING_LOG.md')){$h[$rel]=(Get-FileHash -LiteralPath (Join-Path $root $rel)).Hash}
 return $h
}
function ProtectedArtifactMetadata {
 $items=[ordered]@{}
 foreach($area in @('Docs','output')){foreach($f in Get-ChildItem -LiteralPath (Join-Path $root $area) -Recurse -File){
 if($f.FullName.StartsWith($qa+[IO.Path]::DirectorySeparatorChar,[StringComparison]::OrdinalIgnoreCase) -or $f.FullName.StartsWith($build+[IO.Path]::DirectorySeparatorChar,[StringComparison]::OrdinalIgnoreCase)){continue}
 $items[$f.FullName.Substring($root.Length+1).Replace('\','/')]=@{bytes=$f.Length;modified_utc_ticks=$f.LastWriteTimeUtc.Ticks}
 }}
 return $items
}
$manifestPath=Join-Path $PSScriptRoot 'ProtectionStart.json'
if($Mode-eq'Start'){
 NewJson $manifestPath ([ordered]@{utc=[DateTimeOffset]::UtcNow.ToString('o');HEAD=(Git @('rev-parse','HEAD'));origin_main=(Git @('rev-parse','origin/main'));branch=(Git @('branch','--show-current'));status=(Git @('status','--short'));tracked_status=(Git @('status','--short','-uno'));staged=(Git @('diff','--cached','--name-only'));hashes=(Hashes);protected_artifact_metadata=(ProtectedArtifactMetadata)})
 'Start protection recorded; original Source/Scene/BGM hashes and prior Docs/output file metadata.'
}
if($Mode-eq'CopyPng'){
 if(-not(Test-Path -LiteralPath $manifestPath)){throw 'Start first'}
 $folder=Join-Path $root 'Assets/Resources/Art/TutorialTerrainHybrid'
 New-Item -ItemType Directory -Path $folder -Force|Out-Null
 foreach($name in @('Start','Landing1','BridgeBank','MergeBank','Goal')){
 $rel='Assets/Resources/Art/TutorialTerrainHybrid/'+$name+'.png';$target=Join-Path $root $rel
 if(Test-Path -LiteralPath $target){throw "Refuse existing production asset $rel"}
 Copy-Item -LiteralPath (Join-Path $approved $rel) -Destination $target
 if((Get-FileHash -LiteralPath $target).Hash-ne(Get-FileHash -LiteralPath (Join-Path $approved $rel)).Hash){throw 'Asset hash mismatch'}
 }
 'Five approved PNGs copied byte-for-byte; no existing asset overwritten.'
}
if($Mode-eq'CreateVerificationCopy'){
 if(Test-Path -LiteralPath $project){throw 'Refuse existing verification project'}
 New-Item -ItemType Directory -Path $project|Out-Null
 foreach($area in @('Assets','Packages','ProjectSettings')){Copy-Item -LiteralPath (Join-Path $root $area) -Destination (Join-Path $project $area) -Recurse}
 $same=0;$different=@();$now=Hashes
 foreach($rel in $now.Keys){if($rel-eq'README.md'-or$rel-eq'Docs/LEARNING_LOG.md'){continue};if((Get-FileHash -LiteralPath (Join-Path $project $rel)).Hash-ne$now[$rel]){$different+=$rel}else{$same++}}
 NewJson (Join-Path $PSScriptRoot 'VerificationCopyIdentity.json') (@{source='Current actual Production Assets/Packages/ProjectSettings';project=$project;matched=$same;different=$different;hashes=$now;old_proof_resources_copied=$false})
 if($different.Count){throw 'Verification source mismatch'}
 "Production verification copy ready: $same matching files, no prior proof resources/helpers."
}
if($Mode-eq'End'){
 $before=Get-Content -LiteralPath $manifestPath -Raw|ConvertFrom-Json -AsHashtable
 $now=Hashes;$changed=@();$added=@()
 foreach($rel in $before.hashes.Keys){if(-not$now.Contains($rel)-or$now[$rel]-ne$before.hashes[$rel]){$changed+=$rel}}
 foreach($rel in $now.Keys){if(-not$before.hashes.ContainsKey($rel)){$added+=$rel}}
 $allowed=@('Assets/Scripts/TutorialTerrainHybridVisual.cs','Assets/Scripts/TutorialTerrainHybridVisual.cs.meta','Assets/Resources/Art/TutorialTerrainHybrid.meta')
 foreach($name in @('Start','Landing1','BridgeBank','MergeBank','Goal')){$allowed+='Assets/Resources/Art/TutorialTerrainHybrid/'+$name+'.png';$allowed+='Assets/Resources/Art/TutorialTerrainHybrid/'+$name+'.png.meta'}
 $unexpected=@($added|Where-Object{$_-notin$allowed});$missing=@($allowed|Where-Object{$_-notin$added})
 $artNow=ProtectedArtifactMetadata;$artifactChanges=@()
 foreach($rel in $before.protected_artifact_metadata.Keys){$old=$before.protected_artifact_metadata[$rel];if(-not$artNow.Contains($rel)-or$artNow[$rel].bytes-ne$old.bytes-or$artNow[$rel].modified_utc_ticks-ne$old.modified_utc_ticks){$artifactChanges+=$rel}}
 $gitSame=(Git @('rev-parse','HEAD'))-eq$before.HEAD-and(Git @('rev-parse','origin/main'))-eq$before.origin_main-and(Git @('branch','--show-current'))-eq$before.branch-and(Git @('status','--short','-uno'))-eq$before.tracked_status-and(Git @('diff','--cached','--name-only'))-eq$before.staged
 $record=@{status=$(if(-not$changed.Count-and-not$unexpected.Count-and-not$missing.Count-and-not$artifactChanges.Count-and$gitSame){'PASS'}else{'FAIL'});original_hashes_count=$before.hashes.Count;changed_original=$changed;added=$added;unexpected=$unexpected;missing=$missing;prior_artifact_count=$before.protected_artifact_metadata.Count;prior_artifact_metadata_changes=$artifactChanges;git_state_unchanged=$gitSame;HEAD=(Git @('rev-parse','HEAD'));origin_main=(Git @('rev-parse','origin/main'));hashes=$now;artifact_verification='Source/Scene/BGM SHA256; prior Docs/output file existence/length/LastWriteTimeUtc (not full artifact SHA256)'}
 NewJson (Join-Path $PSScriptRoot 'ProtectionEnd.json') $record
 $record|Select-Object status,original_hashes_count,changed_original,added,unexpected,missing,prior_artifact_count,prior_artifact_metadata_changes,git_state_unchanged|ConvertTo-Json -Depth 4
}
