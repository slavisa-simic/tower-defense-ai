# Asset pipeline setup report

Date: 2026-09-18, Europe/Sarajevo. Scope: preparation for Phase 3A-1 only.

## Status

**READY for Phase 3A-1 master-reference generation when requested.** Unity compilation and all 64 existing tests passed. No image-generation call was made, no production image or animation was created, and no approved master reference exists yet. Mass production follows its recorded selection and a validated pilot.

## Environment inspected

- Read repository rules, README, phase/environment reports, project version/dependencies, presentation/facing/build-HUD code, data and prefab import conventions, saved scene/build configuration, and existing tests.
- The checkout already contains substantial staged/intent-to-add and untracked prototype work. It was not committed, reset, staged, or cleaned. A pre-change SHA-256 snapshot of Assets, Packages and ProjectSettings was captured for this setup's final comparison.
- Project Editor: **6000.3.24f1**, installed at `C:/Users/Slavisa/AppData/Local/Programs/Unity/Hub/Editor/6000.3.24f1/Editor/Unity.exe`. Another installed Editor, 6000.6.0f1, was not used. The project version was not upgraded.
- Unity CLI: **1.0.0-beta.10**, verified from the executable, newer than the historical environment report. The Editor was initially closed, then opened and connected through the existing **Unity Pipeline 0.7.0-exp.1**. No Unity MCP tools were exposed directly to this task; verified CLI/Pipeline commands provide the integration route.
- Existing URP **17.3.0**, UI Toolkit and Test Framework **1.6.0** remain. The primary scene is strict top-down XY with separate visual children, local-+X facing and an existing URP 2D renderer. Gameplay remains owned by the prototype's existing systems.
- Built-in `image_gen` and local `view_image` are available. Availability was verified without generating a test image or requiring API credentials. A successful generation/alpha-quality check is deliberately deferred to the requested production phase.
- `aseprite` and `magick` were not found on PATH; that is not proof of absence elsewhere. Neither is required for the initial PNG, Unity slicing and frame-animation workflow. No external art-editor purchase, plugin connection or CLI/API generation dependency was introduced.

## Installed and created skills

| Skill | Action and location | Role |
|---|---|---|
| `unity-cli` | Installed the official offline skill bundled with Unity CLI using `unity skill install codex`; `C:/Users/Slavisa/.agents/skills/unity-cli/` | Editor discovery/control, imports, package commands, compilation and tests |
| `unity-pipeline` | Copied the official skill shipped inside the existing Pipeline package to `.agents/skills/unity-pipeline/SKILL.md` | Portable project guidance for live compile/test/control operations; original content preserved |
| `game-art-pipeline-engineering` | Created `.agents/skills/game-art-pipeline-engineering/SKILL.md` with `agents/openai.yaml`; matching user copy at `C:/Users/Slavisa/.codex/skills/game-art-pipeline-engineering/` | Reference-first batch production, organization, sprites/animation/UI integration, preservation and records |
| `game-software-engineering` | Existing `C:/Users/Slavisa/.codex/skills/game-software-engineering/SKILL.md`, read and verified | Good general Unity/C# engineering discipline, debugging and EditMode/PlayMode validation |
| `imagegen` | Existing system skill, read and verified | Built-in reference-guided image creation/editing and transparency workflow |
| `skill-creator` | Existing system skill, used to create/validate the new skill | Frontmatter and instruction validation |
| `openai-docs`, `plugin-management` | Existing skills read for capability/setup inventory | Official setup information and preference for already-available capabilities |

A good engineering skill already existed. The custom skill was nevertheless created to fill the missing **art-specific** orchestration, naming, reference reuse, sprite-sheet and UI workflow; it supplements the existing engineering skill rather than replacing it. Debugging/testing needs are covered by the engineering + Unity skills and existing tests, so no duplicate debugging skill was installed.

The project copy is the durable source for its custom skill. Keep its user copy synchronized when deliberately updating it. The official Pipeline skill was sourced from the resolved `com.unity.pipeline` package's `.claude/skills/unity-pipeline/SKILL.md`; refresh that project copy when upgrading the package. New skill files were read and validated in this task; picker refresh behavior is not a prerequisite for using the explicit paths in AGENTS.md.

