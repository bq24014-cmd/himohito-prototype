# C14 定規の緑フック

固定入口は [LATEST.json](LATEST.json)。最新は[正式採用とProduction統合](runs/20261020T112131_JST/Report.md)。[採用元の描画順だけの比較](runs/20261020T110726_JST/Report.md)と[隔離手動起動方法](runs/20261020T110726_JST/Launch.md)も保持する。

Humanが描画順だけの候補を正式採用。Production main `d88160da59fd5902222ef68a76a8b74882fafe98` へ統合・push済み。固定具7部品は維持し、再読込後もリング描画順8を確認した。実行確認はmanaged overlayで、新規Unity Buildではない。

旧候補 `6190090cd638b1b28c6eecb3608e47b0e05a6af9` は **Human不採用**。評価「逆におかしくなっている」。[不採用理由](runs/20261020T110726_JST/HumanReviewResult.md)を記録し、旧素材・実装・Build・比較資料は失敗分析用に保持する。採用済みとして再利用しない。
