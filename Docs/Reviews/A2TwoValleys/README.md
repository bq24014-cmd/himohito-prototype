# ヒモヒト A2 Two Valleys：固定レビュー入口

固定ブランチ：`review/a2-two-valleys`。Production/mainの正式採用資料ではありません。

1. 毎回このブランチの [LATEST.json](LATEST.json) を読む。
2. `report_path` の最新報告書と `human_results_path` を同じcommitから読む。
3. 報告書内の相対リンクから通常のGitHub画像・QA JSONへ進む。ZIP添付は不要。
4. 過去の `runs/<JST日時>/` は上書きしない。次回は新runを追加し、LATESTと最新報告を同一commitで更新する。

最新：[20261016T184703_JST](runs/20261016T184703_JST/Report.md)。状態：試遊結果あり、経験者1名。現行A2 r1は不採用推奨。Human正式採用：未承認。Production統合：false。

本人の追加回答：着地に納得できず、別経路でもう一度遊びたいとは思わなかった。自動QAの技術PASSはこの評価を覆さない。過去runと元Proofは保持し、A2の基本アイデアや既存システム全体の不採用とは区別する。

## 共有範囲

レビュー報告、表示用実Unityプレビュー、必要なJSON、重要ログ抜粋、地形履歴、本人結果のみ。Build/Unity Project/Library/BGM/巨大な全ログ/ローカル絶対パスは含めない。元PNGと全ログは作成元で保持。

**注意：R復帰後の画像は既存資料に未収録。** 復帰QAと本人telemetryはあるが、入口画像を復帰後の画像とは主張しない。技術PASS、面白さ、Visual完成度、実キー確認範囲を分けて扱う。

mainへはmerge/pushしない。これはレビュー専用の共有許可であり、A2や新Worldの正式採用ではない。
