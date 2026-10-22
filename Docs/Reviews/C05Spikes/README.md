# C05 危険物のデザインレビュー

[最新状態](LATEST.json)／[音v4＋切断VFXの報告](runs/20261022T114924_JST/Report.md)／[手動起動](runs/20261022T114924_JST/Launch.md)

本体・1.00倍下降0.95の配置・第2区間の画角・音v4はHuman承認済みです。今回の新しい対象は、切断直前の表示ヒモを使う短い戻り・ほつれです。前側ハサミ1個に限定し、VFXはHuman Review待ち、Production未統合です。

[採用音v4 WAV](runs/20261022T114924_JST/Audio/C05RopeScissors_v4.wav)／[実Player音入り前後比較](runs/20261022T114924_JST/Video/Comparison_NormalSpeed.mp4)／[通常画角と拡大](runs/20261022T114924_JST/Images/ComparisonSheet.jpg)

実切断1回→音1回・VFX1回、旧接触毛羽との重複なし。長さ5／6は通過、新音・VFX0回。長さ7は従来どおり接触。各300フレームの位置・速度・残量・Camera・ハサミ開閉は前後差0。ミュートはPlay0・切断窓PCM0・VFX1。自動入力／APIの記録であり、OS実キー・HumanのVFX採用評価ではありません。

今回の経路はプレイヤーと刃の接触であり、ヒモ線と刃の交点はありません。ヒモを刃まで延ばさず最寄りの頭側端点でほつれを示します。刃とほつれの位置差、720pでの控えめな繊維はHuman Reviewで確認する制限です。

## 履歴

- [旧音v1 c696ae3](runs/20261022T004810_JST/Report.md)：原本・動画・QAを保持。音は別途承認されたv4へ更新。旧報告の採否は当時の記録です。

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


実行は既存非Development Unity Player＋独立managed overlay＋承認PNG／音v4の実読込。新規Unity Buildではありません。録音は実Player内部のAudioRenderer最終ミックスで、音の後付けやOSスピーカー録音ではありません。診断API・自動撮影とOS実キーを区別し、今回のミックスのHuman実聴・VFX採否・操作感は未確認です。

Production `394b4a74e366678a9e0542d895b9a26f86993c19`、既存差分・過去Proofを保持。レビュー資料だけをこのbranchに共有します。C11/C18はHOLD。今回の切断VFX候補はHuman Review待ちです。
