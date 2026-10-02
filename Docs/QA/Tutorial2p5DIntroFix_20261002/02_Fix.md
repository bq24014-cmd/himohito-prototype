# Fix — 表示条件だけの最小変更

変更Source：`Assets/Scripts/TutorialCraftRoomLayers.cs`の`Refresh()`のみ。

```csharp
bool show = enabled && run.Outcome != PrototypeRunController.RunOutcome.WaitingToStart;
```

Preview中・開始布演出中という理由だけで新背景を隠す2条件を取り除いた。Stage SelectionのWaitingToStart、component無効時、asset欠落時のlegacy fallback、OnDisable/OnDestroyでのlegacy復元は維持。

「常にenabled=true」ではない。メニューは従来背景のみ、Tutorial開始確定後は新3層のみ。開始確定は布が閉じた後に行われるため、その裏で表示を切り替え、Openingから既存Intro終了・Gameplay開始まで新3層を維持する。

RestartTutorialの再イントロでも同じ条件が適用される。Clear・Failedは前回から新背景を維持する契約のまま。

変更しないもの：素材・PNG・Shader・follow rates・Z・背景のTransform計算・Camera・Player・Hook・Platform・Collider・Physics・Rope・Bridge・Tutorial進行・Section・MainStage。

開始前のSourceをEvidence/BeforeProductionへ保存。検証用のBeforeProject/AfterProjectはそれぞれWorking Treeから独立コピー。診断コードとBuild設定は隔離コピーだけに追加する。

既存127項目は前TaskのIntegrationQA.csを改変せずコピーして再実行する。イントロ確認はそれとは別のハーネスで実際のStartSelectedStageとスキップしないPreviewを通す。
