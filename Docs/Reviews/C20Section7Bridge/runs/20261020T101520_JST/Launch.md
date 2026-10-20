# 修正前後の手動起動

Windowsの場所：`%USERPROFILE%\.codex\visualizations\2026\08\08\019fdfa3-5ee1-7911-8a24-63282af5bae4\C20DeepSagProof_20261020\Manual\`

- `01_BEFORE_22f35ce.cmd`：採用保留の旧22f候補。既存の旧Playerへ委譲し、原本は上書きしない。
- `02_AFTER_DEEP.cmd`：今回の深いたわみ候補。

ダブルクリックで起動。既定はS7入口・残量24・選択長5・橋0。矢印で上岸の緑フックを狙いE→Q、Dで登る。途中でDを離して止まり、Dで再開。Aで戻れる。Rは24・選択長5・橋0へ復帰する。W/Sで長さ4へ変えると長さ4も確認できる。ランチャー引数4で起動した場合はRの選択長も4。Escでポーズ、終了は×またはAlt+F4。

両cmdを実際に起動し、初期化完了・その後5秒以上生存/応答することをログで確認した。手動版は自動入力・自動生成・撮影・終了・撮影時計・撮影用Camera固定を開始しない。本人がE/Qを操作する。OSキー反応・Human操作感は今回未確認。

Unity 6000.3.21f1の既存非Development Playerへの独立managed overlay。新規Unity BuildPipeline Buildではない。エンジン・素材は不変ハードリンク、DLLは新Proofだけの独立ファイル。EXEだけを移動せず、RuntimeのData等も維持する。Build本体はGitへ追加しない。
