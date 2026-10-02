# Production Diff

## 既存Source

`Assets/Scripts/HimoHitoCraftRoomBackground.cs`：Ensureの既存背景処理後にTutorial専用背景のEnsureを呼ぶ3行、OnEnableに同じ初期化を呼ぶ2行を追加（コメント計3行）。保存済みSceneのstandalone起動はOnEnable経路になるため両方に対応。MainStageでは新Ensureのscene guardで何もしない。

## 新規Source / Shader

- `Assets/Scripts/TutorialCraftRoomLayers.cs`と`.meta`
- `Assets/Resources/TutorialBackgroundEdge.shader`と`.meta`
- `Assets/Resources/TutorialBackgroundFloor.shader`と`.meta`

新コンポーネントは既存craft-room rootにruntime Tutorialだけで設置。入力を読まず、Camera・Gameplay objectを変更しない。背景Sprite/Material/小さな影Textureだけを所有する。

## 新規Asset

`Assets/Resources/Art/Tutorial2p5D/`とfolder `.meta`。Far/Mid/NearRefined PNG各1920x1080と対応`.meta`を承認済みT1からbyte-for-byteコピー。

| PNG | SHA256 |
| --- | --- |
| Tutorial2p5D_Far.png | 6C7E66A3CDCC5E9D6BD68F4C5E4303512ADA2956CDB82E29327C1E5AEF2F74CA |
| Tutorial2p5D_Mid.png | 8378E1A3CFB787A27EE5DF301B112D43AD598558167DBBF5A6DC4C11A816BFD4 |
| Tutorial2p5D_NearRefined.png | 4CDAA97410DE24143A1CF8D9834AA9CFB3AEDC70A541CC9DC9D6418BFBE82495 |

## 責務

旧背景：Stage Selection、title、intro preview/curtain、素材不足時のfallback。保存済み背景はOnEnableからも新3層を初期化し、Editor setup依存を残さない。

新背景：Tutorialプレイ、失敗表示、Clear。Far=.99/Mid=.92/Near=.88。Far/Midは同じ鏡像guard、小物はNearRefined、床はMid下17%をNearへ投影、5つの足場のCollider boundsを読むだけで接地影を配置。

新背景表示中はlegacy SpriteRendererの`forceRenderingOff`だけを変更。無効化/終了時は開始時の値へ戻す。二重描画を回帰で検証。独立trial bootstrapや診断コードをProductionに追加しない。

## 未変更

Player/Hook/Platform位置、Collider/Joint/Physics、Rope/Bridge、Tutorial進行、4区間、Camera基本挙動、Scene、Packages、ProjectSettings、BGM、既存README/LEARNING_LOG、既存QA/Build。

既存MainStage.unityの未Commit状態もそのまま保持する。これをHEADに戻すTaskではない。

## 検証用コード

Editor限定のIntegrationQA/ReferenceTrialとBuild入口、明示的な性能測定flagだけで起動するStandaloneFrameProbeは**隔離コピー限定**。本体Assets/Scriptsには存在しない。通常Human起動ではprobeは何も生成しない。
