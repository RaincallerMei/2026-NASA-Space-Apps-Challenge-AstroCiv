# Balance report

## Current: pausable real time with strategic layout (14 November 2026)

Source: `BalanceTests` run in Unity 6000.6.7f1 on the shipped `Balance.json` and `Levels.json` (all 5 tests passed). Bots act every 2 game hours. All numbers are **fictional balance**, not equipment specs or dose forecasts.

**Main changes:** grade rows 30/20/20/15/15 (Steady power 25 → 15; new Smart layout row 20). Caps: stopped 30, core building missing 60, goal missing 80. Layout links (cable, duct, plant loop, shelter distance), base area (6 tiles from the Habitat), Rover Garage and soil cost for the Greenhouse. Battery 12 kWh. Crew drinks water. Critical power off 10 h stops the mission. Level 3 has a long polar dusk (sun ×0.84 for 56 h). Level 4 is Mars at Jezero.

| Level | Bot (strategy) | Score | Grade | End | Notes |
|---|---|---|---|---|---|
| L2 Commander | Smart (good layout) | 92 | S | Evaluation | 0 brownouts |
| L2 Commander | Careless (builds next to the Habitat) | 74 | A | Evaluation | 9 brownouts, links missed |
| L2 Commander | NoRadio (no Communications) | 60 | B | Evaluation | Capped: core building missing |
| L2 Commander | Idle | 6 | C | NoAir at 36 h | About 72 s at 1x before air runs out |
| L3 Solar Storm | StoredEnergy (second Battery) | 99 | S | Evaluation | Winning strategy 1 |
| L3 Solar Storm | ShedLoads (rests the Greenhouse) | 97 | S | Evaluation | Winning strategy 2 |
| L3 Solar Storm | Greedy (all loads on, one Battery) | 80 | A | Evaluation | 1 storm brownout; cannot reach S |
| L3 Solar Storm | DoNothing | 30 | C | RadiationNoShelter at 34 h | |
| L4 Mars | Smart | 97 | S | Evaluation | |
| L4 Mars | ShedLoads | 94 | S | Evaluation | |
| L4 Mars | Greedy | 80 | A | Evaluation | 9 brownouts in the dust storm; "dim_power" goal missed |
| L4 Mars | Idle | 6 | C | NoAir at 36 h | |

Free Build (Smart bot, 144 h, seeds 1–12, Moon on odd seeds, Mars on even):

| Difficulty | Survived | Grades |
|---|---|---|
| Easy | 12/12 | 11 S, 1 A |
| Normal | 12/12 | 10 S, 2 A |
| Hard | 8/12 | Seeds 1, 5 PowerLoss; 3 NoFood; 9 RadiationNoShelter |

Simplifications to review: Mars sunlight is 0.6 of the Moon's in game (real 0.43, softened for play); Mars has no night cycle; MOLA height is mapped to sun (higher = brighter) as a gameplay assumption; Jezero wet-clay and Moon soil tiles are hand-picked.

## Historical: day turns (14 November 2026, earlier redesign)

Source: `BalanceTests` on the shipped `Balance.json` and `Levels.json`. Bots act once per day, like a player pressing Next Day. All numbers are **fictional balance**, not equipment specs or dose forecasts.

**What changed for day turns:** Battery 4 → 30 kWh (limit 3 → 5 kW). Storm: warning at the start of the day before, particles for a full day, fading over 12 h (onset 24 h, decay 48 h, end 60 h after the warning). Long shadow lasts one day (×0.7 sun). Greenhouse recycles 95% of its water (was 80%). Sunrise climbs over 12 h. Repair costs 5 supplies. A base carried into Level 3 starts with full batteries.

| Level | Bot | Score | Grade | Passed | Notes |
|---|---|---|---|---|---|
| L2 Commander | ShedLoads | 98 | S | yes | 0 brownout days |
| L2 Commander | Idle | 30 | C | no | Air runs out on day 2 (after 20+ game hours) |
| L3 Solar Storm | ShedLoads | 95 | S | yes | Rests the Greenhouse in the storm |
| L3 Solar Storm | StoredEnergy | 92 | S | yes | Second Battery, no storm brownout |
| L3 Solar Storm | Greedy | 86 | S | yes | One storm brownout; scores lowest of the winners |
| L3 Solar Storm | DoNothing | 30 | C | no | No Shelter: RadiationNoShelter |
| L4 Mars | ShedLoads | 92 | S | yes | |
| L4 Mars | StoredEnergy | 86 | S | yes | 2 storm brownouts |
| Free Build ×36 | Smart (uses the forecast) | 89–96 | S | 36/36 | Seeds 1–12 × Easy/Normal/Hard, Moon and Mars |

Open balance question: the Greedy player still earns S (86). If the team wants the storm choice to matter more in the grade, lower the "Steady power" points or raise the S line. Not changed without the team.

