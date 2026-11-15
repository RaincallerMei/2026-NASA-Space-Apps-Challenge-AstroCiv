# Mapmakers of the Moon — Claude Code build brief

## Mission
Build a working **Unity 2D, top-down pixel-art educational game** for the **2026 NASA Space Apps Challenge: Build a Junior Astronaut Mission Trainer**. Target players: ages **7–12**. Judges should see a **playable engineering/systems game** whose story is carried **by the play itself** (at most **20% of play time** is non-interactive story). The promise: *imagine a Moon base, build it, then discover what it takes to keep real people alive.* Tagline: *Every great civilization starts with a dream.*

**Source challenge:** https://www.spaceappschallenge.org/2026/challenges/junior-astronaut-mission-trainer/

## AI-use tracking — team requirement

Maintain the repository-root `Work Log and Review/AI_USE_LOG.md` after every meaningful AI session, generation or revision. Record the date/time if known, actual tool/model/session, purpose, prompt/reference inputs, method, outputs, created/imported/used/discarded status, and actual human edits/review plus automated checks. Link saved prompts; leave unavailable history marked **not recorded**. Do not treat a commit/file timestamp as a generation time or claim approval/testing that did not happen. Keep the 25-hour board's **AI use** page, asset credits and submission disclosure consistent with the log.

**All project-created art assets are AI-created**, confirmed by the team November 14, 2026. Disclose image-generated masters, their derivatives, procedural graphics produced through AI-authored scripts, and code-drawn placeholders. Distinguish creation methods; Python/Unity rendering is not an image model. Record source → transformations → exports → imported paths. Earlier “some art,” free-pack suggestions, or “Midjourney planned” wording is historical, not current provenance. Keep engine/template material, actual third-party content and NASA/CSA source data credited separately.

## Story: "Every great civilization starts with a dream."
**This is not a resource-management game with a story attached.** It is a story about one person's dream of exploring space. Resource management is how the player *lives* that dream. Every building is a promise to a person. Every number is someone's air, light, water or food. Story reaches the player **through play** (events, choices, crew chat), not long cutscenes. At most **20% of play time** may be non-interactive. Every system should still carry a story reason.

**Three ideas to build around**
| Idea | Question | Where the player feels it |
|---|---|---|
| **Wonder** | What does it feel like to look at the Moon and imagine living there? | The bedroom, the drawing, the sunrise and Earth in the ending |
| **Responsibility** | What does it take to keep a group of people alive in an unfamiliar world? | Power, air, water, food, crew chat, the storm |
| **Possibility** | Could a kid from Mississauga really grow up to be part of humanity's space story? | The Hadfield and Seager moment, the photo cards, the final message |

**Main idea:** A child who loved building civilizations on a computer gets the chance to build humanity's first real lunar settlement.
**Tagline:** *Every great civilization starts with a dream.*

### Chapter 1: The Kid Who Looked Up (tutorial, about 4 min)
- The protagonist (player types a name) is an **11-year-old in Mississauga** who loves strategy games. They spend hours building cities, managing resources, and planning ahead.
- One evening, at a local science event, they hear about **Chris Hadfield** and **Sara Seager**: people from Ontario have flown in space and become leading astronomers. They wonder: *What if I could build a civilization on the Moon or Mars?*
- In high school they draw a lunar base on a map. It is ambitious: a **greenhouse, a laboratory, a power station and a rover garage** (the rover would explore the surface and collect soil and samples).
- Their teacher says: *"A civilization needs more than buildings. It needs people, resources, science and a plan."*
- **Play = the tutorial.** The player places Habitat, Solar Array and Greenhouse on the map, and sees power in vs power out. The Laboratory and Rover Garage are on the drawing but **locked**: "Not yet. We need power and people first." This teaches click-to-place and the first rule: power in must beat power out.
- Time jump: three photo cards (University of Toronto astronomy, a PhD, astronaut training). Say once that this is a **dream path, not a promised route**.

### Chapter 2: The Commander (outpost, about 6 to 7 min)
- Years later, the protagonist is an astronaut, chosen as **commander of a lunar research outpost**. The player takes control.
- Persistent **20x15 grid**. Build **Habitat, Solar Arrays, Battery, Life Support, Communications, Greenhouse**. Manage power, air, water, food. Turn optional loads on and off.
- **The childhood drawing becomes the foundation of the real settlement.** (Stretch: show the drawing as a faint ghost guide on the grid.)
- It is no longer a strategy game: **every decision affects real crew members, and the player can chat with them** (see Crew chat).
- Light unlocks: passing the first power shortage unlocks a second Battery. The first harvest unlocks the Shelter. The Laboratory and Rover Garage stay locked ("later missions").

