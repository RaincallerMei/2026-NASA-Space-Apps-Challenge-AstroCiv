"""Build the approved Artemis-led 29-second trailer from authentic media."""
from pathlib import Path
import hashlib,json,re,shutil,subprocess
from PIL import Image,ImageDraw,ImageFont
ROOT=Path(__file__).resolve().parents[3]
OUT=ROOT/'output/trailer'
TMP=ROOT/'tmp/trailer-v3/edit'
TMP.mkdir(parents=True,exist_ok=True)
FFMPEG=str(next((ROOT/'tmp/trailer-deps-v2/imageio_ffmpeg/binaries').glob('*.exe')))
for src,dst in [('arial.ttf','regular.ttf'),('arialbd.ttf','bold.ttf')]:shutil.copyfile(Path('C:/Windows/Fonts')/src,TMP/dst)
W,H,FPS=1920,1080,30
BLUE='0x09132B'
CYAN='0x6BDCE5'
MUSIC=ROOT/'Art/BGM/Starry Twinkles.mp3'
DESCRIPTION=('29-second noncommercial educational Space Apps project demonstration. Original Artemis II crew poster: NASA, used as factual exploration context; no NASA/CSA or astronaut endorsement. '
 'Actual Unity WebGL gameplay and existing in-game cutscene artwork. Game artwork is AI-created; NASA poster is authentic agency media, not AI-generated. '
 'OpenAI Codex assisted with recording, editing, captions, research and documentation. No generated gameplay or voiceover. '
 'Music: Starry Twinkles by raincalleer, generated with Suno, song f5182621-6979-42be-864b-69517b18be60. '
 'Archived NASA data and simplified game rules. Isolated capture profile; mission access staged, scores unedited. Gameplay excerpts are shortened; still holds and crop changes are editorial.')

def run(args,log=None):
 r=subprocess.run([FFMPEG,'-hide_banner','-nostdin',*args],cwd=ROOT,capture_output=True,creationflags=subprocess.CREATE_NO_WINDOW)
 if log:(TMP/log).write_bytes(r.stderr)
 if r.returncode:raise RuntimeError(r.stderr.decode(errors='replace')[-7000:])
 return r.stderr.decode(errors='replace')

def text(value,size,x,y,name,color='white',bold=True):
 p=TMP/(name+'.txt');p.write_text(value,encoding='utf-8')
 font=(TMP/('bold.ttf' if bold else 'regular.ttf')).relative_to(ROOT).as_posix()
 return f"drawtext=fontfile='{font}':textfile='{p.relative_to(ROOT).as_posix()}':fontsize={size}:fontcolor={color}:x={x}:y={y}"

def labels(head,sub,name,ai=True):
 f=',drawbox=x=70:y=915:w=1780:h=3:color='+CYAN+':t=fill'
 f+=','+text(head,52,'(w-text_w)/2',943,name+'-head')
 if sub:f+=','+text(sub,25,'(w-text_w)/2',1014,name+'-sub','0xC8E5F1',False)
 if ai:f+=','+text('AI-created game art',18,'w-text_w-25',16,name+'-ai','0xC8E5F1',False)
 return f

SHOTS=[
 dict(name='artemis',kind='poster',file='sources/artemis-ii-crew-poster.jpg',duration=4,headline="Want to explore humanity's future on Moon or Mars?"),
 dict(name='resource_problem',kind='game',file='raw-v3/v3_resource_toggle.mp4',start=0,duration=1.3,crop='990:620:0:0',headline='Your crew needs a plan.',sub='Balance power, water and food.'),
 dict(name='resource_action',kind='game',file='raw-v3/v3_resource_toggle.mp4',start=1.1,duration=2.8,crop='875:330:0:240',headline='Save power. Grow less food.',sub='Switch the Greenhouse off.'),
 dict(name='resource_result',kind='hud',file='raw-v3/v3_resource_toggle.mp4',start=3.6,duration=2.9,headline='Save power. Grow less food.',sub='Every choice has a cost.'),
 dict(name='placement',kind='placement',file='raw-v3/v3_battery_placement.mp4',start=0,duration=5,speed=1.23,headline='Where you build matters.',sub='Shorter cables. More power.  |  Moon map: NASA LRO/LOLA'),
 dict(name='warning',kind='game',file='raw-v3/v3_storm_warning.mp4',start=0,duration=1.2,crop='580:340:350:205',headline='Real NASA science. Real decisions.',sub='Historical event: NASA DONKI. Timing simplified.'),
 dict(name='protect_action',kind='game',file='raw-v3/v3_storm_response.mp4',start=0,duration=1.4,speed=2.0,headline='Protect your crew.',sub='Recall your geologist. Power the Shelter.'),
 dict(name='protect_result',kind='game',file='raw-v3/v3_storm_response.mp4',start=6.8,duration=1.4,headline='Protect your crew.',sub='Recall your geologist. Power the Shelter.'),
 dict(name='story',kind='game',file='../../26NASAChallenge/Assets/_Game/Art/Resources/Cutscenes/colony.png',duration=2,headline='Your dream. Your command.',sub='Story Mode | Ages 7-12'),
 dict(name='free_menu',kind='game',file='raw-v3/v3_free_menu.mp4',start=.1,duration=1,crop='730:610:275:55',headline='Free Build. Keep experimenting.',sub='Moon + Mars'),
 dict(name='free_mars',kind='game',file='raw-v3/v3_free_mars.mp4',start=0,duration=1,crop='680:510:300:92',headline='Free Build. Keep experimenting.',sub='Moon + Mars'),
 dict(name='ending',kind='ending',file='../../26NASAChallenge/Assets/_Game/Art/Resources/Cutscenes/message.png',duration=5,headline='You can be next!')]
