# Border Defenders art production rules

Prepared 2026-09-18. This is the production contract. No art was generated during setup.

## Visual direction

- Strict top-down, orthographic overhead view perpendicular to XY. No isometric projection, tilted camera, horizon, side-view people, or tall front-facing facades. Roofs, shoulders, helmets, weapons, and ground footprints establish silhouettes.
- Alternate post-nuclear Balkan setting: the Danube defensive line, military quarantine barriers, weathered concrete, muddy riverbanks, improvised fortifications, and infected arrivals from southern radiation zones in radiation-resistant inflatables and improvised river craft.
- Water stays on the left, defensive base on the right. Geographic lore does not rotate the playable map. Existing coastal identifiers remain valid.
- Gritty pixel art with deliberate pixel clusters, restrained surface noise, readable silhouettes, consistent outline weight, scale, light direction, palette, and material treatment. Avoid smooth painted gradients, mismatched pixel sizes, noisy photo textures, or glossy UI.
- Use muted river blue/green, mud brown, concrete gray, military olive, rust, and bone. Reserve limited brighter colors for threats, targeting, warnings, and interaction. Terrain must remain quieter than enemies and defenses. The master reference records exact swatches; no palette is approved yet.
- Use overhead ambient lighting with restrained shading. Strong baked directional shadows on rotating units will rotate incorrectly; keep such shadows separate or minimize them. Prioritize silhouette and value contrast over gore/detail.
- Generate real transparency for isolated sprites and icons. A checkerboard or solid background painted into RGB is not alpha. Tiles and reference boards may be opaque. Inspect edges on light and dark backgrounds.

## Production sequence

1. **Phase 3A-1: master reference.** Follow `Docs/Art/PHASE_3A_1_BRIEF.md`. Produce a coherent reference candidate, record the exact prompt, review it at gameplay scale, then record the user's approval or already-authorized selection. Do not invent approval. No mass production precedes the approved reference.
2. **Pilot batch:** a small representative set covering riverbank/ground, one Crawler, and one defense. Finalize pixel density, silhouettes, pivots, alpha, and Unity rendering against the reference. Validate in the saved prototype before expanding.
3. **Core batches:** environment/base, enemies, defenses, then props/effects in bounded packs. Each has named outputs and measurable acceptance criteria. Reuse the approved reference file, not just a textual description of its style.
4. **Animation batches:** animate approved unit/weapon designs after static silhouettes and scale work. Check every frame against the approved design.
5. **Initial UI batch:** build icons, money icon, HUD basics, wave/start-wave visuals, simple panels. Keep polishing proportional to the world/enemy/defense set's progress.

Only perform the batch requested by the user. Preparation is not permission to execute the next phase. Reuse session authorization rather than asking again for an already-approved action. If the reference requires review, present the concrete candidate before asking for its selection.

## Image generation workflow

- Use the installed `imagegen` skill and built-in image generation by default. Capability is available; setup intentionally does not exercise it. An API key is not required for this built-in path.
- Read local input images with `view_image` before using them. Label each as style reference, edit target, or supporting input. Use the tool's supported local reference paths; do not mix local-path and recent-conversation reference mechanisms.
- Give each call a defined subject, overhead camera, style/palette reference, intended logical size, orientation, transparency, margins, and exclusions. A batch is a bounded list of outputs, not one unstructured sheet of unrelated assets.
- Copy chosen project outputs from the tool's returned location into this repository. The built-in tool commonly saves under `$CODEX_HOME/generated_images`; do not invent a destination argument or leave project dependencies there.
- Record actual output dimensions. Generative output is not guaranteed to have exact pixel clusters, aligned frames, seamless tiles, or clean alpha. Inspect before promotion. Do not label a reference illustration as an import-ready sprite.
- Use image generation for visual edits by default. If deterministic cropping, resizing, palette reduction, or sheet packing is explicitly requested, record its settings and preserve the source; avoid interpolated scaling of pixel art. Unity slicing itself is import configuration.
- Keep output provenance and exact prompts in each batch record. Do not claim a fixed random seed, bit-identical reproducibility, or model identity unless the tool exposes it. Repeatability means a recorded process and retained source.
- CLI/API generation is a separate fallback requiring the user's explicit choice under the imagegen skill. Do not install API dependencies or request credentials for the default built-in workflow.

## Folder ownership

