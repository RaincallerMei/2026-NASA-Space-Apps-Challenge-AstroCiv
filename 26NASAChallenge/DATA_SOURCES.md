# Data sources

The game reads only the offline fixtures in `Assets/_Game/Data/Resources/Fixtures/`. Play never needs internet or an API key.
`tools/fetch_data.py` refreshes them (Python 3 standard library only). It reads `NASA_API_KEY` from the environment, falls back to `DEMO_KEY`, and strips `api_key=` from every stored URL. Each run writes `tools/fetch_log.md`.

The first fixtures were fetched live on **2026-11-14** (UTC). `donki_storm_set.json` and the expanded `osdr_cards.json` were fetched on **2026-11-16 02:05 UTC** (15 November, local time). None is synthetic. If a later run cannot reach a source, the script keeps the earlier real fixture; only if none exists does it write one with `classification: synthetic`.

Every fixture carries `source`, `query_url` (no key), `retrieved_utc`, `units`, `license_note`, `classification`. The EditMode test `DataTests.EveryFixture_Parses_AndHasRequiredHeaderFields` checks this, and `NoFixture_ContainsAnApiKey` checks for `api_key=`.

| Fixture | Source | Query (key removed) | Units | Classification | Feeds (chapter) |
|---|---|---|---|---|---|
| `donki_event_pack.json` | NASA CCMC DONKI: FLR, CME, SEP, GST | `ccmc.gsfc.nasa.gov/DONKI/WS/get/FLR` and `/GST`; `api.nasa.gov/DONKI/CME` and `/SEP`; window 2024-05-10 to 2024-05-12 | UTC times; GOES X-ray class; km/s; Kp | NASA-derived | Storm warning text and real-event card (Ch. 3) |
| `radlab_iss_sample.csv` | NASA OSDR RadLab, ISS DosTel | `visualization.osdr.nasa.gov/radlab/api/?spacecraft=ISS&instrument=DosTel&...2024-05-11T00:00..06:00&format=csv` | absorbed dose rate, µGy/h | NASA-derived | "Real ISS reading" card (Ch. 3) |
| `illumination_grid.csv` | LRO LOLA average illumination `AVGVISIB_85S_060M_201608` (PDS); Mazarico et al. 2011, Icarus 211, 1066–1081 | `imbrium.mit.edu/EXTRAS/ILLUMINATION/IMG/AVGVISIB_85S_060M_201608.IMG` (HTTP Range) | % of time sunlit, 0–100 | simplified | Tile sun fraction and blocked tiles (Ch. 2–3) |
| `power_mississauga.json` | NASA POWER climatology, `ALLSKY_SFC_SW_DWN` | `power.larc.nasa.gov/api/temporal/climatology/point?...latitude=43.59&longitude=-79.64` | kW-hr/m²/day | NASA-derived | "Sun at home, Sun at the pole" card (Ch. 1) |
| `donki_storm_set.json` | NASA CCMC DONKI: 8 real storms (FLR, CME, SEP, GST) | `ccmc.gsfc.nasa.gov/DONKI/WS/get/FLR` 2021-01-01 to 2025-12-31 in 60-day windows, then CME, SEP and GST around each flare | UTC times; GOES X-ray class; km/s; Kp | NASA-derived | Free Build: each storm is a different real event (badge and storm facts); Wiki storm table |
| `osdr_cards.json` | NASA OSDR study search, 12 studies (plants, health, radiation) | `osdr.nasa.gov/osdr/data/search?type=cgene&term=<accession>` | text | NASA-derived | Results screen "Real NASA science" line; Wiki science pages |
| `csa_food_cards.json` | Canadian Space Agency food-production toolkit, 3 pages | `asc-csa.gc.ca/eng/youth-educators/toolkits/food-production/` and linked pages | text | CSA-derived | Greenhouse card (Ch. 2) |

## What each fixture contains

**DONKI.** One historic window chosen because DONKI links the flare to a CME and to particle detections:
X5.8 flare, begin 2024-05-11 01:10, peak 01:23, end 01:39 UTC, active region 13664 (S17W47) → CME 01:36 UTC, 1263 km/s → SEP detections 02:10 UTC (GOES-P SEISS >100 MeV) and 04:07 UTC (STEREO A) → geomagnetic storm on 2024-05-10, Kp 9.
The game uses the flare class, the dates and the order of events. The game's hour-by-hour timeline (warning, onset after 4 game hours, decay, end) and every crew effect are **fictional balance**. The GST entry is an Earth effect only and is never shown as a lunar dose.

**RadLab.** 408 rows from 2024-05-11 00:00–06:00 UTC. Median 4.14 µGy/h (min 1.39, max 28.2). The ISS is in low Earth orbit, inside Earth's magnetic field. The card states that this is **not a Moon dose** and that the game's safety meter is made up.

**LOLA illumination.** A 20×15 crop; each tile is the mean of 2×2 map pixels (60 m pixels, so 120 m per tile). Top-left pixel line 2604, sample 2480. Centre about 89.81° S, 197.5° E, about 5.7 km from the south pole, near the rim of Shackleton crater. Values range from 3.6 % to 71.1 % lit. These are long-term averages, not live shadows. The first CSV data row is the top of the map.
Game mapping (fictional balance, in `Balance.json → terrain`): `sun = 0.5 + 0.5 × min(1, percent / 65)`; tiles below 10 % are blocked, plus a few hand-placed rock tiles. The 10 % construction threshold is a gameplay rule, not evidence that a tile is permanently shadowed; nonzero average illumination means some light is received.

