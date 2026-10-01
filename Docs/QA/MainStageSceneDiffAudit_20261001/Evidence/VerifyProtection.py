"""Read existing protected files and export final receipts; never edit production."""
import analyze as audit
import json,pathlib,datetime,re
out=audit.OUT
baseline=json.loads((out/'protected_hashes_start.json').read_text(encoding='utf-8'))
changed=[]
for rel,expected in baseline.items():
    p=audit.ROOT/rel
    current=audit.sha(p.read_bytes()) if p.exists() else 'MISSING'
    if current!=expected:changed.append({'path':rel,'before':expected,'after':current})
results={}
for name,args in [('status',['status']),('status_short',['status','--short']),('index',['diff','--cached','--name-only']),('branch',['branch','--show-current']),('head',['rev-parse','HEAD']),('origin_main',['rev-parse','origin/main'])]:
    rc,data,err=audit.git(*args);audit.write('git_'+name+'_end.txt',data+err);results[name]=data.decode('utf-8',errors='replace').strip()
source_refs=['MainStageFloorCollisionSetup','MainStageVisuals','MainStageSectionFiveSetup','CraftRailPlatformVisual','ToySpikeVisual']
for name in source_refs:
    p=audit.ROOT/'Assets/Scripts'/f'{name}.cs'
    audit.write('Source_'+name+'.txt','\n'.join(f'{i+1}: {l}' for i,l in enumerate(p.read_text(encoding='utf-8-sig').splitlines()))+'\n')
editor=audit.ROOT/'Assets/Editor/MainStageSceneSync.cs'
audit.write('Source_MainStageSceneSync.txt','\n'.join(f'{i+1}: {l}' for i,l in enumerate(editor.read_text(encoding='utf-8-sig').splitlines()))+'\n')
meta=json.loads((out/'serialized_summary.json').read_text(encoding='utf-8'));scene=audit.sha((audit.ROOT/audit.SCENE).read_bytes())
proof={'utc':datetime.datetime.now(datetime.timezone.utc).isoformat(),'protected_files':len(baseline),'changed_files':changed,'scene_start_sha256':meta['working_scene_sha256'],'scene_end_sha256':scene,'scene_byte_invariant':scene==meta['working_scene_sha256'],'branch':results['branch'],'head':results['head'],'origin_main':results['origin_main'],'index_empty':not results['index'],'new_task_dirs_only':['Docs/QA/MainStageSceneDiffAudit_20261001','.codex_tmp/MainStageSceneDiffAudit_20261001'],'existing_tmp_write_actions':0,'commit_push_performed':False}
proof['pass']=not changed and proof['scene_byte_invariant'] and proof['index_empty'] and results['branch']=='main' and results['head']==results['origin_main']=='4919233cd1d0815afb438bd35a7336492ec09548'
audit.write('protection_result.json',json.dumps(proof,ensure_ascii=False,indent=2))
print(json.dumps(proof,ensure_ascii=False,indent=2))
if not proof['pass']:raise SystemExit('PROTECTION CHECK FAILED')
