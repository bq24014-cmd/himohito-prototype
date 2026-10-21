# C05 危険物のデザインレビュー

[最新状態](LATEST.json)／[ハサミ切断音の報告](runs/20261022T004810_JST/Report.md)／[手動起動](runs/20261022T004810_JST/Launch.md)

本体・1.00倍下降0.95の配置・第2区間の画角はHuman承認済みです。今回追加したのは、前側ハサミ1個で実際の切断が成立したときだけ鳴る0.22秒の音です。音は未承認、Production未統合です。

[試聴用WAV](runs/20261022T004810_JST/Audio/HimoHitoScissorsSnip-v1.wav)／[実Player音声入り動画](runs/20261022T004810_JST/Video/After_PlayerMix.mp4)／[切断音なしの比較](runs/20261022T004810_JST/Video/Before_PlayerMix.mp4)

実切断1回→再生1回、非接触・E解除・開閉だけは0回、効果音ミュートは新音0回。音量50%の再生振幅比は約0.5です。300フレームの位置・速度・残量・Camera・ハサミ開閉は音追加前後で差0。自動入力／APIの記録であり、OS実キー・Humanの実聴評価ではありません。

## 履歴

- [画角下降案 254d53f](runs/20261021T234155_JST/Report.md)：本体・配置・画角をHuman承認。今回の音追加の基準として保持。Production未統合。

- [1.00倍・下降0.95案 fbee470](runs/20261021T231056_JST/Report.md)：今回の配置基準として保持。元画角では持ち手が画面外。長さ5／6の通過と7の接触記録を保持。

- [0.50倍案 866a8de](runs/20261021T220112_JST/Report.md)：Human不採用。大きさを変えず下げる指示。原本保持。

- [開閉と刃判定 300027a](runs/20261021T211000_JST/Report.md)：Human採用保留。長さ6接触の指摘。原本・比較資料を保持。

- [上向き静止版 0b4f31a](runs/20261021T203813_JST/Report.md)：本体と配置の採用方針。旧矩形判定の原本を保持。

- [床寝かせ案 e560ab4](runs/20261021T202109_JST/Report.md)：Human不採用。原本保持。
- [前回配置 32809c7](runs/20261021T193948_JST/Report.md)：Humanから「向きが不自然」の修正指示。原本・開閉動画を保持。
- [暖色本体 5c64b9e](runs/20261021T180204_JST/Report.md)：本体の形・色・厚みをHuman承認。今回の基準。
- [床置き緑案 25ba244](runs/20261021T164023_JST/Report.md)：Human不採用。原本保持。
- [旧赤吊り案 38a83d9](runs/20261021T160305_JST/Report.md)：Human不採用。原本保持。
- [三角形素材調整案 80a004](runs/20261021T150807_JST/Report.md)：Human採用保留。


実行は既存非Development Unity Player＋独立managed overlay＋承認PNG／新WAVの実読込。新規Unity Buildではありません。録音は実Player内部のAudioRenderer最終ミックスで、音の後付けやOSスピーカー録音ではありません。診断API・自動撮影とOS実キーを区別し、Human実聴と操作感は未確認です。

Production `394b4a74e366678a9e0542d895b9a26f86993c19`、既存差分・過去Proofを保持。レビュー資料だけをこのbranchに共有します。C11/C18はHOLD。今回の音候補はHuman Review待ちです。
