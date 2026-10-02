# 容量上位30フォルダ

Path略号は[00_README](00_README.md)の絶対Pathに対応。GiBは論理容量。

**以下は包含関係を持つ生ランキング。DとD/Logs、HとH/.codex_tmp等を足してはいけない。** 削除候補合計には重複しない具体的Pathだけを使用した。

| 順位 | Path | GiB | File count | Last modified JST | 用途 | 分類 |
| --- | --- | ---: | ---: | --- | --- | --- |
| 1 | D | 54.569 | 741010 | 2026-09-30 18:05:18 | FROZEN Production + 検証コピー | A KEEP |
| 2 | D/Logs | 51.160 | 704291 | 2026-09-30 09:54:34 | 診断Unityコピー/Art/ログ混在 | D DO NOT DELETE YET |
| 3 | C | 30.518 | 194405 | 2026-10-02 01:26:20 | Codex作業領域 | A KEEP |
| 4 | DC | 30.413 | 42474 | 2026-10-02 01:26:12 | Codex出力/Project親 | A KEEP |
| 5 | DC/2026-08-07/new-chat | 30.265 | 41470 | 2026-10-02 01:26:12 | HimoHitoを含む親 | A KEEP |
| 6 | DC/2026-08-07 | 30.265 | 41470 | 2026-10-02 01:26:12 | 日付別出力親 | A KEEP |
| 7 | H | 30.244 | 40901 | 2026-10-02 01:26:12 | HimoHito本体/隔離/QA/Build | A KEEP |
| 8 | DC/2026-08-07/new-chat/outputs | 30.244 | 40901 | 2026-10-02 01:26:12 | HimoHitoを含む出力親 | A KEEP |
| 9 | C/visualizations/2026 | 19.217 | 186795 | 2026-09-30 18:05:18 | Proof/Repository/画像 | A KEEP |
| 10 | C/visualizations | 19.217 | 186795 | 2026-09-30 18:05:18 | 同上の親 | A KEEP |
| 11 | V | 19.168 | 186243 | 2026-09-30 18:05:18 | DESERT旧Proof/Frozen repo/raw | D DO NOT DELETE YET |
| 12 | C/visualizations/2026/09/12 | 19.168 | 186243 | 2026-09-30 18:05:18 | Vを含む親 | A KEEP |
| 13 | C/visualizations/2026/09 | 19.168 | 186243 | 2026-09-30 18:05:18 | 同上の親 | A KEEP |
| 14 | H/.codex_tmp | 18.365 | 33432 | 2026-10-02 01:08:17 | 最新2.5Dと旧隔離コピー混在 | D DO NOT DELETE YET |
| 15 | C/sessions/2026 | 9.414 | 190 | 2026-10-02 01:26:20 | 会話/実行履歴 | A KEEP |
| 16 | C/sessions | 9.414 | 190 | 2026-10-02 01:26:20 | 同上の親 | A KEEP |
| 17 | H/.codex_tmp/MainStageCleanReproduction_20261001 | 6.243 | 10197 | 2026-10-01 16:03:47 | 旧再現性診断コピー | C PROBABLY SAFE（全体削除不可） |
| 18 | H/output | 6.105 | 2366 | 2026-10-02 01:15:12 | 最新2.5Dと旧Windows Build | D DO NOT DELETE YET |
| 19 | D/Logs/M5CameraBPlaytest | 5.962 | 87513 | 2026-09-22 08:21:26 | 旧カメラ診断Project | D DO NOT DELETE YET |
| 20 | C/sessions/2026/09 | 4.938 | 156 | 2026-10-02 01:26:20 | 会話/実行履歴 | A KEEP |
| 21 | C/sessions/2026/08 | 4.152 | 17 | 2026-09-11 20:25:47 | 会話/実行履歴 | A KEEP |
| 22 | C/sessions/2026/08/08 | 4.060 | 2 | 2026-09-11 20:25:47 | 会話/実行履歴 | A KEEP |
| 23 | V/UDF2 | 3.915 | 9092 | 2026-09-30 18:05:07 | Frozen source/checkpoint/raw | D DO NOT DELETE YET |
| 24 | H/.codex_tmp/MainStageCleanReproduction_20261001/DeterministicAB_20261001_154205 | 3.839 | 6012 | 2026-10-01 16:03:47 | A/B再現性検証コピー | C PROBABLY SAFE（全体削除不可） |
| 25 | D/Logs/SpaceLoopPhase1 | 3.551 | 57869 | 2026-09-24 12:01:21 | 旧Scene/Source/Unity診断 | D DO NOT DELETE YET |
| 26 | V/UDF2/Source | 3.281 | 4744 | 2026-09-30 18:05:07 | GitHub saved subset + raw | D DO NOT DELETE YET |
| 27 | V/UDF2/Source/Docs | 3.276 | 4251 | 2026-09-30 18:05:07 | QA/Art資料とraw | D DO NOT DELETE YET |
| 28 | V/UDF2/Source/Docs/ArtReview | 3.187 | 3705 | 2026-09-30 18:05:07 | Art Review Evidence | D DO NOT DELETE YET |
| 29 | H/Docs | 3.144 | 2237 | 2026-10-02 01:26:12 | 最新未保存QA/Archive/資料 | A KEEP |
| 30 | H/Docs/QA | 3.046 | 2135 | 2026-10-02 01:18:07 | 最新2.5D等の検証Evidence | A KEEP |

