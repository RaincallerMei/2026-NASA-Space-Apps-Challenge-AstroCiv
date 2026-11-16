# AstroCivilization **[Play the game in your browser](https://raincallermei.github.io/2026-NASA-Space-Apps-Challenge-AstroCiv/)**

A Unity space-engineering game for ages 7–12, created for the 2026 NASA Space Apps Challenge (Junior Astronaut Mission Trainer). A child from Mississauga dreams of building a base on the Moon and grows up to command one. The player builds the outpost, balances power, air, water and food, keeps a fictional crew safe, and responds to a solar storm built from real NASA data.

Use a current desktop browser with WebGL support. No Unity installation, GitHub account, NASA API key or sign-in is needed. The game files total about 87 MB; allow the first load to finish. The hosted build was made on 15 November 2026 from the source in this repository. Mobile performance and touch controls are not guaranteed. Earlier public browser checks are recorded in [PUBLIC_ACCESS_CHECK.md](Work%20Log%20and%20Review/PUBLIC_ACCESS_CHECK.md).

## What is in the game

| Part | What the player does |
|---|---|
| **Story Mode** | Two short tutorials, then three missions. Each one unlocks the next. |
| 1. The Dream (tutorial) | Places the first buildings on a childhood drawing of a Moon base. |
| 2. Meet the Crew (tutorial) | Untimed crew training: the crew introduce each building and the time controls. |
| 3. The Commander | Runs a real-time Moon outpost near the south pole until its evaluation. |
| 4. Solar Storm | Protects the crew during a storm based on the real X5.8 flare of 11 May 2024, then watches the lunar sunrise. |
| 5. Mars Base | Builds on real heights around Jezero crater. |
| **Free Build** | Moon or Mars, Easy/Normal/Hard, a new random map each time. Each storm is one of 8 real DONKI solar storms (2023–2025). |
| **Science Facts** | 24 short, sourced facts. Each appears once as a small card the first time something happens (for example, the first Greenhouse or a storm warning) and is kept in the Science Facts collection, where unfound facts show a hint for how to find them. |
| **Wiki** | Buildings, resources, crew, events, map and lessons, plus a **Science** section with the real-data table, the 8-storm table, radiation, plants, life support, the Moon's south pole and Mars. |

Play is real time and pausable. Each mission opens paused on a briefing and ends with a grade (S/A/B/C), a score out of 100 and stars. It stops early, with an explanation and Retry, if the crew runs out of air, water, food or power, or radiation safety fails.

## Controls

Click a building card and then the map to place it. Right-click or Esc cancels placement. Click a building to inspect it, turn it off or on, repair it or remove it. Click a crew portrait to talk; replies are buttons, with no typing. Space pauses or resumes; the speed button cycles 1x, 2x and 4x. Esc with nothing selected opens the pause menu. The **Settings** window (top left on every screen) holds tips, science fact cards, calm motion, music, sound effects, the Wiki and Data & Credits.

## How we built it

- **Engine and code.** Unity 6000.6.7f1, C#. The simulation is pure C# in fixed time steps, separate from the UI, so the same inputs always give the same result. Balance numbers, story text, levels and Wiki pages are editable JSON files in `26NASAChallenge/Assets/_Game/Data/Resources/`.
- **Real data, offline.** `26NASAChallenge/tools/fetch_data.py` fetches NASA and CSA data once, trims it, and saves small fixture files. The game reads only those files, so it needs no internet and no API key.
- **Tests.** 120 EditMode tests (power, storms, end conditions, layout, data, science facts, training, balance bots) and 10 PlayMode smoke tests of the scene flow. All passed in the editor before this build.
- **Order of work.** Planning, story and art in the days before the event, then the main build on 14–15 November 2026: the simulation and tests first, then the playable scenes, Story Mode levels and grades, real-time play, Mars, onboarding and the Wiki, audio and Settings, and finally the NASA-data science facts.
- **AI assistance.** Claude Code and OpenAI Codex wrote most of the code, tests, data scripts and documents under the team's direction. ChatGPT helped with early planning, the art brief and sprite masters. Midjourney made the 7 story pictures. All project art is AI-created. The team supplied the idea, story, design decisions, references and review. Every session is recorded in the [AI-use log](Work%20Log%20and%20Review/AI_USE_LOG.md).
- **Trailer.** Gameplay was recorded from the real game in a browser and edited with Python and FFmpeg (Codex-assisted); no gameplay footage was generated. It also uses NASA's Artemis II crew poster, a Suno-generated soundtrack supplied by the team, and an AI-edited version of one story picture. Details are in [the trailer credits](output/trailer/v4/CREDITS.md).

## Run or rebuild from source

1. Clone the repository and open `26NASAChallenge/` in Unity Hub with **Unity 6000.6.7f1**. Keep existing package versions.
2. Open `Assets/_Game/Scenes/Boot.unity` and press Play.
3. For the browser build, install the matching Web build module, then choose **Mapmakers → Build WebGL (docs/)**. It builds the five enabled scenes into the repository's `docs/` folder, uncompressed for GitHub Pages.
4. Commit the generated `docs/` files with the source. GitHub Pages serves **main → /docs**. Serve WebGL over HTTP(S); opening `docs/index.html` directly as a file will not run the game reliably.

See the [Unity project README](26NASAChallenge/README.md) for the code map and test commands.

## NASA and CSA data

All fixtures are in `26NASAChallenge/Assets/_Game/Data/Resources/Fixtures/`.

