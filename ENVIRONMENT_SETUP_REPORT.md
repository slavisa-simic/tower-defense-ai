# Environment setup report

Recorded 2026-09-16 for Phase 1, Step 1.

## Initial state

The repository contained only the user-provided `AGENTS.md`, with no commits, Unity project, README, or earlier environment setup report. The rules in `AGENTS.md` were preserved. No prior gameplay architecture existed to replace.

## Toolchain

- Windows; project targets Windows PC.
- Unity Editor **6000.3.24f1**, revision **4e7b9b5b6244** (6.3 LTS).
- Official Unity CLI **1.0.0-beta.9**.
- Official Unity Pipeline **0.7.0-exp.1**, used for live compilation, scene creation, testing, diagnostics, and screenshots. Gameplay code has no Pipeline dependency.
- Runtime Pipeline is explicitly disabled in `ProjectSettings/Packages/com.unity.pipeline/RuntimePipelineConfig.json`; the package reports this as a build advisory.
- Universal Render Pipeline **17.3.0** and Unity Test Framework **1.6.0**, matching the installed Editor's bundled versions.
- UI Toolkit with persisted UXML, theme, and PanelSettings.

The project was initialized from the installed Universal 3D template. Useful URP settings were retained. Unused sample content and optional template packages were removed. Transitive render/test dependencies remain recorded in `Packages/packages-lock.json`. No networking, navigation package, or gameplay framework was added.

## Resolved setup issues

1. Unity initially exited with code 198 because no Editor license was active. The user signed in and activated their license; subsequent Editor launches and tests succeeded.
2. The template pinned Input System 1.12.0, which failed to compile against this Editor (`BuildTarget.ReservedCFE`). Step 1 has no gameplay input; its passive UI uses the built-in input backend. The unused Input System package/action asset were removed, and the matching backend setting was applied. Unity can show an Input Manager deprecation advisory at startup; this does not affect the simulation.
3. The installed URP material-converter folder lacked `ReadonlyMaterialConverter.MaterialReferenceBuilder.cs`, `ReadonlyMaterialConverter.MaterialReferenceChanger.cs`, and their metadata. This caused a package compile error before gameplay could run. The four missing files were restored in the local Editor installation and project package cache from Unity's official `6000.3/staging` Graphics branch. The existing converter source was checked against that branch and matched exactly after line-ending normalization. No existing package source was overwritten, and no custom renderer workaround was added to the repository.

The source reference was [Unity Graphics commit 2d2e78cc](https://github.com/Unity-Technologies/Graphics/tree/2d2e78cc9d6254bc6e7c9c5552cea053508e86cb/Packages/com.unity.render-pipelines.universal/Editor/Tools/Converters/ReadonlyMaterialConverter). Unity staff discuss the expected converter files in [this installation-issue thread](https://discussions.unity.com/t/errors-when-creating-a-new-unity-6-3-urp-project-the-type-or-namespace-name-materialreferencechanger-could-not-be-found/1701487/28).

The repair was outside project source. A complete Unity installation should already contain these files. A different machine with the same incomplete installation would need the corresponding installation repair. Unity emitted an immutable-package modification warning during the local repair; subsequent gameplay validation had zero Console errors or warnings.

## Repository hygiene

Unity-generated Library, Temp, Logs, UserSettings, build outputs, and scratch test outputs are ignored. Source assets and their Unity-generated `.meta` files are retained. Validation summaries and screenshots live under `Docs/Validation`, outside runtime assets. No commit, push, or remote publication was performed.
