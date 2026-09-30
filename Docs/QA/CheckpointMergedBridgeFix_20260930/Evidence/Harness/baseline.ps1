$ErrorActionPreference='Stop'
$root='C:\Users\田尻大翔\Documents\Codex\2026-08-07\new-chat\outputs\HimoHitoPrototype'
$evidence=Join-Path $root 'Docs\QA\CheckpointMergedBridgeFix_20260930\Evidence'
$files=@(Get-ChildItem -LiteralPath $root -File)
foreach($folder in @('Assets','Packages','ProjectSettings','Docs','output','.codex_tmp\CheckpointMergedBridgeRepro_20260930')) {
 $files+=@(Get-ChildItem -LiteralPath (Join-Path $root $folder) -File -Recurse)
}
$manifest=@($files | ForEach-Object { [pscustomobject]@{ path=$_.FullName.Substring($root.Length+1); sha256=(Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash } })
New-Item -ItemType Directory -Path $evidence -Force | Out-Null
$manifest | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath (Join-Path $evidence 'sha_start.json') -Encoding UTF8
[pscustomobject]@{ branch=(& git branch --show-current); local=(& git rev-parse HEAD); remote=(& git rev-parse origin/main); status=@(& git status --short); protectedFiles=$manifest.Count } | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath (Join-Path $evidence 'git_start.json') -Encoding UTF8
Write-Output "Baseline saved: $($manifest.Count) files"
