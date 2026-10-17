# 手動で確認する

Windowsの保存先（`%USERPROFILE%`は本人のユーザーフォルダ）：

```text
%USERPROFILE%\.codex\visualizations\2026\08\08\019fdfa3-5ee1-7911-8a24-63282af5bae4\TabControlsProof_20261017\
```

- 修正版：`START_TabProof.cmd`をダブルクリック。
- 現行比較：`COMPARE_OriginalTab.cmd`をダブルクリック。
- 実行ファイル：`Build_r2\TabControlsProof.exe`。比較は同一EXEの`-tabOriginal`表示切替で行う。
- ステージ選択でTab、またはTutorial開始後にTabで操作説明を開く。Tab／Escで閉じる。ゲーム中のEscポーズは従来のまま。
- 終了はウィンドウの×またはAlt+F4（OSでの終了キー確認は未実施）。

両ランチャーは1600×900の通常ウィンドウ。1280×720／1600×900の実描画比較は報告書に掲載した。BuildはTutorialだけを収録しており、本編起動用ではない。

手動起動に`-tabCapture`はない。自動API操作・診断ポーズ・撮影・自動終了は、明示的な撮影フラグにだけ分離している。撮影時計への変更はない。通常音声は既存のまま（診断撮影だけ-noaudio）。Profiler接続等の開発機能は無効。

Unity付属ファイル一式が必要。EXEだけを改名／移動しない。既存Q・腕のランチャーとは別フォルダであり、それらを上書きしていない。Build本体・Library・BGM実体はGitHubへ追加しない。
