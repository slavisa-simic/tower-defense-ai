# Audio asset provenance and licenses

Prepared 2026-10-02. No final production audio is approved or connected to gameplay.

| Asset | Source | Author/Generator | License / usage status | Source URL | Generated/Downloaded | Modified | Approval / integration |
|---|---|---|---|---|---|---|---|
| `Assets/Game/Audio/Source/Environment/sfx_TEST_pipeline_noise_01.wav` | Original deterministic white noise, seed 1977 | Project-authored FFmpeg lavfi recipe | Project-authored validation content; no third-party sampled material | Local recipe in `Docs/Validation/PrePhase5A/AudioProcessingSmoke.ps1` | Procedurally generated; not AI-generated | Fade-in/out; 48 kHz 24-bit mono PCM | TEST ONLY; not bound to gameplay |
| `Assets/Game/Audio/Processed/Environment/sfx_TEST_pipeline_noise_01.wav` | Above source | Project processing workflow / FFmpeg | Same provenance as original | Same recipe | Derived from procedural test | Trim, silence removal, fade, loudnorm, 48 kHz 16-bit mono PCM | TEST ONLY; Unity import validation |
| `Docs/Validation/PrePhase5A/TEST_conversion_stereo.ogg` | Above processed test | FFmpeg libvorbis | Same provenance as original | Same recipe | Derived test export | Stereo conversion, Vorbis q4 | Validation receipt only; outside Assets |

For future entries add the exact provider/model/version, account-plan terms at generation date, prompt, reference rights, seed/duration/loop parameters, task ID, original URL/receipt, SHA-256, derivative relationship, format measurements, approval/revision and importer/integration target. Never enter an uncreated asset as generated.

## Tool/service records (not audio assets)

- FFmpeg 9.0.1 Essentials installed through WinGet `Gyan.FFmpeg.Essentials`, with installer hash verification. [FFmpeg official download page](https://ffmpeg.org/download.html) links the Gyan Windows build provider. Retain the distributed license files with the external tool; no FFmpeg binary is shipped in the game.
- [Runway provider skill](https://github.com/runwayml/runway-studio-skills) declares MIT for its software; inspected main revision `c1c0348fda3acf4ef864393b6485afb4600eedda`. Software licensing is separate from service output rights.
- [Runway commercial-use help](https://help.runwayml.com/hc/en-us/articles/21668707517587-Can-I-use-the-content-I-made-in-Runway-for-commercial-purposes) states generated content can be used commercially. Before any production generation, record the current service/model/plan terms and rights to all references; this is not clearance of third-party inputs. No external-service audio was created in this setup.
- [Official speech skill](https://github.com/openai/skills/tree/main/skills/.curated/speech) is installed for optional future radio voiceovers. Record applicable OpenAI service terms and AI-voice disclosure when an actual clip is created. No speech clip was created in this setup.
