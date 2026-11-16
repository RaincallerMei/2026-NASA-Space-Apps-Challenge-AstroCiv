from pathlib import Path
import json,urllib.request,websocket
root=Path.cwd();scratch=root/'tmp/trailer-v3'
profile=max(scratch.glob('profile-*'),key=lambda p:p.stat().st_mtime)
port=int((profile/'DevToolsActivePort').read_text().splitlines()[0])
opener=urllib.request.build_opener(urllib.request.ProxyHandler({}))
version=json.load(opener.open(f'http://127.0.0.1:{port}/json/version',timeout=5))
ws=websocket.create_connection(version['webSocketDebuggerUrl'],origin='http://localhost',timeout=5)
ws.send(json.dumps({'id':1,'method':'Browser.close'}))
try:ws.recv()
except Exception:pass
ws.close()
print('Closed only the isolated v3 recording browser.')
