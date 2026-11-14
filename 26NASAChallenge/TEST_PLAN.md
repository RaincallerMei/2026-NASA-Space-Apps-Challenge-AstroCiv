# Test plan

Prepared from the 25-hour board and master prompt. Human sessions below are **not run**. Scene layout, art, pacing, and difficulty changes belong to Opus and the team.

## Automated checks

In Unity 6000.6.7f1, open **Window > General > Test Runner**, select **EditMode**, and run the `Mapmakers.Tests` tests. Record the actual totals; never treat a test file's existence as a pass.

With the installed Unity CLI, from the Unity project folder:

```powershell
unity command test run --mode editor --filter Mapmakers --async-tests true
unity command test status --detail failures
```

The completed execution-pass run on November 14, 2026 was **53/53 passing**, including nine new timing/reset cases. Result: `../review/execution-tests.json`. The earlier baseline was 44/44. No full-scene PlayMode or human test is implied by these counts.

Coverage required by the briefs: exact battery depletion and limits; critical-load priorities; storm does not reduce solar; powered/watered greenhouse and 80% fictional recycling; placement bounds, overlap, terrain and cost; determinism; fixture metadata and no API keys; dialogue structure and word limits; DoNothing fails, ShedLoads wins, StoredEnergy wins, Greedy at best a close call. Also check fixed-step Pause/1x/2x/4x and Reset/Retry execution independently of frame rate.

## Required PlayMode smoke test — pending Main scene

1. Load the completed Main scene through Boot/Menu.
2. Place Habitat, Solar Arrays, Battery, Life Support, Communications and Greenhouse using their runtime commands.
3. Advance to the existing storm warning without changing balance values.
4. Recall the geologist while Communications is powered; activate the Shelter and shed optional demand.
5. Reach the debrief and ending, then Retry.
6. Assert mission outcome and zero errors/warnings from game code. Separately record package warnings.

Do not mark this done until the real game scene, controls, and debrief are connected. A simulation-only test is not a replacement.

## Five-minute usability script

Use three new players, including a child aged 7–12 if available. The team arranges participation. Observe quietly and record what they actually do; do not write results in advance. Pause is allowed at any time.

| Time | Task | Observe |
|---|---|---|
| 0:00–1:00 | Start the tutorial and place the first building | Can they find the build control and a valid tile? |
| 1:00–2:00 | Add solar power and read the resource display | Can they tell power made from power used? |
| 2:00–3:00 | Inspect a building, use Pause, and talk to one crew member | Can they use reply buttons without typing or help? |
| 3:00–4:00 | Use a prepared storm save/setup when available | Can they identify the outside crew member and a safety action? |
| 4:00–5:00 | Explain their choice and answer the questions below | Record exact words and any assistance. |

After the short script, ask for a separate full playthrough. Time the complete game; board target is 12–16 minutes. Check the first outpost against the board's approximately five-minute target. Do not tune these values in this execution pass.

Ask: **What was the battery for? What did the alert mean? Who did you talk to? Want to play more?** Also ask how that crew member felt and one real first step toward space science or astronaut work.

| Tester | Age range | Date | Full-run time | Where stuck / help needed | Battery explanation | Alert explanation | Crew/chat response | Wants more? |
|---|---|---|---|---|---|---|---|---|
| 1 — child if available | Pending | Not run | — | — | — | — | — | — |
| 2 — new player | Pending | Not run | — | — | — | — | — | — |
| 3 — new player | Pending | Not run | — | — | — | — | — | — |

## Full-play and recovery checklist

- Play start to ending three times and record console errors.
- Win once by shedding loads and once with stored energy. Try ignoring the storm and confirm a gentle explanation and visible Retry.
- Spam placement, test every map edge and overlap, and try building without enough supplies.
- Pause during a warning and during the storm. Resume at each specified speed.
- Spam chat/reply buttons; close/reopen every conversation; test recall with the radio off.
- Retry after failure and after success. Confirm no old buildings, events or resource changes remain.
- Check all status information uses text and a shape/icon as well as colour. Verify text at 1920x1080, clipping, pointer targets, and scalable UI once Opus supplies screens.
- Confirm six hints show only once per run and can be disabled. Verify chat reacts to the current base and remembers the storm choice.

## Build and submission checks — pending playable build

- Build WebGL; serve over HTTP and open in a separate browser/computer.
- Block NASA/CSA network access and remove API-key environment variables; play from start to ending. Local host requests for WebGL files are allowed.
- Search tracked source and build output for credentials; only DEMO_KEY may be public. Do not paste secret values into a log.
- Click the playable link, source links, credits and video while logged out. Use the supplied guide's 30-second maximum video / seven-slide maximum; confirm event time zone and any earlier local deadline. See SUBMISSION_REQUIREMENTS.md.
- Review final child-scene artwork and any test recordings against the guide's under-18 likeness clause. Add the required visible labels to AI-generated images/video and metadata/text acknowledgements for generated code/data/audio.
- Record clips and screenshots only after the corresponding screens actually exist.

## Fix round

The team selects the top three observed problems. Opus decides design/aesthetic changes. Fix agreed defects, rerun affected checks and the full suite, then record the feature freeze. No tester quotes, successful runs, or submission confirmation have been collected yet.
