# Documentation batch readme_showcase_001

Date: 2026-10-22. Scope: the user's README redesign request, including artwork-style section thumbnails, actual screenshots, architecture diagrams, phased Codex development history, art-handbook/custom-skill context, and GIFs.

Selection authority: the user authorized this documentation presentation. The agent selected the layout and retained capture takes within that scope. Existing runtime art approval states and master revision v002 remain unchanged.

References: `ART_HANDBOOK.md`, approved `Assets/Game/Art/MasterReferences/bd_ref_danube_master_v002.png`, existing HUD font/build icons, rubber boat and MG preview sprites, saved Phase 6A scenes, and historical Phase 5C/6A UI captures. New ImageGen prompts and generations: none. Documentation graphics use code-native SVG composition of existing pixels and glyphs; GIFs use actual Unity camera frames.

Outputs and exact reproduction commands: `Docs/Readme/MEDIA.md`. Filenames, dimensions, byte counts, source paths and SHA-256 values: `Docs/Readme/media-manifest.json`. SVG authoring source: `Docs/Readme/Tools/build_media.py`. Non-imported capture source: `Docs/Readme/Tools/CaptureShowcase.cs`.

Integration target: root `README.md` only. Unity import settings: not applicable; all outputs live outside Assets. No Unity asset GUIDs, prefabs, scenes, gameplay data, packages, or runtime source were changed.

Validation: SVGs and capture frames visually inspected; each GIF has 120 frames at 960×540 and 12 seconds; Mermaid diagrams and all images render in desktop/narrow previews; relative paths checked. Gameplay test results are referenced from the existing completed cleanup run rather than presented as a new README test run. The capture Editor reported zero errors and one PortEnvironment atlas-request warning, retained in the media notes. Play Mode was stopped and the agent-created headless Editor closed without saving authored scenes.
