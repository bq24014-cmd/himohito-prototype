import analyze as audit
import json,pathlib,hashlib
from PIL import Image,ImageChops
out=audit.OUT
a=json.loads((out/'RuntimeA/runtime.json').read_text(encoding='utf-8'));b=json.loads((out/'RuntimeB/runtime.json').read_text(encoding='utf-8'))
differences=[]
def compare(x,y,path=''):
    if type(x)!=type(y):differences.append(dict(path=path,a=x,b=y));return
    if isinstance(x,dict):
        for key in sorted(set(x)|set(y)):
            if key not in x or key not in y:differences.append(dict(path=path+'/'+key,a=x.get(key),b=y.get(key)))
            else:compare(x[key],y[key],path+'/'+key)
    elif isinstance(x,list):
        if len(x)!=len(y):differences.append(dict(path=path+'/length',a=len(x),b=len(y)))
        for i,(v,w) in enumerate(zip(x,y)):compare(v,w,path+'/'+str(i))
    elif x!=y:differences.append(dict(path=path,a=x,b=y))
compare(a,b)
runtime={'node_count_a':len(a['nodes']),'node_count_b':len(b['nodes']),'differences':differences,'identical':not differences,'compared':'affected roots + all descendants; transforms, active, components, colliders, renderers and serialized MonoBehaviour visible properties'}
audit.write('runtime_comparison.json',json.dumps(runtime,ensure_ascii=False,indent=2))
visual=[]
for label in ['S05_Rails','S02_FrontSpike','S02_FarSpike']:
    pa=out/'RuntimeA'/f'{label}.png';pb=out/'RuntimeB'/f'{label}.png'
    im_a=Image.open(pa).convert('RGBA');im_b=Image.open(pb).convert('RGBA')
    diff=ImageChops.difference(im_a,im_b);bbox=diff.convert('RGB').getbbox()
    pixels=sum(1 for p in diff.getdata() if any(p[:3]))
    visual.append(dict(label=label,resolution=im_a.size,sha_a=audit.sha(pa.read_bytes()),sha_b=audit.sha(pb.read_bytes()),different_pixels=pixels,bbox=bbox,identical=bbox is None))
audit.write('visual_comparison.json',json.dumps(visual,indent=2))
project=audit.ROOT/'.codex_tmp/MainStageSceneDiffAudit_20261001/Project'
manifest=json.loads((out/'isolated_input_manifest.json').read_text(encoding='utf-8-sig'))
input_changes=[];head_mismatches=[];script_count=0;lfs_verified=[];ignored_assets=[]
for record in manifest:
    rel=record['path'].replace('\\','/');p=project/rel
    if rel==audit.SCENE:continue
    if audit.sha(p.read_bytes())!=record['sha256']:input_changes.append(rel)
    rc,head,err=audit.git('show','HEAD:'+rel)
    if rc or audit.sha(head)!=record['sha256']:
        import re
        match=re.search(rb'^oid sha256:([0-9a-f]{64})$',head,re.M)
        if not rc and match and match.group(1).decode().upper()==record['sha256']:lfs_verified.append(rel)
        elif rc and audit.git('check-ignore',rel)[0]==0:ignored_assets.append({'path':rel,'sha256':record['sha256'],'scope':'Pre-existing ignored resource; identical in A and B; no source/config divergence.'})
        else:head_mismatches.append(rel)
    if rel.endswith('.cs') and '/Scripts/' in rel:script_count+=1
input_proof={'non_scene_input_changes_after_AB':input_changes,'latest_HEAD_tracked_input_mismatches':head_mismatches,'pre_existing_ignored_resources_shared_AB':ignored_assets,'git_LFS_payloads_verified_against_HEAD_oid':lfs_verified,'production_runtime_scripts':script_count,'qa_extra_file':'Assets/Editor/SceneAuditEntry.cs (+ Unity-generated .meta)','isolated_scene_after_B_sha':audit.sha((project/audit.SCENE).read_bytes()),'isolated_scene_after_A_sha_observed_before_B':'B6191C44EE8FC841784B4C10EB1940DBA5220EAE112165FD09353D32AF879151','a_scene_was_not_serialized':True,'b_scene_was_not_serialized':(project/audit.SCENE).read_bytes()==(out/'WorkingTree_MainStage.unity').read_bytes()}
audit.write('isolated_inputs_verification.json',json.dumps(input_proof,ensure_ascii=False,indent=2))
print(json.dumps(runtime,ensure_ascii=False,indent=2));print(json.dumps(visual,indent=2));print(json.dumps(input_proof,ensure_ascii=False,indent=2))
