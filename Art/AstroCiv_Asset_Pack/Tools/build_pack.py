from PIL import Image, ImageDraw, ImageFont, ImageFilter
import numpy as np
from scipy import ndimage
from pathlib import Path
import json, shutil, hashlib, math
ROOT=Path(__file__).resolve().parents[1]
SRC=ROOT/'Masters'
OUT=ROOT/'Assets'/'AstroCiv'
for p in ['Buildings','Vehicles','Characters/Frames','Characters/Sheets','Environment/Moon','Environment/Mars','Environment/Space','Corridors','UI/Panels','UI/Icons','UI/Thumbnails','UI/Font','Overlays','Shadows']:(OUT/p).mkdir(parents=True,exist_ok=True)
(ROOT/'Previews').mkdir(exist_ok=True)
PAL=['#080f29','#172344','#354168','#606b95','#9097bc','#bdbdd8','#e1e0ef','#ffffff','#007994','#00b4d1','#00e2ee','#9bf8ff','#0546a8','#167ce5','#70c8ff','#23633c','#40a742','#85d94c','#d2f18b','#7a4142','#ad5950','#d37b59','#eda274','#ffd0a0','#9b692f','#e3ac3e','#ffe969']
COL=np.array([tuple(bytes.fromhex(c[1:])) for c in PAL],dtype=np.int32)
N,W,C,L=PAL[0],PAL[7],PAL[10],PAL[5]
manifest=[]
def quant(im):
 a=np.array(im.convert('RGBA')); rgb=a[:,:,:3].astype(np.int32)
 ix=np.argmin(((rgb[:,:,None,:]-COL[None,None,:,:])**2).sum(3),axis=2)
 a[:,:,:3]=COL[ix];a[:,:,3]=np.where(a[:,:,3]>=160,255,0);a[a[:,:,3]==0]=0
 return Image.fromarray(a)
def clean(im,threshold=200):
 a=np.array(im.convert('RGBA'));mask=a[:,:,3]>=threshold
 lab,n=ndimage.label(mask);counts=np.bincount(lab.ravel());counts[0]=0
 if n:mask=np.isin(lab,np.where(counts>max(8,counts.max()*.0007))[0])
 # Binary silhouette, retaining substantial disconnected details.
 a[:,:,3]=mask.astype('uint8')*255;a[~mask]=0
 im=Image.fromarray(a);return im.crop(im.getbbox()) if im.getbbox() else im

