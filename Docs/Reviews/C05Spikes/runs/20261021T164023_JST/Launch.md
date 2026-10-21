# 床置きハサミの手動確認

Windowsで次の場所にある `01_FLOOR_SCISSORS.cmd` をダブルクリックする。

```text
%USERPROFILE%\.codex\visualizations\2026\08\08\019fdfa3-5ee1-7911-8a24-63282af5bae4\C05FloorScissorsProof_20261021\Manual\01_FLOOR_SCISSORS.cmd
```

MainStage第2区間の入口から開始。残量50、選択長7。A/D・Space・矢印・E/Q/F/R・Tab/Escは既存のまま。今回の入口配置は一度だけ行い、自動操作・撮影時計・自動終了は使わない。

同じフォルダの `02_PRODUCTION_CURRENT.cmd` は三角トゲの比較用。`03_REJECTED_HANGING_REFERENCE.cmd` は保持した不採用の吊り案を起動する参照用で、採用版ではない。一度に1つずつ起動し、ウィンドウ右上の×またはAlt+F4で終了する。

Runtimeの `PlayerGroundingReview.exe` と付属データ、`ReviewAssets/HimoHitoGreenScissorsParts-v2.png` が必要。EXEだけ移動・改名しない。非Developmentの既存Playerへmanaged assemblyと新PNGを組み合わせた実行環境であり、今回新しいUnity Buildを作成したわけではない。

刃と旧判定は一致していない。まず静止した形と置き方、[接触動画](Video/Floor-Contact-FullSize.mp4)で表示反応を確認するデザイン試作。旧判定の外にある刃先へ触れても反応しない点や、見た目より上で反応する点を、完成した当たり判定と混同しない。
