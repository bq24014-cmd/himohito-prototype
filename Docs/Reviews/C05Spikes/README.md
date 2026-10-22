# C05 危険物のデザインレビュー

[最新状態](LATEST.json)／[今回の報告](runs/20261023T001912_JST/Report.md)／[手動起動](runs/20261023T001912_JST/Launch.md)

前側ハサミ1個の切断後コピーを、**長さ削除ではなく、端の曲がり→3本の細繊維→柔らかい消去**へ修正した隔離候補です。完成映像の正式採用は未承認、Human Review待ちです。Production統合はしていません。

[720p音入り通常速度比較](runs/20261023T001912_JST/Video/Before3cacaThenAfterSoft_1280x720_NormalSpeed.mp4)／[900p比較](runs/20261023T001912_JST/Video/Before3cacaThenAfterSoft_1600x900_NormalSpeed.mp4)／[720p同倍率連続コマ](runs/20261023T001912_JST/Images/SequenceSameScale_1280x720.jpg)／[採用音v4 WAV・無加工](runs/20261022T114924_JST/Audio/C05RopeScissors_v4.wav)

動画は旧3cacaを6秒、今回を6秒の順。実Playerの通常画角・音入り記録です。自然落下失敗後も残り、実測で約0.64秒に透明化、約0.66秒に片付きます。長さ5／6到達、7切断、音1・VFX1・旧毛羽0を保持。同一入力300フレームの位置・速度・残量・Camera差0。自動入力／APIであり、OS実キー・Human試遊ではありません。

**刃と表示ヒモ端の1.393823 world差は未解決。** 架空の糸や切断片、刃へずらしたほつれは追加していません。細い先端の背景への溶けやすさも残ります。数値上のPASSと見た目の自己評価・Human採用は区別します。

実行は既存非Development Unity6000.3.21f1 Player＋独立managed overlay。新規Unity Buildではありません。録音は実PlayerのAudioRenderer最終ミックスで、音の後付け・OSループバックではありません。本体・1.00倍下降0.95・S2画角・音v4と演出方向の承認を保持し、今回完成映像のHuman実聴・採否・操作感は未確認です。

## 履歴

- [前3caca・長さ削除型](runs/20261022T233837_JST/Report.md)：巻き取りに見えるという指摘を受けた比較基準。原本・動画・QA保持。VFX正式採用なし。

- [前1ccfbd0・連続性修正](runs/20261022T133717_JST/Report.md)：原本・動画・QAを保持。0.27秒の全体フェードを今回の比較基準に使用。VFX正式採用ではありません。

- [旧VFX 47f66eb](runs/20261022T114924_JST/Report.md)：原本・動画・QAを保持。落下失敗での早期消去は今回修正、切断位置の不一致は未解決。VFXの正式採用ではありません。

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

Production `394b4a74e366678a9e0542d895b9a26f86993c19`、既存差分・過去Proofを保持。C11／C18はHOLD。レビュー資料だけをこのbranchへ共有します。
