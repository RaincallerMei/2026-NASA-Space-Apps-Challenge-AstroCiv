# Cloud agent prompt: real-time base strategy and mission outcomes

Copy the prompt below into a cloud coding agent with this repository attached.

---

Implement a gameplay redesign for **Mapmakers of the Moon / AstroCiv**, our existing Unity 2D educational base-building game for ages 7–12. Convert the current day-turn gameplay into **pausable real-time base strategy**, and implement distinct failure presentations for resource depletion and unsafe crew conditions.

**The key product decision: running out of mission time is an evaluation, not a failure.** Freeze the base at the deadline and grade what the player actually built and maintained. An incomplete base receives a lower grade. Resource depletion or a serious crew-safety incident can end the mission early with a failure scene.

Implement the change, test it, and leave a reviewable diff. Do not stop at a design proposal. Make routine reversible implementation decisions yourself and document them. Do not deploy or push changes.

**Track this AI session:** read and append to the repository-root `Work Log and Review/AI_USE_LOG.md`. Record the actual executor/tool/model if known, date/time, this prompt as input, purpose, method, changed/generated artifacts, revisions, and checks actually run. Mark human review and unknown details honestly. All project-created art is AI-created, including procedural placeholders/derivatives; update asset credits and the board's AI-use summary if new art is created or imported. Prompt creation is already logged separately from this implementation session; do not treat it as proof of completed implementation.

## 1. Read the current project before editing

The repository root contains `Agent Instructions/`; the Unity project root is **`26NASAChallenge/`**, containing `Assets/`, `Packages/`, and `ProjectSettings/`.

Read applicable `AGENTS.md` instructions, then:

- `Agent Instructions/CLAUDE.md` and `Agent Instructions/CLAUDE_CODE_MASTER_PROMPT.md` for project context.
- `26NASAChallenge/README.md`, `GAMEPLAY_SEQUENCE.md`, `PLAYER_LEARNING.md`, and `TEST_PLAN.md`.
- `26NASAChallenge/Assets/_Game/Scripts/Sim/`, especially `Simulator.cs`, `SimState.cs`, `Scenario.cs`, `Scoring.cs`, balance definitions, and bots.
- `Scripts/Game/MissionRun.cs`, `Flow.cs`, level setup and saved-base handling.
- `Scripts/UI/GameScreen.cs`, its other partial files including `GameWindows.cs`, `ResultsScreen.cs`, and existing scene builders.
- `Data/Resources/Balance.json`, `Levels.json`, and `Story.json` under `Assets/_Game/`.
- The existing EditMode and PlayMode tests.

Paths above are relative to the Unity project where appropriate. Verify files and behavior; older handoff documents contain historical information. This prompt is the updated product direction and supersedes older gameplay requirements for day turns, automatic failure at a deadline, and the former delayed food/air failure behavior. Preserve higher-priority repository instructions.

Keep the existing Unity version and package versions. Preserve the four-level story, untimed drawing tutorial, Moon/Mars settings, Free Build seeds, crew interactions, saved progress, art, offline fixtures, and science attribution. Do not remove existing Mars gameplay because an older brief called it future scope. Preserve unrelated work in the checkout.

## 2. Real-time gameplay

Replace the player-facing **Next Day** loop with continuous simulation. Days/sols may remain as the world clock, but players must no longer advance them manually.

- Provide mouse-accessible **Pause, 1×, 2×, and 4×** controls. Space toggles pause when no text entry/modal owns input. Remove Enter-to-Next-Day and obsolete hints.
- Allow building placement, inspection, repair, recall, and optional-system toggles during a player-requested pause. Pausing must never lower the grade.
- Keep the drawing tutorial untimed, with no depletion or deadline.
- At the start of a timed mission, show a brief paused briefing and a **Start mission** button. After Start, the simulation runs without further advance commands.
- Show a persistent remaining-time display labelled **Evaluation in**, alongside the game clock. Its countdown follows simulation time and the selected speed, and stops while paused. Do not use an independent wall-clock countdown.
- Pause for menus, essential teaching dialogs, crew conversations, and loss of application focus. Closing a modal restores the previous play/pause state and speed. Nested pauses must not resume the mission underneath another modal. Returning from focus loss should require explicit Resume.
- Ordinary warning banners remain nonmodal. The first major warning may pause for a short explanation; repeated alerts must not repeatedly interrupt play.
- Display live resource quantities, net rates, and honest depletion estimates when a resource is draining. Identify the affected system and provide a useful action. Use text and icons as well as colour.
- Do not introduce enemies, combat, multiplayer, a technology tree, or a construction-queue system. Keep existing placement unless a small change is essential. The strategic pressure comes from reserves, event timing, supplies, and competing electrical loads.

