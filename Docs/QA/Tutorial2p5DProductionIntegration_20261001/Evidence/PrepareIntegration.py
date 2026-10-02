"""Build/QA the production working tree in a fresh isolated project."""
import json
import shutil
from pathlib import Path

ROOT = Path(__file__).resolve().parents[4]
QA = Path(__file__).resolve().parents[1]
PROJECT = ROOT / ".codex_tmp/Tutorial2p5DProductionIntegration_20261001/IntegrationProject"
assert not PROJECT.exists(), "Refuse existing isolation"
for folder in ("Assets", "Packages", "ProjectSettings"):
    shutil.copytree(ROOT / folder, PROJECT / folder)
for name, dest in (("IntegrationQA.cs", "Assets/Diagnostics/IntegrationQA.cs"),
                   ("ReferenceTrial.cs", "Assets/Diagnostics/ReferenceTrial.cs"),
                   ("IntegrationEntry.cs", "Assets/Editor/IntegrationEntry.cs")):
    target = PROJECT / dest
    target.parent.mkdir(parents=True, exist_ok=True)
    shutil.copyfile(QA / "Evidence" / name, target)
shader = ROOT / ".codex_tmp/Tutorial2p5DAllSections_20261001/TrialProject/Assets/Resources/Proof/FloorBlend.shader"
shutil.copyfile(shader, PROJECT / "Assets/Resources/LegacyFloor.shader")
print(PROJECT)
