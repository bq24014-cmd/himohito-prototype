$ErrorActionPreference = 'Stop'
$taskRoot = (Get-Location).Path
$taskEvidence = Join-Path $taskRoot 'Docs/QA/MainStageCleanReproduction_20261001/Evidence'
$taskPaths = Get-Content -Raw -LiteralPath (Join-Path $taskEvidence 'external_build_paths.json') | ConvertFrom-Json
$taskQa = $taskPaths.copies.QAProject.project
if (-not $taskQa.StartsWith((Join-Path $taskRoot '.codex_tmp/MainStageCleanReproduction_20261001/'))) { throw 'Unexpected QA path' }
New-Item -ItemType Directory -Path (Join-Path $taskQa 'Assets/QA') -Force | Out-Null
Copy-Item -LiteralPath (Join-Path $taskEvidence 'FullSystems.cs') -Destination (Join-Path $taskQa 'Assets/QA/FullSystems.cs')
Copy-Item -LiteralPath (Join-Path $taskEvidence 'ExternalFocused.cs') -Destination (Join-Path $taskQa 'Assets/QA/CleanFocused.cs')
Copy-Item -LiteralPath (Join-Path $taskEvidence 'ExternalEntry.cs') -Destination (Join-Path $taskQa 'Assets/Editor/CleanEntry.cs')
