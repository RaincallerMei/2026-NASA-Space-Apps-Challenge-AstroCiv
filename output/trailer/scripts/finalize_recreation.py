"""Keep the delivery notes and submission desk aligned with the exported trailer."""
from pathlib import Path
import html
import json
import re
import shutil

ROOT = Path(__file__).resolve().parents[3]
OUT = ROOT / 'output/trailer'
data = json.loads((OUT/'TRAILER_MANIFEST.json').read_text(encoding='utf-8'))
assert data['durationSeconds'] == 28 and data['frames'] == 840

rows = []
for shot in data['shots']:
    start = shot['timelineStart']
    stop = start + shot['duration']
    rows.append(f"| {start:g}–{stop:g} s | {shot.get('headline', 'YOUR CREW NEEDS YOU. PLAY ASTROCIV.')} | {shot['file']} |")
readme = f'''# AstroCiv — recreated 28-second gameplay trailer

[Watch the MP4](AstroCiv_Trailer_28s.mp4) · [Previous cut](AstroCiv_Trailer_28s_previous.mp4) · [Credits](CREDITS.md) · [Shot manifest](TRAILER_MANIFEST.json) · [Contact sheet](trailer-contact-sheet.jpg)

**28.00 seconds**, 1280×720, 30 fps, H.264 video and AAC stereo audio; **{data['bytes']/1_000_000:.2f} MB**. Recreated 15 November 2026 to sell the adventure, show science being used in play, celebrate an earned achievement, and connect the Ontario story to curiosity and future problem-solving.

The opening cuts from the Moon to Mars. Real placement footage shows an outpost taking shape. A close view of the NASA sunlight card makes the knowledge legible; the greenhouse connects resources to food. The storm warning cites NASA DONKI, followed by actual shelter/recall actions. The result is an earned **S, 98/100, three stars, 100% crew safety**, followed by the commander connecting with a classroom on Earth. The closing card invites viewers to play and gives the browser-game address.

| Time | On-screen pitch | Recorded source |
|---|---|---|
{chr(10).join(rows)}

Captions and the existing **Starry Twinkles** soundtrack carry the pitch; no voiceover was generated or recorded. Captions sit below the gameplay so building cards, resources and objectives remain visible. The NASA card is cropped closer for readability. Some gameplay is sped up and waiting time is removed; the manifest records source offsets and speeds. All visual source material is captured from the real game. Game balance and storm timing remain simplified educational rules.

Capture used the local `docs/` WebGL build in a fresh, isolated Chrome profile. To reach the requested shots, prerequisite mission-selection flags were set in that capture profile; best scores were never edited. The storm mission used its own native fallback starter base, then was played through: recall the geologist, power the shelter, rest the greenhouse while the battery was low, restore food production and reach inspection. Tutorial 1 also earned S 93/100 in a separate recording. This is a montage of working screens, not a claim to show an uninterrupted campaign playthrough. The user's normal browser/game saves were not used.

Validation decoded all **840 frames**, confirmed **28.00 seconds**, H.264/AAC streams, non-silent audio and no sustained black frames. Audio mean **{data['audioMeanDb']:.1f} dBFS**, peak **{data['audioPeakDb']:.1f} dBFS**. The contact sheet samples every edited shot. Source hashes and WebGL build hashes are in the manifest; render/validation logs are under `tmp/trailer/edit-v2/` in this workspace. Human listening/playback approval is not recorded.

The previous MP4 is preserved. Raw recordings and capture plans remain available for further edits. Rebuild with `python output/trailer/scripts/recreate_trailer.py` from the repository root; FFmpeg is supplied by imageio-ffmpeg 0.8.1 under `tmp/trailer-deps-v2/` (or the earlier `tmp/trailer-deps/` fallback); Pillow assembles the review sheet. In this Windows workspace, the encoder's installed folder requires running the script outside the sandbox. The finalizer updates these notes and the Submission desk after rendering. No public upload or challenge submission was performed.
'''
(OUT/'README.md').write_text(readme, encoding='utf-8', newline='\r\n')

credits = (OUT/'CREDITS.md').read_text(encoding='utf-8')
marker = '\n## Recreated cut — 15 November 2026\n'
if marker in credits:
    credits = credits.split(marker)[0]
credits += marker + '''
The new cut retains the user-supplied Starry Twinkles excerpt (18–46 seconds), existing music provenance, and art/AI acknowledgements. It adds Moon/Mars gameplay, the NASA POWER sunlight card, greenhouse inspection, a DONKI storm warning, actual shelter/recall interactions, an earned storm-mission S 98/100 report and the in-game classroom/Earth ending. Captions and music are the complete audio/text treatment; no synthetic voice or gameplay video was generated.

Only mission-selection prerequisites were staged in a fresh capture profile, to reach the requested scenes. Scores were not edited: the game's own simulation produced the shown 98/100 result after the storm mission was played with its native starter base. The game uses fictional balance values and simplified data interpretations. Agency data credits, original generated-art indicators and the closing music/art disclosure remain visible. The exact source clips, speeds and hashes are in the manifest. Previous music/source terms above still apply.
'''
(OUT/'CREDITS.md').write_text(credits, encoding='utf-8', newline='\r\n')

