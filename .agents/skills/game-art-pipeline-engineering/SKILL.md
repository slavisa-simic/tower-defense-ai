---
name: game-art-pipeline-engineering
description: Prepare and execute phased AI art production, sprite-sheet animation, and Unity prefab/UI integration for Border Defenders while preserving its working gameplay. Use for art batches, import workflows, and art replacement; preparation-only requests must not generate assets.
---

# Game art pipeline engineering

Read the current repository's `AGENTS.md`, `ART_PRODUCTION_RULES.md`, `Docs/Art/UNITY_INTEGRATION.md`, latest phase report, and affected source/consumers before changing anything. Inspect Git status and preserve existing work. These project documents own the current naming, palette, folder, import and validation contract; do not duplicate stale settings in this skill.

## Prepare or produce only the requested phase

- Preparation means inventory, skills/tools, folders, documentation and necessary import support. Do not call image generation, invent approved assets, or modify gameplay during preparation.
- Before production, inspect the asset register and user's requested batch. Generate a master visual reference before mass assets; record its selected version and actual approval evidence. Reuse the approved local image for every subsequent pack, not only a description. Do not repeatedly request approval already given in the session.
- Use `Docs/Art/PHASE_3A_1_BRIEF.md` for the first reference and `Docs/Art/BATCH_TEMPLATE.md` for each bounded batch. Define outputs, reference version, dimensions, alpha, silhouette and integration criteria before generation.
- Strict top-down XY, Danube water left/base right, gritty post-nuclear Balkan quarantine-line pixel art. Keep palette, scale, pixel clusters, light direction and silhouettes consistent. Boat imagery does not authorize boat gameplay.
- Use the installed `imagegen` workflow with the built-in tool by default, including transparent sprites. Inspect local reference/edit images first. Copy selected returned artifacts into the project's category folders; never leave runtime dependencies in the generator's private output folder. Follow explicit tool guidance for edits and fallback modes.
- Preserve raw source sheets and exact prompts. Record actual output sizes; reject fake alpha, warped grids, blurred pixels, mismatched frames or style drift before integration. Iterations should target a specific defect.

## Integrate without breaking play

- Reuse existing gameplay architecture and the `game-software-engineering` workflow for substantial code changes. No duplicate managers, new state in UI, gameplay redesign, speculative import framework or unnecessary packages.
- Retain existing placeholders until replacements pass review. Use stable names and sibling candidate revisions; do not overwrite approved assets outside authorized replacement scope. Document a replacement reason. Move Unity assets through AssetDatabase and preserve GUIDs/sub-sprite identities.
- Verify the pinned Editor and live project via `unity-cli`; use the project `unity-pipeline` skill for Editor operations. Discover commands, enable autotick when unfocused, refresh/compile after changes, and read Console results. Never claim unavailable tooling or unperformed validation as working.
- Keep sprites under presentation children. Preserve XY roots, footprint, sorting, local-+X facing, muzzle references and renderer lifetimes. Recheck child scales/tints inherited from shape placeholders. Keep state changes in existing health, combat, economy, placement and wave owners.
- Record exact sheet rectangles, frame order, pivot, names, FPS and loop flags. Animate renderer sprites only; no root motion or damage/reward events. Respect current facing code, immediate enemy death and mine removal semantics; lingering visuals require separate presentation lifetimes.
- Use existing UI Toolkit and its bindings. Art lives under Art/UI; layouts/styles stay under Game/UI. Start with icons, money/HUD, wave buttons and basic panels, and prioritize core world/enemy/defense art over UI polish.

## Validate and close the batch

Inspect imports, transparency, actual size, sorting, pivots, animation and serialized references. Run relevant existing EditMode/PlayMode tests when integration or code warrants them, check gameplay in the primary scene, and review the complete diff. Fix regressions caused by the batch; report any check that could not run. Preserve the separate legacy regression scene.

Update `Docs/Art/ASSET_REGISTER.md` and the batch record with what was generated, raw and selected paths, reference/approval evidence, import settings, consuming assets, tests/screenshots and unresolved defects. Keep the game playable between batches. Completion of a batch does not authorize unrelated production.
