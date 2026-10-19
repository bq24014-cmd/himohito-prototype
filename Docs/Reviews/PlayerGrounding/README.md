# ヒモヒト C22 C23 接地表示レビュー

身体縮小時の静止・待機で足が浮くC22/C23の表示修正は、2026年10月19日にHumanが正式採用しました。採用用差分だけをProductionへ統合し、mainへpush済みです。元の隔離Proofと比較記録は保持しています。

- 固定入口：[LATEST.json](LATEST.json)
- 統合記録：[Productionの報告と検証結果](https://github.com/bq24014-cmd/himohito-prototype/blob/2a0a2b5f5d5478f5ee2c6be5553121683757ebf7/Docs/QA/PlayerGroundingProductionIntegration_20261019/Report.md)
- 統合commit：[2a0a2b5f5d5478f5ee2c6be5553121683757ebf7](https://github.com/bq24014-cmd/himohito-prototype/commit/2a0a2b5f5d5478f5ee2c6be5553121683757ebf7)
- [今回のHuman採用判断と確認範囲](Integrations/20261019.json)
- 最新報告：[C22 C23の原因と修正版](runs/20261019T163611_JST/Report.md)
- [平地の通常速度動画](runs/20261019T163611_JST/Videos/flat_7_right_BeforeAfter_50fps_silent.mp4)
- [橋の通常速度動画](runs/20261019T163611_JST/Videos/bridge_7_right_BeforeAfter_50fps_silent.mp4)
- [連続コマ](runs/20261019T163611_JST/Frames/)
- [手動起動](runs/20261019T163611_JST/Launch.md)

画像はリポジトリ上の通常JPEGです。動画・画像は自動入力/APIの記録で、Human実キー試遊ではありません。ZIP添付を前提にしません。

過去runのHumanReview.jsonは提出時点の未承認記録として保存し、最新の正式採用判断はLATEST.jsonのhuman_results_pathで分けています。Production移植後の実キー再確認は未実施で、本人へ追加の再試遊は要求していません。

このレビュー資料直下の.gitattributesで、画像・動画をLFSポインタではなく通常Git blobとして保存します。ゲーム側やグローバルのGit設定は変更していません。patchの空行コンテキストだけは文書としての空白検査から除外しています。

このbranchはreview/player-grounding、資料はDocs/Reviews/PlayerGrounding/配下だけです。A2の壁・腕・R修正、汽車、Q案内、Tabのレビューは別のまま保持しています。過去のrunは上書きせず、以後もLATEST.jsonから最新報告を参照する運用です。
