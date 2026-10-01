$ErrorActionPreference='Stop'
$root=(Get-Location).Path
$evidence=Join-Path $root 'Docs\QA\HumanPlaytestFixes_20260930\Evidence'
Add-Type -AssemblyName System.Drawing
function Luma($rgb){
 $linear=@($rgb|ForEach-Object {$x=[double]$_/255;if($x -le .04045){$x/12.92}else{[Math]::Pow(($x+.055)/1.055,2.4)}})
 .2126*$linear[0]+.7152*$linear[1]+.0722*$linear[2]
}
$contrast=@()
foreach($label in @('BeforeSigns','After')){
 $image=[Drawing.Bitmap]::new((Join-Path $evidence "$label\sign_2_detail.png"))
 $rgb=@(0,0,0);$count=0
 for($x=990;$x -lt 1010;$x++){for($y=285;$y -lt 305;$y++){$c=$image.GetPixel($x,$y);$rgb[0]+=$c.R;$rgb[1]+=$c.G;$rgb[2]+=$c.B;$count++}}
 $rgb=@($rgb|ForEach-Object {$_/$count});$image.Dispose()
 $ink=if($label -eq 'BeforeSigns'){@(255,232.05,172.89)}else{@(68.85,25.5,14.025)}
 $a=Luma $rgb;$b=Luma $ink
 $contrast+=[pscustomobject]@{label=$label;backgroundSample='sign_2_detail.png pixels x990..1009 y285..304 (wood beside W)';backgroundRGB=$rgb;declaredForegroundRGB=$ink;approximateContrast=([Math]::Max($a,$b)+.05)/([Math]::Min($a,$b)+.05);note='Approximate contrast: actual rendered wood patch vs declared full-opacity ink; not a UI WCAG certification'}
}
$contrast | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $evidence 'contrast.json') -Encoding utf8
$angles=@()
foreach($label in @('Before','After')){
 foreach($sample in @('left_of_center','near_center','at_center')){
  $rows=Import-Csv -LiteralPath (Join-Path $evidence "$label\angles_$sample.csv")
  foreach($name in @('Main S09 Green Right Bank','Main S09 Center Hook')){
   $selected=@($rows|Where-Object {$_.resolved -eq $name});$actual=@($selected|Where-Object {$_.available -eq 'True' -and $_.highlight -eq $name})
   $metrics=$selected|Measure-Object -Property angle -Minimum -Maximum
   $angles+=[pscustomobject]@{label=$label;sample=$sample;position=@($rows[0].x,$rows[0].y);selected=$rows[0].selected;target=$name;sampleCount=$selected.Count;availableHighlightCount=$actual.Count;minimum=$metrics.Minimum;maximum=$metrics.Maximum;span=if($selected.Count){[double]$metrics.Maximum-[double]$metrics.Minimum}else{0};samplingStep=.25;rightDistance=$rows[0].right_distance}
  }
 }
}
$angles | ConvertTo-Json -Depth 5 | Set-Content -LiteralPath (Join-Path $evidence 'angular_summary.json') -Encoding utf8
$contrast | Format-Table label,approximateContrast
$angles | Format-Table label,sample,target,sampleCount,availableHighlightCount,minimum,maximum,span
