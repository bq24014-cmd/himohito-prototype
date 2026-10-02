$ErrorActionPreference = 'Stop'
$repoRoot = (Get-Location).Path
$recordDir = Join-Path $repoRoot 'Docs/QA/Tutorial2p5DIntroFix_20261002/Evidence/Checkpoint'
$selectionPath = Join-Path $recordDir 'Selection.json'
$localPath = Join-Path $recordDir 'LocalOnly.json'
if ((Test-Path -LiteralPath $selectionPath) -or (Test-Path -LiteralPath $localPath)) { throw 'Selection records already exist; refusing overwrite.' }
$production = @(
    'Assets/Scripts/HimoHitoCraftRoomBackground.cs',
    'Assets/Scripts/TutorialCraftRoomLayers.cs',
    'Assets/Scripts/TutorialCraftRoomLayers.cs.meta',
    'Assets/Resources/TutorialBackgroundEdge.shader',
    'Assets/Resources/TutorialBackgroundEdge.shader.meta',
    'Assets/Resources/TutorialBackgroundFloor.shader',
    'Assets/Resources/TutorialBackgroundFloor.shader.meta',
    'Assets/Resources/Art/Tutorial2p5D.meta',
    'Assets/Resources/Art/Tutorial2p5D/Tutorial2p5D_Far.png',
    'Assets/Resources/Art/Tutorial2p5D/Tutorial2p5D_Far.png.meta',
    'Assets/Resources/Art/Tutorial2p5D/Tutorial2p5D_Mid.png',
    'Assets/Resources/Art/Tutorial2p5D/Tutorial2p5D_Mid.png.meta',
    'Assets/Resources/Art/Tutorial2p5D/Tutorial2p5D_NearRefined.png',
    'Assets/Resources/Art/Tutorial2p5D/Tutorial2p5D_NearRefined.png.meta'
)
$qaRoots = @(
    'Docs/QA/Tutorial2p5DProof_20261001',
    'Docs/QA/Tutorial2p5DForegroundRefine_20261001',
    'Docs/QA/Tutorial2p5DAllSections_20261001',
    'Docs/QA/Tutorial2p5DProductionIntegration_20261001',
    'Docs/QA/Tutorial2p5DIntroFix_20261002',
    'Docs/VisualConcepts/Tutorial2p5D_20261001'
)
$selected = @($production | ForEach-Object { [pscustomobject]@{Path=$_;Group='production'} })
$excluded = @()
foreach ($qaRoot in $qaRoots) {
    foreach ($file in Get-ChildItem -LiteralPath $qaRoot -File -Recurse) {
        $relative = $file.FullName.Substring($repoRoot.Length+1).Replace('\','/')
        $reason = $null
        if ($file.Extension -eq '.ppm') { $reason = 'Raw capture; curated PNG/GIF and results are saved. Keep local, not approved for deletion.' }
        elseif ($relative -match '/ImportedMetadata/') { $reason = 'Generated trial-only import metadata; not Production asset identity.' }
        elseif ($relative -match '/BeforeProduction/') { $reason = 'Diagnostic source snapshot; patches, Git baseline, and results saved instead.' }
        elseif ($relative -match '/(__pycache__|Library|Temp|Logs|obj)/') { $reason = 'Generated cache; not adoption evidence.' }
        if ($reason) {
            $excluded += [pscustomobject]@{Path=$relative;Bytes=$file.Length;Reason=$reason}
        } else {
            $selected += [pscustomobject]@{Path=$relative;Group='production_qa'}
        }
    }
}
foreach ($file in Get-ChildItem -LiteralPath 'Docs/Maintenance/DiskUsageAudit_20261002' -File -Recurse) {
    $selected += [pscustomobject]@{Path=$file.FullName.Substring($repoRoot.Length+1).Replace('\','/');Group='disk_audit'}
}
$records = @($selected | Sort-Object Path -Unique | ForEach-Object {
    $file = Get-Item -LiteralPath $_.Path
    [ordered]@{Path=$_.Path;Group=$_.Group;Bytes=$file.Length;
        SHA256=(Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash}
})
if (@($records | Where-Object {$_.Bytes -ge 100MB}).Count) { throw 'A selected file reaches GitHub file limit.' }
[ordered]@{Baseline='702a4bc044b0eb35bedb552753ca8495599bc1ee';Files=$records;
    Note='Selection.json and LocalOnly.json are also explicitly staged as checkpoint manifests.'} |
    ConvertTo-Json -Depth 5 | Out-File -LiteralPath $selectionPath -Encoding utf8
[ordered]@{Files=$excluded;OtherProtected=@('Assets/Scenes/MainStage.unity','README.md','Docs/LEARNING_LOG.md',
    'Assets/Resources/Audio/YasashiiOdori.mp3','output/','Builds/','.codex_tmp/',
    'Docs/QA/FullGameRegression_20260930/','Docs/QA/HumanPlaytest_20260930/',
    'Docs/QA/MainStageCleanReproduction_20261001/Evidence/ArchiveExpansion/',
    'Docs/QA/MainStageSceneDiffAudit_20261001/Evidence/__pycache__/');CleanupAuthorized=$false} |
    ConvertTo-Json -Depth 5 | Out-File -LiteralPath $localPath -Encoding utf8
$records | Group-Object Group | ForEach-Object {
    [pscustomobject]@{Group=$_.Name;Files=$_.Count;MiB=[Math]::Round(($_.Group | Measure-Object Bytes -Sum).Sum/1MB,2)}
} | Format-Table -AutoSize
Write-Output ("Local-only evidence files: "+$excluded.Count)
