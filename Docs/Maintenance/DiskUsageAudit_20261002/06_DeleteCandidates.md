# 削除候補 — 実行しない

削除の提案と実行を分離する。本Taskでは1件も削除していない。この文書はcleanupの自動実行指示ではない。

## 分類契約

| 分類 | 意味 | 本監査の主な対象 |
| --- | --- | --- |
| A KEEP | 絶対残す | Production/未commit変更、.git、BGM原本、必須Archive、資料/履歴 |
| B SAFE TO DELETE AFTER CONFIRMATION | 再生成可能。使用中でないことと入力保持の確認後のみ | 旧Unity Project直下のLibrary/Temp |
| C PROBABLY SAFE | 不要判断と当時入力/保存先の確認が必要 | 指定旧Build、旧隔離コピーの非cache部分 |
| D DO NOT DELETE YET | 現在作業中/未保存/固有入力未照合 | 最新2.5D全体、Production稼働cache、DESERT raw、親Logs/output/tmp |

GitHub保存済みでも必須Archiveや`.git`を削除候補にはしていない。未保存のSourceを残したまま生成cacheのみを再importする場合はBとした。

## 無条件で即削除可とした候補

**0 GiB。** Unity.exe PID15828（Himo本体）に加えPID14912/25900（ProjectPath不明）が起動中。全Editor終了・対象確認なしにLibrary/Tempを触るのは不可。

## LOW RISK — 確認後70.256 GiB

| 範囲 | 非重複候補GiB | 対象 |
| --- | ---: | --- |
| DESERT LOOP本体と旧診断コピー | 49.713 | Project直下Library。Source/Scene/Docs/Logs親を残す |
| DESERT LOOP visualizations | 12.545 | 旧ProofのLibrary + Temp。raw/Source/Repoは残す |
| HimoHito旧.codex_tmp | 7.998 | 旧QA ProjectのLibrary。最新2.5D全て除外 |
| 合計 | 70.256 | 57個の正確なPath。親子重複なし |

候補は55 Library + 2 Temp。objはProject直下に見つからず0。純粋なlogへの絞込みは候補として採用していない（診断Evidence保持を優先）。

正確なPath、File count、Last modified、所属Project、Git/GitHub、再生成条件、リスクは [Evidence/LowRiskCandidates.csv](Evidence/LowRiskCandidates.csv)。読みやすい一覧は [LowRisk_Table.md](Evidence/LowRisk_Table.md)。

必要条件：

1. 全Unity Editorを閉じ、Project不明の2プロセスも使用していないことを確認。
2. 親ProjectのAssets / ProjectSettings / Packages / 固有Probe / 外部入力を保持。
3. 当該Pathが通常の生成cacheであり、唯一のauthoring/Evidenceを手置きしていないことを確認。
4. 最新Tutorial2p5DのSource/QA/Build/copyには一切触らない。
5. 将来の別Taskで削除対象Pathを明示してHuman承認を得る。

再importは時間・ネットワーク・Unity version・外部依存を必要とする。cache削除後の初回起動速度や完全同一GPU結果は保証しない。

## MEDIUM RISK — 確認後1.483 GiB

| H/output配下 | GiB | files | Last modified JST |
| --- | ---: | ---: | --- |
| DeterministicSceneSyncAB_20261001_154205 | 0.742 | 290 | 2026-10-01 15:52:52 |
| MainStageCleanReproduction_ExternalBGM_20261001_151833 | 0.371 | 146 | 2026-10-01 15:35:22 |
| HumanPlaytestFixes_20261001_065300 | 0.371 | 153 | 2026-10-01 07:08:08 |

対応QAとProduction SourceはGitHub checkpointに含まれる。BuildそのものはGit未保存。最新2.5D Buildとは別。

「当時のレビューexeを再利用する必要がない」「必要なHuman確認用を保持する」「対応commit/Unity/BGMから再Buildできる」ことを確認するまでC。歴史Buildのbyte同一再生成は未保証。実行中exeの終了も必要。

## HIGH RISK — 削除非推奨、0 GiBを推奨候補に計上

- Himo最新2.5D関連最低12.126 GiB + Production/Art/Shader/VisualConcepts。
- Himo MainStage.unityと未保存Source、README / LEARNING_LOG変更。
- Himo BGM原本、提出用Build/資料、必須Scene Archive、全`.git`。
- DESERT Production1114変更、Source/Scene/設定/未保存Art、Freeze資料。
- DESERT未保存GPU raw2.596 GiB。84SHA照合済みでも再解析要否未確定。
- `C/sessions` 9.414 GiB。会話/実行履歴であり、単なる再生成logではない。
- VHの0.049 GiBのレビュー、C/generated_images等の原画像。
- H/.codex_tmp、H/output、D/Logs、C/visualizationsの親丸ごと。

旧copy全体、Share/Windows/Friend Build等は将来の追加監査で候補化できる可能性があるが、今回の候補合計には含めない。

## 合計と二重計上防止

LOW70.256 + MEDIUM1.483 = **71.739 GiB**（確認後の候補、未承認）。全候補Pathは互いに親子ではないとscriptで検証。最新Tutorial2p5D内の候補数0。

Top30の親Path容量やraw2.596 GiBを加算していない。logical file lengthsのため、実際に空くdisk容量とは異なる場合がある。
