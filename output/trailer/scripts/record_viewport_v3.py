"""Record only the isolated game viewport through Chrome's compositor."""
from pathlib import Path
import base64,json,subprocess,sys,threading,time,urllib.request
import websocket
ROOT=Path(__file__).resolve().parents[3]
TMP=ROOT/'tmp/trailer-v3'
RAW=ROOT/'output/trailer/raw-v3'
RAW.mkdir(parents=True,exist_ok=True)
FFMPEG=str(next((ROOT/'tmp/trailer-deps-v2/imageio_ffmpeg/binaries').glob('*.exe')))
profile=max(TMP.glob('profile-*'),key=lambda p:p.stat().st_mtime)
port=int((profile/'DevToolsActivePort').read_text().splitlines()[0])
opener=urllib.request.build_opener(urllib.request.ProxyHandler({}))
targets=json.load(opener.open(f'http://127.0.0.1:{port}/json'))
url=next(x['webSocketDebuggerUrl'] for x in targets if x['type']=='page')
class Connection:
 def __init__(self):
  self.ws=websocket.create_connection(url,origin='http://localhost',timeout=30);self.seq=0
 def call(self,method,params=None):
  self.seq+=1;current=self.seq
  self.ws.send(json.dumps(dict(id=current,method=method,params=params or {})))
  while True:
   r=json.loads(self.ws.recv())
   if r.get('id')==current:
    if 'error' in r:raise RuntimeError(r['error'])
    return r.get('result',{})
 def evaluate(self,code):
  r=self.call('Runtime.evaluate',dict(expression=code,awaitPromise=True,returnByValue=True))
  if 'exceptionDetails' in r:raise RuntimeError(r['exceptionDetails'])
  return r.get('result',{}).get('value')
cmd=Connection()
cmd.call('Page.bringToFront')
cmd.call('Emulation.setFocusEmulationEnabled',{'enabled':True})
recording=None
pointer=(1100,650)
records=[]
class Recorder:
 def __init__(self):
  self.conn=Connection();self.stop=threading.Event();self.frames=[];self.started=time.monotonic()
  self.folder=TMP/('frames-'+str(time.time_ns()));self.folder.mkdir()
  self.thread=threading.Thread(target=self.loop);self.thread.start()
 def loop(self):
  while not self.stop.is_set():
   begun=time.monotonic()
   r=self.conn.call('Page.captureScreenshot',dict(format='jpeg',quality=94,fromSurface=True,captureBeyondViewport=False))
   p=self.folder/f'{len(self.frames):05d}.jpg';p.write_bytes(base64.b64decode(r['data']))
   self.frames.append((p,begun-self.started,pointer))
   self.stop.wait(max(0,.075-(time.monotonic()-begun)))
 def finish(self,name):
  ended=time.monotonic()-self.started;self.stop.set();self.thread.join(timeout=40)
  if len(self.frames)<2:raise RuntimeError('Insufficient captured frames')
  self.conn.ws.close()
  concat=self.folder/'frames.ffconcat';lines=['ffconcat version 1.0']
  for i,(p,t,mouse) in enumerate(self.frames):
   duration=(self.frames[i+1][1] if i+1<len(self.frames) else ended)-t
   lines.extend([f"file '{p.name}'",f'duration {max(.001,duration):.6f}'])
  lines.append(f"file '{self.frames[-1][0].name}'")
  concat.write_text('\n'.join(lines)+'\n')
  out=RAW/(name+'.mp4')
  args=[FFMPEG,'-hide_banner','-loglevel','error','-y','-f','concat','-safe','0','-i',str(concat),'-vf','fps=30,format=yuv420p','-c:v','libx264','-preset','fast','-crf','17','-threads','3','-t',str(ended),'-an',str(out)]
  r=subprocess.run(args,capture_output=True,creationflags=subprocess.CREATE_NO_WINDOW)
  if r.returncode:raise RuntimeError(r.stderr.decode(errors='replace'))
  info=dict(file=str(out.relative_to(ROOT)),duration=ended,capturedFrames=len(self.frames),source='real isolated Chrome game viewport',pointerSamples=[dict(t=t,x=m[0],y=m[1]) for _,t,m in self.frames])
  records.append(info);print(json.dumps({k:v for k,v in info.items() if k!='pointerSamples'}),flush=True)
  return info

def perform(a):
 global recording,pointer
 kind=a['action']
 if kind=='wait':time.sleep(a.get('seconds',1))
 elif kind in ['click','move']:
  x=a['x']*1280;y=a['y']*720;pointer=(x,y)
  cmd.call('Input.dispatchMouseEvent',dict(type='mouseMoved',x=x,y=y))
  if kind=='click':
   cmd.call('Input.dispatchMouseEvent',dict(type='mousePressed',x=x,y=y,button='left',clickCount=1))
   time.sleep(.16)
   cmd.call('Input.dispatchMouseEvent',dict(type='mouseReleased',x=x,y=y,button='left',clickCount=1))
  time.sleep(a.get('after',.35))
 elif kind=='key':
  k=a['key'];v={' ':32,'Escape':27,'Enter':13}.get(k,0)
  for t in ['keyDown','keyUp']:cmd.call('Input.dispatchKeyEvent',dict(type=t,key=k,windowsVirtualKeyCode=v))
  time.sleep(a.get('after',.3))
 elif kind=='js':return cmd.evaluate(a['code'])
 elif kind=='screenshot':
  r=cmd.call('Page.captureScreenshot',dict(format='png',fromSurface=True))
  p=TMP/(a['name']+'.png');p.write_bytes(base64.b64decode(r['data']));return str(p)
 elif kind=='reload':
  cmd.call('Page.reload');time.sleep(12)
  cmd.evaluate("document.querySelector('#unity-container').style.cssText='position:fixed;left:0;top:0;transform:none;width:1280px;height:720px';document.querySelector('#unity-fullscreen-container').style.cssText='width:1280px;height:720px';document.querySelector('#unity-canvas').style.cssText='width:1280px;height:720px;display:block';document.querySelector('#unity-footer').style.display='none';true")
 elif kind=='record-start':recording=Recorder();time.sleep(.15)
 elif kind=='record-stop':
  r=recording.finish(a['name']);recording=None;return {k:v for k,v in r.items() if k!='pointerSamples'}
 else:raise ValueError(kind)
 return dict(action=kind,done=True)
plan=Path(sys.argv[1])
try:
 for a in json.loads(plan.read_text(encoding='utf-8-sig')):
  r=perform(a);print(json.dumps(dict(action=a['action'],result=r)),flush=True)
finally:
 if recording:recording.stop.set();recording.thread.join(timeout=40)
 cmd.ws.close()
 (TMP/(plan.stem+'-capture.json')).write_text(json.dumps(records,indent=2))
