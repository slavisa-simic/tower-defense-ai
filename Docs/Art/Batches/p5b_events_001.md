# Phase 5B event presentation reuse

Reference contract: approved Danube master v002 and ART_HANDBOOK.md. This batch adds no generated raster art and replaces no existing approved asset. Existing sprite GUIDs/import settings and all gameplay prefab roots remain intact.

Consumers:
- `EndlessEventDefinition.uiIcon`: existing wave chevrons for assault/pattern/weather events; existing Auto Cannon icon for heavy/militant events; existing dismantle tools icon for Field Engineers; existing RSD paper bundle for Emergency Supply. Names and effects remain functional UI Toolkit text.
- `EndlessEventPresentation`: existing Square sprite stretched as one transparent fog quad (Simple draw mode, Effects order 18, alpha .24); existing shallow-water tile repeated over one affected inland column (GroundObjects order 1, alpha .64). Existing terrain sprites/data are unchanged. Overlays have no colliders, occupancy or navigation ownership.
- Pirate Raid reuses the existing PirateBossTransport prefab. No new faction markings or propaganda assets.
- Existing wave panel contains the small event row. During an event it replaces the ordinary status line so the northern battlefield remains visible. Existing speed, build, selection and Start Wave commands are retained.

Approval: within the user's explicit autonomous Phase 5B scope; no independent human art approval is claimed. No new image-generation prompt/source/revision exists because no bitmap production was necessary.

Validation: all thirteen scripted live event fixtures, Preparation/active captures under `Docs/Validation/Phase5B/`, forced runtime tests, saved-scene audit and Console inspection. Actual live captures are 1920x1080. No new art importer settings or prefab transforms were changed.
