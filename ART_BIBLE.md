# Border Defenders — visual direction

The primary game is strict perpendicular overhead pixel art in XY. Water remains left; quarantine defenses and inland exits remain right. Preserve existing camera, grid scale, sorting layers, defense footprints and authored environment. The old isometric scene is a regression reference.

## Shared contact and cast shadows

Use clearly visible short, hard pixel casts toward the lower-right, with light from upper left: world XY (+X,-Y), neutral #1F2629, 36–40% opacity. Shared PixelShadow Low/Medium/Tall materials offset (+4,-3)/(+8,-6)/(+12,-9) logical source pixels at 51.2 PPU, after rotation and scale. Small creatures, small props, vegetation, wire and mines use Low; containers/ordinary structural props/defense footprints use Medium; buildings/cranes/bosses use Tall. Evaluate at fitted gameplay size, where source pixels can be smaller than screen pixels. Do not stretch, shear, blur or project silhouettes into a perspective view. This R2 rule supersedes the first correction's 2/3/5-pixel weak contacts.

Separate child SpriteRenderers use the owner's alpha silhouette and GroundObjects -20, above Environment terrain/floor markings and below solid object sprites. Keep one shadow per owner. Animated creatures reuse the actual body frame under Visual; their existing presentation owner hides contact in water, underground, on death and during pooling. Water carriers use their existing submerged/emergence/wake treatment without a land contact shadow. Retain subtle painted edge/material shading, but do not add another directional baked cast shadow or duplicate an existing separate shadow. The old partially transparent ellipse is historical inventory. See SHADOW_AUDIT_REPORT.md and batch shadow_audit_001.

## Aquatic creature direction

Original biological river horror: grounded grotesque anatomy, wet skin, gills, fin membranes, lamprey-like mouths, crustacean limbs, sensory organs, mud, algae and bone/keratin plating. Use recurring anatomy across the ecosystem. Avoid humanoid bodies, generic fantasy demons, ordinary fish with attached legs and recognizable copies of other games' creatures.

Readable silhouette comes first. Crawler: low, broad amphibian. Skitter: narrow body and long angular limbs. Bulwark: broad plated shell. Spitter: elongated body with swollen toxic throat. Broodling: tiny translucent parasite and red-brown sac. Burrower: segmented mud worm with shovel limbs. Dredger: massive clawed sediment armor. Broodmother: bloated reproductive chambers and asymmetrical fins.

Drift Pods are rupturing organic cysts. Brood Carriers open living brood/gill chambers. Leviathans are massive aquatic beasts that release cargo and retreat underwater. Their presentation must communicate biology rather than seats and hulls. Submerged creatures use tinted silhouettes, bubbles and wake; land creatures use subtle contact shadows. Burrowers always have visible mud disturbance.

Muted muddy olive, river teal, sediment brown, bone and rust match the established environment. Reserve limited turquoise for corrosive anatomy and readable water effects. Restrained fluid/rupture effects must not obscure combat. Lighting is overhead and consistent; no rotated baked directional shadows.

Selected Phase 6B masters: `Assets/Game/Art/MasterReferences/ref_aquatic_enemy_lineup_01.png`, `ref_aquatic_carriers_01.png`, `ref_aquatic_boss_lineup_01.png`. Agent-selected under the autonomous Phase 6B request, with no independent human approval claimed. Existing `bd_ref_danube_master_v002.png` continues to govern environment palette and defense presentation; its former enemy imagery is superseded.

Production/import specifications and provenance remain in `ART_PRODUCTION_RULES.md`, `Docs/Art/UNITY_INTEGRATION.md`, `Docs/Art/ASSET_REGISTER.md` and batch `p6b_aquatic_001`. Runtime world art is Point-filtered at 51.2 PPU, discrete sprite frames, transparent margins and centered local +X orientation. Animation does not drive damage, movement, release timing, rewards or wave state. Audio remains deferred.
