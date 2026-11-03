# Phase 6D contextual guidance UI

Scope: runtime UI Toolkit tutorial panel, short banner, pulsing UI outlines and one temporary world outline. Existing `bd_ui_hud_panel_medium_v001.png` supplies the panel texture. No new raster art, prompts, source sheets, sprite imports, prefabs or approved reference replacements.

Implementation: `Assets/Game/Scripts/Tutorial/TutorialView.cs` builds one reusable card. Styles belong to `Assets/Game/UI/ProductMenus.uss`. Text and lesson policy live in `TutorialDefinition.cs`. World and UI targets retain their existing owners and bindings. Menus embed guidance in their real scrolling content; gameplay reserves the center below the boss warning and above the build bar. Functional labels and buttons remain UI Toolkit controls.

Authorization: user's autonomous Phase 6D implementation request. This is UI integration using existing art; no independent human approval or new generated-art approval is claimed. Existing texture GUIDs/import settings, pixel font, scene/prefab metadata, sorting, footprints and world sprites are preserved.

Validation evidence and any remaining issues: `Docs/Validation/Phase6D/` and `PHASE_6D_TUTORIAL_REPORT.md`.
