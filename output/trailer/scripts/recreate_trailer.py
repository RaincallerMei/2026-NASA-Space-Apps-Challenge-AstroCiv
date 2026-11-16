"""Cut recorded gameplay into the revised 28-second science/adventure trailer."""
from pathlib import Path
import hashlib
import json
import re
import shutil
import subprocess
from PIL import Image

ROOT = Path(__file__).resolve().parents[3]
OUT = ROOT / 'output/trailer'
EDIT = ROOT / 'tmp/trailer/edit-v2'
EDIT.mkdir(parents=True, exist_ok=True)
ffmpeg_candidates = [
    * (ROOT/'tmp/trailer-deps-v2/imageio_ffmpeg/binaries').glob('ffmpeg-*.exe'),
    * (ROOT/'tmp/trailer-deps/imageio_ffmpeg/binaries').glob('ffmpeg-*.exe'),
]
FFMPEG = str(ffmpeg_candidates[0]) if ffmpeg_candidates else shutil.which('ffmpeg')
if not FFMPEG:
    raise RuntimeError('Install imageio-ffmpeg into tmp/trailer-deps-v2 or provide ffmpeg on PATH.')
for source, target in [('arial.ttf', 'regular.ttf'), ('arialbd.ttf', 'bold.ttf')]:
    shutil.copyfile(Path('C:/Windows/Fonts') / source, EDIT / target)

def run(args):
    result = subprocess.run([FFMPEG, '-hide_banner', '-nostdin', *args], capture_output=True,
                            creationflags=subprocess.CREATE_NO_WINDOW, cwd=ROOT)
    if result.returncode:
        raise RuntimeError(result.stderr.decode(errors='replace')[-6000:])
    return result

def label(text, size, x, y, name, colour='white', bold=True):
    file = EDIT / (name + '.txt')
    file.write_text(text, encoding='utf-8')
    font = (EDIT / ('bold.ttf' if bold else 'regular.ttf')).relative_to(ROOT).as_posix()
    path = file.relative_to(ROOT).as_posix()
    return f"drawtext=fontfile='{font}':textfile='{path}':fontsize={size}:fontcolor={colour}:x={x}:y={y}"

# Every visual comes from a recording of the running Unity build.
# Source selection is kept explicit for review and future edits.
SHOTS = [
    dict(file='v2_moon_hero.webm', start=0, duration=1.2, name='moon',
         headline='WANT TO EXPLORE THE MOON & MARS?', sub='AstroCiv puts young explorers in command.'),
    dict(file='v2_mars.webm', start=0, duration=1.2, name='mars', crop='668:430:306:165',
         headline='WANT TO EXPLORE THE MOON & MARS?', sub='AstroCiv puts young explorers in command.'),
    dict(file='v2_building.webm', start=.25, duration=4.2, speed=1.7, name='build',
         headline='BUILD A HOME BEYOND EARTH.', sub='Power. Air. Water. Food. Every choice connects.'),
    dict(file='v2_science_sun.webm', start=.2, duration=3.4, name='science', crop='580:340:350:190',
         headline='REAL NASA DATA. SMARTER DECISIONS.', sub='Discover how sunlight changes the way you build.'),
    dict(file='v2_greenhouse.webm', start=0, duration=2.8, name='harvest',
         headline='GROW FOOD. KEEP YOUR CREW GOING.', sub='Learn the science by using it.'),
    dict(file='v2_storm_warning.webm', start=0, duration=3.4, name='warning',
         headline='SOLAR STORM. YOUR CREW NEEDS YOU.', sub='A real NASA event inspires the challenge. Game timing is simplified.'),
    dict(file='v2_storm_response.webm', start=1.5, duration=2.4, name='response', speed=1.6,
         headline='PROTECT PEOPLE. THINK AHEAD.', sub='Recall your geologist. Power the shelter.'),
    dict(file='v2_storm_report.webm', start=.15, duration=2.8, name='achievement',
         headline='BUILD. TEST. EARN YOUR MOMENT.', sub='S grade. 98/100. Three stars. Crew safe.'),
    dict(file='v2_earth_ending.webm', start=0, duration=3, name='future',
         headline='BIG DREAMS START IN ONTARIO.', sub='Practise the problem-solving our future needs.'),
    dict(file='v2_earth_ending.webm', start=0, duration=3.6, name='closing', closing=True),
]
assert abs(sum(s['duration'] for s in SHOTS) - 28) < 1e-6

