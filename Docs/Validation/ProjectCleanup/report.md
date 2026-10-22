# Repository cleanup — 2026-10-22

Removed 22 empty Unity folders: 18 reserved audio category folders, Audio/Definitions, Audio/Mixers, and the empty Settings/Pipeline/Resources and Settings/Pipeline hierarchy. Their folder metadata had no consumers in the remaining project assets or code. Audio categories are now created when actual assets need them; the existing Environment test clips retain their paths and GUIDs.

Removed 11 redundant `.gitkeep` files from populated folders and the ignored root command-discovery capture. Removed the unused private `BoatLandingSystem.nextPoint` field, which the Unity C# compiler identified with CS0169 when that warning was enabled. No unused top-level gameplay class was established by the reference review.

The prototype spawner, prototype HUD, Phase 2 scene builders, and MG compatibility components remain referenced by saved content, authoring tools, or regression tests. Source sheets, approved references, historical QA records, and the separate legacy scene remain available.

The first full PlayMode run passed 74/75 tests. The settings integration test reproduced a pre-existing first-run problem at the batch Editor's 640×480 Game View size: SettingsService accepted that size as a default while its own save validator requires at least 800×600. Apply then rejected all setting changes. SettingsService now validates display-derived defaults and falls back to the existing 1920×1080 defaults when needed. Valid saved settings retain precedence. The integration test now checks successful UI save feedback before checking the applied settings.

Unity 6000.3.24f1 was found in an existing installation outside the CLI registry and registered with `unity editors add`. The README's earlier claim that this Editor was absent was corrected.

Validation completed with Unity 6000.3.24f1:

- Focused settings regression: failed with `Invalid display settings.; resolution=640 x 480` before the fix, passed afterward.
- Final EditMode: 122/122 passed, zero failed/skipped.
- Final PlayMode: 75/75 passed, zero failed/skipped.
- Unity compilation and a direct compilation of the runtime assembly with the installed Unity compiler: no C# errors or warnings. Unity batch logs were reviewed; the EditMode observer-failure exception is explicitly expected by its test.
- Metadata audit: 807 remaining `.meta` files; no duplicate GUIDs, missing metadata, orphan metadata, unresolved references to removed folder GUIDs, or unrepresented Unity folders. Project JSON/assembly definitions parse and all four build scenes exist.
- Diff review: no saved scene, prefab, gameplay data, package, or ProjectSettings changes. `git diff --check` passed.

Actual counts and durations are in [test-summary.json](test-summary.json). Full local NUnit reports and compiler response files are under the ignored `TestResults/Cleanup/` folder. The final suites were run with:

```powershell
unity test . --editor-version 6000.3.24f1 --mode EditMode --output TestResults/Cleanup/editmode-final.xml
unity test . --editor-version 6000.3.24f1 --mode PlayMode --output TestResults/Cleanup/playmode-final.xml
```

A standalone build and manual visual walkthrough were not repeated for this cleanup.
