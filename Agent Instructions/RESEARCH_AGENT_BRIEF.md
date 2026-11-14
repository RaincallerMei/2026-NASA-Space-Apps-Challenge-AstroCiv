# Mapmakers of the Moon — research agent brief

## Purpose
Supply a Unity 2D implementation agent with a compact, verifiable science/design research pack for **Mapmakers of the Moon**, a 2026 NASA Space Apps **Junior Astronaut Mission Trainer** for ages 7–12. Core mechanic: a persistent lunar base whose power, life support, hazard response, food/water, and expedition systems interact. Story is carried **by the play** (<=20% of play time non-interactive); resource management is how the player lives the dream. Tagline: *Every great civilization starts with a dream.* Read `CLAUDE.md` for implementation scope and the full story.

## Context to preserve
- **Narrative (3 chapters):** (1) *The Kid Who Looked Up*: an 11-year-old in Mississauga who loves strategy games hears about Hadfield and Seager at a local science event, then draws a lunar base (greenhouse, laboratory, power station, rover garage); the teacher says a civilization needs people, resources, science and a plan. (2) *The Commander*: years later the astronaut commands a lunar research outpost; the childhood drawing becomes the real base; the player manages power, air, water, food and **chats with three fictional crew**. (3) *Solar Storm and the First Lunar Sunrise*: a real-data alert, a crew member outside, a survived emergency, then the crew at the observation window. Final message: "I used to build this world on a computer. Now we are building it together." The astronomy PhD and astronaut path is a **fictional dream**; selection is not guaranteed in real life.
- **Three ideas:** Wonder, Responsibility, Possibility (see `CLAUDE.md` → Story).
- **Two inspirations:** **Sara Seager** (born/raised in Toronto; exoplanet scientist; former TESS Deputy Science Director) represents discovery and astronomy. **Chris Hadfield** represents Canadian spaceflight and mission operations. Do not misstate Sara as overall TESS director or suggest an astronomy PhD guarantees astronaut selection. No portraits, quotes or endorsement.
- **Audience:** children 7–12; simple English, low punishment, visible cause/effect, short science notes. Judges should see engineering decisions **and** a story that is felt through play, not a visual novel.
- **Scope:** research should make **Ch. 1 tutorial + Ch. 2 outpost (greenhouse included) + Ch. 3 solar storm and sunrise** easy to implement now. Rover expedition, communication delay and Mars are later (locked Lab and Rover Garage slots only).

## Story facts to verify (primary sources only; mark each `verified` or `unverified`)
| # | Claim the story wants to make | What to check | Safe wording if unverified |
|---|---|---|---|
| S1 | Hadfield is an Ontario-connected Canadian astronaut | CSA/NASA biography: hometown and flights | "A Canadian astronaut" |
| S2 | Seager is Toronto-born, a leading exoplanet scientist, former TESS Deputy Science Director | saraseager.com biography; tess.mit.edu team page | "A Toronto-born exoplanet scientist" |
| S3 | "People from Ontario flew in space and became leading astronomers" | Keep only the parts S1 and S2 support | Split into two separate true statements |
| S4 | Real route to astronaut | CSA requirements page (in the table above). Say it is hard and not guaranteed | "Many steps. Study hard." |
| S5 | Earth appears low above the horizon at a lunar outpost | NASA page on Earth's apparent position/libration seen from the Moon. Do **not** call it "Earthrise" unless confirmed | "Earth hangs in the sky" |
| S6 | Sunrise at the outpost site | Lunar day is about 29.5 Earth days; at the poles the Sun skims the horizon (found on NASA SVS 4893). Confirm with the illumination data and keep the wording vague | "Sunlight returns" |
| S7 | Earth-Moon radio delay about 1.3 s one way; Mars minutes | NASA page. Needed only for the later "messages from home" chapter | Leave out |
| S8 | Greenhouse and water recycling in space | CSA food toolkit and linked pages | Use only sentences from the page |
| S9 | The local science event, teacher, crew | Fictional. Do not name real organizations | Keep generic |

