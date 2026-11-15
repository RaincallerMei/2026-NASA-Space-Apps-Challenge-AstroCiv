# Asset credits — current inventory

Inventory reconciled November 14, 2026. **All project-created art assets are AI-created**, as confirmed by the team: generated illustrations, their derivatives, and procedural art made through AI-authored code. Deterministic rendering/cleanup tools are recorded separately from image models. Engine/template material and NASA/CSA data keep their original provenance. This inventory does not claim every available asset is integrated into the game.

Session history, purpose, creation method, prompt references, known human review and missing details are recorded in the [AI-use log](../Work%20Log%20and%20Review/AI_USE_LOG.md). Image generation tool/model versions, full prompts and session/job IDs remain unrecorded where the log says so.

| Material | Current source / state | Credit or outstanding check |
|---|---|---|
| Unity 2D welcome image `Assets/Welcome/2d-template.png` and template scene/settings | Existing Unity project/template content | Unity Technologies. Template provenance retained; original distribution terms govern this material. Not selected as final game art |
| Existing packages, including URP 2D, uGUI, Input System and test framework | `Packages/manifest.json` and `packages-lock.json` | Unity Technologies and the authors named in each package's LICENSE / Third Party Notices. Package cache is not committed |
| NASA/CSA reference data and study-card text | Six offline fixtures and their metadata | See DATA_SOURCES.md; credit NASA source products and © Canadian Space Agency; no endorsement |
| Pixel illustration in the planning board | Inline canvas drawing in `../Mapmakers 25-Hour Board.html` | **AI-created procedural art** under the team's clarification; exact authoring tool/session not recorded. Not a directly generated raster image |
| Current game tiles, building shapes and placeholder portraits/icons | Procedural drawing/code in the game's UI/map sources | **AI-created through code**; distinguish current placeholders from the exported art pack, whose integration is not verified here |
| Cozy Space Planet / Cozy Space Lite / Kenney Pixel UI | Historical candidates; no selected/imported pack established by this inventory | Not a source for the team's current AI-created art. Do not credit a suggested pack as if it were used |
| Story cutscene pictures (7 files) in `Assets/_Game/Art/Resources/Cutscenes/`: bedroom, classroom, graduation, launch, colony, storm, message | Midjourney; originals in `../Art/cutscenes/`. All seven imported PNGs matched their original bytes by SHA-256 during log reconciliation; see A1–A7 in the AI-use log | **AI-generated.** Existing code labels cutscenes/menu images. Not NASA-made. Existing child-scene review remains open; see SUBMISSION_REQUIREMENTS.md. No new visual compliance check is claimed |
| AstroCiv concept image `../Art/cutscenes/astroCiv idea figure.png` and pack reference `Masters/approved_reference.png` | AI-created per team clarification; generator/session and relationship between these references not recorded | Reference/design inputs; do not label them as gameplay screenshots or invent full prompts |
| AstroCiv pixel asset pack in `../Art/AstroCiv_Asset_Pack/` | Built-in image-generation tool created 10 documented master groups; Python/Pillow/NumPy cleanup, palette mapping, exact geometry and export created the final pack | **All project-created pack art is AI-created.** 142 exported PNGs match `asset_manifest.json`; three atlases, masters and previews are additional. See `GENERATION_NOTES.md` for prompt specifications. **Integrated 14 Nov 2026:** a subset (8 buildings with shadows and Mars dust, rover, astronaut frames, Moon/Mars craters, rocks and ground textures, 14 icons, thumbnails) was cropped with Python/Pillow (transparent margins only, no repainting) into `Assets/_Game/Art/Resources/AstroCiv/` and shown on the square map. Corridors, bitmap font, panels and Laboratory/Command hub/Airlock are not used yet. Final human art approval not recorded |
| Pack UI, icons, bitmap font, corridors, shadows, masks, thumbnails and derivatives | Procedural graphics and transformations from generated masters/AI-authored scripts in `../Art/AstroCiv_Asset_Pack/Tools/` | AI-created art with a **procedural/derived method**; Python itself is not an AI model. The pack reports no bundled external stock art or third-party font file |
| Historical unused Midjourney classroom variant containing `dd730eda` | Mentioned in older credits; not found in the current `../Art/cutscenes/` inventory | Historical provenance note only; not verified as a currently present/imported asset |
| Lock, star, soil, supplies, crew-heart, clock, play/pause icons; failure-scene shapes; ripple rings | Drawn in code (`Scripts/UI/UIKit.cs`, `UIFx.cs`, `EndScreens.cs`); building/resource icons now come from the AstroCiv pack, with code-drawn fallbacks | **AI-created procedural graphics**, not Midjourney-generated sprites |
| Game music and sound effects | Imported November 15, 2026 into `Assets/_Game/Audio/Resources/Audio/` | Third-party, not AI-created. See **Game audio** below for per-file credits |