## Available skill inventory

The initial session catalog exposed these skills (provider prefixes retained where useful). Availability does not mean every external account is authenticated or suitable for this game's art.

- System: `imagegen`, `openai-docs`, `plugin-creator`, `skill-creator`, `skill-installer`.
- Project/personal engineering and design: `game-software-engineering`, `ui-ux-pro-max`, `ckm:banner-design`, `ckm:brand`, `ckm:design`, `ckm:design-system`, `ckm:ui-styling`.
- Adobe: `adobe-batch-edit-photos`, `adobe-create-mockups`, `adobe-create-social-variations`, `adobe-design-from-template`, `adobe-edit-quick-cut`, `adobe-retouch-portraits` (provider `app-69312da8e4dc81919370cb86fd172b6c`).
- Canva: `canva-brand-check`, `canva-branded-presentation`, `canva-bulk-create`, `canva-design-feedback`, `canva-edit-design`, `canva-implement-feedback`, `canva-resize-for-social-media`, `canva-translate-design`.
- Documents/workspace: `computer-use:computer-use`, `documents:documents`, `pdf:pdf`, `presentations:Presentations`, `spreadsheets:Spreadsheets`, `spreadsheets:excel-live-control`, `template-creator:template-creator`, `visualize:visualize`.
- Services: `plugin-management:plugin-management`, `sites:sites-building`, `sites:sites-hosting`, `sites:sites-preview-troubleshooting`.
- Added by this setup: `unity-cli`, `unity-pipeline`, `game-art-pipeline-engineering`.

Disk inspection additionally found a system `review-agent`, cached GitHub/Stripe skills and artifact-template skills not listed in this task's initial skill catalog. Cached files alone were not treated as active skills. No dedicated sprite/animation production skill was found in the initial catalog; the new custom skill and integration guide cover that gap.

## Plugin inventory and decision

The inspected Codex configuration marks these plugins enabled: Stripe, GitHub, Documents, Spreadsheets, Presentations, PDF, Template Creator, Visualize, Computer Use, Browser, Codex App Tools, and Unified Computer Use. Their cache files were inspected separately from tool availability.

This task also exposes Canva, GitHub, Plugin Management, Sites, Spreadsheets and Codex App Tools connector capabilities, plus computer-use/browser surfaces and Adobe/Canva skills. Presence of a skill or connector is not a claim that an external account was tested. The user's recommended-plugin list identifies installable suggestions, not installed plugins.

No external Codex plugin was installed: built-in image generation and the locally installed Unity CLI/Pipeline satisfy the task. No social/template design service was invoked for pixel sprites. Unity skill installation and Unity package additions are distinct from external-plugin installation.

## Unity authoring dependencies added

Both additions were made through the running Editor's `package_add`, with terminal completion verified and package lock files resolved by Unity:

- **`com.unity.2d.sprite` 1.0.0**: the Editor-bundled Sprite Editor for exact slicing, pivots and sprite borders. This was missing from the project.
- **`com.unity.modules.animation` 1.0.0**: the Editor-bundled animation module for the planned AnimationClip/Animator workflow.

No gameplay framework, replacement renderer, skeletal 2D Animation package, Aseprite importer, Pixel Perfect package or global art importer was added. Built-in Sprite Editor and frame animation cover the initial requested workflow. Unity's [Sprite Editor setup documentation](https://docs.unity3d.com/6000.3/Documentation/Manual/sprite/renderer/install-2d-sprite-package.html) explains the authoring dependency; the exact versions above were verified locally.

## Durable files and folders

