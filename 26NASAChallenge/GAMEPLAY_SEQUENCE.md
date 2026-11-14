# Gameplay sequence

Normal entry: open `Assets/_Game/Scenes/Boot.unity` and press Play. Boot loads Prologue; the build scene order is Boot, Prologue, Main. Opening Main directly remains an outpost-only development shortcut.

| Order | Player action / story | Guard |
|---|---|---|
| 1. Childhood dream | Continue through the existing Mississauga, science-event and inspiration lines | No simulation/resource timer |
| 2. Draw the base | Place Habitat, then Solar Array, then Greenhouse on valid tiles | Cannot skip placement or build later systems; power-in/out preview runs on a clone |
| 3. Teacher | Read the two existing teacher lines | Placement complete before Continue is available |
| 4. Photo cards | University, PhD, astronaut training, in order | Existing dream-not-promise disclaimer appears on the third card |
| 5. Departure | Continue to the Moon | One scene transition only; repeated clicks cannot skip the next chapter |
| 6. Commander | Drawing, commander, Mississauga class question and life-support task | Main receives the three drawing positions, starts with fresh 150 supplies and no ticking until these lines finish |
| 7. Outpost | Build Habitat, Solar Arrays, Battery, Life Support, Communications and Greenhouse | Existing footprint, terrain, cost, overlap, power and resource rules |
| 8. Shadow and harvest | Balance demand during the shadow; grow the first harvest | Existing simulation unlocks second Battery after shadow and Shelter after harvest; the same outpost persists |
| 9. Storm | Recall Yuki, power Shelter, shed optional loads or use stored energy | Existing scripted warning/onset/decay; storm does not change solar output |
| 10. Debrief | Read success/close-call or the explained failure | Success waits for Continue; failure offers Retry and cannot advance to the ending |
| 11. Window ending | Existing window, sunlight, Earth, old drawing and final-message lines | Only follows completed successful/close-call mission; separate sunrise lighting occurs in the simulation |
| 12. Blueprint v2 | Read the final new-kid/blueprint text | End state; Replay tutorial starts Prologue again |

Reset/Retry in Prologue returns to the tutorial intro and clears its placements. Reset/Retry in Main starts a fresh outpost attempt. Replay tutorial is available explicitly; it does not silently reset a live outpost.

Story text is loaded from the existing Story.json. Bedroom, photo cards and observation window currently use labelled text/greybox placeholders; final illustrations and the crew-chat screen remain separate Opus work. No final art, dialogue or balance values were changed for this sequence.

Automated coverage: CampaignSmokeTests follows Boot through the real scene change, checks placement order, timer safety, fresh Moon resources/drawing transfer, the storm, mandatory debrief and Blueprint ending. A second test checks tutorial Reset and that failure cannot enter the success ending. Existing outpost button tests remain in place. The combined suite passes 57/57 tests; evidence is in [campaign-tests.json](../review/campaign-tests.json).

Human reading time, child usability and the overall 12–16 minute target still require the manual test plan; automated fast-forward tests are not timing/usability evidence.
