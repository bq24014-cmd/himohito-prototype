# Checkpoint Merged Bridge Fix — 2026-09-30

判定: **Checkpoint Restore Fix = PASS**（指定A〜Fの制御Runtime回帰）。commit/pushせずSTOP。

対象はHimoHitoPrototypeのみ。DESERT LOOPにはアクセスしていない。

基準: main / HEAD・origin/mainともf0b0709b446a7f4372f2223aad38acaebaf8c6af。Unity 6000.3.21f1。

前Taskで確定した「MainStage S9統合→checkpoint10→Rで中央Hookが復活」を、解除Hookのsnapshotと再適用で最小修正。Production Sourceは3ファイルのみ変更。Scene、Art、物理、残量計算、区間設定は不変。

- 01_FixDesign.md: Hook寿命確認、capture/restore設計、検証条件
- 02_CodeDiff.md: Source差分と診断用変更の区別
- 03_MainStageRegression.md: A〜Eの結果
- 04_TutorialSmoke.md: Fの通常進行結果
- 05_FinalDecision.md: 最終判定、保護結果、ログ上の既存問題、対象外
- Evidence/: SHA/Git証跡、Source差分、各ケースJSONL/assertions/result/PNG/Unity log、診断harness

Production Sceneへの診断code追加なし。隔離コピーは.codex_tmp/CheckpointMergedBridgeFix_20260930/DiagnosticProject。前TaskのQAおよび隔離コピーは保持。

## 再実行

Evidence/Harness/prepare.ps1はfresh DiagnosticProjectを作成する。既存コピーがある場合は意図的に停止するため、上書きせず新しい診断保存先を準備して実行すること。

Unityをbatchmodeで隔離projectに対し起動し、-executeMethod CheckpointMergedBridgeFix.Run、-fixCase Merged/Unmerged/Normal/Tutorial、-fixEvidence ケース固有の出力先、-logFile ケース固有のログを渡す。driverがPASS/FAILを保存しEditorを終了する。-quitは付けない（PlayModeテストの完了前終了を避ける）。

同一session内のcheckpoint復元を検証したもので、永続save、Scene跨ぎ、配布build、手動キー入力の確認まで保証する資料ではない。
