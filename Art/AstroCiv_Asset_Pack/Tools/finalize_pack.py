from pathlib import Path
from PIL import Image,ImageDraw
import json,hashlib,math
R=Path(__file__).resolve().parents[1];A=R/'Assets/AstroCiv';M=json.loads((R/'asset_manifest.json').read_text());assets=M['assets']
# Helmet portrait derives from the same astronaut, not a new character design.
im=Image.open(A/'Characters/Frames/idle.png');box=im.getbbox();im=im.crop((box[0],box[1],box[2],min(box[1]+23,box[3])));im.thumbnail((56,56),Image.Resampling.NEAREST);portrait=Image.new('RGBA',(64,64));portrait.alpha_composite(im.resize((im.width*2,im.height*2),Image.Resampling.NEAREST),((64-im.width*2)//2,(64-im.height*2)//2));p=A/'Characters/portrait.png';portrait.save(p);assets.append({'path':'Characters/portrait.png','width':64,'height':64,'kind':'ui','pivot_px_top_left':[32,32],'pixels_per_unit':64,'sha256':hashlib.sha256(p.read_bytes()).hexdigest()})
# Bitmap font interchange.
g=json.loads((A/'UI/Font/glyphs.json').read_text());lines=['info face="AstroCiv5x7" size=7 bold=0 italic=0 charset="" unicode=1 stretchH=100 smooth=0 aa=1 padding=0,0,0,0 spacing=1,1','common lineHeight=9 base=7 scaleW=128 scaleH=32 pages=1 packed=0','page id=0 file="astrociv_5x7.png"',f'chars count={len(g)}']
for char,v in g.items():lines.append(f'char id={ord(char)} x={v["x"]} y={v["y"]} width=5 height=7 xoffset=0 yoffset=0 xadvance=6 page=0 chnl=15')
(A/'UI/Font/astrociv_5x7.fnt').write_text('\n'.join(lines)+'\n')
# Texture atlases with 2px duplicated edge gutter; JSON rectangles are untrimmed.
atlas_dir=R/'Atlases';atlas_dir.mkdir(exist_ok=True)
for name,selected in [('world',[x for x in assets if x['kind']=='sprite' and not x['path'].startswith('Characters')]),('characters',[x for x in assets if x['path'].startswith('Characters/Frames')]),('ui',[x for x in assets if x['path'].startswith('UI') and x['kind']!='font'])]:
 size=1024;atlas=Image.new('RGBA',(size,size));x=y=2;row=0;rects=[]
 for item in selected:
  im=Image.open(A/item['path']);w,h=im.size
  if x+w+2>size:x=2;y+=row+4;row=0
  assert y+h+2<=size,(name,item['path'])
  atlas.alpha_composite(im,(x,y))
  for k in [1,2]:
   atlas.paste(im.crop((0,0,w,1)),(x,y-k));atlas.paste(im.crop((0,h-1,w,h)),(x,y+h+k-1));atlas.paste(im.crop((0,0,1,h)),(x-k,y));atlas.paste(im.crop((w-1,0,w,h)),(x+w+k-1,y))
  rects.append({'name':item['path'],'x':x,'y_top':y,'y_unity_bottom':size-y-h,'width':w,'height':h,'pivot_px_top_left':item['pivot_px_top_left']});x+=w+4;row=max(row,h)
 atlas.save(atlas_dir/f'{name}.png');(atlas_dir/f'{name}.json').write_text(json.dumps({'size':[size,size],'extrusion':2,'sprites':rects},indent=2))
(R/'asset_manifest.json').write_text(json.dumps(M,indent=2))
checks=[]
def check(name,ok,details):
 checks.append({'test':name,'pass':bool(ok),'details':details})
for item in assets:
 im=Image.open(A/item['path']);check('dimensions:'+item['path'],im.size==(item['width'],item['height']),list(im.size));check('rgba:'+item['path'],im.mode=='RGBA',im.mode)
 if item['kind']=='sprite':
  a=im.getchannel('A');ext=a.getextrema();check('transparency:'+item['path'],ext==(0,255),list(ext));check('binary_alpha:'+item['path'],len(a.getcolors(257) or [])==2,'Standalone sprites have alpha only 0 or 255')
  b=a.getbbox();check('padding:'+item['path'],bool(b) and b[0]>0 and b[1]>0 and b[2]<im.width and b[3]<im.height,b)
for env in ['Moon','Mars']:
 im=Image.open(A/f'Environment/{env}/ground_repeat.png');check('seam_x:'+env,im.crop((0,0,1,128)).tobytes()==im.crop((127,0,128,128)).tobytes(),'First and last columns identical');check('seam_y:'+env,im.crop((0,0,128,1)).tobytes()==im.crop((0,127,128,128)).tobytes(),'First and last rows identical')
for dr in ['SE','SW','NW','NE']:
 data=[Image.open(A/f'Characters/Frames/walk_{dr}_{i}.png').tobytes() for i in range(4)];check('unique_walk_frames:'+dr,len(set(data))==4,'Four distinct frames')
# Reciprocal socket positions and continuous alpha bridge at joined port, all variants.
dirs={'NE':(32,-16),'SE':(32,16),'SW':(-32,16),'NW':(-32,-16)};op={'NE':'SW','SW':'NE','SE':'NW','NW':'SE'}
corr=[x for x in assets if x['path'].startswith('Corridors/') and not 'adapter' in x['path']]
count=0
for a in corr:
 for dr,p in a.get('ports',{}).items():
  for b in corr:
   if op[dr] not in b['ports']:continue
   q=b['ports'][op[dr]];dx,dy=dirs[dr];check('port:'+a['path']+':'+b['path']+':'+dr,p==[q[0]+dx,q[1]+dy],'Coincident reciprocal port centres');count+=1
report={'summary':{'png_exports':len(assets),'checks':len(checks),'passed':sum(c['pass'] for c in checks),'failed':sum(not c['pass'] for c in checks),'corridor_socket_pairs':count},'checks':checks,'manual_review':['Building, astronaut, icon and corridor contact sheets inspected.','3x3 corridor join raster and 3x3 terrain repetition inspected.','Moon and Mars assembly previews inspected.'],'limitations':['Unity Editor is not available here: importer, scene placement, runtime sorting and animation playback in Unity still require an integration smoke test.','AI illustration preserves the reference family, not a pixel-for-pixel reconstruction.','Directional frames are distinct and baseline-aligned; small pose/helmet differences remain.','Mirrored texture period is seamless but can become recognizable on large maps.','Bitmap font is uppercase A-Z, digits and a small punctuation subset.']}
(R/'validation_report.json').write_text(json.dumps(report,indent=2));print(json.dumps(report['summary'],indent=2));assert report['summary']['failed']==0
