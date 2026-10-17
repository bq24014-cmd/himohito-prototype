# 起動経路だけの差分

変更はProduction外の新しい `PlayerSwingManualBuild_20261017/UnityProof` に限定。[正確なdiff・hash・新規コード](CodeDiff.json)。本体コードファイルをレビューbranchへ配置したものではない。

- `Assets/PlayerPresentation/PlayerMotionPresentation.cs`：先頭の起動クラスだけ変更。既定候補2、手動専用、capture時計0、Captureでなく表示bootstrapを登録。**PlayerMotionPresentationクラスの演出計算は同一**。
- `Assets/PlayerPresentation/PresentationCapture.cs`：手動版ではAwake／StartからQAを開始しないガード。
- `Assets/A2Proof/A2Fixture.cs`：Ready後の自動撮影・QA起動を遮断、出口の自動撮影を抑止、手動ログ先を分離。入力driverの手動ガード。入口設定・地形・説明文・R処理は維持。
- `Assets/A2Proof/A2ScriptedQA.cs`：手動版ではStart冒頭から戻る。
- 新規 `Assets/PlayerPresentation/ManualPresentationSession.cs`＋`.meta`：入口準備後の表示登録と、入力を送らない読取記録。35秒記録完了後もゲームを継続する。

表示を起動するためだけに自動攻略を動かす必要をなくした。姿勢のために新入力方式を作るのではなく、元からある入力・速度をそのまま読む。

**変更なし**：主要5コード、全Scene、Packages、ProjectSettings、全既存Art／Shader、SwingPresentationState、色調、接続・解除・着地・影・音・粒子。Q文言も変更なし。候補1／2の旧コピー・旧Buildは保持。
