# H2 — Clear Footstep

## Runtimeで確認した経路

本編のMainStageGoalZoneとTutorialのBeginClearSequenceはbody.simulated=falseにしますが、PlayerMoverはenabledのままでした。UpdateがA/Dを読み続け、FixedUpdateが接地判定→ApplyGroundControl→UpdateFootstepAudioを続けます。物理で位置が動かなくても、移動入力と速度値を使ってfootstepAudioSource.PlayOneShotを呼べる状態でした。

隔離コピーのInput戻り値seamで実Update/FixedUpdateを通し、実際のfootstepAudioSource.PlayOneShotの直後をカウント。各Sceneの実GoalZoneからClearへ入り、A/Dを交互に20回、各0.3秒+解放0.1秒、Jumpも供給しました。

| Scene | BEFORE 足音 | AFTER 足音 | AFTER 移動/Jump入力読み取り | 試行数 |
| --- | ---: | ---: | ---: | ---: |
| Tutorial | 20 | 0 | 0 | 20 |
| MainStage | 20 | 0 | 0 | 20 |

両方ともmover.enabled=true、body.simulated=falseのまま、プレイヤー位置差0。入力を処理しないためjump bufferも0。通常本編では両版とも歩行0.8秒で実足音1回を確認。

## 最小修正

PlayerMover.UpdateとFixedUpdateの先頭を同じ `ClearInputWhileSimulationStopped()` でガード。停止中はmoveInput、jumpBufferTimer、coyoteTimer、footstepTimerを0へ戻してreturnします。移動・ジャンプ・接地/着地・足音処理を実行しません。

GoalPose・Clearコルーチン・R復元・Scene遷移・音源・音量には変更なし。IsGroundedの表示用状態も維持し、通常の歩行・空中・振り子の物理式には手を加えていません。

証拠：Evidence/Before/checks.txt、After/checks.txtのCLEAR_INPUT/CLEAR_RESULT/NORMAL_FOOTSTEPS。クリア描画はtutorial_clear.png/main_clear.png（world-only）。本編のR復元と最終ClearはMainRegressionでも確認。

判定：原因・入力seam回帰 **PASS**。Windowsのcomputer-use接続失敗のため、OSキー送信による確認は **未実施**（人間の実操作PASSとは区別）。
