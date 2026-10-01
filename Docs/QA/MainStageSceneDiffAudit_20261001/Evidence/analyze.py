"""Read-only production audit; generated evidence is confined to this QA folder."""
import subprocess, pathlib, hashlib, json, re, collections
ROOT = pathlib.Path(__file__).resolve().parents[4]
OUT = pathlib.Path(__file__).resolve().parent
SCENE = 'Assets/Scenes/MainStage.unity'
def git(*args):
    p = subprocess.run(['git','-c','safe.directory='+ROOT.as_posix(),*args],cwd=ROOT,capture_output=True)
    return p.returncode,p.stdout,p.stderr
def write(name, data):
    (OUT/name).write_bytes(data if isinstance(data,bytes) else data.encode('utf-8'))
def sha(b): return hashlib.sha256(b).hexdigest().upper()
def docs(b):
    result={}
    for m in re.finditer(r'^--- !u!(\d+) &(\d+)[^\n]*\n(.*?)(?=^--- !u!|\Z)',b.decode('utf-8-sig'),re.M|re.S):
        cid,fid,body=m.groups(); props={}; stack=[]; lines=body.splitlines()
        for i,line in enumerate(lines[1:]):
            match=re.match(r'^( *)([A-Za-z_][\w]*):(?: (.*))?$',line)
            if not match: continue
            spaces,key,val=match.groups(); indent=len(spaces)
            while stack and stack[-1][0]>=indent: stack.pop()
            path='.'.join([x[1] for x in stack]+[key]); val=(val or '').strip()
            following=lines[i+2:]
            if not val and following and re.match(r'^ *-',following[0]):
                block=[]
                for nxt in following:
                    pm=re.match(r'^( *)([A-Za-z_][\w]*):',nxt)
                    if pm and len(pm.group(1))<=indent: break
                    if nxt.strip(): block.append(nxt.strip())
                props[path]='\n'.join(block)
            elif val or not following or len(following[0])-len(following[0].lstrip())<=indent:
                props[path]=val
            else: stack.append((indent,key))
        result[fid]={'class':int(cid),'type':body.split(':',1)[0],'props':props,'raw':body}
    return result
def identity(ds):
    names={fid:d['props'].get('m_Name','') for fid,d in ds.items() if d['class']==1}
    tr={}; parents={}
    for fid,d in ds.items():
        if d['class']==4:
            go=re.search(r'fileID: (\d+)',d['props'].get('m_GameObject',''))
            if go: tr[go.group(1)]=fid
            parent=re.search(r'fileID: (\d+)',d['props'].get('m_Father',''))
            if parent: parents[fid]=parent.group(1)
    inv={v:k for k,v in tr.items()}
    def path(go,seen=()):
        if go in seen:return names.get(go,'?')
        parent=inv.get(parents.get(tr.get(go,''),''))
        return (path(parent,seen+(go,))+'/' if parent else '')+names.get(go,'?')
    info={}
    for fid,d in ds.items():
        go=fid if d['class']==1 else (re.search(r'fileID: (\d+)',d['props'].get('m_GameObject','')) or [None,''])[1]
        guid=(re.search(r'guid: ([0-9a-f]+)',d['props'].get('m_Script','')) or [None,''])[1]
        info[fid]={'go':go,'name':names.get(go,''),'path':path(go) if go else '', 'guid':guid,'type':d['type']}
    return info