No NASA/CSA logo or real-person portrait has been added in this pass. The final asset review must inspect the finished game and publicity images.

For each new generation/revision/import, append a session to the AI-use log and record exact files, generator/model if known, actual prompt/reference input, source/master, transformations, output/import paths, job/session ID, selection/review, and used/unused status. Record applicable distribution terms and required attribution without inventing a license. Keep candidates distinct from assets actually used. Carry the all-AI-created-art disclosure into final credits and the project page. Existing submission-review items remain in SUBMISSION_REQUIREMENTS.md.


## Downloaded audio and trailer — November 15, 2026

The team has supplied downloaded music in `Art/BGM/` and Kenney Interface Sounds in `Art/kenney_interface-sounds/` (included license: CC0). These are separate from the AI-created visual-art inventory. Do not infer runtime integration just from file presence.

The locally produced trailer uses **Starry Twinkles**, artist tag **raincalleer**, with embedded Suno generation metadata and song ID `f5182621-6979-42be-864b-69517b18be60`. [Song source](https://suno.com/song/f5182621-6979-42be-864b-69517b18be60). Original location: `Art/BGM/Starry Twinkles.mp3`; final use: `output/trailer/AstroCiv_Trailer_28s.mp4`, excerpt 18–46 seconds with fades/loudness processing. No new audio generation occurred in the capture/edit session. Account tier/model/prompt and release permissions are not established by the tags.

See [trailer credits](../output/trailer/CREDITS.md), [production manifest](../output/trailer/TRAILER_MANIFEST.json) and section 15 of the [AI-use log](../Work%20Log%20and%20Review/AI_USE_LOG.md). The video is recorded gameplay edited with AI-assisted scripts; it is not AI-generated gameplay footage.


## Game audio — integrated November 15, 2026

Five music tracks and 26 Kenney sound effects play in the game. None is AI-generated. The in-game **Data & Credits** page (main menu, and Menu → Data & Credits during a mission) shows the credit text below.

### Music

| In-game file (`Assets/_Game/Audio/Resources/Audio/Music/`) | Where it plays | Title — creator | Source page | License | Original file |
|---|---|---|---|---|---|
| `menu_dream_culture.mp3` | Main menu | "Dream Culture" — Kevin MacLeod | [incompetech.com](https://incompetech.com/music/royalty-free/index.html?Search=Search&isrc=USUAN1300046) | [CC BY 4.0](https://creativecommons.org/licenses/by/4.0/) | `Art/BGM/Main_Page_Dream%20Culture.mp3` |
| `cutscene_space_jazz.mp3` | Story pictures and lessons | "Space Jazz" — Kevin MacLeod | [incompetech.com](https://incompetech.com/music/royalty-free/index.html?Search=Search&isrc=USUAN2100030) | [CC BY 4.0](https://creativecommons.org/licenses/by/4.0/) | `Art/BGM/CutScene_Space%20Jazz.mp3` |
| `game_cipher.mp3` | Gameplay playlist | "Cipher" — Kevin MacLeod | [incompetech.com](https://incompetech.com/music/royalty-free/index.html?Search=Search&isrc=USUAN1100844) | [CC BY 4.0](https://creativecommons.org/licenses/by/4.0/) | `Art/BGM/GamePLay_Cipher2.mp3` |
| `game_cat_caffe.mp3` | Gameplay playlist | "Cat caffe" — TAD | [OpenGameArt: Lofi compilation](https://opengameart.org/content/lofi-compilation) | [CC0](https://creativecommons.org/publicdomain/zero/1.0/) (as stated on the page) | `Art/BGM/GamePLay_Cat caffe.mp3` |
| `game_cue.mp3` | Gameplay playlist | "Cue" — TAD | [OpenGameArt: Lofi compilation](https://opengameart.org/content/lofi-compilation) | [CC0](https://creativecommons.org/publicdomain/zero/1.0/) (as stated on the page) | `Art/BGM/GamePLay_Cue.mp3` |

**Required attribution (CC BY 4.0), as shown in the game:**

> "Dream Culture", "Space Jazz" and "Cipher" by Kevin MacLeod (incompetech.com). Licensed under Creative Commons: By Attribution 4.0 License. http://creativecommons.org/licenses/by/4.0/
> Changes: volume levelled and compressed for the web; gameplay songs play in random order.

TAD's tracks are CC0, so credit is voluntary; it is given anyway. Kenney's CC0 pack likewise.

**Changes made:** files renamed and copied; Unity re-encodes them on import (compressed, quality setting 0.4); playback gain is set per track so all five play at a similar loudness. Audio content is not edited, trimmed or looped beyond Unity's built-in looping of the menu and story tracks.

**Provenance checks done (Nov 15):** the three Kevin MacLeod files carry ID3 title/artist tags matching the credits; the Space Jazz file also carries ISRC `USUAN2100030`. Cipher's file length (3:51) matches the creator listing quoted in `AUDIO_CANDIDATES.md`. The two TAD files have no title tags; their match to the OpenGameArt page rests on the team's download. License pages were checked in the Nov 15 research pass (`AUDIO_CANDIDATES.md`), not re-fetched in this integration pass.

### Sound effects

All from **"Interface Sounds" by Kenney** ([kenney.nl/assets/interface-sounds](https://kenney.nl/assets/interface-sounds)), [CC0](https://creativecommons.org/publicdomain/zero/1.0/). The pack's `License.txt` is kept at `Assets/_Game/Audio/KENNEY_LICENSE.txt`. Files copied unchanged into `Assets/_Game/Audio/Resources/Audio/Sfx/`; Unity converts them to mono on import.

| Game event | File | Weight |
|---|---|---|
| Place a building | `drop_002` + `glass_006` (quiet layer) | Important |
| Pick a building card | `select_005` | Important |
| Select a building on the map | `select_004` | Medium |
| Cannot build / invalid tile / locked card | `error_008` | Medium |
| Remove a building | `minimize_008` | Medium |
| Turn a building on / off | `switch_005` / `switch_002` | Small |
| Fix a building | `drop_004` | Medium |
| Objective done | `confirmation_001` | Medium |
| Warning (dimming, broken panel, brownout, new amber alert) | `error_006` | Medium |
| Danger (solar storm, critical power out, new red alert) | `bong_001` | Important |
| Good news (harvest, new Battery, sunrise) | `glass_001` | Medium |
| Any window opens (pop-ups, crew chat, menu, Wiki & Help) | `maximize_009` | Small |
| Any window closes | `minimize_009` | Small |
| Any button / Back, Close, Cancel | `click_002` / `back_002` | Small |
| Story line next / back | `click_003` / `back_004` | Small |
| Pause / Resume / Speed | `pluck_001` / `pluck_002` / `click_005` | Small |
| Inspection grade shown / mission stopped | `confirmation_004` / `minimize_006` | Important |

Effects were chosen by file name, measured loudness and measured brightness, not by a listening test. On Nov 15 the team reported that the Wiki sounds were sharp; every effect with a spectral centroid above 4 kHz or more than 10% of its energy above 4 kHz was replaced (for example `open_002`/`close_002`: centroid 8.8 kHz, 63% above 4 kHz). All windows now share one open and one close sound. Each volume was set from the file's measured RMS so that small clicks sit near the music level and important actions about 9 dB above it. Only one effect plays per frame (the most important), and warnings repeat at most every 2 seconds. Hover makes no sound.

### Not used in the game

- `Art/BGM/Starry Twinkles.mp3` (Suno-generated, artist tag `raincalleer`) is used only in the trailer; see the section above.
- The other 76 Kenney Interface Sounds files stay in `Art/kenney_interface-sounds/` and are not in the build.
