# p4c_content_001 — special infected, pirate boss, transports and dismantle

Date2026-10-02. User Phase4C autonomous art generation/integration authorization. Approved style reference: Assets/Game/Art/MasterReferences/bd_ref_danube_master_v002.png. Exact successful prompts, reference paths and tool-returned source paths: [prompt receipt](p4c_content_001.prompts.json). Native dimensions and export methods: [art-import receipt](../../Validation/Phase4C/art-import.json). Final GUID/import audit: [audit](../../Validation/Phase4C/final-audit.json).

## Approval and blocked deliverable

Agent-selected revisions under user authorization, no separate human approval invented. Requested ref_boss_lineup_01.png depicting the two violent boss characters was rejected by imagegen: HTTP400 moderation_blocked, public-figure/violence categories, request02cf49aa-5477-4cd8-8c88-a3aba2eba5c3. Intended prompt requested a strict-overhead pixel-art lineup of Undead Osama Bin Laden and Somali Pirate Warlord against the approved Danube master style. No lineup asset or new Osama animation was produced. Existing Brute sprites remain the explicit functional placeholder on UndeadOsama_TopDown. This limitation makes Phase4C PARTIAL.

## Source lineage and runtime inventory

All raw source files remain byte-identical beneath Assets/Game/Art/SourceSheets/p4c_content_001; unused candidates are retained there without runtime consumers.

| Selected source | Rejected/superseded | Runtime target |
|---|---|---|
| breacher_v002.png | breacher_v001.png tilted/front anatomy | IsisBreacher_TopDown |
| heavy_v002.png | heavy_v001.png tilted/front anatomy | IsisHeavy_TopDown |
| runner_v002.png | runner_v001.png tilted/front anatomy | PirateRunner_TopDown |
| raider_v002.png | raider_v001.png tilted/front anatomy | PirateRaider_TopDown |
| pirateBoss_v003.png | pirateBoss_v001/v002.png excessive face/front view | PirateWarlord_TopDown; BossDefinition.bossArt |
| militantBoat_v001.png | none | MilitantBossTransport.prefab |
| pirateBoat_v001.png | none | PirateBossTransport.prefab |
| toolsIcon_v001.png | original soft fringe removed at export | TopDownHud.uss dismantle icon |

For each of IsisBreacher/IsisHeavy/PirateRunner/PirateRaider/PirateWarlord, five sheets Assets/Game/Art/Enemies/Special/<Id>_<idle,move,attack,hit,death>_v002.png and corresponding Assets/Game/Animations/Enemies/Special/<Id>_<action>.anim. Four frames per action, 20 per character, 100 total. Source layout four columns/five rows. Ordinary cells64x64, sheets256x64; Warlord cells96x96, sheets384x96. Sprite names semantic __f000–__f003, center pivot,51.2PPU; frames bind existing EnemySpritePresentation idle/walk/attack/hit/death arrays and Visual/Body SpriteRenderer. Clips animate only Visual/Body m_Sprite. Clip inventory: movement8FPS except PirateRunner12, death9, other10; live observer retains inherited walk12FPS on runner-derived prefabs and6 on Brute-derived prefabs, idle5, attack10, death9. Clips are asset inventory; the existing runtime sequencer owns playback.

Boat runtimes: Assets/Game/Art/Props/{MilitantBossTransport,PirateBossTransport}_v001.png,192x96, same bounds as normal transport. Hull sprite changes only; seven seats, routes and cargo managed by shared BoatLandingSystem. Icon: Assets/Game/Art/UI/ui_icon_dismantle.png64x64, crossed tools, build HUD with functional refund text.

## Export/import and integration

Docs/Validation/Phase4C/Art.cs records cell selection and nearest export. Alpha>=220 retained as255, others0. Five-bit channels, no dither; common fit scale per character, opaque bounds centered per cell, padding12; boat/icon padding8. Native sources are never overwritten. Runtime textures Point, uncompressed, no mipmaps, Clamp, NPOTNone, max4096,51.2PPU,FullRect, center pivot; Multiple sprites sliced through SpriteDataProvider with stable IDs on repeat authoring. Final.cs enforces/audits FullRect on all28textures. Raw files remain ordinary textures. Existing asset GUIDs and originals retained; new prefab roots preserve existing movement, health, shadow, burn and arrival presentation architecture. No collision/footprint art changes.

## Validation

Full88EditMode/50PlayMode; post-review5Phase4CPlayTests and2ArtIntegrationTests. Primaryscene clean, missing scripts0, finalConsole0errors/0warnings. Actual wave50 and dismantle screenshots inspected. Boss-name contrast corrected after inspection. Performance and remaining art/manual limitations are documented in [Phase4C report](../../../PHASE_4C_REPORT.md).

