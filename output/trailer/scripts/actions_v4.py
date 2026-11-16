"""Send a task-local capture plan to the isolated v3 browser."""
import json
from pathlib import Path
import sys
import urllib.request

ROOT = Path(__file__).resolve().parents[3]
control = json.loads((ROOT / 'tmp/trailer-v4/control.json').read_text())
plan = Path(sys.argv[1]).read_bytes()
request = urllib.request.Request(
    f"http://127.0.0.1:{control['port']}/actions",
    data=plan, headers={'Content-Type': 'application/json'})
opener = urllib.request.build_opener(urllib.request.ProxyHandler({}))
print('Executing isolated game capture plan:', sys.argv[1], flush=True)
try:
    with opener.open(request, timeout=120) as response:
        result = response.read().decode()
        (ROOT / 'tmp/trailer-v4/last_result.json').write_text(result)
        print(result, flush=True)
except urllib.error.HTTPError as error:
    print(error.read().decode(), flush=True)
    raise
