# C14 描画順だけの手動比較

今回のフォルダは次の場所。旧候補のランチャーは上書きしていない。

```text
%USERPROFILE%\.codex\visualizations\2026\08\08\019fdfa3-5ee1-7911-8a24-63282af5bae4\C14S4HookProof_20261020\OrderOnly\Manual
```

- **02_ORDER_ONLY_C14.cmd**：今回の描画順だけの候補。
- **01_BEFORE_Production.cmd**：既存のProduction基準ランチャーを呼び出す。斜め板の不採用候補ではない。

片方ずつダブルクリックする。S4出発岸、残量45、選択長5、橋0から開始。矢印または既存マウス照準で定規左上の緑フックを狙いE、Qで橋を作る。以後は通常操作。終了はAlt+F4。

自動入力・自動接続・自動Q・自動撮影・自動終了・撮影用時計は手動起動では無効。新候補の実体は `OrderOnly/Runtime/PlayerGroundingReview.exe` と付属データ一式。既存の非Development Playerを使うmanaged overlayで、新規Unity Buildではない。

旧 `Manual/02_CANDIDATE_C14.cmd` は斜め板の不採用候補なので、今回の候補と取り違えないこと。失敗分析用に保持しているだけで、採用済みとして使用しない。
