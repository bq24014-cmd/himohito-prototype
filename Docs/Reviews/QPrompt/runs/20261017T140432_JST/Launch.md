# 手動で比較する

Windows側の保存先（`%USERPROFILE%`は本人のユーザーフォルダ）：

```text
%USERPROFILE%\.codex\visualizations\2026\08\08\019fdfa3-5ee1-7911-8a24-63282af5bae4\TutorialQPromptProof_20261017\
```

- `START_QPrompt.cmd`：修正版。`Build_After_r3\TutorialQPrompt.exe`を起動する。
- `COMPARE_CurrentPrompt.cmd`：現行版。`Build_Before_r2\TutorialQPrompt.exe`を起動する。
- 最初のステージ選択で「チュートリアル」を開始。Qパネルは従来どおり第3／第4区間の接続時に表示。
- Tutorialだけの検証Build。本編は収録していない。
- 終了はウィンドウの×またはAlt+F4。終了操作のOS実測は未確認。

A/D移動、Spaceジャンプ、矢印照準、W/S長さ、E接続／解除、Q橋生成、F統合、R復帰、Z説明、Tab操作説明、Escポーズは既存のまま。操作を増やしていない。

ランチャーには診断フラグ・自動撮影・自動入力・自動終了・Profiler接続指定なし。音は従来どおり。BuildフォルダのUnity付属ファイル一式が必要で、EXEだけを改名・移動しない。

既存`HimoHitoReviewRuntime`、腕候補、A2凍結版はそのまま保持した。Qのランチャーはこれらを上書きしない。Build本体はGitHubに追加していない。

比較撮影では、別途明示フラグを使いAPI／Reflectionで5状態を用意した。診断の成功を手動キー操作やHumanの理解確認へ置き換えない。
