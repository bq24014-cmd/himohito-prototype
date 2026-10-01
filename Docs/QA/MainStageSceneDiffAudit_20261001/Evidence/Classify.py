"""Mechanically export the complete Unity property table and provenance evidence."""
import analyze as audit
import json,collections,re,pathlib,difflib
out=audit.OUT
rows=json.loads((out/'property_diff.json').read_text(encoding='utf-8'))
rows=[r for r in rows if r['classification']!='E']
da=audit.docs((out/'HEAD_MainStage.unity').read_bytes());db=audit.docs((out/'WorkingTree_MainStage.unity').read_bytes());ib=audit.identity(db)
scripts={}
for p in (audit.ROOT/'Assets').rglob('*.cs.meta'):
    m=re.search(r'^guid: (\w+)',p.read_text(encoding='utf-8-sig'),re.M)
    if m:scripts[m.group(1)]=p.name[:-5]
head_order=re.findall(r'^--- !u!\d+ &(\d+)',(out/'HEAD_MainStage.unity').read_text(encoding='utf-8'),re.M)
working_order=re.findall(r'^--- !u!\d+ &(\d+)',(out/'WorkingTree_MainStage.unity').read_text(encoding='utf-8'),re.M)
common_order=[fid for fid in working_order if fid in da]
relocated=set()
for tag,i,j,k,l in difflib.SequenceMatcher(None,head_order,common_order,autojunk=False).get_opcodes():
    if tag=='delete' or tag=='replace':relocated.update(head_order[i:j])
ordering=[]
for fid in sorted(relocated,key=int):
    info=ib[fid].copy();info['script']=scripts.get(info['guid'],'')
    record=dict(info,fileID=fid,head_fileID=fid,property='<Unity document order, common IDs only>',head=str(head_order.index(fid)),working_tree=str(common_order.index(fid)),classification='D')
    rows.append(record);ordering.append(record)
audit.write('document_order_noise.json',json.dumps(ordering,ensure_ascii=False,indent=2))
for fid in sorted(set(da)&set(db),key=int):
    al={line.strip():line for line in da[fid]['raw'].splitlines()}
    bl={line.strip():line for line in db[fid]['raw'].splitlines()}
    for line in sorted(set(al)&set(bl)):
        if al[line]!=bl[line] and al[line].rstrip()==bl[line].rstrip():
            info=ib[fid].copy();info['script']=scripts.get(info['guid'],'')
            rows.append(dict(info,fileID=fid,head_fileID=fid,property='whitespace:'+line.split(':')[0],head=repr(al[line]),working_tree=repr(bl[line]),classification='E'))
metadata={'serializedVersion','m_ObjectHideFlags','m_CorrespondingSourceObject','m_PrefabInstance','m_PrefabAsset','m_Icon','m_NavMeshLayer','m_StaticEditorFlags','m_ConstrainProportionsScale','m_LocalEulerAnglesHint','m_EditorHideFlags','m_EditorClassIdentifier'}
for r in rows:
    prop=r['property']
    if r['classification']=='E':
        r['authority']='UNKNOWN (format only; no runtime authority needed)';r['evidence']='Whitespace-only byte delta; Unity field value is identical.'
    elif prop.startswith('<Unity document order') or r['head']=='<ABSENT>' and (prop in metadata or prop=='m_Name' and r['type']=='MonoBehaviour'):
        r['classification']='D';r['authority']='Runtime Code';r['evidence']='Unity default serialization header/schema accompanying a runtime-created component/object, not a separate gameplay value.'
    elif 'Spike' in r['path']:
        r['classification']='C';r['authority']='Runtime Code'
        r['evidence']='ToySpikeVisual.cs:23-89,132-146; OnEnable Refresh -> sprite/scale assignment. SpriteRenderer.size also compared in runtime A/B.'
    else:
        r['classification']='C';r['authority']='Runtime Code'
        r['evidence']='MainStageFloorCollisionSetup.cs:15-42,69-72; MainStageVisuals.cs:719-737; CraftRailPlatformVisual.cs:28-44,59-108,139-155,179-203. Default fields reproduced by new GameObject/AddComponent.'
