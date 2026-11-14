# Mapmakers of the Moon — Unity project

Open this folder in **Unity 6000.6.7f1** through Unity Hub. This is the folder with `Assets`, `Packages`, and `ProjectSettings`. Do not upgrade the editor or packages.

The simulation, data loaders, story data and automated tests exist. **The map, UI controls, scene flow and playable browser build are pending.** Pressing Play currently does not start the planned game.

## Work already specified

- Pure C# simulation: `Assets/_Game/Scripts/Sim/`.
- Pause/1x/2x/4x and Reset/Retry state execution: `Assets/_Game/Scripts/Game/GameSession.cs`. Views still need to connect to it. The session preserves the selected speed during Reset; screen behavior can explicitly set a speed when Opus connects the UI.
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

## Planned controls and demo path — not yet connected

The agreed controls are mouse placement/inspection, optional load toggles, crew reply buttons, Pause/1x/2x/4x and Reset/Retry. The agreed demo route is tutorial placement → lunar base/greenhouse → crew chat → storm recall/shelter/power choice → debrief and sunrise. Opus must connect these screens before this can become a verified three-minute click guide.

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
