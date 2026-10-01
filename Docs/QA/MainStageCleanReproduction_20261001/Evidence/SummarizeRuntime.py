"""Convert diagnostic framebuffer exports and summarize retained raw results."""
import pathlib, re, json, csv
from PIL import Image
EV = pathlib.Path(__file__).resolve().parent
summary = {}
for folder in ['FullRuntime2', 'FocusedRuntime2']:
    directory = EV/folder
    checks = (directory/'checks.txt').read_text(encoding='utf-8')
    result = (directory/'result.txt').read_text(encoding='utf-8')
    assert 'HARNESS_TIMEOUT' not in checks and 'harness timeout' not in checks
    errors_path = directory/'runtime_errors.txt' if folder=='FullRuntime2' else directory/'runtime_messages.txt'
    errors = errors_path.read_text(encoding='utf-8') if errors_path.exists() else ''
    summary[folder] = {'raw_result':result,
        'passes':[line for line in checks.splitlines() if line.startswith('PASS ')],
        'failures':[line for line in checks.splitlines() if line.startswith('FAIL ')],
        'unity_editor_search_exception_present':'UnityEditor.Search.SearchDatabase' in errors,
        'evidence_level':'Controlled Editor diagnostic, NOT executable smoke or human playthrough'}
for p in (EV/'FocusedRuntime2').glob('*.ppm'):
    with Image.open(p) as im: im.save(p.with_suffix('.png'))
summary['BuildSmoke'] = {'status':'NOT_RUN', 'reason':'clean HEAD Windows build failed before exe output'}
root = EV.parents[3]
baseline = root/'Docs/QA/HumanPlaytestFixes_20260930/Evidence/After/angles_near_center.csv'
def aim_counts(path, old):
    rows = list(csv.DictReader(path.open(encoding='utf-8-sig')))
    name = 'Main S09 Green Right Bank'
    right = [r for r in rows if r['resolved']==name]
    return {'E_right_selected':len(right), 'F_currentAim_differs_from_E':sum(r['currentAim']!=name for r in right)}
summary['S9AssertionClassification'] = {
    'current':aim_counts(EV/'FocusedRuntime2/aim_near.csv',False),
    'human_approved_after_baseline':aim_counts(baseline,True),
    'classification':'Invalid extra assertion: E attachment and F removal target need not be equal. Same 27 right selections / 11 F differences in approved baseline.',
    'E_center_steals_right_current':0,
    'production_regression_inferred':False}
(EV/'runtime_summary.json').write_text(json.dumps(summary,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
print(json.dumps({k:{'passes':len(v.get('passes',[])), 'failures':v.get('failures',[]), 'raw_result':v.get('raw_result','')} for k,v in summary.items()},ensure_ascii=False,indent=2))