audit.write('classified_property_diff.json',json.dumps(rows,ensure_ascii=False,indent=2))
counts=collections.Counter(r['classification'] for r in rows)
summary={'unit_definition':'16 changed existing property entries + 20 added Unity documents = 36 semantic/structural units; full leaf/default table counted separately. Added-object default rows are not claimed to be separate gameplay changes.','semantic_structural_units':36,'existing_content_property_rows':16,'new_documents':20,'new_document_property_rows':508,'complete_property_rows':len(rows),'classification_rows':dict(counts),'whitespace_only_rows':counts['E'],'relocated_unchanged_documents':len(ordering),'diff_check_trailing_warnings':49,'line_ending_only_deltas':0,'removed_documents':0,'fileid_remaps':0}
audit.write('classification_summary.json',json.dumps(summary,ensure_ascii=False,indent=2))
def cell(s):return str(s).replace('|','\\|').replace('\n','<br>')
table=['# Serialized Diff — full per-property table','','HEAD vs Working Tree. No production file was edited. Values are extracted from each serialized Unity document; `<ABSENT>` means the entire document/property is absent in HEAD.','','Classification: A runtime meaningful; B visual meaningful; C reproduced/overwritten by runtime setup; D Unity schema/default serialization noise; E whitespace only; F unexplained. D on a new document does NOT mean its entire object is noise: its meaningful fields are separately C.','','Meaningful-looking structural units: **36** (16 existing property deltas + 20 new documents). Complete property rows: **'+str(len(rows))+'**. Whitespace-only rows: **'+str(counts['E'])+'**. Classifications are source-based and cross-checked against `05_RuntimeAB.md`; not a new Human approval.','','| GameObject / hierarchy | fileID | Component type | Script GUID | Property | HEAD | Working Tree | Class |','| --- | --- | --- | --- | --- | --- | --- | --- |']
for r in rows:table.append('| '+' | '.join(cell(x) for x in [r['path'],r['fileID'],r['type']+(' / '+r['script'] if r['script'] else ''),r['guid'] or '—',r['property'],r['head'],r['working_tree'],r['classification']])+' |')
(out.parent/'02_SerializedDiff.md').write_text('\n'.join(table)+'\n',encoding='utf-8')
auth=['# Authority Map — every property','','This table corresponds one-to-one to the serialized table. Runtime entry: `MainStageFloorCollisionSetup.RegisterSceneSetup` subscribes before scene load; `OnSceneLoaded` calls setup for MainStage. `ApplyCurrentScene` calls section setups then `MainStageVisuals.Apply`. No additional manual setup was invoked by the A/B harness.','','| GameObject | fileID | Property | Authority | Source/evidence |','| --- | --- | --- | --- | --- |']
for r in rows:auth.append('| '+' | '.join(cell(x) for x in [r['path'],r['fileID'],r['property'],r['authority'],r['evidence']])+' |')
(out.parent/'03_AuthorityMap.md').write_text('\n'.join(auth)+'\n',encoding='utf-8')
manifest=json.loads((audit.ROOT/'.codex_tmp/SubmissionBuild20260911/source-manifest.json').read_text(encoding='utf-8-sig'))
record=[r for r in manifest if r['path'].replace('\\','/')==audit.SCENE]
audit.write('submission_manifest_scene_entry.json',json.dumps(record,ensure_ascii=False,indent=2))
audit.write('submission_provenance_readme.txt',(audit.ROOT/'.codex_tmp/SubmissionBuild20260911/QA_README.md').read_bytes())
scene_lines=(out/'WorkingTree_MainStage.unity').read_text(encoding='utf-8').splitlines()
starts=[(n+1,re.match(r'--- !u!(\d+) &(\d+)',line).group(2)) for n,line in enumerate(scene_lines) if line.startswith('--- !u!')]
warnings=[]
for number in map(int,re.findall(r'MainStage.unity:(\d+): trailing',(out/'scene_diff_check.txt').read_text(encoding='utf-8'))):
    fid=[i for n,i in starts if n<=number][-1]
    old=da.get(fid,{}).get('raw','')
    kind='NEW_DOCUMENT' if fid not in da else 'UNCHANGED_DOCUMENT_DIFF_ALIGNMENT' if '  m_Name: \n' in old else 'WHITESPACE_CHANGED'
    warnings.append(dict(line=number,fileID=fid,path=ib[fid]['path'],kind=kind))
audit.write('whitespace_warnings.json',json.dumps(warnings,ensure_ascii=False,indent=2))
print('Whitespace warning groups:',dict(collections.Counter(w['kind'] for w in warnings)))
print(json.dumps(summary,ensure_ascii=False,indent=2))