old = 'The current trailer is source material for this revised edit; this outline does not mean the MP4 has been recut or a voiceover recorded.'
new = 'The recreated 28-second trailer uses on-screen captions and the existing music. Its exact edited sequence and measured validation are recorded in output/trailer/README.md. The voiceover above remains an optional spoken version; no narration was recorded.'
for relative in ['Mapmakers 25-Hour Board.html', 'Work Log and Review/SUBMISSION_DRAFT.md']:
    path = ROOT / relative
    text = path.read_text(encoding='utf-8')
    before, after = (html.escape(old), html.escape(new)) if relative.endswith('.html') else (old, new)
    if before in text:
        text = text.replace(before, after, 1)
    if relative.endswith('.html') and 'id="recreated-trailer"' not in text:
        link = '<p id="recreated-trailer"><b>Recreated trailer: 28.00 seconds.</b> <a href="output/trailer/AstroCiv_Trailer_28s.mp4">Watch the new cut</a> · <a href="output/trailer/README.md">Actual shot sequence and validation</a>. Includes Moon/Mars, NASA knowledge, crew protection, an earned S 98/100 result and the classroom ending.</p>\n'
        text = text.replace('<h2>Demo outline</h2>\n', '<h2>Demo outline</h2>\n' + link, 1)
    elif relative.endswith('.md') and '[Watch the recreated 28-second trailer]' not in text:
        text = text.replace('## Demo plan: choose one format\n', '## Demo plan: choose one format\n\n[Watch the recreated 28-second trailer](../output/trailer/AstroCiv_Trailer_28s.mp4) · [Actual shots and validation](../output/trailer/README.md).\n', 1)
    path.write_text(text, encoding='utf-8', newline='\r\n')

plans = ['v2_mission_setup', 'v2_building', 'v2_first_result', 'v2_warning', 'v2_protect_actual',
         'v2_greenhouse_and_survive', 'v2_toward_finish', 'v2_win', 'v2_ending_capture', 'v2_mars_capture']
for name in plans:
    source = ROOT/'tmp/trailer'/(name+'.json')
    if source.is_file():
        shutil.copy2(source, OUT/'scripts'/source.name)

log = ROOT/'Work Log and Review/AI_USE_LOG.md'
existing = log.read_text(encoding='utf-8')
heading = 'Trailer recreated around science, play and achievement'
if heading not in existing:
    number = max(map(int, re.findall(r'^## (\d+)\.', existing, re.M)), default=0)+1
    entry = f'''
## {number}. {heading} — 15 November 2026

- **Tool:** OpenAI Codex; exact runtime model/session identifier not recorded. Chrome/CDP and canvas MediaRecorder for real gameplay; Python and FFmpeg for editing, captions, music mix and validation.
- **User direction:** recreate the trailer within 29 seconds, showing why the game is engaging and important, cool gameplay, science/data and proud achievements.
- **Method:** reviewed the prior edit and implemented game, then recorded new footage in a fresh isolated profile. Staged mission-selection prerequisites only; did not edit scores. Played the game's native starter-base storm mission, protected the crew, managed greenhouse/battery tradeoffs and earned S 98/100 with 100% safety. Recorded the NASA sunlight card, Moon/Mars building, result report and classroom/Earth ending. Used the same supplied Starry Twinkles soundtrack; generated no narration or synthetic gameplay footage.
- **Outputs:** recreated `output/trailer/AstroCiv_Trailer_28s.mp4`, previous-cut backup, source recordings/plans, reusable renderer, updated manifest/contact sheet, credits/README and a current trailer link in the Submission desk and shared draft. The video's assertions are grounded in working screens; future educational benefits remain ambitions, not measured learning outcomes.
- **Checks:** decoded all 840 frames; measured exactly 28.00 seconds; H.264/AAC, non-silent audio, peak below 0 dBFS, no sustained black frames. Audio mean {data['audioMeanDb']:.1f} dBFS, peak {data['audioPeakDb']:.1f} dBFS. Contact sheet and detailed frames reviewed as part of delivery QA. No game-code changes or game test rerun; no upload or event submission.
- **Human involvement:** user supplied the theme and earlier hook/song choices; final human playback/listening approval remains unrecorded.
'''
    with log.open('a', encoding='utf-8', newline='\r\n') as file:
        file.write(entry)
print('Updated trailer notes, source plans, submission links and AI-use record.')
