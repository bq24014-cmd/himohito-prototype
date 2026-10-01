import pathlib,json,subprocess,io,zipfile,shutil,hashlib
ROOT=pathlib.Path.cwd();EV=ROOT/'Docs/QA/MainStageCleanReproduction_20261001/Evidence/DeterministicSceneSync'
paths=json.loads((EV/'paths.json').read_text());manifest=json.loads((EV/'input_manifest.json').read_text())
project=pathlib.Path(paths['task'])/'TraceBuildProject';project.mkdir(exist_ok=False)
payload=subprocess.run(['git','archive','--format=zip','HEAD','Assets','Packages','ProjectSettings'],cwd=ROOT,check=True,capture_output=True).stdout
with zipfile.ZipFile(io.BytesIO(payload)) as archive:archive.extractall(project)
if any(hashlib.sha256((project/p).read_bytes()).hexdigest().upper()!=sha for p,sha in manifest.items()):raise SystemExit('STOP clean input mismatch')
shutil.copyfile(ROOT/'Assets/Resources/Audio/YasashiiOdori.mp3',project/'Assets/Resources/Audio/YasashiiOdori.mp3')
shutil.copyfile(EV/'SyncProbe.cs',project/'Assets/Editor/SyncProbe.cs')
paths['trace_build_project']=str(project);(EV/'paths.json').write_text(json.dumps(paths,ensure_ascii=False,indent=2),encoding='utf-8')
print(project)