assert abs(sum(s['duration'] for s in SHOTS)-29)<1e-9
pieces=[]
for i,s in enumerate(SHOTS):
 source=(OUT/s['file']).resolve();d=s['duration'];image=source.suffix.lower() in ['.jpg','.png','.jpeg']
 args=['-y','-filter_complex_threads','2']
 if image:args+=['-loop','1','-framerate','30']
 args+=['-i',str(source)]
 n=f"setpts=PTS-STARTPTS,trim=start={s.get('start',0)},setpts=(PTS-STARTPTS)/{s.get('speed',1)},tpad=stop_mode=clone:stop_duration={d},trim=duration={d},fps=30"
 k=s['kind']
 if k=='poster':
  f='[0:v]'+n+',scale=1920:-2:flags=lanczos,crop=1920:1080:0:0,setsar=1,format=yuv420p'
  f+=',drawbox=x=0:y=842:w=1920:h=238:color=black@0.90:t=fill'
  f+=','+text("Want to explore humanity's future",61,'(w-text_w)/2',862,'opening-1')
  f+=','+text('on Moon or Mars?',61,'(w-text_w)/2',942,'opening-2')
  f+=','+text('Image: NASA  |  Independent educational Space Apps project',19,'(w-text_w)/2',1042,'poster-credit','0xC8E5F1',False)
 elif k=='ending':
  f='[0:v]'+n+',crop=1232:480:0:0,scale=1920:748:flags=lanczos,pad=1920:1080:0:0:color='+BLUE+',setsar=1,format=yuv420p'
  f+=','+text('You can be next!',98,'(w-text_w)/2',783,'ending-hero')
  f+=','+text('AstroCivilization',40,'(w-text_w)/2',920,'ending-title',CYAN)
  f+=','+text('raincallermei.github.io/2026-NASA-Space-Apps-Challenge-AstroCiv/',25,'(w-text_w)/2',996,'ending-url','0xC8E5F1',False)
  f+=','+text('AI-created game art',18,'w-text_w-25',16,'ending-ai','0xC8E5F1',False)
  f+=f',fade=t=out:st={d-.22}:d=0.22'
 elif k=='hud':
  f='[0:v]'+n+',split=2[basein][detail];[basein]scale=1600:900:flags=lanczos,pad=1920:1080:160:0:color='+BLUE+',setsar=1[base];'
  f+='[detail]crop=510:64:90:0,scale=1760:220:flags=lanczos[hud];[base][hud]overlay=80:105:shortest=1,format=yuv420p'
  f+=labels(s['headline'],s['sub'],s['name'])
 elif k=='placement':
  f='[0:v]'+n+',split=2[inspect][map];[inspect]crop=290:310:8:250,scale=570:610:flags=lanczos,pad=1920:1080:35:145:color='+BLUE+',setsar=1[base];'
  f+='[map]crop=675:510:305:92,scale=1190:900:flags=lanczos[mapzoom];[base][mapzoom]overlay=690:0:shortest=1,format=yuv420p'
  f+=labels(s['headline'],s['sub'],s['name'])
 else:
  f='[0:v]'+n
  if s.get('crop'):f+=',crop='+s['crop']
  f+=',scale=1700:900:force_original_aspect_ratio=decrease:flags=lanczos,pad=1920:1080:(ow-iw)/2:(900-ih)/2:color='+BLUE+',setsar=1,format=yuv420p'
  f+=labels(s['headline'],s.get('sub',''),s['name'])
 f+='[v]'
 (TMP/(s['name']+'-filter.txt')).write_text(f,encoding='utf-8')
 piece=TMP/f'{i:02d}-{s["name"]}.mp4'
 print('Rendering',s['name'],d,'seconds',flush=True)
 run(args+['-filter_complex',f,'-map','[v]','-an','-t',str(d),'-r','30','-c:v','libx264','-preset','fast','-crf','18','-threads','4','-pix_fmt','yuv420p',str(piece)],s['name']+'-render.log')
 pieces.append(piece)
