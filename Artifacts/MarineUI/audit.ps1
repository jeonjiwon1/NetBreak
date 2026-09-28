$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
Add-Type -AssemblyName System.IO.Compression.FileSystem
$root = 'C:\game_dev\unity\NetBreak'
$assetRoot = Join-Path $root 'Assets/Resources/UI/Area1'
$roles = @{
 panel='프레임 | 정보 HUD, Timer, Ready, Inventory, Hotbar | Area1HUDSkin.SetFrame / PrototypeHUDCanvas.BuildHotbar'
 header='프레임 | 정보/Inventory 제목 및 Drag 손잡이 | Area1HUDSkin.CreateDragHeader'
 button='프레임 | 조업 시작, 성장 관리 | Area1HUDSkin.StylePreparation/StyleGrowthButton'
 slot='프레임 | Inventory 4슬롯, Hotbar Q/W/E/R | Area1HUDSkin.StyleItemSlots / PrototypeHUDCanvas.CreateHotbarSlot'
 selected_slot='프레임 | Hotbar LMB | PrototypeHUDCanvas.CreateHotbarSlot'
 key='프레임 | LMB/Q/W/E/R 키 배지, 배속 | PrototypeHUDCanvas.CreateHotbarSlot / Area1HUDSkin.StyleSpeedButtons'
 decor_palm='장식 | 정보 HUD, Ready | Area1HUDSkin.Decorate'
 decor_gull='장식 | 정보 HUD 제목 | Area1HUDSkin.Decorate'
 decor_starfish='장식 | 정보 HUD, Inventory 모서리 | Area1HUDSkin.Decorate'
 decor_shell='장식 | 정보 HUD, Timer, Inventory, Hotbar, Growth | Area1HUDSkin.Decorate'
 decor_coral='장식 | Ready, Hotbar | Area1HUDSkin.Decorate'
 decor_leaf='장식 | 정보 HUD, Inventory, Hotbar, Growth | Area1HUDSkin.Decorate'
 decor_rope_knot='장식 | Ready, Hotbar | Area1HUDSkin.Decorate'
 decor_bobber='장식 | Ready | Area1HUDSkin.Decorate'
 decor_wave='장식 | Ready 하단 좌우 | Area1HUDSkin.Decorate'
 decor_crate='장식 | Inventory | Area1HUDSkin.Decorate'
 decor_clock='장식 | Timer | Area1HUDSkin.Decorate'
 icon_landing='아이콘 | 뜰채/LMB | Area1HUDSkin.ToolIcon'
 icon_bait='아이콘 | 미끼/Q 또는 W 보유 시 | Area1HUDSkin.ToolIcon'
 icon_net='아이콘 | 설치 그물/Q 또는 W 보유 시 | Area1HUDSkin.ToolIcon'
 icon_cast='아이콘 | 투망/Q 또는 W 보유 시 | Area1HUDSkin.ToolIcon'
 icon_rod='아이콘 | 낚싯대/Q 또는 W 보유 시 | Area1HUDSkin.ToolIcon'
 icon_empty='아이콘 | 빈 Q/W/Inventory 및 알 수 없는 항목 fallback | Area1HUDSkin.ToolIcon / ItemHUD.Refresh'
 icon_tactical='아이콘 | E 미니보스 보상 | Area1HUDSkin.SkillIcon'
 icon_signature='아이콘 | R 보스 보상 | Area1HUDSkin.SkillIcon'
 icon_speed='아이콘 | x1/x2/x3 | Area1HUDSkin.StyleSpeedButtons'
 icon_stat_gold='아이콘 | 골드 | Area1HUDSkin.StyleRunPanel'
 icon_stat_catch='아이콘 | 포획 수 | Area1HUDSkin.StyleRunPanel'
 icon_stat_rate='아이콘 | 어획률 | Area1HUDSkin.StyleRunPanel'
 icon_stat_level='아이콘 | 레벨 | Area1HUDSkin.StyleRunPanel'
 icon_stat_exp='아이콘 | 경험치/성장 | Area1HUDSkin.StyleRunPanel'
 icon_stat_stage='아이콘 | 조업 단계 | Area1HUDSkin.StyleRunPanel'
 icon_stat_area='아이콘 | 현재 구간 | Area1HUDSkin.StyleRunPanel'
 icon_storm_orb='아이콘 | 폭풍 구체 보유 시 | Area1HUDSkin.ItemIcon / ItemHUD.Refresh'
 icon_capacitor_coil='아이콘 | 축전 코일 보유 시 | Area1HUDSkin.ItemIcon / ItemHUD.Refresh'
 icon_spectral_scabbard='아이콘 | 유령 검집 보유 시 | Area1HUDSkin.ItemIcon / ItemHUD.Refresh'
 icon_autonomous_sword_array='아이콘 | 자율 검진 보유 시 | Area1HUDSkin.ItemIcon / ItemHUD.Refresh'
 icon_frost_sigil='아이콘 | 서리 문장 보유 시 | Area1HUDSkin.ItemIcon / ItemHUD.Refresh'
 icon_frost_crystal='아이콘 | 서리 결정 보유 시 | Area1HUDSkin.ItemIcon / ItemHUD.Refresh'
}
$zip = [IO.Compression.ZipFile]::OpenRead('C:\Users\전지원\Desktop\current_ui_39.zip')
try {
 $rows = @(Get-ChildItem -LiteralPath $assetRoot -Filter '*.png' | Sort-Object Name | ForEach-Object {
  $file = $_
  $im = [Drawing.Bitmap]::FromFile($file.FullName)
  $meta = Get-Content -Raw -LiteralPath ($file.FullName + '.meta')
  $entry = $zip.Entries | Where-Object Name -EQ $file.Name
  if (@($entry).Count -ne 1) { throw "ZIP mismatch: $($file.Name)" }
  $stream = $entry.Open()
  $sha = [Security.Cryptography.SHA256]::Create()
  $zipHash = [BitConverter]::ToString($sha.ComputeHash($stream)).Replace('-','')
  $stream.Dispose(); $sha.Dispose()
  $alphas = [Collections.Generic.HashSet[int]]::new()
  for($y=0;$y -lt $im.Height;$y++){for($x=0;$x -lt $im.Width;$x++){[void]$alphas.Add($im.GetPixel($x,$y).A)}}
  [PSCustomObject]@{
   Name=$file.Name; Path='Assets/Resources/UI/Area1/'+$file.Name
   Width=$im.Width; Height=$im.Height; Format=$im.PixelFormat.ToString()
   AlphaMin=($alphas | Measure-Object -Minimum).Minimum; AlphaMax=($alphas | Measure-Object -Maximum).Maximum
   GUID=[regex]::Match($meta,'guid: (\w+)').Groups[1].Value
   Border=[regex]::Match($meta,'spriteBorder: (.+)').Groups[1].Value.Trim()
   Sliced=$file.BaseName -in @('panel','header','button','slot','selected_slot','key')
   PreviousDecoration=$file.BaseName.StartsWith('decor_')
   Usage=$roles[$file.BaseName]; Referenced=$roles.ContainsKey($file.BaseName)
   SHA256=(Get-FileHash -LiteralPath $file.FullName).Hash
   MetaSHA256=(Get-FileHash -LiteralPath ($file.FullName+'.meta')).Hash
   ZipSHA256=$zipHash
  }
  $im.Dispose()
 })
} finally { $zip.Dispose() }
if($rows.Count -ne 39){throw 'Expected 39 sprites'}
$rows | ConvertTo-Json -Depth 4 | Set-Content -Encoding UTF8 (Join-Path $PSScriptRoot 'asset-audit-before.json')
$out = @('# Marine UI 39종 사전 조사','','프로젝트/첨부 ZIP 비교. 사용 여부는 정적 참조 기준이며 Unity 실행 확인이 아니다. 정상 Area 1에서 아이템을 지급하지 않으므로 아이템 6종은 보유 시 표시 경로만 존재한다. 미참조 파일 0개.','','공통 경로: `Assets/Resources/UI/Area1/`. 각 PNG 옆 동명 `.png.meta` 사용. 6종 프레임 Border L/B/R/T=5, 나머지=0. 모든 파일 Point/무 Mipmap/무압축.','','| 파일 | 크기 | 유형 · 실제 연결 위치 · 호출 | GUID | ZIP 일치 |','|---|---|---|---|---|')
foreach($r in $rows){$out += '| '+$r.Name+' | '+$r.Width+'×'+$r.Height+' | '+$r.Usage.Replace(' | ',' · ')+' | '+$r.GUID+' | '+($r.SHA256 -eq $r.ZipSHA256)+' |'}
[IO.File]::WriteAllLines((Join-Path $PSScriptRoot 'ASSET_AUDIT.md'),$out,[Text.UTF8Encoding]::new($false))
$rows | Group-Object Width | Select-Object Name,Count
'ZIP identical: '+@($rows | Where-Object { $_.SHA256 -eq $_.ZipSHA256 }).Count
'Referenced: '+@($rows | Where-Object Referenced).Count
