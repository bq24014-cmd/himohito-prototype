# Tutorial smoke — PASS

修正後の隔離Projectで、authored Tutorialの開始地点から通常の物理経路で区間1〜4を進行。位置の直接変更、手動checkpoint capture、merge後の新checkpoint追加は一切行わない。

## 実行結果

1. BeginGameFromTitleで開始。ステージプレビューは既存Advanceのskip経路で終了。
2. T1で右へ歩行、長さ6でE接続、振り子中に右入力、右上がりの時点で解除して対岸へ着地。物理判定で区間2へ移行。
3. 区間2入口でR。CurrentLength=99、SelectedRopeLength=6、区間2が復元。
4. T2で長さ6の振り子、棘を越えて右の床へ着地。物理判定で区間3へ移行。
5. 選択長7でT3のE/Q。長さ7のヒモ橋を生成し、歩いて区間4入口に到達。
6. 区間4入口でR。T3橋1本（端点(36,0.35)→(30,0.35)、長さ7）、残量92、選択長7が復元。
7. 選択長6でT4の2本をE/Q生成。中央Hookを狙いF。既存T3橋とT4統合橋の計2本、中央Hook inactive、removedHooks=1、残量80を確認。
8. Rでauthored第4区間入口の統合前snapshotへ戻る。中央Hook active、removedHooks=0、T3橋1本、残量92、選択長7、区間4、位置(39,0.95)へ戻る。

merge後checkpointは元々ないため、TutorialにMainStageと同じ「統合済みcheckpointの復元」を作り込んでいない。最後のRで中央Hookがactiveになるのは正しい入口snapshotの復元であり、本TaskのMainStageの不具合とは異なる。

根拠: Evidence/Tutorial/assertions.txt（15項目PASS）、state.jsonl、result.txt、01_tutorial_merged.png、02_tutorial_after_restart.png、Tutorial_runtime.log。

操作は診断driverによる既存メソッド呼出しと移動入力注入。手動キーボード操作・音の聴覚評価・配布Buildテストを実施したとの意味ではない。