## Git / 再生成性 / 未保存状況

各30件の完全な個別記録（Git管理件数、origin/main保存件数、再生成可能性、未保存内容、削除リスク）は [Evidence/Top30.csv](Evidence/Top30.csv) に記録。一般化して省略判定したのではなく、対応Repoのtracked list / origin tree / dirty pathsを各Pathで照合した。

- H：main / HEAD=origin/main=GitHub `702a4bc044b0eb35bedb552753ca8495599bc1ee`。Production 2.5D変更は未保存。H/.codex_tmpはignore、H/outputはuntracked。
- D：main / HEAD `5a004e294713789a1f6642ca56b3b5c9bc601e57`。origin/main/GitHubは `e6dc6acf0227382cc94398e6c1d36f716ff3bbb0`。1114件の未保存状態を保護。
- V/UDF2/Source：e6dc6acのtracked subsetは保存済み。84 rawは未保存。Docs/ArtReview全体を保存済みとは扱わない。
- C/sessions：GitHub保存未確認、再生成不可/未確認。通常の削除可能なlogとして扱わない。
- 親フォルダ：未保存成果物/履歴を含む。容量が大きくても全体削除は不可。

## 主要な非重複10フォルダ

生ランキングの年/月/日やProject親の重複を除くと、占有元は次の10区画として見やすく整理できる。これは全ディレクトリ生ランキングとは別の表示。

| Path | GiB | 主な内容 |
| --- | ---: | --- |
| D/Logs | 51.160 | 旧診断Unityコピー。Library部分だけ候補 |
| V | 19.168 | DESERT Proof/隔離Repo。Library部分だけ候補 |
| H/.codex_tmp | 18.365 | 旧QA + 最新2.5D。旧Library部分だけ候補 |
| C/sessions | 9.414 | 会話/実行履歴。KEEP |
| H/output | 6.105 | 最新2.5D Build等。親はKEEP |
| H/Docs | 3.144 | QA/Archive/文書。KEEP |
| D/Library | 1.984 | 再import可能な本体cache、終了確認後 |
| H/Builds | 1.758 | 提出/共有/旧Build。提出物KEEP |
| D/Docs | 0.869 | 未保存Art/検証資料を含む。KEEP |
| D/.git | 0.537 | 履歴/参照/Index。KEEP |

この10件にも「削除可容量」を意味するものはない。具体的な57 cache候補と3 Build候補は別表に限定している。
