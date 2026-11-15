# Project submission draft and operator checklist

Prepared 15 November 2026 from `NASA Website/Project Submission Guide.pdf` (all 16 pages read; form/submit pages visually checked), the implemented source, and the existing science/asset records. This is a working draft, not a submitted entry. Text in square brackets needs a real team input. The guide supplies no word/character limits for these text fields; obey any additional limits shown by the live form.

## Project Name

AstroCiv: Mapmakers of the Moon

## High-Level Project Summary

AstroCiv: Mapmakers of the Moon is a story-driven space-engineering game for children aged 7–12, built in Unity. Players turn a dream of exploring space into a working lunar outpost by balancing electricity, air, water and food, supporting a fictional crew, and responding to a solar-storm warning. Saved NASA and Canadian Space Agency resources connect the game's decisions to real space science while keeping its simplified rules clearly separate from real mission engineering.

## Link to Project Demo

[PUBLIC DEMO URL — upload an English slide deck of no more than 7 slides, including title/intro, OR a video of 30 seconds or less. Test without signing in.]

The playable game link is separate from this required explanatory demo. A repository or game alone does not replace the required demo format.

## Link to Final Project

Intended browser game: https://raincallermei.github.io/2026-NASA-Space-Apps-Challenge-AstroCiv/

Source and documentation: https://github.com/RaincallerMei/2026-NASA-Space-Apps-Challenge-AstroCiv

Use the browser address only after the anonymous-access check passes; see `PUBLIC_ACCESS_CHECK.md` for the final verification record. The source repository is a public fallback for inspection, but requires Unity to run locally.

## Detailed Project Description

### What it does

AstroCiv introduces children to the interdependent systems that keep a space outpost running. Its fictional story begins in Mississauga, Ontario, and follows a young person's dream into a future role as commander. Players place buildings on a grid, compare their needs and outputs, and keep a crew supplied with power, air, water and food. A solar-storm scenario asks them to protect the crew and plan reserve capacity. Story missions and Free Build offer guided and exploratory ways to experiment, with a Mars/Jezero setting extending the theme beyond the Moon.

### How it works

The game uses a deterministic C# simulation with building costs, resource production and consumption, terrain/placement rules, a mission clock and scripted events. Players can pause to think and change simulation speed. Crew dialogue is scripted; the game does not call a live AI model or require a player's API key. Evaluation screens explain outcomes and invite another attempt. The WebGL version packages the game for browser delivery; the Unity source is public for inspection and rebuilding.

### How science supports the experience

Historical NASA DONKI space-weather records supply context for the storm scenario. Saved NASA resources also support illumination, Earth sunlight, life-science cards and a Mars terrain example. We distinguish historical observations and source-derived values from simplified gameplay settings. The game is an educational systems model, not a radiation predictor, astronaut qualification tool or realistic habitat-design simulator. A solar warning does not itself switch off solar panels; lighting and radiation hazards are separate concepts.

### Benefits, intended impact and next steps

Our goal is to make tradeoffs tangible: adding a building creates new needs, batteries help with energy gaps, and protecting people matters alongside expanding a base. The Mississauga setting connects a local child's curiosity to a wider space story. Automated simulation and scene checks support development, but we have not established learning outcomes through a formal study. Next steps are beginner playtests, clearer onboarding, accessibility review and careful tuning based on observed confusion.

### Tools and implementation

Unity 6000.6.7f1, C#, JSON configuration and offline fixtures, Python data/documentation/image-processing tools, and a WebGL browser build. GitHub stores the source and GitHub Pages is the intended public host. AI assistance and image generation are disclosed separately below; third-party materials retain their own credits. See the repository's test plan, science assumptions and AI-use log for evidence and limitations.

Not endorsed by NASA, CSA, Sara Seager or Chris Hadfield. The astronaut path is fictional and aspirational, not a promised career route.

## Use of Super Intelligence (SI)

