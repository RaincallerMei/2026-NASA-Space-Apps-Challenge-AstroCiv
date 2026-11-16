"""Write delivery notes from the measured 29-second export."""
from pathlib import Path
import json,re
ROOT=Path(__file__).resolve().parents[3]
OUT=ROOT/'output/trailer'
m=json.loads((OUT/'TRAILER_29S_MANIFEST.json').read_text())
assert m['durationSeconds']==29 and m['frames']==870
credits='''# Artemis II trailer: sources and credits

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
'''
(OUT/'CREDITS_29S.md').write_text(credits,encoding='utf-8')
rows=[]
for s in m['shots']:
 a=s['timelineStart'];b=a+s['duration']
 rows.append(f"| {a:g}-{b:g} s | {s['headline']} |")
readme=f'''# AstroCivilization: Artemis II trailer, 29 seconds

[Watch the new MP4](AstroCiv_Trailer_29s.mp4) | [Credits and NASA image-use review](CREDITS_29S.md) | [Shot/source manifest](TRAILER_29S_MANIFEST.json) | [Contact sheet](trailer-29s-contact-sheet.jpg) | [Previous 28-second cut](AstroCiv_Trailer_28s.mp4)

**29.00 seconds; 1920 x 1080; 30 fps; H.264/AAC; {m['bytes']/1_000_000:.2f} MB.** Produced November 15, 2026 from the approved seven-part pitch with the user's revised Artemis II poster hook and “You can be next!” ending. On-screen branding follows the current build's title, AstroCivilization; the file keeps the AstroCiv project prefix.

The trailer shows a real Greenhouse toggle and the resulting power/food trade-off, a Battery-placement comparison with actual Solar Array output changing from 1.6 to 2.3 kW, the historical DONKI warning and crew protection, Story Mode, and Free Build on Mars. The opening uses the exact requested NASA poster. The short ending uses the adult commander/Earth portion of existing game art, followed by the game title and playable address.

| Time | Caption |
|---|---|
{chr(10).join(rows)}

Captions and the existing Starry Twinkles soundtrack carry the pitch; there is no voiceover. NASA attribution appears on the opening; AI-created game art is labelled on the relevant shots. Audio generation and AI-assisted code/editing are acknowledged in the description/comment metadata and credits. The original NASA image was not AI-generated or regenerated.

Technical checks decoded all **870 frames**, confirmed exactly **29.00 seconds**, H.264/AAC streams and no sustained black frames. Audio mean **{m['audioMeanDb']:.1f} dBFS**, peak **{m['audioPeakDb']:.1f} dBFS**. Every shot was sampled for visual review, including full-size opening, resource, placement and ending frames. The final published file has not been uploaded or submitted by this workflow.

The footage is a montage of real game interactions and existing cutscene art. Sparse compositor source frames are resampled with holds; paused previews make resource-rate changes readable. See the credits for exact scientific limits, capture treatment and music terms. The previous trailer is preserved.

Rebuild from the repository root with `python output/trailer/scripts/produce_trailer_v3.py`. The existing local encoder is under `tmp/trailer-deps-v2/`; this Windows workspace requires access outside the sandbox to invoke it. Render logs and QA frames are under `tmp/trailer-v3/edit/`.
'''
(OUT/'README_29S.md').write_text(readme,encoding='utf-8')
old=OUT/'README.md';s=old.read_text(encoding='utf-8')
marker='<!-- artemis-29s-current -->'
if marker not in s:
 s=marker+'\n**Current version: [29-second Artemis II trailer](AstroCiv_Trailer_29s.mp4)** | [Credits, shots and validation](README_29S.md). Earlier cut and notes follow.\n\n---\n\n'+s
 old.write_text(s,encoding='utf-8')
rootreadme=ROOT/'README.md';s=rootreadme.read_text(encoding='utf-8')
s=s.replace('[28-second gameplay trailer](output/trailer/AstroCiv_Trailer_28s.mp4)','[29-second Artemis II trailer](output/trailer/AstroCiv_Trailer_29s.mp4)')
rootreadme.write_text(s,encoding='utf-8')
log=ROOT/'Work Log and Review/AI_USE_LOG.md';s=log.read_text(encoding='utf-8')
heading='Artemis II opening and 29-second trailer'
if heading not in s:
 n=max(map(int,re.findall(r'^## (\d+)\.',s,re.M)),default=0)+1
 entry=f'''\n## {n}. {heading} - November 15, 2026

- User approved the seven-element pitch with the supplied NASA crew poster, the question “Want to explore humanity's future on Moon or Mars?” and the ending “You can be next!”.
- Codex checked NASA's original poster listing and media-use guidelines, preserved/credited the authentic image, recorded game interactions in a new isolated browser profile and edited with Python/FFmpeg. No new generative image, video, voice or music output was made. Existing AI-created game art and the supplied Suno track remain disclosed.
- The final export includes real power/food changes, a verified 1.6-to-2.3 kW placement comparison, historical DONKI warning/crew response, existing colony story art, Free Build/Mars and the adult commander/Earth ending crop. Mission-selection access was staged; scores and the user's normal save were untouched. Compositor capture and frame holds are described in the trailer credits.
- Outputs: `output/trailer/AstroCiv_Trailer_29s.mp4`, `README_29S.md`, `CREDITS_29S.md`, `TRAILER_29S_MANIFEST.json`, contact sheet, source poster, recordings and reproducible scripts. The prior trailer remains available.
- Validation: all 870 frames decoded; exactly 29.00 seconds; 1080p H.264/AAC; audio mean {m['audioMeanDb']:.1f} dBFS and peak {m['audioPeakDb']:.1f} dBFS; no sustained black frames. Full-size QA frames reviewed. No game-code change, public upload or judging submission.
'''
 log.write_text(s+entry,encoding='utf-8')
print('Wrote verified delivery notes, source credits and AI-use record.')
