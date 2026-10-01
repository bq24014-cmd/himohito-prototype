"""Read-only revalidation of an identical repeated audit request.
Append a new receipt only; never rerun writers that replace prior QA evidence.
"""
import pathlib,json,hashlib,subprocess,datetime
ROOT=pathlib.Path(__file__).resolve().parents[4]
EV=pathlib.Path(__file__).resolve().parent
RECEIPT=EV/'Recheck_20261001.json'
EXPECTED='4919233cd1d0815afb438bd35a7336492ec09548'
def sha(data):return hashlib.sha256(data).hexdigest().upper()
def read(name):return json.loads((EV/name).read_text(encoding='utf-8-sig'))
def git(*args):
    p=subprocess.run(['git','-c','safe.directory='+ROOT.as_posix(),*args],cwd=ROOT,capture_output=True)
    return p.returncode,p.stdout,p.stderr
if RECEIPT.exists():raise SystemExit('STOP: receipt already exists; preserve it.')
start=datetime.datetime.now(datetime.timezone.utc).isoformat()
scene=ROOT/'Assets/Scenes/MainStage.unity'
scene_start=sha(scene.read_bytes())
existing_qa={str(p.relative_to(EV.parent)):sha(p.read_bytes()) for p in EV.parent.rglob('*') if p.is_file()}
head=git('rev-parse','HEAD')[1].decode().strip()
remote=git('rev-parse','origin/main')[1].decode().strip()
branch=git('branch','--show-current')[1].decode().strip()
if head!=EXPECTED or remote!=EXPECTED or branch!='main':raise SystemExit('STOP: Git baseline changed.')
diff_rc,diff_bytes,diff_err=git('diff','--','Assets/Scenes/MainStage.unity')
check_rc,check_bytes,check_err=git('diff','--check','--','Assets/Scenes/MainStage.unity')
baseline=read('protected_hashes_start.json')
changed=[]
for rel,expected in baseline.items():
    p=ROOT/rel
    current=sha(p.read_bytes()) if p.exists() else 'MISSING'
    if current!=expected:changed.append(dict(path=rel,before=expected,after=current))
prior_inputs=read('isolated_input_manifest.json')
input_changes=[]
for record in prior_inputs:
    rel=record['path'].replace('\\','/')
    if rel=='Assets/Scenes/MainStage.unity':continue
    p=ROOT/rel
    current=sha(p.read_bytes()) if p.exists() else 'MISSING'
    if current!=record['sha256']:input_changes.append(rel)
submission=ROOT/'.codex_tmp/SubmissionBuild20260911/Assets/Scenes/MainStage.unity'
current_equals_submission=scene.read_bytes()==submission.read_bytes()
runtime_a=read('RuntimeA/runtime.json');runtime_b=read('RuntimeB/runtime.json')
pngs=[]
for label in ['S05_Rails','S02_FrontSpike','S02_FarSpike']:
    pa=EV/'RuntimeA'/f'{label}.png';pb=EV/'RuntimeB'/f'{label}.png'
    pngs.append(dict(label=label,sha_a=sha(pa.read_bytes()),sha_b=sha(pb.read_bytes()),identical=pa.read_bytes()==pb.read_bytes()))
prior_pngs=read('visual_comparison.json')
pngs_intact=all(p['sha_a']==old['sha_a'] and p['sha_b']==old['sha_b'] for p,old in zip(pngs,prior_pngs))
qa_changes=[rel for rel,expected in existing_qa.items() if not (EV.parent/rel).exists() or sha((EV.parent/rel).read_bytes())!=expected]
status=git('status','--short')[1].decode('utf-8',errors='replace')
index=git('diff','--cached','--name-only')[1].decode().strip()
scene_end=sha(scene.read_bytes())
result=dict(task='MainStage Scene Diff Audit — repeated request revalidation',start_utc=start,end_utc=datetime.datetime.now(datetime.timezone.utc).isoformat(),head=head,origin_main=remote,branch=branch,scene_start_sha256=scene_start,scene_end_sha256=scene_end,scene_byte_invariant=scene_start==scene_end,scene_matches_prior_evidence=scene.read_bytes()==(EV/'WorkingTree_MainStage.unity').read_bytes(),head_scene_matches_prior_evidence=git('show','HEAD:Assets/Scenes/MainStage.unity')[1]==(EV/'HEAD_MainStage.unity').read_bytes(),full_diff_matches_prior_evidence=diff_bytes==(EV/'scene.diff').read_bytes(),diff_check_exit=check_rc,diff_check_matches_prior_evidence=check_bytes+check_err==(EV/'scene_diff_check.txt').read_bytes(),protected_files_checked=len(baseline),protected_file_changes=changed,production_inputs_match_prior_AB=not input_changes,production_input_changes=input_changes,existing_qa_files_checked=len(existing_qa),existing_qa_file_changes=qa_changes,working_equals_submission_scene=current_equals_submission,submission_sha256=sha(submission.read_bytes()),previous_runtime_evidence_equal=runtime_a==runtime_b,previous_runtime_node_count=len(runtime_a['nodes']),previous_visual_evidence=pngs,previous_visual_evidence_intact=pngs_intact,unity_rerun=False,unity_rerun_reason='Identical HEAD, scene bytes, and production inputs; intact previous scoped A/B evidence is reused, not presented as a new runtime test or Human PASS.',classification=read('classification_summary.json'),decision='CASE S4',index_empty=not index,git_status_short=status,scene_source_changes=False,stage_commit_push_performed=False,desert_loop_access=False)
result['pass']=not changed and not qa_changes and not input_changes and not index and scene_start==scene_end and result['scene_matches_prior_evidence'] and result['head_scene_matches_prior_evidence'] and result['full_diff_matches_prior_evidence'] and result['diff_check_matches_prior_evidence'] and current_equals_submission and runtime_a==runtime_b and pngs_intact and all(p['identical'] for p in pngs)
RECEIPT.write_text(json.dumps(result,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
print(json.dumps({k:result[k] for k in ['pass','scene_start_sha256','scene_end_sha256','protected_files_checked','protected_file_changes','production_input_changes','existing_qa_files_checked','existing_qa_file_changes','full_diff_matches_prior_evidence','working_equals_submission_scene','previous_runtime_node_count','previous_visual_evidence_intact','unity_rerun','decision','index_empty']},ensure_ascii=False,indent=2))
if not result['pass']:raise SystemExit('STOP: revalidation did not pass.')
