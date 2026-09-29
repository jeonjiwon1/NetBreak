# Deterministic original anticipation sounds. Run from the repository root.
$sampleRate = 44100
$outputDir = Join-Path (Get-Location) 'Assets/Audio/SFX/Boss'
New-Item -ItemType Directory -Path $outputDir -Force | Out-Null

function Write-Warning($name, $duration, $startHz, $endHz, $peak, $seed, $toneWeight) {
    $count = [int][Math]::Round($duration * $sampleRate)
    $samples = New-Object double[] $count
    $random = [Random]::new($seed)
    $phase = 0.0
    $noiseLow = 0.0
    $noiseLower = 0.0
    $maxAbs = 0.0
    for ($i = 0; $i -lt $count; $i++) {
        $t = $i / [double]$sampleRate
        $u = $t / $duration
        $frequency = $startHz + ($endHz - $startHz) * $u * $u
        $phase += 2.0 * [Math]::PI * $frequency / $sampleRate
        $noise = 2.0 * $random.NextDouble() - 1.0
        $noiseLow += 0.075 * ($noise - $noiseLow)
        $noiseLower += 0.006 * ($noiseLow - $noiseLower)
        $bandNoise = $noiseLow - $noiseLower
        $rise = [Math]::Pow($u, 0.9)
        $fadeIn = [Math]::Min(1.0, $t / 0.045)
        $fadeOut = [Math]::Min(1.0, ($duration - $t) / 0.055)
        $pulse = 0.8 + 0.2 * [Math]::Sin(2.0 * [Math]::PI * (5.0 + 6.0 * $u) * $t)
        $tone = [Math]::Sin($phase) + 0.14 * [Math]::Sin(2.5 * $phase)
        $samples[$i] = $fadeIn * $fadeOut * (0.2 + 0.8 * $rise) * $pulse *
            ($toneWeight * $tone + (1.0 - $toneWeight) * 5.0 * $bandNoise)
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

Write-Warning 'GiantTuna_ChargeWarning.wav' 0.34 180 275 0.37 3167 0.38
Write-Warning 'SharkBoss_ChargeWarning.wav' 0.40 100 170 0.42 4291 0.52
