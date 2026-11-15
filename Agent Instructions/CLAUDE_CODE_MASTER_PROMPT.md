# Master prompt for Claude Code: build "Mapmakers of the Moon"

> **How to use:** put `CLAUDE.md` and `RESEARCH_AGENT_BRIEF.md` (the updated versions) in the Unity project root. Open the Unity project in the Editor and leave it open. Copy everything below the line into Claude Code as one message.

---

You are the lead developer for **Mapmakers of the Moon**, our entry for the **2026 NASA Space Apps Challenge, "Junior Astronaut Mission Trainer"**. Players are ages 7 to 12. The hackathon is live now (Nov 14 to 15, 2026). We have about 25 hours total, so this must be finished fast and tested. You work alone and without waiting for me. Do not stop at plans. Build, run, test, fix, repeat.

## 0. Read first, in this order
1. `CLAUDE.md` (build brief, simulation rules, **Real data layer**).
2. `RESEARCH_AGENT_BRIEF.md` (science rules, **Data pack**, art sources).
3. If they exist: `DATA_SOURCES.md`, `SCIENCE_SOURCES.md`, `SCIENCE_ASSUMPTIONS.csv`, `SCENARIOS.md`, `ASSET_CANDIDATES.md`.

If this prompt and `CLAUDE.md` disagree, **this prompt wins**. The one big change: **the greenhouse is part of the basic outpost** (it was a stretch goal before).

**Updated team requirement, November 14:** maintain the repository-root `Work Log and Review/AI_USE_LOG.md` after each AI session, recording purpose, actual tool/model if known, prompt/input, method, output, used/proposed status and real review/checks. **All project-created art is AI-created**, including procedural art and derivatives. This supersedes older “some art” disclosure and stock-pack plans below; do not invent third-party asset use. See the current AI-use log and ASSET_CREDITS.md for actual provenance.

## 1. What we are building (scope)
One Unity 2D, top-down, pixel-art game. About **12 to 15 minutes** of play.

| Part | Length | What the player does |
|---|---|---|
| **Tutorial: "The Dream"** | ~3 min | A Mississauga bedroom, night, a strategy game on a computer. Player learns click-to-place by building a tiny Moon base: Habitat + Solar Array. Sees power in vs out. Then 3 photo cards (U of T astronomy, PhD, astronaut training) and a jump to the Moon. |
| **Lunar outpost** | ~6 min | Persistent 20×15 grid. Build **Habitat, Solar Arrays, Battery, Life Support, Communications, Greenhouse**. Manage power, air, water, food. Turn optional loads on and off. |
| **Solar storm** | ~4 min | Scripted alert built from real DONKI data. Crew member is outside. Player decides, base reacts, outcome changes. |
| **Debrief** | ~1 min | Plain-language report, one science line per decision, stars, "Blueprint v2", retry. |

**Not now (leave clean extension points only):** rover/roamer surface exploration, space-station interaction, Mars outpost. Create empty interfaces `IExpeditionSystem` and a `PlanetProfile` ScriptableObject (Moon filled in, Mars stub with a TODO). Write **no** other code for them.

## 2. Story rules (about 15%, never more than 20%)
- Hero: a kid from Ontario who loves strategy games and the stars. Studies astronomy at the University of Toronto, earns a PhD, later trains as an astronaut and commands a lunar base. The kid's childhood base drawing becomes the mission blueprint. Ending: a new kid on Earth opens "Blueprint v2".
- Inspiration only: **Sara Seager** (Toronto-born exoplanet scientist, former TESS **Deputy** Science Director) and **Chris Hadfield** (Canadian astronaut). Names only. **No portraits, no quotes, no endorsement.** Say clearly the career path is a dream, not a promise.
- Total story text **under 300 words**. Max **12 words per on-screen line**. Reading level: grade 3. Every story beat must hand the player a task.
- Put all text in one `Story.json` / localizable table. Write the lines yourself in this tone: warm, short, curious.
- Beat order: dream (bedroom) → three photo cards → "I drew this when I was ten. It is missing things." → first steps on Moon → radio from a class in Mississauga asks "How do you breathe?" (triggers life support task) → mission control warns of solar activity → storm → debrief → Blueprint v2.

