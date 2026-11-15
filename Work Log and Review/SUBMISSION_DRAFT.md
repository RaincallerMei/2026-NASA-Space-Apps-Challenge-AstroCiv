# Project submission draft and operator checklist

Prepared 15 November 2026 from `NASA Website/Project Submission Guide.pdf` (all 16 pages read; form/submit pages visually checked), the implemented source, and the existing science/asset records. This is a working draft, not a submitted entry. Text in square brackets needs a real team input. The guide supplies no word/character limits for these text fields; obey any additional limits shown by the live form.

## Project Name

AstroCiv: Mapmakers of the Moon

## High-Level Project Summary

Want to explore the Moon and Mars? AstroCiv: Mapmakers of the Moon puts children aged 7–12 in command of a space outpost. Build a home, grow food, balance power and life support, and protect your crew during a solar storm. NASA data connects the challenge to real science; a fictional story beginning in Mississauga, Ontario, makes that adventure personal. Guided missions and Free Build turn curiosity into a cycle of building, testing and improving. Our goal is to help the next generation see themselves as people who can ask scientific questions and solve problems, in space and at home.

## Link to Project Demo

[PUBLIC DEMO URL — upload an English slide deck of no more than 7 slides, including title/intro, OR a video of 30 seconds or less. Test without signing in.]

The playable game link is separate from this required explanatory demo. A repository or game alone does not replace the required demo format.

## Link to Final Project

Intended browser game: https://raincallermei.github.io/2026-NASA-Space-Apps-Challenge-AstroCiv/

Source and documentation: https://github.com/RaincallerMei/2026-NASA-Space-Apps-Challenge-AstroCiv

Use the browser address only after the anonymous-access check passes; see `PUBLIC_ACCESS_CHECK.md` for the final verification record. The source repository is a public fallback for inspection, but requires Unity to run locally.

## Detailed Project Description

### The issue that inspired us: space curiosity needs somewhere to go

A child can dream about the Moon and Mars long before they can explain how people would live there. We built AstroCiv around that gap between being excited by space and having a chance to make scientific decisions for yourself. Energy, clean water, food and caring for people are connected challenges on Earth as well as in space. Helping children explore those connections matters: the next generation will inherit problems that require curiosity, persistence and an understanding of how one decision affects a whole system.

Our story begins with a child in Mississauga, Ontario, imagining a future among the stars. That local beginning gives our project its purpose: make space exploration feel like something a child in our community can take part in, starting with a game they can play today.

### Our solution: put the player in command

AstroCiv: Mapmakers of the Moon is a story-driven space-engineering game for ages 7–12, built in Unity and playable in a desktop browser. Players build an outpost, grow food, balance electricity, air and water, and look after a fictional crew. Guided lessons and crew training introduce the systems; story missions and Free Build let players experiment on the Moon and Mars.

The fun comes from making a plan and discovering whether it works. Where should the solar panels go? Can the base power its air supply and greenhouse? When a solar-storm warning arrives, can you bring the crew inside and power their shelter in time? Players can pause, inspect their base and try a different design. Mission reports explain what worked and what to improve, giving each attempt a reason to become a better one.

NASA data has a visible role in the experience. Saved LRO/LOLA illumination values influence lunar solar output; MOLA heights shape the Mars terrain; historical DONKI events give the storm scenario real-world context. NASA life-science and Canadian Space Agency food resources connect the greenhouse to actual research. Source observations and simplified game rules are identified separately. The simulation is educational, with fictional balance values and storm timing; it does not predict radiation or design real habitats.

Our playable browser demo lets judges build, inspect resources and experience the missions themselves. Placing solar power changes the energy available to air support and food production. In the storm mission, recalling the crew and powering the shelter turns a warning into a protective action. These decisions and their visible consequences demonstrate our central idea: science becomes something the player uses to care for people.

### Why it stands out, who it can help, and where it goes next

AstroCiv brings together a colourful pixel-art adventure, a personal Ontario story, connected resource systems and traceable NASA data. Its distinctive promise is that learning happens inside the decisions that make the game engaging. A greenhouse needs resources; a growing outpost needs planning; a crew needs protection. Players have a reason to understand the science because their next decision depends on it.

For children in Ontario, we hope the journey from a familiar hometown to a space outpost makes a future in science feel imaginable. For families, libraries and classrooms, the browser game could become a starting point for conversations about energy, food, resilience and teamwork. The same habits of asking questions, testing ideas and improving a design matter when caring for our own communities and planet. These are intended benefits; we have not yet measured learning outcomes.

We want to keep developing AstroCiv with children and educators: observe where beginners get stuck, improve readability and accessibility, and develop classroom activities that connect each mission to its science sources. Later missions could explore more resource and environmental tradeoffs. Our ambition is to help a generation move from “I want to explore” to “I can learn how to build, investigate and help.”

Built with Unity, C#, offline NASA/CSA reference files and a WebGL browser build. Source code, data provenance, asset credits and AI-use disclosures accompany the project. NASA and CSA do not endorse AstroCiv; its story and astronaut journey are fictional.

## Use of Super Intelligence (SI)