## Historical: real-time version (before the redesign)

Generated UTC: 2026-11-14 17:22:52Z
Source: shipped `Balance.json`, real LOLA terrain, and the existing scenario bots. No values were tuned by this export.

All power, costs, resource rates, safety, and mission timing below are fictional balance values, not equipment specifications or a radiation-dose forecast.

| Bot | Result | Stars | Fail reason | Game hours | Lowest storm safety | Storm brownouts | Batteries at onset | Stored kWh at onset |
|---|---|---|---|---|---|---|---|---|
| DoNothing | Fail | 0 | RadiationNoShelter | 20.2 | 38.123 | 0 | 1 | 4 |
| ShedLoads | Win | 3 | None | 25.1 | 100 | 0 | 1 | 3.584 |
| StoredEnergy | Win | 3 | None | 25.1 | 100 | 0 | 2 | 7.584 |
| Greedy | CloseCall | 1 | None | 25.1 | 100 | 1 | 1 | 3.584 |
| Idle | Fail | 0 | NoAir | 34.25 | 100 | 0 | 0 | -1 |

## Shipped building values

| Building | Footprint | Cost | Generation kW | Demand kW | Battery kWh | Charge/discharge limit kW | Priority |
|---|---|---|---|---|---|---|---|
| Habitat | 3x3 | 30 | 0 | 1 | 0 | 0 | 1 |
| Solar Array | 2x2 | 10 | 3 | 0 | 0 | 0 | 0 |
| Battery | 1x2 | 12 | 0 | 0 | 4 | 3 | 0 |
| Life Support | 2x2 | 20 | 0 | 4 | 0 | 0 | 1 |
| Communications | 1x1 | 8 | 0 | 1 | 0 | 0 | 4 |
| Greenhouse | 3x2 | 25 | 0 | 4 | 0 | 0 | 3 |
| Shelter | 2x2 | 15 | 0 | 2 | 0 | 0 | 2 |

Starting reserves: 150 supplies; 100 L water; 20 meals; 24 air-hours. Step: 0.05 game hours. One game hour = 5 real seconds at 1x.

Greenhouse: 5 L/h in, 4 L/h returned (80% recycled), net loss 1 L/h; 0.5 meals/h gross while powered and watered. Crew consumption: 0.375 meals/h. Solar generation is multiplied by tile sunlight. The storm does not cut solar output.

## Verification limits

These are automated simulation runs, not human playthroughs. Successful bots finish in roughly 25.1 game hours (about 126 real seconds at 1x), excluding construction/reading. Pacing, ease of use, and how fair it feels are awaiting human testing and Opus review. Failed runs end with a reason. The functional Main scene now includes Retry; human usability and full-story integration remain pending.

## Complete event logs

### DoNothing

```text
0h placed_Habitat: Habitat placed.
0h placed_SolarArray: Solar Array placed.
0h placed_SolarArray: Solar Array placed.
0h placed_SolarArray: Solar Array placed.
0h placed_SolarArray: Solar Array placed.
0h placed_LifeSupport: Life Support placed.
0h placed_Battery: Battery placed.
0h placed_Communications: Communications placed.
0h placed_Greenhouse: Greenhouse placed.
0.1h shadow_warning: A long shadow will cross the base in 2 hours.
2.1h shadow_start: A long shadow covers the base. Less sunlight reaches the panels.
3.1h brownout: Battery empty at 11:00: demand 10 kW > supply 7.8 kW. Switched off: Communications.
3.2h brownout: Battery empty at 11:00: demand 10 kW > supply 7.8 kW. Switched off: Greenhouse.
5.1h shadow_end: The long shadow has passed. Full sunlight is back.
5.1h shadow_passed: Power shortage passed. A second Battery is unlocked.
7.1h first_harvest: First harvest! A supply lander brings 30 supplies. Shelter unlocked.
10.1h storm_warning: Solar particle warning from mission control.
14.1h storm_onset: Solar particles have reached the Moon.
15.1h self_return: Yuki hears her suit alarm and heads back.
16.6h geologist_inside: Yuki is back inside.
20.1h storm_decay: The particle storm is fading.
20.2h failed: Mission paused: crew safety stayed too low.
```

### ShedLoads

