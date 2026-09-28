# Export ImageGen frame materials without flattening their interior or rails.
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing
$project=Split-Path $PSScriptRoot
$out=Join-Path $project 'Artifacts/MarineUIReference'
$dest=Join-Path $out 'Candidate'
New-Item -ItemType Directory -Force $dest | Out-Null
$records=Get-Content -Raw (Join-Path $out 'generation-manifest.json') | ConvertFrom-Json
foreach($r in $records){
 $src=[Drawing.Bitmap]::FromFile((Join-Path $out ('Sources/'+$r.name+'.png')))
 $size=if($r.name -eq 'decor_wave'){256}elseif($r.name -eq 'decor_rope_knot'){48}else{64}
 $height=if($r.name -eq 'decor_wave'){48}else{$size}
 $dst=[Drawing.Bitmap]::new($size,$height,[Drawing.Imaging.PixelFormat]::Format32bppArgb)
 for($y=0;$y -lt $height;$y++){for($x=0;$x -lt $size;$x++){
  $sy=if($r.name -eq 'decor_wave'){$src.Height*(0.47+($y+0.5)*0.39/$height)}else{($y+0.5)*$src.Height/$height}
  $c=$src.GetPixel([int][Math]::Floor(($x+0.5)*$src.Width/$size),[int][Math]::Floor($sy))
  $a=if($c.A -ge 128){255}else{0}
  # Quantized tonal steps keep pixel clusters distinct at runtime.
  $red=[Math]::Min(255,[int][Math]::Round($c.R/8.0)*8)
  $green=[Math]::Min(255,[int][Math]::Round($c.G/8.0)*8)
  $blue=[Math]::Min(255,[int][Math]::Round($c.B/8.0)*8)
  if($size -eq 64 -and $x -ge 12 -and $x -lt 52 -and $y -ge 12 -and $y -lt 52){
   # Some generated navy centers contain unwanted transparent patches.
   # Composite those over navy; keep the authored opaque mottling intact.
   # Remove generated smooth vignettes while retaining the brighter authored
   # pixel clusters; broad gradient bands otherwise repeat as medallions.
   $best=if($r.name -eq 'button'){
    if($a -gt 0 -and $red -ge 240 -and $green -ge 216){@(255,225,155)}else{@(245,205,126)}
   }else{
    if($a -gt 0 -and $green -ge 64 -and $blue -ge 80){@(11,58,73)}else{@(3,43,59)}
   }
   $red=$best[0];$green=$best[1];$blue=$best[2];$a=255
  }
  $dst.SetPixel($x,$y,[Drawing.Color]::FromArgb($a,$red,$green,$blue))
 }}
 if($size -eq 64){
  # Mirror the authored tile interior and long rails, not the fixed corner
  # caps. Tile endpoints now match without replacing texture by a flat fill.
  $copy=[Drawing.Bitmap]$dst.Clone()
  for($y=0;$y -lt 64;$y++){for($x=0;$x -lt 64;$x++){
   $sx=if($x -ge 32 -and $x -lt 52){63-$x}else{$x}
   $sy=if($y -ge 32 -and $y -lt 52){63-$y}else{$y}
   $dst.SetPixel($x,$y,$copy.GetPixel($sx,$sy))
  }}
  $copy.Dispose()
 }
 $dst.Save((Join-Path $dest ($r.name+'.png')),[Drawing.Imaging.ImageFormat]::Png)
 $dst.Dispose();$src.Dispose()
}
'Exported '+$records.Count+' reference materials to Candidate; no project overwrite.'
