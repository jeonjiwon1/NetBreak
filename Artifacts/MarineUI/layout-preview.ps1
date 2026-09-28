# Offline composition from the current runtime coordinates. Not a Unity screenshot.
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing
$root='C:\game_dev\unity\NetBreak'
$imgs=@{}
Get-ChildItem (Join-Path $root 'Assets/Resources/UI/Area1') -Filter '*.png' | ForEach-Object {$imgs[$_.BaseName]=[Drawing.Bitmap]::FromFile($_.FullName)}
Get-ChildItem (Join-Path $PSScriptRoot 'Candidate') -Filter '*.png' | ForEach-Object {$imgs[$_.BaseName].Dispose();$imgs[$_.BaseName]=[Drawing.Bitmap]::FromFile($_.FullName)}
$sheet=[Drawing.Bitmap]::new(1920,1080)
$g=[Drawing.Graphics]::FromImage($sheet)
$bg=[Drawing.Bitmap]::FromFile((Join-Path $root 'Assets/Resources/Area1/CoastBackground.png'))
$g.DrawImage($bg,0,0,1920,1080);$bg.Dispose()
$g.InterpolationMode=[Drawing.Drawing2D.InterpolationMode]::NearestNeighbor
$g.PixelOffsetMode=[Drawing.Drawing2D.PixelOffsetMode]::Half
$fonts=[Drawing.Text.PrivateFontCollection]::new()
$fonts.AddFontFile((Join-Path $root 'Assets/UI/Fonts/NanumGothic-Bold.ttf'))
function Sprite($name,[single]$x,[single]$y,[single]$w,[single]$h){
 $im=$imgs[$name];if(!$im){return}
 $side=[Math]::Min($w,$h)
 $g.DrawImage($im,[Drawing.RectangleF]::new($x+($w-$side)/2,$y+($h-$side)/2,$side,$side),[Drawing.RectangleF]::new(0,0,$im.Width,$im.Height),[Drawing.GraphicsUnit]::Pixel)
}
function Frame($name,[single]$x,[single]$y,[single]$w,[single]$h){
 $im=$imgs[$name];if(!$im){return}
 # Unity Image pixelsPerUnit = Sprite 32 / Canvas reference 100.
 $bx=[Math]::Min(15.625,$w/2);$by=[Math]::Min(15.625,$h/2)
 $sx=@(0,5,27,32);$sy=@(0,5,27,32)
 $dx=@($x,($x+$bx),($x+$w-$bx),($x+$w));$dy=@($y,($y+$by),($y+$h-$by),($y+$h))
 for($iy=0;$iy -lt 3;$iy++){for($ix=0;$ix -lt 3;$ix++){
  $dw=$dx[$ix+1]-$dx[$ix];$dh=$dy[$iy+1]-$dy[$iy]
  if($dw -le 0 -or $dh -le 0){continue}
  $g.DrawImage($im,[Drawing.RectangleF]::new($dx[$ix],$dy[$iy],$dw,$dh),[Drawing.RectangleF]::new($sx[$ix],$sy[$iy],($sx[$ix+1]-$sx[$ix]),($sy[$iy+1]-$sy[$iy])),[Drawing.GraphicsUnit]::Pixel)
 }}
}
function Label($text,[single]$x,[single]$y,[single]$w,[single]$h,[single]$size=16,$color='#f7efcf',$align='Center'){
 $font=[Drawing.Font]::new($fonts.Families[0],$size,[Drawing.FontStyle]::Bold,[Drawing.GraphicsUnit]::Pixel)
 $brush=[Drawing.SolidBrush]::new([Drawing.ColorTranslator]::FromHtml($color))
 $fmt=[Drawing.StringFormat]::new();$fmt.Alignment=[Drawing.StringAlignment]::$align;$fmt.LineAlignment=[Drawing.StringAlignment]::Center
 $g.DrawString($text,$font,$brush,[Drawing.RectangleF]::new($x,$y,$w,$h),$fmt)
 $font.Dispose();$brush.Dispose();$fmt.Dispose()
}
# Top-left information panel.
Frame panel 20 20 256 254
Frame header 30 27 236 26
Label '≡  NETBREAK' 75 27 180 26 15 '#84efe5' Near
Sprite decor_palm 22 22 48 48
Sprite decor_gull 233 23 38 38
Sprite decor_leaf 6 233 43 43
Sprite decor_starfish 244 239 40 40
Sprite decor_shell 36 255 27 27
$labels=@('골드','포획 수','어획률','레벨','경험치','조업 단계','현재 구간')
$values=@('0','0','0.0%','1','0 / 30','준비','조업 준비')
$icons=@('gold','catch','rate','level','exp','stage','area')
for($i=0;$i -lt 7;$i++){
 $y=59+$(if($i -lt 5){$i*27}else{149+($i-5)*27})
 $brush=[Drawing.SolidBrush]::new([Drawing.Color]::FromArgb(190,3,27,43));$g.FillRectangle($brush,32,$y,232,25);$brush.Dispose()
 Sprite ('icon_stat_'+$icons[$i]) 37 ($y+2) 20 20
 Label $labels[$i] 63 $y 81 25 15 '#f7efcf' Near
 Label $values[$i] 146 $y 113 25 16 $(if($i -eq 0){'#ffd777'}elseif($i -ge 5){'#84efe5'}else{'#f7efcf'}) Far
}
# Timer and preparation.
Frame panel 810 16 300 46
Sprite decor_clock 822 24 29 29
Sprite decor_shell 1085 13 24 24
Label '플레이 시간: 00:00' 842 19 260 38 21
Frame panel 804 70 312 112
Sprite decor_palm 789 57 56 56
Sprite decor_bobber 821 85 30 40
Sprite decor_rope_knot 1084 65 35 35
Sprite decor_coral 1071 140 46 46
Sprite decor_wave 794 164 67 32
Sprite decor_wave 1051 164 67 32
Label '조업 준비' 817 80 286 40 25
Frame button 836 129 248 42
Label '조업 시작' 848 134 224 32 21 '#39251b'
# Inventory.
Frame panel 1512 20 388 132
Frame header 1522 27 368 26
Label '≡  아이템' 1556 27 320 26 15 '#84efe5' Near
Sprite decor_leaf 1503 23 43 43
Sprite decor_crate 1852 23 44 44
Sprite decor_shell 1504 129 30 30
Sprite decor_starfish 1885 134 29 29
for($i=0;$i -lt 4;$i++){
 $x=1527+$i*91;Frame slot $x 60 84 82
 Sprite icon_empty ($x+23) 77 38 38
 Label ($i+1).ToString() ($x+5) 63 18 18 14 '#84efe5'
 Label '비어 있음' ($x+5) 114 74 21 15
}
# Bottom hotbar, unchanged 716 x 120 at bottom +14.
Frame panel 602 946 716 120
$bindings=@('LMB','Q','W','E','R');$hotIcons=@('icon_landing','icon_empty','icon_empty','icon_tactical','icon_signature')
$hotText=@("뜰채`n고정 도구",'비어 있음','비어 있음',"잠김`n미니보스 보상","잠김`n보스 보상")
for($i=0;$i -lt 5;$i++){
 $x=612+$i*140
 Frame $(if($i -eq 0){'selected_slot'}else{'slot'}) $x 953 136 106
 Sprite $hotIcons[$i] ($x+49) 965 38 38
 $kw=if($i -eq 0){47}else{29}
 Frame key ($x+6) 958 $kw 23
 Label $bindings[$i] ($x+6) 958 $kw 23 15
 Label $hotText[$i] ($x+6) 1003 124 52 16
}
Sprite decor_leaf 581 982 53 53
Sprite decor_coral 1288 1013 49 49
Sprite decor_shell 602 1043 26 26
Sprite decor_rope_knot 1297 940 28 28
# Speed and growth.
for($i=0;$i -lt 3;$i++){
 $y=816+$i*45
 Frame key 1781 $y 116 40
 Sprite icon_speed 1792 ($y+7.5) 25 25
 Label ('x'+($i+1)) 1824 ($y+5) 65 30 20
}
Frame button 1675 1002 225 58
Sprite decor_leaf 1659 990 45 45
Sprite decor_shell 1878 1020 32 32
Label '성장 관리 [Tab]' 1685 1010 205 42 20 '#39251b'
# Explicitly mark the artifact as an offline art layout approximation.
Label '정적 합성 미리보기 · Unity 화면 아님 · TMP/상호작용 미검증' 470 300 980 35 18 '#ffffff'
$g.Dispose();$fonts.Dispose()
$sheet.Save((Join-Path $PSScriptRoot 'HUD_STATIC_PREVIEW.png'),[Drawing.Imaging.ImageFormat]::Png)
$sheet.Dispose();foreach($im in $imgs.Values){$im.Dispose()}
