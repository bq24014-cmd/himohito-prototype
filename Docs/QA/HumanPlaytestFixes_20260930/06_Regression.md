# Regression

Unity `6000.3.21f1`、新規隔離コピーの実Playモードで確認。本番Sceneは保存していません。Input seamと直接E/Q/F/R APIを使用する自動回帰であり、OS実キー/人間の通しプレイとは区別します。

| 必須項目 | 結果 | 根拠 |
| --- | --- | --- |
| 1. Tutorial Sign 1〜4 | PASS | BeforeSigns/Afterに各world/detail画像、キーRendererログ |
| 2. Tutorial full run | PASS | TutorialRegression 23/23。開始→T1/T2 swing→T3 Q7→T4 E/Q/F→R→再生成→実Goal→Clear→MainStage |
| 3. MainStage通常footstep | PASS | After/checks.txt、0.8秒歩行で実PlayOneShot 1回 |
| 4. Tutorial Clear A/D | PASS（入力seam） | 20回、footstep 0・入力読み取り0・位置差0 |
| 5. MainStage Clear A/D | PASS（入力seam） | 20回、footstep 0・入力読み取り0・位置差0 |
| 6. Section9 first bridge | PASS | MainRegression S9_LEFT E/Q成功、選択長分消費 |
| 7. Section9 second targeting | PASS | 同一near_center座標、0.25度sweep、右有効+highlight 16→27点 |
| 8. Section9 merge | PASS | second E/Q→中央F、1本長さ12、中央inactive、追加消費なし |
| 9. checkpoint restore | PASS | 統合橋から実checkpoint10着地→R3回。端点/長さ/本数/残量/選択長/削除Hook identity/inactive/collider数が一致 |
| 10. Section10 Clear | PASS | 実現在位置から長さ10橋生成→歩行→宝箱→物理停止→Clear→Stage Selection→本編再開始 |

本編は **148/148 assertions PASS**。追加で落下R×5、残量切れR×3、スイッチ/戻り足場、S4橋/解除運動量、S5遮光/復元、S6上下ルート・空中再接続API・期限、S7橋/Jumpも既存FullSystemsで確認。人間のMain5→6成立は01の確認済み事実として維持し、自動テストのcontrolled-entryを自然通しプレイと呼びません。

## Checkpoint Restore Fix

PASS維持。MainStageRespawnOnFall.cs、PrototypeRunController.cs、RopePlatformBuilder.csの修復済みproduction sourceは今回変更していません。統合橋は長さ12・本数1、削除中央Hookは同一instanceでinactive、removed記録1のままR3回後も維持。Tutorialは保存前のT4 checkpointへ戻り、T3長さ7橋・残量92・選択7を復元する従来契約を維持。

## Warning / Exceptionの区別

- 修正後のC# compile error/warning、新しいゲーム由来Exception/Warning：検出0。
- Before/After/本編回帰/Tutorial回帰でUnityEditor.Search.SearchDatabaseのArgumentOutOfRangeExceptionを各1件検出。前回Full Regressionでも同じEditor例外があり、今回のproduction変更による新規例外とは扱いません。
- MainRegressionの生resultは `tests=148 failures=0 runtimeErrors=1`。元ハーネスはEditor例外もruntimeErrorsへ加算し、exit code 2になります。これを隠したり「完全に例外0」と言い換えず、ゲームassertions PASS / Editor既存例外ありとして判断。
- 初回の低権限Unity起動はライセンス/パッケージキャッシュアクセスに失敗。正常な既存接続で再実行し実Playに成功。失敗起動ログも保持。
- computer-use list_appsの3回の接続タイムアウトにより、OS実キー追試は未実施。人間による修正後の見やすさ・手触り・A/D追試は別途必要。

Git diff --check全体には既存MainStage.unityの末尾空白があるため警告が出ます。保護対象を勝手に整形せず、今回変更したSource/README/LEARNING_LOGだけのdiff --checkが成功することを確認します。
