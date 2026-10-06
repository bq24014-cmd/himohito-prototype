param([switch]$Verify,[string]$VerificationOutput)
$ErrorActionPreference='Stop'
$repo='C:/Users/田尻大翔/Documents/Codex/2026-08-07/new-chat/outputs/HimoHitoPrototype'
$qa=Join-Path $repo 'Docs/QA/MainS3ReturnStepAProduction_20261006'
$env:GIT_OPTIONAL_LOCKS='0'
function ReadGit { param([string[]]$Arguments) @(& git -C $repo -c "safe.directory=$repo" -c core.quotepath=false @Arguments) }
$rows=@()
foreach($folder in @('Assets','Packages','ProjectSettings')) {
  foreach($file in Get-ChildItem -LiteralPath (Join-Path $repo $folder) -File -Recurse) {
    $relative=$file.FullName.Substring($repo.Length+1).Replace('\','/')
    if($relative -in @('Assets/Scripts/MainS3PencilVisual.cs','Assets/Scripts/MainS3PencilVisual.cs.meta','Assets/Resources/MainS3Pencil.shader','Assets/Resources/MainS3Pencil.shader.meta')) {continue}
    $rows += [pscustomobject]@{path=$relative;bytes=$file.Length;sha256=(Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash}
  }
}
foreach($name in @('README.md','AGENTS.md','Docs/LEARNING_LOG.md','Docs/QA/MainStageCleanReproduction_20261001/Evidence/DeterministicSceneSync/ExeA.log','Docs/QA/MainStageCleanReproduction_20261001/Evidence/DeterministicSceneSync/ExeB.log')) {
  $file=Get-Item -LiteralPath (Join-Path $repo $name)
  $rows += [pscustomobject]@{path=$name;bytes=$file.Length;sha256=(Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash}
}
$snapshot=[ordered]@{time=(Get-Date -Format o);branch=[string](ReadGit @('branch','--show-current'));head=[string](ReadGit @('rev-parse','HEAD'));originMain=[string](ReadGit @('rev-parse','origin/main'));status=@(ReadGit @('status','--short'));staged=@(ReadGit @('diff','--cached','--name-only'));inputs=$rows;buildRoots=@(Get-ChildItem -LiteralPath (Join-Path $repo 'output') -Directory | Select-Object Name,LastWriteTime);freeBytes=(Get-PSDrive C).Free}
if(-not $Verify) {
  if(Test-Path -LiteralPath (Join-Path $qa 'Evidence/ProtectionBefore.json')) {throw 'Do not overwrite existing baseline'}
  if($snapshot.branch -ne 'main' -or $snapshot.head -ne 'fdc7fd39b6ec2b5a9131b1ad2fc53f0adf370f7e' -or $snapshot.originMain -ne $snapshot.head -or $snapshot.staged.Count -ne 0) {throw 'Unexpected baseline'}
  $snapshot | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath (Join-Path $qa 'Evidence/ProtectionBefore.json') -Encoding utf8
  [pscustomobject]@{branch=$snapshot.branch;head=$snapshot.head;originMain=$snapshot.originMain;protectedFiles=$rows.Count;staged=$snapshot.staged;modified=@($snapshot.status|Where-Object {-not $_.StartsWith('??')});untrackedEntries=@($snapshot.status|Where-Object {$_.StartsWith('??')}).Count} | ConvertTo-Json -Depth 6
  exit
}
$before=Get-Content -LiteralPath (Join-Path $qa 'Evidence/ProtectionBefore.json') -Raw | ConvertFrom-Json
$changed=@(Compare-Object $before.inputs $rows -Property path,bytes,sha256)
$oldExisting=@($before.status|Where-Object {-not $_.StartsWith('?? Docs/QA/MainS3ReturnStepAProduction_20261006')})
$newExisting=@($snapshot.status|Where-Object {$_ -notmatch '^(\?\?|A ) (Assets/Scripts/MainS3PencilVisual\.cs(\.meta)?|Assets/Resources/MainS3Pencil\.shader(\.meta)?|Docs/QA/MainS3ReturnStepAProduction_20261006)'})
$statusChanged=@(Compare-Object $oldExisting $newExisting)
$missingBuilds=@($before.buildRoots | Where-Object {-not (Test-Path -LiteralPath (Join-Path $repo ('output/'+$_.Name)))})
$result=[ordered]@{time=$snapshot.time;protectedFiles=$rows.Count;hashChanges=$changed;existingStatusChanges=$statusChanged;oldBuildDirectories=$before.buildRoots.Count;missingBuildDirectories=$missingBuilds;staged=$snapshot.staged;head=$snapshot.head;originMain=$snapshot.originMain;unchanged=($changed.Count -eq 0 -and $statusChanged.Count -eq 0 -and $missingBuilds.Count -eq 0)}
if(-not $VerificationOutput) {$VerificationOutput=Join-Path $qa 'Evidence/ProtectionAfter.json'}
$result | ConvertTo-Json -Depth 12 | Set-Content -LiteralPath $VerificationOutput -Encoding utf8
$result | ConvertTo-Json -Depth 12
if(-not $result.unchanged) {throw 'Protected files or status changed'}
