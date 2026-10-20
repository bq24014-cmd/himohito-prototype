# C06 C07 布HUD案内のProduction統合

2026年10月20日、Humanの「採用で」という正式承認により、布タグ修正版をProductionへ限定統合した。承認元はレビューcommit `89ad6617837371afbb43c85ff83d84f1fb654a3a`、開始時のmainは `d88160da59fd5902222ef68a76a8b74882fafe98`。

## 統合範囲

- `PrototypeHud.cs`／`MainStageHud.cs`：左HUDの「Tab 操作説明」を削除。
- `HimoHitoUiParts.cs`：左HUDのパネル・描画領域を206→172pxへ短縮。既存素材と角を保つ布タグ描画を追加。
- `StageOverlayControls.cs`：右上を「Tab：操作方法　Esc：ポーズ」の布タグへ変更。20pxの既存フォント、文字色と余白を採用候補どおりに保持。

4本すべて採用候補ソースとバイト一致。既存文字・バー・接続状態のサイズ、Tab説明の文面・配置・背景、入力・ポーズ・復帰は変更していない。診断fixture、比較切替、自動入力、撮影、終了処理、設定変更は取り込んでいない。

## 統合ソースによる確認

| Scene | 解像度 | 結果 |
|---|---|---|
| Tutorial | 1600×900 | 布タグ、下余白、接続中の文字、ヘルプ／ポーズ復帰 PASS |
| Tutorial | 1280×720 | 同上 PASS |
| MainStage | 1600×900 | 同上 PASS |
| MainStage | 1280×720 | 同上 PASS |

実行エラー0件。各条件の通常・ヘルプ・ポーズ・再開・接続中の5画面は採用Proofとピクセル一致。全記録時点のRoot位置・速度・残量・選択長・Camera・接続状態・TimeScaleも一致した。旧Proofの確認結果を再利用し、統合後は各条件を一巡だけ確認した。

Productionの119ソースだけをUnity 6.3アセンブリ／Roslynでコンパイル成功。別途、同じProductionソースへ外部QAfixtureを加えた **既存Unity 6000.3.21f1非Development Player＋managed overlay** で表示とAPI経路を確認した。新規Unity Build成功ではなく、QAfixtureはProduction外に保持している。既存CS0649警告4件ずつ、新規コンパイルエラー0件。

## 保護と未確認

C08・C14・C20・C22/C23を含む対象外243ファイル、既存tracked差分5件、承認済み・不採用ProofのソースとDLLを保持。既存metaも不変。対象差分とstage内容の `git diff --check` はPASS。既存Scene／旧ログにある無関係な空白差分には触れていない。

統合後のOS実キー、Unity Editorでの現在のdirty SceneのPlay、新規Windows Build、全区間通し再プレイは未実施。API確認をOSキー操作やHumanの再試遊とは扱わない。既存配布Buildは更新していない。

[採用元の比較資料](https://github.com/bq24014-cmd/himohito-prototype/blob/89ad6617837371afbb43c85ff83d84f1fb654a3a/Docs/Reviews/C06C07HudHints/runs/20261020T222355_JST/Report.md)／[固定入口と統合確認証拠](https://github.com/bq24014-cmd/himohito-prototype/blob/codex/review-c06-c07-hud-hints/Docs/Reviews/C06C07HudHints/LATEST.json)
