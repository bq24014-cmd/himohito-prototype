$ErrorActionPreference='Stop'
$root=(Get-Location).Path
$taskRoot=$PSScriptRoot
$evidence=Join-Path $root 'Docs\QA\HumanPlaytestFixes_20260930\Evidence'
$copy=Join-Path $taskRoot 'Project'
if(Test-Path -LiteralPath $copy){throw 'Fresh copy required'}
New-Item -ItemType Directory -Path $evidence -Force | Out-Null
$files=@(Get-ChildItem -LiteralPath $root -File)
foreach($dir in @('Assets','Packages','ProjectSettings','Docs','output','Builds','.codex_tmp')){
 $path=Join-Path $root $dir
 if(Test-Path -LiteralPath $path){$files+=@(Get-ChildItem -LiteralPath $path -File -Recurse | Where-Object {!$_.FullName.StartsWith($taskRoot+'\') -and !$_.FullName.StartsWith((Split-Path $evidence)+'\')})}
}
$manifest=@($files | ForEach-Object {[pscustomobject]@{path=$_.FullName.Substring($root.Length+1);sha256=(Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash}})
$manifest | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath (Join-Path $evidence 'sha_start.json') -Encoding utf8
$gitArgs=@('-c',"safe.directory=$($root.Replace('\','/'))")
[pscustomobject]@{utc=[DateTime]::UtcNow.ToString('o');head=(& git @gitArgs rev-parse HEAD);remote=(& git @gitArgs rev-parse origin/main);branch=(& git @gitArgs branch --show-current);status=@(& git @gitArgs status --short);protectedFiles=$manifest.Count} | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $evidence 'git_start.json') -Encoding utf8
New-Item -ItemType Directory -Path $copy | Out-Null
foreach($dir in @('Assets','Packages','ProjectSettings')){Copy-Item -LiteralPath (Join-Path $root $dir) -Destination $copy -Recurse}
New-Item -ItemType Directory -Path (Join-Path $evidence 'SourceBefore') | Out-Null
foreach($name in @('PlayerMover.cs','TutorialSignInscription.cs','TutorialSectionGuide.cs','RopeController.cs')){Copy-Item -LiteralPath (Join-Path $root "Assets\Scripts\$name") -Destination (Join-Path $evidence 'SourceBefore')}
$path=Join-Path $copy 'Packages\manifest.json'
$manifest=[IO.File]::ReadAllText($path)|ConvertFrom-Json
$manifest.dependencies | Add-Member -NotePropertyName 'com.unity.modules.imageconversion' -NotePropertyValue '1.0.0' -Force
[IO.File]::WriteAllText($path,($manifest|ConvertTo-Json -Depth 10),[Text.UTF8Encoding]::new($false))
Write-Output "Baseline $($files.Count) files; copy $copy"