concat=TMP/'pieces.ffconcat';concat.write_text('ffconcat version 1.0\n'+'\n'.join(f"file '{p.name}'" for p in pieces)+'\n')
final=OUT/'AstroCiv_Trailer_29s.mp4'
print('Mixing soundtrack and assembling 29-second export.',flush=True)
run(['-y','-f','concat','-safe','0','-i',str(concat),'-i',str(MUSIC),'-filter_complex','[1:a]atrim=start=18:duration=29,asetpts=PTS-STARTPTS,loudnorm=I=-18:TP=-1.5:LRA=8,afade=t=in:st=0:d=0.2,afade=t=out:st=27.8:d=1.2[a]','-map','0:v:0','-map','[a]','-c:v','copy','-c:a','aac','-b:a','192k','-ar','48000','-t','29','-movflags','+faststart+use_metadata_tags','-metadata','title=AstroCivilization - Artemis II - 29 second Space Apps demo','-metadata','artist=AstroCivilization; music raincalleer / Suno','-metadata','description='+DESCRIPTION,'-metadata','comment='+DESCRIPTION,str(final)],'assemble.log')
check=run(['-i',str(final),'-vf','blackdetect=d=0.3:pix_th=0.02','-af','volumedetect','-f','null','-'],'validation.log')
m=re.search(r'Duration: (\d+):(\d+):(\d+\.\d+)',check)
duration=int(m[1])*3600+int(m[2])*60+float(m[3]);frames=int(re.findall(r'frame=\s*(\d+)',check)[-1])
mean=float(re.search(r'mean_volume: ([-\d.]+) dB',check)[1]);peak=float(re.search(r'max_volume: ([-\d.]+) dB',check)[1])
assert duration==29 and frames==870,(duration,frames)
assert 'Video: h264' in check and 'Audio: aac' in check
assert 'black_duration:' not in check,'Sustained black frames'
assert -40<mean<-8 and peak<0,(mean,peak)
review=Image.new('RGB',(1440,((len(SHOTS)+2)//3)*290),'#09132b');draw=ImageDraw.Draw(review)
font=ImageFont.truetype('C:/Windows/Fonts/arial.ttf',17)
cursor=0;timeline=[]
for i,s in enumerate(SHOTS):
 frame=TMP/(s['name']+'-review.jpg')
 run(['-y','-ss',str(cursor+s['duration']/2),'-i',str(final),'-frames:v','1','-update','1',str(frame)])
 with Image.open(frame) as im:review.paste(im.resize((480,270)),((i%3)*480,(i//3)*290))
 draw.text(((i%3)*480+8,(i//3)*290+272),f'{cursor:g}-{cursor+s["duration"]:g}s | {s["name"]}',font=font,fill='white')
 timeline.append(dict(s,timelineStart=round(cursor,3)));cursor+=s['duration']
review.save(OUT/'trailer-29s-contact-sheet.jpg',quality=94)
hashfile=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
manifest=dict(file=final.name,durationSeconds=duration,frames=frames,width=W,height=H,fps=FPS,videoCodec='H.264',audioCodec='AAC',bytes=final.stat().st_size,audioMeanDb=mean,audioPeakDb=peak,shots=timeline,musicFile=str(MUSIC.relative_to(ROOT)),musicExcerptSeconds=[18,47],musicSourceSha256=hashfile(MUSIC),sourceSha256={s['file']:hashfile((OUT/s['file']).resolve()) for s in SHOTS},webBuildSha256={p.name:hashfile(p) for p in (ROOT/'docs/Build').iterdir() if p.is_file()},provenance=DESCRIPTION,validation='All 870 frames decoded; exactly 29.00 seconds; H.264/AAC; audible audio below clipping; no sustained black frames.',status='local export; not uploaded or submitted')
(OUT/'TRAILER_29S_MANIFEST.json').write_text(json.dumps(manifest,indent=2)+'\n',encoding='utf-8')
print(json.dumps({k:manifest[k] for k in ['file','durationSeconds','frames','bytes','audioMeanDb','audioPeakDb','validation']},indent=2),flush=True)
