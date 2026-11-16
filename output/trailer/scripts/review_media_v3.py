"""Inspect real capture sources and make a small review sheet."""
from pathlib import Path
import re
import subprocess
from PIL import Image, ImageDraw

ROOT = Path(__file__).resolve().parents[3]
TMP = ROOT/'tmp/trailer-v3'
TMP.mkdir(parents=True, exist_ok=True)
FFMPEG = str(next((ROOT/'tmp/trailer-deps-v2/imageio_ffmpeg/binaries').glob('*.exe')))
names = ['v2_power', 'v2_greenhouse', 'v2_storm_response', 'v2_storm_story', 'v2_building', 'v2_mars']
sheet = Image.new('RGB',(1440,6*300),'#09132b')
draw=ImageDraw.Draw(sheet)
for row,name in enumerate(names):
    path=ROOT/'output/trailer/raw'/(name+'.webm')
    meta=subprocess.run([FFMPEG,'-hide_banner','-i',str(path)],capture_output=True,creationflags=subprocess.CREATE_NO_WINDOW).stderr.decode(errors='replace')
    m=re.search(r'Duration: (\d+):(\d+):(\d+\.\d+)',meta)
    seconds=int(m[1])*3600+int(m[2])*60+float(m[3]) if m else 5
    print(name, seconds, flush=True)
    for col,f in enumerate([.15,.5,.85]):
        out=TMP/(name+'-'+str(col)+'.jpg')
        subprocess.run([FFMPEG,'-hide_banner','-loglevel','error','-y','-ss',str(seconds*f),'-i',str(path),'-frames:v','1','-update','1',str(out)],capture_output=True,creationflags=subprocess.CREATE_NO_WINDOW,check=True)
        with Image.open(out) as image:
            image.thumbnail((480,270))
            sheet.paste(image,(col*480,row*300))
        draw.text((col*480+8,row*300+274),f'{name} / {seconds*f:.2f}s',fill='white')
sheet.save(TMP/'existing-footage-review.jpg',quality=90)
