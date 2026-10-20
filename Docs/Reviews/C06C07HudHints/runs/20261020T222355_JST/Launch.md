# 布タグ修正版の手動起動

エクスプローラーのアドレス欄で次を開いてください。

```text
%USERPROFILE%\.codex\visualizations\2026\08\08\019fdfa3-5ee1-7911-8a24-63282af5bae4\C06C07HudHintProof_20261020\ClothTag\Manual
```

| ファイル | 内容 |
|---|---|
| 01_CLOTH_Tutorial.cmd | 布タグ修正版 Tutorial 1600×900 |
| 02_CLOTH_MainStage.cmd | 布タグ修正版 MainStage 1600×900 |
| 03_REJECTED_Tutorial.cmd | 不採用cd879b9版 Tutorial 1600×900 |
| 04_REJECTED_MainStage.cmd | 不採用cd879b9版 MainStage 1600×900 |
| 05_CLOTH_Tutorial_720.cmd | 修正版 Tutorial 1280×720 |
| 06_CLOTH_MainStage_720.cmd | 修正版 MainStage 1280×720 |

一方ずつダブルクリックしてください。Tutorialは既存のステージ選択から開始し、MainStageは既存の開始演出後に遊べます。入力・操作方法は従来どおりです。

左の最終行の下余白、右上の布タグ、接続前後の枠、Tab開閉、Escのポーズと再開を確認してください。ポーズ中にTabを開いて閉じると、元のポーズへ戻ります。

修正版は既存隔離環境の `ClothTag/Runtime`、比較元は以前の `Runtime` を使います。旧候補のDLL・ランチャー・資料を上書きしていません。EXE単体を改名・移動せず付属ファイルを保持してください。

手動版は通常の実時間で動き、自動入力・診断配置・撮影・終了を開始しません。終了は×またはAlt+F4です。QA用の背景色も表示しません。

実行方式は既存非Development Playerとmanaged overlayであり、新規Unity Buildではありません。通常起動と待機は確認済み、今回のOSキー入力とHuman評価は未確認です。Productionへの正式採用・統合はしていません。
