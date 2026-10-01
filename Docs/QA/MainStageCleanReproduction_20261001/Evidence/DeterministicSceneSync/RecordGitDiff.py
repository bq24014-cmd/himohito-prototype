import pathlib,subprocess,json,hashlib
EV=pathlib.Path.cwd()/'Docs/QA/MainStageCleanReproduction_20261001/Evidence/DeterministicSceneSync'
normalized={}
for label in ['A','B']:
    before=f'Copy{label}_before.unity.txt';after=f'Copy{label}_after.unity.txt'
    call=subprocess.run(['git','diff','--no-index','--',before,after],cwd=EV,capture_output=True)
    if call.returncode!=1:raise SystemExit('Unexpected diff return code')
    (EV/f'Copy{label}_git_diff_raw.patch').write_bytes(call.stdout)
    text=call.stdout.decode('utf-8').replace(before,'HEAD_MainStage.unity').replace(after,'SYNC_MainStage.unity')
    normalized[label]=text.encode('utf-8')
    (EV/f'Copy{label}_git_diff_normalized.patch').write_bytes(normalized[label])
result={'equal':normalized['A']==normalized['B'],'normalization':'Only A/B filenames in diff headers replaced. No hunk/body/value normalization. Raw git diffs retained.','sha256_A':hashlib.sha256(normalized['A']).hexdigest().upper(),'sha256_B':hashlib.sha256(normalized['B']).hexdigest().upper()}
(EV/'git_diff_comparison.json').write_text(json.dumps(result,indent=2),encoding='utf-8')
print(json.dumps(result,indent=2))
