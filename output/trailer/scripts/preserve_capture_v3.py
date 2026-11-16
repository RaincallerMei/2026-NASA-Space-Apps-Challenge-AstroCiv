import json,time,base64,urllib.request
from pathlib import Path
import websocket
root=Path.cwd();scratch=root/'tmp/trailer-v3'
profile=max(scratch.glob('profile-*'),key=lambda p:p.stat().st_mtime)
port=int((profile/'DevToolsActivePort').read_text().splitlines()[0])
opener=urllib.request.build_opener(urllib.request.ProxyHandler({}))
targets=json.load(opener.open(f'http://127.0.0.1:{port}/json'))
ws=websocket.create_connection(next(x['webSocketDebuggerUrl'] for x in targets if x['type']=='page'),origin='http://localhost',timeout=30)
i=0
def call(method,params=None):
 global i
 i+=1;ws.send(json.dumps(dict(id=i,method=method,params=params or {})))
 while True:
  r=json.loads(ws.recv())
  if r.get('id')==i:
   if 'error' in r:raise RuntimeError(r['error'])
   return r.get('result',{})
code="const original=HTMLCanvasElement.prototype.getContext;HTMLCanvasElement.prototype.getContext=function(type,attrs){if(type==='webgl'||type==='webgl2'||type==='experimental-webgl')attrs={...(attrs||{}),preserveDrawingBuffer:true};return original.call(this,type,attrs)};"
print('Enabling faithful canvas recording in this isolated browser only.',flush=True)
print(call('Page.addScriptToEvaluateOnNewDocument',{'source':code}),flush=True)
call('Page.reload');time.sleep(12)
style="document.querySelector('#unity-container').style.cssText='position:fixed;left:0;top:0;transform:none;width:1280px;height:720px';document.querySelector('#unity-fullscreen-container').style.cssText='width:1280px;height:720px';document.querySelector('#unity-canvas').style.cssText='width:1280px;height:720px;display:block';document.querySelector('#unity-footer').style.display='none';document.querySelector('#unity-canvas').getContext('webgl2').getContextAttributes()"
print(call('Runtime.evaluate',{'expression':style,'returnByValue':True}),flush=True)
ws.close()
