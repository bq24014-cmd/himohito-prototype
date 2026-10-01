import hashlib, importlib.util, io, json, pathlib, shutil, subprocess, zipfile
ROOT=pathlib.Path.cwd(); EV=ROOT/'Docs/QA/MainStageCleanReproduction_20261001/Evidence/DeterministicSceneSync'
paths=json.loads((EV/'paths.json').read_text());manifest=json.loads((EV/'input_manifest.json').read_text())
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest().upper()
def git(*args):return subprocess.run(['git',*args],cwd=ROOT,check=True,capture_output=True).stdout
head=git('show','HEAD:Assets/Scenes/MainStage.unity')
report={};patches={}
for label in ['A','B']:
    project=pathlib.Path(paths['copies'][label]['project']);scene=project/'Assets/Scenes/MainStage.unity'
    log=(EV/f'Build{label}.log').read_text(encoding='utf-8',errors='replace')
    if 'Build Finished, Result: Success.' not in log or not (project/'Builds/Windows/Game/HimoHitoPrototype.exe').is_file():raise SystemExit('STOP build not successful')
    post=scene.read_bytes();(EV/f'Copy{label}_after.unity.txt').write_bytes(post)
    import difflib
    patch=''.join(difflib.unified_diff(head.decode().splitlines(keepends=True),post.decode().splitlines(keepends=True),fromfile='HEAD/Assets/Scenes/MainStage.unity',tofile='SYNC/Assets/Scenes/MainStage.unity'))
    (EV/f'Copy{label}_HEAD_to_SYNC.patch').write_text(patch,encoding='utf-8');patches[label]=patch
    report[label]={'before_sha':paths['copies'][label]['scene_before_sha256'],'after_sha':sha(scene),'scene_size':len(post),'build_success':True,'source_changes':[p for p,s in manifest.items() if p.endswith('.cs') and sha(project/p)!=s]}
    if report[label]['source_changes']:raise SystemExit('STOP source changes')
report['post_scene_byte_identical']=(EV/'CopyA_after.unity.txt').read_bytes()==(EV/'CopyB_after.unity.txt').read_bytes()
report['patch_content_identical']=patches['A']==patches['B']
report['third_prior_build_hash_equal']=report['A']['after_sha']=='71D9DF053D63FD2F1B673CCC5A02FA26CCC6AE0956F3272A82C307FC97DE602A'
(EV/'build_comparison.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
# Inspect the serialized whole scene. No Production or scene writing.
spec=importlib.util.spec_from_file_location('old_audit',ROOT/'Docs/QA/MainStageSceneDiffAudit_20261001/Evidence/analyze.py');audit=importlib.util.module_from_spec(spec);spec.loader.exec_module(audit)
import collections
structure={}
for label in ['A','B']:
    ds=audit.docs((EV/f'Copy{label}_after.unity.txt').read_bytes());ident=audit.identity(ds)
    structure[label]={'documents':len(ds),'objects':sum(d['class']==1 for d in ds.values()),'components_and_documents_by_type':dict(collections.Counter(d['type'] for d in ds.values())),'nodes':sorted({i['path'] for fid,i in ident.items() if ds[fid]['class']==1})}
(EV/'serialized_structure.json').write_text(json.dumps(structure,ensure_ascii=False,indent=2),encoding='utf-8')
# Only post-Build diagnostic additions. Existing Source stays byte-identical.
parent=EV.parent
for label in ['A','B']:
    project=pathlib.Path(paths['copies'][label]['project'])
    qa=project/'Assets/QA';qa.mkdir(exist_ok=False)
    for src,dst in [('FullSystems.cs',qa/'FullSystems.cs'),('ExternalFocused.cs',qa/'CleanFocused.cs'),('ExternalEntry.cs',project/'Assets/Editor/CleanEntry.cs')]:shutil.copyfile(parent/src,dst)
    # Isolate PlayerPrefs per copy; does not change gameplay or existing sources.
    entry=project/'Assets/Editor/CleanEntry.cs'
    entry.write_text(entry.read_text().replace('"CleanReproduction_ExternalBGM_20261001_" + mode','"DeterministicAB_20261001_'+label+'_" + mode'),encoding='utf-8')
    shutil.copyfile(EV/'SyncProbe.cs',project/'Assets/Editor/SyncProbe.cs')
    old= (ROOT/'Docs/QA/MainStageSceneDiffAudit_20261001/Evidence/SceneAuditEntry.cs').read_text()
    old=old.replace('File.WriteAllBytes(System.IO.Path.Combine(output,label+".png"),tex.EncodeToPNG());','WritePpm(System.IO.Path.Combine(output,label+".ppm"),tex);')
    method='''\n    static void WritePpm(string path,Texture2D tex){\n        var pixels=tex.GetPixels32();using(var f=new FileStream(path,FileMode.Create)){\n            var header=System.Text.Encoding.ASCII.GetBytes("P6\\n"+tex.width+" "+tex.height+"\\n255\\n");f.Write(header,0,header.Length);\n            for(int y=tex.height-1;y>=0;y--)for(int x=0;x<tex.width;x++){var c=pixels[y*tex.width+x];f.WriteByte(c.r);f.WriteByte(c.g);f.WriteByte(c.b);}\n        }\n    }\n'''
    pos=old.rfind('}');old=old[:pos]+method+old[pos:]
    (project/'Assets/Editor/SceneAuditEntry.cs').write_text(old,encoding='utf-8')
# Separate Cause project starts at clean HEAD, with passive save-event observer.
cause=pathlib.Path(paths['task'])/'CauseProject';cause.mkdir(exist_ok=False)
with zipfile.ZipFile(io.BytesIO(git('archive','--format=zip','HEAD','Assets','Packages','ProjectSettings'))) as z:z.extractall(cause)
shutil.copyfile(ROOT/'Assets/Resources/Audio/YasashiiOdori.mp3',cause/'Assets/Resources/Audio/YasashiiOdori.mp3')
shutil.copyfile(EV/'SyncProbe.cs',cause/'Assets/Editor/SyncProbe.cs')
paths['cause_project']=str(cause);(EV/'paths.json').write_text(json.dumps(paths,ensure_ascii=False,indent=2),encoding='utf-8')
print(json.dumps(report,indent=2));print(json.dumps({k:{'objects':v['objects'],'documents':v['documents']} for k,v in structure.items()}))
