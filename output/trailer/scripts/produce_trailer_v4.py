"""Produce the approved 29-second v4 trailer from continuous game recordings."""
from pathlib import Path
import hashlib,json,math,re,shutil,subprocess
from PIL import Image,ImageDraw,ImageFont
ROOT=Path(__file__).resolve().parents[3]
OUT=ROOT/'output/trailer/v4';EDIT=ROOT/'tmp/trailer-v4/edit'
EDIT.mkdir(parents=True,exist_ok=True)
FFMPEG=str(next((ROOT/'tmp/trailer-deps-v2/imageio_ffmpeg/binaries').glob('*.exe')))
FPS=30;W=1920;H=1080;BG='0x09132B';CYAN='0x6BDCE5';HANDLE=2
for a,b in [('arial.ttf','regular.ttf'),('arialbd.ttf','bold.ttf')]:shutil.copy2(Path('C:/Windows/Fonts')/a,EDIT/b)
FONT=ImageFont.truetype(str(EDIT/'bold.ttf'),52)
ASSETS=[]

def run(args,log):
 r=subprocess.run([FFMPEG,'-hide_banner','-nostdin',*args],cwd=ROOT,capture_output=True,creationflags=subprocess.CREATE_NO_WINDOW)
 (EDIT/log).write_bytes(r.stderr)
 if r.returncode:raise RuntimeError(r.stderr.decode(errors='replace')[-7000:])
 return r.stderr.decode(errors='replace')

def draw(value,size,x,y,name,color='white',bold=True,animated=False):
 if not value:return ''
 p=EDIT/(name+'.txt');p.write_text(value,encoding='utf-8')
 f=(EDIT/('bold.ttf' if bold else 'regular.ttf')).relative_to(ROOT).as_posix()
 alpha=":alpha='min(t/0.16,1)'" if animated else ''
 return f",drawtext=fontfile='{f}':textfile='{p.relative_to(ROOT).as_posix()}':fontsize={size}:fontcolor={color}:x={x}:y={y}{alpha}"

def encode(args,graph,name,frames):
 p=EDIT/(name+'.mp4');(EDIT/(name+'-filter.txt')).write_text(graph,encoding='utf-8')
 run(['-y','-filter_complex_threads','2',*args,'-filter_complex',graph,'-map','[v]','-an','-frames:v',str(frames),'-r','30','-c:v','libx264','-preset','fast','-crf','18','-threads','4','-pix_fmt','yuv420p','-video_track_timescale','15360',str(p)],name+'-render.log')
 return p

def nfilter(start,speed,frames):
 return f'setpts=PTS-STARTPTS,trim=start={start},setpts=(PTS-STARTPTS)/{speed},fps=30,tpad=stop_mode=clone:stop_duration=4,trim=end_frame={frames},settb=1/30,setsar=1'

