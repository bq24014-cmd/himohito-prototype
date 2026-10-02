# 推奨整理プラン — まだ実行しない

## 現状

- 読み取り可能な対象容量115.500 GiB。
- C drive freeの読み取り値23,608,430,592 bytes、約21.988 GiB（測定時点）。PC全体の不要容量を測った値ではない。
- Unity.exe 3件稼働。Project不明2件。今回はプロセス終了もしていない。

## LOW RISK — 70.256 GiBの候補

最初の将来cleanupは、**DESERT LOOPの旧診断ProjectのLibraryだけ**を小さく区切って行うのがよい。FROZEN Production/Source/未保存Artはそのまま残せる。

容量効果の大きい候補例：

| 対象 | GiB | 条件 |
| --- | ---: | --- |
| D/Logs/GateAContract/UnityProject/Library | 2.784 | Source/Scene/Art保持、全Unity終了確認 |
| D/Logs/M5CameraBPlaytest/20260920_123216_848/Project/Library | 1.972 | 同上 |
| D/Logs/M5CameraBPlaytest/20260920_005553_756/Project/Library | 1.959 | 同上 |
| D/Logs/M5CameraComparison_20260920/Project/Library | 1.958 | 同上 |
| D/Logs/M5CameraBPlaytest/20260920_141708_692/Project/Library | 1.892 | 同上 |
| D/Logs/ProductionArtIntegration03/UnityProject/Library | 1.861 | P03 Source/authoring保持、Editor終了確認 |

この6候補だけで約12.426 GiB。Source/Scene/QAを削除しなくても大きな効果がある。ただし**本Taskでは1件も実行していない**。

次段階：Dの他の旧診断Library → Vの旧Proof Library/Temp → Hの旧.codex_tmp Library。57件全てを確認後に扱う場合の論理容量小計70.256 GiB。

Himo本体の起動中Library0.422 GiB、最新Tutorial2p5D内のLibrary4.316 GiBは対象から除外済み。今の開発を壊してまで小さな容量を取りに行く必要はない。

## MEDIUM RISK — 1.483 GiBの候補

旧Fixes追試とCleanReproductionのWindows Build3件。対応Source/QAは保存済みだが、Human approved版の比較用exeを残したいか判断が必要。

- 不要確認後にだけ個別削除候補として承認を得る。
- 最新2.5D review exeは全て保持。
- 提出用Build/解説書/当時snapshotは保持。
- old `.codex_tmp` Projectや旧visualizationの全体削除は、固有Source/Probe/画像の保存位置が照合できるまで進めない。

## HIGH RISK — 削除しない

Production / `.git` / 未commit / BGM原本 / 最新2.5D / 必須Archive / DESERT raw / 会話履歴 / 未保存Art・資料。今回の推奨削除容量には0 GiBとして扱う。

特に9.414 GiBのCodex sessionsは大きいが、会話と実行履歴を失う可能性がありキャッシュとは違う。認証やCodex runtimeの内部領域も手動cleanupしない。

## 将来の実行前Gate

1. 最新Himo Source/Scene/Shader/画像/QAのGit保存は別の明示Taskで判断する。今回commitしない。
2. 全Unity終了、実行中Build終了、candidateの絶対Pathを再確認。
3. LOW Riskの小さいbatchをHumanが明示承認する。
4. 残すAssets/設定/外部入力、Source/QA/Archiveの存在を再確認。
5. 実行する場合は再取得・再importコストを説明し、除去した正確なPathと容量を記録する。

ここにcleanupコマンドは記載していない。包括的な `Logs` / `.codex_tmp` / `visualizations` 削除は推奨しない。

## 今回の終了状態

今回実行：サイズ・File count・時刻の読み取り、Git/remote/tagの読み取り、Source/BGM/Rawのhash照合、監査文書/Evidenceの新規作成のみ。

禁止操作は実行していない：delete/remove/clean/reset/checkout/stash/move/rename/zip/圧縮、Library/Build/.codex_tmp/visualizations削除、stage/commit/push。

保護照合の最終結果は `Evidence/ProtectionComparison.json`、全項目PASS。HimoのSource/Scene/BGM/既存Build/QA、DESERTのSource/Scene/設定/Git状態を維持した。この監査結果を報告してSTOPし、cleanupへ自動継続しない。
