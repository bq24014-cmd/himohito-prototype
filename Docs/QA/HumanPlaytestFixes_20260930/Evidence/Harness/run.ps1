param([string]$Case='All',[string]$Label='After',[string]$Entry='HumanProbeEntry.Run')
$ErrorActionPreference='Stop'
$root=(Get-Location).Path
$evidence=Join-Path $root "Docs\QA\HumanPlaytestFixes_20260930\Evidence\$Label"
if(Test-Path -LiteralPath $evidence){throw 'Fresh evidence output required'}
$project=Join-Path $PSScriptRoot 'Project'
$extra=if($Entry -eq 'FullSystemsEntry.Run'){@('-fullEvidence',('"'+$evidence+'"'))}elseif($Entry -eq 'CheckpointMergedBridgeFix.Run'){@('-fixCase',$Case,'-fixEvidence',('"'+$evidence+'"'))}else{@('-humanEvidence',('"'+$evidence+'"'),'-humanMode',$Case)}
$args=@('-batchmode','-force-d3d11','-projectPath',('"'+$project+'"'),'-executeMethod',$Entry)+$extra+@('-logFile',('"'+(Join-Path $root "Docs\QA\HumanPlaytestFixes_20260930\Evidence\$($Label)_unity.log")+'"'))
(Start-Process -FilePath 'C:\Program Files\Unity\Hub\Editor\6000.3.21f1\Editor\Unity.exe' -ArgumentList $args -WindowStyle Hidden -PassThru).Id
