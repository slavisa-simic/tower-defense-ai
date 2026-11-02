# Directional shadow revision R2 — 2026-11-02

The user rejected R1's result as still flat and authorized a stronger, clearly visible short cast toward the lower-right on both maps. No scene redesign or new gameplay phase.

## Sources and authoring

Reuse the approved environment master v002 and existing active aquatic references/sprites recorded in ART_BIBLE.md. No ImageGen call, prompt, raster export, new sprite, texture rewrite or approval claim. The only rendering changes are the existing shader's defaults, three existing shared material settings and smaller-contact material assignments. AssetDatabase/PrefabUtility/EditorSceneManager preserve existing GUIDs and owners.

| Tier | Source-pixel offset (world XY) | Color / opacity | Consumers |
|---|---|---|---|
| Low | (+4,-3) | #1F2629 / 36% | Ordinary creatures, small props up to 1.5 world units, reeds/shrubs, wire, mines |
| Medium | (+8,-6) | #1F2629 / 40% | Containers, ordinary structural props, fixed defense footprints |
| Tall | (+12,-9) | #1F2629 / 40% | Buildings, cranes, base, large biological bosses |

Offsets use 51.2 PPU; screen-down corresponds to negative Unity Y. All tiers share a 4:3 lower-right direction, applied after rotation/scale. Structural casts stay a small fraction of their footprints, without blur, shear or perspective. Gameplay review found initial 4/6/8px widths too restrained under the fitted camera; final widths are 4/8/12px.

The sorting audit verifies every enabled shadow above every Environment renderer in both maps. GroundObjects -20 was already correct; no layer changes were needed. Small static props and LandMine/BarbedWire shadows bind Low. All active sprite/animation/lifetime owners and the water-carrier treatment remain intact. No extra sprite renderers, textures, managers, per-frame loop, material instances or shader pass were introduced. Offsets move existing geometry; pass/sample counts stay unchanged. No frame-time benchmark is claimed.

## Evidence and validation

Fresh `r2-before` captures preserve the weak R1 result; final `r2-after` captures use the same scene/camera/1920x1080 fixture. Eight creature types and eight defenses with a disclosed temporary 20,000 RSD QA grant; no fixture objects saved into production scenes. Comparison sheets use identical crops, 2x nearest-neighbor and documentation labels only.

Material settings/consumer changes: `Docs/Validation/ShadowAudit/r2-material-receipt.json`. Sorting: `r2-sorting-audit.json`. Reproducible bounded update: `DirectionalR2.cs`; main Author.cs also retains the new rule for future reauthoring. Compile, actual test counts, Console and visual review are recorded in the R2 section of SHADOW_AUDIT_REPORT.md. R1 full-suite receipts remain historical and are not presented as rerun R2 results.
