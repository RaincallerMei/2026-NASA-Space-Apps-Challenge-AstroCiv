# AstroCiv: Mapmakers of the Moon

**[Play the game in your browser](https://raincallermei.github.io/2026-NASA-Space-Apps-Challenge-AstroCiv/)**

A Unity space-engineering game for ages 7–12, created for the 2026 NASA Space Apps Challenge. Build an outpost, balance power and life support, support a fictional crew and respond to a space-weather warning. Story Mode and Free Build include Moon and Mars settings.

Use a current desktop browser with WebGL support. No Unity installation, GitHub account, NASA API key or sign-in is needed for the hosted game. The initial Unity download is about 70 MB; allow it to finish. Mobile performance and touch controls are not guaranteed. The current publishing/access evidence is recorded in [PUBLIC_ACCESS_CHECK.md](Work%20Log%20and%20Review/PUBLIC_ACCESS_CHECK.md).

The WebGL build completed on November 15, 2026 and includes the latest onboarding, wiki and interface source changes. See the verification record for build and public browser checks.

## Controls

Click a building card and then the map to place it. Right-click or Esc cancels placement. Select a building to inspect it; click crew portraits to talk. Space pauses/resumes. Use the time controls to change speed. Esc with nothing selected opens the pause menu. Follow each mission's briefing and objectives.

## Run or rebuild from source

1. Clone the repository and open `26NASAChallenge/` in Unity Hub with **Unity 6000.6.7f1**. Keep existing package versions.
2. Open `Assets/_Game/Scenes/Boot.unity` and press Play.
3. For the browser build, install the matching Web build module, then choose **Mapmakers → Build WebGL (docs/)**. The existing editor helper builds enabled scenes into the repository's `docs/` folder with uncompressed WebGL files suitable for GitHub Pages.
4. Commit the generated `docs/` files with the source. GitHub Pages serves **main → /docs**. Serve WebGL over HTTP(S); opening `docs/index.html` directly as a local file will not run the game reliably.

See the [Unity project README](26NASAChallenge/README.md) for the code map and test commands. NASA/CSA fixtures are included; gameplay does not fetch live data. Optional data-refresh tooling can use an environment-provided key, but a personal key must never be committed.

## Team documents

- [25-Hour Board](Mapmakers%2025-Hour%20Board.html): open the HTML locally for the interactive task board, collapsible run history and **Submission desk** with editable/copyable drafts.
- [Submission draft and operator checklist](Work%20Log%20and%20Review/SUBMISSION_DRAFT.md).
- [Music and free SFX candidates](Work%20Log%20and%20Review/AUDIO_CANDIDATES.md).
- [Audio creation tools and prompts](Work%20Log%20and%20Review/AUDIO_CREATION_PROMPTS.md).
- [Science sources](26NASAChallenge/SCIENCE_SOURCES.md), [data provenance](26NASAChallenge/DATA_SOURCES.md), [science assumptions](26NASAChallenge/SCIENCE_ASSUMPTIONS.csv).
- [Asset credits](26NASAChallenge/ASSET_CREDITS.md) and [AI-use log](Work%20Log%20and%20Review/AI_USE_LOG.md).

## Scope and credits

The simulation uses simplified gameplay values and historical/offline references; it is not a real mission-planning or radiation-prediction tool. Crew dialogue is scripted and no live AI runs during gameplay. Project-created art and AI assistance are disclosed in the linked records. Candidate audio research does not mean those tracks are used in the game. Third-party and agency materials retain their own terms; public repository access is not a blanket license for every asset.

Not endorsed by NASA, CSA, Sara Seager or Chris Hadfield. The astronaut story is fictional and aspirational.
