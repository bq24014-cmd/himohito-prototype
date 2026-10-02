# .codex/visualizations 容量監査

親 `C/visualizations`：**19.217 GiB / 186,795 files**。一括削除は禁止候補。現在参照中のHimo資料と、DESERT LOOPのRepo・未保存raw・旧Unity cacheが混在する。

## DESERT LOOP側 V

V全体19.168 GiB / 186,243 files。主要な子フォルダは以下。

| 子Path | 全体GiB | 全体の判断 |
| --- | ---: | --- |
| UDF2 | 3.915 | Frozen Source/Checkpoint/raw。D |
| UDF1 | 1.962 | Source/Output/旧Proof cache。D、cacheだけ候補 |
| Gate2R1Proof | 1.412 | 固有入力/Sourceは照合未完。D |
| Gate2Proof | 1.404 | 同上 |
| LTI2 | 1.400 | Art/診断Sourceは保持、Libraryだけ候補 |
| LTI | 1.399 | 同上 |
| HERO | 1.399 | Hero Source再現資料は保存済み、親は保持 |
| GRA | 1.397 | Geometry資料/Source保持 |
| Proof2ColorFix | 1.396 | Color検証資料/Source保持 |
| Proof2Calibration | 1.393 | Calibration資料/Source保持 |
| UDF1B | 1.308 | clean隔離Repo/再現資料を保持 |
| UDF1A | 0.642 | clean隔離Repo/再現資料を保持 |
| Gate2MethodCloseSource | 0.053 | 保存先照合未完、D |
| Gate2MethodCloseCheckpoint | 0.028 | 同上 |
| Gate2R1Source | 0.022 | 同上 |
| FREEZE | <0.001 | freeze receipts / audit / 保存証明。A |

V内の**古いUnity Library / Tempだけ12.545 GiB**がB候補。9つのLibraryに加えてUDF1のTemp等を個別記録。`V` / `UDF2` / `Source` / `Docs` / `Evidence`親全体を候補にしていない。

一部再現SourceとFreezeはe6dc6acでGitHub保存済み。一方、84 raw2.596 GiBはGit未保存でD。原本のSHAとmanifestを照合した。Freeze記録があることを理由に未保存内容まで削除可とはしていない。

## HimoHito側 VH

VHは0.049 GiB / 552 files。今回の作業のvisualization rootでもあるが、内部の主な既存成果物は過去レビュー。

| 子Path | GiB | files | 用途 |
| --- | ---: | ---: | --- |
| deck-review-0824 | 0.016 | 90 | PowerPointレビュー出力 |
| himohito-review-audit | 0.009 | 67 | ゲーム資料レビュー |
| video-review | 0.008 | 284 | 動画確認用出力 |
| ppt-render | 0.007 | 59 | スライド描画 |
| ppt-review-third | 0.006 | 40 | スライドレビュー |
| client-screenshots-2026-08-24 | 0.002 | 8 | screenshot |
| video-review-new | 0.001 | 2 | 動画確認 |

Repoは検出されていない。元入力や採用成果物の保存先との全件照合はしていないため **D DO NOT DELETE YET**。小容量であり、未保存内容を失うリスクに対して優先度が低い。

最新Tutorial2p5D 5 Taskの実体は主にH側のDocs/.codex_tmp/outputにある。VHを空にすれば最新作業に影響がない、とは断定しない。作成コードが他のローカルPathを参照している可能性は、全Source横断依存照合をしていないため未確定。

## 当面のKEEP

全`.git`、Freeze資料/receipt、Source/Art原本、最新参照画像、未保存raw、Himo過去レビューの唯一の出力は保持。対象Rootの丸ごとcleanupは非推奨。

絶対Path / 更新時刻 / Git管理 / GitHub / 再生成可能性は `Evidence/FolderInventory.csv`、用途別一覧は `DesertVisualizations_Table.md` と `HimoVisualizations_Table.md`。
