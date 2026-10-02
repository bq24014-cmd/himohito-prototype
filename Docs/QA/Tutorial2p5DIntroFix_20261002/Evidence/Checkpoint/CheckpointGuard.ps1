param([Parameter(Mandatory=$true)][string]$OutputName)
$ErrorActionPreference = 'Stop'
$repoRoot = (Get-Location).Path
$recordDir = Join-Path $repoRoot 'Docs/QA/Tutorial2p5DIntroFix_20261002/Evidence/Checkpoint'
$recordPath = Join-Path $recordDir $OutputName
if (Test-Path -LiteralPath $recordPath) { throw 'Refusing to overwrite an existing protection record.' }
function Digest([string]$value) {
    $sha = [Security.Cryptography.SHA256]::Create()
    try { return [Convert]::ToHexString($sha.ComputeHash([Text.Encoding]::UTF8.GetBytes($value))) }
    finally { $sha.Dispose() }
}
$sourceFiles = @()
foreach ($relative in @('Assets','ProjectSettings','Packages')) {
    $sourceFiles += Get-ChildItem -LiteralPath (Join-Path $repoRoot $relative) -File -Recurse -Force
}
foreach ($relative in @('README.md','Docs/LEARNING_LOG.md','.gitignore','.gitattributes')) {
    $sourceFiles += Get-Item -LiteralPath (Join-Path $repoRoot $relative)
}
$sources = @($sourceFiles | Sort-Object FullName | ForEach-Object {
    [ordered]@{Path=$_.FullName.Substring($repoRoot.Length+1).Replace('\','/');Bytes=$_.Length;
        SHA256=(Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash}
})
$generated = @()
foreach ($relative in @('output','Builds','.codex_tmp')) {
    $absolute = Join-Path $repoRoot $relative
    $entries = @()
    if (Test-Path -LiteralPath $absolute) {
        $entries = @(Get-ChildItem -LiteralPath $absolute -File -Recurse -Force | Sort-Object FullName)
    }
    $lines = @($entries | ForEach-Object {
        $_.FullName.Substring($repoRoot.Length+1).Replace('\','/')+'|'+$_.Length+'|'+$_.LastWriteTimeUtc.Ticks
    })
    $executables = @($entries | Where-Object { $_.Extension -eq '.exe' } | ForEach-Object {
        [ordered]@{Path=$_.FullName.Substring($repoRoot.Length+1).Replace('\','/');
            SHA256=(Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash}
    })
    $generated += [ordered]@{Root=$relative;Exists=(Test-Path -LiteralPath $absolute);
        Files=$entries.Count;Bytes=($entries | Measure-Object Length -Sum).Sum;
        MetadataSHA256=(Digest ($lines -join "`n"));ExecutableHashes=$executables}
}
$drive = [IO.DriveInfo]::new('C:\')
$record = [ordered]@{TimestampUtc=[DateTime]::UtcNow.ToString('o');
    CDriveFreeBytes=$drive.AvailableFreeSpace;SourceCount=$sources.Count;
    Sources=$sources;Generated=$generated;
    Scope='Assets/ProjectSettings/Packages/docs byte hashes; output/Builds/.codex_tmp file path+size+mtime and exe hashes. No cleanup.'}
$record | ConvertTo-Json -Depth 8 | Out-File -LiteralPath $recordPath -Encoding utf8
Write-Output ([ordered]@{Record=$OutputName;Sources=$sources.Count;Generated=$generated;FreeGiB=$drive.AvailableFreeSpace/1GB} | ConvertTo-Json -Depth 5)
