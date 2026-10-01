# Git / 原本保護

対象repo：`https://github.com/bq24014-cmd/himohito-prototype.git`。DESERT LOOPにはアクセスしていない。

開始時fetch後のbranchは`main`、HEAD / origin/mainはともに`4919233cd1d0815afb438bd35a7336492ec09548`。想定外のremote進行なし。最初の昇格コマンドではfetch以外のGit読取にsafe.directoryがなくownership警告が出たため、全Git呼出しにper-command safe.directoryを付けて開始確認を再実施した。global設定は変更していない。

開始時status：

```text
 M Assets/Scenes/MainStage.unity
 M Docs/LEARNING_LOG.md
 M README.md
?? Docs/QA/FullGameRegression_20260930/
?? Docs/QA/HumanPlaytest_20260930/
?? output/
```

開始時indexは空。終了時に新しく加わるGit上の未追跡物は本QAフォルダだけ。診断projectは新規`.codex_tmp/MainStageSceneDiffAudit_20261001/Project`で、既存コピーを流用・上書きしていない。

## Scene SHA-256

| 対象 | SHA-256 |
| --- | --- |
| 開始Working Tree | `57D2464751B5B9C2A78E6C56F34197E9BC2629983FADAA1DFF6E0BA0B4BC2284` |
| 終了Working Tree | `57D2464751B5B9C2A78E6C56F34197E9BC2629983FADAA1DFF6E0BA0B4BC2284` |
| HEAD blob | `B6191C44EE8FC841784B4C10EB1940DBA5220EAE112165FD09353D32AF879151` |

原本の1byte不変を確認。Source、Scene、Assets/Packages/ProjectSettings、既存Docs、README/AGENTS、output、Buildsについて開始hash一覧と終了照合を`Evidence/protected_hashes_start.json` / `protection_result.json`に保存する。新規本QAは照合対象から除外。既存.codex_tmpは読み取りと新規コピーの作成のみで、既存project/Buildへの書込みなし。Library等のcache全体のhash照合をしたと主張はしない。

`MainStageSceneSync.cs:37-49`はEdit modeでsetupが変更を返すとSaveSceneを呼ぶため、原本Unityは開いていない。隔離A/BのSceneも各実行後のSHAが入力SHAから不変だった。

stage / commit / pushなし。checkout / restore / reset / stash / clean / pull / merge / rebaseなし。Scene保存・whitespace修正なし。
