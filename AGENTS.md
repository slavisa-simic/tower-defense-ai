# Border Defenders project rules

- The game is a strict top-down pixel-art defense game. Primary gameplay uses XY (X horizontal, Y vertical, Z presentation depth), with Danube water left and base right. Older code and reports may call the water ocean/sea/coast; retain those identifiers unless a rename is requested. The Phase 1 isometric/XZ scene is a regression reference only.
- This repository is for a Unity game written in C#. Inspect the repository and relevant documentation before modifying it; understand existing architecture and dependencies first.
- Keep gameplay code modular and responsibilities clear. Prefer data-driven gameplay and ScriptableObjects for configuration when useful. Do not silently replace existing architecture or add duplicate managers.
- Keep gameplay state separate from UI and make state changes explicit so later authoritative multiplayer remains feasible. Do not implement multiplayer unless requested.
- Defensive structures use the shared DefenseDefinition / BuildManager placement / DefenseBase architecture. EconomyManager owns currency; UI observes it. Build occupancy is separate from navigation changes; blocking defenses require real route validation before placement.
- Prefer working software over speculative complexity. Avoid unnecessary packages, unrelated refactors, and final art unless requested.
- For substantial changes, make a short plan, compile in Unity, inspect Console warnings/errors, run relevant EditMode or PlayMode tests, review the diff, and fix regressions. Report any validation that could not be run.
- Document consequential architecture decisions briefly.

## Art production and integration

- Read `ART_PRODUCTION_RULES.md` before art work and `Docs/Art/UNITY_INTEGRATION.md` before imports or prefab/UI changes. Use the project skill `.agents/skills/game-art-pipeline-engineering/SKILL.md`; use `game-software-engineering` for substantial code work and `unity-cli` / the project `unity-pipeline` skill for Editor operations.
- Art direction is gritty, readable strict overhead pixel art: alternate post-nuclear Balkans, Danube military quarantine line, infected incursions by radiation-resistant inflatable boats and improvised river craft from southern radiation zones. This fiction does not authorize new boat, disembarkation, or combat systems.
- Preparation tasks create guidance and tooling only. Phase 3A-1 creates the master visual reference first; reuse its recorded approved revision for later bounded batches. Never treat a generated candidate as approved or begin unrelated assets.
- Keep approved references, raw source sheets, runtime sprites, animations, prefabs, UI layouts, and gameplay data in their documented folders. Record prompts, references, filenames, revisions, import settings, integration targets, approval, and validation in `Docs/Art/ASSET_REGISTER.md` and batch records.
- Preserve existing asset GUIDs, working placeholders, gameplay roots, footprints, sorting conventions, and presentation ownership. Move existing Unity assets through AssetDatabase with their metadata. Version new candidates; replace approved assets only within authorized replacement scope and record the reason.
- Initial UI art is limited to build icons, HUD basics, money, wave/start-wave controls, and basic panels. Keep UI Toolkit, functional text, affordability/selection states, and existing gameplay bindings. Prioritize world, enemy, and defense assets over UI polish.
