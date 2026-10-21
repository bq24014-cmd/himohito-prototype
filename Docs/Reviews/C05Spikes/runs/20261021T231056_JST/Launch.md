# 1.00倍の下降案を手動で確認する

Windowsの保存先は次のフォルダーです。`%USERPROFILE%` はWindowsの自分のユーザーフォルダーです。

```text
%USERPROFILE%\.codex\visualizations\2026\08\08\019fdfa3-5ee1-7911-8a24-63282af5bae4\C05WarmScissorsLoweredProof_20261021\Manual
```

1. `01_Candidate.cmd` をダブルクリック：元の1.00倍で0.95だけ下げた配置。
2. 比較する場合は一度ゲームを閉じ、`02_Before_FullSize.cmd` をダブルクリック：下げる前の1.00倍ハサミ。
3. `03_Current_Production.cmd`：旧トゲの比較表示。同じ隔離Player内で起動し、Production本体は開きません。

本編第2区間の入口、残量50・選択長6から始まります。未接続でW／Sを押すと5／7へ変更できます。矢印で照準、Eで接続・解除、A／Dで移動・振り子加速。自動入力・撮影・自動終了は開始しません。

Rは既存チェックポイントへ戻ります。対岸到達で次のチェックポイントへ進んだ場合、入口から比較するにはゲームを終了してランチャーを再実行してください。終了はウィンドウの×またはAlt+F4。

`Runtime/PlayerGroundingReview.exe` と隣接するData・付属ファイル一式が必要です。EXEだけを移動・改名しないでください。既存の腕レビュー用固定ランチャー、0.50倍案、旧1.00倍原本は変更していません。

実行方式は既存非Development Player＋独立managed overlayです。Unityの新規Buildではありません。手動起動の待機確認済みですが、今回の候補をHumanが操作した結果ではありません。

今回の重要な制限：大きさとCameraを固定して下げたため、持ち手の大部分は画面下へ隠れます。[通常画角の比較](Report.md)を併せて確認してください。
