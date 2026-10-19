# C08 手動比較の起動

以下のフォルダを開き、比較ランチャーをダブルクリックします。

```text
%USERPROFILE%\.codex\visualizations\2026\08\08\019fdfa3-5ee1-7911-8a24-63282af5bae4\C08BridgeSurfaceAlignmentProof_20261020\
```

- `01_BEFORE.cmd`：C22・C23採用済みの現行表示。
- `02_CANDIDATE.cmd`：今回の描画候補1。

一方を閉じてからもう一方を起動してください。同じEXE・地形・Cameraです。実測では初期化に約16〜17秒、その後5秒待機しました。ロード中は少し待ってください。

T4統合相当橋、残量7/50を診断初期状態として用意します。以降の自動入力・撮影・終了はありません。右へ歩いて橋へ乗り、停止・待機し、左の岸へ戻れます。A/D、Space、矢印、E/Q/F/R、Tab、Escは既存どおり。区間案内を開いた時はZで閉じ、ゲームの終了はウィンドウを閉じます。

通常・中間サイズはターミナルから `02_CANDIDATE.cmd 50`、`02_CANDIDATE.cmd 30` で起動できます。BEFOREも同じ引数です。初期残量の指定であり、新しいゲーム操作ではありません。

標準cmdからEXEを直接起動します。初期PowerShell版は実行制限で動かなかったため、提出cmdではPowerShellを呼びません。実行制限・ファイアウォール・OS設定は変更していません。既存ランチャーは上書きしていません。

## 実行方式

これは **既存非Development Unity Playerへの独立managed overlay** です。Engine・素材144ファイルは内容不変のハードリンクで再利用し、Assemblyだけを隔離先へCompileしました。新規Unity BuildPipeline Build・Editor Playの成功とは区別します。Project／Libraryの複製はありません。

```text
EXE  Runtime\PlayerGroundingReview.exe
DLL  Runtime\PlayerGroundingReview_Data\Managed\Assembly-CSharp.dll
DLL SHA256  FA727DED5AE07BD8ACBE63330E7DD1E6F35E8A21911C7E6CA10C239468777F1F
```

EXEだけを移動・改名しないでください。手動起動は `-c08QA` を渡さず、自動driver・撮影時計を開始しません。ログは `ManualSessions` の新規フォルダへ保存します。Build一式はGitHubへ追加していません。

ランチャー起動・初期化ログ・待機は確認済みですが、OS実キーの画面反応とHumanのプレイ感は未確認です。比較動画は自動入力の別記録です。
