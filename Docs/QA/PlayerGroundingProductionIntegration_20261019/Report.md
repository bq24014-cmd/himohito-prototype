# C22 C23 接地表示のProduction統合

2026年10月19日のHuman正式承認に基づき、身体縮小後の接地表示だけを本体へ統合した。採用元は [PlayerGroundingProofのレビューcommit 54fbc8b](https://github.com/bq24014-cmd/himohito-prototype/blob/54fbc8b2b7b809292cb50a2272fd3e5087cdb849/Docs/Reviews/PlayerGrounding/runs/20261019T163611_JST/Report.md)。本人の採用判断は完了しており、再承認待ちではない。

## 統合範囲

変更した実装は `Assets/Scripts/RopeBodyVisual.cs` のみ。[採用用差分](https://github.com/bq24014-cmd/himohito-prototype/blob/54fbc8b2b7b809292cb50a2272fd3e5087cdb849/Docs/Reviews/PlayerGrounding/runs/20261019T163611_JST/Diff/ProposedRopeBodyVisual.patch)の適用結果と、改行コードを除いて完全一致する。比較切替を含むIsolated版ではない。

接地中の静止・照準・減速・関連ポーズの足元基準を、既存の歩行基準にそろえた。中央pivotの立ち姿は、身体サイズに応じた足裏位置から表示位置を求める。身体の縮小、足運び、体重移動、橋のしなりを残し、空中を床へ吸着させない。首と頭を分ける既存ポーズも同じ基準を使う。

物理Root、Collider、Joint、移動、ジャンプ、振り子、接続点、Camera、橋生成・統合、残量、復帰処理、Sceneは変更していない。既存metaも同一。診断切替・GroundingReview・撮影支援・Proof初期状態・Packages・製品名は取り込んでいない。A2、汽車、腕、BodyPump、Q、Tab、C08は対象外。

## Productionで今回確認したこと

Unity 6000.3.21f1の既存Libraryを使い、実際の本体Sourceを再コンパイルした。コンパイルエラー0。編集Sceneを空のままPlayへ入り、Unity APIで保存済みTutorialを実行時ロードした。ステージ選択の待機状態と、既存開始APIからゲーム実行へ進むことを確認した。実行中の接地確認は次のとおり。

| 診断で設定した残量 | 身体縮小率 | 接地 | 表示の足元基準 local Y |
| --- | --- | --- | --- |
| 50 / 50 | 1.0000 | true | -0.575 |
| 31 / 50 | 0.8810 | true | -0.575 |
| 18 / 50 | 0.7655 | true | -0.575 |
| 7 / 50 | 0.6500 | true | -0.575 |

最小サイズで左右の照準方向、待機への切替、ポーズのtimeScale 0と再開後の1、Rと共通の復帰API後の接地を確認した。R時は診断で最大容量を50にしたため、元の入口保存量99は既存ルールで50に制限されて復元された。復帰仕様を変更した結果ではない。

これはローカルのUnityデバッガー経由のAPI確認であり、OS実キーでも本人の追加試遊でもない。ProductionのAssetsへ診断Sourceを追加せず、Sceneも保存していない。外部診断DLLの依存解決は失敗したためPASSに数えず、既存Unity APIを直接使う方式で上記を確認した。起動時のライセンス接続ログはゲームのエラーと区別し、Tutorialロード以降のログにゲームのError／Exceptionはなかった。終了APIの応答待ちは今回起動した検証プロセスだけを特定して終了した。

## 採用済みProofから再利用した回帰確認

本体の118個の非対象Runtime Sourceは検証済みProofとハッシュ一致。対象Sourceも、診断切替を取り除いた採用版と一致するため、同じQAを量産せず次の結果を再利用した。

- Windows比較元45/45、修正版45/45、双方Runtime Error 0。4サイズ、平地と生成橋、左右向き、歩行→減速→停止→待機→照準→再歩行。
- ジャンプ・着地、E接続／解除と速度保持・返却、Qで2本の橋生成、F統合、R復帰、ポーズ／再開は自動入力・APIで確認済み。
- 125比較群で位置・速度・入力・残量・選択長・Camera・接地／接続・橋支持・Collider底の最大差0。
- 頭頂と描画ヒモ始点の誤差0、分割ポーズの首17サンプルの誤差0。橋の既存表示変形は約-0.14185〜0を維持。

根拠は採用元の [QA資料](https://github.com/bq24014-cmd/himohito-prototype/tree/54fbc8b2b7b809292cb50a2272fd3e5087cdb849/Docs/Reviews/PlayerGrounding/runs/20261019T163611_JST/QA)と、今回の [Checks.json](Evidence/Checks.json)。今回Productionでこれら45項目すべてを再実行したという意味ではない。

## 保護と未確認事項

MainStage Scene、README、学習ログ、既存QAログ2件の未保存差分、主要ゲームコード、Tutorial Scene、meta、Packages、ProjectSettingsの保護ハッシュは一致した。未承認Proof・既存Build・outputは追加・削除・上書きしていない。既存未追跡の診断Sourceも今回のcommitから除外する。

OS操作ツールは画面一覧取得でタイムアウトし、実キー再確認は未実施。本人の採用済みVisual評価と、今回の技術確認を区別する。音、C08の緑フック端の専用再現、新しいProduction Windows Buildは未確認。空き容量は開始約3.24 GiBであり、追加Build・フルコピー・Library複製を避けた。既存の比較用非Development Buildは保持している。

本件の完了に本人の追加操作や再試遊は必要ない。既存レビューの [固定入口](https://github.com/bq24014-cmd/himohito-prototype/blob/review/player-grounding/Docs/Reviews/PlayerGrounding/LATEST.json)から、統合先commitを追跡する。