Keep the simulation pure C# and deterministic. Feed its fixed steps from one runtime clock/accumulator outside the simulation; do not pass variable frame time into resource calculations. Pause stops accumulation. Prevent a large catch-up burst after tab suspension or a long frame. Speed changes must preserve accumulated fractional time. UI animation can continue while the simulation is paused.

Do not rebuild the entire map/UI or run a whole-day forecast every frame. Adapt the existing revision/refresh system, update visible counters at a reasonable cadence, and refresh structural UI only as needed. Forecast/preview operations must not mutate live state, trigger failure scenes, or advance the mission clock.

## 3. Separate end reasons, grades, and progression

Represent these concepts explicitly, using the smallest clear change to the existing architecture:

1. **Running**: simulation and player commands are active.
2. **Deadline evaluation**: mission duration elapsed; evaluate a frozen snapshot.
3. **Mission failed**: a specific resource/safety condition ended the mission early.

Pause is a runtime control state, not an end reason. Grade and campaign eligibility are separate result properties. Avoid continuing to overload `completed`, `failed`, or `score.passed` with several meanings.

All terminal paths must freeze the simulation, reject further gameplay mutations, capture the result once, and transition once. A later callback must not replace a failure with an evaluation or submit duplicate progress records. Store the end reason, timestamp, actual resource readings, relevant system state, and score inputs so the presentation can explain the result.

**Deadline behavior:**

- Stop exactly at the configured mission duration; do not simulate beyond it or process events scheduled after it. Use durations aligned to fixed steps or handle the final step explicitly.
- Evaluate the base even if required buildings or goals are missing. Do not invent a `TimeOut` failure.
- Use the heading **Evaluation complete**, with the grade, completed/missing requirements, and concrete improvement suggestions. A low grade must not select failure artwork, a failure animation, or “mission failed” copy.
- Building everything early does not automatically end the survival/storm mission. The player must operate the base until evaluation.
- If a resource/safety failure is reached on the same final step as the deadline, failure takes precedence. If resources remain above their terminal thresholds, use evaluation even when the base is incomplete.
- No end-state edits may improve the submitted snapshot.

Retain sensible campaign prerequisites independently of result presentation. A safe low-grade evaluation with the required base can progress. If an incomplete base cannot support the next mission, explain the missing prerequisite and offer Retry/Menu without calling it a failure. Never load an invalid carried base or silently give credit for missing buildings. Preserve earlier best scores and valid saved bases after a worse retry or failure. Keep existing save keys compatible where possible, and document any necessary migration.

## 4. Resource and safety failure rules

The updated default is **essential resource exhaustion ends the mission when the reserve reaches zero**, following clear warnings beforehand. Do not leave oxygen/food at zero for an invisible multi-hour delay while waiting for the old aggregate safety rule. Clamp quantities at zero; do not allow negative reserves.

| Condition | Required outcome |
|---|---|
| Oxygen/air reserve reaches zero | End immediately with `NoAir` or an equivalent explicit reason. Explain whether Life Support was missing or unpowered. |
| Food reserve reaches zero | End immediately with `NoFood`. Explain the actual cause: missing greenhouse, no power, insufficient production, or no water. |
| Usable water reaches zero | End immediately with `NoWater`. Explain that the mission's usable reserve is exhausted. |
| Battery reaches zero | Warn and apply the existing power-priority rules. This alone is not failure: generation may still cover essential loads. |
| Sustained loss of critical electrical service | End with a distinct power failure only if a documented, configurable emergency tolerance is exceeded. Oxygen depletion can end the mission sooner. A brief brownout remains recoverable. |
| Radiation exposure / unsafe crew | Preserve a forgiving, configurable safety threshold and exposure duration, with separate outside-crew and missing/unpowered-shelter explanations. |
| Building supplies reach zero | No automatic failure. Explain unaffordable actions; a functioning base can run with zero supplies. |
| Deadline arrives with unfinished buildings/goals | Evaluate and reduce the grade; no failure scene. |

