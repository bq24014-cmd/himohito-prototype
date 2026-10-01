$ErrorActionPreference='Stop'
$taskRoot=(Get-Location).Path
$taskEvidence=Join-Path $taskRoot 'Docs/QA/MainStageCleanReproduction_20261001/Evidence/DeterministicSceneSync'
$taskPaths=Get-Content (Join-Path $taskEvidence 'paths.json') -Raw | ConvertFrom-Json
$taskOutput=Join-Path $taskRoot 'output/DeterministicSceneSyncAB_20261001_154205'
if(Test-Path -LiteralPath $taskOutput){throw 'Existing output: STOP'}
New-Item -ItemType Directory -Path $taskOutput | Out-Null
$taskRecords=@()
foreach($taskLabel in @('A','B')){
    $taskSource=Join-Path $taskPaths.copies.$taskLabel.project 'Builds/Windows/Game'
    $taskDest=Join-Path $taskOutput ($taskLabel+'/Game')
    $taskAscii='C:\Users\Public\HimoHitoDeterministicAB_20261001_154205_'+$taskLabel
    foreach($taskCopy in @($taskDest,$taskAscii)){
        if(Test-Path -LiteralPath $taskCopy){throw 'Existing destination: STOP'}
        New-Item -ItemType Directory -Path $taskCopy | Out-Null
        Get-ChildItem -LiteralPath $taskSource | Copy-Item -Destination $taskCopy -Recurse
    }
    $taskManifest=@()
    foreach($taskFile in Get-ChildItem -LiteralPath $taskSource -File -Recurse){
        $taskRel=$taskFile.FullName.Substring($taskSource.Length+1)
        $taskHash=(Get-FileHash -LiteralPath $taskFile.FullName -Algorithm SHA256).Hash
        foreach($taskCopy in @($taskDest,$taskAscii)){
            if((Get-FileHash -LiteralPath (Join-Path $taskCopy $taskRel) -Algorithm SHA256).Hash -ne $taskHash){throw 'Copy hash mismatch'}
        }
        $taskManifest+=[pscustomobject]@{file=$taskRel;sha256=$taskHash;size=$taskFile.Length}
    }
    $taskManifest | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $taskEvidence ('player_manifest_'+$taskLabel+'.json')) -Encoding utf8
    $taskExe=Join-Path $taskAscii 'HimoHitoPrototype.exe'
    $taskLog=Join-Path $taskEvidence ('Exe'+$taskLabel+'.log')
    $taskProc=Start-Process -FilePath $taskExe -WorkingDirectory $taskAscii -ArgumentList @('-screen-fullscreen','0','-screen-width','1280','-screen-height','720','-logFile',('"'+$taskLog+'"')) -WindowStyle Normal -PassThru
    $taskRecords+=[pscustomobject]@{label=$taskLabel;exe=$taskExe;output=$taskDest;process_id=$taskProc.Id;started=(Get-Date).ToString('o');copied_files=$taskManifest.Count;copy_sha_match=$true;ui_verified=$false}
}
$taskRecords | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $taskEvidence 'exe_launch.json') -Encoding utf8
$taskRecords