if __name__=='__main__':
    a=git('show','HEAD:'+SCENE)[1]; b=(ROOT/SCENE).read_bytes()
    write('HEAD_MainStage.unity',a);write('WorkingTree_MainStage.unity',b)
    states={cmd:' '.join(args) for cmd,args in []}
    for name,args in [('git_status_start.txt',['status']),('git_status_short_start.txt',['status','--short']),('git_index_start.txt',['diff','--cached','--name-only']),('scene.diff',['diff','--',SCENE]),('scene_diff_check.txt',['diff','--check','--',SCENE]),('git_diff_all_start.txt',['diff','--stat'])]:
        rc,o,e=git(*args);write(name,o+e)
    da,db=docs(a),docs(b); ia,ib=identity(da),identity(db)
    scripts={}
    for p in (ROOT/'Assets').rglob('*.cs.meta'):
        m=re.search(r'^guid: (\w+)',p.read_text(encoding='utf-8-sig'),re.M)
        if m:scripts[m.group(1)]=p.name[:-5]
    removed=set(da)-set(db);added=set(db)-set(da); remap={}
    for old in removed:
        candidates=[new for new in added if ia[old]['path']==ib[new]['path'] and da[old]['type']==db[new]['type'] and ia[old]['guid']==ib[new]['guid']]
        if len(candidates)==1:remap[old]=candidates[0]
    rows=[]
    def compare(old,new):
        x=da.get(old,{}).get('props',{});y=db.get(new,{}).get('props',{}); info=(ib.get(new) or ia[old]).copy()
        info['script']=scripts.get(info['guid'],'')
        for key in sorted(set(x)|set(y)):
            av=x.get(key,'<ABSENT>');bv=y.get(key,'<ABSENT>')
            if av!=bv or old!=new:
                mapped=re.sub(r'fileID: (\d+)',lambda m:'fileID: '+remap.get(m.group(1),m.group(1)),av)
                classification='D' if mapped==bv and old!=new or mapped==bv and av!=bv else 'F'
                rows.append(dict(info,fileID=new or old,head_fileID=old,property=key,head=av,working_tree=bv,classification=classification))
    for fid in sorted(set(da)&set(db),key=int):compare(fid,fid)
    for old,new in remap.items():compare(old,new)
    for old in removed-set(remap):compare(old,None)
    for new in added-set(remap.values()):compare(None,new)
    whitespace=[]
    for fid in sorted(set(da)&set(db),key=int):
        if da[fid]['raw']!=db[fid]['raw']:
            al=da[fid]['raw'].splitlines();bl=db[fid]['raw'].splitlines()
            for n,(x,y) in enumerate(zip(al,bl)):
                if x!=y and x.rstrip()==y.rstrip():
                    info=ib[fid].copy();info['script']=scripts.get(info['guid'],'')
                    rows.append(dict(info,fileID=fid,head_fileID=fid,property='whitespace:'+x.strip().split(':')[0],head=repr(x),working_tree=repr(y),classification='E'))
                    whitespace.append({'fileID':fid,'head':x,'working_tree':y})
    write('property_diff.json',json.dumps(rows,ensure_ascii=False,indent=2))
    write('fileid_remap.json',json.dumps(remap,indent=2))
    meta={'branch':git('branch','--show-current')[1].decode().strip(),'head':git('rev-parse','HEAD')[1].decode().strip(),'origin_main':git('rev-parse','origin/main')[1].decode().strip(),'head_scene_sha256':sha(a),'working_scene_sha256':sha(b),'head_CRLF':a.count(b'\r\n'),'working_CRLF':b.count(b'\r\n'),'head_LF':a.count(b'\n'),'working_LF':b.count(b'\n'),'added_documents':len(added),'removed_documents':len(removed),'fileid_remap':remap,'property_rows':len(rows),'whitespace_only_property_rows':len(whitespace),'diff_check_trailing_warnings':git('diff','--check','--',SCENE)[1].count(b'trailing whitespace.')}
    submission=ROOT/'.codex_tmp/SubmissionBuild20260911/Assets/Scenes/MainStage.unity'
    if submission.exists():
        s=submission.read_bytes();meta['submission_scene_path']=str(submission);meta['submission_scene_sha256']=sha(s);meta['working_equals_submission']=b==s
        write('Submission_MainStage.unity',s)
    write('serialized_summary.json',json.dumps(meta,ensure_ascii=False,indent=2))
    # Protect current production files, existing docs and builds; new QA evidence is excluded.
    protected={}
    for folder in ['Assets','Packages','ProjectSettings','Docs','output','Builds']:
        for p in (ROOT/folder).rglob('*'):
            if p.is_file() and 'MainStageSceneDiffAudit_20261001' not in p.parts:
                protected[p.relative_to(ROOT).as_posix()]=sha(p.read_bytes())
    for name in ['README.md','AGENTS.md']:
        p=ROOT/name
        if p.exists():protected[name]=sha(p.read_bytes())
    write('protected_hashes_start.json',json.dumps(protected,ensure_ascii=False,indent=2))
    print(json.dumps(meta,ensure_ascii=False,indent=2))
    groups=collections.defaultdict(list)
    for r in rows:
        if r['classification']!='E':groups[(r['path'],r['type'],r['script'])].append(r)
    for k,rs in groups.items():
        print(k,'properties=',len(rs),'ids=',set(r['fileID'] for r in rs))
        if len(rs)<20:
            for r in rs:print(' ',r['property'],r['head'],'=>',r['working_tree'],r['classification'])