| Dataset (fixture) | Source | What it is | Used in the game for |
|---|---|---|---|
| DONKI event pack (`donki_event_pack.json`) | NASA CCMC, Space Weather Database | The 10–11 May 2024 event: an X5.8 flare, a CME at 1263 km/s, energetic particles (SEP) and a Kp 9 geomagnetic storm on Earth | The Solar Storm level's warning and its storm facts |
| DONKI storm set (`donki_storm_set.json`) | NASA CCMC, Space Weather Database | 8 real X-class flares from 2023–2025, each linked to a CME and SEP event | Free Build storms (one real storm per storm), storm facts and the Wiki storm table |
| RadLab ISS dose rate (`radlab_iss_sample.csv`) | NASA OSDR RadLab, ISS DosTel instrument | 408 readings over 6 hours on 11 May 2024, median 4.14 µGy/h | A storm fact comparing a real Space Station reading (not a Moon dose) |
| LOLA illumination (`illumination_grid.csv`) | LRO LOLA, via PDS (Mazarico et al. 2011) | Share of time each spot near the Moon's south pole is sunlit; 20×15 crop of 120 m tiles near Shackleton crater | Each Moon tile's sun level, which sets solar output and which tiles are blocked |
| MOLA Jezero (`mars_jezero_mola.csv`) | Mars Global Surveyor MOLA, via PDS | A 20×15 height map around Jezero crater, about 3.7 km per tile | Mars terrain: steep tiles are blocked, and height affects sun level |
| NASA POWER (`power_mississauga.json`) | NASA POWER climatology | Sunlight at ground level in Mississauga: June 6.08, December 1.17 kWh/m²/day | The "Sun at home" fact at the lunar sunrise |
| OSDR studies (`osdr_cards.json`) | NASA Open Science Data Repository | 12 real studies on plants, muscle and bone, and radiation | The results screen's "Real NASA science" line and the Wiki science pages |
| CSA food cards (`csa_food_cards.json`) | Canadian Space Agency (not NASA) | Tomatosphere, the Deep Space Food Challenge and a growing-food page | Plant facts and the Wiki's plants page |

The text-only science facts also cite NASA pages on life support, radio delay, Artemis I radiation, Mars rovers and MOXIE, and space debris. Each fact lists its source in the game.

**Honest limits**

- **Real data, invented rules.** Power, costs, recycling, air hours, crew safety and event timing are made up for play. This includes the "sun = 0.5 + 0.5 × share lit" rule and the "blocked below 10% sunlit" rule. A storm's order of events is real; its warning hours and crew effects are not.
- **Illumination averages.** The LOLA values are long-term averages, not live shadows.
- **Not a Moon dose.** The ISS reading is from low Earth orbit and is a comparison only. Kp is an Earth storm scale, never a Moon dose.
- **Ground studies.** Four OSDR radiation studies used space-like radiation made on Earth; the Wiki says so.
- **Mars height-to-sun rule.** This is a gameplay assumption, not a lighting model. Mars solar power is softened to 60% of the Moon's.

**API key.** Only the DONKI CME and SEP fetches on `api.nasa.gov` use a key. The key is not in the project: stored URLs have `api_key=` stripped, and the test `NoFixture_ContainsAnApiKey` checks this. To refresh the data, keep a personal key in a local `.env` (gitignored); [`26NASAChallenge/.env.example`](26NASAChallenge/.env.example) holds only the `DEMO_KEY` placeholder.

In-game credit line: "Data: NASA DONKI, NASA OSDR (studies and RadLab), NASA POWER, LRO/LOLA and MGS/MOLA (PDS), CSA. Not endorsed by NASA or CSA." Full source details, query URLs and retrieval dates are in [DATA_SOURCES.md](26NASAChallenge/DATA_SOURCES.md); every science fact's source is in [SCIENCE_SOURCES.md](26NASAChallenge/SCIENCE_SOURCES.md).

## Team documents

- [Submission draft and checklist](Work%20Log%20and%20Review/SUBMISSION_DRAFT.md).
- [29-second Artemis II trailer](output/trailer/AstroCiv_Trailer_29s_v4.mp4) and [capture workflow](Work%20Log%20and%20Review/TRAILER_WORKFLOW.md).
- [Science sources](26NASAChallenge/SCIENCE_SOURCES.md), [data provenance](26NASAChallenge/DATA_SOURCES.md), [science assumptions](26NASAChallenge/SCIENCE_ASSUMPTIONS.csv).
- [Asset credits](26NASAChallenge/ASSET_CREDITS.md), [music and sound candidates](Work%20Log%20and%20Review/AUDIO_CANDIDATES.md) and [AI-use log](Work%20Log%20and%20Review/AI_USE_LOG.md).

## Scope and credits

The simulation uses simplified gameplay values and saved historical data; it is not a mission-planning or radiation-prediction tool. Crew dialogue is scripted and no live AI runs during play. Chris Hadfield, Sara Seager and David Saint-Jacques are named for inspiration and facts only, with no portraits or quotes. Game music (Kevin MacLeod, CC BY 4.0; TAD, CC0) and sound effects (Kenney, CC0) are credited in the game's Data & Credits page and in the asset credits. Third-party and agency materials keep their own terms; public repository access is not a licence for every asset. The team's own code and art are under the [MIT License](LICENSE).

Not endorsed by NASA, CSA, or any named individuals in the game. The astronaut story is half realistic and half fictional and aspirational.
