# DESERT LOOP — FROZEN / 読み取り監査

DESERT LOOPの実装・Source・Scene・Git状態を変更していない。現在の凍結判断を維持し、他Taskや再開工程は開始していない。

## Production本体

本体は指定2ルート外の `D = C:/Users/田尻大翔/Documents/UnityProjects/DesertLoop` に存在。Project一覧とfilesystemを読み取り確認し、対象に追加した。

- Branch: main
- HEAD: `5a004e294713789a1f6642ca56b3b5c9bc601e57`
- local origin/main / GitHub main: `e6dc6acf0227382cc94398e6c1d36f716ff3bbb0`
- 未保存状態1114件：tracked変更17 + untracked1097。Indexは空。
- HEADはorigin/mainの祖先。凍結Sourceのcheckpointは隔離Repo側で保存されたもので、Production Working Treeはcleanではない。
- pull / checkout / reset / stash / cleanup等は実施していない。

| Path | GiB | files | 分類/理由 |
| --- | ---: | ---: | --- |
| D | 54.569 | 741010 | A KEEP、Production親 |
| D/Logs | 51.160 | 704291 | D、診断Project/Source/Art/ログ混在 |
| D/Library | 1.984 | 32475 | B、全Unity終了確認後のみcache候補 |
| D/Docs | 0.869 | 2785 | A、未保存Art/Review/Manualあり |
| D/.git | 0.537 | 819 | A、local履歴/index/参照 |
| D/Assets | 0.019 | 587 | A、未保存Source/Scene/Artあり |

Assets / ProjectSettings / Packagesおよび変更Source等622ファイルのSHAとGit状態を保護照合し、開始/終了一致を確認。Production全体やLogs親を削除可能とは判定しない。

## Freeze checkpointのGitHub確認

読み取り `git ls-remote` で以下を確認した。fetchはしていない。

- main: `e6dc6acf0227382cc94398e6c1d36f716ff3bbb0`
- tag: `desert-loop-frozen-2026-09-30`
- tag object: `5a128c8ebc88d00ab497ea7e68918c9a76618970`
- tagのpeeled commit: `e6dc6acf0227382cc94398e6c1d36f716ff3bbb0`

`V/UDF2/Source/Docs/Progress/DESERT_LOOP_FROZEN_20260930.md` と `DESERT_LOOP_FREEZE_MANIFEST_20260930.md` を読み取り確認。HeroShot等9 ArtReviewフォルダ、保存payload627ファイル/78,085,487 bytes、source-only再現資料の保存記録がある。

この記録とorigin/main treeを照合した。**「DESERT LOOPの全てがGitHubにある」という意味ではない。** 本体の未保存変更やLogs内Project、除外rawを分けている。

## 旧診断UnityコピーとLibrary

| 診断親Path | 全体GiB | 処理方針 |
| --- | ---: | --- |
| D/Logs/M5CameraBPlaytest | 5.962 | Project/Source保持。3つのLibraryのみ候補 |
| D/Logs/SpaceLoopPhase1 | 3.551 | Project/Source保持。Libraryのみ候補 |
| D/Logs/GateAContract | 2.851 | Source/Scene/Art保持。Library2.784 GiBのみ候補 |
| D/Logs/M5CameraComparison_20260920 | 2.005 | Library約1.958 GiBのみ候補 |
| D/Logs/ProductionArtIntegration03 | 1.963 | P03 authoring/Scene等保持。Library約1.861 GiBのみ候補 |
| D/Logs/ProductionArtIntegration02 | 1.818 | Library約1.792 GiBのみ候補 |
| D/Logs/M5DistanceAB / C / D / E1 / M5E1V2等 | 各約1.77 | 個別ProjectのLibraryだけ候補 |

本体・診断コピーのLibrary小計 **49.713 GiB**。このうち本体Library1.984、Logs内の診断Library約47.730 GiB。全ProjectのSource/Assets/Packagesは残す条件で再import可能。

Logsの全体51.160 GiBとの差約3.430 GiBにはSource/Scene/生成Art/診断Evidence等が含まれる。名前がLogsでも単なるlogとは扱わない。候補Pathは `Evidence/LowRiskCandidates.csv`、全診断親の容量/時刻/Git状況は `FolderInventory.csv` と `DesertLogs_Table.md`。

## visualizations内の保存済みRepoと未保存データ

| Path | HEAD短縮 | status | 判断 |
| --- | --- | --- | --- |
| V/UDF1/Source、V/UDF1A/Source | 1aab1c6 | clean | e6dc6acの祖先。履歴はremoteに含まれる |
| V/UDF1B/Checkpoint、V/UDF1B/Source | 67f5185 | clean | 同上 |
| V/UDF2/Checkpoint | a4e2a38 | clean | 同上 |
| V/UDF2/Source | e6dc6ac | tracked clean + raw84未追跡 | Frozen sourceの正本。A/D KEEP |

6 RepoのHEADはいずれもorigin/mainの祖先とread-onlyで確認。各`.git`もA KEEPで候補に含めない。Gate2系や旧ProofのRepo外Source全体は、保存位置/byte一致の全件照合が未完のため一括削除候補にはしない。

V内の旧Unity cache（Library + Temp）だけ **12.545 GiB** をLOW RISK候補にした。Source/Scene/ArtとFreeze receiptは保持。

## GitHubに保存されていない巨大raw

`V/UDF2/Source/Docs/ArtReview/.../Evidence/` の `.rgba32f`：

- 84ファイル、各33,177,600 bytes、合計2,786,918,400 bytes = **2.596 GiB**。
- Freeze manifestで意図的にGit対象から除外されたGPU float readback。
- 全84件の現物SHA256を `ExcludedEvidence.csv` と照合し一致。
- CSV/PNG/JSON等の派生解析結果とsource-only再現手順はGitHub保存の記録・treeあり。
- readbackの再撮影は可能でも、GPU/Unity/環境差により当時のfloat値をbyte同一に再生成できるとは保証しない。
- 本監査範囲ではローカルにしかないraw。PC全域/外付け/クラウド上の別copyの有無は未確認。

**D DO NOT DELETE YET / 削除非推奨。** 凍結再開時に再解析が必要かHuman判断するまで残す。LOW/MEDIUMの容量合計には含めない。詳しい84件のPath・SHA・役割は `Evidence/DesertFreezeRawDumps.csv`。

## 凍結再開に必要なKEEP

Production未保存変更、Assets/Scene/設定、`.git`、Freeze資料/receipt、source-only再現資料、Art原本、未保存rawは保持。Libraryだけの将来削除でも、再import時間・package取得・Unity version・依存入力は必要になる。
