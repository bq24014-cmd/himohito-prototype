# C14 手動比較の起動

このPC上のC14専用フォルダから起動する。既存の腕・Q・Tab・A2・汽車・C08・C20ランチャーは上書きしていない。Build一式はGitへ追加していない。

フォルダ：

```text
%USERPROFILE%\.codex\visualizations\2026\08\08\019fdfa3-5ee1-7911-8a24-63282af5bae4\C14S4HookProof_20261020\Manual
```

- **02_CANDIDATE_C14.cmd**：今回の修正版。
- **01_BEFORE_Production.cmd**：Production 1ffac00相当の修正前表示。

片方ずつダブルクリックする。MainStage S4の出発岸、残量45、選択長5、橋0から開始する。矢印または既存のマウス照準で、定規左上の緑フックを狙ってE。Qで橋を作り、A/Dで往復できる。Rで入口へ戻る。終了はAlt+F4、または通常のEscメニューを使用する。

自動入力・E/Q・自動撮影・自動終了・オフライン撮影時計は手動起動では有効にしない。Scene・カメラ・物理は既存のものを使う。起動位置のセットだけは隔離fixtureで行う。

実体はそれぞれ `BeforeRuntime/PlayerGroundingReview.exe` と `Runtime/PlayerGroundingReview.exe`。exeだけを取り出したり改名したりせず、付属ファイルと一緒に保持する。**既存非Development Playerのmanaged overlayであり、新規Unity Build成功ではない。**

手動ランチャーの起動と無操作での継続を確認したが、OSキー反応・Human試遊は未確認。ログは `Manual/Sessions/` に毎回別名で作る。
