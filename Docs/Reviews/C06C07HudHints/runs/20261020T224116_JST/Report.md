# C06 C07 正式採用とProduction統合

Humanの「採用で」により、布タグ候補 `89ad6617837371afbb43c85ff83d84f1fb654a3a` を正式採用し、Productionへ統合しました。

- Production main：`0130f68488fcbde83c2f83dc72acd7adeaba4dee`
- commit message：`Integrate approved C06 C07 cloth HUD hints`
- push先：`bq24014-cmd/himohito-prototype` の `main`
- push後にfetchし、local HEADとorigin/mainの一致を確認。
- [Productionの統合記録](https://github.com/bq24014-cmd/himohito-prototype/blob/0130f68488fcbde83c2f83dc72acd7adeaba4dee/Docs/QA/C06C07HudHintsProduction_20261020/Integration.md)

## 保存した範囲

`PrototypeHud.cs`、`MainStageHud.cs`、`StageOverlayControls.cs`、`HimoHitoUiParts.cs` と、新規の統合記録1本だけをcommitしました。左Tab行削除、左枠206→172px、右上の共通素材の布タグを採用候補どおりに反映。4本すべて承認済みソースとバイト一致しています。

文字・バーのサイズ、Tab説明の文面・配置・背景、入力・ポーズ・復帰は維持。診断コード・自動入力・撮影・終了処理・設定はProductionへ取り込んでいません。

## 統合したソースによる確認

| Scene | 解像度 | 布タグと左余白 | 接続中の収まり | API開閉・復帰 |
|---|---|---|---|---|
| Tutorial | 1600×900 | [PASS](Images/Tutorial-1600-Integrated.jpg) | PASS | PASS |
| Tutorial | 1280×720 | [PASS](Images/Tutorial-1280-Integrated.jpg) | [PASS](Images/Tutorial-1280-ConnectedHud.jpg) | PASS |
| MainStage | 1600×900 | [PASS](Images/MainStage-1600-Integrated.jpg) | PASS | PASS |
| MainStage | 1280×720 | [PASS](Images/MainStage-1280-Integrated.jpg) | [PASS](Images/MainStage-1280-ConnectedHud.jpg) | PASS |

4条件を一巡し、実行エラー0件。通常表示・接続中・ヘルプ・ポーズ・再開の各画像は採用Proofとピクセル一致でした。全記録時点のRoot・速度・残量・選択長・Camera・接続状態・TimeScaleも一致。[実測JSONとソースハッシュ](QA/IntegrationVerification.json)

Tab説明画面：[Tutorial](Images/Tutorial-1280-Help.jpg)／[MainStage](Images/MainStage-1280-Help.jpg)。候補時の比較・確認結果も再利用しており、無関係な全区間QAを繰り返していません。

## 実行方式と保護

Productionの119ソースだけのコンパイルと、同じソースへProduction外のQAfixtureを加えたコンパイルが成功しました。Unity 6.3アセンブリ／Roslynを使用し、既存CS0649警告4件ずつ、新規エラー0件です。

実画面・API確認は **既存Unity 6000.3.21f1非Development Player＋managed overlay**。新規Unity Buildではありません。凍結PlayerのScene／Artを再利用し、現在のdirty Sceneを新しくBuildしたものではありません。

C08・C14・C20・C22/C23を含む対象外243ファイルと過去Proofをハッシュで保護。MainStage Scene、README、学習ログ、既存QAログ2件の5差分は未commitのまま保持し、stage残りはありません。無関係な未追跡物、過去資料、不採用候補を削除・破棄していません。

未確認は統合後のOS実キー、Unity Editor上で現在のdirty SceneをPlayする確認、新規Windows Build、全区間通し再プレイです。API確認を実キー成功とは扱いません。既存配布Buildも更新していません。本人の採用判断は完了しており、再承認待ちへ戻しません。

今回のC06・C07統合は完了です。他の改修には着手していません。[採用候補・比較元の手動起動方法](../20261020T222355_JST/Launch.md)と過去runはそのまま保持しています。
