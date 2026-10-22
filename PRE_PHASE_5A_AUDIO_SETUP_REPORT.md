# Pre-Phase 5A difficulty hotfix and audio setup

Date: 2026-10-02. Primary scene: `Assets/Game/Scenes/CoastalDefense_TopDown.unity`, Unity 6000.3.24f1.

## 1. Status

PARTIAL. Difficulty tuning, automated/live scripted checks, trusted skill installation, processing verification, folders and rules are prepared. Human manual difficulty review, credentialed generative-audio smoke testing and audible Unity playback remain incomplete. Phase 5A has not started. No gameplay audio manager, mixer, event bindings or full sound library were added. Existing workspace changes were preserved; nothing committed or staged.

## 2. Difficulty hotfix

The existing centralized `EndlessSurvival.asset` is the only active tuning asset changed. No individual wave definitions or base enemy HP assets were edited.

| Setting | Previous | Final |
|---|---:|---:|
| EnemyQuantityMultiplier | 10 | 5 |
| GlobalEnemyHealthMultiplier | 3.0 | 1.0 |
| BossHealthMultiplier | 3.0 | 1.0 |
| Maximum standard enemies per wave | 2,400 | 1,200 |
| Early pressure target | 50 | 25 |
| Pressure growth per wave | 8 | 4 |
| Maximum pressure target | 320 | 160 |

The quantity cap scales with the multiplier so the original maximum of 240 reference formations is preserved. Bosses remain single; escorts expand fivefold. Seed 1977 gives counts 80/120/110/145/190/260/435 at Waves 1/2/3/4/5/10/20. Wave 25 is 691 and Wave 50 is 1,441 including the additive boss/escort encounter. Counts are not all spawned at once. Pressure is `min(160, 25 + 4*(wave-1))`: 25/41/61/101 at 1/5/10/20. Existing 20% hysteresis and committed-cargo accounting remain; low pressure still accelerates dispatch.

Normal base HP remains Basic 100, Runner 60, Brute 600, Breacher 180, Heavy 900, Pirate Runner 70, Pirate Raider 220. Boss base HP remains Osama 8,000 and Warlord 6,000. Existing pre-4D wave growth after Wave 20 remains: Osama at Wave 25 is 8,500, Warlord at Wave 50 is 8,250. Formula remains `ceil(baseHP * waveHealth * (boss ? 1 : 1))`; the boss factor replaces, rather than compounds with, the global factor. Spawner/initializer call sites were inspected; no separate active 3x factor remains.

Preserved: boat speed 9, weighted capacity 12, unloading .07 s, departure delay .15 s, first dispatch .25 s, accelerated cadence .15 s, normal cadence 1 s down to .3 s, .4-unit disembark separation, five landing sectors and existing berth/approach queues. Configured boat caps 6/8/10/15 at Waves 1/5/10/20, later 28, with three extra surge slots bounded by 32. Seven-second surges every 24 seconds on Wave 5+ remain enabled. Hard active-enemy safety cap remains 400; the director now maintains a much lower working band.

Receipts: `Docs/Validation/PrePhase5A/Setup.cs`, `tuning.json`, before/after asset snapshots and `difficulty-hotfix.diff`. Historical Phase 4D authoring receipts are retained as historical records, not executed; running their old Apply method would intentionally restore the previous settings.

## 3. Manual difficulty result and validation

No human mouse-only playthrough was performed. Script-driven live Play Mode used real Update combat, actual boat transport, the Start Wave UI submit event, BuildManager purchases and the existing 1,000 Homicides loss condition. Simulation ran at 3x time; screenshots were visually inspected. These receipts do not establish the intended subjective difficulty.

- Conventional 500 RSD opening (Sniper at 23,9; MG at 23,4; Wire at 27,4, then earned-money Flame purchases) failed Wave 1: 26 kills, 1,000 Homicides, peak 40 enemies/3 boats. Wave 1 is still demanding for this opening.
- Adaptive mine placement using enemy-position prediction, actual 75 RSD purchases and no injected money cleared Waves 1–5: 80/120/110/145/190 kills, zero Homicides, peaks 25/30/29/26/33 enemies and 5/5/7/7/6 boats. It ended with 7,450 RSD. This automated reactive strategy is not evidence that ordinary human reactions can reproduce it.
- Prepared Wave 10 used 18,000 injected RSD and the unchanged Phase 4D three-lane mixed layout (MG/Sniper/Flame/Mortar/Cannon/Rocket plus Wire). It cleared all 260 enemies, zero Homicides, peak 74 enemies/8 boats.
- Prepared Wave 20 used the same injection/layout and cleared with 434 kills, one Brute breach (100 Homicides), peak 115 enemies/12 boats. It does not prove Wave 20 is hard or validate earned-money progression to Wave 20. No additional difficulty/economy changes were made beyond the requested hotfix.