### Chapter 3: Solar Storm and the First Lunar Sunrise (about 5 min)
- A scripted alert built from **real DONKI data**. Mission control warns. A crew member is outside. The player decides, the base reacts, the outcome changes. The outpost survives its first major emergency.
- After the storm, **the crew gathers at the observation window.** Sunlight creeps over the ridge (the HUD shows solar output climbing: this is the separate lighting system, **not** the storm). **Earth hangs low above the horizon.**
- The commander receives an old photograph: the little lunar settlement they drew in Mississauga.
- The story ends with a message sent to Earth: **"I used to build this world on a computer. Now we are building it together."**

### Crew chat (emotional connection, built to be safe and offline)
- Crew of three, **fictional** (invented names, no real people): **Engineer** (owns power), **Botanist** (owns the greenhouse), **Geologist** (scouts, and is the one outside in the storm). Placeholder names: Priya, Mateo, Yuki.
- Click a crew portrait (or their building) to chat. Each chat shows 2 to 3 **reply buttons**. **No typing and no live AI** (kid safety, works offline, no key exposure).
- Lines **react to base state** (battery low, first sprouts, storm warning) and the crew **remember the player's choices** (for example `calledGeologistInFirst`). Optional: bond hearts 0 to 3 that change only the ending lines, never survival.
- 3 to 4 short lines per crew member, plus storm lines. Each line max **14 words**.

### Story through play
| Mission event | Gameplay | In this build? |
|---|---|---|
| First habitat setup | Choose where to build (terrain, sun fraction) | Yes |
| Power shortage | Balance electricity between systems | Yes |
| First greenhouse | Grow experimental crops; water/food loop | Yes |
| Solar storm warning | Protect the crew and equipment | Yes |
| Rover expedition | Explore nearby lunar regions | **Later** (Rover Garage is a locked slot) |
| Communication delay | Decide before Earth replies | **Later** (matters most for Mars) |

Why this fits a Civilization-style game: buildings give real services; expanding the base creates new responsibilities; systems depend on each other; events force hard choices; a small outpost grows into a working community.

### Story limits and honesty (non-negotiable)
- Total story + chat text **under about 650 words**. Max **14 words per on-screen line**. Reading level grade 3 to 4. Put all text in one `Story.json`.
- **Sara Seager** (Toronto-born exoplanet scientist; former TESS **Deputy** Science Director) and **Chris Hadfield** (Canadian astronaut) are **inspiration only**. Names only. **No portraits, no quotes, no endorsement.** Verify every fact about them on their official pages before it goes in the game.
- The path from astronomy to astronaut is a **fictional, aspirational dream**, not a promised career route.
- The local science event, teacher and crew are **fictional**. Do not name real organizations unless verified.
- **Earth does not rise or set for most spots on the Moon.** Write "Earth hangs low above the horizon", not "Earthrise", unless a NASA page confirms it for the outpost site.
- The storm and the sunrise are **separate**: a solar warning never switches panels off.

## Design rules
1. **Engineering first:** every building has inputs, outputs, costs, limits, or dependencies. No decorative-only core buildings.
2. **Play before explanation:** story event -> engineering problem -> player decision -> visible consequence -> one-sentence science takeaway.
3. **Systems stack:** mechanics introduced earlier continue to matter later. One persistent lunar base, not disconnected levels.
4. **Age-accessible:** mouse-first controls, visual resource meters, short sentences, forgiving recovery, no required physics knowledge.
5. **Honest science:** distinguish NASA-derived facts, researched approximations, and invented balance numbers. Never frame a training event as an actual radiation-dose prediction.
6. **Story lives in the systems:** a building is a promise to a person. When a number changes, show whose air, light, water or food it is.
7. **People before points:** crew members have names, faces and short chats. A failure is explained through what happened to them, gently, with Retry.

