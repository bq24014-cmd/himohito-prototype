param([string]$Repo = 'C:/Users/田尻大翔/Documents/Codex/2026-08-07/new-chat/outputs/HimoHitoPrototype')
$ErrorActionPreference='Stop'
$qa=Join-Path $Repo 'Docs/QA/MainS3ReturnStepBProduction_20261006/Evidence'
New-Item -ItemType Directory -Path $qa -Force | Out-Null
$env:GIT_OPTIONAL_LOCKS='0'
$status=@(& git -c "safe.directory=$Repo" -C $Repo status --short --untracked-files=all)
$staged=@(& git -c "safe.directory=$Repo" -C $Repo diff --cached --name-only)
if ($staged.Count) { throw 'Existing staged files; STOP' }
$files=@()
foreach($part in @('Assets','Packages','ProjectSettings')) {
 foreach($f in Get-ChildItem -LiteralPath (Join-Path $Repo $part) -Recurse -File) {
  $files+=@{path=$f.FullName.Substring($Repo.Length+1).Replace('\','/');bytes=$f.Length;sha256=(Get-FileHash -LiteralPath $f.FullName -Algorithm SHA256).Hash}
 }
}
foreach($path in @('README.md','Docs/LEARNING_LOG.md','Docs/QA/MainStageCleanReproduction_20261001/Evidence/DeterministicSceneSync/ExeA.log','Docs/QA/MainStageCleanReproduction_20261001/Evidence/DeterministicSceneSync/ExeB.log')) {
 $f=Get-Item -LiteralPath (Join-Path $Repo $path)
 $files+=@{path=$path;bytes=$f.Length;sha256=(Get-FileHash -LiteralPath $f.FullName -Algorithm SHA256).Hash}
}
$previous=@()
foreach($part in @('output','Builds','Docs')) {
 $base=Join-Path $Repo $part
 if(!(Test-Path -LiteralPath $base)){continue}
 foreach($f in Get-ChildItem -LiteralPath $base -Recurse -File) {
  $rel=$f.FullName.Substring($Repo.Length+1).Replace('\','/')
  if($rel.StartsWith('Docs/QA/MainS3ReturnStepBProduction_20261006/')){continue}
  $previous+=@{path=$rel;bytes=$f.Length;utcTicks=$f.LastWriteTimeUtc.Ticks}
 }
}
$record=@{utc=[datetime]::UtcNow.ToString('o');branch=(& git -c "safe.directory=$Repo" -C $Repo branch --show-current);head=(& git -c "safe.directory=$Repo" -C $Repo rev-parse HEAD);originMain=(& git -c "safe.directory=$Repo" -C $Repo rev-parse origin/main);status=$status;staged=$staged;files=$files;previousFiles=$previous}
$record | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $qa 'ProtectionBefore.json') -Encoding utf8
@{branch=$record.branch;head=$record.head;originMain=$record.originMain;protectedHashes=$files.Count;previousMetadataFiles=$previous.Count;staged=$staged;trackedChanges=@($status|Where-Object{!$_.StartsWith('??')})} | ConvertTo-Json -Depth 4
