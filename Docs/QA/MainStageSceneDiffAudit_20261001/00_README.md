# MainStage Scene Diff Audit — 2026-10-01

結論：**CASE S4**。未CommitのMainStageは提出時snapshotと完全一致する。一方、差分対象の起動後状態は最新HEADのRuntimeコードで再現され、現在の遊べる状態を成立させるためのScene差分ではない。

基準：`main` / HEAD / origin/main = `4919233cd1d0815afb438bd35a7336492ec09548`。

今回行ったのは読取監査、新規QA文書・Evidenceの作成、新規隔離コピーでの対象箇所A/B確認だけ。Production Scene/Source、既存QA、README、LEARNING_LOG、output、過去Buildは変更していない。stage / commit / push / revert等も行っていない。

## 資料

- `01_GitState.md`：Gitと原本保護。
- `02_SerializedDiff.md`：全570行のproperty単位分類表。
- `03_AuthorityMap.md`：分類表と一対一のauthority・Source位置。
- `04_SubmissionSnapshotComparison.md`：提出時実ファイルとmanifestの照合。
- `05_RuntimeAB.md`：対象14ノードのRuntime比較。
- `06_VisualAB.md`：同一カメラ・解像度の3組の局所画像比較。
- `07_WhitespaceAnalysis.md`：46箇所のformat差分と49警告の内訳。
- `08_FinalDecision.md`：最終判断・次に行う1作業。

## Evidenceの読み方

HEAD/WorkingTree/SubmissionのSceneコピーは比較証拠であり、原本の書換えではない。`scene.diff`は全diff、`classified_property_diff.json`は機械可読な完全表。追加Unity documentは既存property変更と数え方を区別している。

`RuntimeA` / `RuntimeB`はMainStageを**Play modeでのみ**ロードし、通常のOnEnable・sceneLoaded setup完了から30フレーム後に対象を採取した。Edit modeでMainStageを開いてSceneSyncに保存させる方法は使っていない。追加QAコードは隔離コピーのEditorフォルダに限り、Production Assetsには入れていない。

これは全ゲーム回帰、新たなHuman Art PASS、Human操作の再評価ではない。既存のHuman承認は`HumanPlaytestFixes_20260930/08_HumanRetest_20261001.md`のユーザー報告を維持する。
