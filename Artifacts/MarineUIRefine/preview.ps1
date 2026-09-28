$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
Add-Type -AssemblyName System.IO.Compression.FileSystem
$files=@(Get-ChildItem (Join-Path $PSScriptRoot 'Candidate') -Filter '*.png' | Sort-Object Name)
$cols=5;$cw=272;$ch=172
$sheet=[Drawing.Bitmap]::new($cols*$cw,[int][Math]::Ceiling($files.Count/$cols)*$ch)
$g=[Drawing.Graphics]::FromImage($sheet)
$g.Clear([Drawing.ColorTranslator]::FromHtml('#102e3c'))
$g.InterpolationMode=[Drawing.Drawing2D.InterpolationMode]::NearestNeighbor
$g.PixelOffsetMode=[Drawing.Drawing2D.PixelOffsetMode]::Half
$font=[Drawing.Font]::new('Consolas',10)
$white=[Drawing.SolidBrush]::new([Drawing.Color]::Ivory)
$muted=[Drawing.SolidBrush]::new([Drawing.Color]::LightBlue)
$zip=[IO.Compression.ZipFile]::OpenRead((Join-Path $PSScriptRoot 'BEFORE_39.zip'))
for($i=0;$i -lt $files.Count;$i++){
 $x=($i%$cols)*$cw;$y=[int][Math]::Floor($i/$cols)*$ch
 $file=$files[$i]
 $g.DrawString($file.BaseName,$font,$white,$x+8,$y+5)
 $g.DrawString('BEFORE',$font,$muted,$x+20,$y+148)
 $g.DrawString('CANDIDATE',$font,$muted,$x+145,$y+148)
 $entry=$zip.GetEntry($file.Name);$s=$entry.Open();$old=[Drawing.Bitmap]::FromStream($s)
 $new=[Drawing.Bitmap]::FromFile($file.FullName)
 foreach($side in 0,1){
  $im=if($side -eq 0){$old}else{$new}
  $zoom=if($im.Width -eq 48){2}else{3};$w=$im.Width*$zoom;$h=$im.Height*$zoom
  $g.DrawImage($im,[Drawing.Rectangle]::new($x+18+$side*130,$y+35,$w,$h),0,0,$im.Width,$im.Height,[Drawing.GraphicsUnit]::Pixel)
 }
 $new.Dispose();$old.Dispose();$s.Dispose()
}
$zip.Dispose();$g.Dispose();$font.Dispose();$white.Dispose();$muted.Dispose()
$out=Join-Path $PSScriptRoot 'PNG_BEFORE_AFTER.png'
$sheet.Save($out,[Drawing.Imaging.ImageFormat]::Png);$sheet.Dispose()
$out