**NASA POWER.** 25-year monthly climatology (January 2001 – December 2025) at an approximate Mississauga point. June 6.08, December 1.17, annual 3.65 kW-hr/m²/day. The card's "about five times more in June than in December" is the ratio 6.08 / 1.17 ≈ 5.2.

**OSDR cards.** OSD-268 (lettuce), OSD-767 (tomato light), OSD-622 (dwarf wheat), OSD-678 (Arabidopsis light), OSD-412 (Veggie leafy greens), OSD-7 (Arabidopsis roots and shoots), OSD-342 (mouse muscle and bone), OSD-401 (mouse muscle splicing). Added 15 Nov 2026 for "Radiation and your body": OSD-366 (cosmic-ray-like radiation and mouse DNA breaks), OSD-658 (seedling DNA repair after simulated cosmic rays), OSD-479 (deep space radiation and thinking, mice), OSD-984 (simulated cosmic radiation plus unloading, mouse muscle). All four are OSDR "Ground Study" entries: the radiation was made on Earth, and the Wiki says so. Title, accession, `where` (OSDR flight program or project type) and link only, with a one-line kid summary written from the title alone.

**DONKI storm set (`donki_storm_set.json`, added 15 Nov 2026).** Selection rule, in `tools/fetch_data.py → fetch_donki_storms`: X-class flares from 2021 to 2025 that DONKI links to both a CME and a solar energetic particle (SEP) event (27 found among 2,476 flares); keep the strongest flare per active region; keep the 8 strongest. Free Build picks one per storm from the map seed and storm number, so the same map always meets the same real storm. Story levels keep the May 2024 event from `donki_event_pack.json`.

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
- The CCMC DONKI web service accepts at most 60 days per request ("Date range cannot exceed 60 days"), so the 2021–2025 flare scan runs in 60-day windows.
- **DONKI particle links are not all prompt.** For 4 of the 8 storms, the first SEP event that DONKI links to the flare comes 17 hours to 2.6 days after the peak. The game quotes the real gap in words ("47 minutes", "about 17 hours", "about 3 days") and never calls it fast arrival.
- **Kp in the May 2024 pack is context, not cause.** The Kp 9 storm began on 10 May 15:00 UTC, before the X5.8 flare (11 May 01:10 UTC). The fact therefore says "Around this storm". In the storm set, Kp is shown only where DONKI links a geomagnetic storm to that storm's CME (3 of 8).
- **Artemis I radiation data (checked 15 Nov 2026):** the RadLab API documentation lists only ISS-related instruments and no Orion/Artemis series. An OSDR search for "Artemis" returned one ground lettuce study (OSD-307). No Artemis I dose dataset was found to embed. The game uses NASA's published Artemis I radiation article and the Nature paper (George et al. 2024, Nature 634, 48–52) as text sources only.

## Terms

- NASA data is generally not copyrighted in the United States. Credit NASA. No NASA logo or insignia and no implied endorsement.
- OSDR study titles belong to their authors; cards link to the study pages.
- CSA content: the fixture records "Non-commercial educational use with credit. No endorsement implied. Commercial use needs written permission." Checked on 2026-11-14 against the [CSA terms page](https://www.asc-csa.gc.ca/eng/terms.asp): educational non-commercial reproduction requires accurate use, source credit, and no implied association/endorsement. Commercial redistribution requires prior written permission, and third-party materials have separate rights. This records the published terms; it grants no logo or third-party-media permission.
- In-game Data credits line: "Data: NASA DONKI, NASA OSDR (studies and RadLab), NASA POWER, LRO/LOLA and MGS/MOLA (PDS), CSA. Not endorsed by NASA or CSA." The Wiki page **Science → Real data in this game** repeats this as a table inside the game.

## The 8 real storms in `donki_storm_set.json`

From the fixture. Particle gap = first DONKI-linked SEP event after the flare peak. Free Build quotes these numbers in its storm facts; game timing stays made up.

| # | Flare | Peak (UTC) | Active region | CME speed (km/s) | First linked particles after peak | Linked Kp |
|---|---|---|---|---|---|---|
| 1 | [X5.8](https://ccmc.gsfc.nasa.gov/DONKI/view/FLR/30711/-1) | 2024-05-11T01:23Z | 13664 | 1263 | 47 min | none linked |
| 2 | [X5.1](https://ccmc.gsfc.nasa.gov/DONKI/view/FLR/42610/-1) | 2025-11-11T10:04Z | 14274 | 1856 | 16 min | 7.33 |
| 3 | [X5.0](https://ccmc.gsfc.nasa.gov/DONKI/view/FLR/28419/-1) | 2023-12-31T21:55Z | 13536 | 2464 | 2 days | none linked |
| 4 | [X4.5](https://ccmc.gsfc.nasa.gov/DONKI/view/FLR/33428/-1) | 2024-09-14T15:29Z | 13825 | 1070 | 3 days | 7.67 |
| 5 | [X3.3](https://ccmc.gsfc.nasa.gov/DONKI/view/FLR/29062/-1) | 2024-02-09T13:14Z | 13575 | 2534 | 2 h | none linked |
| 6 | [X3.3](https://ccmc.gsfc.nasa.gov/DONKI/view/FLR/34148/-1) | 2024-10-24T03:57Z | 13869 | 1606 | 3 days | none linked |
| 7 | [X2.8](https://ccmc.gsfc.nasa.gov/DONKI/view/FLR/28145/-1) | 2023-12-14T17:02Z | 13514 | 1081 | 17 h | 6.0 |
| 8 | [X2.2](https://ccmc.gsfc.nasa.gov/DONKI/view/FLR/35510/-1) | 2024-12-08T09:06Z | 13912 | 677 | 74 min | none linked |
