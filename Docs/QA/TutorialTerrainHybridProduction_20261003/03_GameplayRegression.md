# Gameplay Regression — Tutorial Terrain Hybrid Production

実施日：2026-10-03 / 対象：Production実装の新規同一コピー / 実行：`Runtime01`

## 結果

自動Gameplay regressionは **941 checks / 0 failures / 0 runtime errors**。自然IntroからSection 1〜4、実落下後Retry、Bridge、Merge、Checkpoint復元、Goal到達、Clearまで完了した。本書は自動検証時点の記録。後続のProduction版Human承認は[07](07_HumanApprovalAndCheckpoint.md)に分けて記録し、自動操作をHuman操作と混同しない。

| 指標 | 実測結果 |
| --- | --- |
| assertions | 941 PASS / 0 FAIL |
| Gameplay runtime Error / Exception | 0 |
| Editor Search起動時例外 | 1（下記に別記） |
| 実際の区間遷移 | 3：S1→S2→S3→S4 |
| World-camera画像 | 118枚＝59組の同一frame Before / After |
| Intro開始確定後の監視サンプル | 2,509 |
| 同サンプル中の採用背景非表示 | 0 |
| 同サンプル中の旧背景との二重描画 | 0 |
| Route状態記録 | 486行（header除外） |

941は同一frameの状態不変性・5地形のbounds等を繰り返し検証したassertion数であり、941回の独立プレイを意味しない。実行経路は意図的なRetryと再構築を含む1本の自動Tutorialルート。

一次証拠：[result.txt](Evidence/Runtime01/result.txt)、[checks.txt](Evidence/Runtime01/checks.txt)、[route.csv](Evidence/Runtime01/route.csv)、[intro.csv](Evidence/Runtime01/intro.csv)。

## 検証環境とProductionとの関係

Hybrid本体はProductionの`Assets`へ実装済み。そのProductionのAssets / Packages / ProjectSettingsから新規に作った隔離Unityプロジェクトで検証した。旧Proofプロジェクトの継ぎ足しではない。

[VerificationCopyIdentity.json](Evidence/VerificationCopyIdentity.json)ではコピー対象488ファイルが一致し、`different=[]`、`old_proof_resources_copied=false`。検証先は `.codex_tmp/TutorialTerrainHybridProduction_20261003/Project`。

検証用コードは[TerrainProductionQA.cs](Evidence/TerrainProductionQA.cs)と[TerrainProductionQaFacade.cs](Evidence/TerrainProductionQaFacade.cs)。ともに`UNITY_EDITOR`限定で、通常のProductionコードには比較モード・F7・公開テストAPIを要求しない。Before / Afterは検証用facadeが描画フラグを同一frame内だけ一時保存・変更・完全復元して取得する。プレイを継続する前にはHybrid表示へ戻している。

実行時には次もPASSした。

- Production地形クラスに公開proof method / property / fieldがない。
- TutorialのProduction地形componentは1個。
- 旧F7 review helper / Shape proof helperは存在しない。
- 対象は通常地形5個のみ。各地形に元のstatic Rigidbody2DとBoxCollider2Dが1個ずつあり、表示の追加による物理component増加はない。

Windows playerのビルド・配布アセンブリ監査は、本Editor実行の結果とは別の証拠で確認する。この文書だけでstandalone実キー操作の合格を主張しない。

## 実行経路と操作網羅

| 区間 / 段階 | 確認した内容 | 結果 |
| --- | --- | --- |
| Stage Selection→Intro | 既存`StartSelectedStage`から布の遷移を開始。Introをskipせず、開始確定・開始・中間・終盤・Gameplay復帰を記録。復帰後にbody simulated / mover / ropeが有効 | PASS |
| S1 Retry | Start Ground左端から実歩行で落下。既存Fell failureとfall-unravel完了を待ち、既存R復帰処理でS1 checkpoint・操作・資源・橋なしを復元 | PASS |
| S1 | 通常歩行、実移動中撮影、Jump上昇と同一上面着地、看板pause / resume、既存青Hook照準とfeedback、E attach、振り子、E離脱、S2到達 | PASS |
| S2 | 到達checkpointとR復元、歩行・Jump・看板、青Hook照準とE attach、振り子・離脱、S3到達。残量99・選択長6を保持 | PASS |
| S3 | checkpoint、歩行・Jump・看板。選択長7で右緑HookへE、Qによる橋生成、橋上を実歩行してS4到達 | PASS |
| S4入口 | checkpointとR復元でS3橋1本・残量92・選択長7を保持。通常歩行・Jump・看板のpause / resume | PASS |
| S4 Bridge / Merge | 各長さ6の橋を2本作成。既存F処理で中央Hookを外して統合。追加資源消費なし | PASS |
| S4 post-merge Retry | Rで統合前checkpointを復元。S3橋だけの状態、中央Hook復活、removed hooks 0、残量92・選択長7を確認 | PASS |
| S4再構築 | 同じ既存E / Q / F処理で2本の橋を再作成・再統合 | PASS |
| Wide Goal Floor | 統合橋を実歩行して到達。standing / walk / jump / Hook aim / attach / releaseを確認 | PASS |
| Goal / Clear | 実歩行でGoalへ到達し、既存Clearingで物理停止、続いてClear。残量80、生成橋2本（S3＋統合S4） | PASS |
| Clear後 | 地形component lifecycle検証後、既存Clear→Stage Selectionで背景の開始前状態が復元 | PASS |
| MainStage | 非適用を確認するため読み込み。Hybrid地形component 0、Hybrid名renderer 0、Tutorial背景componentなし、元背景あり | PASS |

