# Data sources

The game reads only the offline fixtures in `Assets/_Game/Data/Resources/Fixtures/`. Play never needs internet or an API key.
`tools/fetch_data.py` refreshes them (Python 3 standard library only). It reads `NASA_API_KEY` from the environment, falls back to `DEMO_KEY`, and strips `api_key=` from every stored URL. Each run writes `tools/fetch_log.md`.

All six fixtures below were fetched live on **2026-11-14** (UTC). None is synthetic. If a later run cannot reach a source, the script keeps the earlier real fixture; only if none exists does it write one with `classification: synthetic`.

Every fixture carries `source`, `query_url` (no key), `retrieved_utc`, `units`, `license_note`, `classification`. The EditMode test `DataTests.EveryFixture_Parses_AndHasRequiredHeaderFields` checks this, and `NoFixture_ContainsAnApiKey` checks for `api_key=`.

| Fixture | Source | Query (key removed) | Units | Classification | Feeds (chapter) |
|---|---|---|---|---|---|
| `donki_event_pack.json` | NASA CCMC DONKI: FLR, CME, SEP, GST | `ccmc.gsfc.nasa.gov/DONKI/WS/get/FLR` and `/GST`; `api.nasa.gov/DONKI/CME` and `/SEP`; window 2024-05-10 to 2024-05-12 | UTC times; GOES X-ray class; km/s; Kp | NASA-derived | Storm warning text and real-event card (Ch. 3) |
| `radlab_iss_sample.csv` | NASA OSDR RadLab, ISS DosTel | `visualization.osdr.nasa.gov/radlab/api/?spacecraft=ISS&instrument=DosTel&...2024-05-11T00:00..06:00&format=csv` | absorbed dose rate, µGy/h | NASA-derived | "Real ISS reading" card (Ch. 3) |
| `illumination_grid.csv` | LRO LOLA average illumination `AVGVISIB_85S_060M_201608` (PDS); Mazarico et al. 2011, Icarus 211, 1066–1081 | `imbrium.mit.edu/EXTRAS/ILLUMINATION/IMG/AVGVISIB_85S_060M_201608.IMG` (HTTP Range) | % of time sunlit, 0–100 | simplified | Tile sun fraction and blocked tiles (Ch. 2–3) |
| `power_mississauga.json` | NASA POWER climatology, `ALLSKY_SFC_SW_DWN` | `power.larc.nasa.gov/api/temporal/climatology/point?...latitude=43.59&longitude=-79.64` | kW-hr/m²/day | NASA-derived | "Sun at home, Sun at the pole" card (Ch. 1) |
| `osdr_cards.json` | NASA OSDR study search, 8 studies | `osdr.nasa.gov/osdr/data/search?type=cgene&term=<accession>` | text | NASA-derived | "Real NASA science" debrief card |
| `csa_food_cards.json` | Canadian Space Agency food-production toolkit, 3 pages | `asc-csa.gc.ca/eng/youth-educators/toolkits/food-production/` and linked pages | text | CSA-derived | Greenhouse card (Ch. 2) |

## What each fixture contains

**DONKI.** One historic window chosen because DONKI links the flare to a CME and to particle detections:
X5.8 flare, begin 2024-05-11 01:10, peak 01:23, end 01:39 UTC, active region 13664 (S17W47) → CME 01:36 UTC, 1263 km/s → SEP detections 02:10 UTC (GOES-P SEISS >100 MeV) and 04:07 UTC (STEREO A) → geomagnetic storm on 2024-05-10, Kp 9.
The game uses the flare class, the dates and the order of events. The game's hour-by-hour timeline (warning, onset after 4 game hours, decay, end) and every crew effect are **fictional balance**. The GST entry is an Earth effect only and is never shown as a lunar dose.

**RadLab.** 408 rows from 2024-05-11 00:00–06:00 UTC. Median 4.14 µGy/h (min 1.39, max 28.2). The ISS is in low Earth orbit, inside Earth's magnetic field. The card states that this is **not a Moon dose** and that the game's safety meter is made up.