args = ['-y', '-filter_complex_threads', '2']
for shot in SHOTS:
    args += ['-i', str(OUT / 'raw' / shot['file'])]
music = ROOT / 'Art/BGM/Starry Twinkles.mp3'
args += ['-i', str(music)]
filters = []
for index, shot in enumerate(SHOTS):
    duration = shot['duration']
    speed = shot.get('speed', 1)
    chain = f'[{index}:v]setpts=PTS-STARTPTS,trim=start={shot["start"]},setpts=(PTS-STARTPTS)/{speed}'
    chain += f',tpad=stop_mode=clone:stop_duration={duration},trim=duration={duration},fps=30'
    if shot.get('closing'):
        chain += ',scale=1280:720,setsar=1,format=yuv420p'
        chain += ',drawbox=x=0:y=0:w=iw:h=ih:color=0x08132B@0.88:t=fill'
        chain += ',drawbox=x=100:y=110:w=7:h=425:color=0x62D8E5:t=fill'
        chain += ',' + label('YOUR CREW NEEDS YOU.', 29, 145, 112, 'end-hook', '0x62D8E5')
        chain += ',' + label('PLAY ASTROCIV', 68, 145, 176, 'end-title')
        chain += ',' + label('MAPMAKERS OF THE MOON', 28, 145, 270, 'end-subtitle', '0xF2B84B')
        chain += ',' + label('Build. Learn. Dream bigger.', 30, 145, 335, 'end-promise')
        chain += ',' + label('Free desktop browser game | Ages 7-12', 22, 145, 397, 'end-platform', '0xCDEAF4', False)
        chain += ',' + label('raincallermei.github.io/2026-NASA-Space-Apps-Challenge-AstroCiv/', 21, 145, 451, 'end-url', '0xCDEAF4', False)
        chain += ',' + label('Recorded gameplay | AI-created game art | No NASA/CSA endorsement', 16, 145, 585, 'end-art', '0xCDEAF4', False)
        chain += ',' + label('Music: Starry Twinkles - raincalleer / Suno (AI-generated)', 16, 145, 615, 'end-music', '0xCDEAF4', False)
        chain += f',fade=t=out:st={duration-.28}:d=0.28'
    else:
        if shot.get('crop'):
            chain += ',crop=' + shot['crop']
        chain += ',scale=1080:608:force_original_aspect_ratio=decrease'
        chain += ',pad=1280:720:(ow-iw)/2:0:color=0x0B1430,setsar=1,format=yuv420p'
        chain += ',drawbox=x=100:y=620:w=1080:h=3:color=0x62D8E5:t=fill'
        chain += ',' + label(shot['headline'], 30, '(w-text_w)/2', 636, shot['name'] + '-headline')
        chain += ',' + label(shot['sub'], 19, '(w-text_w)/2', 682, shot['name'] + '-sub', '0xCDEAF4', False)
        chain += ',' + label('ASTROCIV', 15, 10, 10, shot['name'] + '-brand', '0x62D8E5')
        chain += ',' + label('Recorded game', 10, 6, 40, shot['name'] + '-recorded', '0xCDEAF4', False)
        chain += ',' + label('AI-created art', 10, 6, 55, shot['name'] + '-ai', '0xCDEAF4', False)
    filters.append(chain + f'[v{index}]')

filters.append(''.join(f'[v{i}]' for i in range(len(SHOTS))) + f'concat=n={len(SHOTS)}:v=1:a=0[v]')
filters.append(f'[{len(SHOTS)}:a]atrim=start=18:duration=28,asetpts=PTS-STARTPTS,loudnorm=I=-18:TP=-1.5:LRA=8,afade=t=in:st=0:d=0.25,afade=t=out:st=26.7:d=1.3[a]')
filter_text = ';\n'.join(filters)
(EDIT / 'filters.txt').write_text(filter_text, encoding='utf-8')
final = OUT / 'AstroCiv_Trailer_28s.mp4'
temporary = EDIT / final.name
description = ('Recreated trailer: actual Unity WebGL game recordings, edited by OpenAI Codex and FFmpeg. '
               'Game art is AI-created. Music Starry Twinkles by raincalleer, generated with Suno, '
               'song f5182621-6979-42be-864b-69517b18be60. No synthetic gameplay video. '
               'Mission selection unlocked in an isolated capture profile; native starter base used for the storm. '
               'Shown S 98/100 result was earned by playing the Solar Storm mission; no score was edited. '
               'Gameplay excerpts are shortened and may be sped up. NASA/CSA data is simplified for education.')
