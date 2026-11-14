# AstroCiv — Mapmakers of the Moon

A Unity 2D educational lunar-base game in development for the 2026 NASA Space Apps Challenge. Players build an outpost, manage power and life support, and protect a fictional crew during a solar-particle alert.

## Open the Unity project

1. Clone this repository.
2. In Unity Hub, select **Add project from disk** and choose **`26NASAChallenge/`**, the folder containing `Assets`, `Packages`, and `ProjectSettings`.
3. Open with **Unity 6000.6.7f1**. Keep the existing package versions.

## Current progress

The simulation, building-placement rules, offline data loaders and story text are implemented. A functional greybox Main scene now connects the map, controls, resource display, storm actions and text debrief. The tutorial and chapter sequence are now connected using the existing story text. Crew-chat visuals, final artwork and WebGL remain in development. Open `26NASAChallenge/Assets/_Game/Scenes/Boot.unity` and press Play; see `26NASAChallenge/GAMEPLAY_SEQUENCE.md`.

On November 14, 2026, the initial combined run passed **55 tests**: 53 EditMode and two real-scene PlayMode tests. These include the full winning outpost/storm/debrief route and failure/Retry. Final verification evidence is saved under `review/runtime-tests.json` after UI fixes.

The tutorial/campaign update passes **57 tests**: 53 EditMode and four PlayMode tests, including Boot through Blueprint v2. Evidence: `review/campaign-tests.json`. Story scenes currently use labelled text/greybox placeholders.

To rerun them, open Unity's Test Runner and run the EditMode tests in `Mapmakers.Tests`. Full player testing and a WebGL build remain pending.

## Repository guide

| Location | Contents |
|---|---|
| `26NASAChallenge/Assets/_Game/Scripts/Sim/` | Pure C# simulation and scripted players |
| `26NASAChallenge/Assets/_Game/Scripts/Data/` | Data and story loaders |
| `26NASAChallenge/Assets/_Game/Data/Resources/` | Editable balance, story, and six offline fixtures |
| `26NASAChallenge/Assets/_Game/Tests/EditMode/` | Automated tests |
| `26NASAChallenge/tools/` | Optional data refresh script and fetch log |
| `26NASAChallenge/DATA_SOURCES.md` | Data provenance and unresolved research checks |
| `26NASAChallenge/README.md` | Unity development commands and current technical handoff |
| `26NASAChallenge/BALANCE_REPORT.md` | Generated existing-bot results and unchanged settings |
| `26NASAChallenge/TEST_PLAN.md` | Manual testing script and blank tester record |
| `CLAUDE.md` | Game design and implementation brief |
| `CLAUDE_CODE_MASTER_PROMPT.md` | Build requirements and milestones |
| `RESEARCH_AGENT_BRIEF.md` | Science and research requirements |
| `review/PROGRESS_REVIEW.md` | Progress audit and resumption checklist |

The game is designed to read saved fixtures without network access. Optional data refresh uses `NASA_API_KEY` from the environment, or `DEMO_KEY` if unset. Never commit a personal API key.

Building power, costs, recycling, and safety values are fictional gameplay settings. Source-derived data is labeled in the fixtures. NASA and CSA do not endorse this project. Remaining science and reuse checks are listed in `DATA_SOURCES.md`.
