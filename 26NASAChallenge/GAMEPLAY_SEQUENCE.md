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

Each level unlocks the next. Progress and best grades are saved (PlayerPrefs, works in WebGL).

| # | Level | Before | Play | After |
|---|---|---|---|---|
| 1 | The Dream (tutorial) | Bedroom, classroom | Draw Habitat, Solar Array, Greenhouse. Fix the power deficit with a second Solar Array. No clock | Teacher |
| 2 | The Commander | Graduation, launch, colony | Build all six base parts. Day 3: long shadow. First harvest unlocks the Shelter; the shadow unlocks a second Battery | — |
| 3 | Solar Storm | Colony, storm | Starts from the player's Level 2 base. Day 2: DONKI-based warning, Yuki outside. Days 3–4: particles. Day 5: sunrise | Window and final message |
| 4 | Mars Base | Launch | Same rules on red ground, counted in sols. Sol 3: dust storm. Sol 6: solar storm | Bedroom, Blueprint v2 |

## Free Build

No story. Choose Moon or Mars and Easy, Normal or Hard. Every run has a new seed:

- a new made-up map (sunlight pattern, craters, rocks);
- new days for dim days, solar storms, panel faults and supply landers.

The seed is shown (Map #1234). "Same map" replays it.

## One day (turn)

1. Look at the top bar, the goal card and the alerts. Alerts use tomorrow's forecast.
2. Build, switch systems on or off, fix, talk to the crew.
3. Press **Next Day** (amber when tomorrow looks risky).
4. Read the day summary and any event card.

## Grades

Score out of 100: crew safety 25, steady power 25, air/water/food 25, goals 25. The tutorial uses power balance 25, sunny solar spots 25 and goals 50.

| Letter | Score | Stars |
|---|---|---|
| S | 81–100 | 3 |
| A | 61–80 | 2 |
| B | 31–60 | 1 |
| C | 0–30 | 0 |

A failed mission is capped at 30 (C) and does not unlock the next level.

## Tests

- EditMode (55): simulation, data, day turns, events, grades, random worlds, level data, crew chat, bot balance on every level and on 36 Free Build seeds.
- PlayMode (3): Boot → Menu; tutorial cutscene → drawing → grade; a full Free Build run on Mars → grade.
