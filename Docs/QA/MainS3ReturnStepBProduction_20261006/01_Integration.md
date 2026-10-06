# S3 Return Step B 移植範囲

対象は `Main Section 3 Return Step B`。位置(41, -10.775, 0)、回転identity、scale(1.8, 10.55, 1)、BoxCollider2D size(1, 1)、offset(0, 0)、Static Rigidbody2Dを維持します。歩行上面はY=-5.50、幅は1.8のままです。

承認済み `S3ReturnStepBMarkerProofVisual` とShaderから、形状生成・材質・色・FELTラベル・キャップ上面をそのまま移植しています。Shader本文の変更は名前だけです。実測メッシュ頂点・法線・三角形・UVも採用Proofと一致しました。

本番の起動処理は既存のStep A色鉛筆と同じ方式です。MainStageにのみPresentation ownerを1個作り、既存のStep BへMeshFilterとMeshRendererだけの子Visualを追加します。対象名・Transform・Collider・Static Rigidbody2Dが想定と違う場合は旧表示を残します。Tutorialではownerを作りません。

Step Bの旧Rendererだけをmaskし、既存階段のRevealが上昇・fade・非表示状態を管理します。対象床のroot Transformや物理設定をVisualに合わせて変更しません。非表示と再表示、MainStage再ロードでも二重生成しないことを確認しました。

## Productionファイル

- Assets/Scripts/MainS3MarkerVisual.cs
- Assets/Scripts/MainS3MarkerVisual.cs.meta
- Assets/Resources/MainS3Marker.shader
- Assets/Resources/MainS3Marker.shader.meta

新規PNGは不要です。既存の木部テクスチャを採用Proofと同じように使います。metaのGUIDは新規・Assets内一意です。

F7、比較UI、入力注入、診断配置、QA observer、Editor entry、Proof referenceはProductionへ移植していません。これらの診断コードはDocsのEvidenceと既存隔離Project内のみです。

差分は `Evidence/ApprovedToProductionSource.diff` と `Evidence/ApprovedToProductionShader.diff`、スコープ・meta確認は `Evidence/ProductionScope.json` に記録しています。