The current code uses water only for the greenhouse. Make the usable-water rule meaningful by adding a small configurable crew water-consumption rate and displaying it. Keep greenhouse net consumption and recycling separate; do not accidentally double-count recycling or imply that plants supply the crew's drinking water. All new rates are **fictional game balance**, not medical guidance or NASA specifications. Use existing starting reserves and conservation choices for the initial scope; do not add a new water-building subsystem. Tune duration and reserves so water conservation matters and valid strategies remain winnable.

Do not exempt a run from consumption or resource failure merely because the player has not placed a Habitat, or removes it. There is an expedition crew throughout a timed mission. Keep the untimed drawing tutorial exempt.

Add warning/critical levels before exhaustion, using remaining time where practical. Give a new player reasonable time to act at 1×. Brief shortages of electrical service or declining reserves should support recovery before the terminal condition. Resolve simultaneous causes deterministically, record secondary active hazards, and explain the actual terminal cause rather than automatically blaming the largest historical damage total.

## 5. Design and implement failure scenes

Create a reusable failure presentation with **distinct visual variants**, integrated into the scene flow. It may be a shared scene or a full-screen state with the frozen map behind it; do not create many nearly identical scenes. It must feel like a short story moment, not only a red error dialog.

For every failure:

1. Freeze gameplay immediately and capture the final base view/state.
2. Play a short, skippable 2–4 second visual beat using UI time independent of the stopped simulation.
3. Show **Mission stopped**, the specific cause, a short explanation from actual telemetry, and one practical improvement.
4. Offer **Try again**, **View report**, and **Menu**. Retry uses the same mission/seed and intended starting base, not the damaged terminal state. No checkpoint feature is required.

Use the existing pixel-art palette, building silhouettes, crew portraits, and reusable UI elements. Provide a replaceable art slot for each variant; use polished procedural placeholders if artwork is missing. No new asset download or image-generation dependency is required. Keep text in the existing story/localization data rather than scattered through UI code.

| Variant | Visual direction | Example player-facing text / improvement |
|---|---|---|
| No air | Close view of Habitat and Life Support; empty air meter; calm emergency lights | “The air reserve is empty.” / “Keep Life Support powered.” |
| No food | Empty meal storage; greenhouse visibly inactive or dry if that was the cause | “The food reserve is empty.” / Show the relevant fix, such as “Keep the greenhouse powered and watered.” |
| No water | Empty tank indicator; dry greenhouse when present | “The water reserve is empty.” / Explain the largest actual water use and recommend conserving it. |
| Critical power | Darkened critical modules; empty battery only if actually empty; power-flow diagram stops | “Essential systems lost power for too long.” / “Protect Life Support before optional systems.” |
| Outside crew radiation | Frozen crew marker outside; storm particles; highlighted recall control | “The crew could not reach safety in time.” / “Call the crew in before particles arrive.” |
| Missing/inactive shelter | Habitat silhouette during particles; Shelter missing, off, or unpowered as appropriate | “The crew needed a powered Shelter.” / Give the matching build, activation, or power advice. |

Do not show death, injury, suffocation, panic, explosions, or blame directed at the child. The scene represents mission control stopping an unsafe training mission. Do not falsely narrate a successful in-game rescue. Keep sounds gentle, respect Mute, avoid flashing effects, and provide a reduced-motion/static presentation. Players can skip animation immediately; buttons must remain usable even if an optional asset/audio clip is absent.

The **deadline evaluation** gets a different visual beat: inspection overlay, blueprint checklist, and a grade reveal over the frozen base. Missing items appear as clear checklist gaps. It must remain distinguishable from resource failure even at the same letter grade.

## 6. Grading, timing, and balance

Retain a transparent 0–100 grade with S/A/B/C and stars. Adapt the existing categories rather than building an unrelated scoring system:

