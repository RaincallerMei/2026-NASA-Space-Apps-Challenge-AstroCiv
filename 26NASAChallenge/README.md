# Mapmakers of the Moon — Unity project

Open this folder in **Unity 6000.6.7f1** through Unity Hub. This is the folder with `Assets`, `Packages`, and `ProjectSettings`. Do not upgrade the editor or packages.

## Play

Open `Assets/_Game/Scenes/Boot.unity` and press Play. The menu offers **Story Mode** (4 levels, each unlocks the next), **Free Build** (random map and events) and **Data & Credits**. Story pictures play in their own Cutscene scene. Play is turn based: build, then press **Next Day**. Each level ends on a Results screen with a grade (S/A/B/C), a score out of 100 and stars. Full flow: `GAMEPLAY_SEQUENCE.md`.

Controls: click a building card, then click the map. Right-click or Esc cancels. Click a building to inspect it, turn it off or on, fix it or remove it. Click a crew member to talk. Enter = Next Day. Esc with nothing selected = pause menu.

Rebuild the five scenes and the build order with **Mapmakers → Build Scenes**.

## Code map

| Folder | Contents |
|---|---|
| `Assets/_Game/Scripts/Sim/` | Pure C# simulation: day turns, event schedule (`Scenario.cs`), grades (`Scoring.cs`), random maps and events (`RandomWorld.cs`), bots |
| `Assets/_Game/Scripts/Data/` | Loaders for balance, story, fixtures and levels (`LevelData.cs`) |
| `Assets/_Game/Scripts/Game/` | Scene flow and progress (`Flow.cs`), one level run (`MissionRun.cs`), crew chat choice (`CrewChat.cs`) |
| `Assets/_Game/Scripts/UI/` | Screens built in code: Menu, Cutscene, Game (map, windows), Results; `UIKit.cs` holds the palette and placeholder pixel icons |
| `Assets/_Game/Data/Resources/` | `Balance.json`, `Story.json`, `Levels.json`, six offline fixtures |
| `Assets/_Game/Art/Resources/Cutscenes/` | Seven AI-generated story pictures (watermarked in game) |
| `Assets/_Game/Editor/` | Scene builder and cutscene import settings |

## Run tests and regenerate technical reports

With this editor open and the Unity CLI installed, run from this folder:

```powershell
unity command test run --mode editor --filter Mapmakers --async-tests true
unity command test run --mode playmode --async-tests
unity command test status --detail failures
python tools/export_assumptions.py
```

Wait for compilation to finish before starting tests. Alternatively use Unity's Test Runner, EditMode, `Mapmakers.Tests`. Reports describe automated simulation outcomes, not player testing.

Refresh source data only if requested: `python tools/fetch_data.py`. It reads NASA_API_KEY from the environment or uses DEMO_KEY; do not put a personal key in project files. Existing offline fixtures are already present and should not be refreshed just to run the game/tests.

## Handoff documents

| File | Purpose |
|---|---|
| `BALANCE_REPORT.md` | Generated existing-bot outcomes and unchanged balance values |
| `TEST_PLAN.md` | Automated checks, manual script, blank tester table and build checks |
| `PLAYER_LEARNING.md` | Existing learning and crew-chat checks |
| `SCIENCE_SOURCES.md` / `SCIENCE_ASSUMPTIONS.csv` | Verified primary sources and data/config classification |
| `SCENARIOS.md` | Existing scenario calculations and recorded bot outcomes |
| `DATA_SOURCES.md` | Fetch history, metadata, units and source terms |
| `ASSET_CREDITS.md` | Current asset inventory; final art entries pending |
| `SUBMISSION_NOTES.md` | Factual draft text and remaining submission inputs |

The checked-in CLAUDE.md and RESEARCH_AGENT_BRIEF.md are copies of the repository-root briefs. If the team edits the briefs, keep those copies in sync. The repository-root master prompt remains the controlling implementation plan.
