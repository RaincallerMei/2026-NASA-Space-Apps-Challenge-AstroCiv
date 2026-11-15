"""Capture real Unity WebGL footage in an isolated Chrome profile, via CDP.
Task-local control process; only the game canvas is recorded, never the desktop.
"""
import base64
import http.server
import json
from pathlib import Path
import subprocess
import threading
import time
import urllib.parse
import urllib.request
import uuid
import websocket

ROOT = Path.cwd().resolve()
OUT = ROOT / 'output/trailer'
RAW = OUT / 'raw'
RAW.mkdir(parents=True, exist_ok=True)
SCRATCH = ROOT / 'tmp/trailer'
SCRATCH.mkdir(parents=True, exist_ok=True)
events = []
sequence = 0
lock = threading.Lock()
ws = None
chrome = None

def call(method, params=None):
    global sequence
    sequence += 1
    current = sequence
    ws.send(json.dumps({'id':current,'method':method,'params':params or {}}))
    while True:
        value=json.loads(ws.recv())
        if value.get('id') == current:
            if 'error' in value: raise RuntimeError(value['error'])
            return value.get('result',{})
        events.append(value)

def evaluate(code):
    value=call('Runtime.evaluate', {'expression':code,'awaitPromise':True,'returnByValue':True})
    if value.get('exceptionDetails'):raise RuntimeError(value['exceptionDetails'])
    return value.get('result',{}).get('value')

def bounds():
    return evaluate("(()=>{const r=document.querySelector('#unity-canvas').getBoundingClientRect();return {x:r.x,y:r.y,width:r.width,height:r.height,scale:1}})()")

def capture(name):
    r=call('Page.captureScreenshot', {'format':'png','clip':bounds(),'captureBeyondViewport':False})
    path=SCRATCH/(name+'.png')
    path.write_bytes(base64.b64decode(r['data']))
    return str(path.relative_to(ROOT))

def perform(a):
    kind=a['action']
    if kind=='wait':
        time.sleep(min(float(a.get('seconds',1)),60))
    elif kind=='click':
        b=bounds();x=b['x']+a['x']*b['width'];y=b['y']+a['y']*b['height']
        call('Input.dispatchMouseEvent',{'type':'mouseMoved','x':x,'y':y})
        call('Input.dispatchMouseEvent',{'type':'mousePressed','x':x,'y':y,'button':'left','clickCount':1})
        call('Input.dispatchMouseEvent',{'type':'mouseReleased','x':x,'y':y,'button':'left','clickCount':1})
        time.sleep(a.get('after',.45))
    elif kind=='key':
        key=a['key']; code={'Enter':13,'Escape':27,' ':32}.get(key,0)
        call('Input.dispatchKeyEvent',{'type':'keyDown','key':key,'windowsVirtualKeyCode':code})
        time.sleep(.06)
        call('Input.dispatchKeyEvent',{'type':'keyUp','key':key,'windowsVirtualKeyCode':code})
        time.sleep(a.get('after',.45))
    elif kind=='screenshot':return capture(a['name'])
    elif kind=='js':return evaluate(a['code'])
    elif kind=='record-start':
        return evaluate("""(()=>{
          const canvas=document.querySelector('#unity-canvas');
          const stream=canvas.captureStream(30);
          const type=MediaRecorder.isTypeSupported('video/webm;codecs=vp8')?'video/webm;codecs=vp8':'video/webm';
          window.__chunks=[];window.__capture=new MediaRecorder(stream,{mimeType:type,videoBitsPerSecond:7000000});
          window.__capture.ondataavailable=e=>{if(e.data.size)__chunks.push(e.data)};
          window.__recordStart=performance.now();__capture.start(250);
          return {width:canvas.width,height:canvas.height,mimeType:type};
        })()""")
    elif kind=='record-stop':
        meta=evaluate("""new Promise(resolve=>{
          __capture.onstop=async()=>{
            const blob=new Blob(__chunks,{type:__capture.mimeType});
            const bytes=new Uint8Array(await blob.arrayBuffer());
            let s='';for(let i=0;i<bytes.length;i+=32768)s+=String.fromCharCode(...bytes.subarray(i,i+32768));
            window.__captureBase64=btoa(s);resolve({bytes:bytes.length,wallSeconds:(performance.now()-__recordStart)/1000});
          };__capture.stop();
        })""")
        parts=[]
        for offset in range(0,evaluate('__captureBase64.length'),512000):
            parts.append(evaluate(f'__captureBase64.slice({offset},{offset+512000})'))
        path=RAW/(a['name']+'.webm')
        path.write_bytes(base64.b64decode(''.join(parts)))
        meta['file']=str(path.relative_to(ROOT));return meta
    elif kind=='reload':call('Page.reload');time.sleep(4)
    else:raise ValueError(kind)
    return {'action':kind,'done':True}

