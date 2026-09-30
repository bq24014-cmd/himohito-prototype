# Reproduction plan

Unity 6000.3.21f1。元Assets/Packages/ProjectSettingsを新しい隔離Unity projectへコピー。既存Libraryや別診断projectの改変sourceは流用しない。

実行時にPNG保存専用imageconversion moduleを隔離Packagesへ追加。元PackagesはSHA一致で不変。診断専用company/product名で本番PlayerPrefsを分離。

## 計測

隔離コピーにlogging-only instrumentationを追加。Production sourceは不変。各Recordはprivateリスト/保存配列をread-only reflectionで読む。CapturePlatformStatesを計測のために追加呼び出ししない。

各時点でsection、生成橋のStart/End/長さ、removedHooks、中央activeSelf/Hierarchy、rope残量/選択、保存配列、bindingのrelease/wrap数、frame/time/player座標をJSONLへ記録。

## Tutorial

通常のタイトル開始ハンドラーを実行、既存intro skip相当を実行。既存checkpoint床2→3→4へ位置移動して実際に着地（前半パズル操作は省略）。長さ6でE/Q相当の実メソッドを呼び、最初の橋上をPlayerMoverのD相当入力で歩く。2本目をE/Q、中央照準を設定しF相当ハンドラーで統合。checkpointのauthored配置をRuntime列挙し、後続checkpointがない場合R3を記録。保存を捏造しない。R相当で入口snapshotに戻る結果は参考観測。

## MainStage

Scene通常起動、intro skip相当。既存checkpoint9床へ位置移動して着地（区間1〜7のパズル操作は省略）。長さ6の橋2本を同じE/Q経路で生成しF相当で統合。以後位置移動せずPlayerMoverのD相当入力で統合橋を歩き、既存checkpoint10のCollisionStay/着地ゲートでsnapshotを保存。R相当のRestartFromCheckpointを呼ぶ。

この方法は制御されたRuntime再現であり、全区間をキー入力で手動通過した証拠ではない。API呼び出しのE/Q/F/R、reflectionによる照準/移動入力注入、開始位置の準備を開示する。物理モデル、snapshot構造、Hook状態処理、Scene配置は変更しない。

## 証拠

Camera.RenderのPNG（プレイ空間のみ、IMGUIなし）、Unity log、state.jsonl、CSV、SHA保護結果。Cameraの一時的な診断構図は撮影後に戻す。gameplay camera設定やSceneファイルには保存しない。

回帰テストや修正案の実装は行わない。
