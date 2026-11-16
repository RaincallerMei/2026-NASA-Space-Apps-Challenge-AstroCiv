"""Close only the isolated browser created for this trailer capture."""
from pathlib import Path
import json,urllib.request,websocket
ROOT=Path(__file__).resolve().parents[3]
scratch=ROOT/'tmp/trailer-v4'
profile=max(scratch.glob('profile-*'),key=lambda p:p.stat().st_mtime)
assert scratch.resolve() in profile.resolve().parents
port=int((profile/'DevToolsActivePort').read_text().splitlines()[0])
opener=urllib.request.build_opener(urllib.request.ProxyHandler({}))
version=json.load(opener.open(f'http://127.0.0.1:{port}/json/version',timeout=5))
ws=websocket.create_connection(version['webSocketDebuggerUrl'],origin='http://localhost',timeout=5)
ws.send(json.dumps({'id':1,'method':'Browser.close'}))
try:ws.recv()
except Exception:pass
ws.close()
p=scratch/'control.json';info=json.loads(p.read_text())
info.update(source='isolated current Unity WebGL build in tmp/trailer-v4/web/',browserState='closed',profile=str(profile.relative_to(ROOT)))
p.write_text(json.dumps(info,indent=2)+'\n')
print('Closed only the isolated v4 recording browser.')
