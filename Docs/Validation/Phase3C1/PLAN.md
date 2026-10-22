# Phase 3C.1 plan and baseline findings

Baseline run on 2026-10-01 in Unity 6000.3.24f1: preparation captured, wave one started through WaveManager, existing direct spawning observed, then Play Mode stopped. Preserve the preexisting staged/untracked project; no reset or unrelated cleanup.

1. Keep the verified XY orthographic, identity-rotation camera. Review active art at gameplay scale and native resolution. Roofs, MG, sniper deck, crates, barrels and rubber boat are already overhead; do not regenerate them. The static wreck's diagonal in-plane rotation is not camera pitch. Radar/antenna/floodlight footprints warrant close final review.
2. Add shared short lower-right contact-shadow sprites under presentation roots, sparse deterministic reeds/grass/shrubs with no gameplay components, and clear active landing lanes of static boat props.
3. Add bounded configurable boat transport driven by WaveManager's existing scheduler. EnemySpawner remains the sole enemy initializer and validates unloading routes/spacing. Pending enemies remain pending until actually unloaded. Game Over cancels boats before further spawn commands.
4. Configure local sprite forward axis once, start MG/sniper left, and return smoothly left after target loss. Build a 16-mask wire visual set with identical edge anchors; update only changed cell and four neighbors, including removal and ghosts.
5. Compile, run all suites and new rule/integration tests, review arrival/unloading/combat/wire screenshots, inspect Console and complete phase report/register/handbook.

Actual presentation gaps: missing consistent shadow layer, empty vegetation zones, static-only boat dressing, enemies materializing at water spawn cells, idle weapon rotation left at identity (+X/right), and isolated wire visuals. Existing wire horizontal/corner sprites stop short of cell boundaries and cannot form gap-free connected chains.
