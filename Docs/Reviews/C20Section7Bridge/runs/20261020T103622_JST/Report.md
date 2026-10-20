# C20 深いたわみ候補の正式採用と統合

Human評価「完璧です」を受け、[b36092fの候補](https://github.com/bq24014-cmd/himohito-prototype/blob/b36092f60aab80974272ec1b5d8f7c03c544934b/Docs/Reviews/C20Section7Bridge/runs/20261020T101520_JST/Report.md)を正式採用した。旧22f案は採用していない。

Production commitは **`1ffac00416db372b53c6df04273bf11557ac8a7d`**。`main`への通常push成功、GitHub remote HEADとlocal HEADの一致を確認した。

## 統合範囲と回帰

変更はRopePlatformBuilder.csの対象曲線51行と最小統合記録2件だけ。採用元ソースとSHA256が完全一致している。診断fixture、比較切替、自動入力、撮影・終了処理はProductionへ入れていない。C08・C22・C23、既存の未保存Scene・README・学習ログ・QAログ、過去Proofは維持した。

- S7の長さ4／5：既存E/Qで生成、登坂、中央・谷・急坂で停止再開、上岸到達、左岸へ帰還、再登坂、ジャンプ着地、R復帰を確認。長さ5の谷は4より約0.42738深い。
- C08：支持橋面への足元表示補正と歩行時のしなりを実画面・状態記録で確認。空中・上岸では橋補正が解除される。
- 対照T4：既存Qで6+6の2橋→Fで12の1橋、残量50→44→38を維持。R後50／選択長6／橋0／未接続。曲線は従来どおり。
- Production Runtime 119本のみのUnity用コンパイルはエラー0、既存警告4件。隔離Player実行はS7・T4ともError／Exception 0件。

[Production統合記録](https://github.com/bq24014-cmd/himohito-prototype/blob/1ffac00416db372b53c6df04273bf11557ac8a7d/Docs/QA/C20BridgeCurveProduction_20261020/00_Integration.md)と[統合後の検証値](https://github.com/bq24014-cmd/himohito-prototype/blob/1ffac00416db372b53c6df04273bf11557ac8a7d/Docs/QA/C20BridgeCurveProduction_20261020/Evidence/Verification.json)を参照。

## 既存の比較資料と制限

[通常画角の長さ4／5比較](../20261020T101520_JST/Figures/03_SameCameraGenerated.jpg)、[曲線重ね図](../20261020T101520_JST/Figures/01_OverlayMeasuredCurves.jpg)、[長さ5の通常速度動画](../20261020T101520_JST/Video/L5_ClimbStopResume_1x.mp4)、[隔離比較ランチャーの説明](../20261020T101520_JST/Launch.md)はそのまま保持した。画像・動画は承認元の隔離候補の記録であり、統合後の新規Build撮影とは区別する。

今回の実行方式は、統合済みと同一のProduction Runtimeソースに隔離診断fixtureを加えた **既存非Development Playerへのmanaged overlay**。新規Unity Build、Editor Play、OS実キー、統合後Human再試遊、音、全区間通しは未確認。Human正式採用は承認済み候補について記録し、API・自動入力を本人の実操作へ置き換えない。

追加の本人操作や再承認は今回の統合完了条件ではない。他課題・新機能には着手せず、C20の統合完了で停止する。
