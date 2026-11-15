# Gameplay sequence

Open `Assets/_Game/Scenes/Boot.unity` and press Play. Rebuild the scenes any time with **Mapmakers → Build Scenes**.

## Scenes

| Scene | What it shows |
|---|---|
| Boot | Loads the menu |
| Menu | Title. **Story Mode** (level list), **Free Build** (setup), **Data & Credits** |
| Cutscene | Full-screen story picture, one short line at a time. Click, Space or Enter continues. Skip leaves |
| Game | The map-first play screen. Same scene for every level and Free Build |
| Results | Grade letter, score out of 100, stars, why, and what to do next |

Story pictures never share the screen with the game UI.

## Story Mode

Each level unlocks the next. Progress, best grades and the best saved base are kept (PlayerPrefs, works in WebGL).

| # | Level | Mission time | Events (game hours) | Goals |
|---|---|---|---|---|
| 1 | The Dream (tutorial) | No clock | — | Draw Habitat, Solar Array, Greenhouse; power in beats power out |
| 2 | The Commander | 96 h (about 3 min at 1x) | Landers 16, 34, 56; long shadow 64 | Core base, rover, first harvest, safe to the end |
| 3 | Solar Storm | 84 h (about 3 min) | Polar dusk 2–58; storm warning 16; lander 36; sunrise 60 | Call Yuki in early, use the Shelter, keep power in the storm, safe to the end |
| 4 | Mars Base (Jezero crater) | 120 h (about 4 min) | Landers 14, 32, 56, 84; dust storm 40–70; solar storm 78 | All of the above plus "no brownout in the dim days" |

Level 3 starts from the player's best Level 2 base (or a starter base). Mars counts sols (24.66 h). Real minutes depend on pauses and speed.

## Real-time play

| Control | Effect |
|---|---|
| **Start mission** (briefing) | The clock waits until this is pressed |
| Play/Pause button, **Space** | Pause or resume |
| **1x / 2x / 4x** | 1x = 2 real seconds per game hour |
| Any window or menu | Holds the clock; closing it resumes |
| Window loses focus | Pauses; press Play to resume |

The sim always steps in fixed 0.05 h steps. A slow frame never makes it jump ahead.

The bottom bar shows **Evaluation in …**. Alerts give hours left and an action ("Water runs out in 6 h. Rest the Greenhouse").

## Placement strategy

| Rule | Effect |
|---|---|
| Habitat first | All other buildings must be within 6 tiles of it (shaded area) |
| Solar → Battery within 2 tiles | Otherwise the long cable loses 30% (shown as "~") |
| Life Support → Habitat within 1 tile | Otherwise air refills at half speed |
| Greenhouse → Life Support within 1 tile | Life Support uses 1 kW less |
| Shelter → Habitat within 2 tiles | Otherwise only half protection |
| Rover Garage near soil/ice | Trips take 1 h + 0.5 h per tile; brings soil (Greenhouse needs 6), then water |
| Sunny tiles | Solar output follows the real sunlight map |

## How a mission ends

| Ending | When | Screen |
|---|---|---|
| **Evaluation** | The mission clock reaches its end | Inspection sweep, blueprint checklist, grade |
| **Mission stopped** | Air, water or food runs out; critical power off too long (10 h); crew in particles outside or without a powered Shelter | Short calm scene per cause, then cause, telemetry, "Next time" fix. Try again / View report / Menu |

If a failure and the deadline happen on the same step, the failure counts. Nothing changes after the end.

## Grades

| Row | Points |
|---|---|
| Base and goals | 30 |
| Smart layout (sunny solar, links met) | 20 |
| Crew safety | 20 |
| Air, water and food left | 15 |
| Steady power | 15 |

| Cap | Max score |
|---|---|
| Mission stopped | 30 (C) |
| A core building missing | 60 (B), cannot continue |
| A goal missing | 80 (A) |

| Letter | Score | Stars |
|---|---|---|
| S | 81–100 | 3 |
| A | 61–80 | 2 |
| B | 31–60 | 1 |
| C | 0–30 | 0 |

## Free Build

No story. Moon or Mars; Easy, Normal or Hard; 144 h. Each seed makes a new map (sunlight, craters with ice, soil patches) and a new event schedule. The seed is shown (Map #1234).

## Tests

- EditMode (89): simulation, clock, end conditions, layout, data, events, grades, random worlds, crew chat, bot balance on every level and 36 Free Build seeds.
- PlayMode (6): Boot → Menu; tutorial; paused briefing → idle failure → Retry; every failure scene from a real trigger; evaluation → report; incomplete base → evaluation.
