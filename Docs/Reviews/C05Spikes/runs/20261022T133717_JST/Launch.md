# 手動比較の起動

Windowsのこのタスクの作業領域にある `C05CutVfxContinuityProof_20261022/Manual/` を開きます。正確な絶対パスはCodex完了返信にも記載します。

- `Play_Revised_Fade.cmd`：今回の修正版。
- `Play_Previous47f.cmd`：同じRuntimeで旧47fの早期消去／戻り／フェードを比較するモード。

どちらか一方をダブルクリックします。非Development・1600×900・通常実時間・MainStage第2区間入口、残量50、選択長7、1.00倍下降0.95／Camera下降0.95。W/Sで未接続時の選択長を5／6／7へ変更できます。A/D移動・振り子加速、矢印照準、E接続／解除、R復帰、Escポーズ。終了はウィンドウの×またはAlt+F4です。

自動入力・自動撮影・自動終了は起動しません。入口への初期配置は従来のProof共通処理です。比較するときは一方を終了してからもう一方を起動します。音量は既存効果音設定に従います。

GitHubの[ランチャー控え](Launchers)だけでは起動できません。ローカルでは隣のRuntimeフォルダが必要です。巨大なUnity Player一式は通常Gitに追加していません。元47fの原本は `C05CutVfxProof_20261022/Manual/Play_Candidate_v4_CutVFX.cmd` に保持しています。

実行方式：既存Unity 6000.3.21f1非Development Player＋独立managed overlay。新規Unity Buildではありません。手動起動／待機／正常終了は確認。OSキー反応・今回のHuman試遊は未確認です。
