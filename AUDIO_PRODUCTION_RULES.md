# Border Defenders audio production rules

Prepared 2026-10-02 for Phase 5A. This document sets production requirements; it does not authorize starting Phase 5A or mark any test clip as production content.

## Formats and folders

- Preserve the generator's original download unchanged in `Assets/Game/Audio/Source/<category>/`. Prefer lossless WAV PCM, 48 kHz, 24-bit source masters when the provider can actually deliver them. Retain a native MP3 if that is the only provider output; transcoding does not recover lost fidelity and must be labeled as a decoded working master.
- Use 48 kHz PCM WAV, 16-bit or 24-bit, for processed short SFX. Mono is the default for positional weapons, impacts, enemies and boats. Stereo is appropriate for music, broad ambience and authored UI where justified.
- Use OGG Vorbis for approved longer music/ambience exports where useful. Unity can also compress a WAV during import; avoid double lossy compression. Choose runtime import settings after target-device profiling.
- Categories for Source and Processed: Weapons, Explosions, Impacts, Enemies, Bosses, Boats, Environment, UI, Music, Radio. Create a category folder when it receives its first asset; only Environment currently contains test clips. Likewise, create Mixers and Definitions when actual assets need them. Keep prompts/receipts/license records in `Docs/Audio/`, not runtime folders. Preserve existing asset GUIDs and do not relocate working assets without AssetDatabase.

## Names, revisions and variations

Use lowercase snake case, category prefix, descriptive action, and two-digit variation: `sfx_mg_shot_01.wav`, `sfx_sniper_shot_01.wav`, `sfx_mortar_launch_01.wav`, `sfx_mortar_impact_01.wav`, `amb_danube_water_01.wav`, `music_combat_pressure_01.ogg`. Candidates use `_r01`; approval records identify an exact revision. Validation content must contain `TEST` and must never be referenced by a production definition or saved scene.

Plan at least three materially distinct automatic-weapon variants and two impacts/explosions where the action repeats frequently. Use bounded pitch/gain variation and avoid immediate repeated variants. Do not randomly change radio intelligibility. Only generate a bounded approved batch after the generation smoke test succeeds.

## Loudness and editing

Keep headroom; do not peak-normalize every clip to 0 dBFS. Starting candidate ceiling is -3 dBTP. Balance by perceived loudness and importance in a representative dense scene; a quiet UI cue and an explosion need different levels. Integrated LUFS is useful for long music/ambience but unreliable for very short transients. Use sample/true peak and short-term loudness plus listening for SFX. Record measurements before/after; no universal LUFS target is final until in-game mix review.

Trim unnecessary leading/trailing silence, preserve weapon attacks, and use short fades to prevent discontinuities. FFmpeg supports trimming, fades, gain/loudnorm, resampling, channel conversion, silence removal, peak and EBU R128 measurement; verified setup receipts are under `Docs/Validation/PrePhase5A/`. Use two-pass measured loudness normalization for long assets when needed. Never overwrite originals while processing.

Loop candidates must be checked across multiple joins for clicks, changing noise floor, rhythm drift and stereo discontinuities. A provider loop flag is a request, not proof. Author/crossfade and listen to seam exports before approval. The current procedural test is not a seamless-loop asset.

## Provenance and licensing

Register every generated/downloaded source and derivative in `AUDIO_LICENSES.md` before approval. Record exact asset, source/provider, author/model/version, license/terms and URL/date, generated/downloaded status, prompt and any uploaded reference provenance, generation parameters/task ID, modifications, measured format/duration/channel layout, approval and Unity target/import settings. Keep original receipts and hashes.

Use original recordings, legitimate generation services or sources with explicit game/commercial-use rights. No ripped commercial-game, movie, TV, YouTube, music or military-footage audio. Generator software license and generated-output usage terms are different records. Check provider/model/plan terms and reference rights for the actual generation; a marketplace listing alone is insufficient. AI radio voiceovers require end-user disclosure and must not impersonate a real person without authorization.

## Future runtime performance contract

100+ infected, simultaneous boats, automatic weapons, explosions and flamethrowers require a bounded presentation system. Gameplay must never wait for or depend on audio. Reuse committed gameplay events; keep damage, spawns, rewards and wave state in their existing owners. Do not add one unrestricted AudioSource per gameplay event.

Phase 5A must establish pooling, global and category voice caps, priorities, steal/drop rules, per-emitter cooldowns, distance culling, nearby-event aggregation, variation selection and profiling counters. Suggested initial planning budget: at most 24 simultaneous SFX, including four reserved UI/critical slots; at most two ambience, two music crossfade and one radio voice (29 total). This is a design starting point, not an implemented guarantee. Critical UI/radio/boss warnings should outrank distant repetitive shots. Automatic fire and flame need bounded bursts/loops rather than unlimited per-bullet voices. Do not suppress gameplay events to meet the audio budget.

The current project has 32 real and 512 virtual Unity voices; virtualization alone does not prevent event storms or allocations. Phase 5A needs one active AudioListener, an evaluated mixer routing plan, category volume controls and tests at 100/200/400 enemies. Short frequent effects are candidates for preloaded DecompressOnLoad; long music is a streaming candidate. Confirm memory/CPU/latency on the target build before fixing settings. Avoid loading/decoding files or creating/destroying AudioSources in combat loops.

## Generation tooling status

Installed official `openai/skills` speech skill: radio/TTS only, not weapons/music. Installed provider-owned `runwayml/runway-studio-skills`: sound effects, TTS, voice conversion/dubbing/isolation. Runway SFX exposes a 0.5–30 second duration and loop request. Its checked CLI does not select output sample rate, bit depth, channels or WAV encoding; native downloaded output must be probed. Music generation is advertised by the unconnected Runway plugin, but is not an operation in this installed skill and has not been verified. Credentials and a successful small real-generation test are required before production.
