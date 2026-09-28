# Repack existing ImageGen art; preserve filenames, dimensions and Unity metadata.
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing
Add-Type -AssemblyName System.IO.Compression.FileSystem
$project=Split-Path $PSScriptRoot
$out=Join-Path $project 'Artifacts/MarineUIRefine'
$candidate=Join-Path $out 'Candidate'
New-Item -ItemType Directory -Force $candidate | Out-Null
$palette=@('#032534','#063848','#10556b','#177f91','#259eb3','#51cbd0','#84efe5','#d4f6ed','#f7efcf','#3b291e','#63422a','#986034','#bf8745','#e2af61','#f5cd7e','#ffe19b','#204b31','#3c7536','#68963c','#a8bc51','#6f3045','#ba4650','#ef7060','#ffa586','#635380','#9a85b1','#eeeeed') | ForEach-Object {[Drawing.ColorTranslator]::FromHtml($_)}
function Quantize([Drawing.Color]$c){
 if($c.A -lt 128){return [Drawing.Color]::Transparent}
 $best=$palette[0];$distance=1e10
 foreach($p in $palette){$d=2*[Math]::Pow($c.R-$p.R,2)+3*[Math]::Pow($c.G-$p.G,2)+[Math]::Pow($c.B-$p.B,2);if($d -lt $distance){$distance=$d;$best=$p}}
 return $best
}
$zip=[IO.Compression.ZipFile]::OpenRead((Join-Path $out 'BEFORE_39.zip'))
foreach($entry in $zip.Entries){
 if(!$entry.Name.EndsWith('.png')){continue}
 $stream=$entry.Open();$src=[Drawing.Bitmap]::FromStream($stream)
 $dst=[Drawing.Bitmap]::new($src.Width,$src.Height,[Drawing.Imaging.PixelFormat]::Format32bppArgb)
 $name=[IO.Path]::GetFileNameWithoutExtension($entry.Name)
 $step=if($name.StartsWith('decor_')){2}else{1}
 for($y=0;$y -lt $dst.Height;$y+=$step){for($x=0;$x -lt $dst.Width;$x+=$step){
  $color=Quantize ($src.GetPixel($x,$y))
  for($dy=0;$dy -lt $step;$dy++){for($dx=0;$dx -lt $step;$dx++){$dst.SetPixel($x+$dx,$y+$dy,$color)}}
 }}
 if($name -in @('panel','header','button','slot','selected_slot','key')){
  # Bend the existing rail cross-section around a 5px pixel radius. Merely
  # erasing corner pixels would leave open gaps on the 1px key border.
  for($y=0;$y -lt 32;$y++){for($x=0;$x -lt 32;$x++){
   $cx=[Math]::Min($x,31-$x);$cy=[Math]::Min($y,31-$y)
   if($name -eq 'header' -and $x -ge 2 -and $x -lt 30 -and $y -ge 2 -and $y -lt 30){$dst.SetPixel($x,$y,[Drawing.ColorTranslator]::FromHtml('#3b291e'))}
  }}
  for($y=0;$y -lt 32;$y++){for($x=0;$x -lt 32;$x++){
   $cx=[Math]::Min($x,31-$x);$cy=[Math]::Min($y,31-$y)
   if($cx -lt 5 -and $cy -lt 5){
    $depth=[int][Math]::Floor(5-[Math]::Sqrt([Math]::Pow(4.5-$cx,2)+[Math]::Pow(4.5-$cy,2)))
    $c=if($depth -lt 0){[Drawing.Color]::Transparent}else{$dst.GetPixel(16,$depth)}
    $dst.SetPixel($x,$y,$c)
   }
  }}
 }
 $dst.Save((Join-Path $candidate $entry.Name),[Drawing.Imaging.ImageFormat]::Png)
 $dst.Dispose();$src.Dispose();$stream.Dispose()
}
$zip.Dispose()
# Newly generated continuous foam strip; crop unused transparent sky, then use
# the same 24 logical pixels / 48px canvas as all other decorations.
$wavePath=Join-Path $out 'wave-source.png'
$src=[Drawing.Bitmap]::FromFile($wavePath)
$dst=[Drawing.Bitmap]::new(48,48,[Drawing.Imaging.PixelFormat]::Format32bppArgb)
for($y=0;$y -lt 24;$y++){for($x=0;$x -lt 24;$x++){
 $sx=[int][Math]::Floor(($x+0.5)*$src.Width/24)
 $sy=[int][Math]::Floor($src.Height*0.65+($y+0.5)*$src.Height*0.35/24)
 $c=Quantize ($src.GetPixel($sx,[Math]::Min($src.Height-1,$sy)))
 for($dy=0;$dy -lt 2;$dy++){for($dx=0;$dx -lt 2;$dx++){$dst.SetPixel($x*2+$dx,$y*2+$dy,$c)}}
}}
# Equal edge columns remove color/alpha seams between adjacent copies.
for($y=0;$y -lt 48;$y++){for($x=46;$x -lt 48;$x++){$dst.SetPixel($x,$y,$dst.GetPixel($x-46,$y))}}
$dst.Save((Join-Path $candidate 'decor_wave.png'),[Drawing.Imaging.ImageFormat]::Png)
$dst.Dispose();$src.Dispose()
'Exported 39 refined PNGs; project assets not automatically overwritten.'
