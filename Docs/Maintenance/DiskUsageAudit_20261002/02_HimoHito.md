# HimoHitoPrototype 保護・容量・Git保存状況

## 開始確認

`git status` / `git branch --show-current` / `git rev-parse HEAD` / `git rev-parse origin/main` を読み取り確認。Optional locksを無効化し、index refreshの任意書込みを抑制した。

- Branch: main
- HEAD / origin/main / GitHub main: `702a4bc044b0eb35bedb552753ca8495599bc1ee`
- Stage: 空。stage / commit / push / fetchは未実施。
- 既存tracked変更4件：MainStage.unity、HimoHitoCraftRoomBackground.cs、Docs/LEARNING_LOG.md、README.md。
- 監査フォルダを除くuntracked 4030ファイル。最新2.5D Source / Shader / 素材 / QA / output等を含む。未保存成果物を最優先で保護。

**MainStage.unityは現在diff=0ではない。** 過去のcleanup結果から推測せず、今回の実Working Treeを保護対象とした。内容を戻していない。

## 主な占有元

| Path | GiB | files | 用途/分類 |
| --- | ---: | ---: | --- |
| H | 30.244 | 40901 | Productionを含む親。A KEEP |
| H/.codex_tmp | 18.365 | 33432 | 最新保護対象と旧コピー混在。D、全体削除禁止 |
| H/output | 6.105 | 2366 | 最新Buildと旧Build混在。D、全体削除禁止 |
| H/Docs | 3.144 | 2237 | QA/Archive/未保存原本。A |
| H/Docs/QA | 3.046 | 2135 | 上の内数。A |
| H/Builds | 1.758 | 889 | 提出物/旧共有Build。混在、全体削除しない |
| H/Library | 0.422 | 1385 | 起動中Production。D |
| H/.git | 0.339 | 77 | A KEEP |
| H/Assets | 0.110 | 451 | Source/Scene/画像/BGM。A |
| H/Docs/VisualConcepts | 0.018 | 10 | 未保存の画像原本。A |

## 最新2.5D作業：候補から全面除外

| 作業 | .codex_tmp GiB | QA GiB | 状態 |
| --- | ---: | ---: | --- |
| Tutorial2p5DProof_20261001 | 0.778 | 0.420 | P1、保護 |
| Tutorial2p5DForegroundRefine_20261001 | 0.794 | 0.421 | R1、保護 |
| Tutorial2p5DAllSections_20261001 | 0.794 | 0.335 | T1、保護 |
| Tutorial2p5DProductionIntegration_20261001 | 1.515 | 0.995 | Production統合、保護 |
| Tutorial2p5DIntroFix_20261002 | 1.208 | 0.624 | Human採用、保護 |

加えて `H/.codex_tmp/Tutorial2p5D_20261001` の準備用ファイルを保護。関連BuildはBaseline/Production/Proof/Refine/Trial/IntroFixを含め**全て**保護。

非重複の保護小計：隔離5.089 + QA2.794 + output4.243 = **12.126 GiB**。Production Source/Shader/ArtとVisualConceptsはこの小計以外にもKEEP。最新作業内のLibraryだけを取り出すことも今回は候補にしない。

特に保持する未保存のProduction：

- `Assets/Scripts/HimoHitoCraftRoomBackground.cs`
- `Assets/Scripts/TutorialCraftRoomLayers.cs` と `.meta`
- `Assets/Resources/Art/Tutorial2p5D/` と関連 `.meta`
- `Assets/Resources/TutorialBackgroundEdge.shader` / `TutorialBackgroundFloor.shader` と `.meta`
- `Assets/Scenes/MainStage.unity`
- README / LEARNING_LOGの既存変更
- 上記5 QAフォルダ、VisualConcepts、最新review exeを含むoutput

## QA保存状況

| QAフォルダ | ローカル GiB | GitHub origin/mainにあるファイル数 | 判定 |
| --- | ---: | ---: | --- |
| MainStageCleanReproduction_20261001 | 0.078 | 224 | 保存済みsubset + ローカル追加物。QA全体KEEP |
| MainStageSceneDiffAudit_20261001 | 0.003 | 68 | 保存済みsubset + ローカルcache。QA全体KEEP |
| HumanPlaytestFixes_20260930 | 0.064 | 118 | 保存済み、履歴としてKEEP |
| CheckpointMergedBridgeFix_20260930 | 0.016 | 48 | 保存済み、KEEP |
| CheckpointMergedBridgeRepro_20260930 | 0.014 | 36 | 保存済み、KEEP |
| FullGameRegression_20260930 | 0.075 | 0 | 未保存。D/KEEP |
| HumanPlaytest_20260930 | 0.001 | 0 | 未保存。D/KEEP |
| 最新2.5D QA 5件 | 2.794 | 0 | 未保存。D/KEEP、巨大Evidenceも除外しない |

保存済みQAの巨大画像やEvidenceを積極的な候補にはしていない。最新作業の連番画像/PPM等はHuman比較と説明根拠のため保持。

## BGM・必須Archive

- `H/Assets/Resources/Audio/YasashiiOdori.mp3`: 1,786,604 bytes。
- SHA256: `25C52DA234D42C7A5E5CC91D45AEED14829F845EA15932A88AC94AB006781E83`。
- `.gitignore`はライセンス方針により当該mp3の単独再配布を除外。GitHubに無い必須外部依存であり、**A KEEP**。Build/cache内のコピーと原本を混同しない。
- `H/Docs/Archive/SubmissionScene_20260911/`: A KEEP、GitHub保存済みsubset確認。
- 既存ZIPを読み取りだけで検査。内部はMainStage.unity 1件。展開ファイルは作っていない。
- 内部Scene SHA: `57D2464751B5B9C2A78E6C56F34197E9BC2629983FADAA1DFF6E0BA0B4BC2284`。提出時SHAに一致。
- 現在のMainStage.unityも同じSHAで監査開始時に保護。Archiveがあることを理由にWorking Treeを上書きしない。

## Build判定

- 最新2.5D `output/Tutorial2p5D*`：全てD。特に `Tutorial2p5DIntroFix_20261002_Review01` は最新採用レビュー用。
- 旧HumanPlaytestの初回Build：QA未保存のため今回は候補合計に含めない。
- 旧Fixes追試 / CleanReproduction A/B / ExternalBGM Build：対応Source/QAの保存を確認した3フォルダ1.483 GiBのみCの条件付き候補。
- `Builds/Submission` 0.897 GiB、`Builds/SubmissionDocs` 0.003 GiB：提出原本としてA。
- Share 0.501、Windows 0.160、Friend 0.145 GiBと既存Friend ZIP約0.052 GiB：C。配布履歴/入力/当時Sourceの対応未確認のため今回の推奨候補合計には入れない。
- output/manual / output/pdfの資料：A。Buildと思ってまとめて消さない。

## 保護照合

監査初期の主要SHAに加え、Production Assets / ProjectSettings / Packages / Git HEAD・index等481ファイルを読み取りhash照合。Build/QA/隔離コピー/VisualConcepts/Archiveはfile path・size・mtimeリストで開始/終了一致。結果は `Evidence/ProtectionComparison.json` に記録、全項目PASS。

本体への変更、追加実装、Unity起動、Build再生成、Git保存は今回実施していない。