```text
0h placed_Habitat: Habitat placed.
0h placed_SolarArray: Solar Array placed.
0h placed_SolarArray: Solar Array placed.
0h placed_SolarArray: Solar Array placed.
0h placed_SolarArray: Solar Array placed.
0h placed_LifeSupport: Life Support placed.
0h placed_Battery: Battery placed.
0h placed_Communications: Communications placed.
0h placed_Greenhouse: Greenhouse placed.
0.1h shadow_warning: A long shadow will cross the base in 2 hours.
2.1h shadow_start: A long shadow covers the base. Less sunlight reaches the panels.
3.1h brownout: Battery empty at 11:00: demand 10 kW > supply 7.8 kW. Switched off: Communications.
3.2h brownout: Battery empty at 11:00: demand 10 kW > supply 7.8 kW. Switched off: Greenhouse.
5.1h shadow_end: The long shadow has passed. Full sunlight is back.
5.1h shadow_passed: Power shortage passed. A second Battery is unlocked.
7.1h first_harvest: First harvest! A supply lander brings 30 supplies. Shelter unlocked.
10.1h storm_warning: Solar particle warning from mission control.
10.1h recall: Yuki is called back to base.
10.1h placed_Shelter: Shelter placed.
11.6h geologist_inside: Yuki is back inside.
13.6h on_Shelter: Shelter switched on.
14.1h storm_onset: Solar particles have reached the Moon.
14.1h off_Greenhouse: Greenhouse switched off.
20.1h storm_decay: The particle storm is fading.
22.1h storm_over: All clear. The storm is over.
22.1h sunrise_start: Sunlight creeps over the ridge.
22.1h off_Shelter: Shelter switched off.
22.1h on_Greenhouse: Greenhouse switched on.
25.1h sunrise_done: The first lunar sunrise lights the whole base.
```

### StoredEnergy

```text
0h placed_Habitat: Habitat placed.
0h placed_SolarArray: Solar Array placed.
0h placed_SolarArray: Solar Array placed.
0h placed_SolarArray: Solar Array placed.
0h placed_SolarArray: Solar Array placed.
0h placed_LifeSupport: Life Support placed.
0h placed_Battery: Battery placed.
0h placed_Communications: Communications placed.
0h placed_Greenhouse: Greenhouse placed.
0.1h shadow_warning: A long shadow will cross the base in 2 hours.
2.1h shadow_start: A long shadow covers the base. Less sunlight reaches the panels.
3.1h brownout: Battery empty at 11:00: demand 10 kW > supply 7.8 kW. Switched off: Communications.
3.2h brownout: Battery empty at 11:00: demand 10 kW > supply 7.8 kW. Switched off: Greenhouse.
5.1h shadow_end: The long shadow has passed. Full sunlight is back.
5.1h shadow_passed: Power shortage passed. A second Battery is unlocked.
5.1h placed_Battery: Battery placed.
7.1h first_harvest: First harvest! A supply lander brings 30 supplies. Shelter unlocked.
10.1h storm_warning: Solar particle warning from mission control.
10.1h recall: Yuki is called back to base.
10.1h placed_Shelter: Shelter placed.
11.6h geologist_inside: Yuki is back inside.
13.6h on_Shelter: Shelter switched on.
14.1h storm_onset: Solar particles have reached the Moon.
20.1h storm_decay: The particle storm is fading.
22.1h storm_over: All clear. The storm is over.
22.1h sunrise_start: Sunlight creeps over the ridge.
22.1h off_Shelter: Shelter switched off.
25.1h sunrise_done: The first lunar sunrise lights the whole base.
```

### Greedy

```text
0h placed_Habitat: Habitat placed.
0h placed_SolarArray: Solar Array placed.
0h placed_SolarArray: Solar Array placed.
0h placed_SolarArray: Solar Array placed.
0h placed_SolarArray: Solar Array placed.
0h placed_LifeSupport: Life Support placed.
0h placed_Battery: Battery placed.
0h placed_Communications: Communications placed.
0h placed_Greenhouse: Greenhouse placed.
0.1h shadow_warning: A long shadow will cross the base in 2 hours.
2.1h shadow_start: A long shadow covers the base. Less sunlight reaches the panels.
3.1h brownout: Battery empty at 11:00: demand 10 kW > supply 7.8 kW. Switched off: Communications.
3.2h brownout: Battery empty at 11:00: demand 10 kW > supply 7.8 kW. Switched off: Greenhouse.
5.1h shadow_end: The long shadow has passed. Full sunlight is back.
5.1h shadow_passed: Power shortage passed. A second Battery is unlocked.
7.1h first_harvest: First harvest! A supply lander brings 30 supplies. Shelter unlocked.
10.1h storm_warning: Solar particle warning from mission control.
10.1h recall: Yuki is called back to base.
10.1h placed_Shelter: Shelter placed.
11.6h geologist_inside: Yuki is back inside.
13.6h on_Shelter: Shelter switched on.
14.1h storm_onset: Solar particles have reached the Moon.
18.4h brownout: Battery empty at 02:00: demand 12 kW > supply 11.2 kW. Switched off: Communications.
20.1h storm_decay: The particle storm is fading.
22.1h storm_over: All clear. The storm is over.
22.1h sunrise_start: Sunlight creeps over the ridge.
22.1h off_Shelter: Shelter switched off.
25.1h sunrise_done: The first lunar sunrise lights the whole base.
```

### Idle

```text
34.2h failed: Mission paused: crew safety stayed too low.
```
