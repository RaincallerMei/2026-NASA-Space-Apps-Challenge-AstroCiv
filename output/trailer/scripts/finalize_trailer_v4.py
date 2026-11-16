"""Document the measured v4 trailer without rewriting previous cut records."""
from pathlib import Path
import json,re
ROOT=Path(__file__).resolve().parents[3]
OUT=ROOT/'output/trailer/v4'
m=json.loads((OUT/'MANIFEST.json').read_text(encoding='utf-8'))
assert m['durationSeconds']==29 and m['frames']==870
prompt="""Use case: precise-object-edit. Edit target: the supplied pixel-art lunar-base observation-window cutscene. Change ONLY the picture displayed INSIDE the cyan-framed communication monitor on the right. Replace all children and other people inside that monitor with a warm, empty classroom displaying several cheerful hand-drawn Moon-base and Mars-base blueprints and drawings on desks and a board. No people or faces anywhere inside that monitor. Preserve the existing cyan monitor frame, its perspective, and its reflected warm light. Everything OUTSIDE the monitor must remain as close as possible to the original: the adult astronaut at left with the Canadian flag, the adult crew in the lower right foreground, lunar landscape, bright Earth through the window, small plants, model rover, walls, exact original composition, lighting, colors and pixel-art texture. Preserve the entire original full landscape composition and its approximately 4:3 aspect ratio. Do not add words, captions, logos or other design elements. This is an aspirational video end-card background; text is added separately in the video editor."""
credits=f'''# Revised 29-second trailer: credits and provenance

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

> {prompt}

OpenAI Codex assisted with research, capture scripts, shot selection, editing, captions, documentation and technical checks. Python/FFmpeg perform ordinary editing; no synthetic gameplay video was generated. AI art is visibly indicated; music and editing assistance are acknowledged in this file and the MP4 metadata.

## Validation and traceability

All **870 frames** decoded; exactly **29.00 seconds**, **1920 x 1080**, **30 fps**, H.264/AAC. Audio mean **{m['audioMeanDb']:.1f} dBFS**, peak **{m['audioPeakDb']:.1f} dBFS**; no sustained black frames. Full-frame samples, both resource changes, the placement comparison and construction motion were reviewed. This is technical/visual QA; human listening approval is not recorded.

[MANIFEST.json](MANIFEST.json) records source/build SHA-256 hashes, exact source trims, speeds, timings and measured checks. [Contact sheet](contact-sheet.jpg). Reusable capture/edit scripts are in `../scripts/`; local render logs and detailed QA frames are in `tmp/trailer-v4/edit/`. Previous cuts are preserved. The video has not been uploaded or submitted by this workflow.
'''
(OUT/'CREDITS.md').write_text(credits,encoding='utf-8')
rows='\n'.join(f"| {s['startSeconds']:.2f}-{s['endSeconds']:.2f} s | {s['headline']} |" for s in m['chapters'])
readme=f'''# AstroCivilization - revised 29-second trailer

[Watch the MP4](../AstroCiv_Trailer_29s_v4.mp4) | [Contact sheet](contact-sheet.jpg) | [Credits and source records](CREDITS.md) | [Manifest](MANIFEST.json) | [Previous 29-second cut](../AstroCiv_Trailer_29s.mp4)

**29.00 seconds; 1920 x 1080; 30 fps; H.264/AAC; {m['bytes']/1_000_000:.2f} MB.** Uses the supplied **Pastel Twinkles** soundtrack. Built from the user's revised scene instructions: the animated NASA poster over the home screen, 4x Moon/Mars construction, both Greenhouse resource cases, strategic placement, scrolling scientific achievements, Free Build and five moving construction maps. The full-composition ending says **You are the next explorer!** and begins at **27.50 seconds**, with a half-transparent caption panel.

| Time | Title |
|---|---|
{rows}

The Moon and Mars invitation clips each last **1.50 seconds at 4x** and use gentle camera movement up to **5%**. Five different Free Build construction clips each have a **0.50-second** moving gameplay core at 4x; transitions are **two frames (0.0667 seconds)**, matching the requested rapid 0.07-second transition at 30 fps. Other scene transitions use short four-frame blends. The three seconds removed from the hook/ending were distributed across the seven middle sections in whole-frame increments (13 frames in six sections and 12 frames in the map montage).

Greenhouse OFF restores power while food falls; Greenhouse ON restores a positive food rate. The placement demonstration shows the selected Solar Array improving from 2.1 to 2.9 kW after a nearby Battery is built. The achievements scroll includes both earned facts and locked hints. Actual science citations remain visible. Gameplay was recorded continuously from an isolated current build/profile. No resource values, achievements or scores were fabricated.

The authentic NASA poster remains unaltered apart from normal framing/fade/scale. Game art and the supplied Suno track are disclosed. The full ending uses a derivative of the existing cutscene with classroom drawings in the monitor; see its saved asset and exact image-edit prompt in the credits.

Verified **870 decoded frames**, exactly **29.00 seconds**, audible unclipped soundtrack, and no sustained black frames. Audio mean {m['audioMeanDb']:.1f} dBFS; peak {m['audioPeakDb']:.1f} dBFS. Full-frame samples and the key action/result sequences were reviewed. Local file only; previous cuts remain available.

Rebuild with `python output/trailer/scripts/produce_trailer_v4.py`, then `python output/trailer/scripts/finalize_trailer_v4.py`. It uses the existing FFmpeg binary in `tmp/trailer-deps-v2/` and Pillow. This Windows workspace requires outside-sandbox access to the local encoder folder. Capture plans and source clips are preserved for further edits.
'''
(OUT/'README.md').write_text(readme,encoding='utf-8')
p=ROOT/'output/trailer/README.md'
s=p.read_text(encoding='utf-8').replace('**Current version: [29-second Artemis II trailer]', '**Previous version: [29-second Artemis II trailer]')
intro='<!-- trailer-v4-current -->\n**Latest: [revised 29-second trailer](AstroCiv_Trailer_29s_v4.mp4)** | [Timings, credits and validation](v4/README.md) | [Contact sheet](v4/contact-sheet.jpg). Earlier cuts and their historical notes follow.\n\n---\n\n'
if '<!-- trailer-v4-current -->' not in s:p.write_text(intro+s,encoding='utf-8')
p=ROOT/'README.md';s=p.read_text(encoding='utf-8')
s=s.replace('[29-second Artemis II trailer](output/trailer/AstroCiv_Trailer_29s.mp4)','[revised 29-second Artemis II trailer](output/trailer/AstroCiv_Trailer_29s_v4.mp4)')
p.write_text(s,encoding='utf-8')
p=ROOT/'Work Log and Review/AI_USE_LOG.md';s=p.read_text(encoding='utf-8')
heading='Revised trailer: continuous construction, Science Achievements and Pastel Twinkles'
if heading not in s:
 n=max(map(int,re.findall(r'^## (\d+)\.',s,re.M)),default=0)+1
 s+=f'''\n## {n}. {heading} - November 15, 2026

- User directed the revised captions, an animated Artemis II poster over the home screen, 4x Moon/Mars construction with 5% camera motion, both Greenhouse tradeoffs, strategic placement, a scrolling earned/locked Science Facts collection, five moving map clips, a 27.5-29 s ending with half-transparent banners, and the supplied Pastel Twinkles music.
- Codex built the current Unity project to an isolated temporary WebGL folder to capture the latest collection. The published build and normal browser/game saves were not replaced. Continuous canvas video was recorded at approximately 30 fps with capture-only preserveDrawingBuffer enabled. Mission-selection prerequisites were staged; actual play earned 18/24 facts. No resources, scores or achievements were fabricated.
- Genuine evidence: Greenhouse OFF power -0.7 to +2.3 kW and food +0.1 to -0.4/h; Greenhouse ON food -0.4 to +0.1/h after naturally depleting reserves; selected Solar Array 2.1 to 2.9 kW after nearby Battery placement. Five distinct Moon/Mars seeds and construction layouts are recorded.
- Image editing: built-in image_gen.imagegen, edit mode, changed only the classroom monitor in the existing message cutscene to show Moon/Mars drawings without children, preserving the full adult crew/Earth scene. Saved derivative and exact prompt: output/trailer/v4/ending-full-drawings.png and CREDITS.md. The original game asset was not replaced. NASA's original crew poster was not AI-generated or regenerated.
- Music: Pastel Twinkles, raincalleer, supplied Suno track 0a49d446-3f17-465c-8971-c0398cc51905. No new music, voice or synthetic gameplay video generated. Game art and audio provenance are disclosed on screen, in credits and in MP4 metadata.
- Outputs: output/trailer/AstroCiv_Trailer_29s_v4.mp4 plus v4/README.md, CREDITS.md, MANIFEST.json, contact sheet, source recordings and reusable scripts. Prior cuts preserved.
- Validation: all 870 frames decoded; exactly 29.00 s; 1920 x 1080, 30 fps, H.264/AAC; audio mean {m['audioMeanDb']:.1f} dBFS, peak {m['audioPeakDb']:.1f} dBFS; no sustained black frames. Visual samples and genuine action/result sequences reviewed. No game-code edits for this task; no upload or challenge submission.
'''
 p.write_text(s,encoding='utf-8')
print('Wrote v4 delivery notes, credits, image-edit prompt and AI-use record.')
