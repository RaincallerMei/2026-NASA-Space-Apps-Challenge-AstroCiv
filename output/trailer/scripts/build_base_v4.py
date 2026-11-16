from pathlib import Path
import json,sys,runpy
root=Path(__file__).resolve().parents[3]
planet=sys.argv[1];seed=int(sys.argv[2]);name=sys.argv[3]
plan=json.loads((root/'output/trailer/v4'/f'plan-{planet.lower()}-{seed}.json').read_text())
spots=plan['spots'];cardx={'Habitat':.029,'SolarArray':.11,'Battery':.175,'LifeSupport':.245,'Communications':.33,'Greenhouse':.41,'RoverGarage':.49,'Shelter':.55}
def click(x,y,after=.23):return dict(action='click',x=x,y=y,after=after)
def build(seq):
 actions=[];last=None
 for b in seq:
  if last!=b['type']:actions.append(click(cardx[b['type']],.935,.23))
  actions.append(click((460+(b['x']+.5)*50)/1920,(895-(b['y']+.5)*50)/1080,.38))
  last=b['type'] if b['type']=='SolarArray' else None
 return actions
p=[click(.45 if planet=='Moon' else .56,.367),click(.45,.45),click(.5,.67,.5),click(.4,.712,.4),dict(action='record-start'),dict(action='wait',seconds=.5)]
initial=spots[:9]+spots[11:13] if planet=='Mars' else spots[:6]
p+=build(initial)+[dict(action='wait',seconds=.8),dict(action='record-stop',name=name+'_build'),dict(action='screenshot',name=name+'-wave1')]
first=next(e['hour'] for e in plan['events'] if e['kind']=='SupplyDrop')
p += [click(.773,.043,.12),click(.773,.043,.12),click(.727,.043,.15),dict(action='wait',seconds=(first+.4)/2),click(.727,.043,.2),dict(action='screenshot',name=name+'-funds'),dict(action='record-start'),dict(action='wait',seconds=.4)]
later=spots[9:11] if planet=='Mars' else spots[6:9]
p+=build(later)+[dict(action='wait',seconds=1),dict(action='record-stop',name=name+'_grow'),dict(action='screenshot',name=name+'-final')]
p +=[dict(action='reload'),dict(action='wait',seconds=2),dict(action='js',code="document.querySelector('#unity-container').style.cssText='position:fixed;left:0;top:0;transform:none;width:1280px;height:720px';document.querySelector('#unity-fullscreen-container').style.cssText='width:1280px;height:720px';document.querySelector('#unity-canvas').style.cssText='width:1280px;height:720px;display:block';document.querySelector('#unity-footer').style.display='none';true"),click(.5,.528,.4),dict(action='screenshot',name=name+'-next-seed')]
f=root/'output/trailer/scripts'/('v4_'+name+'.json');f.write_text(json.dumps(p,indent=2))
sys.argv=['actions_v4.py',str(f)];runpy.run_path(str(root/'output/trailer/scripts/actions_v4.py'),run_name='__main__')
