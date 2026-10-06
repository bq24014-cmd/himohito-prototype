param([switch]$RefreshOwnStage)
$ErrorActionPreference='Stop'
$repo=$PWD.Path
$gitArgs=@('-c',"safe.directory=$repo")
$qaRel='Docs/QA/MainS3ReturnStepBProduction_20261006'
$qa=Join-Path $repo $qaRel
$expected='44f81fe2a27da2661d2607c35e1cd84c657f95f6'
if((& git @gitArgs branch --show-current) -ne 'main' -or (& git @gitArgs rev-parse HEAD) -ne $expected -or (& git @gitArgs rev-parse origin/main) -ne $expected){throw 'Unexpected Git baseline; STOP'}
$initialStage=@(& git @gitArgs diff --cached --name-only)
if($initialStage.Count -and !$RefreshOwnStage){throw 'Existing staged files; STOP'}
if($initialStage.Count){
 $previousManifest=Get-Content -Raw -LiteralPath (Join-Path $qa 'Evidence/StageManifest.json')|ConvertFrom-Json
 if(@($initialStage|Where-Object {$_ -notin $previousManifest.files}).Count){throw 'Existing unrelated staged files; STOP'}
 foreach($path in $initialStage){ & git @gitArgs restore --staged -- $path; if($LASTEXITCODE -ne 0){throw 'Cannot refresh owned stage safely'} }
}
$result=Get-Content -Raw -LiteralPath (Join-Path $qa 'Evidence/Run04/Result.json')|ConvertFrom-Json
$pixel=Get-Content -Raw -LiteralPath (Join-Path $qa 'Evidence/PixelComparison.json')|ConvertFrom-Json
$scope=Get-Content -Raw -LiteralPath (Join-Path $qa 'Evidence/ProductionScope.json')|ConvertFrom-Json
$protection=Get-Content -Raw -LiteralPath (Join-Path $qa 'Evidence/ProtectionAfter.json')|ConvertFrom-Json
if($result.failures -ne 0 -or $result.errors.Count -ne 0 -or $pixel.failures -ne 0 -or $scope.status -ne 'PASS' -or $protection.status -ne 'PASS'){throw 'QA prerequisite failed; STOP'}
$production=@('Assets/Scripts/MainS3MarkerVisual.cs','Assets/Scripts/MainS3MarkerVisual.cs.meta','Assets/Resources/MainS3Marker.shader','Assets/Resources/MainS3Marker.shader.meta')
$docs=@()
$excluded=@()
foreach($f in Get-ChildItem -LiteralPath $qa -Recurse -File){
 $rel=$f.FullName.Substring($repo.Length+1).Replace('\','/')
 $sub=$rel.Substring($qaRel.Length+1)
 if($sub -match '^Evidence/Run0[123]/' -and $sub -notmatch '/(Result|Progress)\.json$'){$excluded+=$rel;continue}
 if($sub -match '^Evidence/Run0[1234]\.log$'){$excluded+=$rel;continue}
 if($sub -in @('Evidence/RunLogs.zip','Evidence/RunLogsManifest.json')){$excluded+=$rel;continue}
 if($sub -in @('Evidence/StageAudit.json','Evidence/StageManifest.json')){continue}
 $docs+=$rel
}
$manifestPath="$qaRel/Evidence/StageManifest.json"
$auditPath="$qaRel/Evidence/StageAudit.json"
$all=@($production+$docs+$manifestPath+$auditPath|Sort-Object -Unique)
$manifest=@{baseline=$expected;production=$production;files=$all;excludedLocalIntermediates=$excluded;scope='Only approved Step B visual and necessary new QA; existing changes/output/BGM/isolated project excluded'}
$manifest|ConvertTo-Json -Depth 6|Set-Content -LiteralPath (Join-Path $repo $manifestPath) -Encoding utf8
foreach($rel in $all | Where-Object {$_ -ne $auditPath}){
 & git @gitArgs add -- $rel
 if($LASTEXITCODE -ne 0){throw "git add failed: $rel"}
}
$cached=@(& git @gitArgs diff --cached --name-only)
$unexpected=@($cached|Where-Object {$_ -notin $all})
if($unexpected.Count){throw "Unexpected stage scope; STOP: $unexpected"}
& git @gitArgs diff --cached --check
if($LASTEXITCODE -ne 0){throw 'Cached whitespace check failed; STOP'}
$audit=@{status='PASS';baseline=$expected;stagedBeforeAuditRecord=$cached;finalExpectedStage=$all;unexpected=$unexpected;cachedDiffCheck='PASS';note='This audit record is explicitly added afterward; final path set and whitespace are reverified before commit.'}
$audit|ConvertTo-Json -Depth 6|Set-Content -LiteralPath (Join-Path $repo $auditPath) -Encoding utf8
& git @gitArgs add -- $auditPath
if($LASTEXITCODE -ne 0){throw 'Audit add failed'}
$cached=@(& git @gitArgs diff --cached --name-only)
$delta=@(Compare-Object -ReferenceObject @($all|Sort-Object) -DifferenceObject @($cached|Sort-Object))
if($delta.Count){throw "Final cached file set mismatch; STOP: $delta"}
& git @gitArgs diff --cached --check
if($LASTEXITCODE -ne 0){throw 'Final cached whitespace check failed; STOP'}
& git @gitArgs diff --cached --name-only
& git @gitArgs diff --cached --stat
& git @gitArgs status --short --untracked-files=normal
[pscustomobject]@{stageAudit='PASS';stagedFiles=$cached.Count;productionFiles=$production.Count;excludedLocalIntermediates=$excluded.Count}|ConvertTo-Json
