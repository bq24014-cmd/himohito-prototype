# C05 危険物のデザインレビュー

[最新状態](LATEST.json)／[向きと接地の比較](runs/20261021T202109_JST/Report.md)／[比較起動](runs/20261021T202109_JST/Launch.md)

最新は、本体承認済みの暖色ハサミについて、向きと接地だけを見直した隔離候補です。持ち手を手前、刃先を右奥へ向け、面を床へ寝かせています。本編手前1か所のみ。素材・開閉ロジック・判定は前回のままです。

[同一画角比較](runs/20261021T202109_JST/Images/Normal_Comparison.jpg)／[接地拡大](runs/20261021T202109_JST/Images/Contact_Comparison.jpg)／[修正版の全画面](runs/20261021T202109_JST/Images/After_Normal.jpg)

今回の比較は実Unityの静止画です。寝かせたことで通常画角の刃が細く見える点は、自然な置き方と合わせてHuman Reviewに残します。判定の不一致は未解消で、配置・動き・判定・本番統合の承認とは分けて扱います。

## 履歴

- [前回配置 32809c7](runs/20261021T193948_JST/Report.md)：Humanから「向きが不自然」の修正指示。原本・開閉動画を保持。
- [暖色本体 5c64b9e](runs/20261021T180204_JST/Report.md)：本体の形・色・厚みをHuman承認。今回の基準。
- [床置き緑案 25ba244](runs/20261021T164023_JST/Report.md)：Human不採用。原本保持。
- [旧赤吊り案 38a83d9](runs/20261021T160305_JST/Report.md)：Human不採用。原本保持。
- [三角形素材調整案 80a004](runs/20261021T150807_JST/Report.md)：Human採用保留。

実行は既存非Development Unity Player＋独立managed overlay＋承認PNGの実読込。新規Unity Buildではありません。API/診断撮影とOS実キーを区別し、実キーは未確認です。画像は通常のGitHubファイルとして共有しています。

Production `394b4a74e366678a9e0542d895b9a26f86993c19`、既存差分・過去Proofを保持。mainへのcommit/push・本番統合・他のトゲへの展開は行っていません。C11・C18はHOLD、向きと接地のHuman Review待ちです。
