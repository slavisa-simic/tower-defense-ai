# Border Defenders

A Unity 6 strict top-down pixel-art defense game set along a post-nuclear Danube quarantine line. Build defenses, repel infected river landings, and survive escalating waves on two maps.

This repository imports the existing **Phase 6A** project. Earlier reports describe historical milestones; they do not all reflect the current game.

## Run

1. Install **Unity 6000.3.24f1 (6.3 LTS)** with an activated license and open this repository as a Unity project.
2. Let Unity restore the packages listed in `Packages/manifest.json`.
3. Open `Assets/Game/Scenes/MainMenu.unity` and press **Play**.
4. Choose **NEW GAME**, select a map, and press **START**. Build defenses during Preparation, then use **START WAVE**.

The saved scenes, prefabs, data, sprites, animations, and UI are included. Normal play does not require scene generation or the Unity CLI. The build starts at MainMenu.

| Map | Scene | Layout |
| --- | --- | --- |
| Danube Border | `CoastalDefense_TopDown.unity` | Open river border with broad building space |
| Danube Industrial Port | `DanubeIndustrialPort.unity` | Docks, blocked container yards, split routes, and concrete building pockets |

`CoastalDefense_Prototype.unity` is the original isometric/XZ regression scene. Current gameplay uses XY: water to the left, the inland border to the right, and Z for presentation depth. Do not load gameplay scenes additively.

## Play

Start with **500 RSD**. Kill rewards and wave bonuses fund construction, repairs, and upgrades. Eight defenses share the same placement system:

| Defense | Initial cost (RSD) |
| --- | ---: |
| MG Nest | 150 |
| Barbed Wire | 50 |
| Land Mine | 75 |
| Sniper Tower | 300 |
| Mortar | 400 |
| Auto Cannon | 600 |
| Flamethrower | 300 |
| Rocket Launcher | 900 |

Select a defense and left-click a valid green preview to build. Red previews indicate invalid placement. Right-click cancels placement; click an existing structure to inspect its available management actions. Defenses support Level II/III upgrades, with repairs and dismantling available where applicable.

- **1–8:** select the corresponding defense button.
- **Escape:** cancel placement or selection, close a dialog, or open/resume the pause menu.
- **Space:** pause or resume.
- **Delete:** request dismantling of the selected structure, with confirmation and a displayed refund.

The HUD also provides normal/double speed controls. Endless waves include river transports, multiple enemy types, bosses, and events such as fog, flooding, and multi-sector landings. Border breaches increase the run's Homicides counter; reaching its configured threshold ends the run.

The menu provides Continue, map selection, settings, and run history. Saves record Preparation checkpoints; leaving during a wave returns to the last checkpoint. Continue restores the saved map. Local JSON data lives under `Application.persistentDataPath/BorderDefenders`.

## Project structure

- `Assets/Game/Scripts/`: grid/pathfinding, placement, economy, defenses, enemies, waves, persistence, presentation, and UI.
- `Assets/Game/Data/`: ScriptableObject definitions for maps, defenses, enemies, waves, and events.
- `Assets/Game/Scenes/`, `Prefabs/`, `Animations/`, and `UI/`: saved playable content and UI Toolkit assets.
- `Assets/Game/Art/`: runtime art, master references, and versioned source sheets.
- `Assets/Game/Audio/`: audio pipeline fixtures; create additional category folders when they receive assets. Production audio is not connected to gameplay.
- `Assets/Game/Tests/`: EditMode and PlayMode regression suites.
- `Docs/Art/`: asset register, batch provenance, and integration guidance.
- `Docs/Validation/`: historical test results, screenshots, authoring scripts, and build evidence.

`DefenseDefinition`, `BuildManager`, and `DefenseBase` own defense configuration, placement, and behavior. `EconomyManager` owns money; UI observes it. Build occupancy and navigation changes remain separate. Maps use `MapDefinition`, `MapCatalog`, and scene-owned `MapRuntimeContext`; shared combat, wave, and save services retain their existing responsibilities.

Tune the assets under `Assets/Game/Data/` and inspect their saved scene consumers. Scene rebuild helpers under **Tools > Shoreline Defense** replace authored layout or overrides; use them only for intentional scene authoring.

## Validation

Open **Window > General > Test Runner** and run the `ShorelineDefense.EditModeTests` and `ShorelineDefense.PlayModeTests` assemblies. With the Editor closed and the Unity CLI installed:

```powershell
unity test . --mode EditMode --output TestResults/editmode.xml
unity test . --mode PlayMode --output TestResults/playmode.xml
```

The recorded Phase 6A validation reports **122/122 EditMode** and **75/75 PlayMode** tests passed, plus a successful Windows release build. These are historical results from 2026-10-03; see [the Phase 6A report](PHASE_6A_REPORT.md) and [test summary](Docs/Validation/Phase6A/test-summary.json).

Fresh cleanup checks on **2026-10-22**, using the pinned Unity 6000.3.24f1 Editor, passed **122/122 EditMode** and **75/75 PlayMode** tests with no C# compiler errors or warnings. See [the repository cleanup report](Docs/Validation/ProjectCleanup/report.md) for scope and results.

If the Unity CLI cannot find an existing Editor installation, register its executable with `unity editors add "C:/path/to/Editor/Unity.exe"`.

Human comparative balance checks, earned-income midgame/boss victories, and sustained standalone profiling remain limits documented in the Phase 6A report.

## Contribution and production rules

Read [AGENTS.md](AGENTS.md) before changing the project. Art work also follows [ART_PRODUCTION_RULES.md](ART_PRODUCTION_RULES.md), [Unity integration guidance](Docs/Art/UNITY_INTEGRATION.md), and the [asset register](Docs/Art/ASSET_REGISTER.md). Preserve Unity GUIDs and existing gameplay/presentation ownership. Audio provenance and current fixture status are recorded in [AUDIO_LICENSES.md](AUDIO_LICENSES.md).

The repository license is in [LICENSE](LICENSE).