## Campaign roadmap (do NOT implement the whole campaign first)
| Stage | Story trigger | Gameplay | In this build? |
|---|---|---|---|
| Ch. 1: The Kid Who Looked Up | A Mississauga kid hears about Hadfield and Seager, then draws a lunar base | Tutorial: click/place Habitat + Solar + Greenhouse; power in vs out; Lab and Rover Garage drawn but locked | **Yes** |
| Ch. 2: The Commander | The adult astronaut commands a lunar outpost | Persistent 20x15 grid: Habitat, Solar, Battery, Life Support, Comms, **Greenhouse**; power/air/water/food; optional loads; crew chat | **Yes** |
| Ch. 3: Solar Storm and the First Lunar Sunrise | Mission control warns of solar activity; a crew member is outside | Shelter, EVA recall, load priorities, reserve power; ending at the observation window | **Yes** |
| Later A: Rover expedition | Scientists spot a promising site | Rover Garage unlocks; route, battery, samples | No. Locked slot + `IExpeditionSystem` stub |
| Later B: Messages from Home | Communication delay | Decide before Earth replies (Moon about 1.3 s one way, Mars minutes: verify first) | No |
| Later C: Next Frontier | Plan a Martian settlement | Mars atmosphere, dust, energy limits | No. `PlanetProfile` Mars stub |

Story moments are short and mostly interactive: one tutorial scene, one time jump, crew chats, event banners, one ending scene. Prioritize player choices, not cinematic scenes.

## Build THIS vertical slice first
**Core playable flow** (12 to 16 minutes total):
- **Ch. 1 tutorial:** Ontario bedroom and the kid's drawing on a map. Player places Habitat, Solar Array, Greenhouse; sees positive/negative resource flow. Lab and Rover Garage are drawn but locked. Teacher line, then a short time jump (three photo cards) to the Moon.
- **Ch. 2 outpost:** persistent top-down **20x15 tile grid**; players place **Habitat, Solar Array, Battery, Life Support, Communications, Greenhouse**. Placement checks footprint, overlap, cost and valid terrain. Select buildings to inspect; enable/disable optional loads. Crew chat available any time. First harvest and first power shortage are milestones.
- **Ch. 3 storm and sunrise:** scripted **solar radiation alert** from real space-weather data. Players recall the outside crew member, activate the shelter, and manage extra electrical demand while protecting life support. Do **not** suggest a solar storm switches off solar panels. At least two viable solutions, then the observation-window ending and an engineering debrief. A separate lighting/shadow scenario may reduce solar output.
- Stretch only after the above runs: drawing ghost overlay on the grid, repair action, rover mini mission, low-sunlight scenario.

## Simulation specification
Use data-driven building definitions (ScriptableObjects or serializable config), with **pure C# simulation logic separated from Unity UI**.
- Simulate in explicit increments of `dtHours`, not frame-dependent resource arithmetic.
- `powerGenerationKW = sum(active generator outputs)`; `powerDemandKW = sum(active loads)`.
- `batteryEnergyKWh = clamp(previous + (generation - demand) * dtHours, 0, capacityKWh)` as a conceptual ideal-battery starting point; implement charge/discharge power limits, cap by available energy, and track unmet critical demand separately. Never allow energy to go negative.
- Habitat requires power and operating life support. Use **oxygen reserve / crew-safety status** as a simplified second system; document its units and assumptions. Building material/supplies are a construction cost.
- Power prioritization: life support and shelter are critical; communication/science/greenhouse can be curtailed. Display exactly why each warning occurs.
- Game includes 1x/pause speed, resource totals plus per-hour rates, selected-building panel, build menu, task checklist, event notification, tutorial hint, reset/retry, success/failure state.
- Use initial **gameplay-tuned** values; put all tunable values in one editable data location. Example: generator 9 kW; essential loads 5 kW; communications 1 kW; greenhouse 4 kW (optional load, part of the basic outpost); 4 kWh starter battery. These are fictional balance numbers, not equipment specifications.
- Event should come from a local, reproducible scenario fixture built from **real DONKI historical event metadata** (see **Real data layer** below). **No live API dependency to play**: the game reads saved fixtures only.

## Real data layer (NASA + CSA)
The team chose to ground the game in real NASA and CSA data. Full source list, endpoints and cautions: `RESEARCH_AGENT_BRIEF.md` → **Data pack**. Details of what was actually tested: `DATA_SOURCES.md` (research agent writes it).

**Principle:** fetch real data **at build time**, trim it, save it as **offline fixtures**, and have the game read only the fixtures. Play never needs internet.

