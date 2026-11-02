# Visual shadow audit — 2026-11-02

1. Inventory both saved maps, active URP/camera renderers, materials, lights/casters, prefab shadows and source artwork; capture a reproducible baseline before edits.
2. Retain separate sprite shadows. Correct effective alpha, use short world-aligned pixel offsets and appropriate silhouettes, preserve all gameplay roots/GUIDs/footprints. Check baked contact shading before adding shadows.
3. Compile in the pinned Unity Editor, inspect Console, run relevant suites, compare identical before/after fixtures on both maps, review the scoped diff and record results in SHADOW_AUDIT_REPORT.md and the art register.

Existing Phase 6B work is already present and uncommitted. Preserve it. No scene redesign, asset regeneration, gameplay phase, balance, navigation or UI changes are authorized by this task.

## Directional revision R2 — user feedback, 2026-11-02

Capture the current weak result with the existing fixture/camera, verify actual terrain sorting, and review same-camera gameplay images. Initial R2 4/6/8px width was still restrained at fitted gameplay scale; final lower-right world-space casts are (+4,-3)/(+8,-6)/(+12,-9) logical pixels at 36/40/40% opacity. Small props/wire/mines use Low. Keep the renderer/silhouette system and adjust regression assertions to the stronger authorized rule. Compile, run focused presentation tests, inspect Console/shader output, update the current contract/report and retain separate R2 screenshot evidence.
