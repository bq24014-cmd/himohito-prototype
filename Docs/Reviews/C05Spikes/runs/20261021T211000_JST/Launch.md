# 手動確認の起動

今回の候補は、Windowsで次をダブルクリックします。

`%USERPROFILE%\.codex\visualizations\2026\08\08\019fdfa3-5ee1-7911-8a24-63282af5bae4\C05WarmScissorsMotionCollisionProof_20261021\Manual\01_PLAY_CANDIDATE.cmd`

本編第2区間の入口から始まり、手前の1個だけが上向きハサミになります。残量50、選択長7。A/D・W/S・矢印照準・E・Rなど、従来の操作を使えます。自動操作や撮影・自動終了は始まりません。

比較元の独立した開状態は、同じManualフォルダの `02_PREVIOUS_STATIC_OPEN.cmd` で起動できます。比較元は旧矩形判定のままなので、形の比較用です。一方ずつ起動し、終了はAlt+F4を使用してください。

実行本体は、このProof内の `Runtime/PlayerGroundingReview.exe` です。EXEだけを移動せず、RuntimeとReviewAssetsを保持してください。既存の非Development Playerに管理DLLと承認画像を独立配置した方式であり、新規Unity Buildではありません。

手動起動35秒の待機と応答ログは確認済みです。OS実キーやHuman試遊の成功を確認した意味ではありません。Windows通信確認が表示された場合、以前の固定レビューEXEとは別パスであり、その許可を勝手に変更していません。

[結果と動画へ戻る](Report.md)
