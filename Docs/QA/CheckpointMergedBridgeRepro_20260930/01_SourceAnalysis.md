# Source analysis — Runtimeと分離

## 保存対象

`Assets/Scripts/RopePlatformBuilder.cs:20` のPlatformStateはStart / End / RopeLengthだけを保持。CapturePlatformStates（188行）は生成橋のこの3情報だけを配列にする。Hook identity、active状態、removedHooksは保存されない。

## 統合

TryRemoveAimedHook（143行）は共有Hookに接する橋2本の外端を取得、長さを合計、旧橋を撤去、1本を生成。ReleaseCenterを呼び、中央HookをremovedHooksに追加してinactiveにする。

## 復元

RestorePlatformStates（220行）→ ClearPlatforms（304行）→ RestoreRemovedHooks（313行）。後者はremovedHooksの各HookをSetActive(true)、リストをClearする。続いて保存済みのStart / End / RopeLengthから橋を生成する。中央Hookを再びinactiveにする処理はこの復元経路にない。

## Checkpoint経路

MainStageRespawnOnFall: TryReachSection（177行）→ CaptureCheckpointState（200行）→ builder.CapturePlatformStates。

RestartFromCheckpoint（220行）→ DetachAndRefund → rope残量/選択復元 → RestorePlatformStates → rail shelf復元 → プレイヤー位置/運動/入力復元。

MainStageCheckpointはOnCollisionStay2D経由。CheckpointLandingGateは上面支持、速度、床内の中心とrespawn位置、0.12秒の安定着地を確認する。

## Authored配置

TutorialCheckpointは2〜4区間限定。第4区間開始床で保存し、2本生成/F統合はその後。T4 goalにはGoalZoneがあり、統合後のcheckpointはない。従ってT4の入口snapshotに戻ることと、統合snapshot復元不整合は別。

MainStageSectionNineSetupは左右(180,-2.15)/(191,-2.15)、中央(185.5,-1.05)、長さ6×2。右岸のMain S09 Goal Floorにcheckpoint10、respawn(192,-1.45)。統合後にcheckpoint10へ着地して保存できる構成。

## Binding

RopeBridgeBindings.ReleaseCenterは2本の一時release線を作り、0.4秒でClearReleases。Start/EndのwrapはEnsureで再生成。PlatformStateにrelease履歴はない。ただし終了済みの一過性演出を再実行しないことだけでは別バグと断定できない。中央Hookの誤復活とは分離して測る。

以上はコード上の事実・疑いであり、Confirmed Causeの判定にはRuntime証拠が必要。
