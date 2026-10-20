# C06 C07 隔離候補の手動起動

Windows上の新しいC06/C07専用ランチャーです。既存の腕・Q・Tabレビュー用ランチャーは上書きしていません。

エクスプローラーのアドレス欄へ次を貼り付けて開いてください。

```text
%USERPROFILE%\.codex\visualizations\2026\08\08\019fdfa3-5ee1-7911-8a24-63282af5bae4\C06C07HudHintProof_20261020\Manual
```

| ファイル | 内容 |
|---|---|
| 01_CANDIDATE_Tutorial.cmd | 候補 Tutorial 1600×900 |
| 02_CANDIDATE_MainStage.cmd | 候補 MainStage 1600×900 |
| 03_BASELINE_Tutorial.cmd | 修正前 Tutorial 1600×900 |
| 04_BASELINE_MainStage.cmd | 修正前 MainStage 1600×900 |
| 05_CANDIDATE_Tutorial_720.cmd | 候補 Tutorial 1280×720 |
| 06_CANDIDATE_MainStage_720.cmd | 候補 MainStage 1280×720 |

一方ずつダブルクリックしてください。Tutorialは既存のステージ選択から開始します。MainStageは既存の開始演出後にプレイできます。自動攻略、診断配置、自動撮影、撮影時計、自動終了は起動しません。

1. 左HUDに区間・残量・バー・選択長が残り、Tabの行だけがなくなったことを確認。
2. 右上の「Tab：操作方法　Esc：ポーズ」を確認。
3. Tabで説明を開閉。説明画面の文面・紙・配置は従来どおり。
4. Escでポーズ・再開。ポーズ中のTab説明から戻る場合も確認。
5. 戻った後に右上案内が復帰するか確認。

終了はウィンドウの×またはAlt+F4です。フォルダ内のEXEを改名・移動せず、付属ファイルを保持してください。

実行方式は凍結済み非Development Playerの再利用とmanaged overlayです。今回の新規Unity Buildではありません。起動・待機の確認済みですが、OS実キーの反応とHumanによる読みやすさは未確認です。レビューのための操作・撮影・起動終了は継続許可済みとして記録し、許可と操作ツールの実動可否を区別しています。

巨大なPlayer一式はGitへ含めていません。この起動経路は制作PC向けです。画像・差分・QAはリポジトリ内から直接確認できます。
