# Production移植

## 採用元と範囲

採用元は `TutorialTerrainHybridAll_20261002/Evidence/ProductionCandidate/Assets/`。先行Humanの「問題なし・採用してよい」はTA1候補に対する承認として記録し、今回TP版の承認とは区別する。

| 対象 | 位置 | scale / world Collider | 画像 | 役割 |
| --- | --- | --- | --- | --- |
| Start Ground | (-4,-4.65,0) | (6,10,1) / 6×10 | Start | S1出発 |
| Tutorial Landing | (12,-4.65,0) | (6,10,1) / 6×10 | Landing1 | S1到着・S2出発 |
| Tutorial T2 Landing | (27,-4.65,0) | (6,10,1) / 6×10 | BridgeBank | S2到着・S3左岸 |
| Tutorial T3 Landing | (39,-4.65,0) | (6,10,1) / 6×10 | MergeBank | S3右岸・S4左岸 |
| Tutorial T4 Goal Floor | (59.5,-4.65,0) | (15,10,1) / 15×10 | Goal | S4通常歩行床 |

5床とも回転identity、BoxCollider2D local size=(1,1)、offset=(0,0)、Static Rigidbody。上面y=0.35。いずれも読取り検証のみで書き換えない。GoalZoneとCheckpoint componentが床rootに同居していても機能は変更せず、旗・宝箱・看板・梁・Hook・Bridgeを表示置換しない。

## Runtime構造

`TutorialTerrainHybridVisual` をruntime初期化／sceneLoadedからTutorialにだけ追加する。Sceneファイルへのcomponent追加は不要。明示5rootの存在、既存Craft Wood Body、Transform / Collider契約、5素材寸法を確認してから表示を準備する。

各床の子にRendererだけを持つ `Tutorial Hybrid Terrain Visual` を生成。quadの四辺は既存Colliderのlocal矩形と同一。承認版のPNG / crop / UV / Sprites/Default / 色 / sortingOrderを維持する。旧木表示は名前allowlistのRendererだけをforceRenderingOffとし、床rootやGameplay componentは無効化しない。

契約不一致／素材不足時は旧木表示を保持し、地形・Colliderを修復しない。Disable / Destroyは所有Visualとmesh/materialだけを解放し、保存した旧Renderer状態を復元する。Re-enableは新規生成し直す。Sceneごとに1managerでMainStageには生成しない。

## Proof専用機能の除去

Productionから `Current` / `Ready` / `HybridEnabled` / `TargetCount` / `TextureCropInfo` / `SetHybrid` / `GetTarget` / `GetVisualBounds` を除去。準備フラグはprivate `ready`、通常表示はcomponentの有効状態で管理する。F7入力、比較画面、OnGUI、UnityEditor参照、診断用フックを追加しない。

比較とprivate状態読取りは隔離検証コピーの `UNITY_EDITOR` QA facadeにのみ存在する。QA型はビルド済みWindows DLLにも残っていないことをCecilメタデータで検査した。

## 安全な検証コピー

ユーザーのUnity終了回答を受け、Production Editorが閉じていることを確認してから13ファイルを追加した。既存MainStageSceneSyncの再コンパイル時保存を避け、本体Unityは開かなかった。

検証コピーは `.codex_tmp/TutorialTerrainHybridProduction_20261003/Project`。古いProof projectを流用せず、新しいProductionからAssets / Packages / ProjectSettingsだけをコピーした。Proof PNG、Small素材、F7ヘルパーは持ち込んでいない。

Build後のSource・SceneはProductionと一致。唯一の既存コピー差分はレビュー識別用companyName/productName（隔離側ProjectSettings.assetのみ）。本体ProjectSettingsは固定。検証用Sourceは隔離の `Assets/VerificationOnly/`、Evidence原本はDocs内に保持する。