def leaf(name,source,frames,start=0,speed=1,mode='full',zoom=0):
 source=Path(source);ASSETS.append(dict(name=name,file=str(source.relative_to(ROOT)),start=start,speed=speed,frames=frames,mode=mode))
 args=[]
 if source.suffix.lower() in ['.png','.jpg','.jpeg']:args+=['-loop','1','-framerate','30']
 if name=='opening':args+=['-stream_loop','-1']
 args+=['-i',str(source)]
 nf=nfilter(start,speed,frames)
 if mode=='hook':
  poster=ROOT/'output/trailer/sources/artemis-ii-crew-poster.jpg';args+=['-loop','1','-framerate','30','-i',str(poster)]
  g='[0:v]'+nf+',scale=1920:1080,eq=brightness=-0.12,format=yuv420p[home];'
  g+='[1:v]fps=30,scale=w=\'2*trunc(1280*(0.94+0.06*min(t/0.42,1))/2)\':h=-2:eval=frame,format=rgba,fade=t=in:st=0.18:d=0.25:alpha=1[poster];'
  g+='[home][poster]overlay=x=(W-w)/2:y=55:shortest=1,drawbox=x=0:y=846:w=1920:h=234:color=0x09132B@0.64:t=fill'
  g+=draw("Want to explore humanity's future",58,'(w-text_w)/2',868,'hook-question1',animated=True)
  g+=draw('on Moon and Mars?',58,'(w-text_w)/2',943,'hook-question2',animated=True)
  g+=draw('Image: NASA | Independent educational project',18,'(w-text_w)/2',1044,'hook-credit','0xC8E5F1',False)
  g+=',drawbox=x=1600:y=0:w=320:h=65:color=0x09132B@0.75:t=fill'
  g+=draw('AI-created game background',17,'w-text_w-22',18,'hook-bg-ai','0xC8E5F1',False)
 elif mode=='hud':
  g='[0:v]'+nf+',split=2[b0][h0];[b0]crop=1280:656:0:64,scale=1600:820,pad=1920:1080:160:80:color='+BG+'[base];'
  g+='[h0]crop=510:64:90:0,scale=1760:220:flags=lanczos[detail];[base][detail]overlay=80:42:shortest=1,format=yuv420p'
 elif mode=='placement':
  g='[0:v]'+nf+',split=2[inspect][map];[inspect]crop=290:252:8:250,scale=570:496:flags=lanczos,pad=1920:1080:35:190:color='+BG+'[base];'
  g+='[map]crop=675:510:305:92,scale=1190:900:flags=lanczos[m];[base][m]overlay=690:0:shortest=1,format=yuv420p'
 elif mode=='ending':
  g='[0:v]'+nf+',split=2[bg0][fg0];[bg0]scale=1920:1080:force_original_aspect_ratio=increase,crop=1920:1080,gblur=sigma=25,eq=brightness=-0.18[back];'
  g+='[fg0]scale=1920:1080:force_original_aspect_ratio=decrease[front];[back][front]overlay=(W-w)/2:(H-h)/2:shortest=1,format=yuv420p'
 else:
  g='[0:v]'+nf
  if mode=='map':g+=',crop=668:502:306:95';sw,sh=1200,900
  elif mode=='science':g+=',crop=1068:616:106:52';sw,sh=1560,900
  elif mode=='warning':g+=',crop=580:340:350:205';sw,sh=1536,900
  elif mode=='controls':g+=',crop=730:610:275:55';sw,sh=1076,900
  else:sw,sh=1600,900
  g+=f',scale={sw}:{sh}:flags=lanczos'
  if zoom:
   z=f'1+0.05*(1-cos(PI*on/{max(1,frames-1)}))/2' if zoom>0 else f'1.05-0.05*(1-cos(PI*on/{max(1,frames-1)}))/2'
   g+=f",zoompan=z='{z}':x='iw/2-iw/(2*zoom)':y='ih/2-ih/(2*zoom)':d=1:s={sw}x{sh}:fps=30"
  g+=f',pad=1920:1080:{(1920-sw)//2}:0:color='+BG+',format=yuv420p'
 g+=',settb=1/30,setpts=PTS-STARTPTS[v]'
 return encode(args,g,name,frames)

def labels(path,name,frames,title,subtitle='',ending=False,case=False):
 f='[0:v]fps=30,settb=1/30,setpts=PTS-STARTPTS'
 if ending:
  f+=',drawbox=x=0:y=840:w=1920:h=240:color=0x09132B@0.50:t=fill'
  f+=draw(title,80,'(w-text_w)/2',870,name+'-title')
  f+=draw('AstroCivilization',32,'(w-text_w)/2',974,name+'-brand',CYAN)
  f+=draw('raincallermei.github.io/2026-NASA-Space-Apps-Challenge-AstroCiv/',21,'(w-text_w)/2',1030,name+'-url','0xC8E5F1',False)
 else:
  size=52
  while ImageFont.truetype(str(EDIT/'bold.ttf'),size).getlength(title)>1780:size-=1
  f+=',drawbox=x=70:y=916:w=1780:h=3:color='+CYAN+':t=fill'
  f+=draw(title,size,'(w-text_w)/2',942,name+'-title',animated=True)
  if subtitle:f+=draw(subtitle,27,'(w-text_w)/2',1015,name+'-sub','0xC8E5F1',False,True)
 f+=draw('AI-created game art',18,'w-text_w-24',16,name+'-ai','0xC8E5F1',False)
 f+='[v]'
 return encode(['-i',str(path)],f,name,frames)

