# Minimal Fixes

## Source

1. TutorialSignInscription.cs：Operation KeyとCreamの床/矢印線だけをDiagramInkへ。画像・フォント寸法・配置・色付きHook・毛糸・Animationはそのまま。
2. PlayerMover.cs：Update / FixedUpdate先頭のsimulation停止ガードと入力bufferの消去。Sceneごとの独立なClear処理を増やさず、既存のphysics停止契約を共有。
3. RopeController.cs：同じray内で、既存の対フック橋への候補をfallbackにし、有効な未生成先を優先。距離判定とCanAttachToHook判定の後なので到達制限を回避しない。既存Hookへの再接続、Fの中央選択も維持。
4. TutorialSectionGuide.cs：残量例の `99 → 93` だけを `99 → 92` へ。説明画面のstyleや他文言は変更なし。

## S2の仕様確認

RopePlatformBuilder.GetCurrentPlatformCostはActiveRopeLength、CanBuildもActiveRopeLengthを使用。RopeResource.TrySpendはcurrentLengthからamountを減算。選択長7でT3をQ生成すると99−7=92です。READMEの既存説明も99→92→80で一致していました。TutorialRegressionでT3長さ7生成後、T4チェックポイントからRを実行し、T3の橋・残量92・選択7が復元されることを確認します。

## 文書

READMEは今回変更した照準競合とphysics停止中入力契約の2文のみ追加。LEARNING_LOGには原因確認と証拠種別の学びを追記。今回QAの8資料とEvidenceを新規作成。

## 変更していないもの

MainStage.unityその他Scene、Art、Audio素材、音量、Hook collider、aimRotationSpeed、距離grace、ステージ地形、Difficulty、移動物理式、Joint、RopeResource、RopePlatformBuilder、既存Checkpoint Restore Source。既存output、過去Build/QA/.codex_tmpも保持。DESERT LOOPへアクセスなし。

新しい診断用Input seam、Audioカウンター、Editor entryは隔離コピーだけにあり、productionには含めません。commit / push / stageを行っていません。
