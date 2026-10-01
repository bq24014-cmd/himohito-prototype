import hashlib,json,pathlib,zipfile
from PIL import Image, ImageChops
ROOT=pathlib.Path.cwd();EV=ROOT/'Docs/QA/MainStageCleanReproduction_20261001/Evidence/DeterministicSceneSync'
paths=json.loads((EV/'paths.json').read_text())
def load(p):
    if p.exists():return json.loads(p.read_text(encoding='utf-8-sig'))
    with zipfile.ZipFile(EV/'SceneSnapshots.zip') as z:return json.loads(z.read(p.relative_to(EV).as_posix()))
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest().upper()
def differences(a,b,path=''):
    out=[]
    if type(a)!=type(b):return [{'path':path,'a':a,'b':b}]
    if isinstance(a,dict):
        for k in sorted(set(a)|set(b)):
            if k not in a or k not in b:out.append({'path':path+'/'+k,'a':a.get(k),'b':b.get(k)})
            else:out+=differences(a[k],b[k],path+'/'+k)
    elif isinstance(a,list):
        if len(a)!=len(b):out.append({'path':path+'/length','a':len(a),'b':len(b)})
        for i,(x,y) in enumerate(zip(a,b)):out+=differences(x,y,path+'/'+str(i))
    elif a!=b:out.append({'path':path,'a':a,'b':b})
    return out
def normalized(packet):
    packet=json.loads(json.dumps(packet))
    excluded=[]
    for node in packet['nodes']:
        for part in node['components']:
            ref_paths=[f['property'] for f in part['fields'] if f['type']=='ObjectReference']
            kept=[]
            for f in part['fields']:
                # SerializedObject's reference-child raw IDs are process-specific.
                # Parent references already retain normalized hierarchy/type or GUID/localID.
                if any(f['property'].startswith(r+'.') for r in ref_paths): excluded.append({'node':node['path'],'component':part['type'],'field':f})
                else:kept.append(f)
            part['fields']=kept
    return packet,excluded
result=load(EV/'build_comparison.json')
wholeA=load(EV/'SnapshotA/all_scene_serialized.json');wholeB=load(EV/'SnapshotB/all_scene_serialized.json')
normalizedA,idsA=normalized(wholeA);normalizedB,idsB=normalized(wholeB)
result['all_scene_objects_A_B']=[wholeA['objects'],wholeB['objects']]
result['all_scene_components_A_B']=[wholeA['components'],wholeB['components']]
result['all_snapshot_raw_differences']=differences(wholeA,wholeB)
result['all_snapshot_semantic_differences']=differences(normalizedA,normalizedB)
result['normalized_reference_child_counts']=[len(idsA),len(idsB)]
if not (EV/'SceneSnapshots.zip').exists():
    for label,n in [('A',normalizedA),('B',normalizedB)]: (EV/f'Snapshot{label}/semantic_all_scene.json').write_text(json.dumps(n,ensure_ascii=False,indent=2),encoding='utf-8')
result['serialized_file_structure_equal']=load(EV/'serialized_structure.json')['A']==load(EV/'serialized_structure.json')['B']
a=load(EV/'Runtime14A/runtime.json');b=load(EV/'Runtime14B/runtime.json')
result['runtime_14_node_counts']=[len(a['nodes']),len(b['nodes'])]
result['runtime_14_differences']=differences(a,b)
prior=ROOT/'Docs/QA/MainStageSceneDiffAudit_20261001/Evidence/RuntimeA/runtime.json'
result['runtime_14_vs_previous_CASE_S4_HEAD']=differences(load(prior),a)
visual=[]
for label in ['S05_Rails','S02_FrontSpike','S02_FarSpike']:
    pa=EV/f'Runtime14A/{label}.ppm';pb=EV/f'Runtime14B/{label}.ppm'
    if not pa.exists():pa=pa.with_suffix('.png')
    if not pb.exists():pb=pb.with_suffix('.png')
    with Image.open(pa) as ia,Image.open(pb) as ib:
        da=ia.convert('RGB');db=ib.convert('RGB');diff=ImageChops.difference(da,db)
        pixels=sum(any(pixel) for pixel in diff.getdata())
        for p,im in [(pa,da),(pb,db)]: im.save(p.with_suffix('.png'))
        visual.append({'capture':label,'different_pixels':pixels,'resolution':da.size,'ppm_sha_A':sha(pa),'ppm_sha_B':sha(pb)})
result['visual']=visual
stack=(EV/'SnapshotCause/scene_saving_stack.txt').read_text()
result['cause_stack_has_sync_save']=all(s in stack for s in ['MainStageSceneSync.Sync','MainStageSceneSync.OnSceneOpened','EditorSceneManager.SaveScene'])
result['cause_scene_sha256']=sha(pathlib.Path(paths['cause_project'])/'Assets/Scenes/MainStage.unity')
result['cause_scene_matches_AB']=result['cause_scene_sha256']==result['A']['after_sha']==result['B']['after_sha']
(EV/'comparison.json').write_text(json.dumps(result,ensure_ascii=False,indent=2),encoding='utf-8')
small={k:v for k,v in result.items() if 'differences' not in k and k!='runtime_14_vs_previous_CASE_S4_HEAD'}
for k in ['all_snapshot_raw_differences','all_snapshot_semantic_differences','runtime_14_differences','runtime_14_vs_previous_CASE_S4_HEAD']: small[k+'_count']=len(result[k])
print(json.dumps(small,ensure_ascii=False,indent=2))
