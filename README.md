# AstroCiv: Mapmakers of the Moon

**[Play the game in your browser](https://raincallermei.github.io/2026-NASA-Space-Apps-Challenge-AstroCiv/)**

A Unity space-engineering game for ages 7–12, created for the 2026 NASA Space Apps Challenge. Build an outpost, balance power and life support, support a fictional crew and respond to a space-weather warning. Story Mode and Free Build include Moon and Mars settings.

Use a current desktop browser with WebGL support. No Unity installation, GitHub account, NASA API key or sign-in is needed for the hosted game. The initial Unity download is about 70 MB; allow it to finish. Mobile performance and touch controls are not guaranteed. The current publishing/access evidence is recorded in [PUBLIC_ACCESS_CHECK.md](Work%20Log%20and%20Review/PUBLIC_ACCESS_CHECK.md).

The current WebGL build (November 15, 2026, evening) adds one Settings window on every screen, resource rates that update while paused, and songs levelled to the menu song's loudness. These sit on top of the afternoon build, which brought music, sound effects with a small music player, the Tutorial 2 crew training mission and Back/Next for story text, and the earlier onboarding, wiki and interface work. See the verification record for public browser checks of earlier builds.

## Controls

Click a building card and then the map to place it. Right-click or Esc cancels placement. Select a building to inspect it; click crew portraits to talk. Space pauses/resumes. Use the time controls to change speed. Esc with nothing selected opens the pause menu. Follow each mission's briefing and objectives.

## Run or rebuild from source

1. Clone the repository and open `26NASAChallenge/` in Unity Hub with **Unity 6000.6.7f1**. Keep existing package versions.
2. Open `Assets/_Game/Scenes/Boot.unity` and press Play.
3. For the browser build, install the matching Web build module, then choose **Mapmakers → Build WebGL (docs/)**. The existing editor helper builds enabled scenes into the repository's `docs/` folder with uncompressed WebGL files suitable for GitHub Pages.
4. Commit the generated `docs/` files with the source. GitHub Pages serves **main → /docs**. Serve WebGL over HTTP(S); opening `docs/index.html` directly as a local file will not run the game reliably.

See the [Unity project README](26NASAChallenge/README.md) for the code map and test commands.

## NASA and CSA data

The game reads only offline files: the fixtures in `26NASAChallenge/Assets/_Game/Data/Resources/Fixtures/`. The browser build needs no internet connection and no API key.

| Dataset (fixture) | Source | What it is | Used in the game for |
|---|---|---|---|
| DONKI (`donki_event_pack.json`) | NASA CCMC, Space Weather Database | A real solar event on 10–11 May 2024: an X5.8 flare, then a CME at 1263 km/s, then energetic particles (SEP), then a geomagnetic storm (Kp 9) | The solar storm warning and the real-event card in Chapter 3 |
| RadLab ISS dose rate (`radlab_iss_sample.csv`) | NASA OSDR RadLab, ISS DosTel instrument | 408 readings over 6 hours, median 4.14 µGy/h | The "Real ISS reading" comparison card in Chapter 3 |
| LOLA illumination (`illumination_grid.csv`) | LRO LOLA, via PDS (Mazarico et al. 2011) | The share of time each spot near the Moon's south pole is sunlit. The 20×15 crop has 120 m tiles near the Shackleton crater rim | Each tile's sun level, which sets solar output and which tiles are blocked |
| NASA POWER (`power_mississauga.json`) | NASA POWER climatology | Sunlight reaching the ground in Mississauga: June 6.08, December 1.17 kW·h/m²/day | The "Sun at home, Sun at the pole" card in Chapter 1 |
| OSDR study cards (`osdr_cards.json`) | NASA Open Science Data Repository | 8 real plant and muscle studies (lettuce, tomato, wheat, Arabidopsis, mouse) | The "Real NASA science" card in the debrief |
| MOLA Jezero (`mars_jezero_mola.csv`) | Mars Global Surveyor MOLA elevation data | A 20×15 height map around Jezero crater, about 3.7 km per tile | The Mars level terrain: steep tiles are blocked, and height affects sun level |
| CSA food cards (`csa_food_cards.json`) | Canadian Space Agency (not NASA) | Tomatosphere, the Deep Space Food Challenge and a growing-food page | The greenhouse card in Chapter 2 |

**Honest limits**

- **Real data, invented rules.** The game's balance numbers are made up. This includes the hour-by-hour storm timeline, the crew effects, the safety meter, the "sun = 0.5 + 0.5 × share lit" rule and the "blocked below 10% sunlit" rule.
- **Illumination averages.** The LOLA values are long-term averages, not live shadows.
- **Not a Moon dose.** The ISS reading is from low Earth orbit and is a comparison only.
- **Earth-only storm.** The geomagnetic storm is an Earth effect, never a Moon dose.
- **Mars height-to-sun rule.** This is a gameplay assumption, not a lighting model.

**API key.** Only two fetches use a key: DONKI CME and SEP on `api.nasa.gov`. All other sources need no key. The key is not in the project: stored URLs have `api_key=` stripped, and the test `NoFixture_ContainsAnApiKey` checks this. To refresh the data, keep a personal key in a local `.env` (gitignored); `.env.example` holds only the `DEMO_KEY` placeholder.

In-game credit line: "Data: NASA DONKI, NASA OSDR RadLab, NASA POWER, LRO/LOLA and MGS/MOLA (PDS), CSA. Not endorsed by NASA or CSA." Full source details, query URLs and retrieval dates are in [DATA_SOURCES.md](26NASAChallenge/DATA_SOURCES.md).

## Team documents

- [25-Hour Board](Mapmakers%2025-Hour%20Board.html): open the HTML locally for the interactive task board, collapsible run history and **Submission desk** with editable/copyable drafts.
- [Submission draft and operator checklist](Work%20Log%20and%20Review/SUBMISSION_DRAFT.md).
- [Music and free SFX candidates](Work%20Log%20and%20Review/AUDIO_CANDIDATES.md).
- [28-second gameplay trailer](output/trailer/AstroCiv_Trailer_28s.mp4) and [capture workflow/free editing options](Work%20Log%20and%20Review/TRAILER_WORKFLOW.md).
- [Science sources](26NASAChallenge/SCIENCE_SOURCES.md), [data provenance](26NASAChallenge/DATA_SOURCES.md), [science assumptions](26NASAChallenge/SCIENCE_ASSUMPTIONS.csv).
- [Asset credits](26NASAChallenge/ASSET_CREDITS.md) and [AI-use log](Work%20Log%20and%20Review/AI_USE_LOG.md).

## Scope and credits

The simulation uses simplified gameplay values and historical/offline references; it is not a real mission-planning or radiation-prediction tool. Crew dialogue is scripted and no live AI runs during gameplay. Project-created art and AI assistance are disclosed in the linked records. Game music (Kevin MacLeod, CC BY 4.0; TAD, CC0) and sound effects (Kenney, CC0) are credited in the game's Data & Credits page and in the asset credits. Third-party and agency materials retain their own terms; public repository access is not a blanket license for every asset. The team's own code and art are under the [MIT License](LICENSE).

Not endorsed by NASA, CSA, Sara Seager or Chris Hadfield. The astronaut story is fictional and aspirational.
