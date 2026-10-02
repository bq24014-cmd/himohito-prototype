# Tutorial 2.5D Intro Fix — 2026-10-02

対象は前Task PI2の「イントロ中だけ新3層が隠れ、旧背景が見える」1件。Production変更はTutorialCraftRoomLayers.Refreshの表示条件のみ。

基準：main / HEAD・origin/main `702a4bc044b0eb35bedb552753ca8495599bc1ee`。Unity `6000.3.21f1`。既存Working Treeの2.5D統合・継ぎ目修正・外部BGMを含む検証。clean HEADだけのBuildではない。

資料：

- 01_RootCause.md：現行Sourceの原因と表示ライフサイクル。
- 02_Fix.md：許可された条件変更と保護する契約。
- 03_IntroTimeline.md：A〜Fの表示状態とCamera。
- 04_Regression.md：既存127項目・継ぎ目・Performance・保護。
- 05_HumanReviewPacket.md：比較画像・GIF・新規Buildの確認手順。
- 06_FinalDecision.md：技術判定とHuman承認待ち。
- Evidence/：Before Source、条件diff、隔離専用ハーネス、実測CSV・結果。
- Screenshots/：実Unity world-camera PNGと比較・GIF。素材再生成なし。

診断SourceはDocsと隔離コピーにのみ保存。Production Assetsへ追加しない。既存QAは改変しない。過去Buildを上書きしない。BGM実体、Build、.codex_tmpはGitへ追加しない。

自動診断のAPI/cached-input操作はHuman実キーボードレビューとは別。2026-10-02、今回の通常Buildについて「問題なし・採用してよい」というHuman承認を受領。Intro修正はIF1。commit / pushは禁止、承認結果を記録して停止する。
