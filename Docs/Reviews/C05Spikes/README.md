# C05 危険物のデザインレビュー

[最新状態](LATEST.json)／[暖色ハサミの配置と開閉](runs/20261021T193948_JST/Report.md)／[手動起動](runs/20261021T193948_JST/Launch.md)

最新は、本体デザインをHumanが承認した暖色ハサミを、本編手前のトゲ1個だけへ配置した隔離候補です。形・色・厚みを保持し、吊り具なし、床付近への斜め置き、共通支点での短い閉じ戻りを実Unity画面で確認できます。

[通常画角](runs/20261021T193948_JST/Images/Warm_Open.jpg)／[拡大比較](runs/20261021T193948_JST/Images/Detail_Comparison.jpg)／[3秒の開閉動画](runs/20261021T193948_JST/Video/Warm-OpenClose-FullSize.mp4)／[判定重ね図](runs/20261021T193948_JST/Images/BladeTrigger_Overlay.jpg)

既存判定は保持しているため、刃間の空白への反応と刃の一部のはみ出しが残ります。本体承認と、配置・動き・判定・本番統合の承認を分けて扱います。

## 履歴

- [暖色本体 5c64b9e](runs/20261021T180204_JST/Report.md)：本体の形・色・厚みをHuman承認。今回の基準。
- [床置き緑案 25ba244](runs/20261021T164023_JST/Report.md)：Human不採用。原本保持。
- [旧赤吊り案 38a83d9](runs/20261021T160305_JST/Report.md)：Human不採用。原本保持。
- [三角形素材調整案 80a004](runs/20261021T150807_JST/Report.md)：Human採用保留。

実行は既存非Development Unity Player＋独立managed overlay＋承認PNGの実読込。新規Unity Buildではありません。API/診断撮影とOS実キーを区別し、実キーは未確認です。画像・動画は通常のGitHubファイルとして共有しています。

Production `394b4a74e366678a9e0542d895b9a26f86993c19`、既存差分・過去Proofを保持。mainへのcommit/push・本番統合・他のトゲへの展開は行っていません。C11・C18はHOLD、配置と動きのHuman Review待ちです。