`route.csv`には移動・速度・接地・attach・資源・橋数・Camera / Far / Mid / Near位置を記録している。終端Clearの記録はPlayer `(63.22131, 0.9649998)`、残量80、橋2本。経路を成立させるfixture teleport、Camera Transform直接代入、区間番号直接変更は使用していない。R復帰時の位置リセットは既存checkpoint処理自体の仕様である。

## Rope / Hookの検証内容

- E接続は実キーボード処理と同じ`body.position + KeyboardAimDirection * maximumShotDistance`方向を既存`TryAttach`へ渡している。選択したHook、接続点、選択長、DistanceJoint2Dの有効状態・距離・anchorを照合。
- E接続自体は残量を消費しない。S1 / S2のE離脱では線形・角運動量と資源保持を同一frameで照合。Wideでの離脱では線形運動量と資源を照合。
- Qは選択長だけを消費する。通常の資源推移は`99 → 92 → 86 → 80`。S4 Retryで92へ復元後、再構築で80になる。
- Fは既存の中央Hook除去・橋統合処理を呼び、追加消費なしと統合後の橋接続を確認。

照準の一次証拠：[aim_resolution.csv](Evidence/Runtime01/aim_resolution.csv)。8回すべてで期待E Hook、実際のE resolver、実際のvisual feedbackが一致した。

特にS4の2本目では、F用のraw pickerは`Tutorial T4 Center Hook`、E resolverとfeedbackは`Tutorial T4 Green Right Bank`を返す。これは左橋作成後に中央HookをE対象から除外しつつ、Fの対象として残す既存仕様。両者を混同せず個別に検証している。

## 同一frameの安全性・背景・lifecycle

59組のBefore / Afterごとに、表示変更前後のCollider / Rigidbody / Joint / Hook / Rope / Camera / 進行状態・資源・生成橋状態を照合した。5地形の元Transform / Collider / Rigidbodyの状態一致も確認。表示boundsはCollider上面・外周の検証許容値内（上面差 `<0.015`、他端差 `<0.02` world unit）だった。

同一frameで背景、Far / Mid / Near、material参照、除外prop、看板、Hook、Bridge、Playerのrenderer状態が保持されることも確認。3回の実区間遷移では既存parallax位相が連続し、Introの非表示・二重描画は0。既存seam-fix shaderが使用され、背景にCollider / Rigidbody追加はない。

Clear後のcomponent disable / enableでは以下を確認した。

1. 初期のHybrid renderer子は5個。
2. Disable直後のphysics / Camera / 背景 / 除外renderer状態が不変。旧木rendererの元フラグを復元し、Hybrid子を非表示。
3. private readyと所有参照を解放。1frame後に所有renderer子は0個。
4. Enable時も同期的なphysics等は不変。その後readyとなり、component 1個・renderer子5個が復帰。
5. 子の重複、Collider / Rigidbodyの増加なし。再生成後も同一frame Before / After検証がPASS。

これは通常のDisable / Enableとscene遷移の検証であり、Editorのlive assembly reloadを実施した記録ではない。

## 証拠の読み方と未確認範囲

- **OSのキーボード入力ではない。** A / D相当はcached move input、Space相当はjump bufferへ値を入れ、長さ選択・照準・E / Q / F / R・看板は既存APIまたは既存処理を呼び出した。実際の力・衝突・接地・振り子・橋移動はUnity physicsで進行している。キー配線、キーrepeat、矢印を手動で回す操作感、マウス操作の網羅テストではない。
- **画像はworld-camera描画。** [captures.csv](Evidence/Runtime01/captures.csv)の118枚は1920×1080。HUD、布transition、看板説明UI、Clear UI等のOverlay UIの最終表示品質は画像に含まれない。UIのopen / pause / resumeやOutcome遷移は状態として検証した。
- **FPS・性能計測ではない。** 診断用`Time.captureDeltaTime=1/60`を使用。ProductionのfixedDeltaTimeは変更しない。画像保存やassertion処理の負荷があり、frame数・routeのtime値から通常プレイのFPSを推定しない。定量的FPS、frame-time、GPU負荷、メモリregressionは未実施。
- **MainStageは非適用確認のみ。** 本編の通しプレイ、S6到達性・操作・shape proofの再検証は行っていない。本編ProductionへHybridを適用したという主張はない。本体MainStageファイルの保護は別途ファイル保護証拠で扱う。
- 同期renderer比較とGameplay PASSは、素材の自然さ・世界観・読みやすさに対するHuman承認を代替しない。今回Production統合に対する新しいHuman Review / TP判定は未完了。

## Editor起動時例外

`editorSearchStartupErrors=1`を隠さず別記する。[editor_search_errors.txt](Evidence/Runtime01/editor_search_errors.txt)には`ArgumentOutOfRangeException`と`UnityEditor.Search.SearchDatabase` / `SearchInit.IndexationOnStartup`のstackがある。既知のEditor Search起動時分類で、Gameplay runtime Error / Exception 0とは区別している。「Unity全体の例外が0」とは記載しない。

補足ログ：`Evidence/UnityRuntime01.log` はローカル保持（Unity device/licensing metadataを含むためGit保存対象外）。構造化結果・checks・CSV・検査コードは保存する。この文書の作成で新テスト・ビルド・Production編集・commit / pushは行っていない。