## Required deliverables (concise, source-backed)
1. **`SCIENCE_SOURCES.md`:** 8–15 authoritative primary links, each with a one-sentence gameplay use, date accessed, and key factual constraint.
2. **`SCIENCE_ASSUMPTIONS.csv`** (or table): variable, unit, candidate value/range, source, classification (**NASA-derived / simplified / fictional balance**), confidence, and where it appears in gameplay. Never invent real-world power or radiation specifications without clear labeling.
3. **`SCENARIOS.md`:** 3 player-facing test cases with starting setup, decision choices, outcome calculations, science takeaway, and difficulty. At least one successful alternate solution per scenario. Focus on power shortage, radiation alert, and low sunlight; keep solar weather distinct from power generation.
4. **`ASSET_CANDIDATES.md`:** 5–10 pixel-art sources, direct URL, actual asset contents and dimensions, perspective, license/attribution, permitted modification/distribution, style match, and **one recommended cohesive stack**. Link publisher pages, not image-search previews. Flag access/payment issues and AI-generated content where disclosed.
5. **`PRECEDENTS.md`:** a short verified comparison of past Space Apps examples. Each row: what the team built -> the exact mechanic we can adopt -> what we will do differently. Do not make unsupported award claims.
6. **`PLAYER_LEARNING.md`:** 5 measurable learning outcomes (e.g. predict battery depletion, identify critical vs optional loads); a 5-minute usability test for ages 7–12; accessibility notes. Include **one story-linked outcome** (e.g. a child can name one real first step toward becoming an astronaut or astronomer) and a **crew-chat check** (can a 7-year-old finish a chat with buttons only, and say how the crew felt?).
7. **`DATA_SOURCES.md` + `Assets/_Game/Data/Fixtures/`:** for every source in the **Data pack** section below: one tested query URL (**key removed**), HTTP status, date, a <=20-line sample, fields used, units, terms/license note, fixture path, and the gameplay variable it feeds. If a call fails, record the exact error and ship a hand-made fixture marked `synthetic`. Never present a failed or untested call as verified.

## Priority science research and official entry points
| Priority | Topic to verify | Primary starting source | Gameplay question |
|---|---|---|---|
| P0 | Official challenge constraints | https://www.spaceappschallenge.org/2026/challenges/junior-astronaut-mission-trainer/ | What must the demo teach and show? |
| P0 | Electricity generation/storage | https://www.nasa.gov/lunar-surface-technology/ | Why do solar power and storage compete with loads? |
| P0 | Lunar south-pole lighting and terrain | https://www.nasa.gov/reference/moonbase-environment/ and https://trek.nasa.gov/moon/ | How do shadows/location affect operations? |
| P0 | Crew air, oxygen, water system dependencies | https://www.nasa.gov/reference/environmental-control-and-life-support-systems-eclss/ | What fails if critical life support loses power? |
| P0 | Space weather event metadata | https://api.nasa.gov/ (DONKI) and https://ccmc.gsfc.nasa.gov/tools/DONKI/ | What alerts can motivate a training radiation scenario? Do not infer lunar dose directly. Endpoints + key rules: see **Data pack**. |
| P0 | Real dose measurements (comparison only) | https://visualization.osdr.nasa.gov/radlab/gui/data-api/ | What does a real ISS dose rate look like, so kids see a true number next to the fictional game meter? |
| P0 | Lunar polar illumination (low-sunlight scenario) | https://pgda.gsfc.nasa.gov/products/69 | Can a real illumination map set each tile's sun fraction? |
| P1 | Plants and biological research | https://science.nasa.gov/biological-physical/data/osdr/ | How can greenhouse/resource loops be simplified honestly? |
| P1 | Astronaut career accuracy | https://www.asc-csa.gc.ca/eng/astronauts/how-to-become-an-astronaut/requirements-and-conditions.asp | What is the real selection pathway? |
| P1 | Sara Seager biography and role | https://www.saraseager.com/personalbiography and https://tess.mit.edu/tess-team/ | How to portray inspiration accurately without endorsement? |
| P1 | NASA image/logo policies | https://www.nasa.gov/nasa-brand-center/images-and-media/ | What illustrations/logos can be used? |

## Data pack: team-chosen NASA/CSA sources, plus extras
**Rule:** real data is fetched at **build time**, trimmed, and saved as **offline fixtures** in `Assets/_Game/Data/Fixtures/`. The game never calls a live API to play. Each fixture records `source`, `query_url` (no key), `retrieved_utc`, `units`, `license_note`, `classification`.