Live combat is substantially more killable than the previous 3x HP fixtures, but layout sensitivity and human difficulty remain unresolved. The adaptive run's waves took roughly 9–13 simulated seconds; prepared 10/20 took about 31/38 seconds. These short outcomes do not establish satisfying human durations.

Unity compile completed. EditMode: 97/97 passed. Initial five failures were historical 10x population-range expectations; those ranges were halved for the authorized 5x tuning, and the seeded-count test now requires fivefold formations with a single boss. Relevant PlayMode: SurvivalPlayTests 6/6 (including the real-boat 100-wave simulation), Phase4CPlayTests 5/5 (boats, boss HP/armor/HUD/reward/breach, dismantle), AssaultPoolTests 1/1. A first combined `|` test filter matched zero tests; it was replaced with individually verified fixture runs. No zero-test result is counted as validation.

Console history was archived; the audio-output validation exception is disclosed below. After clearing captured test history, final Console has zero errors/warnings and no compilation failure. Scene/runtime audit and receipts are in `Docs/Validation/PrePhase5A/`. The final Editor is stopped, the primary scene is clean, and it has zero missing scripts and zero test audio references. Fixture placements and transient audio objects were not saved. TimeManager bytes are unchanged. Focused review confirmed only seven tuning fields and the two obsolete population assertions changed in the existing gameplay/test files. Global `git diff --check` flags a pre-existing blank EOF line in PrototypePlayTests.cs; that unrelated file was not edited by this task.

## 4. Installed audio skills found

Initial installed skill search across `.codex/skills`, `.agents/skills`, plugin cache and project skills found no dedicated SFX/music generator. Keyword matches in UI/UX and Drive guidance were incidental. Adobe Quick Cut works on video highlights; exposed Adobe speech-enhancement separates/cleans speech, music/background and reverb, and summarize analyzes spoken content. These are not SFX generators. Unity CLI/Pipeline supports Editor control, importer inspection and playback validation, not sound synthesis.

## 5. Audio skill / plugin search results

The supported marketplace search returned Runway (advertises sound effects, music and voiceover), Descript (video/speech editing), Pocket AI (recording retrieval), OpenArt and other primarily visual tools. Runway was the relevant SFX candidate and was suggested for installation/connection; no confirmed plugin connection exists. Marketplace results are not exhaustive.

The official `openai/skills` curated catalog contains `speech` and `transcribe`; speech is TTS/radio, transcribe is analysis. No dedicated SFX/music skill was in that catalog. The supported `.experimental` lookup returned path-not-found; no capability was inferred from it.

