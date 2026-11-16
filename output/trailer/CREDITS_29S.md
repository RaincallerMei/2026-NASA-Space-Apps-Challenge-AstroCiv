# Artemis II trailer: sources and credits

Prepared November 15, 2026 for a noncommercial educational NASA Space Apps project demonstration.

## NASA poster and permitted use

Opening image: **Artemis II crew poster, NASA**. [Original supplied JPEG](https://www.nasa.gov/wp-content/uploads/2024/05/artemis-ii-crew-poster-web-page-1.jpg?resize=1200,783), [NASA educational resource listing](https://www.nasa.gov/learning-resources/join-artemis/), [NASA media-use guidelines](https://www.nasa.gov/nasa-brand-center/images-and-media/).

NASA provides this poster as an educational resource. Its media guidelines allow factual educational/informational reuse with source acknowledgement and without implying endorsement. This use is an independent educational challenge demonstration. It does not establish permission for commercial advertising or merchandise using astronaut likenesses or agency marks. The supplied image showed no separate third-party credit. NASA and Artemis marks remain within the authentic poster and are not adopted as the game's branding. The opening visibly credits NASA and identifies an independent project. No NASA, CSA or astronaut endorsement is claimed.

The original JPEG is preserved in `sources/artemis-ii-crew-poster.jpg`. Ordinary video framing and a separate caption layer were used; no generative image/video model changed or recreated the NASA poster.

## Music

**Starry Twinkles**, raincalleer, generated with **Suno**. User-supplied file: `Art/BGM/Starry Twinkles.mp3`. Song ID: `f5182621-6979-42be-864b-69517b18be60`; [song page](https://suno.com/song/f5182621-6979-42be-864b-69517b18be60). The new cut uses seconds 18-47, with loudness adjustment and fades. Existing source/provenance information is retained; no new music or voiceover was generated. The applicable account/download rights remain those of the supplied track; this document grants no additional music licence. AI audio generation is acknowledged here and in the MP4 description/comment metadata.

## Game art, footage and data

All project-created game art is AI-created, as documented in the project asset credits and AI-use log. Midjourney supplied story illustrations; the project's existing generated pixel art and UI are visible in gameplay. This edit uses the existing colony cutscene artwork for Story Mode and a crop of the existing observation-window/message artwork for the ending. The ending crop shows the adult commander and Earth; the illustrated children in the lower part of the original are outside the frame. No new character artwork or synthetic gameplay was generated.

Gameplay comes from the local Unity WebGL build in a new, isolated Chrome profile. Only mission-selection prerequisites were staged; scores were not edited or displayed. The user's normal browser profile and saved game were not used. The source viewport captures contain sparse compositor frames, resampled to 30 fps with frame holds; cuts/zoomed crops emphasize real UI changes. Resource-preview decisions were recorded while paused. Waiting time is omitted in the crew-response montage. This is a demonstration montage rather than an uninterrupted campaign run.

Observed resource decision: Greenhouse off changes power balance from **-0.7 to +2.3 kW**, and food from **+0.1 to -0.4 per hour**, at the recorded base state. Placement decision: adding a Battery close to the selected Solar Array changes its displayed output from **1.6 to 2.3 kW**. All those values and connection penalties are simplified game rules.

The Moon mission's terrain uses archived **NASA LRO/LOLA average illumination**. The storm warning references a historical **NASA CCMC DONKI** event from May 2024. Warning timing, power/storage/resource rates, crew effects and the powered Shelter are gameplay abstractions. This is not a live space-weather feed or real mission-planning tool. Free Build terrain is generated practice terrain and is not presented as a NASA map. Full scientific provenance: `26NASAChallenge/DATA_SOURCES.md` and `SCIENCE_ASSUMPTIONS.csv`.

## Editing and traceability

OpenAI Codex assisted with source research, real-game capture, shot selection, captions, editing, documentation and technical validation. Chrome/CDP captures the game viewport; Python and FFmpeg perform ordinary editing and encoding. Reusable capture plans and the producer are in `scripts/`. Source hashes, exact trims/speeds, build hashes, audio provenance and measured export checks are in `TRAILER_29S_MANIFEST.json`.

Source materials retain their applicable terms. The NASA educational reuse above is not a blanket licence for all game assets or music.
