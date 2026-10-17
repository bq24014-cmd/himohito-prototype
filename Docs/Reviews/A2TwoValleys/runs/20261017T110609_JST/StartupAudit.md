# 起動経路の監査

今回の手動Buildだけが対象。旧候補・自動撮影Buildの起動処理は変更していない。

| 起動箇所 | 元の処理 | 手動版 |
|---|---|---|
| PlayerPresentationProof.Bootstrap (BeforeSceneLoad) | 30fpsオフライン時計、SceneロードでPresentationCapture生成 | 時計0、ManualPresentationSessionだけ生成。既定候補2 |
| ManualPresentationSession.Start | 新規分離 | A2 Ready後、表示用コンポーネントを重複なしで追加。以降は状態JSONの読取のみ |
| A2Fixture.Register／Loaded | MainStageでA2Fixture追加 | 維持。地形と入口24／6／0、既存CP・Camera初期化に必要 |
| A2Fixture.Start末尾 | .8秒後撮影、引数によるQA／smoke終了 | Ready直後に手動版はyield break。自動登録・撮影・終了に到達しない |
| A2Fixture.Update | 出口観測＋撮影、定期telemetry | 観測維持、撮影だけ手動版で抑止。既存OnGUIの全文は同一 |
| PresentationCapture.Awake／Start | 入力driver追加、API接続／解除・移動・配置・ポーズ・終了 | 手動ガード、生成経路もない。実行中0インスタンス |
| A2ScriptedQA.Start | 入力driver生成、攻略・診断配置・自動終了 | 手動ガード。Fixtureから生成されず、実行中0 |
| A2InputDriver.FixedUpdate | PlayerMover入力フィールド上書き | 手動ガード、実行中0。元のPlayerMover.UpdateのA/Dを上書きしない |

既存ゲームのRuntimeInitializeOnLoadも確認した。MainStageFloorCollisionSetup、MainStageVisuals、採用Visual登録、BGM、音設定、StageCatalog、StageStartTransition、RopeResourceGauge、Tutorial用Visual登録は元の処理を保持。これらを新しい自動攻略driverへ結び付ける変更はない。Sceneのserialized componentも変更なし。

Editor側A2ProofEditor.Buildは既存のMainStage＋Tutorial、Windows64 Development Build手順を再利用した。EditorのScene SyncやBuild処理をProductionで実行していない。新しい隔離Sceneも元コピーからbyte同一を照合した。

## 表示と入力の依存

PlayerMotionPresentation.LateUpdateの候補2計算はそのまま。既存の `mover.MovementInput`、Rigidbodyの速度、Ropeの接続状態を読み、SwingPresentationStateへ渡す。QAドライバーへの参照はない。表示初期化が以前Capture内にあった点だけを分離した。

PlayerMover、RopeController、RopePlatformBuilder、MainStageRespawnOnFallの既存UpdateにA/D・Space・矢印・W/S・E・Q・F・R・Escの入力経路が残っていることを確認。主要5コード・InputManager・Physics設定のhashを保持した。ただし、コード上の受付経路はOSのキー反応確認とは別。

## 例外的に残す入口処理

A2 Bの地形作成、元のIntro解除、入口初期位置、残量・選択長・CP設定、Camera ResetFramingは元A2の入口処理を保持する。これらまで除去すると別ステージ／別初期状態になる。手動版の起動後に行う自動攻略・診断テレポートではない。

R時に元のS5オブジェクトが戻るのを隔離対象だけ非表示へ戻す既存LateUpdateもそのまま。R本体の実装は変更しない。今回RのOSキー試験をしたという意味ではない。
