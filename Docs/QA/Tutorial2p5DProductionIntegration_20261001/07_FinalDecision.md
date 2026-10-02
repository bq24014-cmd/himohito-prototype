# Final Decision

**PI2 — 一部問題あり。プレイ中の技術回帰PASS、Humanでイントロの旧背景表示を確認。採用保留。**

## 根拠

- P1/R1/T1の背景、follow、構図、玩具・床接地処理を維持。PNGは承認済みT1とbyte一致。
- 既存HimoHitoCraftRoomBackgroundのEnsure/OnEnable経路へ統合。Editor bootstrap依存なし。
- MainStageは対象外。Gameplay/Scene/Physics/Camera基本挙動を変更しない。
- ゴール右端の透明pixel sampleとfloor flipをShaderだけで補正。比較画像で縦線の解消を確認。
- 最終Runtime05：127 checks PASS / failures0 / runtimeErrors0 /3物理Section境界 /35motion samples。
- Stage Selection、4区間操作、R復帰、橋/merge、Clear、メニュー復帰、旧背景非表示を確認。
- Baseline05/Production04 Windows Build成功。実exeでTutorial開始まで成功。
- StandaloneProductionAdoption：3層visible/legacyHidden必須assertがPASS、240実frames取得。
- 限定性能測定で明らかなFPS悪化なし。Build約21.78MiB増、有効3枚のEditor counter約45.49MiB。release VRAM差は未取得と明記。
- 保護対象3866ファイルは既存背景入口Source1件だけが許可変更。既存Scene/BGM/過去Build/QA/README/LEARNING_LOGを保持。stageなし、HEAD/origin/mainは基準のまま。

## Human Review

**ISSUE REPORTED / NOT APPROVED**。2026-10-02 JSTのProduction04 Human feedback：

> 最初のスタート地点に向かっていくカメラのときの画面が旧のものになってる

原因：TutorialCraftRoomLayers.Refreshの`!MainStagePreview.IsActive`が新背景を非表示にし、legacy rendererを戻すため。現行の試作/統合Planでは旧背景をintro previewへ残していたが、Humanの期待するProduction全体の見た目と一致しない。**Cameraの経路や速度の不具合ではない。背景の可視判定の問題。**

以前の自動QAは「Stage Selection/introは旧背景を維持」を期待値としていたためPASSだった。プレイ中127checksやゴールの継ぎ目修正PASSは維持するが、全体の採用PASSとはしない。

次の背景専用Taskで、イントロ中にも新3層を表示する最小変更を検討する。Camera基本挙動/Gameplayは変更せず、goal→start全行程の継ぎ目・floor/小物・表示切替を追加回帰し、新規Buildで再Human Reviewが必要。

過去T1の「自然で見やすい」は基準採用の根拠。今回の最終承認に流用しない。全4区間の今回Human最終評価もまだ確定していない。

新規Production04 Buildと比較画像を渡し、全4区間の見やすさ、ゴール右端の移動中の継ぎ目、体感の引っかかりを確認してもらう。

Humanで部分的な背景問題を確認したためPI2。Gameplay regressionやBuild失敗のPI3ではない。今回はHuman Reviewで停止する指定に従い、追加Source修正へ進まない。

## 停止

commit / push未実施。新規GameplayやMainStage背景への展開なし。Production IntegrationとQAの成果物を保持し、Human ReviewでSTOP。
