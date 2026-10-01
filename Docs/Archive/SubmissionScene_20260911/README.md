# Submission Scene Archive — 2026-09-11

このArchiveは、2026-09-11提出時のMainStage Sceneと同一のWorking Tree Sceneを、byte-for-byte保存した制作過程記録です。

- ZIP：`MainStage_submission_20260911.zip`。内部は`MainStage.unity`1件のみ。
- Scene SHA-256：`57D2464751B5B9C2A78E6C56F34197E9BC2629983FADAA1DFF6E0BA0B4BC2284`。
- 現Working Treeと提出用snapshot実ファイル・SHA manifestが一致。
- 比較基準HEAD：`4919233cd1d0815afb438bd35a7336492ec09548`。
- `MainStage_submission_vs_HEAD.patch`は上記HEAD Sceneから保存対象Sceneへの差分。元のSceneを再現する方向はHEAD → submission。
- Runtime authorityは現在のsetup code。`Docs/QA/MainStageSceneDiffAudit_20261001/`と再確認で**CASE S4**。
- HEAD SceneとWorking Tree Sceneはsetup完了後の対象14ノードが一致。
- 同一Camera / resolutionによるVisual A/B 3組は差分0pixel。

これは制作過程保存用です。Production Sceneとして直接使用しないでください。必要な場合はZIPを別の安全な場所へ展開して復元できます。元Sceneへの書戻しは、必要性を確認して明示的な作業として行ってください。

ZIP展開後SHAとpatch・QA Evidence・ファイルサイズの検証は、`Docs/QA/MainStageCleanReproduction_20261001/Evidence/archive_verification.json`に記録します。元Sceneに隣接する.metaはこのZIPに含めず、指定どおりScene1件のみを保存します。
