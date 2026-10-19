# C22 C23 手動レビューの起動

ローカルのPlayerGroundingProof_20261019フォルダから、START_C22C23_Fixed.cmd（修正版）またはCOMPARE_C22C23_Original.cmd（比較元）をダブルクリックします。一方ずつ使用してください。

両方ともBuild/PlayerGroundingReview.exeを使い、-groundVariant after / beforeだけが表示の差です。手動ランチャーには-groundQAを付けません。実時間で動き、自動入力ドライバー・自動撮影・自動終了を作りません。

診断初期状態はT3右岸x=39、残量7/50、選択長6、T3生成橋1本です。A/D、Space、W/S、矢印、E、Q、F、R、Z、Tab、Escは既存キーのままです。初期状態の設定は今回の表示比較用で、Productionの開始条件を変更したものではありません。

左の橋へ移動して停止・待機・再歩行を比較してください。Escはポーズ、終了は×またはAlt+F4。入力の成功・Humanの手応えは未確認です。

Buildは非Developmentで、Profiler/Script Debugging/Deep Profilingは無効です。Build一式はローカル保存で、このGitHub branchにexeを格納していません。既存の腕レビュー用固定ランチャーは上書きしていません。

Build Assembly SHA256: 4F74D997A3D6DE70246EF42A66B1B0132189011846FD1A173BEB1054F6594C0E
