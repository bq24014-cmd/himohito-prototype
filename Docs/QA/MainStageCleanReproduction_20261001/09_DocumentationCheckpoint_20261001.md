# 整理フェーズ最終checkpoint — 2026-10-01

## 保存する内容

Productionを変更せず、次の制作過程・検証資料のみをGit保存対象とする。

- `Docs/Archive/SubmissionScene_20260911/`
- `Docs/QA/MainStageSceneDiffAudit_20261001/`
- `Docs/QA/MainStageCleanReproduction_20261001/`
- `Docs/Dependencies/ExternalAudio_20261001.md`

基準は `main` / `4919233cd1d0815afb438bd35a7336492ec09548`。
開始時fetch後もHEAD / origin/mainはこのSHAと一致し、indexは空だった。
既存のroot README、LEARNING_LOG、その他QAの未Commit状態は今回の保存対象外。
過去資料中の「commit / push未実施」は各検証時点の履歴として保持する。

## 最終判定と再現条件

Scene差分監査は **CASE S4**。独立Build A/Bの再現性検証は **CASE D1 / MainStage Scene Cleanup = PASS**。

**指定Git HEAD + documented external BGM dependency + Unity 6000.3.21f1 + existing deterministic MainStage Scene Sync** が再現条件。
HEADだけでSceneファイルが不変という意味ではない。
詳細・Runtime smokeの範囲・Human確認範囲は `08_DeterministicSceneSync.md` を参照。

## Archive再検証

ZIP内部は `MainStage.unity` 1件のみ。今回新規の検証用一時ディレクトリへ展開し、SHA256を再照合した。

`57D2464751B5B9C2A78E6C56F34197E9BC2629983FADAA1DFF6E0BA0B4BC2284`

README、SHA256.txt、HEADとの差分patchも存在する。
Archiveは制作過程保存用であり、Production Sceneとして直接使用しない。

## 保護確認

開始時にProduction Assets / Packages / ProjectSettings、既存output / Builds、保存対象外Docs、rootファイルの計2,552ファイルのsize / SHA256を記録。
終了時に同じファイル集合・内容を再照合する。保護manifestはGit保存対象外のtask一時ディレクトリに保持。

- Production MainStage SHA256: `B6191C44EE8FC841784B4C10EB1940DBA5220EAE112165FD09353D32AF879151`。HEADとの差分0。
- BGM SHA256: `25C52DA234D42C7A5E5CC91D45AEED14829F845EA15932A88AC94AB006781E83`。
- BGMはGit未追跡、既存 `.gitignore` の除外方針を維持。
- BGM実体、Production Source / Scene、Build実体は今回の保存対象に含めない。

## QAフォルダ内でもGit保存しない生成物

次の既存生成物はローカルに保持し、明示的なpathspec除外でstageしない。

- `MainStageSceneDiffAudit_20261001/Evidence/__pycache__/` のPython実行キャッシュ。
- `MainStageCleanReproduction_20261001/Evidence/ArchiveExpansion/` のZIP展開用コピー。正式Archive ZIPから復元できる。

SceneDiffAuditの `HEAD_MainStage.unity` / `WorkingTree_MainStage.unity` / `Submission_MainStage.unity` は、許可されたQAフォルダ内の監査証拠であり、Production Sceneではない。
これらとraw patch / logは歴史的証拠として保持する。診断用 `.cs` もDocs内の証拠であり、本体Scriptsへ導入しない。

各保存対象フォルダ内の `.gitattributes` は、Archive ZIP / patchおよびQA Evidenceを `-text` とし、WindowsのGit改行変換で証拠のバイト・SHAが変わらないようにする。Production側の属性設定は変更しない。
raw Scene / patch / log等に含まれる元来の末尾空白は履歴の一部として保持し、整形しない。解説Markdown・診断コードは別途diff-checkする。

Gameplay追加・調整・修正は行わない。Git保存完了後は停止し、新機能は別工程で検討する。
