# Root Cause — 現行Productionの確認

対象：`Assets/Scripts/TutorialCraftRoomLayers.cs` / `Refresh()`。

修正前の表示判定：

```csharp
bool show = enabled && run.Outcome != PrototypeRunController.RunOutcome.WaitingToStart &&
    !MainStagePreview.IsActive && !StageStartTransition.IsActive;
```

`WaitingToStart`はStage Selection。`MainStagePreview.IsActive`は既存のゴール→スタート紹介カメラ。`StageStartTransition.IsActive`はStage Selection→開始の布演出。

`show`がfalseの場合、新3層・端のguard・floor strip・背景投影影が無効になり、同じメソッド内でlegacyの`forceRenderingOff`が元へ戻る。したがってカメラが動いている間だけ新3層が消え、旧背景が表示される。カメラの移動経路・速度の問題ではない。

## ライフサイクル

1. `PrototypeRunController.Start()` → `EnterStartScreen()`：WaitingToStart。Stage Selectionは従来背景を使用する。
2. `StartSelectedStage()` → `StageStartTransition.Begin()`：Thread/Closing/Coveredの間はWaitingToStartのまま。メニュー背景契約を維持する。
3. `StageStartTransition.CommitStart()` → `CommitTutorialStart()`：OutcomeがPlayingになり、`MainStagePreview.PlayFor()`が開始。カメラはゴールへ移る。布はOpeningに入る。
4. 布が開く間、MainStagePreviewはゴールで待機。布が終了してから既存のhold/panが進む。
5. Preview終了のLateUpdateで操作・カメラ追従を復元。OutcomeはPlayingのまま。
6. Clearは新背景を維持。Stage Selectionへ戻る際はWaitingToStartとなりlegacyを復元。

問題は3〜4〜5の間に追加されていたPreview/Transitionの否定条件。現行Sourceから確認したものであり、前Taskの推測を根拠にはしていない。

関連Sourceは読み取りのみ：PrototypeRunController、StageStartTransition、MainStagePreview。背景のEnsure/OnEnable統合は前Taskのまま。MainStageはTutorial専用Ensureの対象外。
