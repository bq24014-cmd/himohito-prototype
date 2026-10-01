param([ValidateSet('A','B')][string]$Case)
$ErrorActionPreference='Stop'
$taskRoot=(Get-Location).Path
$taskProject=Join-Path $taskRoot '.codex_tmp/MainStageSceneDiffAudit_20261001/Project'
$taskInput=Join-Path $PSScriptRoot $(if($Case -eq 'A'){'HEAD_MainStage.unity'}else{'WorkingTree_MainStage.unity'})
$taskOut=Join-Path $PSScriptRoot ('Runtime'+$Case)
if(Test-Path -LiteralPath $taskOut){throw 'Fresh output required.'}
Copy-Item -LiteralPath $taskInput -Destination (Join-Path $taskProject 'Assets/Scenes/MainStage.unity')
$taskArgs=@('-batchmode','-force-d3d11','-projectPath',('"'+$taskProject+'"'),'-executeMethod','SceneAuditEntry.Run','-sceneAuditEvidence',('"'+$taskOut+'"'),'-logFile',('"'+(Join-Path $PSScriptRoot ($Case+'_unity.log'))+'"'))
$taskProcess=Start-Process -FilePath 'C:\Program Files\Unity\Hub\Editor\6000.3.21f1\Editor\Unity.exe' -ArgumentList $taskArgs -WindowStyle Hidden -PassThru
Write-Output ('CASE '+$Case+' PID '+$taskProcess.Id)