## 3. Environment and Unity handling
- A Unity project is **already open in the Editor**. First read `ProjectSettings/ProjectVersion.txt`, `Packages/manifest.json`, and the folder tree. **Keep the Unity version and packages. Do not upgrade.**
- Load the `unity:unity-cli` skill and use it to drive the running Editor (refresh/compile, run tests, read the console, enter Play Mode, capture a Game view screenshot if supported). Also use `unity:2d-pixel-perfect`, `unity:create-tile-palette-or-rule-tiles`, and `unity:ui` / `unity:ui-ugui` when you reach those jobs.
- If the Editor is open, batch-mode runs on the same project will fail because of the project lock. Do not close my Editor. Use the CLI against the open Editor. If that cannot work, say so once, write the code, and give me the exact steps to run the tests by hand.
- Create folders: `Assets/_Game/{Scenes,Scripts,Prefabs,Data,Art,UI,Tests}`. Scripts split into `Sim` (pure C#, no `UnityEngine`), `Data`, `Game`, `UI`.
- Use mouse-first input. Use whatever input system the project already uses. Do not mix two.
- Make a git commit at every milestone (section 12) in the project's repo. **Do not push.**
- **Secrets:** read `NASA_API_KEY` from the environment only. Never print it, save it, or put it in any file or build. Add `.env`, `*.key` to `.gitignore`. Add `.env.example` with `NASA_API_KEY=DEMO_KEY`.

## 4. Simulation (pure C#, unit-tested)
Fixed step `dtHours` (never frame time). `SimState` in, `SimState` out, deterministic. Speeds: Pause, 1×, 2×, 4×. Default **1 game hour = 5 real seconds** at 1× (tune).

**Resources and units (document in `SCIENCE_ASSUMPTIONS`-style comments and in the data file):**
| Resource | Unit | Rule |
|---|---|---|
| Power | kW generation/demand, kWh battery | `generation = Σ active generators × tile.sunFraction`; `demand = Σ active loads`; battery clamps 0..capacity; charge/discharge power limit; unmet **critical** demand tracked separately; energy never negative |
| Air | "air-hours" reserve (hours the crew can breathe), cap 24 | Life Support powered → refills; unpowered → drains 1 per hour |
| Water | litres | Greenhouse uses water; **recycling returns ~80%** (fictional); net loss makes water finite, so recycling matters |
| Food | meals | Crew eats; Greenhouse adds when powered **and** watered |
| Crew safety | 0..100 | Drops with low air, radiation exposure (crew outside or no shelter during storm), long food zero; recovers slowly. **Gentle: never instant game over**; failure = safety stays below threshold for a set time, with a clear reason and Retry |
| Supplies | build points | Spent on buildings |

**Priority order when power is short (shed from the bottom first):** 1 Life Support + Habitat (critical) → 2 Shelter (during alert) → 3 Greenhouse → 4 Communications. Player can also toggle optional loads by hand. Always show **exactly why** a warning fired ("Battery empty at 14:00: demand 12 kW > supply 9 kW").

**Starting balance (all FICTIONAL, keep in ONE editable data file, e.g. `BuildingDefs` ScriptableObjects + `Balance.asset`). Tune with bots (section 9):**
| Building | Footprint | Cost | Power | Other |
|---|---|---|---|---|
| Habitat | 3×3 | 30 | −1 kW | Crew lives here. Needs Life Support |
| Solar Array | 2×2 | 10 | +3 kW × tile sun | Max 4 on valid tiles |
| Battery | 1×2 | 12 | — | 4 kWh, 3 kW charge/discharge limit |
| Life Support | 2×2 | 20 | −4 kW (critical) | Makes air |
| Communications | 1×1 | 8 | −1 kW (optional) | Needed for mission-control messages |
| **Greenhouse** | 3×2 | 25 | −4 kW (optional) | Water in 5 L/h, 4 L/h returned; +0.5 meal/h; small air bonus |
| Shelter (storm only) | 2×2 | 15 | −2 kW while active | Protects crew from radiation |
Start: 150 supplies, 100 L water, 20 meals, 24 air-hours. Terrain: valid and blocked tiles, plus a per-tile `sunFraction` (0.5 to 1.0) from the illumination fixture.

**Rules that must hold:**
- A solar **warning never switches solar panels off.** Low sunlight comes only from `tile.sunFraction`.
- The default sensible build is **tight but winnable**: at least one real trade-off appears (for example, keep the greenhouse or protect reserve power).
- Do-nothing gives a **gentle, explained** fail with Retry. It never crashes or softlocks.

## 5. Solar storm scenario (the core decision)
Built from a **fixture** made from real DONKI data (one historic window with a flare + CME + SEP). Timeline example (12 game hours): T+0 warning ("Solar particle warning from mission control"), T+2 crew member is outside on EVA, T+4 particle onset, T+10 decays. Player choices include:
- Call the crew inside / cancel EVA.
- Activate the Shelter (extra 2 kW).
- Shed Greenhouse and/or Communications.
- Pre-charge or add a second Battery **before** onset (needs foresight; the warning comes early on purpose).

**Require at least two different winning strategies** (for example "shed loads" and "stored energy") and one clearly failing one. Debrief shows what the player did, what happened, and one science line each:
- "A solar warning is not a power cut. The Sun still shines."
- "Batteries save energy for later."
- "Critical jobs first: air before plants."
Show the **real** flare class and times from the fixture, with the "Real data" badge. Show the real ISS dose-rate card (RadLab) as a comparison and say plainly: the ISS is in low Earth orbit, this is **not** a Moon dose. Never present the game meter as a real dose prediction.

## 6. NASA and CSA data (follow `CLAUDE.md` → Real data layer exactly)
- Fetch at build time into `Assets/_Game/Data/Fixtures/`. Game reads fixtures only. **No internet needed to play.**
- Write `tools/fetch_data.py` (reads `NASA_API_KEY`, strips the key from saved URLs). Run it. Record each call's status, date, units in `DATA_SOURCES.md`. Fixtures to produce: `donki_event_pack.json`, `radlab_iss_sample.csv`, `illumination_grid.csv` (20×15 from LRO/LOLA polar illumination), `power_mississauga.json` (NASA POWER), `osdr_cards.json` (6 to 10 plant/health studies), `csa_food_cards.json`.
- If a call fails (the earlier sandbox got 403 from NASA hosts), write a hand-made fixture marked `classification: synthetic`, say so in `DATA_SOURCES.md`, and carry on. Never label synthetic data as real.
- Load fixtures through `Resources` TextAssets (so WebGL works without web requests). Add a test that every fixture parses and has `source`, `retrieved_utc`, `classification`.
- In-game: small "Real data" badge where fixture data shows. **Data credits** screen: "NASA DONKI, NASA OSDR RadLab, NASA POWER, LRO/LOLA (PDS), CSA. Not endorsed by NASA or CSA." Credit CSA as "© Canadian Space Agency" when used. No NASA logo. No astronaut portraits.
- Greenhouse content: show a short card from `csa_food_cards.json` (Tomatosphere / Veg-04A / Deep Space Food Challenge) and one OSDR plant study when the greenhouse is first built. Only use facts you read from the source page; cite it.
- Local-impact moment: prologue card comparing real Mississauga sunlight (POWER) with the Moon's pole (Sun stays within about 1.5° of the horizon, so shadows are long; source: NASA SVS). Label it simplified.

## 7. Art (pixel art, nearest-neighbour)
- Use `unity:2d-pixel-perfect`. **16 px per unit, Point filter, no compression.** Reference resolution **480×270** (4× to 1080p). Grid 20×15 tiles = 320×240 px play area, side panels for UI.
- Palette: grey Moon, white/blue/amber modules, clear status colours. Never rely on red/green alone: add icons and shapes. Readable at 1080p.
- **Verify the license on the publisher page and inside each downloaded archive before importing.** Preferred: Cozy Space Planet (itch.io, aquumgifts), Kenney Pixel UI Pack (CC0), optional Cozy Space Lite. Record creator, URL, license, edits in `ASSET_CREDITS.md`. If a download is blocked or the license is unclear, **generate tidy procedural placeholder sprites** and keep going.
- Build an `ArtManifest` (one asset per slot) so I can drop in Midjourney/other art later without touching code. Placeholders fill every slot so the game is always playable. Slots and sizes:
  - Tiles 16×16: ground (3 variants), crater, shadow, rock.
  - Buildings: Habitat 48×48, Solar 32×32, Battery 16×32, Life Support 32×32, Comms 16×16, Greenhouse 48×32, Shelter 32×32. On/off state each.
  - Crew astronaut 16×16 (idle, walk 2 frames, in-shelter).
  - Icons 16×16: power, battery, air, water, food, crew safety, alert, real-data badge.
  - Scenes: bedroom 480×270, Earth-in-sky strip, 3 photo cards 160×120, title art 480×270.
- AI-made art (Midjourney) must be listed as AI-generated in `ASSET_CREDITS.md` and on the project page. Do not attribute it to NASA.

## 8. UI and feel (ages 7 to 12)
Canvas UI (uGUI). Mouse only. Short sentences. Big buttons.
- HUD: resource totals **and per-hour rates**, battery bar, crew-safety bar, clock, Pause/1×/2×/4×.
- Build menu with cost; ghost preview (green/blue valid, orange-with-X invalid + reason).
- Click a building: info panel (inputs, outputs, status, why) + on/off toggle for optional loads.
- Task checklist per chapter. Tutorial hint bubbles (6 hints, each shown once, can be turned off).
- Event banner for alerts. Debrief screen. **Reset / Retry** everywhere. No dead ends.
- Juice (cheap): pop when placing, cable glow when powered, blinking alert icon, twinkling stars, Earth in the sky. Optional CC0 sounds with a Mute button.
- Accessibility: colour plus icon plus text, scalable UI, no timing pressure beyond Pause.

## 9. Testing (do all of it, then fix what fails)
1. **Edit-mode unit tests** (NUnit, in `Assets/_Game/Tests`):
   - Power deficit drains the battery to exactly 0 at the right hour; energy never negative.
   - Load shedding keeps Life Support and Habitat alive while shedding Greenhouse then Communications.
   - A storm alert does **not** reduce solar generation.
   - Greenhouse: powered + watered → food rises; unpowered → no food; water net loss matches the 80% recycling.
   - Placement: footprint, overlap, cost, blocked terrain.
   - Determinism: same inputs → same state.
   - All fixtures parse and have required header fields.
2. **Scenario bot tests** (pure C#, no Editor UI): scripted players run the whole outpost + storm and assert outcomes:
   - `DoNothing` → gentle fail, with a stated reason.
   - `ShedLoads` → win. `StoredEnergy` → win. `Greedy` (keeps everything on) → fails or barely survives.
   - Use these bots to **tune** the balance values until the targets in section 4 hold. Record final values and bot results in `BALANCE_REPORT.md`.
3. **Play-mode smoke test:** load Main scene, place the six buildings by code, fast-forward to the storm, choose an option, reach the debrief. **Zero console errors/warnings** from our code.
4. **Look at it:** take Game-view screenshots (if the CLI supports it) at 1920×1080 and check legibility, overlaps, clipped text, colour-only signals. Fix what you see.
5. **Manual playthrough checklist** in `TEST_PLAN.md` for me and 3 testers (including one child): 5-minute usability script, questions ("What was the battery for? What did the alert mean? Want to play more?"), and a table to record where each tester got stuck.
6. Report exactly what you **ran** and what you could **not** run. Never say "tested" for something you did not run.

## 10. Build and delivery
- Make **Main** and **Prologue** scenes bootable from a Boot/Menu scene. Build **WebGL** (primary, so judges can click a link) and a Windows/macOS standalone if time allows. Check the WebGL build runs without network access to NASA and without any API key.
- Write `README.md`: Unity version, how to open and play, controls, architecture, how to tune values, the demo path (what to click in 3 minutes), known limits. Link the maintained AI-use log and final asset provenance.
- Write `ASSET_CREDITS.md`, `SCIENCE_SOURCES.md`, `DATA_SOURCES.md`, `BALANCE_REPORT.md`, `TEST_PLAN.md`, and `SUBMISSION_NOTES.md` (project summary in 3 short sentences, how it meets the challenge, data used, AI tools actually recorded in the log, disclosure that all project-created art is AI-created, local-impact note for Mississauga/Toronto, demo-video shot list with English subtitle text; use the current SUBMISSION_REQUIREMENTS.md for final demo limits).
- Final check: search the repo and build output for the real NASA key and for `api_key=` followed by a real value. Only `DEMO_KEY` may appear.

## 11. Honesty rules (non-negotiable)
- Label every number: **NASA-derived**, **simplified**, or **fictional balance**. All building kW, kWh, costs, recycling % are fictional. Say so in the Debrief or Info screen.
- Never imply NASA, CSA, Sara Seager or Chris Hadfield endorse the game. No NASA logo as branding. No real-person portraits.
- A radiation warning is not a power blackout. Earth geomagnetic (GST) and ISS dose data are context, not Moon dose.
- Do not invent facts. If you cannot verify a number from a source page, leave it out or mark it fictional.

## 12. Work plan and milestones (commit at each)
| # | Milestone | Time box | Done when |
|---|---|---|---|
| M0 | Inspect project, folders, `.gitignore`, `.env.example`, task list | 30 min | Report of version, packages, blockers |
| M1 | Sim core + unit tests | 2 h | Tests green: deficit, shedding, greenhouse, determinism |
| M2 | Data layer: fetch script, fixtures (real or `synthetic`), loader + test | 1 h | Fixtures load; `DATA_SOURCES.md` started |
| M3 | Grid, placement, HUD, info panel, build menu (placeholder art) | 3 h | Place all six buildings, see live rates |
| M4 | **Greybox end-to-end**: tutorial → outpost → storm → debrief | 2.5 h | A full run is playable start to finish |
| M5 | Bots + balance + `BALANCE_REPORT.md` | 2 h | Two winning paths, one fail, all gentle |
| M6 | Art import + `ArtManifest` + juice + data cards + credits screen | 3 h | Looks like one pixel style; legible at 1080p |
| M7 | Play-mode smoke test, screenshots, polish, fixes | 2 h | Zero console errors |
| M8 | WebGL build, README, submission notes, key search | 2 h | Build runs offline; docs done |
| Buffer | Keep 1 h unused | | |

**Cut order if behind:** sound → juice → second storm-fixture polish → prologue animation → data cards. **Never cut:** sim tests, the storm decision, the greenhouse, Retry, the WebGL build.

## 13. How to report back
Use the task list. At each milestone, post 3 lines: what works, what you ran, what is next. Do not ask me questions unless you are truly blocked or a choice cannot be undone. If blocked, pick the safest default, say which, and continue. At the end, give me: (1) the demo path, (2) test results (ran vs not run), (3) known limits, (4) a list of files I must fill by hand (Midjourney art, demo video).

Start now with M0.
