# 手動比較Playerの起動

Windowsの置き場は `%USERPROFILE%\.codex\visualizations\2026\08\08\019fdfa3-5ee1-7911-8a24-63282af5bae4\C08IntegrationC20Proof_20261020\Manual\`。

- `01_BEFORE.cmd`：C08統合済み、C20変更前。
- `02_CANDIDATE.cmd`：今回のS7長さ5修正版。

どちらもダブルクリックで起動する。既定は入口24・選択長5・橋0、MainStage第7区間の下岸。起動後は矢印で緑フックを狙いE→Q、Dで登る。Aで戻る。W/Sで4に変えると従来の長さ4も試せる。Rで24・選択長5・橋0へ戻る。起動引数に4を渡す場合はRの選択長も4。操作は既存のまま。Escでポーズ、終了はウィンドウの×またはAlt+F4。

手動版は診断用の入口配置だけ行い、自動入力・自動橋生成・自動撮影・自動ポーズ・自動終了・オフライン時計を起動しない。最初のE/Qは本人が行う。既存のランチャーを上書きしていない。

Unity 6000.3.21f1の既存非Development Player＋独立Assembly-CSharp.dllによるmanaged overlay。新規Unity BuildPipeline Buildではない。エンジン・素材は不変ハードリンク、DLLは各版の独立コピー。EXE単独で他所へ移さず、Data等を含むフォルダを保持する。Build本体はGitHubに入れない。

起動・初期化完了・その後5秒の生存/応答、自動Result/Framesがないことをプロセス・ログで確認した。OSの画面取得/キー反応とHumanの手応えは未確認。OS操作ツールのlist_appsはタイムアウトし、許可不足とは扱っていない。