- Updated `AGENTS.md`: Danube terminology, art constraints, skill/document entry points and preservation rules.
- Created `ART_PRODUCTION_RULES.md`: perspective/style, master-first sequence, source/runtime ownership, names/revisions, transparency and approval records.
- Created this `ASSET_PIPELINE_SETUP_REPORT.md`.
- Created `Docs/Art/UNITY_INTEGRATION.md`: inspected architecture, proposed density, import settings, sheet/animation contract, UI and validation procedure.
- Created `Docs/Art/PHASE_3A_1_BRIEF.md`: prepared master-reference deliverable and review criteria, with no generation.
- Created `Docs/Art/ASSET_REGISTER.md`, `Docs/Art/BATCH_TEMPLATE.md`, and `Docs/Art/Batches/`.
- Created project skill files listed above and setup evidence in `Docs/Validation/ArtPipelineSetup/`.
- Created `Assets/Game/Art/{MasterReferences,Environment,Enemies,Defenses,Effects,UI,Props,Base,SourceSheets}/` and `Assets/Game/Animations/`, with Git keep files and Unity-created folder metadata.
- Verified and retained `Assets/Game/Art/Placeholders/`, `Assets/Game/Prefabs/`, `Assets/Game/UI/`, and `Assets/Game/Data/`; no duplicate hierarchy.
- Unity updated `Packages/manifest.json` and `Packages/packages-lock.json` for the two focused authoring dependencies.

An ignored temporary virtual environment under `.utmp/art-pipeline-setup/` holds PyYAML 6.0.3 solely for the bundled skill validator; neither available Python runtime initially contained it. This is not a project runtime dependency. Temporary inspection scripts and editor logs also remain ignored there.

## Validation

| Check | Actual result | Evidence |
|---|---|---|
| Skill validation | Official CLI, project Pipeline, and both custom-skill copies pass the bundled validator | `Docs/Validation/ArtPipelineSetup/skills-validation.json` |
| Unity compilation | Completed; no compilation failure or compiler errors | `Docs/Validation/ArtPipelineSetup/compile-status.json` |
| EditMode regression suite | 42 passed, 0 failed, 0 skipped | `Docs/Validation/ArtPipelineSetup/editmode-results.json` |
| PlayMode regression suite | 22 passed, 0 failed, 0 skipped | `Docs/Validation/ArtPipelineSetup/playmode-results.json` |
| Live importer readiness | All 13 required category/consumer folders valid with GUIDs; Sprite Editor assembly loaded; AnimationClip/Animator types available; both new packages registered at 1.0.0 | `Docs/Validation/ArtPipelineSetup/unity-import-readiness.json` |
| Existing-file preservation | 264 existing files checked; only Packages/manifest.json and Packages/packages-lock.json changed; no missing files or other changes | `Docs/Validation/ArtPipelineSetup/repository-integrity.json` |
| Art inventory | Only the two pre-existing Square/Triangle placeholder PNGs; no generated production images | Same repository-integrity evidence |
| Diff hygiene | `git diff --check` passed; Git reported only a pre-existing CRLF-normalization advisory for EnemySpawner.cs, whose bytes match the baseline | Terminal check and integrity evidence |
| Editor/Console inspection | No current Console errors/warnings or failed compilation after validation; diagnostic history explained below | `Docs/Validation/ArtPipelineSetup/console.json`, `editor-status.json` |

The Console history retains the previously documented Input Manager deprecation warning and an **expected** `InvalidOperationException: Expected observer failure` from `BrokenObserverCannotTurnCommittedPaymentIntoFailure`. The test explicitly expects that exception and passed; it is not an art-pipeline regression. No input-system migration or unrelated repair was performed. The first test-dispatch attempt crossed the animation-package domain reload and lost its connection; after compilation completed and test status confirmed no run, the suite was dispatched successfully. Only completed test results are counted above.

The final live inspection found Play Mode stopped. No player build or new visual/animation playback review was performed: this step adds workflow/folders and authoring dependencies, and intentionally has no new visual assets to review. Pixel fidelity, generated alpha, sheet continuity and in-game art readability remain acceptance checks for the later production batches. The existing PlayMode suite did exercise gameplay after the package additions.

## Phase 3A-1 readiness

**No real blocker remains for beginning Phase 3A-1.** The master-reference brief, generation capability, import/animation tools, folder ownership, naming, batch records and preservation workflow are prepared. No approved reference is expected at setup: producing and selecting it is the first Phase 3A-1 task. The proposed 32-PPU baseline and exact palette remain subject to that reference/pilot review. Unity integration of generated alpha, sheets and animation is tested against real outputs during their authorized batch, not fabricated during preparation.

Official skill format reference: [Build skills](https://learn.chatgpt.com/docs/build-skills). Local installed skill instructions and validator were used for this setup.
