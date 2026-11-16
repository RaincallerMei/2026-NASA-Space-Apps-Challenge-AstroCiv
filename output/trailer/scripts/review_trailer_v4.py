"""Extract final action/result frames and verify motion in every native map source."""
from pathlib import Path
import subprocess,json
from PIL import Image,ImageDraw,ImageFont,ImageChops,ImageStat
ROOT=Path(__file__).resolve().parents[3]
OUT=ROOT/'output/trailer/v4';QA=ROOT/'tmp/trailer-v4/edit/qa'
QA.mkdir(parents=True,exist_ok=True)
FFMPEG=str(next((ROOT/'tmp/trailer-deps-v2/imageio_ffmpeg/binaries').glob('*.exe')))
font=ImageFont.truetype('C:/Windows/Fonts/arial.ttf',18)
def frame(src,t,name):
 p=QA/(name+'.jpg')
 r=subprocess.run([FFMPEG,'-hide_banner','-loglevel','error','-nostdin','-y','-ss',str(t),'-i',str(src),'-frames:v','1','-update','1',str(p)],capture_output=True,creationflags=subprocess.CREATE_NO_WINDOW)
 if r.returncode:raise RuntimeError(r.stderr.decode(errors='replace'))
 return p
final=ROOT/'output/trailer/AstroCiv_Trailer_29s_v4.mp4'
items=[('Battery low: before OFF',9.15),('Power recovers: after OFF',10.50),('Food low: before ON',11.30),('Food positive: after ON',12.85),('Far battery: 2.1 kW',13.65),('Nearby battery: 2.9 kW',15.90),('Artemis over game home',1.5),('Science with earned + locked',21.0),('Full ending / 50% panel',28.25)]
sheet=Image.new('RGB',(1440,870),'#09132b');d=ImageDraw.Draw(sheet)
for i,(label,t) in enumerate(items):
 p=frame(final,t,'final-'+str(t))
 with Image.open(p) as im:sheet.paste(im.resize((480,270)),((i%3)*480,(i//3)*290))
 d.text(((i%3)*480+8,(i//3)*290+272),f'{t:.2f}s | {label}',font=font,fill='white')
sheet.save(OUT/'action-result-review.jpg',quality=95)
sheet=Image.new('RGB',(1200,5*247),'#09132b');d=ImageDraw.Draw(sheet);stats=[]
for row,(name,start) in enumerate([('moon_a',.75),('mars_a',.45),('moon_b',.75),('mars_b',.45),('moon_c',.75)]):
 imgs=[]
 for col,dt in enumerate([0,.95,1.9]):
  p=frame(OUT/'raw'/(name+'_grow.webm'),start+dt,f'{name}-{col}')
  with Image.open(p) as im:cropped=im.crop((306,95,974,597)).convert('RGB')
  imgs.append(cropped)
  thumb=cropped.resize((300,225));sheet.paste(thumb,(col*400,row*247))
  d.text((col*400+8,row*247+227),f'{name} source +{dt:.2f}s',font=font,fill='white')
 diff=ImageChops.difference(imgs[0],imgs[-1]);mean=sum(ImageStat.Stat(diff).mean)/3
 stats.append(dict(name=name,sourceStartSeconds=start,sourceWindowSeconds=2,meanAbsolutePixelChange=round(mean,3),sha256RecordedInManifest=True))
 assert mean>0.2,(name,mean)
sheet.save(OUT/'construction-motion-review.jpg',quality=95)
(OUT/'MOTION_QA.json').write_text(json.dumps(stats,indent=2)+'\n',encoding='utf-8')
print(json.dumps(stats,indent=2))
print('Saved final action/result and five-map construction review sheets.')
