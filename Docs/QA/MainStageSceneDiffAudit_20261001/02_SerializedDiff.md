# Serialized Diff — full per-property table

HEAD vs Working Tree. No production file was edited. Values are extracted from each serialized Unity document; `<ABSENT>` means the entire document/property is absent in HEAD.

Classification: A runtime meaningful; B visual meaningful; C reproduced/overwritten by runtime setup; D Unity schema/default serialization noise; E whitespace only; F unexplained. D on a new document does NOT mean its entire object is noise: its meaningful fields are separately C.

Meaningful-looking structural units: **36** (16 existing property deltas + 20 new documents). Complete property rows: **570**. Whitespace-only rows: **46**. Classifications are source-based and cross-checked against `05_RuntimeAB.md`; not a new Human approval.

| GameObject / hierarchy | fileID | Component type | Script GUID | Property | HEAD | Working Tree | Class |
| --- | --- | --- | --- | --- | --- | --- | --- |
| Main Section 2 Front Spike/Wooden Toy Spike Face | 227514497 | Transform | — | m_LocalScale | {x: 0.096339114, y: 0.10341261, z: 1} | {x: 0.09372071, y: 0.1003009, z: 1} | C |
| Main Section 2 Front Spike/Wooden Toy Spike Face | 227514498 | SpriteRenderer | — | m_Size | {x: 10.38, y: 9.67} | {x: 10.67, y: 9.97} | C |
| Main Section 2 Far Spike/Wooden Toy Spike Face | 722332612 | Transform | — | m_LocalScale | {x: 0.096339114, y: 0.10341261, z: 1} | {x: 0.09372071, y: 0.1003009, z: 1} | C |
| Main Section 2 Far Spike/Wooden Toy Spike Face | 722332613 | SpriteRenderer | — | m_Size | {x: 10.38, y: 9.67} | {x: 10.67, y: 9.97} | C |
| Main S05 Right Shelf/Blue Railway Platform Visual | 943331324 | Transform | — | m_Children | [] | - {fileID: 1947320070}<br>- {fileID: 1012757152}<br>- {fileID: 1301727166} | C |
| Main S05 Right Shelf/Blue Railway Platform Visual | 943331324 | Transform | — | m_LocalPosition | {x: 0, y: 0, z: 0} | {x: 0, y: 0.05, z: 0} | C |
| Main S05 Right Shelf/Blue Railway Platform Visual | 943331324 | Transform | — | m_LocalScale | {x: 0.059101656, y: 0.6802721, z: 1} | {x: 0.33333334, y: 1.6666666, z: 1} | C |
| Main S05 Right Shelf/Blue Railway Platform Visual | 943331325 | SpriteRenderer | — | m_DrawMode | 0 | 2 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual | 943331325 | SpriteRenderer | — | m_Size | {x: 16.92, y: 1.47} | {x: 3, y: 0.54} | C |
| Main S05 Left Shelf | 1417814682 | GameObject | — | m_Component | - component: {fileID: 1417814688}<br>- component: {fileID: 1417814687}<br>- component: {fileID: 1417814686}<br>- component: {fileID: 1417814685}<br>- component: {fileID: 1417814684}<br>- component: {fileID: 1417814691}<br>- component: {fileID: 1417814690}<br>- component: {fileID: 1417814689} | - component: {fileID: 1417814688}<br>- component: {fileID: 1417814687}<br>- component: {fileID: 1417814686}<br>- component: {fileID: 1417814685}<br>- component: {fileID: 1417814684}<br>- component: {fileID: 1417814691}<br>- component: {fileID: 1417814690}<br>- component: {fileID: 1417814689}<br>- component: {fileID: 1417814692} | C |
| Main S05 Left Shelf/Blue Railway Platform Visual | 1639130446 | Transform | — | m_Children | [] | - {fileID: 1489753901}<br>- {fileID: 265655748}<br>- {fileID: 1946492882} | C |
| Main S05 Left Shelf/Blue Railway Platform Visual | 1639130446 | Transform | — | m_LocalPosition | {x: 0, y: 0, z: 0} | {x: 0, y: 0.05, z: 0} | C |
| Main S05 Left Shelf/Blue Railway Platform Visual | 1639130446 | Transform | — | m_LocalScale | {x: 0.059101656, y: 0.6802721, z: 1} | {x: 0.33333334, y: 1.6666666, z: 1} | C |
| Main S05 Left Shelf/Blue Railway Platform Visual | 1639130447 | SpriteRenderer | — | m_DrawMode | 0 | 2 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual | 1639130447 | SpriteRenderer | — | m_Size | {x: 16.92, y: 1.47} | {x: 3, y: 0.54} | C |
| Main S05 Right Shelf | 1829554967 | GameObject | — | m_Component | - component: {fileID: 1829554973}<br>- component: {fileID: 1829554972}<br>- component: {fileID: 1829554971}<br>- component: {fileID: 1829554970}<br>- component: {fileID: 1829554969}<br>- component: {fileID: 1829554976}<br>- component: {fileID: 1829554975}<br>- component: {fileID: 1829554974} | - component: {fileID: 1829554973}<br>- component: {fileID: 1829554972}<br>- component: {fileID: 1829554971}<br>- component: {fileID: 1829554970}<br>- component: {fileID: 1829554969}<br>- component: {fileID: 1829554976}<br>- component: {fileID: 1829554975}<br>- component: {fileID: 1829554974}<br>- component: {fileID: 1829554977} | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1489753902 | SpriteRenderer | — | m_AdaptiveModeThreshold | <ABSENT> | 0.5 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1489753902 | SpriteRenderer | — | m_AutoUVMaxAngle | <ABSENT> | 89 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1489753902 | SpriteRenderer | — | m_AutoUVMaxDistance | <ABSENT> | 0.5 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1489753902 | SpriteRenderer | — | m_CastShadows | <ABSENT> | 0 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1489753902 | SpriteRenderer | — | m_Color | <ABSENT> | {r: 1, g: 1, b: 1, a: 1} | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1489753902 | SpriteRenderer | — | m_CorrespondingSourceObject | <ABSENT> | {fileID: 0} | D |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1489753902 | SpriteRenderer | — | m_DrawMode | <ABSENT> | 0 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1489753902 | SpriteRenderer | — | m_DynamicOccludee | <ABSENT> | 1 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1489753902 | SpriteRenderer | — | m_Enabled | <ABSENT> | 1 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1489753902 | SpriteRenderer | — | m_FlipX | <ABSENT> | 0 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1489753902 | SpriteRenderer | — | m_FlipY | <ABSENT> | 0 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1489753902 | SpriteRenderer | — | m_ForceMeshLod | <ABSENT> | -1 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1489753902 | SpriteRenderer | — | m_GameObject | <ABSENT> | {fileID: 1489753900} | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1489753902 | SpriteRenderer | — | m_GlobalIlluminationMeshLod | <ABSENT> | 0 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1489753902 | SpriteRenderer | — | m_IgnoreNormalsForChartDetection | <ABSENT> | 0 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1489753902 | SpriteRenderer | — | m_ImportantGI | <ABSENT> | 0 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1489753902 | SpriteRenderer | — | m_LightProbeUsage | <ABSENT> | 1 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1489753902 | SpriteRenderer | — | m_LightProbeVolumeOverride | <ABSENT> | {fileID: 0} | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1489753902 | SpriteRenderer | — | m_LightmapParameters | <ABSENT> | {fileID: 0} | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1489753902 | SpriteRenderer | — | m_MaskInteraction | <ABSENT> | 0 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1489753902 | SpriteRenderer | — | m_Materials | <ABSENT> | - {fileID: 10754, guid: 0000000000000000f000000000000000, type: 0} | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1489753902 | SpriteRenderer | — | m_MeshLodSelectionBias | <ABSENT> | 0 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1489753902 | SpriteRenderer | — | m_MinimumChartSize | <ABSENT> | 4 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1489753902 | SpriteRenderer | — | m_MotionVectors | <ABSENT> | 1 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1489753902 | SpriteRenderer | — | m_ObjectHideFlags | <ABSENT> | 0 | D |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1489753902 | SpriteRenderer | — | m_PrefabAsset | <ABSENT> | {fileID: 0} | D |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1489753902 | SpriteRenderer | — | m_PrefabInstance | <ABSENT> | {fileID: 0} | D |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1489753902 | SpriteRenderer | — | m_PreserveUVs | <ABSENT> | 0 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1489753902 | SpriteRenderer | — | m_ProbeAnchor | <ABSENT> | {fileID: 0} | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1489753902 | SpriteRenderer | — | m_RayTraceProcedural | <ABSENT> | 0 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1489753902 | SpriteRenderer | — | m_RayTracingAccelStructBuildFlags | <ABSENT> | 1 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1489753902 | SpriteRenderer | — | m_RayTracingAccelStructBuildFlagsOverride | <ABSENT> | 0 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1489753902 | SpriteRenderer | — | m_RayTracingMode | <ABSENT> | 0 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1489753902 | SpriteRenderer | — | m_ReceiveGI | <ABSENT> | 1 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1489753902 | SpriteRenderer | — | m_ReceiveShadows | <ABSENT> | 0 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1489753902 | SpriteRenderer | — | m_ReflectionProbeUsage | <ABSENT> | 1 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1489753902 | SpriteRenderer | — | m_RendererPriority | <ABSENT> | 0 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1489753902 | SpriteRenderer | — | m_RenderingLayerMask | <ABSENT> | 1 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1489753902 | SpriteRenderer | — | m_ScaleInLightmap | <ABSENT> | 1 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1489753902 | SpriteRenderer | — | m_SelectedEditorRenderState | <ABSENT> | 0 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1489753902 | SpriteRenderer | — | m_Size | <ABSENT> | {x: 0.28147542, y: 0.34} | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1489753902 | SpriteRenderer | — | m_SmallMeshCulling | <ABSENT> | 1 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1489753902 | SpriteRenderer | — | m_SortingLayer | <ABSENT> | 0 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1489753902 | SpriteRenderer | — | m_SortingLayerID | <ABSENT> | 0 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1489753902 | SpriteRenderer | — | m_SortingOrder | <ABSENT> | 2 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1489753902 | SpriteRenderer | — | m_Sprite | <ABSENT> | {fileID: 0} | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1489753902 | SpriteRenderer | — | m_SpriteSortPoint | <ABSENT> | 0 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1489753902 | SpriteRenderer | — | m_SpriteTileMode | <ABSENT> | 0 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1489753902 | SpriteRenderer | — | m_StaticBatchInfo.firstSubMesh | <ABSENT> | 0 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1489753902 | SpriteRenderer | — | m_StaticBatchInfo.subMeshCount | <ABSENT> | 0 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1489753902 | SpriteRenderer | — | m_StaticBatchRoot | <ABSENT> | {fileID: 0} | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1489753902 | SpriteRenderer | — | m_StaticShadowCaster | <ABSENT> | 0 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1489753902 | SpriteRenderer | — | m_StitchLightmapSeams | <ABSENT> | 1 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1489753902 | SpriteRenderer | — | m_WasSpriteAssigned | <ABSENT> | 1 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1489753902 | SpriteRenderer | — | serializedVersion | <ABSENT> | 2 | D |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1946492883 | SpriteRenderer | — | m_AdaptiveModeThreshold | <ABSENT> | 0.5 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1946492883 | SpriteRenderer | — | m_AutoUVMaxAngle | <ABSENT> | 89 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1946492883 | SpriteRenderer | — | m_AutoUVMaxDistance | <ABSENT> | 0.5 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1946492883 | SpriteRenderer | — | m_CastShadows | <ABSENT> | 0 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1946492883 | SpriteRenderer | — | m_Color | <ABSENT> | {r: 1, g: 1, b: 1, a: 1} | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1946492883 | SpriteRenderer | — | m_CorrespondingSourceObject | <ABSENT> | {fileID: 0} | D |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1946492883 | SpriteRenderer | — | m_DrawMode | <ABSENT> | 0 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1946492883 | SpriteRenderer | — | m_DynamicOccludee | <ABSENT> | 1 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1946492883 | SpriteRenderer | — | m_Enabled | <ABSENT> | 1 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1946492883 | SpriteRenderer | — | m_FlipX | <ABSENT> | 0 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1946492883 | SpriteRenderer | — | m_FlipY | <ABSENT> | 0 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1946492883 | SpriteRenderer | — | m_ForceMeshLod | <ABSENT> | -1 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1946492883 | SpriteRenderer | — | m_GameObject | <ABSENT> | {fileID: 1946492881} | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1946492883 | SpriteRenderer | — | m_GlobalIlluminationMeshLod | <ABSENT> | 0 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1946492883 | SpriteRenderer | — | m_IgnoreNormalsForChartDetection | <ABSENT> | 0 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1946492883 | SpriteRenderer | — | m_ImportantGI | <ABSENT> | 0 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1946492883 | SpriteRenderer | — | m_LightProbeUsage | <ABSENT> | 1 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1946492883 | SpriteRenderer | — | m_LightProbeVolumeOverride | <ABSENT> | {fileID: 0} | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1946492883 | SpriteRenderer | — | m_LightmapParameters | <ABSENT> | {fileID: 0} | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1946492883 | SpriteRenderer | — | m_MaskInteraction | <ABSENT> | 0 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1946492883 | SpriteRenderer | — | m_Materials | <ABSENT> | - {fileID: 10754, guid: 0000000000000000f000000000000000, type: 0} | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1946492883 | SpriteRenderer | — | m_MeshLodSelectionBias | <ABSENT> | 0 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1946492883 | SpriteRenderer | — | m_MinimumChartSize | <ABSENT> | 4 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1946492883 | SpriteRenderer | — | m_MotionVectors | <ABSENT> | 1 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1946492883 | SpriteRenderer | — | m_ObjectHideFlags | <ABSENT> | 0 | D |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1946492883 | SpriteRenderer | — | m_PrefabAsset | <ABSENT> | {fileID: 0} | D |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1946492883 | SpriteRenderer | — | m_PrefabInstance | <ABSENT> | {fileID: 0} | D |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1946492883 | SpriteRenderer | — | m_PreserveUVs | <ABSENT> | 0 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1946492883 | SpriteRenderer | — | m_ProbeAnchor | <ABSENT> | {fileID: 0} | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1946492883 | SpriteRenderer | — | m_RayTraceProcedural | <ABSENT> | 0 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1946492883 | SpriteRenderer | — | m_RayTracingAccelStructBuildFlags | <ABSENT> | 1 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1946492883 | SpriteRenderer | — | m_RayTracingAccelStructBuildFlagsOverride | <ABSENT> | 0 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1946492883 | SpriteRenderer | — | m_RayTracingMode | <ABSENT> | 0 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1946492883 | SpriteRenderer | — | m_ReceiveGI | <ABSENT> | 1 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1946492883 | SpriteRenderer | — | m_ReceiveShadows | <ABSENT> | 0 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1946492883 | SpriteRenderer | — | m_ReflectionProbeUsage | <ABSENT> | 1 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1946492883 | SpriteRenderer | — | m_RendererPriority | <ABSENT> | 0 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1946492883 | SpriteRenderer | — | m_RenderingLayerMask | <ABSENT> | 1 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1946492883 | SpriteRenderer | — | m_ScaleInLightmap | <ABSENT> | 1 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1946492883 | SpriteRenderer | — | m_SelectedEditorRenderState | <ABSENT> | 0 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1946492883 | SpriteRenderer | — | m_Size | <ABSENT> | {x: 0.28147542, y: 0.34} | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1946492883 | SpriteRenderer | — | m_SmallMeshCulling | <ABSENT> | 1 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1946492883 | SpriteRenderer | — | m_SortingLayer | <ABSENT> | 0 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1946492883 | SpriteRenderer | — | m_SortingLayerID | <ABSENT> | 0 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1946492883 | SpriteRenderer | — | m_SortingOrder | <ABSENT> | 2 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1946492883 | SpriteRenderer | — | m_Sprite | <ABSENT> | {fileID: 0} | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1946492883 | SpriteRenderer | — | m_SpriteSortPoint | <ABSENT> | 0 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1946492883 | SpriteRenderer | — | m_SpriteTileMode | <ABSENT> | 0 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1946492883 | SpriteRenderer | — | m_StaticBatchInfo.firstSubMesh | <ABSENT> | 0 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1946492883 | SpriteRenderer | — | m_StaticBatchInfo.subMeshCount | <ABSENT> | 0 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1946492883 | SpriteRenderer | — | m_StaticBatchRoot | <ABSENT> | {fileID: 0} | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1946492883 | SpriteRenderer | — | m_StaticShadowCaster | <ABSENT> | 0 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1946492883 | SpriteRenderer | — | m_StitchLightmapSeams | <ABSENT> | 1 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1946492883 | SpriteRenderer | — | m_WasSpriteAssigned | <ABSENT> | 1 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1946492883 | SpriteRenderer | — | serializedVersion | <ABSENT> | 2 | D |
| Main S05 Right Shelf | 1829554977 | MonoBehaviour / CraftRailPlatformVisual.cs | 4f2bc7b1f75245d0a5983a4a7150e66c | allowTutorialLegacy | <ABSENT> | 0 | C |
| Main S05 Right Shelf | 1829554977 | MonoBehaviour / CraftRailPlatformVisual.cs | 4f2bc7b1f75245d0a5983a4a7150e66c | m_CorrespondingSourceObject | <ABSENT> | {fileID: 0} | D |
| Main S05 Right Shelf | 1829554977 | MonoBehaviour / CraftRailPlatformVisual.cs | 4f2bc7b1f75245d0a5983a4a7150e66c | m_EditorClassIdentifier | <ABSENT> | Assembly-CSharp::HimoHito.CraftRailPlatformVisual | D |
| Main S05 Right Shelf | 1829554977 | MonoBehaviour / CraftRailPlatformVisual.cs | 4f2bc7b1f75245d0a5983a4a7150e66c | m_EditorHideFlags | <ABSENT> | 0 | D |
| Main S05 Right Shelf | 1829554977 | MonoBehaviour / CraftRailPlatformVisual.cs | 4f2bc7b1f75245d0a5983a4a7150e66c | m_Enabled | <ABSENT> | 1 | C |
| Main S05 Right Shelf | 1829554977 | MonoBehaviour / CraftRailPlatformVisual.cs | 4f2bc7b1f75245d0a5983a4a7150e66c | m_GameObject | <ABSENT> | {fileID: 1829554967} | C |
| Main S05 Right Shelf | 1829554977 | MonoBehaviour / CraftRailPlatformVisual.cs | 4f2bc7b1f75245d0a5983a4a7150e66c | m_Name | <ABSENT> |  | D |
| Main S05 Right Shelf | 1829554977 | MonoBehaviour / CraftRailPlatformVisual.cs | 4f2bc7b1f75245d0a5983a4a7150e66c | m_ObjectHideFlags | <ABSENT> | 0 | D |
| Main S05 Right Shelf | 1829554977 | MonoBehaviour / CraftRailPlatformVisual.cs | 4f2bc7b1f75245d0a5983a4a7150e66c | m_PrefabAsset | <ABSENT> | {fileID: 0} | D |
| Main S05 Right Shelf | 1829554977 | MonoBehaviour / CraftRailPlatformVisual.cs | 4f2bc7b1f75245d0a5983a4a7150e66c | m_PrefabInstance | <ABSENT> | {fileID: 0} | D |
| Main S05 Right Shelf | 1829554977 | MonoBehaviour / CraftRailPlatformVisual.cs | 4f2bc7b1f75245d0a5983a4a7150e66c | m_Script | <ABSENT> | {fileID: 11500000, guid: 4f2bc7b1f75245d0a5983a4a7150e66c, type: 3} | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1489753901 | Transform | — | m_Children | <ABSENT> | [] | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1489753901 | Transform | — | m_ConstrainProportionsScale | <ABSENT> | 0 | D |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1489753901 | Transform | — | m_CorrespondingSourceObject | <ABSENT> | {fileID: 0} | D |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1489753901 | Transform | — | m_Father | <ABSENT> | {fileID: 1639130446} | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1489753901 | Transform | — | m_GameObject | <ABSENT> | {fileID: 1489753900} | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1489753901 | Transform | — | m_LocalEulerAnglesHint | <ABSENT> | {x: 0, y: 0, z: 0} | D |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1489753901 | Transform | — | m_LocalPosition | <ABSENT> | {x: -1, y: -0.33, z: 0} | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1489753901 | Transform | — | m_LocalRotation | <ABSENT> | {x: 0, y: 0, z: 0, w: 1} | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1489753901 | Transform | — | m_LocalScale | <ABSENT> | {x: 1, y: 1, z: 1} | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1489753901 | Transform | — | m_ObjectHideFlags | <ABSENT> | 0 | D |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1489753901 | Transform | — | m_PrefabAsset | <ABSENT> | {fileID: 0} | D |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1489753901 | Transform | — | m_PrefabInstance | <ABSENT> | {fileID: 0} | D |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1489753901 | Transform | — | serializedVersion | <ABSENT> | 2 | D |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1947320070 | Transform | — | m_Children | <ABSENT> | [] | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1947320070 | Transform | — | m_ConstrainProportionsScale | <ABSENT> | 0 | D |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1947320070 | Transform | — | m_CorrespondingSourceObject | <ABSENT> | {fileID: 0} | D |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1947320070 | Transform | — | m_Father | <ABSENT> | {fileID: 943331324} | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1947320070 | Transform | — | m_GameObject | <ABSENT> | {fileID: 1947320069} | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1947320070 | Transform | — | m_LocalEulerAnglesHint | <ABSENT> | {x: 0, y: 0, z: 0} | D |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1947320070 | Transform | — | m_LocalPosition | <ABSENT> | {x: -1, y: -0.33, z: 0} | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1947320070 | Transform | — | m_LocalRotation | <ABSENT> | {x: 0, y: 0, z: 0, w: 1} | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1947320070 | Transform | — | m_LocalScale | <ABSENT> | {x: 1, y: 1, z: 1} | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1947320070 | Transform | — | m_ObjectHideFlags | <ABSENT> | 0 | D |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1947320070 | Transform | — | m_PrefabAsset | <ABSENT> | {fileID: 0} | D |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1947320070 | Transform | — | m_PrefabInstance | <ABSENT> | {fileID: 0} | D |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1947320070 | Transform | — | serializedVersion | <ABSENT> | 2 | D |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1947320069 | GameObject | — | m_Component | <ABSENT> | - component: {fileID: 1947320070}<br>- component: {fileID: 1947320071} | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1947320069 | GameObject | — | m_CorrespondingSourceObject | <ABSENT> | {fileID: 0} | D |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1947320069 | GameObject | — | m_Icon | <ABSENT> | {fileID: 0} | D |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1947320069 | GameObject | — | m_IsActive | <ABSENT> | 1 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1947320069 | GameObject | — | m_Layer | <ABSENT> | 0 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1947320069 | GameObject | — | m_Name | <ABSENT> | Craft Rail Tie 0 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1947320069 | GameObject | — | m_NavMeshLayer | <ABSENT> | 0 | D |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1947320069 | GameObject | — | m_ObjectHideFlags | <ABSENT> | 0 | D |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1947320069 | GameObject | — | m_PrefabAsset | <ABSENT> | {fileID: 0} | D |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1947320069 | GameObject | — | m_PrefabInstance | <ABSENT> | {fileID: 0} | D |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1947320069 | GameObject | — | m_StaticEditorFlags | <ABSENT> | 0 | D |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1947320069 | GameObject | — | m_TagString | <ABSENT> | Untagged | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1947320069 | GameObject | — | serializedVersion | <ABSENT> | 6 | D |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1946492882 | Transform | — | m_Children | <ABSENT> | [] | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1946492882 | Transform | — | m_ConstrainProportionsScale | <ABSENT> | 0 | D |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1946492882 | Transform | — | m_CorrespondingSourceObject | <ABSENT> | {fileID: 0} | D |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1946492882 | Transform | — | m_Father | <ABSENT> | {fileID: 1639130446} | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1946492882 | Transform | — | m_GameObject | <ABSENT> | {fileID: 1946492881} | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1946492882 | Transform | — | m_LocalEulerAnglesHint | <ABSENT> | {x: 0, y: 0, z: 0} | D |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1946492882 | Transform | — | m_LocalPosition | <ABSENT> | {x: 1, y: -0.33, z: 0} | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1946492882 | Transform | — | m_LocalRotation | <ABSENT> | {x: 0, y: 0, z: 0, w: 1} | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1946492882 | Transform | — | m_LocalScale | <ABSENT> | {x: 1, y: 1, z: 1} | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1946492882 | Transform | — | m_ObjectHideFlags | <ABSENT> | 0 | D |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1946492882 | Transform | — | m_PrefabAsset | <ABSENT> | {fileID: 0} | D |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1946492882 | Transform | — | m_PrefabInstance | <ABSENT> | {fileID: 0} | D |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1946492882 | Transform | — | serializedVersion | <ABSENT> | 2 | D |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 265655748 | Transform | — | m_Children | <ABSENT> | [] | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 265655748 | Transform | — | m_ConstrainProportionsScale | <ABSENT> | 0 | D |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 265655748 | Transform | — | m_CorrespondingSourceObject | <ABSENT> | {fileID: 0} | D |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 265655748 | Transform | — | m_Father | <ABSENT> | {fileID: 1639130446} | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 265655748 | Transform | — | m_GameObject | <ABSENT> | {fileID: 265655747} | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 265655748 | Transform | — | m_LocalEulerAnglesHint | <ABSENT> | {x: 0, y: 0, z: 0} | D |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 265655748 | Transform | — | m_LocalPosition | <ABSENT> | {x: 0, y: -0.33, z: 0} | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 265655748 | Transform | — | m_LocalRotation | <ABSENT> | {x: 0, y: 0, z: 0, w: 1} | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 265655748 | Transform | — | m_LocalScale | <ABSENT> | {x: 1, y: 1, z: 1} | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 265655748 | Transform | — | m_ObjectHideFlags | <ABSENT> | 0 | D |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 265655748 | Transform | — | m_PrefabAsset | <ABSENT> | {fileID: 0} | D |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 265655748 | Transform | — | m_PrefabInstance | <ABSENT> | {fileID: 0} | D |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 265655748 | Transform | — | serializedVersion | <ABSENT> | 2 | D |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1489753900 | GameObject | — | m_Component | <ABSENT> | - component: {fileID: 1489753901}<br>- component: {fileID: 1489753902} | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1489753900 | GameObject | — | m_CorrespondingSourceObject | <ABSENT> | {fileID: 0} | D |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1489753900 | GameObject | — | m_Icon | <ABSENT> | {fileID: 0} | D |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1489753900 | GameObject | — | m_IsActive | <ABSENT> | 1 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1489753900 | GameObject | — | m_Layer | <ABSENT> | 0 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1489753900 | GameObject | — | m_Name | <ABSENT> | Craft Rail Tie 0 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1489753900 | GameObject | — | m_NavMeshLayer | <ABSENT> | 0 | D |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1489753900 | GameObject | — | m_ObjectHideFlags | <ABSENT> | 0 | D |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1489753900 | GameObject | — | m_PrefabAsset | <ABSENT> | {fileID: 0} | D |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1489753900 | GameObject | — | m_PrefabInstance | <ABSENT> | {fileID: 0} | D |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1489753900 | GameObject | — | m_StaticEditorFlags | <ABSENT> | 0 | D |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1489753900 | GameObject | — | m_TagString | <ABSENT> | Untagged | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1489753900 | GameObject | — | serializedVersion | <ABSENT> | 6 | D |
| Main S05 Left Shelf | 1417814692 | MonoBehaviour / CraftRailPlatformVisual.cs | 4f2bc7b1f75245d0a5983a4a7150e66c | allowTutorialLegacy | <ABSENT> | 0 | C |
| Main S05 Left Shelf | 1417814692 | MonoBehaviour / CraftRailPlatformVisual.cs | 4f2bc7b1f75245d0a5983a4a7150e66c | m_CorrespondingSourceObject | <ABSENT> | {fileID: 0} | D |
| Main S05 Left Shelf | 1417814692 | MonoBehaviour / CraftRailPlatformVisual.cs | 4f2bc7b1f75245d0a5983a4a7150e66c | m_EditorClassIdentifier | <ABSENT> | Assembly-CSharp::HimoHito.CraftRailPlatformVisual | D |
| Main S05 Left Shelf | 1417814692 | MonoBehaviour / CraftRailPlatformVisual.cs | 4f2bc7b1f75245d0a5983a4a7150e66c | m_EditorHideFlags | <ABSENT> | 0 | D |
| Main S05 Left Shelf | 1417814692 | MonoBehaviour / CraftRailPlatformVisual.cs | 4f2bc7b1f75245d0a5983a4a7150e66c | m_Enabled | <ABSENT> | 1 | C |
| Main S05 Left Shelf | 1417814692 | MonoBehaviour / CraftRailPlatformVisual.cs | 4f2bc7b1f75245d0a5983a4a7150e66c | m_GameObject | <ABSENT> | {fileID: 1417814682} | C |
| Main S05 Left Shelf | 1417814692 | MonoBehaviour / CraftRailPlatformVisual.cs | 4f2bc7b1f75245d0a5983a4a7150e66c | m_Name | <ABSENT> |  | D |
| Main S05 Left Shelf | 1417814692 | MonoBehaviour / CraftRailPlatformVisual.cs | 4f2bc7b1f75245d0a5983a4a7150e66c | m_ObjectHideFlags | <ABSENT> | 0 | D |
| Main S05 Left Shelf | 1417814692 | MonoBehaviour / CraftRailPlatformVisual.cs | 4f2bc7b1f75245d0a5983a4a7150e66c | m_PrefabAsset | <ABSENT> | {fileID: 0} | D |
| Main S05 Left Shelf | 1417814692 | MonoBehaviour / CraftRailPlatformVisual.cs | 4f2bc7b1f75245d0a5983a4a7150e66c | m_PrefabInstance | <ABSENT> | {fileID: 0} | D |
| Main S05 Left Shelf | 1417814692 | MonoBehaviour / CraftRailPlatformVisual.cs | 4f2bc7b1f75245d0a5983a4a7150e66c | m_Script | <ABSENT> | {fileID: 11500000, guid: 4f2bc7b1f75245d0a5983a4a7150e66c, type: 3} | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 265655749 | SpriteRenderer | — | m_AdaptiveModeThreshold | <ABSENT> | 0.5 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 265655749 | SpriteRenderer | — | m_AutoUVMaxAngle | <ABSENT> | 89 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 265655749 | SpriteRenderer | — | m_AutoUVMaxDistance | <ABSENT> | 0.5 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 265655749 | SpriteRenderer | — | m_CastShadows | <ABSENT> | 0 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 265655749 | SpriteRenderer | — | m_Color | <ABSENT> | {r: 1, g: 1, b: 1, a: 1} | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 265655749 | SpriteRenderer | — | m_CorrespondingSourceObject | <ABSENT> | {fileID: 0} | D |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 265655749 | SpriteRenderer | — | m_DrawMode | <ABSENT> | 0 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 265655749 | SpriteRenderer | — | m_DynamicOccludee | <ABSENT> | 1 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 265655749 | SpriteRenderer | — | m_Enabled | <ABSENT> | 1 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 265655749 | SpriteRenderer | — | m_FlipX | <ABSENT> | 0 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 265655749 | SpriteRenderer | — | m_FlipY | <ABSENT> | 0 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 265655749 | SpriteRenderer | — | m_ForceMeshLod | <ABSENT> | -1 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 265655749 | SpriteRenderer | — | m_GameObject | <ABSENT> | {fileID: 265655747} | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 265655749 | SpriteRenderer | — | m_GlobalIlluminationMeshLod | <ABSENT> | 0 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 265655749 | SpriteRenderer | — | m_IgnoreNormalsForChartDetection | <ABSENT> | 0 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 265655749 | SpriteRenderer | — | m_ImportantGI | <ABSENT> | 0 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 265655749 | SpriteRenderer | — | m_LightProbeUsage | <ABSENT> | 1 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 265655749 | SpriteRenderer | — | m_LightProbeVolumeOverride | <ABSENT> | {fileID: 0} | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 265655749 | SpriteRenderer | — | m_LightmapParameters | <ABSENT> | {fileID: 0} | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 265655749 | SpriteRenderer | — | m_MaskInteraction | <ABSENT> | 0 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 265655749 | SpriteRenderer | — | m_Materials | <ABSENT> | - {fileID: 10754, guid: 0000000000000000f000000000000000, type: 0} | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 265655749 | SpriteRenderer | — | m_MeshLodSelectionBias | <ABSENT> | 0 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 265655749 | SpriteRenderer | — | m_MinimumChartSize | <ABSENT> | 4 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 265655749 | SpriteRenderer | — | m_MotionVectors | <ABSENT> | 1 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 265655749 | SpriteRenderer | — | m_ObjectHideFlags | <ABSENT> | 0 | D |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 265655749 | SpriteRenderer | — | m_PrefabAsset | <ABSENT> | {fileID: 0} | D |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 265655749 | SpriteRenderer | — | m_PrefabInstance | <ABSENT> | {fileID: 0} | D |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 265655749 | SpriteRenderer | — | m_PreserveUVs | <ABSENT> | 0 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 265655749 | SpriteRenderer | — | m_ProbeAnchor | <ABSENT> | {fileID: 0} | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 265655749 | SpriteRenderer | — | m_RayTraceProcedural | <ABSENT> | 0 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 265655749 | SpriteRenderer | — | m_RayTracingAccelStructBuildFlags | <ABSENT> | 1 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 265655749 | SpriteRenderer | — | m_RayTracingAccelStructBuildFlagsOverride | <ABSENT> | 0 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 265655749 | SpriteRenderer | — | m_RayTracingMode | <ABSENT> | 0 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 265655749 | SpriteRenderer | — | m_ReceiveGI | <ABSENT> | 1 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 265655749 | SpriteRenderer | — | m_ReceiveShadows | <ABSENT> | 0 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 265655749 | SpriteRenderer | — | m_ReflectionProbeUsage | <ABSENT> | 1 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 265655749 | SpriteRenderer | — | m_RendererPriority | <ABSENT> | 0 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 265655749 | SpriteRenderer | — | m_RenderingLayerMask | <ABSENT> | 1 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 265655749 | SpriteRenderer | — | m_ScaleInLightmap | <ABSENT> | 1 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 265655749 | SpriteRenderer | — | m_SelectedEditorRenderState | <ABSENT> | 0 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 265655749 | SpriteRenderer | — | m_Size | <ABSENT> | {x: 0.28147542, y: 0.34} | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 265655749 | SpriteRenderer | — | m_SmallMeshCulling | <ABSENT> | 1 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 265655749 | SpriteRenderer | — | m_SortingLayer | <ABSENT> | 0 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 265655749 | SpriteRenderer | — | m_SortingLayerID | <ABSENT> | 0 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 265655749 | SpriteRenderer | — | m_SortingOrder | <ABSENT> | 2 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 265655749 | SpriteRenderer | — | m_Sprite | <ABSENT> | {fileID: 0} | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 265655749 | SpriteRenderer | — | m_SpriteSortPoint | <ABSENT> | 0 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 265655749 | SpriteRenderer | — | m_SpriteTileMode | <ABSENT> | 0 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 265655749 | SpriteRenderer | — | m_StaticBatchInfo.firstSubMesh | <ABSENT> | 0 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 265655749 | SpriteRenderer | — | m_StaticBatchInfo.subMeshCount | <ABSENT> | 0 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 265655749 | SpriteRenderer | — | m_StaticBatchRoot | <ABSENT> | {fileID: 0} | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 265655749 | SpriteRenderer | — | m_StaticShadowCaster | <ABSENT> | 0 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 265655749 | SpriteRenderer | — | m_StitchLightmapSeams | <ABSENT> | 1 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 265655749 | SpriteRenderer | — | m_WasSpriteAssigned | <ABSENT> | 1 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 265655749 | SpriteRenderer | — | serializedVersion | <ABSENT> | 2 | D |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1301727167 | SpriteRenderer | — | m_AdaptiveModeThreshold | <ABSENT> | 0.5 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1301727167 | SpriteRenderer | — | m_AutoUVMaxAngle | <ABSENT> | 89 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1301727167 | SpriteRenderer | — | m_AutoUVMaxDistance | <ABSENT> | 0.5 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1301727167 | SpriteRenderer | — | m_CastShadows | <ABSENT> | 0 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1301727167 | SpriteRenderer | — | m_Color | <ABSENT> | {r: 1, g: 1, b: 1, a: 1} | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1301727167 | SpriteRenderer | — | m_CorrespondingSourceObject | <ABSENT> | {fileID: 0} | D |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1301727167 | SpriteRenderer | — | m_DrawMode | <ABSENT> | 0 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1301727167 | SpriteRenderer | — | m_DynamicOccludee | <ABSENT> | 1 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1301727167 | SpriteRenderer | — | m_Enabled | <ABSENT> | 1 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1301727167 | SpriteRenderer | — | m_FlipX | <ABSENT> | 0 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1301727167 | SpriteRenderer | — | m_FlipY | <ABSENT> | 0 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1301727167 | SpriteRenderer | — | m_ForceMeshLod | <ABSENT> | -1 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1301727167 | SpriteRenderer | — | m_GameObject | <ABSENT> | {fileID: 1301727165} | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1301727167 | SpriteRenderer | — | m_GlobalIlluminationMeshLod | <ABSENT> | 0 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1301727167 | SpriteRenderer | — | m_IgnoreNormalsForChartDetection | <ABSENT> | 0 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1301727167 | SpriteRenderer | — | m_ImportantGI | <ABSENT> | 0 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1301727167 | SpriteRenderer | — | m_LightProbeUsage | <ABSENT> | 1 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1301727167 | SpriteRenderer | — | m_LightProbeVolumeOverride | <ABSENT> | {fileID: 0} | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1301727167 | SpriteRenderer | — | m_LightmapParameters | <ABSENT> | {fileID: 0} | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1301727167 | SpriteRenderer | — | m_MaskInteraction | <ABSENT> | 0 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1301727167 | SpriteRenderer | — | m_Materials | <ABSENT> | - {fileID: 10754, guid: 0000000000000000f000000000000000, type: 0} | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1301727167 | SpriteRenderer | — | m_MeshLodSelectionBias | <ABSENT> | 0 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1301727167 | SpriteRenderer | — | m_MinimumChartSize | <ABSENT> | 4 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1301727167 | SpriteRenderer | — | m_MotionVectors | <ABSENT> | 1 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1301727167 | SpriteRenderer | — | m_ObjectHideFlags | <ABSENT> | 0 | D |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1301727167 | SpriteRenderer | — | m_PrefabAsset | <ABSENT> | {fileID: 0} | D |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1301727167 | SpriteRenderer | — | m_PrefabInstance | <ABSENT> | {fileID: 0} | D |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1301727167 | SpriteRenderer | — | m_PreserveUVs | <ABSENT> | 0 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1301727167 | SpriteRenderer | — | m_ProbeAnchor | <ABSENT> | {fileID: 0} | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1301727167 | SpriteRenderer | — | m_RayTraceProcedural | <ABSENT> | 0 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1301727167 | SpriteRenderer | — | m_RayTracingAccelStructBuildFlags | <ABSENT> | 1 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1301727167 | SpriteRenderer | — | m_RayTracingAccelStructBuildFlagsOverride | <ABSENT> | 0 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1301727167 | SpriteRenderer | — | m_RayTracingMode | <ABSENT> | 0 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1301727167 | SpriteRenderer | — | m_ReceiveGI | <ABSENT> | 1 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1301727167 | SpriteRenderer | — | m_ReceiveShadows | <ABSENT> | 0 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1301727167 | SpriteRenderer | — | m_ReflectionProbeUsage | <ABSENT> | 1 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1301727167 | SpriteRenderer | — | m_RendererPriority | <ABSENT> | 0 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1301727167 | SpriteRenderer | — | m_RenderingLayerMask | <ABSENT> | 1 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1301727167 | SpriteRenderer | — | m_ScaleInLightmap | <ABSENT> | 1 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1301727167 | SpriteRenderer | — | m_SelectedEditorRenderState | <ABSENT> | 0 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1301727167 | SpriteRenderer | — | m_Size | <ABSENT> | {x: 0.28147542, y: 0.34} | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1301727167 | SpriteRenderer | — | m_SmallMeshCulling | <ABSENT> | 1 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1301727167 | SpriteRenderer | — | m_SortingLayer | <ABSENT> | 0 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1301727167 | SpriteRenderer | — | m_SortingLayerID | <ABSENT> | 0 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1301727167 | SpriteRenderer | — | m_SortingOrder | <ABSENT> | 2 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1301727167 | SpriteRenderer | — | m_Sprite | <ABSENT> | {fileID: 0} | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1301727167 | SpriteRenderer | — | m_SpriteSortPoint | <ABSENT> | 0 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1301727167 | SpriteRenderer | — | m_SpriteTileMode | <ABSENT> | 0 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1301727167 | SpriteRenderer | — | m_StaticBatchInfo.firstSubMesh | <ABSENT> | 0 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1301727167 | SpriteRenderer | — | m_StaticBatchInfo.subMeshCount | <ABSENT> | 0 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1301727167 | SpriteRenderer | — | m_StaticBatchRoot | <ABSENT> | {fileID: 0} | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1301727167 | SpriteRenderer | — | m_StaticShadowCaster | <ABSENT> | 0 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1301727167 | SpriteRenderer | — | m_StitchLightmapSeams | <ABSENT> | 1 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1301727167 | SpriteRenderer | — | m_WasSpriteAssigned | <ABSENT> | 1 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1301727167 | SpriteRenderer | — | serializedVersion | <ABSENT> | 2 | D |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1301727165 | GameObject | — | m_Component | <ABSENT> | - component: {fileID: 1301727166}<br>- component: {fileID: 1301727167} | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1301727165 | GameObject | — | m_CorrespondingSourceObject | <ABSENT> | {fileID: 0} | D |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1301727165 | GameObject | — | m_Icon | <ABSENT> | {fileID: 0} | D |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1301727165 | GameObject | — | m_IsActive | <ABSENT> | 1 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1301727165 | GameObject | — | m_Layer | <ABSENT> | 0 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1301727165 | GameObject | — | m_Name | <ABSENT> | Craft Rail Tie 2 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1301727165 | GameObject | — | m_NavMeshLayer | <ABSENT> | 0 | D |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1301727165 | GameObject | — | m_ObjectHideFlags | <ABSENT> | 0 | D |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1301727165 | GameObject | — | m_PrefabAsset | <ABSENT> | {fileID: 0} | D |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1301727165 | GameObject | — | m_PrefabInstance | <ABSENT> | {fileID: 0} | D |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1301727165 | GameObject | — | m_StaticEditorFlags | <ABSENT> | 0 | D |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1301727165 | GameObject | — | m_TagString | <ABSENT> | Untagged | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1301727165 | GameObject | — | serializedVersion | <ABSENT> | 6 | D |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1947320071 | SpriteRenderer | — | m_AdaptiveModeThreshold | <ABSENT> | 0.5 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1947320071 | SpriteRenderer | — | m_AutoUVMaxAngle | <ABSENT> | 89 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1947320071 | SpriteRenderer | — | m_AutoUVMaxDistance | <ABSENT> | 0.5 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1947320071 | SpriteRenderer | — | m_CastShadows | <ABSENT> | 0 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1947320071 | SpriteRenderer | — | m_Color | <ABSENT> | {r: 1, g: 1, b: 1, a: 1} | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1947320071 | SpriteRenderer | — | m_CorrespondingSourceObject | <ABSENT> | {fileID: 0} | D |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1947320071 | SpriteRenderer | — | m_DrawMode | <ABSENT> | 0 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1947320071 | SpriteRenderer | — | m_DynamicOccludee | <ABSENT> | 1 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1947320071 | SpriteRenderer | — | m_Enabled | <ABSENT> | 1 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1947320071 | SpriteRenderer | — | m_FlipX | <ABSENT> | 0 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1947320071 | SpriteRenderer | — | m_FlipY | <ABSENT> | 0 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1947320071 | SpriteRenderer | — | m_ForceMeshLod | <ABSENT> | -1 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1947320071 | SpriteRenderer | — | m_GameObject | <ABSENT> | {fileID: 1947320069} | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1947320071 | SpriteRenderer | — | m_GlobalIlluminationMeshLod | <ABSENT> | 0 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1947320071 | SpriteRenderer | — | m_IgnoreNormalsForChartDetection | <ABSENT> | 0 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1947320071 | SpriteRenderer | — | m_ImportantGI | <ABSENT> | 0 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1947320071 | SpriteRenderer | — | m_LightProbeUsage | <ABSENT> | 1 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1947320071 | SpriteRenderer | — | m_LightProbeVolumeOverride | <ABSENT> | {fileID: 0} | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1947320071 | SpriteRenderer | — | m_LightmapParameters | <ABSENT> | {fileID: 0} | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1947320071 | SpriteRenderer | — | m_MaskInteraction | <ABSENT> | 0 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1947320071 | SpriteRenderer | — | m_Materials | <ABSENT> | - {fileID: 10754, guid: 0000000000000000f000000000000000, type: 0} | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1947320071 | SpriteRenderer | — | m_MeshLodSelectionBias | <ABSENT> | 0 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1947320071 | SpriteRenderer | — | m_MinimumChartSize | <ABSENT> | 4 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1947320071 | SpriteRenderer | — | m_MotionVectors | <ABSENT> | 1 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1947320071 | SpriteRenderer | — | m_ObjectHideFlags | <ABSENT> | 0 | D |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1947320071 | SpriteRenderer | — | m_PrefabAsset | <ABSENT> | {fileID: 0} | D |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1947320071 | SpriteRenderer | — | m_PrefabInstance | <ABSENT> | {fileID: 0} | D |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1947320071 | SpriteRenderer | — | m_PreserveUVs | <ABSENT> | 0 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1947320071 | SpriteRenderer | — | m_ProbeAnchor | <ABSENT> | {fileID: 0} | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1947320071 | SpriteRenderer | — | m_RayTraceProcedural | <ABSENT> | 0 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1947320071 | SpriteRenderer | — | m_RayTracingAccelStructBuildFlags | <ABSENT> | 1 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1947320071 | SpriteRenderer | — | m_RayTracingAccelStructBuildFlagsOverride | <ABSENT> | 0 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1947320071 | SpriteRenderer | — | m_RayTracingMode | <ABSENT> | 0 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1947320071 | SpriteRenderer | — | m_ReceiveGI | <ABSENT> | 1 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1947320071 | SpriteRenderer | — | m_ReceiveShadows | <ABSENT> | 0 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1947320071 | SpriteRenderer | — | m_ReflectionProbeUsage | <ABSENT> | 1 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1947320071 | SpriteRenderer | — | m_RendererPriority | <ABSENT> | 0 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1947320071 | SpriteRenderer | — | m_RenderingLayerMask | <ABSENT> | 1 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1947320071 | SpriteRenderer | — | m_ScaleInLightmap | <ABSENT> | 1 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1947320071 | SpriteRenderer | — | m_SelectedEditorRenderState | <ABSENT> | 0 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1947320071 | SpriteRenderer | — | m_Size | <ABSENT> | {x: 0.28147542, y: 0.34} | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1947320071 | SpriteRenderer | — | m_SmallMeshCulling | <ABSENT> | 1 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1947320071 | SpriteRenderer | — | m_SortingLayer | <ABSENT> | 0 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1947320071 | SpriteRenderer | — | m_SortingLayerID | <ABSENT> | 0 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1947320071 | SpriteRenderer | — | m_SortingOrder | <ABSENT> | 2 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1947320071 | SpriteRenderer | — | m_Sprite | <ABSENT> | {fileID: 0} | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1947320071 | SpriteRenderer | — | m_SpriteSortPoint | <ABSENT> | 0 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1947320071 | SpriteRenderer | — | m_SpriteTileMode | <ABSENT> | 0 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1947320071 | SpriteRenderer | — | m_StaticBatchInfo.firstSubMesh | <ABSENT> | 0 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1947320071 | SpriteRenderer | — | m_StaticBatchInfo.subMeshCount | <ABSENT> | 0 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1947320071 | SpriteRenderer | — | m_StaticBatchRoot | <ABSENT> | {fileID: 0} | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1947320071 | SpriteRenderer | — | m_StaticShadowCaster | <ABSENT> | 0 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1947320071 | SpriteRenderer | — | m_StitchLightmapSeams | <ABSENT> | 1 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1947320071 | SpriteRenderer | — | m_WasSpriteAssigned | <ABSENT> | 1 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 0 | 1947320071 | SpriteRenderer | — | serializedVersion | <ABSENT> | 2 | D |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1301727166 | Transform | — | m_Children | <ABSENT> | [] | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1301727166 | Transform | — | m_ConstrainProportionsScale | <ABSENT> | 0 | D |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1301727166 | Transform | — | m_CorrespondingSourceObject | <ABSENT> | {fileID: 0} | D |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1301727166 | Transform | — | m_Father | <ABSENT> | {fileID: 943331324} | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1301727166 | Transform | — | m_GameObject | <ABSENT> | {fileID: 1301727165} | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1301727166 | Transform | — | m_LocalEulerAnglesHint | <ABSENT> | {x: 0, y: 0, z: 0} | D |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1301727166 | Transform | — | m_LocalPosition | <ABSENT> | {x: 1, y: -0.33, z: 0} | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1301727166 | Transform | — | m_LocalRotation | <ABSENT> | {x: 0, y: 0, z: 0, w: 1} | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1301727166 | Transform | — | m_LocalScale | <ABSENT> | {x: 1, y: 1, z: 1} | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1301727166 | Transform | — | m_ObjectHideFlags | <ABSENT> | 0 | D |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1301727166 | Transform | — | m_PrefabAsset | <ABSENT> | {fileID: 0} | D |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1301727166 | Transform | — | m_PrefabInstance | <ABSENT> | {fileID: 0} | D |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1301727166 | Transform | — | serializedVersion | <ABSENT> | 2 | D |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 1012757153 | SpriteRenderer | — | m_AdaptiveModeThreshold | <ABSENT> | 0.5 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 1012757153 | SpriteRenderer | — | m_AutoUVMaxAngle | <ABSENT> | 89 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 1012757153 | SpriteRenderer | — | m_AutoUVMaxDistance | <ABSENT> | 0.5 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 1012757153 | SpriteRenderer | — | m_CastShadows | <ABSENT> | 0 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 1012757153 | SpriteRenderer | — | m_Color | <ABSENT> | {r: 1, g: 1, b: 1, a: 1} | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 1012757153 | SpriteRenderer | — | m_CorrespondingSourceObject | <ABSENT> | {fileID: 0} | D |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 1012757153 | SpriteRenderer | — | m_DrawMode | <ABSENT> | 0 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 1012757153 | SpriteRenderer | — | m_DynamicOccludee | <ABSENT> | 1 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 1012757153 | SpriteRenderer | — | m_Enabled | <ABSENT> | 1 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 1012757153 | SpriteRenderer | — | m_FlipX | <ABSENT> | 0 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 1012757153 | SpriteRenderer | — | m_FlipY | <ABSENT> | 0 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 1012757153 | SpriteRenderer | — | m_ForceMeshLod | <ABSENT> | -1 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 1012757153 | SpriteRenderer | — | m_GameObject | <ABSENT> | {fileID: 1012757151} | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 1012757153 | SpriteRenderer | — | m_GlobalIlluminationMeshLod | <ABSENT> | 0 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 1012757153 | SpriteRenderer | — | m_IgnoreNormalsForChartDetection | <ABSENT> | 0 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 1012757153 | SpriteRenderer | — | m_ImportantGI | <ABSENT> | 0 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 1012757153 | SpriteRenderer | — | m_LightProbeUsage | <ABSENT> | 1 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 1012757153 | SpriteRenderer | — | m_LightProbeVolumeOverride | <ABSENT> | {fileID: 0} | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 1012757153 | SpriteRenderer | — | m_LightmapParameters | <ABSENT> | {fileID: 0} | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 1012757153 | SpriteRenderer | — | m_MaskInteraction | <ABSENT> | 0 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 1012757153 | SpriteRenderer | — | m_Materials | <ABSENT> | - {fileID: 10754, guid: 0000000000000000f000000000000000, type: 0} | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 1012757153 | SpriteRenderer | — | m_MeshLodSelectionBias | <ABSENT> | 0 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 1012757153 | SpriteRenderer | — | m_MinimumChartSize | <ABSENT> | 4 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 1012757153 | SpriteRenderer | — | m_MotionVectors | <ABSENT> | 1 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 1012757153 | SpriteRenderer | — | m_ObjectHideFlags | <ABSENT> | 0 | D |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 1012757153 | SpriteRenderer | — | m_PrefabAsset | <ABSENT> | {fileID: 0} | D |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 1012757153 | SpriteRenderer | — | m_PrefabInstance | <ABSENT> | {fileID: 0} | D |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 1012757153 | SpriteRenderer | — | m_PreserveUVs | <ABSENT> | 0 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 1012757153 | SpriteRenderer | — | m_ProbeAnchor | <ABSENT> | {fileID: 0} | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 1012757153 | SpriteRenderer | — | m_RayTraceProcedural | <ABSENT> | 0 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 1012757153 | SpriteRenderer | — | m_RayTracingAccelStructBuildFlags | <ABSENT> | 1 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 1012757153 | SpriteRenderer | — | m_RayTracingAccelStructBuildFlagsOverride | <ABSENT> | 0 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 1012757153 | SpriteRenderer | — | m_RayTracingMode | <ABSENT> | 0 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 1012757153 | SpriteRenderer | — | m_ReceiveGI | <ABSENT> | 1 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 1012757153 | SpriteRenderer | — | m_ReceiveShadows | <ABSENT> | 0 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 1012757153 | SpriteRenderer | — | m_ReflectionProbeUsage | <ABSENT> | 1 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 1012757153 | SpriteRenderer | — | m_RendererPriority | <ABSENT> | 0 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 1012757153 | SpriteRenderer | — | m_RenderingLayerMask | <ABSENT> | 1 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 1012757153 | SpriteRenderer | — | m_ScaleInLightmap | <ABSENT> | 1 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 1012757153 | SpriteRenderer | — | m_SelectedEditorRenderState | <ABSENT> | 0 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 1012757153 | SpriteRenderer | — | m_Size | <ABSENT> | {x: 0.28147542, y: 0.34} | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 1012757153 | SpriteRenderer | — | m_SmallMeshCulling | <ABSENT> | 1 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 1012757153 | SpriteRenderer | — | m_SortingLayer | <ABSENT> | 0 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 1012757153 | SpriteRenderer | — | m_SortingLayerID | <ABSENT> | 0 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 1012757153 | SpriteRenderer | — | m_SortingOrder | <ABSENT> | 2 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 1012757153 | SpriteRenderer | — | m_Sprite | <ABSENT> | {fileID: 0} | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 1012757153 | SpriteRenderer | — | m_SpriteSortPoint | <ABSENT> | 0 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 1012757153 | SpriteRenderer | — | m_SpriteTileMode | <ABSENT> | 0 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 1012757153 | SpriteRenderer | — | m_StaticBatchInfo.firstSubMesh | <ABSENT> | 0 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 1012757153 | SpriteRenderer | — | m_StaticBatchInfo.subMeshCount | <ABSENT> | 0 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 1012757153 | SpriteRenderer | — | m_StaticBatchRoot | <ABSENT> | {fileID: 0} | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 1012757153 | SpriteRenderer | — | m_StaticShadowCaster | <ABSENT> | 0 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 1012757153 | SpriteRenderer | — | m_StitchLightmapSeams | <ABSENT> | 1 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 1012757153 | SpriteRenderer | — | m_WasSpriteAssigned | <ABSENT> | 1 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 1012757153 | SpriteRenderer | — | serializedVersion | <ABSENT> | 2 | D |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1946492881 | GameObject | — | m_Component | <ABSENT> | - component: {fileID: 1946492882}<br>- component: {fileID: 1946492883} | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1946492881 | GameObject | — | m_CorrespondingSourceObject | <ABSENT> | {fileID: 0} | D |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1946492881 | GameObject | — | m_Icon | <ABSENT> | {fileID: 0} | D |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1946492881 | GameObject | — | m_IsActive | <ABSENT> | 1 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1946492881 | GameObject | — | m_Layer | <ABSENT> | 0 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1946492881 | GameObject | — | m_Name | <ABSENT> | Craft Rail Tie 2 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1946492881 | GameObject | — | m_NavMeshLayer | <ABSENT> | 0 | D |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1946492881 | GameObject | — | m_ObjectHideFlags | <ABSENT> | 0 | D |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1946492881 | GameObject | — | m_PrefabAsset | <ABSENT> | {fileID: 0} | D |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1946492881 | GameObject | — | m_PrefabInstance | <ABSENT> | {fileID: 0} | D |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1946492881 | GameObject | — | m_StaticEditorFlags | <ABSENT> | 0 | D |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1946492881 | GameObject | — | m_TagString | <ABSENT> | Untagged | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 2 | 1946492881 | GameObject | — | serializedVersion | <ABSENT> | 6 | D |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 265655747 | GameObject | — | m_Component | <ABSENT> | - component: {fileID: 265655748}<br>- component: {fileID: 265655749} | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 265655747 | GameObject | — | m_CorrespondingSourceObject | <ABSENT> | {fileID: 0} | D |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 265655747 | GameObject | — | m_Icon | <ABSENT> | {fileID: 0} | D |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 265655747 | GameObject | — | m_IsActive | <ABSENT> | 1 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 265655747 | GameObject | — | m_Layer | <ABSENT> | 0 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 265655747 | GameObject | — | m_Name | <ABSENT> | Craft Rail Tie 1 | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 265655747 | GameObject | — | m_NavMeshLayer | <ABSENT> | 0 | D |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 265655747 | GameObject | — | m_ObjectHideFlags | <ABSENT> | 0 | D |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 265655747 | GameObject | — | m_PrefabAsset | <ABSENT> | {fileID: 0} | D |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 265655747 | GameObject | — | m_PrefabInstance | <ABSENT> | {fileID: 0} | D |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 265655747 | GameObject | — | m_StaticEditorFlags | <ABSENT> | 0 | D |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 265655747 | GameObject | — | m_TagString | <ABSENT> | Untagged | C |
| Main S05 Left Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 265655747 | GameObject | — | serializedVersion | <ABSENT> | 6 | D |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 1012757151 | GameObject | — | m_Component | <ABSENT> | - component: {fileID: 1012757152}<br>- component: {fileID: 1012757153} | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 1012757151 | GameObject | — | m_CorrespondingSourceObject | <ABSENT> | {fileID: 0} | D |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 1012757151 | GameObject | — | m_Icon | <ABSENT> | {fileID: 0} | D |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 1012757151 | GameObject | — | m_IsActive | <ABSENT> | 1 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 1012757151 | GameObject | — | m_Layer | <ABSENT> | 0 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 1012757151 | GameObject | — | m_Name | <ABSENT> | Craft Rail Tie 1 | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 1012757151 | GameObject | — | m_NavMeshLayer | <ABSENT> | 0 | D |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 1012757151 | GameObject | — | m_ObjectHideFlags | <ABSENT> | 0 | D |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 1012757151 | GameObject | — | m_PrefabAsset | <ABSENT> | {fileID: 0} | D |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 1012757151 | GameObject | — | m_PrefabInstance | <ABSENT> | {fileID: 0} | D |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 1012757151 | GameObject | — | m_StaticEditorFlags | <ABSENT> | 0 | D |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 1012757151 | GameObject | — | m_TagString | <ABSENT> | Untagged | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 1012757151 | GameObject | — | serializedVersion | <ABSENT> | 6 | D |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 1012757152 | Transform | — | m_Children | <ABSENT> | [] | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 1012757152 | Transform | — | m_ConstrainProportionsScale | <ABSENT> | 0 | D |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 1012757152 | Transform | — | m_CorrespondingSourceObject | <ABSENT> | {fileID: 0} | D |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 1012757152 | Transform | — | m_Father | <ABSENT> | {fileID: 943331324} | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 1012757152 | Transform | — | m_GameObject | <ABSENT> | {fileID: 1012757151} | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 1012757152 | Transform | — | m_LocalEulerAnglesHint | <ABSENT> | {x: 0, y: 0, z: 0} | D |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 1012757152 | Transform | — | m_LocalPosition | <ABSENT> | {x: 0, y: -0.33, z: 0} | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 1012757152 | Transform | — | m_LocalRotation | <ABSENT> | {x: 0, y: 0, z: 0, w: 1} | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 1012757152 | Transform | — | m_LocalScale | <ABSENT> | {x: 1, y: 1, z: 1} | C |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 1012757152 | Transform | — | m_ObjectHideFlags | <ABSENT> | 0 | D |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 1012757152 | Transform | — | m_PrefabAsset | <ABSENT> | {fileID: 0} | D |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 1012757152 | Transform | — | m_PrefabInstance | <ABSENT> | {fileID: 0} | D |
| Main S05 Right Shelf/Blue Railway Platform Visual/Craft Rail Tie 1 | 1012757152 | Transform | — | serializedVersion | <ABSENT> | 2 | D |
| Main S10 Goal Floor | 29111734 | MonoBehaviour / CraftWoodPlatformVisual.cs | 92322e3412d147aca7c4d87c6fce590f | whitespace:m_Name | '  m_Name:' | '  m_Name: ' | E |
| Main Section 4 Landing | 34308095 | MonoBehaviour / CraftWoodPlatformVisual.cs | 92322e3412d147aca7c4d87c6fce590f | whitespace:m_Name | '  m_Name:' | '  m_Name: ' | E |
| Main S09 Green Right Bank/Green Rope Anchor Ring Visual | 126668277 | MonoBehaviour / HookWoodMountVisual.cs | e8ca1aa1a80d4988981b9b7bf0127b5b | whitespace:m_Name | '  m_Name:' | '  m_Name: ' | E |
| Main S10 Green Right Bank/Green Rope Anchor Ring Visual | 148069158 | MonoBehaviour / HookWoodMountVisual.cs | e8ca1aa1a80d4988981b9b7bf0127b5b | whitespace:m_Name | '  m_Name:' | '  m_Name: ' | E |
| Main Hook 1/Blue Toy Hook Visual | 167088365 | MonoBehaviour / HookWoodMountVisual.cs | e8ca1aa1a80d4988981b9b7bf0127b5b | whitespace:m_Name | '  m_Name:' | '  m_Name: ' | E |
| Main S06 Merge | 197578227 | MonoBehaviour / CraftWoodPlatformVisual.cs | 92322e3412d147aca7c4d87c6fce590f | whitespace:m_Name | '  m_Name:' | '  m_Name: ' | E |
| Main S06 Lower Hook B/Blue Toy Hook Visual | 320842319 | MonoBehaviour / HookWoodMountVisual.cs | e8ca1aa1a80d4988981b9b7bf0127b5b | whitespace:m_Name | '  m_Name:' | '  m_Name: ' | E |
| Main S07 Green Middle Bridge End Hook/Green Rope Anchor Ring Visual | 454119712 | MonoBehaviour / HookWoodMountVisual.cs | e8ca1aa1a80d4988981b9b7bf0127b5b | whitespace:m_Name | '  m_Name:' | '  m_Name: ' | E |
| Main S05 Flashlight Source | 519685056 | MonoBehaviour / CraftWoodPlatformVisual.cs | 92322e3412d147aca7c4d87c6fce590f | whitespace:m_Name | '  m_Name:' | '  m_Name: ' | E |
| Main Hook 3/Blue Toy Hook Visual | 661798983 | MonoBehaviour / HookWoodMountVisual.cs | e8ca1aa1a80d4988981b9b7bf0127b5b | whitespace:m_Name | '  m_Name:' | '  m_Name: ' | E |
| Main Section 5 Green Shadow End Hook/Green Rope Anchor Ring Visual | 725730771 | MonoBehaviour / HookWoodMountVisual.cs | e8ca1aa1a80d4988981b9b7bf0127b5b | whitespace:m_Name | '  m_Name:' | '  m_Name: ' | E |
| Main Hook 2/Blue Toy Hook Visual | 750751285 | MonoBehaviour / HookWoodMountVisual.cs | e8ca1aa1a80d4988981b9b7bf0127b5b | whitespace:m_Name | '  m_Name:' | '  m_Name: ' | E |
| Main S05 Central Hook/Blue Toy Hook Visual | 825668484 | MonoBehaviour / HookWoodMountVisual.cs | e8ca1aa1a80d4988981b9b7bf0127b5b | whitespace:m_Name | '  m_Name:' | '  m_Name: ' | E |
| Main S05 Landing | 839588654 | MonoBehaviour / CraftWoodPlatformVisual.cs | 92322e3412d147aca7c4d87c6fce590f | whitespace:m_Name | '  m_Name:' | '  m_Name: ' | E |
| Main Section 5 Green Shadow Start Hook/Green Rope Anchor Ring Visual | 888321129 | MonoBehaviour / HookWoodMountVisual.cs | e8ca1aa1a80d4988981b9b7bf0127b5b | whitespace:m_Name | '  m_Name:' | '  m_Name: ' | E |
| Main Stage Night Child Room Background | 900000104 | MonoBehaviour / HimoHitoCraftRoomBackground.cs | 75cbd9d95e0c4e2d990f2c82b225e77b | whitespace:m_Name | '  m_Name:' | '  m_Name: ' | E |
| Main Section 4 Intermediate Column | 947926183 | MonoBehaviour / CraftWoodPlatformVisual.cs | 92322e3412d147aca7c4d87c6fce590f | whitespace:m_Name | '  m_Name:' | '  m_Name: ' | E |
| Main Landing 2 | 1090326792 | MonoBehaviour / CraftWoodPlatformVisual.cs | 92322e3412d147aca7c4d87c6fce590f | whitespace:m_Name | '  m_Name:' | '  m_Name: ' | E |
| Main Start Ground | 1129056248 | MonoBehaviour / CraftWoodPlatformVisual.cs | 92322e3412d147aca7c4d87c6fce590f | whitespace:m_Name | '  m_Name:' | '  m_Name: ' | E |
| Main S06 Lower Hook A/Blue Toy Hook Visual | 1135571039 | MonoBehaviour / HookWoodMountVisual.cs | e8ca1aa1a80d4988981b9b7bf0127b5b | whitespace:m_Name | '  m_Name:' | '  m_Name: ' | E |
| Main S09 Green Left Bank/Green Rope Anchor Ring Visual | 1137895901 | MonoBehaviour / HookWoodMountVisual.cs | e8ca1aa1a80d4988981b9b7bf0127b5b | whitespace:m_Name | '  m_Name:' | '  m_Name: ' | E |
| Main Hook 3 Upper/Blue Toy Hook Visual | 1181016050 | MonoBehaviour / HookWoodMountVisual.cs | e8ca1aa1a80d4988981b9b7bf0127b5b | whitespace:m_Name | '  m_Name:' | '  m_Name: ' | E |
| Main S09 Hanging Beam | 1301360679 | MonoBehaviour / CraftWoodPlatformVisual.cs | 92322e3412d147aca7c4d87c6fce590f | whitespace:m_Name | '  m_Name:' | '  m_Name: ' | E |
| Main S06 Green Upper Bridge End Hook/Green Rope Anchor Ring Visual | 1313190105 | MonoBehaviour / HookWoodMountVisual.cs | e8ca1aa1a80d4988981b9b7bf0127b5b | whitespace:m_Name | '  m_Name:' | '  m_Name: ' | E |
| Main S07 Landing | 1438313561 | MonoBehaviour / CraftWoodPlatformVisual.cs | 92322e3412d147aca7c4d87c6fce590f | whitespace:m_Name | '  m_Name:' | '  m_Name: ' | E |
| Main S07 Upper Hook/Blue Toy Hook Visual | 1498441521 | MonoBehaviour / HookWoodMountVisual.cs | e8ca1aa1a80d4988981b9b7bf0127b5b | whitespace:m_Name | '  m_Name:' | '  m_Name: ' | E |
| Main S07 Green Lower Bridge Start Hook/Green Rope Anchor Ring Visual | 1512403161 | MonoBehaviour / HookWoodMountVisual.cs | e8ca1aa1a80d4988981b9b7bf0127b5b | whitespace:m_Name | '  m_Name:' | '  m_Name: ' | E |
| Main S07 Upper Step | 1535825987 | MonoBehaviour / CraftWoodPlatformVisual.cs | 92322e3412d147aca7c4d87c6fce590f | whitespace:m_Name | '  m_Name:' | '  m_Name: ' | E |
| Main Section 4 Green Bridge End Hook/Green Rope Anchor Ring Visual | 1590596619 | MonoBehaviour / HookWoodMountVisual.cs | e8ca1aa1a80d4988981b9b7bf0127b5b | whitespace:m_Name | '  m_Name:' | '  m_Name: ' | E |
| Main Section 4 Far Hook/Blue Toy Hook Visual | 1630339974 | MonoBehaviour / HimoHitoHookRingVisual.cs | 3d00bd5e0b5b4e878bb89da53c2e0a47 | whitespace:m_Name | '  m_Name:' | '  m_Name: ' | E |
| Main Section 4 Far Hook/Blue Toy Hook Visual | 1630339975 | MonoBehaviour / HookWoodMountVisual.cs | e8ca1aa1a80d4988981b9b7bf0127b5b | whitespace:m_Name | '  m_Name:' | '  m_Name: ' | E |
| Main S06 Green Upper Bridge Start Hook/Green Rope Anchor Ring Visual | 1648997225 | MonoBehaviour / HookWoodMountVisual.cs | e8ca1aa1a80d4988981b9b7bf0127b5b | whitespace:m_Name | '  m_Name:' | '  m_Name: ' | E |
| Main S09 Goal Floor | 1717291301 | MonoBehaviour / CraftWoodPlatformVisual.cs | 92322e3412d147aca7c4d87c6fce590f | whitespace:m_Name | '  m_Name:' | '  m_Name: ' | E |
| Main S06 Upper Final Step | 1721261338 | MonoBehaviour / CraftWoodPlatformVisual.cs | 92322e3412d147aca7c4d87c6fce590f | whitespace:m_Name | '  m_Name:' | '  m_Name: ' | E |
| Main S10 Green Left Bank/Green Rope Anchor Ring Visual | 1763044992 | MonoBehaviour / HookWoodMountVisual.cs | e8ca1aa1a80d4988981b9b7bf0127b5b | whitespace:m_Name | '  m_Name:' | '  m_Name: ' | E |
| Main S06 Lower Hook C/Blue Toy Hook Visual | 1870317079 | MonoBehaviour / HookWoodMountVisual.cs | e8ca1aa1a80d4988981b9b7bf0127b5b | whitespace:m_Name | '  m_Name:' | '  m_Name: ' | E |
| Main S07 Middle Shelf | 1999290182 | MonoBehaviour / CraftWoodPlatformVisual.cs | 92322e3412d147aca7c4d87c6fce590f | whitespace:m_Name | '  m_Name:' | '  m_Name: ' | E |
| Main Landing 1 | 2056289256 | MonoBehaviour / CraftWoodPlatformVisual.cs | 92322e3412d147aca7c4d87c6fce590f | whitespace:m_Name | '  m_Name:' | '  m_Name: ' | E |
| Main Section 4 Green Bridge Start Hook/Green Rope Anchor Ring Visual | 2072268208 | MonoBehaviour / HookWoodMountVisual.cs | e8ca1aa1a80d4988981b9b7bf0127b5b | whitespace:m_Name | '  m_Name:' | '  m_Name: ' | E |
| Main Landing 3 | 2109282685 | MonoBehaviour / CraftWoodPlatformVisual.cs | 92322e3412d147aca7c4d87c6fce590f | whitespace:m_Name | '  m_Name:' | '  m_Name: ' | E |
| Main S09 Center Hook/Blue Toy Hook Visual | 2128043424 | MonoBehaviour / HookWoodMountVisual.cs | e8ca1aa1a80d4988981b9b7bf0127b5b | whitespace:m_Name | '  m_Name:' | '  m_Name: ' | E |
| Main Section 3 Low Dead End | 9100000020 | MonoBehaviour / CraftWoodPlatformVisual.cs | 92322e3412d147aca7c4d87c6fce590f | whitespace:m_Name | '  m_Name:' | '  m_Name: ' | E |
| Main Section 3 Return Step A | 9100000029 | MonoBehaviour / CraftWoodPlatformVisual.cs | 92322e3412d147aca7c4d87c6fce590f | whitespace:m_Name | '  m_Name:' | '  m_Name: ' | E |
| Main Section 3 Return Step B | 9100000039 | MonoBehaviour / CraftWoodPlatformVisual.cs | 92322e3412d147aca7c4d87c6fce590f | whitespace:m_Name | '  m_Name:' | '  m_Name: ' | E |
| Main Section 3 Return Step C | 9100000049 | MonoBehaviour / CraftWoodPlatformVisual.cs | 92322e3412d147aca7c4d87c6fce590f | whitespace:m_Name | '  m_Name:' | '  m_Name: ' | E |
| Main Section 3 Return Step D | 9100000059 | MonoBehaviour / CraftWoodPlatformVisual.cs | 92322e3412d147aca7c4d87c6fce590f | whitespace:m_Name | '  m_Name:' | '  m_Name: ' | E |