**NASA API key (strict):**
- Key lives only in env var `NASA_API_KEY` (a local `.env`). Fall back to `DEMO_KEY` if unset (30 req/hour, 50 req/day per IP; a personal key gets 1,000 req/hour; over the limit returns HTTP 429).
- Add `.env` and `*.key` to `.gitignore`. Commit `.env.example` containing `NASA_API_KEY=DEMO_KEY`.
- **Never** write the key into code, fixtures, saved URLs, README, logs, or any WebGL/desktop build. Strip `api_key=` from every `query_url` you store.
- The key was shared in a chat. If the repo is public or the key ever shows up in a commit, tell the user to regenerate it at https://api.nasa.gov/.

**Layout:**
```
Assets/_Game/Data/Fixtures/        # committed, offline, small (<1 MB each)
Assets/_Game/Scripts/Data/         # FixtureLoader (pure C#, unit-tested) + DTOs
tools/fetch_data.py                # optional refresh script; reads NASA_API_KEY; idempotent
```
Every fixture file has a header/field: `source`, `query_url` (no key), `retrieved_utc`, `units`, `license_note`, `classification` (`NASA-derived` | `simplified` | `synthetic`).

**Data → gameplay map**
| Data (fixture) | Feeds | Chapter | Label |
|---|---|---|---|
| DONKI FLR + CME + SEP for one historic window (`donki_event_pack.json`) | `SolarAlertScenario` timeline: warning time, flare class text, SEP onset → mission-control messages | Ch. 3 | event = NASA-derived; crew response = fictional balance |
| DONKI GST (Earth geomagnetic storm) | Flavor only, e.g. "aurora over Ontario tonight" in debrief | Ch. 3 | Earth effect; **never** a lunar dose |
| RadLab ISS dose-rate sample (`radlab_iss_sample.csv`, μGy/h, μSv/h) | A "Real ISS reading" comparison card next to the fictional shelter meter | Ch. 3 | NASA-derived reference; ISS is in low Earth orbit, not the Moon |
| LOLA polar illumination grid (`illumination_grid.csv`, 20×15) | `tile.sunFraction` → solar output × fraction; drives the separate low-sunlight scenario; also the sunrise ending | Ch. 2 to 3 | simplified from real map |
| NASA POWER, Mississauga (`power_mississauga.json`) | Ch. 1 card: "Sun in Mississauga vs sun at the Moon's pole" | Ch. 1 | NASA-derived (Earth surface) |
| OSDR study cards (`osdr_cards.json`, 6–10 curated) | Debrief "Real NASA science" card with link | Ch. 2 to 3 | NASA-derived, title/abstract only |
| CSA food toolkit pointers (`csa_food_cards.json`) | Greenhouse/water cards: Tomatosphere, Veg-04A, Deep Space Food Challenge | Ch. 2 | CSA-derived |
| Moon Trek / Mars Trek terrain crop | Tile height/slope → valid placement now, rover cost later | Ch. 2, later | simplified |

**Rules**
1. A solar flare, SEP or CME **warning** adds crew-safety demands (shelter, cancel EVA, extra load). It never switches solar panels off. Low sunlight comes only from the illumination grid.
2. Show a small **"Real data" badge** wherever a number or event comes from a fixture, with source name and date.
3. Add an in-game **Data credits** screen: "Data: NASA DONKI, NASA OSDR RadLab, NASA POWER, LRO/LOLA (PDS), CSA. Not endorsed by NASA or CSA." Credit CSA as "© Canadian Space Agency" when its content is used.
4. NASA media is generally free to use with credit, but **no NASA logo/insignia** and no implied endorsement. Do not attribute AI-made art to NASA. Mark Midjourney art as AI-generated in `ASSET_CREDITS.md`.
5. Do not use portraits of astronauts or scientists. CSA astronaut-image rules are unclear: skip them.
6. If a source cannot be fetched, ship a hand-made fixture with `classification: synthetic` and say so in `DATA_SOURCES.md`. Do not fake a real label.
7. Stretch only: an Editor-only "Fetch latest space weather" button. Off in WebGL builds. Falls back to the fixture on any error.

