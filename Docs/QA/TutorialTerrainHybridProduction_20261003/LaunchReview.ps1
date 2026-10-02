param([ValidateSet('Launch','Check')][string]$Mode='Launch')
$ErrorActionPreference='Stop'
$root=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../..'))
$build=Join-Path $root 'output/TutorialTerrainHybridProduction_20261003/Windows'
$exe=Join-Path $build 'HimoHitoTutorialTerrainHybridProduction.exe'
$evidence=Join-Path $PSScriptRoot 'Evidence'
function NewJson($path,$data){if(Test-Path -LiteralPath $path){throw "Refuse overwrite: $path"};[IO.File]::WriteAllText($path,($data|ConvertTo-Json -Depth 6),[Text.UTF8Encoding]::new($false))}
if($Mode-eq'Launch'){
 $audit=Get-Content -LiteralPath (Join-Path $evidence 'PlayerAssemblyAudit02/PLAYER_ASSEMBLY_AUDIT.json') -Raw|ConvertFrom-Json
 if(-not$audit.passed){throw 'Built player assembly audit must pass before review'}
 if(-not(Test-Path -LiteralPath $exe)){throw 'Missing review exe'}
 $stamp=Get-Date -Format 'yyyyMMdd_HHmmss'
 $log=Join-Path $evidence "HumanPlayer_$stamp.log"
 $arguments='-screen-fullscreen 0 -screen-width 1440 -screen-height 810 -logFile "'+$log+'"'
 $process=Start-Process -FilePath $exe -WorkingDirectory $build -ArgumentList $arguments -WindowStyle Normal -PassThru
 NewJson (Join-Path $evidence "Launch_$stamp.json") (@{utc=[DateTimeOffset]::UtcNow.ToString('o');pid=$process.Id;exe=$exe;exe_sha256=(Get-FileHash -LiteralPath $exe).Hash;player_assembly_sha256=$audit.sha256;log=$log;args=$arguments;human_review='PENDING; launch does not constitute visual approval'})
 "Launched PID $($process.Id); log $log"
}else{
 $latest=Get-ChildItem -LiteralPath $evidence -Filter 'Launch_*.json'|Sort-Object LastWriteTimeUtc -Descending|Select-Object -First 1
 if(-not$latest){throw 'No launch record'}
 $launch=Get-Content -LiteralPath $latest.FullName -Raw|ConvertFrom-Json
 $process=Get-Process -Id $launch.pid -ErrorAction SilentlyContinue
 $logText=if(Test-Path -LiteralPath $launch.log){Get-Content -LiteralPath $launch.log -Raw}else{''}
 $report=@{utc=[DateTimeOffset]::UtcNow.ToString('o');pid=$launch.pid;alive=($null-ne$process);responding=$(if($process){$process.Responding}else{$false});window_title=$(if($process){$process.MainWindowTitle}else{''});log=$launch.log;input_initialized=$logText.Contains('Input System module state changed to: Initialized');error_lines=@(($logText-split"`n")|Where-Object{$_-match 'Exception|Error|MissingReference|NullReference'});human_review='PENDING; process/input readiness only, not screen or keyboard verification'}
 NewJson (Join-Path $evidence ('LaunchCheck_'+(Get-Date -Format 'yyyyMMdd_HHmmss')+'.json')) $report
 $report|ConvertTo-Json -Depth 5
}
