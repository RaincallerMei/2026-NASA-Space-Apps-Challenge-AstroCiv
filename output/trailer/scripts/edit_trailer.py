"""Edit recorded AstroCiv gameplay into a 28-second trailer. No generated footage."""
from pathlib import Path
import array
import hashlib
import json
import math
import re
import shutil
import subprocess
import sys

ROOT=Path.cwd()
FFMPEG=str(next((ROOT/'tmp/trailer-deps/imageio_ffmpeg/binaries').glob('ffmpeg-*.exe')))
OUT=ROOT/'output/trailer';OUT.mkdir(parents=True,exist_ok=True)
TMP=Path('tmp/trailer/edit');TMP.mkdir(parents=True,exist_ok=True)
font=TMP/'font.ttf';bold=TMP/'bold.ttf'
shutil.copyfile('C:/Windows/Fonts/arial.ttf',font)
shutil.copyfile('C:/Windows/Fonts/arialbd.ttf',bold)

def run(args):
    r=subprocess.run([FFMPEG,'-hide_banner','-nostdin',*args],capture_output=True,creationflags=subprocess.CREATE_NO_WINDOW)
    if r.returncode:raise RuntimeError(r.stderr.decode(errors='replace')[-5000:])
    return r

def textfile(name,text):
    p=TMP/(name+'.txt');p.write_text(text,encoding='utf8');return p.as_posix()

regular=font.as_posix();heavy=bold.as_posix()
def draw(text,size,x,y,name,color='white'):
    p=textfile(name,text)
    return f"drawtext=fontfile='{heavy}':textfile='{p}':fontsize={size}:fontcolor={color}:x={x}:y={y}"

shots=[
 {'file':'01_menu.webm','start':0,'seconds':3,'caption':'YOUR CREW. YOUR CALL.','name':'menu'},
 {'file':'05_launch.webm','start':0,'seconds':4,'caption':'A DREAM BECOMES A MISSION.','name':'launch'},
 {'file':'02_building.webm','start':0.15,'seconds':6,'caption':'BUILD A HOME BEYOND EARTH.','name':'build'},
 {'file':'04_crew.webm','start':0,'seconds':5,'caption':'EVERY BUILDING SUPPORTS SOMEONE.','name':'crew'},
 {'file':'06_mission.webm','start':0,'seconds':6,'caption':'BALANCE RESOURCES. PLAN AHEAD.','name':'mission'},
]
args=['-y']
for shot in shots:args+=['-i',str(OUT/'raw'/shot['file'])]
args+=['-f','lavfi','-i','color=c=0x0b1430:s=1280x720:r=30:d=4']
music=ROOT/'Art/BGM/Starry Twinkles.mp3'
args+=['-i',str(music)]
filters=[]
for i,shot in enumerate(shots):
    d=shot['seconds']
    # Padding only holds the final recorded frame if Chrome ends a clip early.
    f=f'[{i}:v]trim=start={shot["start"]},setpts=PTS-STARTPTS,tpad=stop_mode=clone:stop_duration={d},trim=duration={d},fps=30,scale=1280:720,setsar=1,format=yuv420p'
    f+=',drawbox=x=0:y=535:w=1280:h=70:color=0x0b1430@0.90:t=fill'
    f+=','+draw(shot['caption'],32,'(w-text_w)/2',554,'caption-'+shot['name'])
    f+=f',fade=t=in:st=0:d=0.12,fade=t=out:st={d-.12}:d=0.12[v{i}]'
    filters.append(f)