## Unity + visual direction
- Inspect existing repo and `ProjectSettings/ProjectVersion.txt`; preserve its Unity version and packages if present. If empty, create a Unity 2D project compatible with installed editor; do not silently upgrade.
- Use Unity **Tilemap/Grid, orthographic camera, sprites, Canvas UI**, clean prefabs. Aim for readable **16×16 or 32×32 pixel art**, nearest-neighbor/Point filtering, consistent pixels-per-unit, accessible scaling. Prefer a square top-down grid for the prototype over expensive isometric art.
- A grey Moon surface, compact white/blue/amber habitat modules, solar panels, connections/cables, clear status colors. UI must be legible at 1080p; don't rely on red/green alone.
- **Search and assess** compatible existing art (candidate starting points below); verify the actual downloaded pack's license **on the publisher's page and in its archive** before downloading/importing; rehosted versions may carry different terms. Record creator, URL, license, modifications in `ASSET_CREDITS.md`. Prefer free/CC0 or permissive commercial-use PNG. Do not scrape previews or use assets with unclear rights. If blocked, use tidy procedural placeholder sprites and provide asset integration instructions.
  - Moon surface + solar/habitat/rover: https://aquumgifts.itch.io/cozy-space-planet
  - Station rooms/crew (optional): https://aquumgifts.itch.io/cozy-space-lite
  - UI panels/buttons (CC0): https://kenney.nl/assets/pixel-ui-pack
  - Sci-fi interior 32×32 (optional): https://dithart.itch.io/ditharts-free-sci-fi-tileset
  - Space decorative shapes (CC0, non-pixel fallback): https://kenney.nl/assets/simple-space

## Architecture and tasks
1. Inspect repo/Unity environment; summarize findings, identify blockers; keep existing work.
2. Create project folders: `Assets/_Game/{Scenes,Scripts,Prefabs,Data,Art,UI,Tests}`; make a bootable tutorial (Ch. 1) scene and main (Ch. 2 to 3) scene.
3. Implement resource simulation + tests, building definitions/placement, resource HUD, event logic.
4. Connect UI and readable art, import licensed sprites or placeholders, add tutorial/debrief content.
5. Run Unity batch-mode compile/edit-mode tests if Unity is available; fix errors. If editor is not available, do static checks and explicitly state what remains unverified.
6. Write `README.md` with Unity version, open/play steps, controls, architecture, tuning, demo path, known limits. Add `ASSET_CREDITS.md`, `SCIENCE_SOURCES.md` and `DATA_SOURCES.md`.
7. Data layer: create `.gitignore` entries and `.env.example`; write `FixtureLoader` + DTOs with an edit-mode test that every fixture parses and has its header fields; write `tools/fetch_data.py` (reads `NASA_API_KEY`, strips the key from saved URLs); use the research agent's fixtures if present, otherwise hand-made `synthetic` ones. Wire the "Real data" badge and Data credits screen.

## Definition of done
- Project opens in target Unity version, **Play** starts a functional scene without console errors (verify if editor available).
- Player can place/inspect buildings; resources update predictably; battery drains/charges correctly; critical failures have understandable reasons.
- Solar-alert decision is **interactive** and changes survival/mission outcome; player can succeed through multiple strategies and retry.
- At least one automated test checks power deficit + battery depletion and one checks load-shedding/critical safety.
- A newcomer can play the demo using the README without hand-editing source code.
- The solar alert runs from an offline fixture (real DONKI data, or labeled `synthetic`). Game works with **no internet and no API key**.
- A search of the repo and build output for the user's real key (kept in `.env`) and for `api_key=` followed by a real value returns nothing. Only `DEMO_KEY` may appear.
- One automated test checks that every fixture loads and carries `source`, `retrieved_utc`, `classification`. One checks that an alert (flare/SEP) does **not** reduce solar generation.
- Data credits screen and "Real data" badges are present.
- **Crew chat works:** 3 fictional crew, reply buttons only, lines react to base state, at least one line remembers the player's storm choice. A dialogue test checks no dead ends, every line within the word limit, and total text under about 650 words.
- **Ending plays** after the storm: observation window, sunrise (solar output rises on the HUD), Earth low above the horizon, childhood drawing, final message.
- Lab and Rover Garage show as **locked** slots in the build menu.

**Working mode:** implement, inspect, test, and iterate; do not stop at design documents. Prefer a small end-to-end working experience to ambitious incomplete features. If tooling or asset downloads fail, keep the game playable and report the exact limitation.
