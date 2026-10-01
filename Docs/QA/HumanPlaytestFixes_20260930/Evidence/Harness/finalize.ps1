$ErrorActionPreference='Stop'
$root=(Get-Location).Path
$evidence=Join-Path $root 'Docs\QA\HumanPlaytestFixes_20260930\Evidence'
$allowed=@('Assets\Scripts\PlayerMover.cs','Assets\Scripts\RopeController.cs','Assets\Scripts\TutorialSignInscription.cs','Assets\Scripts\TutorialSectionGuide.cs','README.md','Docs\LEARNING_LOG.md')
$baseline=Get-Content -LiteralPath (Join-Path $evidence 'sha_start.json') -Raw|ConvertFrom-Json
$changes=@();$unexpected=@();$missing=@()
foreach($file in $baseline){
 $path=Join-Path $root $file.path
 if(!(Test-Path -LiteralPath $path)){$missing+=$file.path;continue}
 $hash=(Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash
 if($hash -ne $file.sha256){$changes+=$file.path;if($file.path -notin $allowed){$unexpected+=$file.path}}
}
$gitArgs=@('-c',"safe.directory=$($root.Replace('\','/'))")
$head=(& git @gitArgs rev-parse HEAD);$remote=(& git @gitArgs rev-parse origin/main)
$staged=@(& git @gitArgs diff --cached --name-only)
$result=[pscustomobject]@{baselineFiles=$baseline.Count;changedBaselineFiles=$changes;unexpectedChanges=$unexpected;missingFiles=$missing;head=$head;originMain=$remote;branch=(& git @gitArgs branch --show-current);staged=$staged;status=@(& git @gitArgs status --short);mainStagePreserved=(!($changes -contains 'Assets\Scenes\MainStage.unity') -and !($missing -contains 'Assets\Scenes\MainStage.unity'));outputPreserved=(@($unexpected+$missing|Where-Object {$_ -like 'output\*'}).Count -eq 0);priorTmpPreserved=(@($unexpected+$missing|Where-Object {$_ -like '.codex_tmp\*'}).Count -eq 0);pass=($unexpected.Count -eq 0 -and $missing.Count -eq 0 -and $staged.Count -eq 0 -and $head -eq '7644fdcdf7adb793fcc6e3ac9d6540f317766ec6' -and $remote -eq $head)}
$result|ConvertTo-Json -Depth 6|Set-Content -LiteralPath (Join-Path $evidence 'protection_result.json') -Encoding utf8
$result|ConvertTo-Json -Depth 6
New-Item -ItemType Directory -Path (Join-Path $evidence 'Harness'),(Join-Path $evidence 'SourceAfter') -Force | Out-Null
foreach($name in @('prepare.ps1','install_probe.ps1','run.ps1','HumanProbe.cs','HumanProbeEntry.cs','analyze.ps1','finalize.ps1')){Copy-Item -LiteralPath (Join-Path $PSScriptRoot $name) -Destination (Join-Path $evidence 'Harness')}
foreach($name in @('PlayerMover.cs','RopeController.cs','TutorialSignInscription.cs','TutorialSectionGuide.cs')){Copy-Item -LiteralPath (Join-Path $root "Assets\Scripts\$name") -Destination (Join-Path $evidence 'SourceAfter')}
foreach($path in @('Assets\Scripts\FullSystems.cs','Assets\Editor\FullSystemsEntry.cs','Assets\Scripts\FixTrace.cs','Assets\Editor\CheckpointMergedBridgeFix.cs')){Copy-Item -LiteralPath (Join-Path $PSScriptRoot "Project\$path") -Destination (Join-Path $evidence 'Harness')}
& git @gitArgs diff --check -- Assets/Scripts/PlayerMover.cs Assets/Scripts/RopeController.cs Assets/Scripts/TutorialSignInscription.cs Assets/Scripts/TutorialSectionGuide.cs README.md Docs/LEARNING_LOG.md
if($LASTEXITCODE -ne 0){throw 'Changed file whitespace check failed'}
if(!$result.pass){throw 'Git/file protection check failed'}