def pad(path,name,left,right,frames):
 f=f'[0:v]fps=30,settb=1/30,setpts=PTS-STARTPTS,tpad=start_mode=clone:start_duration={left/30}:stop_mode=clone:stop_duration={right/30},trim=end_frame={frames+left+right},format=yuv420p[v]'
 return encode(['-i',str(path)],f,name,frames+left+right)

def blend_join(parts,lengths,name,fade=4):
 padded=[pad(p,name+'-pad-'+str(i),fade//2 if i else 0,fade//2 if i<len(parts)-1 else 0,n) for i,(p,n) in enumerate(zip(parts,lengths))]
 args=[]
 for p in padded:args+=['-i',str(p)]
 graph=[]
 for i in range(len(parts)):graph.append(f'[{i}:v]fps=30,settb=1/30,setpts=PTS-STARTPTS[a{i}]')
 previous='a0';cursor=lengths[0]
 for i in range(1,len(parts)):
  nxt='v' if i==len(parts)-1 else 'j'+str(i)
  graph.append(f'[{previous}][a{i}]xfade=transition=fade:duration={fade/30}:offset={(cursor-fade//2)/30}[{nxt}]')
  previous=nxt;cursor+=lengths[i]
 return encode(args,';'.join(graph),name,sum(lengths))

def concat(parts,name,frames):
 args=[];g=[]
 for i,p in enumerate(parts):args+=['-i',str(p)];g.append(f'[{i}:v]fps=30,settb=1/30,setpts=PTS-STARTPTS[a{i}]')
 g.append(''.join(f'[a{i}]' for i in range(len(parts)))+f'concat=n={len(parts)}:v=1:a=0[v]')
 return encode(args,';'.join(g),name,frames)

raw=OUT/'raw'
print('Rendering home-screen Artemis opening.',flush=True)
hook=leaf('opening',raw/'home.webm',90,mode='hook')
print('Rendering 4x Moon and Mars construction with 5 percent camera motion.',flush=True)
bridge=leaf('invitation-bridge',raw/'home.webm',13,zoom=1)
moon=leaf('invitation-moon',raw/'moon_a_build.webm',45,start=.65,speed=4,mode='map',zoom=1)
mars=leaf('invitation-mars',raw/'mars_a_build.webm',45,start=3.7,speed=4,mode='map',zoom=-1)
invite=blend_join([bridge,moon,mars],[13,45,45],'invitation-base')
invite=labels(invite,'invitation',103,'Build the first Moon base for human civilization!','and Mars base')
balance=leaf('balance-base',raw/'resources_overview.webm',73,start=.1,mode='hud')
balance=labels(balance,'balance',73,'Balance power, water, air and food')
print('Rendering both genuine Greenhouse trade-offs.',flush=True)
off=leaf('manage-off-base',raw/'greenhouse_off.webm',66,start=.45,mode='hud')
on=leaf('manage-on-base',raw/'greenhouse_on.webm',67,start=.45,mode='hud')
off=labels(off,'manage-off',66,'','Power running low: switch OFF')
on=labels(on,'manage-on',67,'','Food running low: switch ON')
manage=blend_join([off,on],[66,67],'management')
manage=labels(manage,'management-title',133,'Switch buildings on and off to actively learn resource tradeoffs')
place=leaf('placement-base',raw/'placement.webm',103,start=.15,speed=1.5,mode='placement')
place=labels(place,'placement',103,'Place your building strategically','Learn interactions between different buildings')
print('Rendering NASA warning and earned Science Achievements collection.',flush=True)
warning=leaf('science-warning',raw/'storm_warning.webm',27,start=.1,mode='warning')
facts=leaf('science-facts',raw/'science_facts.webm',151,start=.15,mode='science')
science=blend_join([warning,facts],[27,151],'science-base')
science=labels(science,'science',178,'Unlock all Science Achievements!','each one backed by real Space Agency data.')
free=leaf('free-base',raw/'free_menu.webm',58,start=.15,mode='controls')
free=labels(free,'free',58,'Build your base on different Moon & Mars maps','Explore the countless possibilities')
print('Rendering five 0.50-second construction videos at 4x.',flush=True)
cores=[]
for name,start in [('moon_a',.75),('mars_a',.45),('moon_b',.75),('mars_b',.45),('moon_c',.75)]:
 cores.append(leaf('montage-'+name,raw/(name+'_grow.webm'),15,start=start,speed=4,mode='map',zoom=1))
sequence=[]
# Extra handles and 2-frame blends are separate from the five 15-frame gameplay cores.
lead=encode(['-i',str(cores[0])],'[0:v]fps=30,trim=end_frame=2,settb=1/30,setpts=PTS-STARTPTS[v]','montage-lead',2);sequence.append(lead)
for i,c in enumerate(cores):
 sequence.append(c)
 if i<len(cores)-1:
  g='[0:v]fps=30,trim=start_frame=13:end_frame=15,settb=1/30,setpts=PTS-STARTPTS[a];[1:v]fps=30,trim=end_frame=2,settb=1/30,setpts=PTS-STARTPTS[b];'
  g+="[a][b]blend=all_expr='A*(1-min(T/(2/30),1))+B*min(T/(2/30),1)'[v]"
  sequence.append(encode(['-i',str(c),'-i',str(cores[i+1])],g,'montage-blend-'+str(i),2))
tail=encode(['-i',str(cores[-1])],'[0:v]fps=30,trim=start_frame=13:end_frame=15,settb=1/30,setpts=PTS-STARTPTS[v]','montage-tail',2);sequence.append(tail)
variety=concat(sequence,'variety-base',87)
variety=labels(variety,'variety',87,'Build your base on different Moon & Mars maps','Explore the countless possibilities')
ending=leaf('ending-base',OUT/'ending-full-drawings.png',45,mode='ending')
ending=labels(ending,'ending',45,'You are the next explorer!',ending=True)
chapters=[hook,invite,balance,manage,place,science,free,variety,ending]
lengths=[90,103,73,133,103,178,58,87,45]
assert sum(lengths)==870
print('Blending sections; final invitation begins at 27.50 seconds.',flush=True)
video=blend_join(chapters,lengths,'video-29s')
final=ROOT/'output/trailer/AstroCiv_Trailer_29s_v4.mp4'
description=('Revised 29-second educational Space Apps trailer. Original Artemis II poster: NASA; independent project without agency/astronaut endorsement. '
 'Continuous 30 fps real Unity WebGL gameplay, edited with OpenAI Codex, Python and FFmpeg. '
 'Construction sequences play at 4x. Five distinct maps have 0.50-second gameplay cores and nominal 0.07-second transitions (two frames at 30 fps). '
 'Greenhouse OFF and ON cases use naturally reached low battery and low food states. Science Facts were earned by actual play; no achievements or scores were fabricated. '
 'Game art is AI-created; the ending monitor was edited with built-in imagegen to show classroom drawings. NASA poster was not AI-generated or regenerated. '
 'Music: Pastel Twinkles by raincalleer, generated with Suno; song 0a49d446-3f17-465c-8971-c0398cc51905. Supplied 30.01-second track gently fitted to 29 seconds. '
 'Historical NASA/CSA references and simplified educational simulation rules. Captured from an isolated current build/profile; mission-selection prerequisites staged only.')
music=ROOT/'Art/BGM/Pastel Twinkles.mp3'
run(['-y','-i',str(video),'-i',str(music),'-filter_complex','[1:a]atrim=duration=30.01,asetpts=PTS-STARTPTS,atempo=1.034827586,loudnorm=I=-18:TP=-1.5:LRA=8,apad=pad_dur=1,atrim=duration=29,afade=t=in:st=0:d=0.18,afade=t=out:st=28.4:d=0.6[a]','-map','0:v:0','-map','[a]','-c:v','copy','-c:a','aac','-b:a','192k','-ar','48000','-t','29','-movflags','+faststart+use_metadata_tags','-metadata','title=AstroCivilization - Explore humanity future - 29 seconds','-metadata','artist=AstroCivilization; music raincalleer / Suno','-metadata','description='+description,'-metadata','comment='+description,str(final)],'final-mix.log')
check=run(['-i',str(final),'-vf','blackdetect=d=0.3:pix_th=0.02','-af','volumedetect','-f','null','-'],'validation.log')
m=re.search(r'Duration: (\d+):(\d+):(\d+\.\d+)',check);duration=int(m[1])*3600+int(m[2])*60+float(m[3]);frames=int(re.findall(r'frame=\s*(\d+)',check)[-1])
mean=float(re.search(r'mean_volume: ([-\d.]+) dB',check)[1]);peak=float(re.search(r'max_volume: ([-\d.]+) dB',check)[1])
assert duration==29 and frames==870,(duration,frames)
assert 'black_duration:' not in check and 'Video: h264' in check and 'Audio: aac' in check
assert -40<mean<-8 and peak<0,(mean,peak)
labels_by_scene=[('Artemis hook',"Want to explore humanity's future on Moon and Mars?"),('Moon / Mars construction','Build the first Moon base for human civilization!'),('Resource balance','Balance power, water, air and food'),('Both resource trade-offs','Switch buildings on and off to actively learn resource tradeoffs'),('Strategic placement','Place your building strategically'),('Science Achievements','Unlock all Science Achievements!'),('Free Build','Build your base on different Moon & Mars maps'),('Five moving maps','Build your base on different Moon & Mars maps'),('Invitation','You are the next explorer!')]
timeline=[];cursor=0
sheet=Image.new('RGB',(1440,3*290),'#09132b');sd=ImageDraw.Draw(sheet);font=ImageFont.truetype('C:/Windows/Fonts/arial.ttf',17)
for i,(n,(name,headline)) in enumerate(zip(lengths,labels_by_scene)):
 a=cursor/30;b=(cursor+n)/30;sample=EDIT/f'final-scene-{i}.jpg'
 run(['-y','-ss',str((a+b)/2),'-i',str(final),'-frames:v','1','-update','1',str(sample)],f'qa-{i}.log')
 with Image.open(sample) as im:sheet.paste(im.resize((480,270)),((i%3)*480,(i//3)*290))
 sd.text(((i%3)*480+8,(i//3)*290+272),f'{a:.2f}-{b:.2f}s | {name}',font=font,fill='white')
 timeline.append(dict(name=name,startSeconds=a,endSeconds=b,frames=n,headline=headline));cursor+=n
sheet.save(OUT/'contact-sheet.jpg',quality=94)
hashfile=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
manifest=dict(file=str(final.relative_to(ROOT)),durationSeconds=duration,frames=frames,width=W,height=H,fps=FPS,bytes=final.stat().st_size,audioMeanDb=mean,audioPeakDb=peak,chapters=timeline,sceneTransitionFrames=4,mapTransitionFrames=2,mapGameplayCoreFrames=15,constructionSpeed=4,cameraScaleRange=[1,1.05],endingPanelOpacity=.5,musicFile=str(music.relative_to(ROOT)),musicSourceSha256=hashfile(music),sources=ASSETS,sourceSha256={s['file']:hashfile(ROOT/s['file']) for s in ASSETS},posterSourceSha256=hashfile(ROOT/'output/trailer/sources/artemis-ii-crew-poster.jpg'),captureBuildSha256={p.name:hashfile(p) for p in (ROOT/'tmp/trailer-v4/web/Build').iterdir() if p.is_file()},description=description,validation='All 870 frames decoded; exactly 29.00 seconds; H.264/AAC; audible unclipped soundtrack; no sustained black frames.',status='Local file; not uploaded or submitted.')
(OUT/'MANIFEST.json').write_text(json.dumps(manifest,indent=2)+'\n',encoding='utf-8')
print(json.dumps({k:manifest[k] for k in ['file','durationSeconds','frames','bytes','audioMeanDb','audioPeakDb','validation']},indent=2),flush=True)
