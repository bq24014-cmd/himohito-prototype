# C05 危険物のデザインレビュー

[最新状態](LATEST.json)／[上向きの開閉静止比較](runs/20261021T203813_JST/Report.md)／[比較起動](runs/20261021T203813_JST/Launch.md)

最新は、Human指定の「上向き・持ち手は下」を、承認済み単体図の180度回転で確認する隔離候補です。正面向きで、持ち手を床に接地。素材・色・大きさ・共通支点を保持し、本編手前1か所だけへ表示しています。

[同一画角の開閉比較](runs/20261021T203813_JST/Images/Normal_OpenClosed.jpg)／[接地拡大](runs/20261021T203813_JST/Images/Detail_OpenClosed.jpg)／[開いた全画面](runs/20261021T203813_JST/Images/Open_Normal.jpg)／[閉じた全画面](runs/20261021T203813_JST/Images/Closed_Normal.jpg)

今回は実Unityの独立した開／閉の静止画です。奥行き方向の傾きは0。アニメーションや接触判定の完成版ではありません。判定の不一致は未解消で、配置・動き・判定・本番統合の承認とは分けて扱います。

## 履歴

- [床寝かせ案 e560ab4](runs/20261021T202109_JST/Report.md)：Human不採用。原本保持。
- [前回配置 32809c7](runs/20261021T193948_JST/Report.md)：Humanから「向きが不自然」の修正指示。原本・開閉動画を保持。
- [暖色本体 5c64b9e](runs/20261021T180204_JST/Report.md)：本体の形・色・厚みをHuman承認。今回の基準。
- [床置き緑案 25ba244](runs/20261021T164023_JST/Report.md)：Human不採用。原本保持。
- [旧赤吊り案 38a83d9](runs/20261021T160305_JST/Report.md)：Human不採用。原本保持。
- [三角形素材調整案 80a004](runs/20261021T150807_JST/Report.md)：Human採用保留。

実行は既存非Development Unity Player＋独立managed overlay＋承認PNGの実読込。新規Unity Buildではありません。API/診断撮影とOS実キーを区別し、実キーは未確認です。画像は通常のGitHubファイルとして共有しています。

Production `394b4a74e366678a9e0542d895b9a26f86993c19`、既存差分・過去Proofを保持。mainへのcommit/push・本番統合・他のトゲへの展開は行っていません。C11・C18はHOLD、向きと接地のHuman Review待ちです。