Yes. We used ChatGPT, Claude Code and OpenAI Codex to assist with planning, implementation, tests, research checks, story text, documentation and gameplay design. The project logs record the available tool/model and prompt evidence and mark missing session details. All project-created art is AI-created: Midjourney produced story illustrations, ChatGPT's image tool produced sprite masters, and AI-authored image-processing/procedural scripts produced derivatives and additional graphics. The team provided direction and references; final human review and approval must be recorded rather than assumed. Crew dialogue is scripted, and no live AI runs during play. NASA/CSA source data is credited separately from generated content. The trailer uses the user-supplied instrumental track Starry Twinkles, whose embedded metadata identifies Suno generation. Its source and AI audio acknowledgement are included in the trailer credits and video metadata. Recording/editing was automated with Chrome and FFmpeg; the footage comes from the real game. Other downloaded audio and any runtime integration are credited separately.

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

Implementation tools include Unity 6000.6.7f1 and the packages listed in `26NASAChallenge/Packages/manifest.json`; C# and Python; Git/GitHub; ChatGPT, Claude Code and Codex; Midjourney and image-processing tools. The full asset inventory, AI methods and available provenance are in `26NASAChallenge/ASSET_CREDITS.md` and `Work Log and Review/AI_USE_LOG.md`. Engine/template material is credited separately from project-created art. Game audio (integrated Nov 15; not AI-generated): "Dream Culture", "Space Jazz" and "Cipher" by Kevin MacLeod (incompetech.com), licensed under Creative Commons: By Attribution 4.0 License (http://creativecommons.org/licenses/by/4.0/), volume levelled and compressed for the web; "Cat caffe" and "Cue" by TAD (opengameart.org/content/lofi-compilation), CC0; sound effects from "Interface Sounds" by Kenney (kenney.nl), CC0. The same credits appear on the in-game Data & Credits page; per-file details are in `26NASAChallenge/ASSET_CREDITS.md`.

Before submission, add any final audio, fonts, libraries, demo-editing software or other materials absent from those records. Free/open-source content must still be referenced, and applicable permissions must be retained.

## Demo plan: choose one format

**Pitch direction:** “Space curiosity becomes decisions that matter.” The issue is the gap between being excited about space and being able to practise scientific thinking. The solution is a game where building, growing food and protecting a crew require the player to use that thinking. Our distinction is the combination of an Ontario child's journey, connected systems and real NASA references. The intended impact is curiosity, confidence and a reason to keep learning.

**Recommended hook:** “Want to explore the Moon and Mars?” Answer it immediately with actual gameplay. A second hook to try is “Could you keep a crew alive on the Moon?” The first sells adventure; the second creates a challenge. Choose one opening. Keep stargazing language for the wider dream: telescope observation and round-the-clock star research are not playable features.

**28-second video outline — 29 seconds maximum for this edit:**

| Time | Actual gameplay / visual | Spoken line |
|---|---|---|
| 0–3 s | Open on a thriving Moon base, then a quick Mars terrain shot. Overlay the title, confirmed team name and selected challenge without delaying gameplay. | “Want to explore the Moon and Mars?” |
| 3–8 s | Brief Ontario childhood image, then cut straight to the player placing a Habitat. Caption: “Ages 7–12 · You are the commander.” | “Space science can feel far away. AstroCiv puts kids in command.” |
| 8–15 s | Three clear actions: place a building, show the greenhouse working, then respond to the storm warning and show the crew sheltered. Use a prepared mission; trim waiting time. | “Build a base. Grow food. Protect your crew from a solar storm.” |
| 15–18 s | Show a real-data card and the lunar sunlight overlay. Caption: “NASA data · simplified simulation.” | “Real NASA data shapes your decisions.” |
| 18–24 s | Show a mission report with one improvement, then an improved base. Briefly echo the Ontario story. Caption: “From an Ontario dream to your own outpost.” | “From an Ontario dream to your own outpost: build, test, try again.” |
| 24–28 s | Hold a beautiful active base with crew and a readable closing card: “Your crew needs you. Play AstroCiv.” Keep the public game address visible; put its clickable link beside the hosted video. | “Your crew needs you. Play AstroCiv.” |

**Continuous voiceover (54 words):**

> Want to explore the Moon and Mars? Space science can feel far away. AstroCiv puts kids in command. Build a base. Grow food. Protect your crew from a solar storm. Real NASA data shapes your decisions. From an Ontario dream to your own outpost: build, test, try again. Your crew needs you. Play AstroCiv.

The suggested timing is a rehearsal target, not a measured recording. Aim to finish speech by about 27 seconds and hold the closing card through 28 seconds; use the remaining second only if needed. Rehearse aloud and measure the final export. Shorten a line if necessary so the delivery has room to breathe.

**What viewers should understand:** Children see a place they want to build and a crew they want to help. Judges see the educational problem, a working solution, a specific NASA connection and the reason to open the playable game. The longer description carries the community impact and development plans; the video earns those claims through visible play.

**Editing priorities:** Show meaningful gameplay in the opening seconds. Keep each action and its result together; crop or zoom so the relevant resource change, warning or crew status is legible. Use large English captions, short overlays and music below speech. Show the game's feedback rather than calling it “the best” or claiming proven learning results. Retain existing credits and required generated-content indicators. Use only working screens from the build being submitted. The current trailer is source material for this revised edit; this outline does not mean the MP4 has been recut or a voiceover recorded.

**Alternative: up to 7 English slides, including title/intro:** (1) title, confirmed team/member names, selected challenge and hook; (2) curiosity-to-action problem and why it matters; (3) building and resource decisions; (4) storm choice and crew outcome; (5) NASA/CSA connections and the game's distinctive combination; (6) intended Ontario/world impact and future development; (7) public playable link, source link and invitation to try. Use real screenshots and concise captions. Keep the existing submission-guide checks below for final review.

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