**LOLA illumination.** A 20×15 crop; each tile is the mean of 2×2 map pixels (60 m pixels, so 120 m per tile). Top-left pixel line 2604, sample 2480. Centre about 89.81° S, 197.5° E, about 5.7 km from the south pole, near the rim of Shackleton crater. Values range from 3.6 % to 71.1 % lit. These are long-term averages, not live shadows. The first CSV data row is the top of the map.
Game mapping (fictional balance, in `Balance.json → terrain`): `sun = 0.5 + 0.5 × min(1, percent / 65)`; tiles below 10 % are blocked, plus a few hand-placed rock tiles. The 10 % construction threshold is a gameplay rule, not evidence that a tile is permanently shadowed; nonzero average illumination means some light is received.

**NASA POWER.** 25-year monthly climatology (January 2001 – December 2025) at an approximate Mississauga point. June 6.08, December 1.17, annual 3.65 kW-hr/m²/day. The card's "about five times more in June than in December" is the ratio 6.08 / 1.17 ≈ 5.2.

**OSDR cards.** OSD-268 (lettuce), OSD-767 (tomato light), OSD-622 (dwarf wheat), OSD-678 (Arabidopsis light), OSD-412 (Veggie leafy greens), OSD-7 (Arabidopsis roots and shoots), OSD-342 (mouse muscle and bone), OSD-401 (mouse muscle splicing). Title, accession and link only, with a one-line kid summary written for the game.

**CSA cards.** Tomatosphere, Deep Space Food Challenge, Growing healthy food in space and remote areas. Short paraphrases and links. Credited "© Canadian Space Agency".

**MOLA Jezero (`mars_jezero_mola.csv`, added 14 Nov 2026).** 20×15 heights in metres from the MGS MOLA MEGDR at 16 pixels per degree (about 3.7 km per tile), fetched with an HTTP Range request (`python tools/fetch_data.py fetch_mola_jezero`; no API key needed). Top-left pixel line 1138, sample 1231, about 18.84° N, 76.97° E, around Jezero crater (centre about 18° N, 77° E, 45 km wide per the JPL Mars 2020 press kit). Classification `simplified`. Game mapping (fictional balance, `Balance.json → mars`): a height step over 300 m to a neighbour blocks the tile; sun = 0.7 + 0.3 × min(1, (height − lowest) / 800 m); tiles below −2640 m are lake mud (soil); a few delta tiles are hand-picked wet clay (water). The height-to-sun rule is a gameplay assumption, not a lighting model.

## Problems met while fetching

- `api.nasa.gov/DONKI/FLR` returned HTTP 404 for this window, while CME and SEP returned 200. The script falls back to the CCMC DONKI web service, which needs no key.
- `api.nasa.gov/DONKI/GST` returned HTTP 429 once (the `DEMO_KEY` rate limit). The CCMC fallback was used.
- `NASA_API_KEY` was not set in the build environment, so `DEMO_KEY` was used.
- `kauai.ccmc.gsfc.nasa.gov` was unreachable; `ccmc.gsfc.nasa.gov` worked.
- The RadLab units documentation page returned HTTP 500 during the initial fetch. Rechecked on 2026-11-14: the [official API field table](https://visualization.osdr.nasa.gov/radlab/gui/data-api/) explicitly gives `absorbed_dose_rate` in µGy/hour and `dose_equivalent_rate` in µSv/hour. This resolves the unit check; the ISS sample remains a comparison, not a Moon dose.
- The LOLA crop position was computed from the label's polar stereographic projection. Orientation was checked only against the known position of Shackleton crater.
- The CSA `pageModified` dates could not be parsed and are stored as empty strings.

## Terms

- NASA data is generally not copyrighted in the United States. Credit NASA. No NASA logo or insignia and no implied endorsement.
- OSDR study titles belong to their authors; cards link to the study pages.
- CSA content: the fixture records "Non-commercial educational use with credit. No endorsement implied. Commercial use needs written permission." Checked on 2026-11-14 against the [CSA terms page](https://www.asc-csa.gc.ca/eng/terms.asp): educational non-commercial reproduction requires accurate use, source credit, and no implied association/endorsement. Commercial redistribution requires prior written permission, and third-party materials have separate rights. This records the published terms; it grants no logo or third-party-media permission.
- In-game Data credits line: "Data: NASA DONKI, NASA OSDR RadLab, NASA POWER, LRO/LOLA (PDS), CSA. Not endorsed by NASA or CSA."
