# C01 C02 C03 メニュー整理のProduction統合

2026年10月21日。Humanが正式採用したメニュー表示だけを統合した。C11・C18はHOLDを継続し、C05など他の改修は行っていない。

## 承認元と変更範囲

- 基準Production：`3b6ac0dfe6a1035f8eed33d2efe720be51c481b3`。開始時はmain、stageは空、origin/mainと一致。
- 承認元：[`3c37d9ba60ead7232be93206853e1578cea12d9f`](https://github.com/bq24014-cmd/himohito-prototype/commit/3c37d9ba60ead7232be93206853e1578cea12d9f)。同commitの`Docs/Reviews/C01C03Menu/runs/20261021T134500_JST/Diff/ProposedStageSelectionView.patch`を照合した。
- 実装変更は`Assets/Scripts/StageSelectionView.cs`だけ。中央の「Enter はじめる」を主ボタンとし、Tab／Escを補助ボタンへ整理。島の補助表記を日本語に統一し、通常時の長い紹介文・残量説明・制作情報を画面から外し、余白を整えた。
- 追加予定の開始不可説明と、読み込み先などのエラー表示は残した。制作情報の元の記載先、StageCatalog.json、島の素材・配置・縦横比、フォント、選択演出、クリア旗、開始時の布演出、Tab説明画面、入力処理は変更していない。
- 統合後のソースは承認候補とバイト単位で一致。SHA-256：`e64f145cc8402ae6af80d1ab284a793eb931682688078df5fe6c3127be80c76b`。

## 回帰確認

Unity 6000.3.21f1の参照アセンブリでProductionの119ソースのみをコンパイルし、エラー0。既存のCS0649警告4件は残る。別途、同じProductionソースと外部QA fixtureをコンパイルして、既存の非Development Windows Playerへmanaged overlayとして読み込んだ。Productionには診断fixture・自動入力・撮影・終了処理を追加していない。

| 確認 | 結果 |
|---|---|
| 1600×900／1280×720の3島、文字、余白、3ボタン | PASS。承認候補と3島すべての実Player静止画像が完全一致。ボタン表示とGUI.ButtonのRectは共通、文字の必要高さは枠内に収まる |
| 追加予定の説明・開始拒否、エラー表示 | PASS。追加予定拒否とエラー表示の画像も承認候補と完全一致。エラーは既存ReportStageStartError APIで表示し、catalogやScene参照は書き換えていない |
| 選択、Tab説明の開閉、背後操作禁止、閉じた同フレームの終了防止 | 既存API経路でPASS。Tab説明の画像も完全一致。OSキーの押下ではない |
| Tutorial／MainStage開始、布演出経路、メニュー復帰、通常の終了経路 | 既存API経路でPASS。開始残量99／50を維持。紹介ツアーは既存RestoreState APIで終了し、ポーズ確認からタイトルへ復帰。全編試遊ではない |
| 実行時のStageCatalog、クリア記録、音量 | catalog全データが現Productionと一致し、記録状態も承認候補と一致。クリア記録・音量は不変 |

2解像度とも23項目PASS、実行エラー0。選択中の島・毛糸枠の代表姿勢とボタン配置も承認候補と一致した。開始布と復帰後の一部のコマは実時間アニメーションの撮影時刻が異なるため、同位相のピクセル一致とは扱わない。StageStartTransition／StageStartCurtainView等のソースは不変で、開始遷移とその完了を確認した。

## 保護と実行上の制限

保護対象262ファイルのうち、変更はStageSelectionView.csだけ。他261ファイルと、比較元・過去Proofの主要13ファイルのハッシュは不変。既存のMainStage.unity、README、学習ログ、ExeA／ExeBログの5件のdirty差分を維持した。StageSelectionView.cs.metaも変更なし。今回のcommit対象は実装1本と本記録だけである。無関係な既存Scene／ログの末尾空白は修正せず、今回の対象差分の`git diff --check`を確認した。

この検証は新規Unity Build成功やEditor Play確認ではない。既存Playerのserialized Scene／Resourcesを利用しており、dirtyなProduction Sceneを再Buildしていない。OS左右／A・D／Enter／Tab／Escと物理クリックの統合後確認は未確認。以前の操作ツールlist_appsタイムアウトは再試行していない。旗が立った追加撮影、Intro全編、複数ページは未実施。Humanの採用判断は完了しており、再承認待ちには戻さない。

## 共有先

[C01 C02 C03 固定入口](https://github.com/bq24014-cmd/himohito-prototype/blob/codex/review-c01-c03-menu/Docs/Reviews/C01C03Menu/LATEST.json)に、統合commit、実画面比較、API確認JSON、未確認事項を記録する。過去候補の画像・動画・手動ランチャーは保持する。
