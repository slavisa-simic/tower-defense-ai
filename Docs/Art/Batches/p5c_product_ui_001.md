# Phase 5C product UI reuse

Authorized scope: user's autonomous Phase 5C main-menu/save/settings/history/UI request, 2026-10-03.

No raster generation, new approval claim, texture replacement, importer change, or prefab change. Reference direction remains approved Danube master v002 and Phase 3C integrated gameplay. Existing environment/base/prop SpriteRenderers are copied through Unity Editor APIs into a static, strictly overhead MainMenu scene. The menu copy contains no gameplay components, colliders, occupancy, enemy spawner, grid labels, or combat systems. Water stays left and checkpoint right.

Existing `bd_ui_hud_panel_large_v001.png` and the existing pixel font are reused for menu, settings, history, pause, confirmation, and run-summary panels. Existing build/world art is preserved. Live text remains UI Toolkit-owned; menu, tooltip, button-state, and settings styles live in `Assets/Game/UI/ProductMenus.uss`. Original GUIDs, texture Point sampling, and recorded 16px panel slices are retained.

Consumers: `Assets/Game/Scenes/MainMenu.unity`, existing `CoastalDefense_TopDown.unity` UIDocument, ProductMenuController, RuntimeTooltips, and existing BuildHud/StructureHud. Shared PanelSettings stays at 1280x720, Scale With Screen Size. Menu cards have proportional maximum size and vertical scrolling.

Authoring receipt: `Docs/Validation/Phase5C/Author.cs`. Actual rendered resolution captures and layout checks: `Docs/Validation/Phase5C/`. Final results are recorded in `PHASE_5C_REPORT.md`.
