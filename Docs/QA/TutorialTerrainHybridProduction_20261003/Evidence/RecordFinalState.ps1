$ErrorActionPreference='Stop'
$env:GIT_OPTIONAL_LOCKS='0'
$root=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../../..'))
$qa=Split-Path $PSScriptRoot -Parent
$build=Join-Path $root 'output/TutorialTerrainHybridProduction_20261003/Windows'
$target=Join-Path $PSScriptRoot 'FinalState.json'
if(Test-Path -LiteralPath $target){throw 'Refuse existing final state'}
function Git([string[]]$params){return ((& git.exe -c "safe.directory=$root" -c core.quotepath=false -C $root @params)-join"`n")}
$diffCheck=Git @('diff','--check');$diffCode=$LASTEXITCODE
$whitespace=@()
$newPaths=@('Assets/Scripts/TutorialTerrainHybridVisual.cs')
$newPaths+=@(Get-ChildItem -LiteralPath $qa -File -Filter '*.md'|ForEach-Object{$_.FullName})
foreach($path in $newPaths){$full=if([IO.Path]::IsPathRooted($path)){$path}else{Join-Path $root $path};$line=0;foreach($s in Get-Content -LiteralPath $full){$line++;if($s-match'\s+$'){$whitespace+=@{path=$path;line=$line}}}}
$assembly=Join-Path $build 'HimoHitoTutorialTerrainHybridProduction_Data/Managed/Assembly-CSharp.dll'
$audit=Get-Content -LiteralPath (Join-Path $PSScriptRoot 'PlayerAssemblyAudit02/PLAYER_ASSEMBLY_AUDIT.json') -Raw|ConvertFrom-Json
$state=[ordered]@{
 utc=[DateTimeOffset]::UtcNow.ToString('o');branch=(Git @('branch','--show-current'));HEAD=(Git @('rev-parse','HEAD'));origin_main=(Git @('rev-parse','origin/main'))
 status=(Git @('status','--short'));staged=(Git @('diff','--cached','--name-only'));tracked_status=(Git @('status','--short','-uno'))
 diff_check_exit=$diffCode;diff_check=$diffCheck;new_runtime_and_document_trailing_whitespace=$whitespace
 build_result_original=(Get-Content -LiteralPath (Join-Path $build 'BUILD_RESULT.txt') -Raw)
 artifact_reaudit_passed=$audit.passed;artifact_sha256=$audit.sha256;built_dll_unchanged=((Get-FileHash -LiteralPath $assembly).Hash-eq$audit.sha256)
 artifact_reaudit_explanation='Original build succeeded. Initial metadata audit failed due to QA-only Reflection ambiguity; Audit02 rereads the unchanged Windows DLL with corrected property lookup. No binary replacement or Production Source change.'
 human_review='PENDING; TP1 technical QA only';commit_performed=$false;push_performed=$false
}
[IO.File]::WriteAllText($target,($state|ConvertTo-Json -Depth 8),[Text.UTF8Encoding]::new($false))
([pscustomobject]$state)|Select-Object branch,HEAD,origin_main,staged,diff_check_exit,new_runtime_and_document_trailing_whitespace,artifact_reaudit_passed,built_dll_unchanged,human_review|ConvertTo-Json -Depth 4
