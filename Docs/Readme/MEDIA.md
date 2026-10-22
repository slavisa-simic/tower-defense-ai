# README media

Created 2026-10-22 for the requested repository presentation refresh. Media stay outside Unity's Assets tree. Existing game art, metadata, scenes, configuration, and gameplay source are unchanged.

The visual hierarchy follows `ui-ux-pro-max` guidance for contrast, consistent styling, descriptive alternatives, responsive layouts, and meaningful motion. The palette comes from `ART_BIBLE.md` and approved Danube master v002. The title and section labels use the existing Border Defenders HUD font, converted into self-contained SVG paths. Existing runtime sprite pixels are represented as SVG paths; no external fonts, embedded scripts, raster links, or new ImageGen assets are required by the SVGs.

| Media | Origin and treatment |
| --- | --- |
| `Media/hero.svg` | Code-native documentation layout, project palette, HUD glyphs, existing rubber boat, MG preview, and wire icon |
| `Media/section-*.svg` | Four matching code-native section labels using existing build/money icons and HUD glyphs |
| `Media/danube-border.png`, `Media/industrial-port.png` | Fresh 960×540 world-camera captures from Unity 6000.3.24f1, after placing three MG nests |
| `Media/*-poster.png` | Frame 60 of each capture, used for reduced-motion presentation |
| `Media/danube-assault.gif`, `Media/port-assault.gif` | 120 actual engine frames each, sampled at 10 fps for 12 seconds; FFmpeg palette generation/use, 192 colors, Bayer dithering, infinite loop |
| Master reference shown in README | Existing `Assets/Game/Art/MasterReferences/bd_ref_danube_master_v002.png`; linked directly and explicitly labeled as art direction |
| HUD screenshots shown in README | Existing Phase 5C and Phase 6A QA captures; linked directly, with their setup-budget/wave-jump context disclosed |

The new engine captures start with the ordinary 500 RSD budget and three 150 RSD MG nests, placed through BuildManager. No enemy, economy, wave, or breach tuning is changed. The capture camera temporarily fills its RenderTexture; it omits UI Toolkit. User save data is isolated under the ignored `TestResults/Readme/LocalData/` directory before gameplay loads. The camera state is restored and Play Mode stopped after recording.

Danube Border uses cells (10,8), (16,12), (21,6), seed -1229781708, and records 10 kills / 0 Homicides. Industrial Port uses cells (8,8), (16,12), (21,6), seed 1610795612, and records 9 kills / 0 Homicides. Both clips finish during Wave 1 spawning. These are scripted presentation captures, not manual balance runs. Seeds and results describe these retained takes; a regeneration selects a new ordinary run seed.

Unity reported no compilation failures or Console errors during capture. It emitted one `SpriteAtlasManager.atlasRequested wasn't listened to while PortEnvironment requested.` warning while loading the port. The captured props and terrain were visually inspected. No runtime repair or atlas change was made as part of this documentation request.

Output sizes, dimensions, and SHA-256 values are recorded in [media-manifest.json](media-manifest.json). The README uses local relative paths, GitHub Mermaid blocks, expandable screenshot sections, and static poster sources for `prefers-reduced-motion`. Desktop and narrow-width preview checks cover image loading, diagram rendering, horizontal overflow, and local links. The 122/75 gameplay test counts quoted in README belong to the separate completed code-cleanup run.

Regenerate the SVGs from the repository root with Python and Pillow:

```powershell
python Docs/Readme/Tools/build_media.py
```

For fresh captures, use a connected Unity 6000.3.24f1 Editor with the existing Pipeline package. The main menu is opened before Play Mode so storage can be isolated before gameplay initializes:

```powershell
unity command set_autotick --enable true --project-path .
unity command run_script --file Docs/Readme/Tools/CaptureShowcase.cs --entry CaptureShowcase.Prepare --project-path .
unity command editor_play --project-path .
unity command run_script --file Docs/Readme/Tools/CaptureShowcase.cs --entry CaptureShowcase.Border --timeout_ms 45000 --project-path .
unity command run_script --file Docs/Readme/Tools/CaptureShowcase.cs --entry CaptureShowcase.Port --timeout_ms 45000 --project-path .
unity command editor_stop --project-path .
```

Frame PNGs are written under the ignored `TestResults/Readme/Frames/`. Encode each sequence with FFmpeg, substituting the corresponding folder/output name for the port:

```powershell
ffmpeg -framerate 10 -i TestResults/Readme/Frames/danube-border/%04d.png -filter_complex "[0:v]split[a][b];[a]palettegen=max_colors=192:stats_mode=full[p];[b][p]paletteuse=dither=bayer:bayer_scale=3:diff_mode=rectangle" -loop 0 -y Docs/Readme/Media/danube-assault.gif
```