Further primary-source search found the provider-owned [Runway Studio Skills repository](https://github.com/runwayml/runway-studio-skills), unarchived and inspected at main commit `c1c0348fda3acf4ef864393b6485afb4600eedda` (repository reports last push 2026-04-20). Its SKILL.md, audio CLI and shared helper were inspected before installation. [Runway's official app documentation](https://help.runwayml.com/hc/en-us/articles/45570040112531-Creating-with-Apps) confirms text-to-SFX capability.

## 6. Installed tools

- `C:/Users/Slavisa/.codex/skills/runway-studio-skills`: provider-owned MIT-licensed skill installed using the official skill-installer. Audio operations are sound-effect, TTS, speech conversion, dubbing and isolation. It does not implement music generation. Skills become available in Codex on the next turn.
- `C:/Users/Slavisa/.codex/skills/speech`: official OpenAI TTS skill, installed for future radio/announcer content; not an SFX/music solution.
- Isolated tooling runtime at `C:/Users/Slavisa/.codex/tools/audio-runtime/`, with runwayml, requests, openai and uv. Both CLI help commands work; Runway's documented `uv run` path works. No Python packages were added to Unity's project.
- FFmpeg/ffprobe/ffplay 9.0.1 Essentials installed through WinGet's exact `Gyan.FFmpeg.Essentials` package; installer hash verified. Bin directory: `C:/Users/Slavisa/AppData/Local/Microsoft/WinGet/Packages/Gyan.FFmpeg.Essentials_Microsoft.Winget.Source_8wekyb3d8bbwe/ffmpeg-9.0.1-essentials_build/bin/`. New shells receive the PATH update; the reproducible smoke script also locates the installed tool.

## 7. Test audio generation result

Runway's installed CLI was invoked for one 0.5-second generic mechanical click. It exited before any request with `Error: No API key. Set RUNWAYML_API_SECRET or pass --api-key.` No task, billed request or service-generated file was created. OpenAI speech also lacks `OPENAI_API_KEY`; no speech generation was claimed. Credentials were checked only for presence, not exposed.

A separate, explicitly procedural FFmpeg white-noise test was generated: 0.5 s, seed 1977, 48 kHz, 24-bit mono PCM. This tests the local pipeline and is not a substitute for successful generative SFX verification. Processed TEST output is 0.449917 s, 48 kHz, 16-bit mono PCM. Both files remain clearly named `sfx_TEST_pipeline_noise_01.wav` and have no saved gameplay references. No gun sound or production library was created.

## 8. Audio processing tool status

Verified generation of a deterministic diagnostic signal, trim, silence removal, fades, loudnorm, WAV PCM conversion/resampling, mono/stereo conversion, OGG Vorbis encoding, ffprobe format validation, sample/true peak and EBU R128 measurement. The processed test measured mean -27.7 dBFS, sample peak -20.1 dBFS, measured true peak -17.2 dBFS and integrated -24.1 LUFS. These values describe only the test, not a production loudness target. Repeatable script: `Docs/Validation/PrePhase5A/AudioProcessingSmoke.ps1`. Checks return success; logs, format probes and hashes are retained. Loop editing is available through trimming/fades/crossfades, but no seamless production loop was generated or approved.

## 9. Unity audio current state and import result

No existing AudioMixer, AudioManager/AudioService, AudioSource component or production AudioClip was found. The primary saved scene has zero AudioListeners; the legacy prototype scene also has no serialized AudioListener/AudioSource. Built-in `ProjectSettings/AudioManager.asset` is Unity engine configuration, not a custom manager. Audio is enabled, volume 1, 32 real voices/512 virtual voices; project sample rate is device-selected (0), and the test reports 48 kHz output.

The processed test imports as a valid PCM AudioClip, DecompressOnLoad, preserve sample rate, force mono. It loads successfully and `GetData` returns 21,596 finite nonzero samples, RMS ~0.040975. A temporary AudioSource and AudioListener were created only in Play Mode and removed; `isPlaying` was true. However `GetOutputData` returned zero output on both attempts, including a repeated looping/frame-delayed check. Editor master mute was false, AudioListener.pause false and volume 1. Thus file/AudioClip import is verified but actual mixed-output and audible playback are NOT verified. The check deliberately reports failure rather than masking this limitation. No persistent listener or final audio architecture was implemented to work around it.

## 10. Audio folder structure

Created through AssetDatabase with Unity metadata:

```
Assets/Game/Audio/
  Source/{Weapons,Explosions,Impacts,Enemies,Bosses,Boats,Environment,UI,Music,Radio}/
  Processed/{Weapons,Explosions,Impacts,Enemies,Bosses,Boats,Environment,UI,Music,Radio}/
  Mixers/
  Definitions/
```

No existing asset was moved. Empty folders are preserved by their Unity `.meta` files. Only the two TEST WAVs were added under this structure.

## 11. Audio format recommendation and capabilities

Source preference: native lossless WAV 48 kHz/24-bit; short runtime candidates: mono PCM WAV 48 kHz/16 or 24-bit; longer candidates: stereo WAV master and optionally OGG Vorbis runtime export. FFmpeg has verified these local formats and channel conversions. Do not present decoded MP3 as a lossless original.

Runway's installed SFX CLI selects `eleven_text_to_sound_v2`, accepts a 0.5–30 second duration and a loop flag, and downloads provider output. It exposes no source-format/sample-rate/bit-depth/channel parameter. The helper infers extensions from output URLs, using MP3 as an audio fallback. Actual native encoding/rate/depth/channels remain unverified until credentialed output can be probed; seamless looping remains an unverified request. No installed music generator was proven.

The official speech skill documents WAV/PCM/FLAC/MP3/Opus/AAC, with raw PCM specified as 24 kHz 16-bit little-endian. Its input limit is 4,096 characters rather than a fixed duration. Channel layout/native WAV details require an actual output probe. It does not generate seamless ambience/music or weapon effects.

## 12. Licensing workflow

`AUDIO_LICENSES.md` contains original/derivative TEST provenance and tool/provider records. [Runway commercial-use help](https://help.runwayml.com/hc/en-us/articles/21668707517587-Can-I-use-the-content-I-made-in-Runway-for-commercial-purposes) allows commercial use; actual provider/model/plan terms, generation receipt and reference rights must still be recorded for each produced clip. MIT applies to the skill code, not automatically to provider audio outputs. No downloaded third-party sound or external-service output was added, and no unlicensed source material was used. See production rules for required per-asset fields and AI-voice disclosure.

## 13. Production rules status

`AUDIO_PRODUCTION_RULES.md` exists: formats, folders, names/revisions, loudness, variations, loops, licensing, provenance, performance and future voice/concurrency requirements. Proposed future budget is at most 24 SFX including four reserved UI/critical slots, two ambience, two music-crossfade and one radio voice. Pooling, priorities, cooldowns, aggregation and bounded variations are prerequisites, not implemented features.

## 14. Blockers / limitations

- Human manual validation of Waves 1–5/10/20 and earned progression to later waves remains outstanding. The conventional Wave 1 loss and generous prepared later-wave clears leave subjective balance unresolved.
- `RUNWAYML_API_SECRET` is absent; a real service-generation smoke test and actual output-format/loop checks remain blocked. Runway plugin installation/connection is also unconfirmed; the installed local provider skill is a supported alternative. Set secrets locally, never paste them into chat. Service credits/plan availability have not been checked.
- Optional speech generation lacks `OPENAI_API_KEY`.
- Unity AudioClip import succeeds, but nonzero output and audible playback remain unverified. Saved gameplay currently has no listener or audio system; those belong to Phase 5A.
- Music generation through a connected service is not proven, and no production asset is approved. No standalone-build audio/performance validation was run.

## 15. Exact recommended Phase 5A workflow

1. Finish human difficulty checks with the saved 5/1/1 tuning. Keep receipts for the conventional opening, Wave 5, an earned Wave 10 layout and hard Wave 20; do not infer fairness from script-reactive mines or injected budgets.
2. Configure `RUNWAYML_API_SECRET` locally and available service credits (or install/connect the Runway plugin). Use the installed provider skill for one 0.5-second TEST click. From its skill root, the verified CLI shape is:

   ```powershell
   & 'C:/Users/Slavisa/.codex/tools/audio-runtime/Scripts/uv.exe' run --python 'C:/Users/Slavisa/.codex/tools/audio-runtime/Scripts/python.exe' scripts/generate_audio.py --output-dir 'C:/Users/Slavisa/Documents/ChatGPT/border-defenders/Assets/Game/Audio/Source/Weapons' sound-effect --prompt 'A single short generic mechanical click, dry, no voices, no music.' --duration 0.5 --filename TEST_generation
   ```

   Verify task/output and probe the actual download; preserve its original container. No success is currently claimed for this request.
3. Record provider/model/plan license terms, prompt, task ID and source/derivative hashes. Produce approved WAV working assets through FFmpeg; retain the original source if lossy. Trim/fade, measure, and listen. For loops request looping and then verify/correct seams. Choose music sourcing only after its rights/capability are confirmed.
4. Resolve Unity playback/output on the local device using the TEST clip and one temporary listener/source; require nonzero output and listening confirmation before final integration. Keep TEST clips excluded from production definitions.
5. When Phase 5A is explicitly authorized, implement one bounded audio presentation service and mixer routing using existing committed events, one active listener, voice caps/priorities/pooling/cooldowns/aggregation and randomized variants. Do not alter gameplay ownership to support sound. Load/stream based on clip duration and profiler evidence.
6. Produce only the bounded approved initial SFX batch, import and validate it, then test dense 100–400-enemy scenes and a standalone build for CPU/memory/voice peaks. Optional radio uses the installed official speech skill once its own credentials/terms and disclosure are ready.

No Phase 5A work was started automatically.