| Folder | Contents |
|---|---|
| `Assets/Game/Art/MasterReferences/` | Master candidates and approved visual reference revisions; never scene backgrounds by default |
| `Assets/Game/Art/SourceSheets/<batch>/` | Retained raw generated PNGs, original sheets, and source revisions; do not reference directly from runtime assets |
| `Assets/Game/Art/Environment/` | Runtime water, shore, terrain, and ground detail |
| `Assets/Game/Art/Enemies/` | Runtime Crawler and later requested enemy sprites/sheets |
| `Assets/Game/Art/Defenses/` | Runtime MG, wire, mine, sniper base/weapon sprites |
| `Assets/Game/Art/Effects/` | Muzzle, tracer, explosion, impact sprites/sheets |
| `Assets/Game/Art/UI/` | Icons, button and panel textures only |
| `Assets/Game/Art/Props/` | Quarantine markers, debris, river craft visuals, and set dressing |
| `Assets/Game/Art/Base/` | Player-base visual pieces |
| `Assets/Game/Art/Placeholders/` | Existing prototype fallback textures; retained |
| `Assets/Game/Animations/<category>/` | Unity `.anim` clips and `.controller` assets |
| `Assets/Game/Prefabs/` | Existing gameplay prefabs, with presentation changed in place when authorized |
| `Assets/Game/UI/` | Existing UI Toolkit UXML, USS, theme and PanelSettings |
| `Assets/Game/Data/` | Existing ScriptableObject configuration; art does not own gameplay values |
| `Docs/Art/` | Register, briefs, batch records, import/integration procedures |
| `Docs/Validation/ArtPipelineSetup/` | Setup verification; later batches use their own validation subfolders |

Keep sources/references out of Resources, StreamingAssets, and runtime references. Their placement under Assets allows inspection but does not make them runtime deliverables. Empty category folders contain `.gitkeep` so Git retains them; Unity supplies folder `.meta` files. Do not create competing top-level art folders.

## Names, versions, and records

New art uses lowercase ASCII snake_case. Use:

`bd_<category>_<subject>_<part-or-action>[_<direction>]_vNNN[__fNNN].png`

Category tokens: `ref`, `env`, `enemy`, `defense`, `fx`, `ui`, `prop`, `base`, `src`. Directions, when needed: `e`, `ne`, `n`, `nw`, `w`, `sw`, `s`, `se`; `e` is local +X. Versions start at `v001`; standalone frame suffixes start at `__f000` and use zero padding. A runtime sheet uses the same name with `_sheet` before the version. Unity sub-sprite names carry the stable semantic name plus frame index.

Examples (names only; these files do not exist yet):

- `bd_ref_danube_master_v001.png`
- `bd_enemy_crawler_walk_e_sheet_v001.png`, sub-sprite `bd_enemy_crawler_walk_e__f000`
- `bd_defense_machine_gun_nest_weapon_v001.png`
- `bd_ui_money_icon_v001.png`
- `bd_fx_mine_explosion_sheet_v001.png`

New animation/controller names use the matching stem and `.anim` / `.controller`. Existing PascalCase prefab, data, code, and UI filenames stay intact. Do not rename them merely to satisfy new-art naming rules.

Batch IDs use `p3a1_master_001`, `p3a2_pilot_001`, and similarly explicit phase/purpose/sequence names. Copy `Docs/Art/BATCH_TEMPLATE.md` to `Docs/Art/Batches/<batch-id>.md` before production. Register every retained deliverable in `Docs/Art/ASSET_REGISTER.md` with its source, reference revision, status, and consuming asset path. Status progresses from planned to candidate to approved to integrated to validated; rejected/superseded states retain a reason. Approval and integration are separate facts.

Never overwrite approved files without a recorded reason and authorized replacement scope. Use sibling versions for new candidates. Preserve `.meta` GUIDs when intentionally replacing a currently referenced asset; use Unity's AssetDatabase for moves and prefab changes. Do not change existing sub-sprite identities casually: clips can depend on their file IDs. Never delete all metadata or blanket-reimport/rewrite the art tree as a convenience.

## Import and integration acceptance

Follow `Docs/Art/UNITY_INTEGRATION.md` for pixel density, slicing, pivots, animation, UI, and validation. No texture is accepted solely because it looks good enlarged in a reference viewer. Check gameplay-size readability, transparency, borders, sorting, dimensions, rotation, clip continuity, and the consuming prefab/UI.

Replace visuals in small reversible batches while keeping the prototype playable. Do not change currency, damage, waves, grid/nav rules, collision/footprints, or spawn logic to accommodate artwork. Report generated paths, reference version, import settings, modified consumers, tests, and remaining defects for every integrated batch.