class Handler(http.server.SimpleHTTPRequestHandler):
    def __init__(self,*args,**kwargs):super().__init__(*args,directory=str(ROOT),**kwargs)
    def log_message(self,*args):pass
    def do_POST(self):
        global chrome
        try:
            if self.path!='/actions':self.send_error(404);return
            plan=json.loads(self.rfile.read(int(self.headers['Content-Length'])))
            result=[]
            with lock:
                for action in plan:
                    r=perform(action);result.append(r)
                    print(json.dumps({'action':action['action'],'result':r}),flush=True)
            data=json.dumps({'ok':True,'results':result}).encode()
            self.send_response(200);self.send_header('Content-Type','application/json');self.end_headers();self.wfile.write(data)
        except Exception as e:
            data=json.dumps({'ok':False,'error':str(e)}).encode()
            self.send_response(500);self.end_headers();self.wfile.write(data)

server=http.server.ThreadingHTTPServer(('127.0.0.1',0),Handler)
threading.Thread(target=server.serve_forever,daemon=True).start()
profile=SCRATCH/('profile-'+uuid.uuid4().hex[:8])
try:
    chrome=subprocess.Popen([
      r'C:\Program Files\Google\Chrome\Application\chrome.exe','--headless=new',
      '--remote-debugging-port=0','--remote-allow-origins=http://localhost',
      '--no-first-run','--no-default-browser-check','--disable-background-networking',
      '--disable-background-timer-throttling','--disable-renderer-backgrounding','--disable-backgrounding-occluded-windows',
      '--use-angle=swiftshader','--enable-unsafe-swiftshader','--mute-audio',
      '--user-data-dir='+str(profile),'about:blank'],stdout=subprocess.DEVNULL,stderr=subprocess.DEVNULL,creationflags=subprocess.CREATE_NO_WINDOW)
    portfile=profile/'DevToolsActivePort'
    deadline=time.monotonic()+20
    while not portfile.exists() and time.monotonic()<deadline:time.sleep(.2)
    port=int(portfile.read_text().splitlines()[0])
    targets=json.load(urllib.request.urlopen(f'http://127.0.0.1:{port}/json'))
    ws=websocket.create_connection(next(x['webSocketDebuggerUrl'] for x in targets if x['type']=='page'),origin='http://localhost',timeout=120)
    call('Page.enable');call('Runtime.enable');call('Log.enable')
    call('Emulation.setDeviceMetricsOverride',{'width':1280,'height':720,'deviceScaleFactor':1,'mobile':False})
    call('Page.navigate',{'url':f'http://127.0.0.1:{server.server_port}/docs/'})
    end=time.monotonic()+120
    while time.monotonic()<end:
        if evaluate("document.querySelector('#unity-loading-container')?.style.display==='none'"):break
        time.sleep(1)
    evaluate("document.body.style.margin='0';document.body.style.background='#0b1430';document.querySelector('#unity-container').style.cssText='position:fixed;inset:0;width:1280px;height:720px';document.querySelector('#unity-fullscreen-container').style.cssText='width:1280px;height:720px';document.querySelector('#unity-canvas').style.cssText='width:1280px;height:720px;display:block';document.querySelector('#unity-footer').style.display='none'")
    call('Page.bringToFront')
    time.sleep(2)
    capture('ready')
    info={'port':server.server_port,'root':str(ROOT),'recording':'canvas only, 1280x720, target 30 fps','source':'local docs/ WebGL build'}
    (SCRATCH/'control.json').write_text(json.dumps(info))
    print('READY '+json.dumps(info),flush=True)
    while True:time.sleep(1)
except KeyboardInterrupt:pass
finally:
    (SCRATCH/'capture-events.json').write_text(json.dumps([e for e in events if e.get('method') in ['Runtime.exceptionThrown']],indent=2))
    if ws:ws.close()
    if chrome:
        chrome.terminate()
        try:chrome.wait(timeout=8)
        except subprocess.TimeoutExpired:chrome.kill()
    server.shutdown();server.server_close()
