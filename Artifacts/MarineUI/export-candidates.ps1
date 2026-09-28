$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$records = Get-Content -Raw -Encoding UTF8 (Join-Path $PSScriptRoot 'generation-manifest.json') | ConvertFrom-Json
$folder = Join-Path $PSScriptRoot 'Candidate'
New-Item -ItemType Directory -Force -Path $folder | Out-Null
foreach($r in $records){
 $size = if($r.name.StartsWith('decor_')){48}else{32}
 $src = [Drawing.Bitmap]::FromFile($r.path)
 $dst = [Drawing.Bitmap]::new($size,$size,[Drawing.Imaging.PixelFormat]::Format32bppArgb)
 # Export at the existing Unity canvas dimensions using nearest-neighbor sampling.
 # Keep the generated composition and RGB values; binary alpha prevents a soft fringe.
 for($y=0;$y -lt $size;$y++){for($x=0;$x -lt $size;$x++){
  $sx=[Math]::Min($src.Width-1,[int][Math]::Floor(($x+0.5)*$src.Width/$size))
  $sy=[Math]::Min($src.Height-1,[int][Math]::Floor(($y+0.5)*$src.Height/$size))
  $c=$src.GetPixel($sx,$sy)
  $a=if($c.A -ge 128){255}else{0}
  $dst.SetPixel($x,$y,[Drawing.Color]::FromArgb($a,$c.R,$c.G,$c.B))
 }}
 if($r.name -in @('panel','header','button','slot','selected_slot','key')){
  # Nine-slice packing: retain generated fixed corners and edge cross-sections.
  # Illustration is never stretched through the center; strip away generated
  # accidental center transparency and texture before Unity's 5px slicing.
  $copy=[Drawing.Bitmap]$dst.Clone()
  $fill=if($r.name -eq 'button'){'#f5cd7e'}elseif($r.name -eq 'panel' -or $r.name -eq 'header'){'#063848'}else{'#032534'}
  $color=[Drawing.ColorTranslator]::FromHtml($fill)
  for($y=0;$y -lt 32;$y++){for($x=0;$x -lt 32;$x++){
   $middleX=$x -ge 5 -and $x -lt 27;$middleY=$y -ge 5 -and $y -lt 27
   if($middleX -and $middleY){$dst.SetPixel($x,$y,$color)}
   elseif($middleX){$dst.SetPixel($x,$y,$copy.GetPixel(16,$y))}
   elseif($middleY){$dst.SetPixel($x,$y,$copy.GetPixel($x,16))}
  }}
  if($r.name -eq 'key' -or $r.name -eq 'header'){
   # At the existing 23/26px runtime height, Unity compresses the 5px slice
   # corners until they meet. Pack the generated material into a thinner rail
   # and reserve the remaining slice pixels as dark inset for the TMP label.
   $rail=if($r.name -eq 'key'){1}else{2}
   for($y=0;$y -lt 32;$y++){for($x=0;$x -lt 32;$x++){
    $sx=if($x -lt $rail){[int][Math]::Floor(($x+0.5)*5/$rail)}elseif($x -ge 32-$rail){27+[int][Math]::Floor(($x-(32-$rail)+0.5)*5/$rail)}else{16}
    $sy=if($y -lt $rail){[int][Math]::Floor(($y+0.5)*5/$rail)}elseif($y -ge 32-$rail){27+[int][Math]::Floor(($y-(32-$rail)+0.5)*5/$rail)}else{16}
    if($sx -eq 16 -and $sy -eq 16){$dst.SetPixel($x,$y,$color)}
    else{$dst.SetPixel($x,$y,$copy.GetPixel($sx,$sy))}
   }}
   foreach($cx in @(0,31)){foreach($cy in @(0,31)){$dst.SetPixel($cx,$cy,[Drawing.Color]::Transparent)}}
  }
  $copy.Dispose()
 }
 $dst.Save((Join-Path $folder ($r.name+'.png')),[Drawing.Imaging.ImageFormat]::Png)
 $dst.Dispose();$src.Dispose()
}
'Exported: '+$records.Count
