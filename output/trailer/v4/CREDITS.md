# Revised 29-second trailer: credits and provenance

## NASA Artemis II poster

The opening uses the user's [original NASA Artemis II crew poster](https://www.nasa.gov/wp-content/uploads/2024/05/artemis-ii-crew-poster-web-page-1.jpg?resize=1200,783), preserved at [sources/artemis-ii-crew-poster.jpg](../sources/artemis-ii-crew-poster.jpg). [NASA's educational resource listing](https://www.nasa.gov/learning-resources/join-artemis/) and [media-use guidelines](https://www.nasa.gov/nasa-brand-center/images-and-media/) were checked for this noncommercial, factual educational project demonstration. NASA is visibly credited, agency marks remain within the authentic poster, and the project does not claim agency or astronaut endorsement. Educational reuse does not establish permission for commercial advertising or merchandise featuring astronaut likenesses or agency marks. The poster was framed, faded and scaled with ordinary editing; no generative tool recreated it. The background behind it is the actual game home screen.

## Pastel Twinkles

User-supplied **Pastel Twinkles**, artist tag **raincalleer**, generated with **Suno**. Source: `Art/BGM/Pastel Twinkles.mp3`. Embedded provenance: `made with suno; created=2026-11-16T01:54:31Z; id=0a49d446-3f17-465c-8971-c0398cc51905`. [Song page](https://suno.com/song/0a49d446-3f17-465c-8971-c0398cc51905). The 30.01-second source is gently fitted to 29 seconds, normalized and faded. No new music or voiceover was generated. Applicable release rights remain those of the supplied track. AI music provenance appears here and in the MP4 metadata.

## Real gameplay

A current Unity WebGL build was made into `tmp/trailer-v4/web/` to include the new Science Facts collection. It was used only for local capture. The existing published `docs/` build was not replaced. A new isolated Chrome profile was used; normal game/browser saves were not used. Mission-selection prerequisite flags were staged, but no resources, achievements, scores or science-fact unlocks were fabricated. Hints and pop-up fact cards were disabled through the capture profile while the collection and actual unlock triggers remained active.

Chrome recorded continuous game-canvas video at approximately 30 frames per second. The capture-only WebGL context enabled `preserveDrawingBuffer` so the recorded canvas contains real rendered frames. No Unity gameplay code was changed for this trailer. Construction footage is real placement input at normal starting budgets; supply drops and soil availability were reached through native gameplay. The map planner calculated valid positions in an editor-only scratch script; its planning budget was not injected into the captured game. Native 4x simulation was sometimes used to reach later mission states. The construction clips are additionally edited to 4x playback, as requested. Paused preview states are used to make resource and adjacency results readable. This is an edited demonstration montage.

Recorded evidence:

- Greenhouse OFF with the battery below 20%: power **-0.7 to +2.3 kW**, food **+0.1 to -0.4/h**. Battery visibly recovers.
- Greenhouse ON after food naturally falls to **5.6**: food rate **-0.4 to +0.1/h**, with food rising to **6.1**. The source includes continued simulation after the toggle.
- Battery placement: the same Solar Array produces **2.1 kW** with the battery far away and **2.9 kW** after a second battery is placed nearby. These are simplified in-game cable-loss and adjacency rules.
- Science Facts collection shows **18/24** earned through actual construction, harvest, shadow, storm and sunrise triggers, alongside locked cards and their hints. A NASA Scientific Visualization Studio citation remains readable in the selected fact detail.
- Five Free Build maps are Moon seeds **3019, 9594, 9419** and Mars seeds **6967, 3560**, with distinct building layouts. Each montage core uses two seconds of continuous construction source played at 4x for **0.50 seconds**. Four two-frame blends last **0.0667 seconds**, the nearest whole-frame match to 0.07 seconds at 30 fps.

The story mission uses archived NASA illumination and a historical DONKI storm reference; Free Build maps are generated practice maps. Resource rates, building interactions and storm timing are educational game abstractions. Dataset references and assumptions remain in the project's [data sources](../../../26NASAChallenge/DATA_SOURCES.md) and [science sources](../../../26NASAChallenge/SCIENCE_SOURCES.md).

## Ending artwork and AI assistance

All project-created game art is AI-created, as documented in the [asset credits](../../../26NASAChallenge/ASSET_CREDITS.md). The original observation-window cutscene is `26NASAChallenge/Assets/_Game/Art/Resources/Cutscenes/message.png`. To retain the full composition while following the supplied submission guide's restriction on under-18 likenesses, only the communication-monitor classroom was edited to show Moon/Mars drawings without children. The foreground adult crew and Earth/window scene remain visible. The original game asset was not replaced.

Saved derivative: [ending-full-drawings.png](ending-full-drawings.png). Mode: **edit**, built-in `image_gen.imagegen`, based on the existing cutscene. The ending is fitted in full with blurred side fill and a **50% opaque** lower caption panel. Exact generation prompt:

> Use case: precise-object-edit. Edit target: the supplied pixel-art lunar-base observation-window cutscene. Change ONLY the picture displayed INSIDE the cyan-framed communication monitor on the right. Replace all children and other people inside that monitor with a warm, empty classroom displaying several cheerful hand-drawn Moon-base and Mars-base blueprints and drawings on desks and a board. No people or faces anywhere inside that monitor. Preserve the existing cyan monitor frame, its perspective, and its reflected warm light. Everything OUTSIDE the monitor must remain as close as possible to the original: the adult astronaut at left with the Canadian flag, the adult crew in the lower right foreground, lunar landscape, bright Earth through the window, small plants, model rover, walls, exact original composition, lighting, colors and pixel-art texture. Preserve the entire original full landscape composition and its approximately 4:3 aspect ratio. Do not add words, captions, logos or other design elements. This is an aspirational video end-card background; text is added separately in the video editor.

OpenAI Codex assisted with research, capture scripts, shot selection, editing, captions, documentation and technical checks. Python/FFmpeg perform ordinary editing; no synthetic gameplay video was generated. AI art is visibly indicated; music and editing assistance are acknowledged in this file and the MP4 metadata.

## Validation and traceability

All **870 frames** decoded; exactly **29.00 seconds**, **1920 x 1080**, **30 fps**, H.264/AAC. Audio mean **-19.9 dBFS**, peak **-4.0 dBFS**; no sustained black frames. Full-frame samples, both resource changes, the placement comparison and construction motion were reviewed. This is technical/visual QA; human listening approval is not recorded.

[MANIFEST.json](MANIFEST.json) records source/build SHA-256 hashes, exact source trims, speeds, timings and measured checks. [Contact sheet](contact-sheet.jpg). Reusable capture/edit scripts are in `../scripts/`; local render logs and detailed QA frames are in `tmp/trailer-v4/edit/`. Previous cuts are preserved. The video has not been uploaded or submitted by this workflow.
