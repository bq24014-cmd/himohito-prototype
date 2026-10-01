import hashlib, json
from pathlib import Path

root = Path.cwd()
evidence = root / 'Docs/QA/MainStageCleanReproduction_20261001/Evidence'
def digest(path):
    with path.open('rb') as stream:
        return hashlib.file_digest(stream, 'sha256').hexdigest().upper()
protected = json.loads((evidence / 'external_protected_start.json').read_text(encoding='utf-8-sig'))
manifest = json.loads((evidence / 'external_input_manifest.json').read_text(encoding='utf-8-sig'))
paths = json.loads((evidence / 'external_build_paths.json').read_text(encoding='utf-8-sig'))
changes = [name for name, sha in protected.items() if not (root/name).is_file() or digest(root/name) != sha]
result = {'protected_existing_files':len(protected), 'production_or_existing_file_changes':changes, 'copies':{}}
for label, data in paths['copies'].items():
    project = Path(data['project'])
    source = [name for name, sha in manifest.items() if name.endswith('.cs') and (not (project/name).is_file() or digest(project/name) != sha)]
    scenes = [name for name, sha in manifest.items() if name.endswith('.unity') and (not (project/name).is_file() or digest(project/name) != sha)]
    bgm = project / 'Assets/Resources/Audio/YasashiiOdori.mp3'
    result['copies'][label] = {'committed_source_differences':source, 'committed_scene_differences':scenes, 'bgm_sha256':digest(bgm)}
result['original_bgm_sha256'] = digest(root/'Assets/Resources/Audio/YasashiiOdori.mp3')
result['scene_sha256'] = digest(root/'Assets/Scenes/MainStage.unity')
result['protection_pass'] = not changes and all(not c['committed_source_differences'] and not c['committed_scene_differences'] and c['bgm_sha256']==result['original_bgm_sha256'] for c in result['copies'].values())
(evidence/'external_protection_result.json').write_text(json.dumps(result, ensure_ascii=False, indent=2),encoding='utf-8')
print(json.dumps(result,ensure_ascii=False,indent=2))
