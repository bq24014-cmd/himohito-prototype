# C05 ハサミ表示試作の起動

本編第2区間の手前トゲ1か所だけを置き換えるデザイン確認用。判定は旧トゲの矩形のままで、拡大した刃の根元と一致していない。完成ギミックや採用版として遊ぶものではない。

Windowsのエクスプローラーのアドレス欄へ、次のフォルダを入力する。

```text
%USERPROFILE%\.codex\visualizations\2026\08\08\019fdfa3-5ee1-7911-8a24-63282af5bae4\C05ScissorsDesignProof_20261021\Manual
```

- `01_SCISSORS_DESIGN.cmd`：今回のハサミ。ダブルクリックで起動。
- `02_PRODUCTION_CURRENT.cmd`：Productionの現行三角トゲ。同じScene・開始位置・Camera。

1本ずつ起動する。終了はウィンドウの×またはAlt＋F4。手前の足場から右下を見ると対象が見える。既存のA/D、矢印照準、E、R、Tab、Esc等を使用する。操作キー・カメラは変更していない。

両方とも本編第2区間の入口へ一度だけ診断配置する。残量50、選択長7、橋0。起動後は通常入力で進み、自動攻略・接触用テレポート・撮影・自動終了は動かない。動画は別のQAモードで撮影している。

実体は同フォルダの1つ上にある `Runtime/PlayerGroundingReview.exe`。Unityの付属フォルダと `ReviewAssets/HimoHitoCraftScissorsParts-v1.png` が必要。EXEだけを移動・改名しない。過去のC05ランチャーとBuildはそのまま残っている。

既存非Development Playerを利用したmanaged overlayであり、新規Buildではない。手動経路の起動とREADY後5秒の生存は[起動結果](QA/ManualStartup.json)に記録。OS実キー／Human操作感は未確認。GitHubへBuild一式は追加していない。
