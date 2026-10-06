$ErrorActionPreference='Stop'
$repo=$PWD.Path
$qa=Join-Path $repo 'Docs/QA/MainS3ReturnStepBProduction_20261006/Evidence'
$before=Get-Content -Raw -LiteralPath (Join-Path $qa 'ProtectionBefore.json') | ConvertFrom-Json
$changed=@()
foreach($row in $before.files) {
 $path=Join-Path $repo $row.path
 if(!(Test-Path -LiteralPath $path -PathType Leaf)){ $changed+=@{path=$row.path;reason='missing'};continue }
 $f=Get-Item -LiteralPath $path
 if($f.Length -ne $row.bytes -or (Get-FileHash -LiteralPath $path).Hash -ne $row.sha256){$changed+=@{path=$row.path;reason='content changed'}}
}
$oldChanged=@()
foreach($row in $before.previousFiles){
 $path=Join-Path $repo $row.path
 if(!(Test-Path -LiteralPath $path -PathType Leaf)){ $oldChanged+=@{path=$row.path;reason='missing'};continue }
 $f=Get-Item -LiteralPath $path
 if($f.Length -ne $row.bytes -or $f.LastWriteTimeUtc.Ticks -ne $row.utcTicks){$oldChanged+=@{path=$row.path;reason='metadata changed'}}
}
$env:GIT_OPTIONAL_LOCKS='0'
$status=@(& git -c "safe.directory=$repo" status --short --untracked-files=all)
$beforeExisting=@($before.status | Where-Object {$_ -notmatch '^\?\? Docs/QA/MainS3ReturnStepBProduction_20261006/'})
$nowExisting=@($status | Where-Object {$_ -notmatch '^.. (Assets/Scripts/MainS3MarkerVisual\.cs(\.meta)?|Assets/Resources/MainS3Marker\.shader(\.meta)?|Docs/QA/MainS3ReturnStepBProduction_20261006/)'})
$statusDelta=@(Compare-Object -ReferenceObject $beforeExisting -DifferenceObject $nowExisting)
$bgm=Join-Path $repo 'Assets/Resources/Audio/YasashiiOdori.mp3'
$scene=Join-Path $repo 'Assets/Scenes/MainStage.unity'
$record=@{utc=[datetime]::UtcNow.ToString('o');protectedHashCount=$before.files.Count;protectedHashChanges=$changed;previousMetadataFileCount=$before.previousFiles.Count;previousFileChanges=$oldChanged;existingGitStatusDelta=$statusDelta;currentStatus=$status;bgmSHA256=(Get-FileHash -LiteralPath $bgm).Hash;mainStageSHA256=(Get-FileHash -LiteralPath $scene).Hash;status=if($changed.Count -eq 0 -and $oldChanged.Count -eq 0 -and $statusDelta.Count -eq 0){'PASS'}else{'FAIL'}}
$record | ConvertTo-Json -Depth 8 | Set-Content -LiteralPath (Join-Path $qa 'ProtectionAfter.json') -Encoding utf8
[pscustomobject]@{status=$record.status;protectedHashCount=$record.protectedHashCount;protectedHashChanges=$changed;previousMetadataFileCount=$record.previousMetadataFileCount;previousFileChanges=$oldChanged;existingGitStatusDelta=$statusDelta;bgmSHA256=$record.bgmSHA256;mainStageSHA256=$record.mainStageSHA256}|ConvertTo-Json -Depth 6
if($record.status -ne 'PASS'){throw 'Protection validation failed; STOP'}