args += ['-filter_complex', filter_text, '-map', '[v]', '-map', '[a]', '-c:v', 'libx264',
         '-preset', 'medium', '-crf', '18', '-pix_fmt', 'yuv420p', '-r', '30', '-threads', '4',
         '-c:a', 'aac', '-b:a', '192k', '-ar', '48000', '-t', '28', '-movflags', '+faststart+use_metadata_tags',
         '-metadata', 'title=AstroCiv - Build, protect, discover - 28 second trailer',
         '-metadata', 'artist=AstroCiv; music by raincalleer / Suno',
         '-metadata', 'description=' + description, '-metadata', 'comment=' + description, str(temporary)]
print('Rendering revised trailer...', flush=True)
result = run(args)
(EDIT / 'render.log').write_bytes(result.stderr)
result = run(['-i', str(temporary), '-vf', 'blackdetect=d=0.3:pix_th=0.02', '-af', 'volumedetect', '-f', 'null', '-'])
validation = result.stderr.decode(errors='replace')
(EDIT / 'validation.log').write_text(validation, encoding='utf-8')
m = re.search(r'Duration: (\d+):(\d+):(\d+\.\d+)', validation)
duration = int(m[1])*3600 + int(m[2])*60 + float(m[3])
frames = int(re.findall(r'frame=\s*(\d+)', validation)[-1])
assert duration == 28 and frames == 840, (duration, frames)
assert 'Audio: aac' in validation and 'Video: h264' in validation
mean = float(re.search(r'mean_volume: ([-\d.]+) dB', validation)[1])
peak = float(re.search(r'max_volume: ([-\d.]+) dB', validation)[1])
assert -40 < mean < -8 and peak < 0, (mean, peak)
assert 'black_duration:' not in validation, 'Unexpected sustained black frames'
shutil.copy2(temporary, final)
sample_cursor = 0
sheet = Image.new('RGB', (1280, 1800), '#0b1430')
for index, shot in enumerate(SHOTS):
    sample = EDIT / f'shot-{index:02d}.png'
    run(['-y', '-ss', str(sample_cursor + shot['duration']/2), '-i', str(final),
         '-frames:v', '1', '-update', '1', str(sample)])
    with Image.open(sample) as frame:
        sheet.paste(frame.resize((640, 360)), ((index % 2)*640, (index // 2)*360))
    sample_cursor += shot['duration']
sheet.save(OUT/'trailer-contact-sheet.jpg', quality=92)
cursor = 0
timeline = []
for shot in SHOTS:
    timeline.append(dict(shot, timelineStart=round(cursor, 2)))
    cursor += shot['duration']
sources = sorted({s['file'] for s in SHOTS})
manifest = dict(file=final.name, durationSeconds=duration, frames=frames, width=1280, height=720, fps=30,
                videoCodec='H.264', audioCodec='AAC', bytes=final.stat().st_size,
                audioMeanDb=mean, audioPeakDb=peak, shots=timeline, provenance=description,
                musicFile=str(music.relative_to(ROOT)), musicExcerptSeconds=[18,46],
                musicSourceSha256=hashlib.sha256(music.read_bytes()).hexdigest(),
                sourceClipSha256={name:hashlib.sha256((OUT/'raw'/name).read_bytes()).hexdigest() for name in sources},
                webBuildSha256={p.name:hashlib.sha256(p.read_bytes()).hexdigest() for p in (ROOT/'docs/Build').iterdir() if p.is_file()},
                validation='All 840 frames decoded; runtime, codecs, non-silent unclipped audio and sustained-black checks passed.',
                status='local file; not uploaded or submitted')
(OUT/'TRAILER_MANIFEST.json').write_text(json.dumps(manifest, indent=2)+'\n', encoding='utf-8')
print(json.dumps({k:manifest[k] for k in ['file','durationSeconds','frames','bytes','audioMeanDb','audioPeakDb','validation']},indent=2), flush=True)
