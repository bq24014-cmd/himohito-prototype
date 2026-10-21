# C05 危険物のデザインレビュー

[最新状態](LATEST.json)／[開閉と刃判定の結果](runs/20261021T211000_JST/Report.md)／[手動起動](runs/20261021T211000_JST/Launch.md)

暖色本体と「持ち手が下・刃先が上」はHumanの採用方針です。今回はその姿を保持し、本編手前1個の連続開閉と刃の判定をまとめた隔離候補です。Production統合はしていません。

[通常画角の開閉](runs/20261021T211000_JST/Video/01_Motion_API.mp4)／[実Triggerでの接触](runs/20261021T211000_JST/Video/02_NativeContact.mp4)／[空白通過](runs/20261021T211000_JST/Video/03_EmptyPassage_ThenFall.mp4)／[判定の重ね合わせ](runs/20261021T211000_JST/Images/01_Open_ColliderOverlay.jpg)

既存の解除・返却・接触繊維を保持し、対象1個の矩形を左右の刃に沿う判定へ変更しました。主試験111チェックPASS。補足では空白通過の最後に通常落下へ移行する1条件をFAILとして残し、刃接触との違いを報告しています。自動QAはHuman採用の代わりにしません。

## 履歴

- [上向き静止版 0b4f31a](runs/20261021T203813_JST/Report.md)：本体と配置の採用方針。旧矩形判定の原本を保持。

- [床寝かせ案 e560ab4](runs/20261021T202109_JST/Report.md)：Human不採用。原本保持。
- [前回配置 32809c7](runs/20261021T193948_JST/Report.md)：Humanから「向きが不自然」の修正指示。原本・開閉動画を保持。
- [暖色本体 5c64b9e](runs/20261021T180204_JST/Report.md)：本体の形・色・厚みをHuman承認。今回の基準。
- [床置き緑案 25ba244](runs/20261021T164023_JST/Report.md)：Human不採用。原本保持。
- [旧赤吊り案 38a83d9](runs/20261021T160305_JST/Report.md)：Human不採用。原本保持。
- [三角形素材調整案 80a004](runs/20261021T150807_JST/Report.md)：Human採用保留。


実行は既存非Development Unity Player＋独立managed overlay＋承認PNGの実読込。新規Unity Buildではありません。診断API・自動撮影とOS実キーを区別し、OS実キー・Humanの今回の操作感は未確認です。

Production `394b4a74e366678a9e0542d895b9a26f86993c19`、既存差分・過去Proofを保持。レビュー資料だけをこのbranchに共有します。C11/C18はHOLD。今回の動作と判定はHuman Review待ちです。
