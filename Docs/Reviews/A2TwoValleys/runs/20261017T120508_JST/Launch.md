# 手動起動（Windows側のみ）

新規 `PlayerConnectionSwingProof_20261017` フォルダを使用する。BuildをGitHubから配布したものではない。依頼者への完了返信に、そのPC上の正確なファイルリンクを示す。

```text
PlayerConnectionSwingProof_20261017/
  START_NewCandidate.cmd
  COMPARE_Candidate2.cmd
  README_起動.txt
  Build_r3/
    ConnectionSwingProof.exe
    ConnectionSwingProof_Data/
    UnityPlayer.dll
    MonoBleedingEdge/
    ...（付属ファイルをすべて保持）
```

新候補は `START_NewCandidate.cmd` をダブルクリック。候補2は一度終了し、`COMPARE_Candidate2.cmd` を開く。同じexe・A2 B地形・1280×720で比較する。

exeを直接開く場合も、**`Build_r3/ConnectionSwingProof.exe` が新候補・手動起動の既定**。`UnityCrashHandler64.exe` ではない。旧 `Build_r1`／`Build_r2` は検証履歴として保持し、今回のHuman用ランチャーはr3を指す。

候補2だけ `-presentation candidate2`、新候補は `-presentation candidate3`。通常ランチャーに `-gestureCapture 1` はない。引数なしでも自動入力・撮影・ポーズ・終了は始まらない。音は元の設定のまま。

終了は右上の×か、ゲームを選択した状態でAlt+F4。Escは既存のポーズ。新しいキー・切替UI・入力モードはない。

A/D＝移動・加速、Space＝ジャンプ、W/S＝ヒモ長、矢印＝照準、E＝接続／解除、Q＝橋、F＝既存の統合／文脈操作、R＝入口へ復帰、Tab＝操作説明。既存マウス補助も保持。

入口は24／選択6／橋0。A2 Bには出口CPがなく、出口後のRも入口へ戻る。今回A2自体を採用した意味ではない。

Humanには、入口で接続した足元、A/Dを押す／離す／逆に押すときの身体、解除から着地、ポーズ／R時の崩れを比較してほしい。正解ルートや解除タイミングの指示はしない。

ダブルクリック・OSキー反応・OS終了操作は今回未確認。確認したのは同じexeのプロセス起動と、ゲーム内の読み取り状態記録まで。記録はUnity persistentDataPathの `ConnectionGestureSessions/` に新規保存され、旧HumanSessionsを書き換えない。
