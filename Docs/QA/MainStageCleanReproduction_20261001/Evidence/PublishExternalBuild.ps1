$ErrorActionPreference = 'Stop'
$taskRoot = (Get-Location).Path
$taskEvidence = Join-Path $taskRoot 'Docs/QA/MainStageCleanReproduction_20261001/Evidence'
$taskPaths = Get-Content (Join-Path $taskEvidence 'external_build_paths.json') -Raw | ConvertFrom-Json
$taskBuild = Join-Path $taskPaths.copies.BuildProject.project 'Builds/Windows/Game'
$taskOutput = Join-Path $taskRoot 'output/MainStageCleanReproduction_ExternalBGM_20261001_151833/Game'
$taskAscii = 'C:\Users\Public\HimoHitoCleanRepro_ExternalBGM_20261001_151833'
foreach ($taskDestination in @($taskOutput, $taskAscii)) {
    if (Test-Path -LiteralPath $taskDestination) { throw "New destination already exists: $taskDestination" }
    New-Item -ItemType Directory -Path $taskDestination | Out-Null
    Get-ChildItem -LiteralPath $taskBuild | Copy-Item -Destination $taskDestination -Recurse
}
$taskManifest = @()
foreach ($taskFile in Get-ChildItem -LiteralPath $taskBuild -File -Recurse) {
    $taskRelative = $taskFile.FullName.Substring($taskBuild.Length + 1)
    $taskHash = (Get-FileHash -LiteralPath $taskFile.FullName -Algorithm SHA256).Hash
    foreach ($taskDestination in @($taskOutput, $taskAscii)) {
        if ((Get-FileHash -LiteralPath (Join-Path $taskDestination $taskRelative) -Algorithm SHA256).Hash -ne $taskHash) { throw "Copy mismatch: $taskRelative" }
    }
    $taskManifest += [pscustomobject]@{file=$taskRelative;size=$taskFile.Length;sha256=$taskHash}
}
$taskManifest | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $taskEvidence 'external_player_manifest.json') -Encoding utf8
$taskExe = Join-Path $taskAscii 'HimoHitoPrototype.exe'
$taskLog = Join-Path $taskEvidence 'ExternalBGMExeSmoke.log'
$taskProcess = Start-Process -FilePath $taskExe -ArgumentList @('-screen-fullscreen','0','-screen-width','1280','-screen-height','720','-logFile',('"' + $taskLog + '"')) -WorkingDirectory $taskAscii -WindowStyle Normal -PassThru
[pscustomobject]@{build_source=$taskBuild;output=$taskOutput;ascii_launch_directory=$taskAscii;exe=$taskExe;process_id=$taskProcess.Id;launch_time=(Get-Date).ToString('o');copied_files=$taskManifest.Count;all_copied_hashes_match=$true;ui_smoke='NOT_YET_VERIFIED'} | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $taskEvidence 'external_exe_launch.json') -Encoding utf8
Get-Content -LiteralPath (Join-Path $taskEvidence 'external_exe_launch.json')
