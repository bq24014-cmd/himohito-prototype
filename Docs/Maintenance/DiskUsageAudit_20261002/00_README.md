# Disk Usage Audit — 2026-10-02

監査のみ完了。削除・移動・rename・圧縮・cleanup・Git変更操作は実施していない。作成したものは、この新しい監査フォルダ内の文書とEvidenceのみ。

## 結論

| 区分 | 容量 | 判定 |
| --- | ---: | --- |
| 読み取りできた対象総容量 | 115.500 GiB / 977,889 files | 親子の二重計上なし、3ルート合計 |
| 無条件で即削除可と判定した候補 | 0 GiB | Unity.exeが3件起動中、2件はProject不明 |
| LOW RISK候補 | 70.256 GiB | 全Unity終了と対象確認後の旧Library / Temp、57フォルダ |
| MEDIUM RISK候補 | 1.483 GiB | 必要な旧Human版を残す判断後のWindows Build、3フォルダ |
| 確認後の候補合計 | 71.739 GiB | 上記LOW + MEDIUM、重複なし。まだ削除未承認 |
| 最新HimoHito 2.5Dの明示保護分 | 最低12.126 GiB | 隔離コピー・QA・Buildだけの小計。Production/画像原本等は別途保護 |
| DESERT LOOP未保存raw Evidence | 2.596 GiB / 84 files | DO NOT DELETE YET。候補合計に含めない |

**最初に整理を検討するなら、DESERT LOOPの旧診断コピーのLibraryだけ。`Logs`やProject全体ではない。** 最大の単一候補は `D/Logs/GateAContract/UnityProject/Library`、2.784 GiB。

## 対象とPath略号

以下の略号は全報告書共通。CSVには省略しない絶対Pathも記録している。

| 略号 | 絶対Path |
| --- | --- |
| H | `C:/Users/田尻大翔/Documents/Codex/2026-08-07/new-chat/outputs/HimoHitoPrototype` |
| D | `C:/Users/田尻大翔/Documents/UnityProjects/DesertLoop` |
| C | `C:/Users/田尻大翔/.codex` |
| DC | `C:/Users/田尻大翔/Documents/Codex` |
| V | `C:/Users/田尻大翔/.codex/visualizations/2026/09/12/01a095fe-94f6-7783-a93d-497f16bab356` |
| VH | `C:/Users/田尻大翔/.codex/visualizations/2026/08/08/019fdfa3-5ee1-7911-8a24-63282af5bae4` |

| 非重複の監査ルート | 容量 GiB | files |
| --- | ---: | ---: |
| DC | 30.413 | 42,474 |
| C | 30.518 | 194,405 |
| D（指定2ルート外に見つかったDESERT LOOP本体） | 54.569 | 741,010 |
| 合計 | 115.500 | 977,889 |

指定2ルートだけでは60.931 GiB。Dを追加した合計が115.500 GiB。PC全体やUnityインストール、`.cache/codex-runtimes`等はこの合計の対象外。

## 読む順序

1. [上位30フォルダ](01_TopFolders.md)
2. [HimoHitoの保護・保存状況](02_HimoHito.md)
3. [DESERT LOOPのFROZEN状態](03_DesertLoop.md)
4. [隔離コピー](04_CodexTemp.md)
5. [visualizations](05_Visualizations.md)
6. [削除候補と除外対象](06_DeleteCandidates.md)
7. [推奨整理順序](07_RecommendedCleanup.md)

## 測定方法・制限

- 単位は **GiB = 1,073,741,824 bytes**。一般的なGB表示とは異なる。
- 容量はファイルの論理長。NTFSの圧縮、sparse、hardlink、allocation unitを考慮した実占有量・実際に空く容量の保証ではない。
- 48件の読取エラー、17件のreparse point / symlinkを除外。権限拒否・長いUnity PackageCache Path等を含む。したがって総容量は読み取りできた範囲の下限値。
- `Last modified`は配下のファイル/ディレクトリの更新時刻の最大値（JST）。最終プレイ時刻や採用日時ではない。
- 稼働中のCodex履歴などは監査中も更新され得る。集計は2026-10-02の点-in-time値。監査文書自体の後続増分は元集計には含まれない。
- `.codex`はメタデータ集計のみ。会話履歴・認証情報・sandbox秘密の内容は読んでいない。
- GitHub保存確認はread-onlyの `git ls-remote` とローカルの対応commit tree/status照合。fetch / pull / merge / rebase / stage / commit / pushは実施していない。
- 「Gitに無い」だけで消してよいとは判断していない。キャッシュと唯一の成果物を分離した。

## Evidence

- `DirectorySurveyWithDesert.json`: 全97,080フォルダの容量メタデータ、エラー、除外links。
- `Top30.csv`: 上位30の絶対Path、容量、files、時刻、用途、Git/GitHub、再生成性、未保存有無、分類、リスク。
- `FolderInventory.csv`: Project/QA/Build/隔離コピーの主要フォルダ詳細。
- `UnityCaches.csv`: Unity ProjectごとのLibrary / Temp / Logs / obj調査。
- `LowRiskCandidates.csv`: 57個の正確な候補Path。親フォルダ削除を意味しない。
- `MediumRiskCandidates.csv`: 旧Windows Build 3件。
- `DesertFreezeRawDumps.csv`: raw 84件、Freeze manifestとのSHA照合。
- `GitRepositories.json` / `RemoteRefs.json`: Repo/remote/Freeze tagの読み取り確認。
- `HimoProtectionStart.json`: 監査初期のGit状態と主要Production/BGM SHA。
- `ProtectionStart.json` / `ProtectionEnd.json` / `ProtectionComparison.json`: Source・設定・Git identityとBuild/QA等の保護照合。
- `CandidateSummary.json`: 非重複容量合計、保護除外判定。

## 終了時保護確認

`Evidence/ProtectionComparison.json` は全項目PASS。

- HimoHito 481ファイル、DESERT LOOP 622ファイルのSource/Assets/設定/Git identityのSHAと状態が保護snapshotから変化なし。
- 監査初期のMainStage / Tutorial / 背景Source / Shader / 2.5D Art / BGMのSHA全一致。
- Himoのoutput / Builds / .codex_tmp / QA / VisualConcepts / Archiveのfile path・size・mtimeリストが一致。
- 両Repoともstageは空。Himo HEAD / origin/mainは702a4bcのまま。
- 既存Working Treeの変更は維持。今回増えた未追跡フォルダはこの監査資料のみ。

これらは検証した対象・範囲の保護確認であり、稼働中Codex session等までbyte不変と主張するものではない。

今回の出力だけを追加し、Productionには手を入れず、ここで停止する。
