$ErrorActionPreference='Stop'
$root='C:\game_dev\unity\NetBreak'
$assets=Join-Path $root 'Assets/Resources/UI/Area1'
$audit=Get-Content -Raw -Encoding UTF8 (Join-Path $PSScriptRoot 'asset-audit-before.json') | ConvertFrom-Json
node (Join-Path $PSScriptRoot 'validate.mjs') --candidate
if($LASTEXITCODE -ne 0){throw 'Candidate validation failed; no assets copied'}
foreach($r in $audit){
 $src=Join-Path (Join-Path $PSScriptRoot 'Candidate') $r.Name
 Copy-Item -LiteralPath $src -Destination (Join-Path $assets $r.Name)
}
node (Join-Path $PSScriptRoot 'validate.mjs')
if($LASTEXITCODE -ne 0){throw 'Project validation failed'}
$zipPath=Join-Path $PSScriptRoot 'NETBREAK_UI_MARINE_FINAL_CANDIDATE.zip'
if(Test-Path -LiteralPath $zipPath){throw 'Output ZIP already exists; inspect before replacing'}
Add-Type -AssemblyName System.IO.Compression.FileSystem
$zip=[IO.Compression.ZipFile]::Open($zipPath,[IO.Compression.ZipArchiveMode]::Create)
try {foreach($r in $audit){[void][IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zip,(Join-Path $assets $r.Name),$r.Name)}}finally{$zip.Dispose()}
$zip=[IO.Compression.ZipFile]::OpenRead($zipPath)
try{
 if($zip.Entries.Count -ne 39){throw 'ZIP count mismatch'}
 foreach($e in $zip.Entries){
  $s=$e.Open();$sha=[Security.Cryptography.SHA256]::Create()
  $actual=[BitConverter]::ToString($sha.ComputeHash($s)).Replace('-','')
  $s.Dispose();$sha.Dispose()
  $expected=(Get-FileHash -LiteralPath (Join-Path $assets $e.Name)).Hash
  if($actual -ne $expected){throw ('ZIP content mismatch: '+$e.Name)}
 }
}finally{$zip.Dispose()}
'ZIP verified: 39 PNG, byte-identical to project; '+$zipPath
