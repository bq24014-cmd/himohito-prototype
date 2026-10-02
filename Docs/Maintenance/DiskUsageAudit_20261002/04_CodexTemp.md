# Codex隔離コピー / .codex_tmp

`H/.codex_tmp` は18.365 GiB、33,432 files。`.gitignore`で親全体が除外されているため、置いてあるだけではGitHub保存されない。

**親フォルダ全体はD DO NOT DELETE YET。** 最新2.5D、原資料、診断Probe、旧Unity cacheが混在する。

## 主要な隔離コピー

| H/.codex_tmp配下 | 全体GiB | 分類 | 判断 |
| --- | ---: | --- | --- |
| MainStageCleanReproduction_20261001 | 6.243 | C | QAの保存済みsubsetあり。LibraryのみB。コピー全体はProbe/入力の照合未完 |
| Tutorial2p5DProductionIntegration_20261001 | 1.515 | D | 最新作業、内部Libraryも全て保護 |
| FullGameRegression_20260930 | 1.496 | D | QA未保存。Source/Probe/結果保持、旧LibraryのみB |
| Tutorial2p5DIntroFix_20261002 | 1.208 | D | 最新採用作業、内部Libraryも全て保護 |
| SubmissionBuild20260911 | 1.096 | A/D | 当時の提出snapshot。LibraryだけB、Source/成果物保持 |
| HumanPlaytest_20260930_231714 | 1.094 | D | QA未保存。LibraryだけB |
| HumanPlaytestFixes_20261001_065300 | 1.094 | C | 採用済みQA/Source保存あり。LibraryのみB、コピー全体はまだ候補外 |
| Tutorial2p5DAllSections_20261001 | 0.794 | D | 最新保護指定 |
| Tutorial2p5DForegroundRefine_20261001 | 0.794 | D | 最新保護指定 |
| Tutorial2p5DProof_20261001 | 0.778 | D | 最新保護指定 |
| CraftUiCheck | 0.483 | C | 旧見た目QA、固有画像/Probe保持。LibraryだけB |
| HumanPlaytestFixes_20260930 | 0.398 | C | LibraryだけB |
| CheckpointMergedBridgeFix_20260930 | 0.398 | C | QA保存済み、LibraryだけB |
| MainStageSceneDiffAudit_20261001 | 0.398 | C | QA保存済み、LibraryだけB |
| CheckpointMergedBridgeRepro_20260930 | 0.398 | C | QA保存済み、LibraryだけB |
| doc_report_0911 | 0.038 | D | Word/PDF等原資料・出力照合が必要 |
| manual_0919 | 0.013 | D | 説明書原資料/出力照合が必要 |
| DocsFinalCheckpoint_20261001 | 0.010 | A/D | Archive/整理フェーズの記録を保護 |
| Tutorial2p5D_20261001 | <0.001 | D | 最新2.5D準備ファイル、保護 |

全件のfiles・Last modified・Git管理状況・未保存有無・再生成性は `Evidence/FolderInventory.csv` / `HimoTemp_Table.md`。

## 再生成できる部分

最新Tutorial2p5D群と稼働中Productionを除く、旧Himo Unity Projectの **Library 7.998 GiB** をB候補にした。これだけを対象にし、Assets / ProjectSettings / Packages / Source / Probe / PNG / QAは残す。

Example：

- CleanReproductionのCopyA/CopyB Libraryは各約0.623 GiB。
- ExternalBGM BuildProject Libraryは約0.621 GiB。
- HumanPlaytestFixes追試Project Libraryは約0.621 GiB。
- 原本BGMはH/Assets配下にKEEP。旧copyのBGMを消す操作も今回実施しない。

**6.243 GiBの再現コピー全体を消せばよい、という判定ではない。** 保存済みQAに無い診断コード・ローカル設定・生成証拠があり得るため、全体はC/Dのまま。将来必要ならSource/入力のbyte照合を別Taskで行う。

## .codex本体のtemp関連

| Path | GiB | 判定 |
| --- | ---: | --- |
| C/.tmp | 0.086 | 現在のCodex runtimeで使う可能性。D、今回の候補に含めない |
| C/.sandbox-bin | 0.401 | 実行環境/補助bin。A/D、手動cleanup非推奨 |
| C/.sandbox | 0.066 | 状態/実行補助、D |
| C/cache | 0.032 | 用途・現在参照を確定していない。D |
| C/plugins | 0.456 | 導入plugin/実行依存、A |
| C/generated_images | 0.283 | 生成画像原本の可能性。A/D、コピー保存先未照合 |

認証・sandbox秘密・sessionsの内容は読んでいない。開発容量が逼迫していてもCodex本体のtemp/cacheを名前だけで削除可とは判断しない。
