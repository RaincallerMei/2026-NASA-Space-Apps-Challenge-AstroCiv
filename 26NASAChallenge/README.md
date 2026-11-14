# Mapmakers of the Moon — Unity project

Open this folder in **Unity 6000.6.7f1** through Unity Hub. This is the folder with `Assets`, `Packages`, and `ProjectSettings`. Do not upgrade the editor or packages.

The simulation now has a functional greybox Main scene with a clickable map, resource display, placement/inspection, pause/speed/retry controls, storm actions and a debrief. The story tutorial, crew-chat screen, final artwork and browser build remain pending.

Open `Assets/_Game/Scenes/Main.unity` and press Play. Pick a building on the right, then click the map; choose Inspect to select existing buildings. Use the selected-system button for optional loads. Recall Yuki while the radio is powered, place the unlocked Shelter and switch it on, and manage optional demand through the storm. Reset / Retry starts over. Existing balance values are unchanged.

## Work already specified

- Pure C# simulation: `Assets/_Game/Scripts/Sim/`.
- Pause/1x/2x/4x and Reset/Retry: `Assets/_Game/Scripts/Game/GameSession.cs`, connected by `MissionController.cs`. `MissionView.cs` is the replaceable greybox uGUI view; Reset creates a fresh session at 1x.
- Runtime data loaders: `Assets/_Game/Scripts/Data/`.
- Balance settings: `Assets/_Game/Data/Resources/Balance.json`; no tuning was performed in this pass.
- Story and crew reply text: `Assets/_Game/Data/Resources/Story.json`.
- Six offline source files: `Assets/_Game/Data/Resources/Fixtures/`. Runtime reads local Resources, with no NASA key or live API calls required by the data loaders.

## Run tests and regenerate technical reports

With this editor open and the Unity CLI installed, run from this folder:

```powershell
unity command test run --mode editor --filter Mapmakers --async-tests true
unity command test status --detail failures
unity command script run --file tools/export_balance.cs
python tools/export_assumptions.py
```

Wait for compilation to finish before starting tests. Alternatively use Unity's Test Runner, EditMode, `Mapmakers.Tests`. Reports describe automated simulation outcomes, not player testing.

Refresh source data only if requested: `python tools/fetch_data.py`. It reads NASA_API_KEY from the environment or uses DEMO_KEY; do not put a personal key in project files. Existing offline fixtures are already present and should not be refreshed just to run the game/tests.

## Scope of the playable greybox

Map placement/inspection, optional load toggles, Pause/1x/2x/4x, recall and Retry are connected. Main starts directly at the outpost; the existing simulation advances through shadow, harvest, storm and sunrise to a text debrief. The full story route, crew reply screen and illustrated ending remain for Opus. This map uses plain uGUI cells, not final Tilemap artwork. The original untitled editor scene was preserved as WorkspaceSnapshot.unity and is excluded from the build scene list.

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
