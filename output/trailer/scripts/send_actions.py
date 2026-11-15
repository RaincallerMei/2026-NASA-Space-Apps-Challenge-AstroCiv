import json
from pathlib import Path
import sys
import urllib.request

control=json.loads(Path('tmp/trailer/control.json').read_text())
plan=Path(sys.argv[1]).read_bytes()
print('Sending recording actions to local browser',flush=True)
request=urllib.request.Request(f"http://127.0.0.1:{control['port']}/actions",data=plan,headers={'Content-Type':'application/json'})
with urllib.request.build_opener(urllib.request.ProxyHandler({})).open(request,timeout=180) as response:print(response.read().decode())
