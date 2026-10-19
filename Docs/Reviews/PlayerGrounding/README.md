# ヒモヒト C22 C23 接地表示レビュー

身体縮小時の静止・待機で足が浮く問題を、Production外の隔離Proofで修正・比較しています。Humanの正式採用は未承認、Production統合は行っていません。

- 固定入口：[LATEST.json](LATEST.json)
- 最新報告：[C22 C23の原因と修正版](runs/20261019T163611_JST/Report.md)
- [平地の通常速度動画](runs/20261019T163611_JST/Videos/flat_7_right_BeforeAfter_50fps_silent.mp4)
- [橋の通常速度動画](runs/20261019T163611_JST/Videos/bridge_7_right_BeforeAfter_50fps_silent.mp4)
- [連続コマ](runs/20261019T163611_JST/Frames/)
- [手動起動](runs/20261019T163611_JST/Launch.md)

画像はリポジトリ上の通常JPEGです。動画・画像は自動入力/APIの記録で、Human実キー試遊ではありません。ZIP添付を前提にしません。

このレビュー資料直下の.gitattributesで、画像・動画をLFSポインタではなく通常Git blobとして保存します。ゲーム側やグローバルのGit設定は変更していません。patchの空行コンテキストだけは文書としての空白検査から除外しています。

このbranchはreview/player-grounding、資料はDocs/Reviews/PlayerGrounding/配下だけです。A2の壁・腕・R修正、汽車、Q案内、Tabのレビューは別のまま保持しています。過去のrunは上書きせず、以後もLATEST.jsonから最新報告を参照する運用です。