- Base completeness and mission goals: partial credit for what is actually present/achieved; evaluate operating state where relevant.
- Crew protection and resource reserves at evaluation.
- Essential-service reliability measured by simulated duration/severity rather than whole-day counts.
- Meaningful storm/shadow preparation and outcomes.

Keep category weights/caps and any new duration thresholds in editable configuration. An incomplete core base must not receive a high mark just because unused reserves remain full. Default policy: missing a required core building caps the evaluation at **B, 60/100**; missing another required mission goal prevents S. Show any cap in the report so displayed points and the final score reconcile. A low grade is still an evaluation, not a safety failure. Do not award reliable-power points for having no served critical systems, or award storm-preparation credit before a storm occurred. Preserve and clearly show any separate safety-failure grade cap.

Tune the **whole campaign** toward the existing approximately 12–16 minute experience; do not give every level a 12-minute timer. Configure mission durations individually. Convert or preserve event offsets coherently, with enough real time at 1× to understand warnings, recall crew, and adjust loads. Ensure the storm and meaningful recovery occur before evaluation. The current day-turn values include 30 kWh batteries, 95% greenhouse recycling, and day-long warnings; inspect and retune them together rather than blindly restoring an old real-time report.

At least two distinct strategies must work: shedding optional loads and preparing additional stored energy. Power shortages and radiation remain separate systems: a solar-particle alert must not switch solar generation off. Keep all game balance labelled fictional and preserve source-data distinctions.

Update Story Mode and the existing finite-duration Free Build runs consistently. Preserve seeded maps/events and all current difficulty options. Do not add an endless mode for this task.

## 7. Verification and delivery

Update obsolete turn-based tests and add focused tests covering:

- Frame-independent fixed stepping, pause/resume, speed changes, modal/focus pauses, and no suspended-tab catch-up burst.
- All essential-resource failure reasons, zero clamping, recovery before exhaustion, and consumption without a placed Habitat.
- An empty battery with adequate generation does not fail; critical-power tolerance does, with accurate diagnostics.
- Deadline evaluation of complete and incomplete bases; incomplete-base grade cap; low grade does not activate failure presentation.
- Failure/deadline precedence, no step past the deadline, no events after the deadline, one result submission, and no mutations after termination.
- Retry, Free Build same seed, saved-base carryover, campaign prerequisites, and preservation of earlier progress.
- At least two winning scenario bots, a genuine resource/safety failure, and a surviving incomplete-base evaluation.
- PlayMode routes through Boot/Menu, timed gameplay, each failure visual variant, Retry, deadline evaluation, and Results. Test real failure triggers as well as presentation routing; do not rely only on fabricated result objects.
- Existing science invariants, placement, power accounting, and offline-data behavior remain covered.

Run applicable EditMode and PlayMode suites and fix regressions. Visually inspect gameplay HUD, all failure variants, and incomplete-base evaluation at common desktop/WebGL dimensions; check legibility, clipped copy, controls, and correct frozen-state visuals. Validate the actual WebGL build if the environment supports it.

A cloud checkout may lack Unity, the required editor version, or a display. Discover available capabilities instead of assuming access to my local editor or an unavailable Unity skill. If Unity cannot run, complete the code, data, tests, documentation, and scene registration/build-script changes; perform available static checks and provide exact local validation steps. Do not claim compilation, visual inspection, playtesting, or a WebGL build succeeded unless you actually ran it. Do not install/upgrade Unity or packages to bypass this constraint.

Update `README.md`, `GAMEPLAY_SEQUENCE.md`, `TEST_PLAN.md`, `BALANCE_REPORT.md`, and affected player-learning/design documentation. Label historical reports and separate analytical/bot evidence from human playtesting. Document controls, active-time semantics, mission durations, end conditions, grading caps, campaign progression, and all new fictional balance values. Do not refetch existing NASA data or touch API secrets for this gameplay change.

Finish with a concise report: implemented behavior, important files changed, mission durations/balance decisions, tests actually run with results, checks not run, and exact steps to try a resource failure versus an incomplete-base deadline evaluation. Deliver a coherent implementation for review, preserving unrelated changes.
