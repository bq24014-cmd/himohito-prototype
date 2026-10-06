$ErrorActionPreference='Stop'
$qa=Join-Path $PWD 'Docs/QA/MainS3ReturnStepBProduction_20261006/Evidence'
$archive=Join-Path $qa 'RunLogsFinal.zip'
if(Test-Path -LiteralPath $archive){throw 'Archive exists; refuse overwrite'}
Add-Type -AssemblyName System.IO.Compression.FileSystem
$zip=[System.IO.Compression.ZipFile]::Open($archive,[System.IO.Compression.ZipArchiveMode]::Create)
try{
 foreach($name in @('Run01.log','Run02.log','Run03.log','Run04.log')){
  [System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zip,(Join-Path $qa $name),$name,[System.IO.Compression.CompressionLevel]::Optimal)|Out-Null
 }
}finally{$zip.Dispose()}
$rows=@()
$zip=[System.IO.Compression.ZipFile]::OpenRead($archive)
try{
 foreach($entry in $zip.Entries){
  $stream=$entry.Open(); $alg=[System.Security.Cryptography.SHA256]::Create()
  try{$copied=[Convert]::ToHexString($alg.ComputeHash($stream))}finally{$stream.Dispose();$alg.Dispose()}
  $original=(Get-FileHash -LiteralPath (Join-Path $qa $entry.FullName)).Hash
  $rows+=@{name=$entry.FullName;bytes=$entry.Length;originalSHA256=$original;archiveEntrySHA256=$copied;match=($original -ceq $copied)}
 }
}finally{$zip.Dispose()}
$pass=$rows.Count -eq 4 -and !(@($rows|Where-Object {!$_.match}).Count)
@{status=if($pass){'PASS'}else{'FAIL'};archive='RunLogsFinal.zip';archiveSHA256=(Get-FileHash -LiteralPath $archive).Hash;entries=$rows}|ConvertTo-Json -Depth 6|Set-Content -LiteralPath (Join-Path $qa 'RunLogsFinalManifest.json') -Encoding utf8
if(!$pass){throw 'Log archive content mismatch'}
$rows|ConvertTo-Json -Depth 4