def save(im,rel,foot=None,pivot=None,kind='sprite',extra=None):
 path=OUT/rel;path.parent.mkdir(parents=True,exist_ok=True);im.save(path)
 obj={'path':str(path.relative_to(OUT)),'width':im.width,'height':im.height,'kind':kind,'pivot_px_top_left':pivot or [im.width//2,im.height-8],'pixels_per_unit':64,'sha256':hashlib.sha256(path.read_bytes()).hexdigest()}
 if foot:obj['footprint_cells']=foot
 if extra:obj.update(extra)
 manifest.append(obj);return im

def sprite(source,box,size,fit,rel,foot=None):
 im=clean(Image.open(SRC/source).crop(box));scale=min(fit[0]/im.width,fit[1]/im.height)
 im=quant(im.resize((max(1,round(im.width*scale)),max(1,round(im.height*scale))),Image.Resampling.NEAREST))
 out=Image.new('RGBA',size);out.alpha_composite(im,((size[0]-im.width)//2,size[1]-8-im.height))
 return save(out,rel,foot)

core='architecture.png';equip='equipment.png';mob='shelter_airlock_rover.png';add='functional.png'
builds=[('command_hub',core,(300,0,900,724),(192,192),(150,174),[3,3]),('habitat',core,(1100,0,1500,724),(128,128),(86,96),[2,2]),('greenhouse',core,(1530,0,2172,724),(192,192),(146,116),[3,2]),('battery_bank',equip,(100,0,750,724),(128,128),(82,80),[2,1]),('communications_dish',equip,(800,0,1400,724),(192,192),(92,144),[2,2]),('solar_array',equip,(1450,0,2172,724),(128,128),(112,88),[2,2]),('regolith_shelter',mob,(0,0,900,724),(192,192),(160,90),[3,3]),('airlock',mob,(920,0,1400,724),(128,128),(52,64),[1,1]),('laboratory',add,(0,0,730,724),(192,192),(112,132),[2,2]),('life_support',add,(740,0,1380,724),(192,192),(110,100),[2,2]),('rover_garage',add,(1400,0,2172,724),(192,192),(158,112),[3,2])]
for name,src,box,size,fit,foot in builds:
 im=sprite(src,box,size,fit,'Buildings/'+name+'.png',foot)
 thumb=im.crop(im.getbbox());thumb.thumbnail((56,56),Image.Resampling.NEAREST);t=Image.new('RGBA',(64,64));t.alpha_composite(thumb,((64-thumb.width)//2,(64-thumb.height)//2));save(t,'UI/Thumbnails/'+name+'.png',kind='ui')
 sh=Image.new('RGBA',size);d=ImageDraw.Draw(sh);x=size[0]//2;y=size[1]-15;ww=int(fit[0]*.85);d.ellipse((x-ww//2,y-ww//6,x+ww//2,y+ww//6),fill=(8,15,41,80));save(sh,'Shadows/'+name+'.png',kind='shadow')
sprite(mob,(1450,0,2172,724),(128,128),(92,70),'Vehicles/rover_six_wheel.png',[2,1])
# Astronauts share one scale within each master and a fixed baseline.
poses=Image.open(SRC/'astronaut_poses.png');walk=Image.open(SRC/'astronaut_walk.png')
for i,name in enumerate(['idle','walk_pose','wave','tablet','repair']):
 box=(int(i*poses.width/5),0,int((i+1)*poses.width/5),poses.height)
 im=clean(poses.crop(box));
 # Wave marks removed: retain largest connected component.
 a=np.array(im);lab,n=ndimage.label(a[:,:,3]>0);counts=np.bincount(lab.ravel());counts[0]=0;a[:,:,3]=np.where(lab==counts.argmax(),255,0);im=Image.fromarray(a);im=im.crop(im.getbbox())
 scale=38/410
 im=quant(im.resize((round(im.width*scale),round(im.height*scale)),Image.Resampling.NEAREST))
 if im.width>30:im=im.resize((30,round(im.height*30/im.width)),Image.Resampling.NEAREST)
 frame=Image.new('RGBA',(32,48));frame.alpha_composite(im,((32-im.width)//2,44-im.height));save(frame,'Characters/Frames/'+name+'.png',pivot=[16,44])
for r,direction in enumerate(['SE','SW','NW','NE']):
 sheet=Image.new('RGBA',(128,48));frames=[]
 for c in range(4):
  cell=walk.crop((c*walk.width//4,r*walk.height//4,(c+1)*walk.width//4,(r+1)*walk.height//4));im=clean(cell)
  scale=38/270;im=quant(im.resize((round(im.width*scale),round(im.height*scale)),Image.Resampling.NEAREST));
  if im.width>30 or im.height>42:
   im.thumbnail((30,42),Image.Resampling.NEAREST)
  frame=Image.new('RGBA',(32,48));frame.alpha_composite(im,((32-im.width)//2,44-im.height));save(frame,f'Characters/Frames/walk_{direction}_{c}.png',pivot=[16,44]);sheet.alpha_composite(frame,(32*c,0));frames.append(frame)
 save(sheet,f'Characters/Sheets/walk_{direction}.png',kind='sheet',extra={'frame_size':[32,48],'frame_count':4,'fps':6,'loop':True})
 previews=[]
 for f in frames:
  b=Image.new('RGBA',(32,48),PAL[1]);b.alpha_composite(f);previews.append(b.resize((192,288),Image.Resampling.NEAREST).convert('RGB'))
 previews[0].save(ROOT/'Previews'/f'walk_{direction}.gif',save_all=True,append_images=previews[1:],duration=160,loop=0)
# Lunar props and consistent Mars palette replacements.
terrain=Image.open(SRC/'terrain_props.png')
boxes=[(0,120,370,450),(370,120,860,460),(870,120,1536,470),(0,540,370,900),(370,540,860,900),(870,500,1536,930)]
for i,box in enumerate(boxes):
 name=('crater_' if i<3 else 'rocks_')+['small','medium','large'][i%3];sz=[32,64,128][i%3];fit=[26,54,112][i%3]
 im=sprite('terrain_props.png',box,(sz,sz),(fit,fit//2+8),'Environment/Moon/'+name+'.png')
 a=np.array(im);mapping={0:19,1:19,2:19,3:20,4:21,5:22,6:23,7:23}
 for x,y in mapping.items():
  mask=(a[:,:,:3]==COL[x]).all(2)&(a[:,:,3]>0);a[mask,:3]=COL[y]
 save(Image.fromarray(a),'Environment/Mars/'+name+'.png')
back=Image.open(SRC/'backdrops_ground.png')
for name,box in [('Moon',(20,60,1655,212)),('Mars',(20,280,1655,441))]:
 im=clean(back.crop(box),220);im=quant(im.resize((512,64),Image.Resampling.NEAREST));save(im,f'Environment/{name}/ridge.png',kind='backdrop')
# Sample generated texture interior, reduce contrast and make periodic with mirrored quadrants.
im=back.crop((635,530,1035,890)).resize((64,64),Image.Resampling.NEAREST).convert('RGB');a=np.array(im);lum=a.mean(2);ix=np.where(lum<157,0,np.where(lum<173,1,2));mp=np.array([[151,159,190],[162,169,198],[173,179,206]],dtype=np.uint8);tile=Image.fromarray(mp[ix]).convert('RGBA')
full=Image.new('RGBA',(128,128));full.paste(tile,(0,0));full.paste(tile.transpose(Image.Transpose.FLIP_LEFT_RIGHT),(64,0));full.paste(tile.transpose(Image.Transpose.FLIP_TOP_BOTTOM),(0,64));full.paste(tile.transpose(Image.Transpose.ROTATE_180),(64,64))
for env in ['Moon','Mars']:
 tex=full.copy()
 if env=='Mars':
  a=np.array(tex);mp2=np.array([[189,105,79],[200,117,86],[211,129,94]],dtype=np.uint8)
  for k in range(3):a[(a[:,:,:3]==mp[k]).all(2),:3]=mp2[k]
  tex=Image.fromarray(a)
 save(tex,f'Environment/{env}/ground_repeat.png',kind='texture',extra={'wrap':'repeat'})
 rep=Image.new('RGBA',(384,384))
 for y in range(3):
  for x in range(3):rep.paste(tex,(x*128,y*128))
 rep.save(ROOT/'Previews'/f'{env.lower()}_terrain_3x3.png')
 # Geometry mask only: use generated texture in a 64x32 ground diamond.
 diamond=tex.resize((64,32),Image.Resampling.NEAREST);mask=Image.new('L',(64,32));ImageDraw.Draw(mask).polygon([(31,0),(63,15),(32,31),(0,16)],fill=255);diamond.putalpha(mask);save(diamond,f'Environment/{env}/ground_diamond.png',pivot=[32,16],kind='tile')
save(Image.new('RGBA',(512,256),(3,7,20,255)),'Environment/Space/black_space.png',kind='backdrop')
save(Image.new('RGBA',(512,256),(255,193,157,255)),'Environment/Mars/peach_sky.png',kind='backdrop')
sprite('earth.png',(0,0,1254,1254),(64,64),(52,52),'Environment/Space/earth.png')
# Decals: small geometric impressions and sparse equipment dust.
rng=np.random.default_rng(70)
for env,color in [('Moon',(96,107,149,140)),('Mars',(122,65,66,140))]:
 for kind in ['footprints','rover_tracks']:
  im=Image.new('RGBA',(64,64));d=ImageDraw.Draw(im)
  for n in range(7):
   x=10+n*6;y=46-n*3
   if kind=='footprints':d.rectangle((x,y,x+2,y+3),fill=color);d.rectangle((x+5,y+2,x+7,y+5),fill=color)
   else:
    for off in [-5,5]:d.line((x+off,y,x+off+4,y-2),fill=color,width=2)
  save(im,f'Environment/{env}/{kind}.png',kind='decal')
for name,*_ in builds:
 src=Image.open(OUT/'Buildings'/f'{name}.png');a=np.array(src);ov=np.zeros_like(a);ys,xs=np.where(a[:,:,3]>0)
 if len(ys):
  pick=(rng.random(len(ys))<.025)&(xs%3==0);ov[ys[pick],xs[pick]]=[211,123,89,90]
 save(Image.fromarray(ov),'Overlays/'+name+'_mars_dust.png',kind='overlay')
# Mars regolith surface overlay preserves the shared reinforced entrance.
base=Image.open(OUT/'Buildings/regolith_shelter.png');a=np.array(base);ex=Image.new('L',base.size);ImageDraw.Draw(ex).polygon([(72,191),(72,151),(88,137),(132,133),(151,147),(153,191)],fill=255);mask=(a[:,:,3]>0)&(np.array(ex)==0);ov=np.zeros_like(a)
for old,new in {0:19,1:19,2:19,3:20,4:21,5:22,6:23,7:23}.items():
 use=mask&(a[:,:,:3]==COL[old]).all(2);ov[use,:3]=COL[new];ov[use,3]=255
save(Image.fromarray(ov),'Overlays/regolith_shelter_mars_surface.png',kind='overlay')
# Technical corridor reconstruction from the generated straight-corridor design.
# All ports share a 0.375-cell wide, 12px tall rectangular cross-section.
from shapely.geometry import box as rect
from shapely.ops import unary_union
D={'NE':(0,-1),'SE':(1,0),'SW':(0,1),'NW':(-1,0)}
variants={'straight_NE_SW':['NE','SW'],'straight_NW_SE':['NW','SE'],'corner_NE_SE':['NE','SE'],'corner_SE_SW':['SE','SW'],'corner_SW_NW':['SW','NW'],'corner_NW_NE':['NW','NE'],'tee_NE_SE_SW':['NE','SE','SW'],'tee_SE_SW_NW':['SE','SW','NW'],'tee_SW_NW_NE':['SW','NW','NE'],'tee_NW_NE_SE':['NW','NE','SE'],'cross':['NE','SE','SW','NW'],'end_NE':['NE'],'end_SE':['SE'],'end_SW':['SW'],'end_NW':['NW']}
def project(u,v,z=0):return (round(48+32*(u-v)),round(40+16*(u+v)-z))
portdata={k:list(project(u*.5,v*.5)) for k,(u,v) in D.items()}
for name,dirs in variants.items():
 w=.1875;parts=[rect(-w,-w,w,w)]
 for dr in dirs:
  u,v=D[dr]
  parts.append(rect(min(-w,u*.5),min(-w,v*.5),max(w,u*.5),max(w,v*.5)))
 poly=unary_union(parts);coords=list(poly.exterior.coords);im=Image.new('RGBA',(96,64));draw=ImageDraw.Draw(im)
 edges=list(zip(coords[:-1],coords[1:]));edges.sort(key=lambda e:sum(e[0])+sum(e[1]))
 for p,q in edges:
  quad=[project(*p),project(*q),project(*q,12),project(*p,12)];draw.polygon(quad,fill=PAL[4],outline=N)
  # Windows on visible wall strips, kept away from sockets.
  if abs(p[0]-q[0])+abs(p[1]-q[1])>.26:
   p1=(p[0]*.75+q[0]*.25,p[1]*.75+q[1]*.25);q1=(p[0]*.25+q[0]*.75,p[1]*.25+q[1]*.75)
   draw.polygon([project(*p1,4),project(*q1,4),project(*q1,8),project(*p1,8)],fill=C)
 draw.polygon([project(*p,12) for p in coords],fill=PAL[6],outline=N)
 # Roof rib across central cell, minimal deterministic construction.
 draw.line([project(-w,-w,13),project(w,-w,13)],fill=W,width=2)
 # Port caps have identical endpoints and cover the raster seam by one pixel.
 for dr in dirs:
  u,v=D[dr];p=(u*.5+(w if v else 0),v*.5+(w if u else 0));q=(u*.5-(w if v else 0),v*.5-(w if u else 0));draw.line([project(*p,12),project(*q,12)],fill=PAL[5],width=1)
 save(im,'Corridors/'+name+'.png',[1,1],[48,40],extra={'ports':{k:portdata[k] for k in dirs},'port_width_cells':.375,'height_px':12})
for dr in D:
 im=Image.open(OUT/'Corridors'/f'end_{dr}.png');save(im,f'Corridors/adapter_{dr}.png',[1,1],[48,40],extra={'ports':{dr:portdata[dr]},'building_overlap_px':6})
# Exact stepped UI borders, no baked copy in reusable panels.
def panel(size,fill=PAL[1],border=C):
 im=Image.new('RGBA',size);d=ImageDraw.Draw(im);w,h=size
 def path(inset):
  x=inset;y=inset;xx=w-1-inset;yy=h-1-inset;s=5
  return [(x+s,y),(xx-s,y),(xx,y+s),(xx,yy-s),(xx-s,yy),(x+s,yy),(x,yy-s),(x,y+s)]
 d.polygon(path(0),fill=N);d.polygon(path(1),fill=W);d.polygon(path(3),fill=border);d.polygon(path(4),fill=fill)
 return im
for name,size in [('resource_bar',(384,40)),('selection_panel',(512,64)),('window',(320,224)),('tooltip',(192,64)),('build_card',(80,96)),('nine_slice',(32,32))]:
 save(panel(size),'UI/Panels/'+name+'.png',kind='ui',extra={'border_lbrt':[8,8,8,8],'scale_mode':'sliced'})
for name,color in [('normal',C),('hover',PAL[11]),('pressed',PAL[8]),('disabled',PAL[3])]:save(panel((112,32),color),'UI/Panels/button_'+name+'.png',kind='ui',extra={'border_lbrt':[8,8,8,8]})
# Resource and action icons constructed on exact 24px grids.
for name in ['ore','water','power','food','crew','oxygen','research','build','repair','info','close','next','warning','pause']:
 im=Image.new('RGBA',(24,24));d=ImageDraw.Draw(im)
 if name=='ore':
  for x,y,s in [(3,13,8),(9,6,10),(14,14,7)]:d.polygon([(x,y),(x+s//2,y-3),(x+s,y+1),(x+s-1,y+7),(x+1,y+7)],fill=L,outline=N);d.line((x+1,y,x+s//2,y-2),fill=W,width=2)
 elif name=='water':d.polygon([(12,1),(4,13),(4,18),(8,22),(16,22),(20,18),(20,13)],fill=PAL[9],outline=PAL[8]);d.line((7,14,7,18,10,20),fill=PAL[11],width=2)
 elif name=='power':d.polygon([(10,1),(20,1),(14,10),(21,10),(6,23),(10,14),(3,14)],fill=PAL[26],outline=PAL[25])
 elif name=='food':d.polygon([(11,20),(8,13),(10,7),(20,3),(20,12),(16,18)],fill=PAL[17]);d.polygon([(10,18),(3,15),(2,7),(8,9),(12,15)],fill=PAL[16]);d.line((9,22,15,10),fill=PAL[15],width=2)
 elif name=='crew':
  for x in [3,13]:d.rectangle((x+1,4,x+5,9),fill=W);d.polygon([(x+1,11),(x+5,11),(x+7,14),(x+7,21),(x-1,21),(x-1,14)],fill=W)
 elif name=='oxygen':d.ellipse((3,3,20,20),outline=C,width=3);d.ellipse((8,8,15,15),fill=PAL[11])
 elif name=='research':d.polygon([(8,2),(16,2),(14,11),(21,20),(20,22),(4,22),(3,20),(10,11)],fill=C,outline=PAL[8]);d.rectangle((7,17,17,20),fill=PAL[11])
 elif name in ['repair','build']:d.line((5,20,18,7),fill=W,width=4);d.arc((12,1,22,11),0,290,fill=C,width=3)
 elif name=='info':d.ellipse((2,2,21,21),outline=C,width=2);d.rectangle((11,10,13,18),fill=W);d.rectangle((11,5,13,7),fill=W)
 elif name=='close':d.line((5,5,19,19),fill=W,width=3);d.line((19,5,5,19),fill=W,width=3)
 elif name=='next':d.polygon([(7,3),(20,12),(7,21)],fill=N)
 elif name=='warning':d.polygon([(12,2),(23,22),(1,22)],fill=PAL[26]);d.rectangle((11,8,13,15),fill=N);d.rectangle((11,18,13,20),fill=N)
 elif name=='pause':d.rectangle((5,4,9,20),fill=W);d.rectangle((15,4,19,20),fill=W)
 save(im,'UI/Icons/'+name+'.png',kind='ui')
# Original 5x7 pixel alphabet, exact editable bitmap glyph source.
patterns={'A':['01110','10001','10001','11111','10001','10001','10001'],'B':['11110','10001','10001','11110','10001','10001','11110'],'C':['01111','10000','10000','10000','10000','10000','01111'],'D':['11110','10001','10001','10001','10001','10001','11110'],'E':['11111','10000','10000','11110','10000','10000','11111'],'F':['11111','10000','10000','11110','10000','10000','10000'],'G':['01111','10000','10000','10111','10001','10001','01111'],'H':['10001','10001','10001','11111','10001','10001','10001'],'I':['11111','00100','00100','00100','00100','00100','11111'],'J':['00111','00010','00010','00010','10010','10010','01100'],'K':['10001','10010','10100','11000','10100','10010','10001'],'L':['10000','10000','10000','10000','10000','10000','11111'],'M':['10001','11011','10101','10101','10001','10001','10001'],'N':['10001','11001','11001','10101','10011','10011','10001'],'O':['01110','10001','10001','10001','10001','10001','01110'],'P':['11110','10001','10001','11110','10000','10000','10000'],'Q':['01110','10001','10001','10001','10101','10010','01101'],'R':['11110','10001','10001','11110','10100','10010','10001'],'S':['01111','10000','10000','01110','00001','00001','11110'],'T':['11111','00100','00100','00100','00100','00100','00100'],'U':['10001','10001','10001','10001','10001','10001','01110'],'V':['10001','10001','10001','10001','10001','01010','00100'],'W':['10001','10001','10001','10101','10101','10101','01010'],'X':['10001','10001','01010','00100','01010','10001','10001'],'Y':['10001','10001','01010','00100','00100','00100','00100'],'Z':['11111','00001','00010','00100','01000','10000','11111'],'0':['01110','10001','10011','10101','11001','10001','01110'],'1':['00100','01100','00100','00100','00100','00100','01110'],'2':['01110','10001','00001','00010','00100','01000','11111'],'3':['11110','00001','00001','01110','00001','00001','11110'],'4':['00010','00110','01010','10010','11111','00010','00010'],'5':['11111','10000','10000','11110','00001','00001','11110'],'6':['01110','10000','10000','11110','10001','10001','01110'],'7':['11111','00001','00010','00100','01000','01000','01000'],'8':['01110','10001','10001','01110','10001','10001','01110'],'9':['01110','10001','10001','01111','00001','00001','01110'],'+':['00000','00100','00100','11111','00100','00100','00000'],'/':['00001','00001','00010','00100','01000','10000','10000'],':':['00000','00100','00100','00000','00100','00100','00000'],'-':['00000','00000','00000','11111','00000','00000','00000'],'.':['00000']*6+['00100'],' ':['00000']*7}
def textpx(im,xy,text,color=W,scale=1):
 d=ImageDraw.Draw(im);x,y=xy
 for ch in text.upper():
  for j,row in enumerate(patterns.get(ch,patterns[' '])):
   for i,v in enumerate(row):
    if v=='1':d.rectangle((x+i*scale,y+j*scale,x+(i+1)*scale-1,y+(j+1)*scale-1),fill=color)
  x+=6*scale
atlas=Image.new('RGBA',(128,32));glyphs={}
for i,ch in enumerate(patterns):
 x=(i%16)*8;y=(i//16)*8;textpx(atlas,(x,y),ch);glyphs[ch]={'x':x,'y':y,'width':5,'height':7,'advance':6}
save(atlas,'UI/Font/astrociv_5x7.png',kind='font');(OUT/'UI/Font/glyphs.json').write_text(json.dumps(glyphs,indent=2));(OUT/'UI/Font/editable_glyphs.json').write_text(json.dumps(patterns,indent=2))
logo=Image.new('RGBA',(288,48));textpx(logo,(0,10),'ASTRO',scale=5);textpx(logo,(150,10),'CIV',C,scale=5);save(logo,'UI/astrociv_wordmark.png',kind='ui')
# Contact sheets, composed strictly from actual exports.
font=ImageFont.truetype('/usr/share/fonts/truetype/dejavu/DejaVuSans.ttf',12)
def contact(paths,name,cols=4,cw=200,ch=220):
 canvas=Image.new('RGBA',(cols*cw,math.ceil(len(paths)/cols)*ch),PAL[1]);d=ImageDraw.Draw(canvas)
 for i,path in enumerate(paths):
  im=Image.open(path);im.thumbnail((cw-16,ch-35),Image.Resampling.NEAREST);x=(i%cols)*cw;y=(i//cols)*ch;canvas.alpha_composite(im,(x+(cw-im.width)//2,y+ch-30-im.height));d.text((x+8,y+ch-23),path.stem,fill=W,font=font)
 canvas.convert('RGB').save(ROOT/'Previews'/name)
contact(list((OUT/'Buildings').glob('*.png'))+list((OUT/'Vehicles').glob('*.png')),'buildings_contact.png')
contact(list((OUT/'Characters/Frames').glob('*.png')),'astronaut_contact.png',7,100,90)
contact(list((OUT/'Corridors').glob('*.png')),'corridors_contact.png',5,140,105)
contact(list((OUT/'UI/Icons').glob('*.png')),'icons_contact.png',7,100,70)
# Corridor 3x3 full connectivity preview, ports meet at exact grid offsets.
prev=Image.new('RGBA',(320,210),PAL[1])
for u,v in sorted([(u,v) for u in range(3) for v in range(3)],key=lambda x:sum(x)):
 im=Image.open(OUT/'Corridors/cross.png');prev.alpha_composite(im,(112+32*(u-v),20+16*(u+v)))
prev.resize((960,630),Image.Resampling.NEAREST).save(ROOT/'Previews/corridor_join_test.png')
# Moon and Mars illustrative assembly at native scale with shared assets.
for env in ['Moon','Mars']:
 scene=Image.new('RGBA',(768,512));sky=Image.open(OUT/('Environment/Space/black_space.png' if env=='Moon' else 'Environment/Mars/peach_sky.png'));scene.paste(sky.resize((768,512)),(0,0));tex=Image.open(OUT/f'Environment/{env}/ground_repeat.png')
 for y in range(128,512,128):
  for x in range(0,768,128):scene.paste(tex,(x,y))
 ridge=Image.open(OUT/f'Environment/{env}/ridge.png').resize((768,96),Image.Resampling.NEAREST);scene.alpha_composite(ridge,(0,56))
 if env=='Moon':scene.alpha_composite(Image.open(OUT/'Environment/Space/earth.png'),(620,12))
 placements=[('communications_dish',95,180),('solar_array',590,185),('habitat',256,256),('battery_bank',544,240),('command_hub',384,320),('regolith_shelter',110,404),('greenhouse',512,384)]
 for dx,dy,corr in [(-32,-16,'straight_NW_SE'),(32,-16,'straight_NE_SW'),(32,16,'straight_NW_SE')]:
  for step in range(1,4 if dy>0 or dx<0 else 6):
   co=Image.open(OUT/f'Corridors/{corr}.png');scene.alpha_composite(co,(384+dx*step-48,320+dy*step-40))
 for prop,x,y in [('crater_medium',665,360),('rocks_medium',48,270),('rocks_small',230,410),('rocks_small',660,220),('crater_small',165,330)]:
  scene.alpha_composite(Image.open(OUT/f'Environment/{env}/{prop}.png'),(x,y))
 for name,x,y in sorted(placements,key=lambda p:p[2]):
  im=Image.open(OUT/f'Buildings/{name}.png');scene.alpha_composite(im,(x-im.width//2,y-im.height+8))
  if env=='Mars' and name=='regolith_shelter':scene.alpha_composite(Image.open(OUT/'Overlays/regolith_shelter_mars_surface.png'),(x-im.width//2,y-im.height+8))
  if env=='Mars':scene.alpha_composite(Image.open(OUT/f'Overlays/{name}_mars_dust.png'),(x-im.width//2,y-im.height+8))
 scene.alpha_composite(Image.open(OUT/'Vehicles/rover_six_wheel.png'),(255,310));scene.alpha_composite(Image.open(OUT/'Characters/Frames/wave.png'),(431,324));scene.alpha_composite(Image.open(OUT/'Characters/Frames/tablet.png'),(497,396))
 bar=Image.open(OUT/'UI/Panels/resource_bar.png');scene.alpha_composite(bar,(8,8))
 for i,(ico,num) in enumerate([('ore','320'),('water','180'),('power','260'),('food','120'),('crew','24')]):
  x=18+i*74;scene.alpha_composite(Image.open(OUT/f'UI/Icons/{ico}.png'),(x,16));textpx(scene,(x+27,24),num)
 p=Image.open(OUT/'UI/Panels/selection_panel.png').resize((752,64),Image.Resampling.NEAREST);scene.alpha_composite(p,(8,440));scene.alpha_composite(Image.open(OUT/'UI/Thumbnails/greenhouse.png'),(16,440));textpx(scene,(88,451),'GREENHOUSE',scale=2);textpx(scene,(88,474),'GROWING FOOD FOR THE COLONY');textpx(scene,(390,453),'FOOD +12 / DAY' if env=='Moon' else 'FOOD +12 / SOL',C);textpx(scene,(390,475),'WORKERS: 2 / 4');scene.alpha_composite(Image.open(OUT/'UI/Panels/button_normal.png'),(632,456));textpx(scene,(640,468),'NEXT DAY' if env=='Moon' else 'NEXT SOL',N)
 scene.convert('RGB').resize((1536,1024),Image.Resampling.NEAREST).save(ROOT/'Previews'/f'{env.lower()}_assembly.png')
(ROOT/'asset_manifest.json').write_text(json.dumps({'grid':{'tile_width':64,'tile_height':32,'axes_screen_px':{'u':[32,16],'v':[-32,16]}},'assets':manifest},indent=2))
(ROOT/'palette.json').write_text(json.dumps({'hex':PAL,'terrain_extra_colors':mp.tolist()+[[189,105,79],[200,117,86],[211,129,94]]},indent=2))
print('Built',len(manifest),'PNG exports')
