# Navigation collision integration — 2026-11-02

Scope: the user's collision/navigation/structure-combat overhaul, not another art production phase. No imagery, source sheets, textures, sprites, animations, imports or visual reference revisions were generated or replaced. Existing Phase 6B art and R2 shadow bindings remain intact.

Saved consumers changed through Unity PrefabUtility/SerializedObject/EditorSceneManager APIs in `Docs/Validation/NavigationCollision/Author.cs`:

* Both playable scenes: separate `Navigation Obstacles` root with authored permanent footprint IDs and simple BoxCollider2Ds. BattlefieldDefinition owns the matching grid rectangles. Existing sprite roots, transforms, sorting and GUIDs retained. Port crane footprints cover right-hand chassis/supports; projecting booms remain overhead and the x=6 quay corridor remains open. Gate supports retain explicit breach openings.
* Eight defense prefabs: PlayerDefense layer; six solid types receive footprint-sized BoxCollider2Ds and reuse damage smoke/hit/destruction VFX. Wire/Mine remain walkable. Gameplay footprint sizes do not change.
* Eight current creature prefabs: Enemy layer and the focused structure-attack component. Existing visual children, clips, shadows and presentation remain bound to original assets.

Reason: sprites/build reservations did not consistently represent navigation obstacles. Existing images are preserved; the new footprint policy is explicitly authorized gameplay work, not an art-driven gameplay adjustment. No prompt/reference approval applies to a code/collider-only integration.

Validation receipts and limitations are recorded in `COLLISION_NAVIGATION_COMBAT_REPORT.md` and `Docs/Validation/NavigationCollision/`.
