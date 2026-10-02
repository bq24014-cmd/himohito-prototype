# Human Review — Tutorial全4区間の2.5D trial

Human全体評価: 2026-10-01「自然で見やすい。全体展開してよい」。詳細は04_HumanReview.md。第1区間P1と前景R1の承認とは別の全区間版承認。以下の個別操作は項目別の回答を得ていないため、未確認のまま保持する。

## 起動

専用Build: `output/Tutorial2p5DAllSections_20261001_Trial01/Game/HimoHitoTutorialAll2p5D.exe`。

- `START_TRIAL.bat`: 全区間版。
- `START_BEFORE.bat`: 第1区間は前回R1、以降は既存背景。
- `START_PRODUCTION.bat`: 全区間既存背景。

比較は別起動で行う。ウィンドウタイトルは `Tutorial2p5DAllSections_20261001`。過去P1/R1のBuildと取り違えないこと。Stage SelectionでTutorialを選択して開始。

## 操作での確認

分類は PASS / FAIL / DIFFICULT BUT INTENDED / UNCLEAR。

| 確認 | 結果・メモ |
|---|---|
| Stage Selection → Tutorial。開始見渡し後に背景が表示される | 未確認 |
| 第1区間: A/D、Space、矢印照準、E接続/解除で振り子を渡る | 未確認 |
| 第2区間: 長さ調整と振り子。棘・Hook・主人公の区別 | 未確認 |
| 第3区間: 長さ7、E接続 → Qで橋。歩行/ジャンプが見やすい | 未確認 |
| 第4区間: 2本の橋 → F統合。中心Hook消失と橋の見やすさ | 未確認 |
| 各区間で左/右移動、ジャンプ。背景が動きすぎず酔わない | 未確認 |
| 各看板を開く/閉じる。看板の文字と絵が読める | 未確認 |
| Section 1→2、2→3、3→4で背景が急に飛ばない | 未確認 |
| R再開、失敗→R。既存仕様どおりに復帰する | 未確認 |
| 宝箱まで進む → Clear → Stage Selectionへ戻る | 未確認 |
| MainStageを開始すると既存背景のまま | 未確認 |

## 見た目

- 同じ子供部屋を進んでいる感じと、Far/Mid/Nearの奥行きがあるか。
- 画面中央の主人公・Hook・Rope・Bridge・足場を背景から区別できるか。
- 汽車の車輪、毛糸玉、積み木の下に不自然な隙間/浮きがないか。
- 第4区間後半/ゴール右端のmirror延長、縦の継ぎ目、棚の対称性が気になるか。
- 前景玩具とGameplay足場は別の奥行きの床。背景の玩具をGameplay障害物と誤認しないか。
- 16:9以外の画面比率や低性能PCは本採用前の追加確認。

FAIL記録: Scene / Section / 操作 / 現象 / 再現率 / screenshotまたはvideo名。

## 最終分類

T1: 全体展開成功、採用価値が高い。

T2: 一部成功、区間によって改善余地あり。

T3: 不成立、別アプローチが必要。

Human Review回答と日付: 2026-10-01「自然で見やすい。全体展開してよい」。全体評価T1。項目別操作、別aspect、低性能環境の確認をすべて実施したとは扱わない。今回のtrialで停止し、Productionへ適用しない。
