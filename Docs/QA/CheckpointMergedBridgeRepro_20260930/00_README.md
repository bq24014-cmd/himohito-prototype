# Checkpoint Restore — Merged Bridge / Removed Center Hook

2026-09-30 / REPRODUCTION ONLY / HimoHitoPrototypeのみ。

基準: main / f0b0709b446a7f4372f2223aad38acaebaf8c6af。開始時のfetch後、origin/mainも同一。

## 結果

MainStage: **CASE R1**。第9区間で統合→既存checkpoint10に物理着地→R相当の復元。長さ12の1本橋は同じ両端で復元、中央Hookはfalse/falseからtrue/trueへ誤復活。

Tutorial: **CASE R3 / NOT REPRODUCIBLE WITH AUTHORED TUTORIAL FLOW**。統合は成立するがその後のcheckpointは存在しない。

原因判断まで完了しSTOP。バグ修正、回帰テスト、commit/pushは未実施。保護対象464ファイルのSHAが全件一致し、隔離コピーの両Sceneも元SceneとSHA一致。

## 証拠の読み方

- `01_SourceAnalysis.md`: Production sourceの静的事実。Runtime結果ではない。
- `02_ReproductionPlan.md`: 診断条件と通常プレイとの差。
- `03_RuntimeEvidence.md`: 実測結果と制限。
- `04_StateTimeline.md`: 保存・復元の実行順と状態。
- `05_RootCauseDecision.md`: CASE分類、原因判断、文章のみの修正候補。
- `06_RegressionPlan.md`: 修正後の回帰候補。今回は未実行。
- `Evidence/sha_start.json`: 元Assets/Packages/ProjectSettings/outputの開始SHA。
- `Evidence/sha_end_comparison.json`: 終了照合。差異0。
- `Evidence/environment_and_git.json`: Unity/Scene/HEAD/Git状態。
- `Evidence/state.jsonl` / `state.json` / `state.csv`: 生計測と集約。
- `Evidence/DiagnosticHarness/`: 診断コード、loggingのみの差分。

診断コードはignoredの `.codex_tmp/CheckpointMergedBridgeRepro_20260930/` に隔離。Productionへの修正・Scene編集・commit・pushは行わない。DESERT LOOPは対象外。

Runtimeの準備として既存checkpoint床まで位置移動し、前半のパズルを省略する。通しの手動プレイ検証ではない。生成・統合後のcheckpoint成立は実際のCollisionStay/着地ゲートで確認する。既存checkpointを改変したり、統合後のsnapshotを診断側で作ったりしない。
