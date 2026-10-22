# Reproduce TEST assets only; no production sound generation.
$ErrorActionPreference = 'Stop'
$taskProject = (Resolve-Path (Join-Path $PSScriptRoot '../../..')).Path
$taskFFmpegCommand = Get-Command ffmpeg.exe -ErrorAction SilentlyContinue
if ($taskFFmpegCommand) { $taskFFmpeg = $taskFFmpegCommand.Source }
else { $taskFFmpeg = (Get-ChildItem -LiteralPath "$env:LOCALAPPDATA/Microsoft/WinGet/Packages" -Filter ffmpeg.exe -Recurse | Where-Object FullName -Like '*Gyan.FFmpeg.Essentials*' | Select-Object -First 1).FullName }
if (-not $taskFFmpeg) { throw 'FFmpeg Essentials is missing' }
$taskSource = Join-Path $taskProject 'Assets/Game/Audio/Source/Environment/sfx_TEST_pipeline_noise_01.wav'
$taskProcessed = Join-Path $taskProject 'Assets/Game/Audio/Processed/Environment/sfx_TEST_pipeline_noise_01.wav'
& $taskFFmpeg -hide_banner -y -f lavfi -i 'anoisesrc=color=white:amplitude=0.1:duration=0.5:sample_rate=48000:seed=1977' -af 'afade=t=in:d=0.005,afade=t=out:st=0.35:d=0.15' -c:a pcm_s24le $taskSource 2> (Join-Path $PSScriptRoot 'procedural-source.log')
if ($LASTEXITCODE -ne 0) { throw 'Source synthesis failed' }
& $taskFFmpeg -hide_banner -y -i $taskSource -af 'atrim=0:0.45,asetpts=PTS-STARTPTS,silenceremove=start_periods=1:start_threshold=-70dB,afade=t=out:st=0.35:d=0.1,loudnorm=I=-24:TP=-3:LRA=7' -ar 48000 -ac 1 -c:a pcm_s16le $taskProcessed 2> (Join-Path $PSScriptRoot 'processing.log')
if ($LASTEXITCODE -ne 0) { throw 'WAV processing failed' }
& $taskFFmpeg -hide_banner -y -i $taskProcessed -ar 48000 -ac 2 -c:a libvorbis -q:a 4 (Join-Path $PSScriptRoot 'TEST_conversion_stereo.ogg') 2> (Join-Path $PSScriptRoot 'ogg-conversion.log')
if ($LASTEXITCODE -ne 0) { throw 'OGG/stereo conversion failed' }
& $taskFFmpeg -hide_banner -i $taskProcessed -af 'volumedetect,ebur128=peak=true' -f null NUL 2> (Join-Path $PSScriptRoot 'loudness.log')
if ($LASTEXITCODE -ne 0) { throw 'Loudness analysis failed' }
& (Join-Path (Split-Path $taskFFmpeg) 'ffprobe.exe') -v error -show_streams -show_format -of json $taskProcessed > (Join-Path $PSScriptRoot 'processed-probe.json')
if ($LASTEXITCODE -ne 0) { throw 'Audio probing failed' }
Write-Output 'TEST source, PCM processing, OGG/stereo conversion and loudness probing passed'
