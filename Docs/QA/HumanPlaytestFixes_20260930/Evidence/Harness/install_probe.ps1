$ErrorActionPreference='Stop'
$root=(Get-Location).Path
$copy=Join-Path $PSScriptRoot 'Project'
foreach($name in @('PlayerMover.cs','PrototypeAudioFeedback.cs','TutorialSignInscription.cs','TutorialSectionGuide.cs','RopeController.cs')){Copy-Item -LiteralPath (Join-Path $root "Assets\Scripts\$name") -Destination (Join-Path $copy "Assets\Scripts\$name")}
$path=Join-Path $copy 'Assets\Scripts\PlayerMover.cs'
$text=[IO.File]::ReadAllText($path).Replace('Input.GetKey(KeyCode.A)','HumanProbeInput.GetKey(KeyCode.A)').Replace('Input.GetKey(KeyCode.D)','HumanProbeInput.GetKey(KeyCode.D)').Replace('Input.GetButtonDown("Jump")','HumanProbeInput.GetButtonDown("Jump")')
[IO.File]::WriteAllText($path,$text,[Text.UTF8Encoding]::new($false))
$path=Join-Path $copy 'Assets\Scripts\PrototypeAudioFeedback.cs'
$text=[IO.File]::ReadAllText($path).Replace('lastFootstepIndex = clipIndex;','HumanProbeInput.Footsteps++; lastFootstepIndex = clipIndex;')
[IO.File]::WriteAllText($path,$text,[Text.UTF8Encoding]::new($false))
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'HumanProbe.cs') -Destination (Join-Path $copy 'Assets\Scripts\HumanProbe.cs')
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'HumanProbeEntry.cs') -Destination (Join-Path $copy 'Assets\Editor\HumanProbeEntry.cs')
Copy-Item -LiteralPath (Join-Path $root '.codex_tmp\FullGameRegression_20260930\FullSystems.cs') -Destination (Join-Path $copy 'Assets\Scripts\FullSystems.cs')
Copy-Item -LiteralPath (Join-Path $root '.codex_tmp\FullGameRegression_20260930\FullSystemsEntry.cs') -Destination (Join-Path $copy 'Assets\Editor\FullSystemsEntry.cs')
Copy-Item -LiteralPath (Join-Path $root '.codex_tmp\FullGameRegression_20260930\DiagnosticProject\Assets\Scripts\FixTrace.cs') -Destination (Join-Path $copy 'Assets\Scripts\FixTrace.cs')
Copy-Item -LiteralPath (Join-Path $root '.codex_tmp\FullGameRegression_20260930\DiagnosticProject\Assets\Editor\CheckpointMergedBridgeFix.cs') -Destination (Join-Path $copy 'Assets\Editor\CheckpointMergedBridgeFix.cs')
Write-Output 'Installed diagnostic-only seams and drivers in new isolated project'
