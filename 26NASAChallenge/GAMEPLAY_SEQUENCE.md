# Gameplay sequence

Open `Assets/_Game/Scenes/Boot.unity` and press Play. Rebuild the scenes any time with **Mapmakers → Build Scenes**.

## Scenes

| Scene | What it shows |
|---|---|
| Boot | Loads the menu |
| Menu | Title. **Story Mode** (level list), **Free Build** (setup), **Wiki & Help** (with lesson replays), **Data & Credits** |
| Cutscene | Full-screen story picture, one short line at a time. Click, Space or Enter continues. Skip leaves. Also hosts the guided **lessons** (`Help.json`) |
| Game | The map-first play screen. Same scene for every level and Free Build |
| Results | Grade letter, score out of 100, stars, why, and what to do next |

Story pictures never share the screen with the game UI.

## Story Mode

Each level unlocks the next. Progress, best grades, seen lessons and the best saved base are kept (PlayerPrefs, works in WebGL).

| Card | Level | Mission time | Events (game hours) | Goals |
|---|---|---|---|---|
| Tutorial 1 | The Dream | No clock | — | Draw Habitat, Solar Array, Greenhouse; power in beats power out |
| Tutorial 2 | Meet the Crew | Clock runs, no time limit, cannot fail | Long shadow and solar storm, started by the steps | Teacher's line and time-jump cards, then a hands-on practice base: 38 crew-guided steps covering every Level 3 action |
| Level 1 | The Commander | 96 h (about 3 min at 1x) | Landers 16, 34, 56; long shadow 64 | Core base, rover, first harvest, safe to the end |
| Level 2 | Solar Storm | 84 h (about 3 min) | Polar dusk 2–58; storm warning 16; lander 36; sunrise 60 | Call Yuki in early, use the Shelter, keep power in the storm, safe to the end |
| Level 3 | Mars Base (Jezero crater) | 120 h (about 4 min) | Landers 14, 32, 56, 84; dust storm 40–70; solar storm 78 | All of the above plus "no brownout in the dim days" |

Level 2 starts from the player's best Level 1 base (or a starter base).

## Onboarding and help

| Part | Required? | Notes |
|---|---|---|
| Meet the Crew (Tutorial 2) | Yes, to unlock Level 1 | Hands-on training (see below). Replay any time from the level list |
| Wiki & Help | Always available | Bottom-left button or **H** in every level; also in the pause menu and the briefing. Pauses the mission |
| Tooltips | Hover, or tap to pin | Resource chips, building cards, map buildings, crew, time controls |
| Placement suggestions | Optional (toggle) | Green glow = spots where every link is met (`Layout.Suggest`). Dark = outside the base. Red = blocked |
| Tips banner | Tips on/off in the menu | Yellow banner; "Next:" tips point at the next useful building |
| Onboarding pointer | Tutorial 1, Tutorial 2, the first Play in every timed level, and the first Habitat in Level 1 | Yellow arrow on the next card, the map or Finish (in Tutorial 2: on every step's button, chip, alert or building) |

The Teacher's text lesson was removed on Nov 15: the crew teaches its content in Tutorial 2. Only the teacher's one story line stays, as the first picture of the time-jump cutscene. Mars counts sols (24.66 h). Real minutes depend on pauses and speed.

## Tutorial 2: hands-on training

A practice base on a made-up map (seed 7, not the Level 3 Mars map). The three crew members take turns in a yellow card under the crew list; the yellow arrow points at what to press; the Objectives panel shows **TRAINING n/38**. Buildings unlock only when a crew member introduces them. Time starts stopped like every level; the player learns Play, Pause and speed first. There is no time limit and nothing fails. Steps come from `Help.json` (`training`); each step's check also accepts other ways of reaching the same state, so a player cannot get stuck.

| Guide | Steps |
|---|---|
| Priya | Say hi; **Play** starts time; **Pause**; the **speed** button; Habitat (cost: Supplies); Air Support next to it; Solar Array; Power at +; Battery near a Solar Array; **what ends a mission** (Air, Water or Food at 0; Habitat or Air Support without power for 10 h; an empty Battery alone is fine); the **low-resource ring and arrow** |
| Yuki | Say hi; Radio Dish; Rover Garage near soil |
| Mateo | Say hi; let time run; wait for 6 soil; Greenhouse next to Air Support; first harvest |
| Priya | Long shadow: select the Greenhouse, turn it off, wait on the Battery, turn it back on; second Battery |
| Yuki | Solar storm: Pause; call Yuki in (time stands still but orders work); build the Shelter; turn it on; Play; wait for the all clear (Crew safety too low for too long also ends a mission) |
| Priya, Yuki, Mateo | Open the Wiki; the time limit and inspection flag; grades S/A/B/C, running out of time is not failing; how long Level 1 lasts at 1× (filled in from `Levels.json`) |

At the end: **Training complete!** with **Start Level 1** or **Back to levels**.

## Real-time play

| Control | Effect |
|---|---|
| **Start mission** (briefing) | Closes the briefing. Time stays stopped; an arrow points at Play until the player presses it |
| Play/Pause button (top right), **Space** | Pause or resume |
| Speed button (top right) | Cycles 1× → 2× → 4× → 1×. 1× = 2 real seconds per game hour. Never pauses or resumes |
| Any window or menu | Holds the clock; closing it resumes |
| Window loses focus | Pauses; press Play to resume |

The sim always steps in fixed 0.05 h steps. A slow frame never makes it jump ahead.

The **Power** chip shows Solar output minus what every switched-on building needs, so it turns amber and negative whenever a building would go without power (before, it showed output minus the power actually delivered and could read + while Air Support was cut).

**Low resources:** when Air, Water or Food will run out within 12 game hours (yellow) or 4 hours (red), or the Battery is draining with under 12 hours left, or the Habitat/Air Support has no power, or Crew safety is below 70%, the chip gets a pulsing ring and a bouncing arrow with a label ("Air low!"). Calm motion keeps the ring still.

The top-right time group shows the clock, **Inspection in …** and a progress bar. The right column shows **Information** (alerts with actions, crew news, event messages) above **Objectives** (a checklist that ticks itself).

## Placement strategy

| Rule | Effect |
|---|---|
| Habitat first | All other buildings must be within 6 tiles of it (shaded area) |
| Solar → Battery within 2 tiles | Otherwise the long cable loses 30% (shown as "~") |
| Air Support → Habitat within 1 tile | Otherwise air refills at half speed |
| Greenhouse → Air Support within 1 tile | Air Support uses 1 kW less |
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

- EditMode (109): simulation, clock, end conditions, layout, data, events, grades, random worlds, crew chat, bot balance on every level and 36 Free Build seeds. `OnboardingTests` (11) check that every number the Wiki teaches matches `Balance.json`, lesson length, icons, keyword highlighting, suggestions, the Habitat power pop-up and the opening text. `TrainingTests` (6) check that the training teaches every building and action with valid speakers, teaches Play/Pause/speed before the first building and the mission-ending rules before Yuki, starts with time stopped, unlocks buildings in order, is untimed and cannot fail, and that a player who follows every step finishes it.
- PlayMode (10): Boot → Menu; tutorial; music, story Back and window open/close sounds; paused briefing → time waits for Play (arrow on Play) → idle failure → Retry; low Air gets the ring and arrow; every failure scene from a real trigger; evaluation → report; incomplete base → evaluation; Tutorial 1 → menu (no text lesson) → the whole Tutorial 2 training played through the real screen (the crew card and the arrow must show on every step) → Level 1; Wiki pause, speed cycle, suggestions and objective messages. The PlayMode tests save and restore the player's progress keys.