**Key rule:** the NASA key is read from env var `NASA_API_KEY` only. Never write it into any file, URL, log, or build. If unset, use `DEMO_KEY`. Limits (api.data.gov manual): personal key **1,000 requests/hour**; `DEMO_KEY` **30/hour and 50/day per IP**; over the limit returns **HTTP 429**; check `X-RateLimit-Remaining`. https://api.data.gov/docs/developer-manual

**Sandbox note (2026-11-14):** the cloud shell that prepared this brief got HTTP 403 from its proxy on every NASA host. Endpoints below come from official pages plus third-party mirrors. They are **not live-tested** and the key was **not tested**. First task: run each query once, record the result in `DATA_SOURCES.md`.

### A. Sources the team supplied
| # | Source | Endpoint / entry (how verified) | Game use | Caution |
|---|---|---|---|---|
| A1 | **NASA Open APIs** | https://api.nasa.gov/ . Pass `api_key=` as a query parameter | Gateway for DONKI, APOD, others | Key rules above |
| A2 | **DONKI** (space weather) | `https://api.nasa.gov/DONKI/{FLR,SEP,CME,CMEAnalysis,GST,IPS,MPC,RBE,HSS,WSAEnlilSimulation}?startDate=yyyy-MM-dd&endDate=yyyy-MM-dd&api_key=…`; `/DONKI/notifications?...&type=all\|FLR\|SEP\|CME\|IPS\|MPC\|GST\|RBE\|report`. Default window is the last 30 days. Direct host: `https://ccmc.gsfc.nasa.gov/DONKI/WS/get/<TYPE>`. Event types confirmed on the CCMC page; URL patterns seen on CCMC plus third-party mirrors | Ch. 2 alert: pick **one historic window** with a flare + CME + SEP. Use flare class, times, SEP onset as mission-control text. Candidate window to check first: the May 2024 solar storm period. Confirm by query, do not assume | Flare/SEP/CME are warnings, not lunar dose. GST is an Earth geomagnetic index. None of them turns solar panels off |
| A3 | **OSDR** (space biology, astronaut health) | Search: `https://osdr.nasa.gov/osdr/data/search?term=KEYWORD` (team-supplied; `from`, `size` seen on a third-party listing). Files: `https://osdr.nasa.gov/osdr/data/osd/files/{OSD_ID}`. Biological Data API: `https://visualization.osdr.nasa.gov/biodata/api/v2/query/metadata/…` (NASA OSDR tutorial) | Ch. 2–3 "Real NASA science" cards. Try keywords: plant growth, bone, muscle, radiation, microgravity. Curate 6–10 studies | Use title + one-line kid summary + link only. Rate limit and terms not found: read OSDR terms |
| A4 | **RadLab** (ISS dosimetry) | Base `https://visualization.osdr.nasa.gov/radlab/api`, GET. Filters: `spacecraft`, `instrument`, `timestamp`, `absorbed_dose_rate` (μGy/hour), `dose_equivalent_rate` (μSv/hour), `flux`. `format=json\|csv`. Doc example: `?spacecraft=ISS&instrument=DosTel&timestamp>=2022-04-01T23:00&absorbed_dose_rate>2&format=json`. Verified on the RadLab data-API page | Ch. 2: a "Real ISS reading" comparison card beside the fictional shelter meter. Save 100–500 rows | ISS is in low Earth orbit, inside Earth's magnetic field. Not the Moon. Page states no license; it links to OSDR terms, so check them |
| A5 | **Mars Trek** | https://trek.nasa.gov/mars/ . Page text lists **no** layers, downloads, or API | Ch. 5 outpost terrain: crop one DEM tile to a 20×15 height/slope grid | **Unverified**: agent must find the real tile/download method in Trek docs. Terms not stated |
| A6 | **Moon Trek** (companion, already in brief) | https://trek.nasa.gov/moon/ . Same limits as A5 | Ch. 1 terrain and valid-placement slope | Same as A5 |
| A7 | **CSA: STEM and youth resources** | https://www.asc-csa.gc.ca/eng/youth-educators/resources-stem-and-youth-organizations.asp . Lists: Indigenous stories about the Moon, Spotting the ISS, AuroraMAX, space careers, astronaut profiles, history of Canada in space, open data, astronautics vocabulary, printables | Canadian hooks for story/local impact; glossary; debrief links. Idea: AuroraMAX links a solar alert to real northern lights | Reuse: non-commercial educational use with credit "© Canadian Space Agency", no implied endorsement; commercial use needs written permission; third-party content needs its owner's permission (CSA terms page). **Astronaut image rules unclear: skip portraits** |
| A8 | **CSA: Food production toolkit** | https://www.asc-csa.gc.ca/eng/youth-educators/toolkits/food-production/ . Modified 2026-05-26. Reading list (6 pages), 10 activities for Grades 6–9 (e.g. Interplanetary Farming, Master Chef: Mars), Tomatosphere, Veg-04A experiment, Deep Space Food Challenge, a water-recycling video | Ch. 3 greenhouse and water loop: cards and design hints, Canadian science | The page is mostly links and has little Moon-specific fact. Pull any number from the linked page and cite that page |

