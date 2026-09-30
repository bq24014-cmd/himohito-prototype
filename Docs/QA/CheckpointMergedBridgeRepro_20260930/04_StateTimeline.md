# State timeline — 実際のログ順

以下はMainStageのstate.jsonlとstack traceに基づく。推定の呼び出し順ではない。

## 保存

| seq | frame | event | 保存状態の変化 |
|---:|---:|---|---|
| 15 | 1206 | REPRO_BEFORE_CHECKPOINT | section9、merged1、中央inactive、removed1。旧保存配列0 |
| 16 | 1660 | TryReachSection.accept | OnCollisionStay2Dから受理（変更前section9） |
| 17 | 1660 | CaptureCheckpointState.enter | section10へ更新済み。旧保存rope50/選択7/橋0 |
| 18 | 1660 | CapturePlatformStates.enter | 保存rope38/選択6へ更新済み。橋配列は代入前 |
| 19 | 1660 | CaptureCheckpointState.exit | 保存橋1、両端180/191、長さ12。中央inactiveのまま |
| 20 | 1660 | CHECKPOINT_CAPTURE_OBSERVED | harnessが保存後を確認 |

frame1660/time4.619998秒で保存。player位置x193.00943、y-1.53501。merged checkpointをテスト側からCaptureしたのではなく、既存床の0.12秒安定着地条件が受理した。

## 復元

| seq | frame | event | 中央Self/Hierarchy | removed数 | 橋数 |
|---:|---:|---|---|---:|---:|
| 21 | 1661 | RESTART_BEFORE | false/false | 1 | 1 |
| 22 | 1661 | RestartFromCheckpoint.enter | false/false | 1 | 1 |
| 23 | 1661 | DetachAndRefund.enter | false/false | 1 | 1 |
| 24 | 1661 | RestoreCurrentLength.enter | false/false | 1 | 1 |
| 25 | 1661 | RestoreSelectedRopeLength.enter | false/false | 1 | 1 |
| 26 | 1661 | RestorePlatformStates.enter | false/false | 1 | 1 |
| 27 | 1661 | ClearPlatforms.enter | false/false | 1 | 1 |
| 28 | 1661 | RestoreRemovedHooks.enter | false/false | 1 | 1 |
| 29 | 1661 | RestoreRemovedHooks.exit | true/true | 0 | 1（旧橋撤去前） |
| 30 | 1661 | CreatePlatform.exit | true/true | 0 | 1（再生成後） |
| 31 | 1661 | RestorePlatformStates.exit | true/true | 0 | 1 |
| 32 | 1661 | RestartFromCheckpoint.exit | true/true | 0 | 1 |
| 33 | 1661 | RESTART_COMPLETE | true/true | 0 | 1 |

restart処理はframe1661/time4.951874秒内で同期完了。RestorePlatformStates出口時点ではplayerはrestart前のx193.11444、その後respawn(192,-1.45)へ戻る。中央Hookはその位置復元より前、RestoreRemovedHooks内で既に復活している。

この検証はR相当で1回のrestartのみ。落下経路、異なる長さ、連続Rは回帰候補で未実行。

## Tutorialの違い

Tutorial seq12でT4入口の橋0を保存 → seq19で橋2 → seq21/22でmerge1/中央inactive → seq23でmerge安定 → seq24〜35でR相当。復元橋0は入口snapshot通り。統合後captureイベントは存在しない。
