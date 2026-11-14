# Scenario checks from the existing design

These are documented test setups for the mechanics already chosen in the board and briefs. They add no events or balance changes. Difficulty/pacing labels await Opus; no new difficulty setting is selected here. All kW, kWh, timings, resources and safety values are fictional balance.

## 1. Power shortage

Starting setup: Habitat, Life Support, Communications, Greenhouse, three full-sun Solar Arrays, one full 4 kWh Battery. This is the existing `PowerTests.Deficit_DrainsBatteryToExactlyZero_AtTheRightHour` setup. Total demand is 10 kW; generation is 9 kW.

| Existing choice | Calculation | Expected result |
|---|---|---|
| Leave loads on | 4 kWh / (10 - 9) kW = 4 game hours | Battery reaches zero; lowest-priority load is shed on the following step |
| Switch Greenhouse off | Demand falls from 10 to 6 kW; surplus is 3 kW | Critical loads stay powered and battery charges |
| Add the fourth Solar Array on full-sun ground | Generation rises to 12 kW; surplus is 2 kW | All ordinary loads stay powered and battery charges |

Takeaway already chosen: batteries store energy for later; reducing demand or increasing generation fixes a deficit. Construction requires a free valid footprint and supplies. Test fixtures may provide supplies; do not confuse those fixtures with an affordable live-game sequence.

## 2. Solar-particle warning

Starting setup: the existing scenario bots build the six-building outpost on the shipped LOLA terrain, pass the existing shadow event, and receive the storm warning after first harvest. The warning uses saved DONKI metadata for its eventual display; the game's 4/10/12-hour particle timeline and crew effects are fictional.

| Existing strategy | Actions | Recorded automated result |
|---|---|---|
| ShedLoads | Recall the geologist with a powered radio, use Shelter, rest Greenhouse during particles | Win, 3 stars; minimum storm safety 100; no storm brownout |
| StoredEnergy | Recall, use Shelter, and add a second charged battery before particles | Win, 3 stars; minimum storm safety 100; no storm brownout |
| DoNothing | Build the outpost but ignore storm protection | Failure, reason RadiationNoShelter |

See [BALANCE_REPORT.md](BALANCE_REPORT.md) for generated values and full event logs. A 4 kWh battery serving a 3 kW deficit can last at most 4/3 game hours, subject to its 3 kW discharge limit; batteries are not generation. These arithmetic numbers come from the board example, not a claim about the bots' exact terrain-averaged deficit.

Takeaway already chosen: a radiation warning is not a power blackout. Recall/shelter protect people; power choices keep the shelter and life support supplied. Never turn DONKI GST or ISS dose into a Moon-dose prediction.

## 3. Low sunlight / shadow

Starting setup: the existing shadow unit test uses four full-sun panels (12 kW), normal demand 10 kW, and the unchanged configured shadow factor 0.7 for three game hours. Shadowed generation is 12 x 0.7 = 8.4 kW. This is a separate lighting event; no radiation protection is required by shadow alone.

| Existing choice | Calculation | Expected result |
|---|---|---|
| One full Battery, keep all demand | Deficit 1.6 kW; 4 / 1.6 = 2.5 game hours | Cannot keep all ordinary loads on for the entire three-hour shadow |
| Rest Greenhouse | Demand 6 kW; surplus 2.4 kW | Critical loads remain supplied through the shadow |
| Use two already-unlocked full Batteries | Three-hour energy need 1.6 x 3 = 4.8 kWh; capacity 8 kWh | Sufficient stored energy and discharge power in this controlled setup |

The alternate battery setup requires the second-battery unlock first; it is not available before the initial shortage. This is an analytical follow-up setup, not a newly added mission. Shadow reduction/restoration is automated-test coverage; the arithmetic alternate setups above are calculations, not claims of new player playthroughs.

Takeaway already chosen: location and shadow change sunlight; a solar-particle warning by itself does not.