Yes. We used ChatGPT, Claude Code and OpenAI Codex to assist with planning, implementation, tests, research checks, story text, documentation and gameplay design. The project logs record the available tool/model and prompt evidence and mark missing session details. All project-created art is AI-created: Midjourney produced story illustrations, ChatGPT's image tool produced sprite masters, and AI-authored image-processing/procedural scripts produced derivatives and additional graphics. The team provided direction and references; final human review and approval must be recorded rather than assumed. Crew dialogue is scripted, and no live AI runs during play. NASA/CSA source data is credited separately from generated content. Audio tools and tracks researched during this session are candidates only; no AI-generated or downloaded music/SFX is claimed as integrated.

Review before pasting: update this statement if audio is subsequently created or imported. The guide requires visible indicators on generated images/video, and descriptive-text plus metadata acknowledgement for generated audio/code/data. Check actual coverage in the final demo/build. Do not invent missing model versions. The guide says this form answer is used for data collection and is not shown to judges; in-project indicators remain visible to them.

## NASA Data

We use saved NASA resources so gameplay does not require a live NASA API connection:

- **CCMC DONKI:** historical flare/CME/particle-event metadata used for space-weather context and a scripted warning. It is not a live alert or a lunar dose forecast. [NASA CCMC DONKI](https://ccmc.gsfc.nasa.gov/tools/DONKI/).
- **OSDR RadLab:** a saved ISS absorbed-dose-rate comparison card, explicitly separate from lunar conditions. [NASA RadLab](https://visualization.osdr.nasa.gov/radlab/).
- **LRO/LOLA:** saved lunar polar illumination information supporting a simplified tile sunlight example; it is not a site-specific mission guarantee. [LRO LOLA](https://lola.gsfc.nasa.gov/).
- **NASA POWER:** Earth solar climatology for an approximate Mississauga location, used as a familiar comparison rather than lunar sunlight. [NASA POWER](https://power.larc.nasa.gov/).
- **NASA OSDR:** linked space-life-science study cards providing background for the greenhouse theme. [NASA OSDR](https://osdr.nasa.gov/).
- **Mars Global Surveyor MOLA:** saved Jezero-area heights used for a simplified Mars terrain/placement example. [MOLA dataset](https://pds-geosciences.wustl.edu/mgs/mgs-m-mola-5-megdr-l3-v1/mgsl_300x/meg016/).

The repository's `26NASAChallenge/DATA_SOURCES.md`, `SCIENCE_SOURCES.md`, `SCIENCE_ASSUMPTIONS.csv` and fixture metadata provide the specific provenance, retrieval dates, units and distinctions between data and invented balance values. Keep this inventory aligned with the final build; do not describe optional sources as implemented mechanics.

## Space Agency Partner Data and Other References

Canadian Space Agency resources provide Tomatosphere and space-food context, plus factual biographies used as inspiration. Credit: © Canadian Space Agency. NASA/CSA resources and names do not imply endorsement. See [CSA Tomatosphere](https://www.asc-csa.gc.ca/eng/educators/tomatosphere.asp) and the project's science-source record for exact references and reuse notes.

Implementation tools include Unity 6000.6.7f1 and the packages listed in `26NASAChallenge/Packages/manifest.json`; C# and Python; Git/GitHub; ChatGPT, Claude Code and Codex; Midjourney and image-processing tools. The full asset inventory, AI methods and available provenance are in `26NASAChallenge/ASSET_CREDITS.md` and `Work Log and Review/AI_USE_LOG.md`. Engine/template material is credited separately from project-created art. No candidate music or SFX should be listed as used until a specific file is actually integrated and credited.

Before submission, add any final audio, fonts, libraries, demo-editing software or other materials absent from those records. Free/open-source content must still be referenced, and applicable permissions must be retained.

## Demo plan: choose one format

**30-second video outline (record only working screens):**

| Time | Visual | Spoken or caption draft |
|---|---|---|
| 0–2 s | Project and team title | “We're [TEAM NAME].” |
| 2–5 s | Child's drawing / game title | “How can children practise space engineering?” |
| 5–10 s | Place a building, reveal resources | “AstroCiv turns a dream into a lunar outpost.” |
| 10–20 s | Power/greenhouse loop, crew and storm choice | “Balance power, air, water and food. Protect your crew. NASA data connects choices to space science.” |
| 20–28 s | Evaluation, hopeful ending, public game address | “Every great civilization starts with a dream. Try it, learn, and build again.” |
| 28–30 s | Credits/AI labels remain visible | Leave a margin before the 30-second maximum. |

Use the real confirmed team name. Keep important text large, caption speech, retain required generated-content indicators, and verify the exported file's actual runtime. Avoid recording child testers or adding real under-18 voices/likenesses. The guide's broader under-18 likeness wording also needs the team's review against the Terms & Conditions for fictional child art; this session makes no eligibility determination.

**Alternative: up to 7 English slides, including title/intro:** (1) project/team/member names and selected challenge; (2) beginner problem; (3) how the player builds/supports the crew; (4) storm decision and consequence; (5) NASA/CSA data and explicit simplifications; (6) implementation, evidence and limits; (7) public game/source links and next steps. Use real screenshots and concise captions.

## Submission procedure and deadline

1. Confirm that all team members are accepted at the same Local Event. Confirm its time zone and any earlier Local Lead judging deadline. For Universal Event, use the Team Owner's residence time zone.
2. The supplied guide's window is **November 14, 2026 at 9:00 a.m. to November 15, 2026 at 11:59 p.m.** in the applicable time zone. If Toronto is the confirmed applicable zone, the closing time is **Sunday, November 15 at 11:59 p.m. EST**. Aim for 10:59 p.m. as a team buffer, not an official deadline.
3. Sign in at the Space Apps website → profile icon → **My Team** → **Project** → **Edit**. Turn the edit lock on while one person edits; the lock is advisory and can be overridden by teammates.
4. Fill all live required fields using the reviewed draft. Use **Save Progress** frequently. Saved work alone is not a submission and remains team-visible until submitted.
5. Upload images only in JPG/PNG/GIF, up to 10 MB each. Host videos/documents externally; the form does not accept those uploads. Supply public links without a login, password, registration or access request.
6. Test every final link in a private window and ask someone outside the account to open it. Test the game until the menu appears and start a mission. Test demo playback/download as well as the project link. Optional: request FortiGuard classification if the new site is filtered; the guide recommends it but does not require it.
7. Review originality, source/asset credits, permissions, English text, AI indicators and the guide's under-18 likeness clause. Do not claim unperformed playtests or educational effectiveness.
8. The team reviews and accepts **Terms and Conditions** and **Confirm All Work is Original**, then clicks **Submit for Judging** before the deadline. This agent has prepared content; it has not accepted terms or submitted the entry.
9. Verify the confirmation banner/message, save evidence, and check the public page. If making later edits before the deadline, save and resubmit as needed; verify confirmation again. The guide says only the last saved submission is reviewed.
10. After submitting, participant certificates are available through Profile → Participant Information → 2026 NASA Space Apps Challenge. The guide recommends a desktop and Letter paper size for the PDF.

## Remaining real inputs

- Confirmed team/member names, selected event, registration status, local deadline and selected challenge wording.
- Public video/deck link and its verified duration/slide count.
- Final browser-access evidence (see `PUBLIC_ACCESS_CHECK.md`), real human playtest notes if any, and final build review.
- Final asset/AI disclosure review, including any later audio additions.
- Human acceptance of terms, submission action and confirmation.

Source: supplied guide, PDF pages 2–13 and 15; [official guide](https://www.spaceappschallenge.org/resources/project-submission-guide/). The supplied copy is the operative evidence here. The detailed Judging and Awards Guide and live team form have not been inspected in this pass.
