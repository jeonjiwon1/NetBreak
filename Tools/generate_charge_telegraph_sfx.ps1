# Deterministic, original prototype SFX. Run from the repository root.
$sampleRate = 44100
$outputDir = Join-Path (Get-Location) 'Assets/Audio/SFX/Boss'
New-Item -ItemType Directory -Path $outputDir -Force | Out-Null

function Write-ChargeSound($name, $duration, $startHz, $endHz, $peak, $seed, $weight) {
    $count = [int][Math]::Round($duration * $sampleRate)
    $samples = New-Object double[] $count
    $random = [Random]::new($seed)
    $phase = 0.0
    $lowNoise = 0.0
    $lastNoise = 0.0
    $maxAbs = 0.0
    for ($i = 0; $i -lt $count; $i++) {
        $t = $i / [double]$sampleRate
        $u = $t / $duration
        $frequency = $startHz * [Math]::Pow($endHz / $startHz, $u)
        $phase += 2.0 * [Math]::PI * $frequency / $sampleRate
        $noise = 2.0 * $random.NextDouble() - 1.0
        $lowNoise += 0.035 * ($noise - $lowNoise)
        $rush = $lowNoise - $lastNoise
        $lastNoise += 0.008 * ($lowNoise - $lastNoise)
        $attack = [Math]::Min(1.0, $t / 0.012)
        $decay = [Math]::Pow(1.0 - $u, 1.7)
        $accent = [Math]::Exp(-[Math]::Pow(($t - 0.045) / 0.028, 2.0))
        $tone = [Math]::Sin($phase) + 0.18 * [Math]::Sin(2.0 * $phase)
        $samples[$i] = $attack * $decay * (($weight * $tone) +
            (1.0 - $weight) * 7.0 * $rush + 0.22 * $accent * [Math]::Sin($phase * 0.5))
        $maxAbs = [Math]::Max($maxAbs, [Math]::Abs($samples[$i]))
    }

    $path = Join-Path $outputDir $name
    $stream = [IO.File]::Create($path)
    try {
        $writer = [IO.BinaryWriter]::new($stream)
        $dataLength = $count * 2
        $writer.Write([Text.Encoding]::ASCII.GetBytes('RIFF'))
        $writer.Write([int](36 + $dataLength))
        $writer.Write([Text.Encoding]::ASCII.GetBytes('WAVEfmt '))
        $writer.Write([int]16)
        $writer.Write([int16]1)
        $writer.Write([int16]1)
        $writer.Write([int]$sampleRate)
        $writer.Write([int]($sampleRate * 2))
        $writer.Write([int16]2)
        $writer.Write([int16]16)
        $writer.Write([Text.Encoding]::ASCII.GetBytes('data'))
        $writer.Write([int]$dataLength)
        foreach ($sample in $samples) {
            $writer.Write([int16][Math]::Round($sample * $peak / $maxAbs * 32767.0))
        }
        $writer.Flush()
    } finally {
        $stream.Dispose()
    }
}

Write-ChargeSound 'GiantTuna_ChargeTelegraph.wav' 0.26 160 95 0.44 1047 0.46
Write-ChargeSound 'SharkBoss_ChargeTelegraph.wav' 0.36 105 58 0.50 2063 0.64
