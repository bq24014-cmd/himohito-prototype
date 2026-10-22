# 手動起動：C05 Soft Unravel

ローカルの `C05SoftUnravelProof_20261023` フォルダを使用します。今回の返信にWindowsの正確な絶対パスを示します。GitHubには個人名入り絶対パスやBuild全体を公開していません。

## 起動

1. 他の比較Playerを閉じる。
2. `Manual/Play_Soft_Unravel.cmd` をダブルクリック（今回の候補）。
3. 基準3cacaと比較するときは終了後に `Manual/Play_Previous3caca.cmd` を起動。

ランチャーの記録：[今回](Launchers/Play_Soft_Unravel.cmd)／[旧3caca](Launchers/Play_Previous3caca.cmd)。GitHubからこのcmdだけを落としてもRuntime一式がない環境では起動できません。既に配置済みの隔離Runtimeを使ってください。

同じ `Runtime/PlayerGroundingReview.exe` を使い、旧比較だけ `-vfxProgressive 1` を指定します。両方とも非Development既存Player＋独立managed overlay、900pウィンドウ、第2区間入口・選択長7です。通常入力・実時間で動きます。`-routeQA`、`-capture`は付けず、自動攻略・撮影時計・自動終了は起動しません。

A/D：移動・振り子加速、矢印：照準、W/S：未接続時の長さ変更、E：接続／解除、Space：未接続ジャンプ、Q：橋生成、F：既存文脈操作、R：復帰、Tab：操作説明、Esc：ポーズ。長さ5／6／7は未接続時に切り替えます。既存操作は変更していません。

終了はウィンドウ右上の×またはAlt+F4。Rで今回の切断コピーは即時に消えます。ポーズ中は余韻も停止します。

## 確認範囲

両ランチャーと同じ引数の通常起動で、準備完了後10秒待機・自動処理なし・通常終了を確認しました。OS実キーの反応、Humanの操作感は今回未確認です。

実装DLL SHA256：
`27ef641f1648872da87d2d351531ddff57407c82c0dbb07074a6d59c29d1df71`

新規Unity Buildは行っていません。基準3cacaの実装・Buildは別フォルダに保持しています。既存の腕・Q・Tabなどのランチャーは上書きしていません。
