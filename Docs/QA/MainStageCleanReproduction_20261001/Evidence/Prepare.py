"""Archive first, then export committed inputs. Never edits production scene/source."""
import pathlib,subprocess,hashlib,json,zipfile,io,datetime,re
ROOT=pathlib.Path(__file__).resolve().parents[4]
EV=pathlib.Path(__file__).resolve().parent
ARCH=ROOT/'Docs/Archive/SubmissionScene_20260911'
TASK=ROOT/'.codex_tmp/MainStageCleanReproduction_20261001'
EXPECTED='4919233cd1d0815afb438bd35a7336492ec09548'
SCENE='Assets/Scenes/MainStage.unity'
SCENE_SHA='57D2464751B5B9C2A78E6C56F34197E9BC2629983FADAA1DFF6E0BA0B4BC2284'
def sha(b):return hashlib.sha256(b).hexdigest().upper()
def git(*args):
    p=subprocess.run(['git','-c','safe.directory='+ROOT.as_posix(),*args],cwd=ROOT,capture_output=True)
    if p.returncode:raise RuntimeError(str(args)+': '+p.stderr.decode(errors='replace'))
    return p.stdout
def evidence(name,value): (EV/name).write_text(json.dumps(value,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
if git('rev-parse','HEAD').decode().strip()!=EXPECTED or git('rev-parse','origin/main').decode().strip()!=EXPECTED or git('branch','--show-current').decode().strip()!='main':raise SystemExit('STOP Git baseline mismatch')
if git('diff','--cached','--name-only').strip():raise SystemExit('STOP unexpected staged files')
if TASK.exists() or (ARCH/'MainStage_submission_20260911.zip').exists():raise SystemExit('STOP fresh destination required')
current=(ROOT/SCENE).read_bytes()
submission=(ROOT/'.codex_tmp/SubmissionBuild20260911'/SCENE).read_bytes()
if sha(current)!=SCENE_SHA or current!=submission:raise SystemExit('STOP submission byte mismatch')
protected={}
for folder in ['Assets','Packages','ProjectSettings','Docs','output','Builds']:
    for p in (ROOT/folder).rglob('*'):
        if p.is_file() and 'MainStageCleanReproduction_20261001' not in p.parts and 'SubmissionScene_20260911' not in p.parts:
            protected[p.relative_to(ROOT).as_posix()]=sha(p.read_bytes())
for filename in ['README.md','AGENTS.md']:
    p=ROOT/filename
    if p.exists():protected[filename]=sha(p.read_bytes())
evidence('protected_start.json',protected)
evidence('git_start.json',{'utc':datetime.datetime.now(datetime.timezone.utc).isoformat(),'head':EXPECTED,'origin_main':EXPECTED,'branch':'main','status':git('status','--short').decode('utf-8'),'scene_sha':sha(current),'production_scene_restore_not_yet_performed':True})
zp=ARCH/'MainStage_submission_20260911.zip'
with zipfile.ZipFile(zp,'w',zipfile.ZIP_DEFLATED,compresslevel=9) as z:
    info=zipfile.ZipInfo('MainStage.unity',date_time=(2026,9,11,0,0,0));info.compress_type=zipfile.ZIP_DEFLATED
    z.writestr(info,current)
with zipfile.ZipFile(zp) as z:
    if z.namelist()!=['MainStage.unity']:raise SystemExit('STOP incorrect ZIP inventory')
    expanded=EV/'ArchiveExpansion';expanded.mkdir()
    z.extract('MainStage.unity',expanded)
expanded_sha=sha((expanded/'MainStage.unity').read_bytes())
patch=git('diff','--',SCENE)
(ARCH/'MainStage_submission_vs_HEAD.patch').write_bytes(patch)
(ARCH/'SHA256.txt').write_text(SCENE_SHA+'  MainStage.unity (ZIP member and extracted file)\n'+sha(zp.read_bytes())+'  MainStage_submission_20260911.zip\n'+sha(patch)+'  MainStage_submission_vs_HEAD.patch\n',encoding='utf-8')
qa=ROOT/'Docs/QA/MainStageSceneDiffAudit_20261001'
files=[p for p in ARCH.rglob('*') if p.is_file()]+[p for p in qa.rglob('*') if p.is_file()]
sizes=[dict(path=p.relative_to(ROOT).as_posix(),bytes=p.stat().st_size) for p in files]
verification={'scene_sha':SCENE_SHA,'expanded_sha':expanded_sha,'exact_zip_inventory':['MainStage.unity'],'patch_exists':bool(patch),'readme_exists':(ARCH/'README.md').exists(),'qa_evidence_exists':(qa/'Evidence/runtime_comparison.json').exists(),'recheck_exists':(qa/'Evidence/Recheck_20261001.json').exists(),'total_saved_bytes':sum(p['bytes'] for p in sizes),'maximum_file_bytes':max(p['bytes'] for p in sizes),'github_file_limit_bytes':100*1024*1024,'files':sizes}
verification['pass']=expanded_sha==SCENE_SHA and all(verification[k] for k in ['patch_exists','readme_exists','qa_evidence_exists','recheck_exists']) and verification['maximum_file_bytes']<100*1024*1024
evidence('archive_verification.json',verification)
if not verification['pass']:raise SystemExit('STOP Archive verification failed')
print('ARCHIVE_VERIFICATION_PASS ZIP bytes='+str(zp.stat().st_size),flush=True)
TASK.mkdir(parents=True)
archive=git('archive','--format=zip','HEAD','Assets','Packages','ProjectSettings')
committed={};lfs=[]
for project_name in ['BuildProject','QAProject']:
    project=TASK/project_name;project.mkdir()
    with zipfile.ZipFile(io.BytesIO(archive)) as z:z.extractall(project)
    for p in project.rglob('*'):
        if not p.is_file():continue
        rel=p.relative_to(project).as_posix();data=p.read_bytes()
        if data.startswith(b'version https://git-lfs.github.com/spec/v1'):
            oid=re.search(rb'oid sha256:([0-9a-f]{64})',data).group(1).decode().upper()
            payload=(ROOT/rel).read_bytes()
            if sha(payload)!=oid:raise SystemExit('STOP LFS payload mismatch '+rel)
            p.write_bytes(payload);data=payload
            if project_name=='BuildProject':lfs.append(dict(path=rel,sha256=oid))
        if project_name=='BuildProject':committed[rel]=sha(data)
        elif sha(data)!=committed[rel]:raise SystemExit('STOP unequal clean project inputs')
evidence('clean_input_manifest.json',committed)
evidence('clean_input_provenance.json',{'head':EXPECTED,'export':'git archive HEAD Assets Packages ProjectSettings','resolved_LFS':lfs,'main_scene_head_sha':sha((TASK/'BuildProject'/SCENE).read_bytes()),'input_files':len(committed),'runtime_scripts':sum(1 for n in committed if n.startswith('Assets/Scripts/') and n.endswith('.cs')),'uncommitted_inputs_used':False,'ignored_bgm_included':False,'old_build_used':False,'build_project':str(TASK/'BuildProject'),'qa_project':str(TASK/'QAProject')})
print('CLEAN_HEAD_PROJECTS_READY files='+str(len(committed)),flush=True)
