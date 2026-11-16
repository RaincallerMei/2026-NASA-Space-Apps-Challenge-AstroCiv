import json,subprocess,sys
from pathlib import Path
root=Path(__file__).resolve().parents[3]
planet=sys.argv[1];seed=int(sys.argv[2])
request=dict(requests=[dict(planet=planet,seed=seed,careful=True)])
(root/'tmp/trailer-v4/plan-request.json').write_text(json.dumps(request))
r=subprocess.run(['unity','command','script','run','--file','../tmp/trailer-v4/TrailerPlans.cs','--entry','TrailerPlans.Main','--result-only','--json'],cwd=root/'26NASAChallenge',capture_output=True,text=True)
(root/'tmp/trailer-v4/planner-result.txt').write_text(r.stdout+'\n'+r.stderr,encoding='utf-8')
if r.returncode:print(r.stdout,r.stderr);raise SystemExit(r.returncode)
p=json.loads((root/'tmp/trailer-v4/plans.json').read_text())['plans'][0]
(root/'output/trailer/v4'/f'plan-{planet.lower()}-{seed}.json').write_text(json.dumps(p,indent=2))
print(json.dumps(p))
