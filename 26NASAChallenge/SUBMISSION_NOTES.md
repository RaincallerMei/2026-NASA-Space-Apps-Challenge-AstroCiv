# Submission preparation

For current copyable text covering every guide field, use [SUBMISSION_DRAFT.md](../Work%20Log%20and%20Review/SUBMISSION_DRAFT.md) or the board's **Submission desk**. The older planning notes below are historical; [PUBLIC_ACCESS_CHECK.md](../Work%20Log%20and%20Review/PUBLIC_ACCESS_CHECK.md) records the current browser-build/access state.

Draft text based on the agreed briefs and current implementation. **No submission has been made.** The supplied Project Submission Guide PDF has now been read; limits are recorded in [SUBMISSION_REQUIREMENTS.md](SUBMISSION_REQUIREMENTS.md). The separate judging guide, event registration and any earlier local judging deadline remain to be checked.

## Three-sentence project summary

Mapmakers of the Moon is an educational Unity 2D game in development for children aged 7–12. Its planned story follows a child from Mississauga who dreams of space, then commands a lunar outpost and manages power, air, water and food. The implemented simulation uses saved NASA and CSA references to support learning about solar power, batteries, plants and protecting a fictional crew during a solar-particle warning.

## Alignment already chosen in the board

These five headings are listed in the supplied submission guide; the separate detailed judging guide has not been read.

| Topic | Existing plan / evidence | Current limitation |
|---|---|---|
| Impact | Ages 7–12; Mississauga/Toronto connection; learning and usability checks | No player feedback collected |
| Creativity | Childhood drawing becomes the lunar outpost; engineering choices carry the story | Visual story scenes and player flow await Opus |
| Relevance | Planned junior-astronaut engineering tasks; NASA/CSA references tied to existing systems | The original challenge page was unavailable in this check; final challenge compliance needs review |
| Validity | Automated power, placement, life-support, data, dialogue and scenario tests | Full scene PlayMode smoke test and WebGL playthrough pending |
| Presentation | Agreed short demo order, screenshots and credits requirements | Video, cover, final screenshots and playable link pending |

## Data and references used

- NASA CCMC DONKI: historical flare, CME and particle metadata; GST is Earth context only.
- NASA OSDR RadLab: saved ISS absorbed-dose-rate comparison; not a Moon dose.
- NASA POWER: Earth-surface solar climatology at an approximate Mississauga location.
- LRO/LOLA: cropped polar average illumination mapped to simplified tile sunlight.
- NASA OSDR: eight linked space-life-science study cards.
- Canadian Space Agency: Tomatosphere and food-production references. Credit: © Canadian Space Agency.

See [SCIENCE_SOURCES.md](SCIENCE_SOURCES.md), [DATA_SOURCES.md](DATA_SOURCES.md), [SCIENCE_ASSUMPTIONS.csv](SCIENCE_ASSUMPTIONS.csv), and the fixture metadata for dates, units and classification.

Required no-endorsement statement from the existing plan: **Not endorsed by NASA, CSA, Sara Seager or Chris Hadfield. The astronaut path is a fictional dream, not a promised career route.**

Local-impact statement from the agreed audience: the project aims to make space engineering approachable for children in Mississauga and Toronto, using local inspiration and a Mississauga sunlight comparison. Do not claim educational effectiveness before testing it with players.

## AI and tools — recorded use

| Tool | Recorded use |
|---|---|
| Claude Code | Board-recorded simulation, tests, offline data pipeline, story text and playable-game implementation; exact session/model details are incomplete |
| Codex | Board-recorded execution/source checks, tests, documentation and board updates; this conversation also records gameplay advice, the real-time/failure-scene cloud prompt, and AI-use-log reconciliation |
| Unity 6000.6.7f1 and installed packages | Project and automated test execution |
| Python and image-processing libraries | Data fetch/configuration export and deterministic art cleanup, procedural rendering, palette mapping and atlas generation; these tools are not AI models |
| Midjourney | **Used:** seven imported story cutscene pictures, matched to originals in `../Art/cutscenes/` by SHA-256 |
| Built-in image-generation tool | **Used:** 10 documented AstroCiv illustration master groups; generator/model version and exact generation times not recorded |
| AI-authored procedural graphics and derived art | **All project-created art is AI-created**, including UI, icons, font, placeholders and derivatives. The 142-PNG pack exists on disk; this inventory does not establish final Unity integration |

The maintained record is [AI_USE_LOG.md](../Work%20Log%20and%20Review/AI_USE_LOG.md), including purposes, creation methods, evidence, prompt references, imported versus proposed outputs, and missing details. See [ASSET_CREDITS.md](ASSET_CREDITS.md) for art provenance. Historical plans that said only “some art” or “Midjourney planned” are superseded by the team's confirmation that **all project-created art assets are AI-created**. No AI audio/video generation is established by this inventory; do not claim it was used.

Draft project-page wording: “We used Claude Code and OpenAI Codex to assist with code, tests, story text, research checks, documentation and gameplay design. All project-created art is AI-created: Midjourney produced story illustrations, a built-in image-generation tool produced illustration masters, and procedural graphics/image-processing scripts produced additional artwork and derivatives. The team supplied creative direction and references. The game uses scripted dialogue and does not call a live AI model during play. NASA/CSA reference data is credited separately.” Final human review and missing tool/prompt details remain to be recorded.

Retain the visible labels and descriptive/metadata acknowledgements documented in SUBMISSION_REQUIREMENTS.md; final build-wide coverage has not been reverified for this log update.

## Demo requirements already decided

The board's proposed story order is **kid's drawing → outpost and greenhouse → crew chat → storm choice → sunrise → message home**. Opus/team chooses how to fit it into the supplied guide's **30-second maximum video**, or chooses a **maximum seven-slide deck**. The older 60–90 second request must not be used for the submission demo. Record only existing screens; Opus/team prepares narration, subtitle timing and edits. Review child-scene/portrait art against the supplied guide's under-18 likeness clause before final production.

## Remaining team/Opus inputs

| Item | State |
|---|---|
| Submission guide, global window, demo length and AI rules | Read from supplied PDF; see SUBMISSION_REQUIREMENTS.md |
| Detailed judging guide, event confirmation and local deadline | Not verified |
| Playable game link and offline browser test | Not built |
| Final art and asset/license record | Pending |
| Three human testers and their real notes/quotes | Not run |
| Demo recording, subtitles, public video link | Not created |
| Four screenshots and cover image | Pending playable scenes |
| Logged-out link check and actual submission/confirmation | Not done |