end='[5:v]format=yuv420p'
end+=',drawbox=x=112:y=150:w=8:h=330:color=0x60d5e5:t=fill'
end+=','+draw('ASTROCIV',38,160,145,'end-kicker','0x60d5e5')
end+=','+draw('MAPMAKERS OF THE MOON',49,160,215,'end-title')
end+=','+draw('Every great civilization starts with a dream.',27,160,290,'end-tag','0xCDEAF4')
end+=','+draw('PLAY. LEARN. BUILD AGAIN.',34,160,370,'end-call','0xF2B84B')
end+=','+draw('Play free in your browser - link in the repository README',21,160,430,'end-link','0xCDEAF4')
end+=','+draw('Music: Starry Twinkles - raincalleer / Suno',17,160,540,'end-music','0xCDEAF4')
end+=','+draw('Recorded gameplay. Game art and music are AI-generated.',17,160,570,'end-ai','0xCDEAF4')
end+=',fade=t=in:st=0:d=0.15,fade=t=out:st=3.65:d=0.35[v5]'
filters.append(end)
filters.append('[v0][v1][v2][v3][v4][v5]concat=n=6:v=1:a=0[v]')
filters.append('[6:a]atrim=start=18:duration=28,asetpts=PTS-STARTPTS,loudnorm=I=-18:TP=-1.5:LRA=8,afade=t=in:st=0:d=0.4,afade=t=out:st=26.5:d=1.5[a]')
filterpath=TMP/'filters.txt';filterpath.write_text(';\n'.join(filters))
output=OUT/'AstroCiv_Trailer_28s.mp4'
description='Actual Unity WebGL gameplay captured with Chrome canvas MediaRecorder; AI-assisted shot selection and editing by OpenAI Codex. Game art is AI-created. Music: Starry Twinkles by raincalleer, generated with Suno, song f5182621-6979-42be-864b-69517b18be60. No synthetic gameplay footage.'
args+=['-filter_complex',filterpath.read_text(),'-map','[v]','-map','[a]',
 '-c:v','libx264','-preset','medium','-crf','19','-pix_fmt','yuv420p','-r','30',
 '-c:a','aac','-b:a','192k','-ar','48000','-t','28',
 '-movflags','+faststart+use_metadata_tags',
 '-metadata','title=AstroCiv - Mapmakers of the Moon - 28 second trailer',
 '-metadata','artist=AstroCiv; music by raincalleer / Suno',
 '-metadata','comment='+description,'-metadata','description='+description,str(output)]
print('Rendering 28-second trailer...',flush=True)
render=run(args);(TMP/'render.log').write_bytes(render.stderr)
check=run(['-i',str(output),'-vf','blackdetect=d=0.3:pix_th=0.02','-af','volumedetect','-f','null','-'])
log=check.stderr.decode(errors='replace');(TMP/'validation.log').write_text(log)
match=re.search(r'Duration: (\d+):(\d+):(\d+\.\d+)',log)
duration=int(match[1])*3600+int(match[2])*60+float(match[3])
assert 27.95<=duration<=28.05,duration
frames=re.findall(r'frame=\s*(\d+)',log);assert frames and int(frames[-1])==840,frames
assert 'Audio: aac' in log and 'Video: h264' in log
mean=re.search(r'mean_volume: ([-\d.]+) dB',log);peak=re.search(r'max_volume: ([-\d.]+) dB',log)
assert mean and float(mean[1])>-60,'Audio is silent'
assert peak and float(peak[1])<0,'Audio may clip'
run(['-i',str(output),'-vf','fps=1/3,scale=426:240,tile=3x3','-frames:v','1','-update','1',str(OUT/'trailer-contact-sheet.jpg')])
cursor=0;timeline=[]
for shot in shots:
    timeline.append({**shot,'timelineStart':cursor});cursor+=shot['seconds']
timeline.append({'name':'closing title','timelineStart':24,'seconds':4})
result={'file':output.name,'durationSeconds':duration,'width':1280,'height':720,'fps':30,'frames':840,'videoCodec':'H.264','audioCodec':'AAC','musicFile':str(music.relative_to(ROOT)),'musicExcerptSeconds':[18,46],'musicSourceSha256':hashlib.sha256(music.read_bytes()).hexdigest(),'musicMeanDb':float(mean[1]),'musicPeakDb':float(peak[1]),'shots':timeline,'provenance':description,'status':'local draft; not uploaded or submitted'}
(OUT/'TRAILER_MANIFEST.json').write_text(json.dumps(result,indent=2)+'\n')
print(json.dumps(result,indent=2),flush=True)
