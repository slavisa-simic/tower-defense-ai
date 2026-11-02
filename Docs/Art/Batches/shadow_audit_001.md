# Visual shadow correction — 2026-11-02

## Scope and provenance

Authorized by the user's “VISUAL SHADOW AUDIT” request: diagnose and fix short pixel-art shadows on Danube Border and Industrial Port without a scene redesign or another gameplay phase. Existing uncommitted Phase 6B work is preserved.

References: approved environment master `Assets/Game/Art/MasterReferences/bd_ref_danube_master_v002.png`; current aquatic masters and identities recorded in ART_BIBLE.md. Existing runtime sprites supply alpha silhouettes. No new raster artwork, ImageGen prompt, source sheet, crop, texture rewrite, or approval claim. The new shader is code-native presentation infrastructure.

## Deliverables and contract

- `Assets/Game/Materials/PixelShadow.shader`: one alpha-only unlit URP 2D pass, Point-filtered source sprites, flat neutral tint, no blur/lighting/3D shadow maps.
- `PixelShadowLow.mat`: (+2,-2) logical source pixels at 51.2 PPU, #1F2629 at .28 opacity.
- `PixelShadowMedium.mat`: (+3,-3), same tone at .30.
- `PixelShadowTall.mat`: (+5,-5), same tone at .33.
- Offset occurs in world XY after sprite rotation/scale. Child transforms align with their owning silhouette, so facing does not rotate the light direction.
- Eight active aquatic enemy prefabs retain one shadow under Visual; duplicate root shadows removed. Existing EnemySpritePresentation synchronizes animated sprite and hides contact underwater, underground, on death and on disable.
- Eight defense prefabs and PlayerBase_TopDown use fixed-footprint/body silhouettes. Wire retains the existing connectivity owner and updates its shadow mask.
- Both saved map scenes use the same materials. Port adds seven missing small-prop shadows. Thirty-one paving/compound/marking renderers move to Environment; corrected shadows use GroundObjects -20. Existing body layers/orders stay intact.
- The hidden retired boat's orphan shadow is disabled. Active organic carriers remain water presentations with wakes/emergence; no land contact or new gameplay is added.

Existing PNG bytes/importers, prefab GUIDs, actor roots, camera, footprints, colliders, navigation, health, currency and wave/arrival scheduling remain intact. Authoring uses AssetDatabase, PrefabUtility and EditorSceneManager through the pinned 6000.3.24f1 Editor. Reproducible authoring and exact receipts: `Docs/Validation/ShadowAudit/Author.cs` and `authoring-receipt.json`.

## Validation

See `SHADOW_AUDIT_REPORT.md` for final compile, Console, tests, performance limits and visual review. Each before/after camera fixture uses eight real spawned creature types, eight purchased defenses and a disclosed 20,000 RSD inspection grant in isolated Play Mode storage. These are presentation inspections, not earned-income playthrough claims. Comparison PNGs are identically positioned screenshot crops at 2x nearest-neighbor, with labels; runtime art is not edited.
