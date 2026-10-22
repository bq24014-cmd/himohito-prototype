# 手動比較の起動

Windowsのこのタスクの作業領域にある `C05ProgressiveUnravelProof_20261022/Manual/` を開きます。絶対パスはCodex完了返信のリンクから開けます。

- `Play_Progressive_Unravel.cmd`：今回の約0.6秒、切り口から順にほどける候補。
- `Play_Previous1cc.cmd`：同じRuntimeで前1ccの約0.27秒フェードを比較。

どちらか一方だけをダブルクリックします。MainStage第2区間入口、残量50、選択長7。非Development・1600×900・通常実時間です。

既存操作を使用します。A/D移動・振り子加速、矢印で照準、E接続／解除、W/Sで未接続時の長さ変更、R復帰、Escポーズ。終了はウィンドウの×またはAlt+F4。比較時は一方を終了してから他方を起動してください。

入口への初期配置はProof共通設定です。手動起動では自動入力・撮影・終了を開始しません。既存音量設定へ連動します。起動完了・10秒以上待機・正常終了は確認済み、OSキー反応と今回のHuman試遊は未確認です。

実行方式は既存Unity 6000.3.21f1 Player＋独立managed overlayで、新規Unity Buildではありません。GitHubの[ランチャー控え](Launchers)だけでは起動できず、ローカルの隣接Runtime一式が必要です。巨大なPlayer・Library一式はGitに追加していません。

基準1ccの原本 `C05CutVfxContinuityProof_20261022` と、その前の `C05CutVfxProof_20261022` は別フォルダで保持しています。