### B. Extra open data the game needs (agent found; verify before use)
| # | Source | Entry | Game use | Caution |
|---|---|---|---|---|
| B1 | **LRO/LOLA lunar polar illumination** (PDS) | https://pgda.gsfc.nasa.gov/products/69 . Illumination maps of the poles averaged over hourly steps across the 18.6-year cycle. 240 m/px (65–90°S and 80–90°S) and 60 m/px (85–90°S). Also sky-visibility maps. Cloud-Optimized GeoTIFF links added 2025-03-20 | Bake `illumination_grid.csv` (20×15): fraction of time lit per tile → solar output multiplier in the low-sunlight scenario | Cite Mazarico et al. 2011, Icarus 211, 1066–1081. Averages, not live shadows. Large files: crop offline |
| B2 | **NASA SVS south-pole illumination** | https://svs.gsfc.nasa.gov/5027 (80°S to pole, 2025–2028, two-hour steps) and https://svs.gsfc.nasa.gov/4893 (shadows over two lunar days) | Art and timing reference for the shadow scenario. Fact: at the poles the Sun stays within about 1.5° of the horizon, so shadows are long | SVS says it does not store the underlying datasets: use as visuals/facts, not numbers. Credit "NASA's Scientific Visualization Studio" |
| B3 | **LROC illumination** | https://lroc.im-ldi.com/visit/exhibits/1/gallery/41 | Science card: the Moon's axis tilts about 1.54°, so no seasons; some peaks are lit over 80% of a year while crater floors can be in permanent shadow | Exhibit page, not a dataset |
| B4 | **NASA POWER** (Earth solar and weather) | `https://power.larc.nasa.gov/api/temporal/daily/point?parameters=ALLSKY_SFC_SW_DWN,T2M&community=RE&latitude=43.59&longitude=-79.64&start=YYYYMMDD&end=YYYYMMDD&format=JSON` (pattern from POWER's own docs and llms.txt; Mississauga coordinates are approximate, confirm). No key. Solar data from 1984 to within about a week of now | Prologue/Ch. 1: "Sun in Mississauga vs sun at the Moon's pole". Local-impact hook | Earth-surface irradiance. Read the unit from the response. Never use as lunar power |
| B5 | **NASA image and media rules** | https://www.nasa.gov/nasa-brand-center/images-and-media/ | Rules for any NASA picture or sound | NASA media is generally not copyrighted in the US. Credit NASA. Logos/insignia are not free. No implied endorsement. Do not attribute AI output to NASA. Labeling AI images is encouraged |
| B6 | **NASA NTRS API** | https://ntrs.nasa.gov/api/openapi/ . Root `https://ntrs.nasa.gov/api` | Find official papers behind science cards | Credit line in its docs: "Data provided by NASA Scientific and Technical Information Program" |
| B7 | **APOD** (Astronomy Picture of the Day) | `https://api.nasa.gov/planetary/apod?api_key=…` | Optional loading-screen picture | Some APOD items carry a `copyright` field: **skip those**. Use only items with no third-party copyright |
| B8 | **NOAA SWPC storm scales** (S1–S5 radiation, G1–G5 geomagnetic) | **Not researched yet.** Agent: find the official scale tables | Could set the in-game alert level names | Mark `unverified` until read from the official page |
| B9 | **PDS Search API** | https://pds.nasa.gov/api/search/1.0/ (seen in an ESA slide deck; unverified) | Backup way to find LOLA/Mars products | Check docs |

### C. Still open
- Does the personal key work against DONKI and POWER? (not tested)
- Exact tile/download method for Moon Trek and Mars Trek.
- Terms for RadLab and OSDR data re-use.
- Which single DONKI window gives a clean flare + CME + SEP story. Pick one, record why.
- Whether `ALLSKY_SFC_SW_DWN` unit is kWh/m²/day or W/m² in the response.

## Prior-game inspirations to verify
- **PureFlow (2025 Best Mission Concept):** 3D space habitat design and testing against real solar-weather threats. https://www.nasa.gov/learning-resources/stem-engagement-at-nasa/nasa-announces-2025-international-space-apps-challenge-global-winners/ . Borrow engineering **design -> test -> improve**.
- **Space Travelers (2021 Best Storytelling):** educational game for children about crew hazards. https://2021.spaceappschallenge.org/challenges/statements/the-trail-to-mars-can-you-keep-your-crew-alive/teams/space-travelers-1/ . Borrow playable danger and accessible feedback, not long dialogues.
- **Eco-Metropolis, TerraTales, Connected Earth Museum (2024):** independently verify project pages and award titles before citing; study city-building consequences, scientific visual communication, and concise human stakes. If not verified, mark as *unverified reference*, not award proof.
- **Update 2026-11-14, checked against NASA/Space Apps pages:** Space Travelers = 2021 Best Storytelling (kids 10–14, keep an avatar alive on a Mars trip; four stats; radiation events need action). TerraTales = 2024 Best Storytelling (Cairo, Egypt; challenge "Tell Us a Climate Story!"). 2025: HerCode Space = Best Use of Storytelling ("A Solar Tale"); Zumorroda-X = Art & Technology; PureFlow = Best Mission Concept. **Still unverified:** Eco-Metropolis, Connected Earth Museum.

## Art research: begin with these actual publisher pages
- **Cozy Space Planet**, 16px top-down terrain and props including dome, rover, solar panels, antenna: https://aquumgifts.itch.io/cozy-space-planet (publisher permits game use, no raw pack resale).
- **Cozy Space Lite**, 16px station/crew/stars: https://aquumgifts.itch.io/cozy-space-lite (free game use, no standalone redistribution).
- **Kenney Pixel UI Pack**, pixel UI, **CC0**: https://kenney.nl/assets/pixel-ui-pack
- **Dithart Free Sci-Fi Tileset**, 32px top-down interiors: https://dithart.itch.io/ditharts-free-sci-fi-tileset (verify exact license and whether it pairs visually with 16px assets).
- **Kenney Simple Space**, space symbols, **CC0**, but not necessarily the desired pixel style: https://kenney.nl/assets/simple-space
- Additional discovery: https://itch.io/game-assets/free/tag-space/tag-tileset/tag-top-down . Each candidate requires **asset-level and downloaded-archive license** verification; the Cozy Space Planet listing and a separate OpenGameArt mirror show different license statements, so do not assume the mirror terms apply to an itch.io download.
- **Unity technical reference:** https://docs.unity.com/en-us/engine/6000.6/manual/unity2d/tilemaps/tilemaps (use the docs matching the repo's installed Unity version).

## Research quality rules
- Prefer NASA, CSA, mission teams, original creator pages, and Unity documentation over reposts.
- Distinguish *radiation warning* from *solar power blackout*. A solar energetic particle event does not by itself imply no sunlight. Do not treat Earth geomagnetic storm indices as direct lunar surface radiation dose.
- Mark every statistic with units and where/how derived. Note unresolved questions, do not fill gaps with confident guesses.
- Treat named scientists' careers as biographical context, not their endorsement or authorization to use portraits. Do not use official NASA logos as a game branding element without rights review.
- The finished report should be **implementation-ready and short**: links, facts, values, decision implications; no essay.
- **Data honesty:** every fixture is labeled `NASA-derived`, `simplified`, or `synthetic`. ISS dose rates (RadLab) and Earth storm indices (DONKI GST) are comparison context only. Never turn them into a Moon dose.
- **Key safety:** never write the NASA key into any deliverable, URL, or log. If a tool echoes it, redact before saving.
- Prefer primary pages (NASA, CCMC, OSDR, PDS, CSA). Third-party API mirrors may be used to find a URL pattern, but confirm it with a real call before calling it verified.
