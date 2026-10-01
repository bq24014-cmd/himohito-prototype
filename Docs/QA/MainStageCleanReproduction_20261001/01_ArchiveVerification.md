# Archive検証

Archive: `Docs/Archive/SubmissionScene_20260911/`

| 確認 | 結果 |
|---|---|
| 現在のWorking Tree Sceneと2026-09-11提出snapshotのbyte一致 | PASS |
| ZIP内はMainStage.unityだけ | PASS |
| 実際のZIP展開後SHA一致 | PASS |
| README / SHA256.txt / HEAD→提出Sceneのpatch | PASS |
| MainStageSceneDiffAuditのEvidence / 再照合記録 | PASS |
| GitHubファイルサイズ上限未満 | PASS |

提出Scene / ZIP展開後SceneのSHA256:

`57D2464751B5B9C2A78E6C56F34197E9BC2629983FADAA1DFF6E0BA0B4BC2284`

ZIPは27,812 bytes。Archiveと既存Auditの合計3,710,246 bytes、最大ファイル526,828 bytes。
詳細はEvidence/archive_verification.json。これはclean検証資料・後から追加したhandoff文書を含める前のサイズ検証値。

Archive PASS後に、このSceneだけを明示的許可に従い`git restore --source=HEAD --worktree -- Assets/Scenes/MainStage.unity`で復元。
HEAD版SceneのSHA256:

`B6191C44EE8FC841784B4C10EB1940DBA5220EAE112165FD09353D32AF879151`

Scene diff=0 / Scene diff --check PASS。Archiveのpatchは現在のHEAD版Sceneに対する`git apply --check`でも適用可能。
過程の復元用データはローカルに保持済みだが、総合PASS未成立のためまだGitHubへ保存していない。
