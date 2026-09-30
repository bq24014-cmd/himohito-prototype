# Runtime evidence

## 実行条件・制限

Unity Editor 6000.3.21f1 / Windows / graphicsありbatchmodeのPlay Mode。SceneはTutorial、MainStage。両Sceneの隔離コピーと原本SHAは同一。Productionへのコード変更なし。logging-only差分はEvidence/DiagnosticHarnessに保存。

既存entry checkpoint床までの位置移動で前半を省略した制御Runtime再現。全区間の手動通常プレイではない。E/Q/F/Rは既存実メソッドへ呼び出し、移動/照準はprivate inputへ注入。MainStageのmerge後からcheckpoint10保存までは位置移動・snapshot手動作成なし、通常のPlayerMover物理とMainStageCheckpoint.OnCollisionStay2Dで成立した。

診断専用PlayerSettings.company/product名を使用（本番PlayerPrefsから隔離）。PNG保存用imageconversion moduleだけを隔離Packagesへ追加。Scene・snapshot・橋/Hook復元ロジックは変更しない。撮影の一時camera構図は撮影後に戻す。

## Tutorial: CASE R3

Runtime列挙されたcheckpointは次の3つだけ。

| 床 | 到達区間 | respawn |
|---|---:|---|
| Tutorial Landing | 2 | (12,0.95) |
| Tutorial T2 Landing | 3 | (27,0.95) |
| Tutorial T3 Landing | 4 | (39,0.95) |

T4入口保存時（Tutorial seq12）は橋0、rope99、選択6、中央activeSelf/Hierarchy=true/true。2本生成でrope87、Fで橋1・長さ12、中央false/false・removedHooks1。統合後のcheckpointはauthored配置にない。統合snapshotを作るためのScene変更やCapture強制呼び出しは行わなかった。

**NOT REPRODUCIBLE WITH AUTHORED TUTORIAL FLOW**。

参考としてR相当を実行すると、入口snapshotの橋0、rope99、選択6、中央true/trueに戻る（seq35）。これは保存済みmerged bridge + Hook誤復活の再現ではない。

## MainStage: CASE R1

第9区間入口の既存checkpointへ着地。長さ6×2を生成（rope50→38）、Fで統合。中央Main S09 Center Hookがinactive、removedHooksに1件。

| 時点 / MainStage seq | CurrentSection | 橋数 | 両端 / RopeLength | removedHooks数 | 中央Self / Hierarchy | rope / selected |
|---|---:|---:|---|---:|---|---|
| checkpoint前・seq15 | 9 | 1 | (180,-2.15)→(191,-2.15) / 12 | 1 | false / false | 38 / 6 |
| capture直後・seq19 | 10 | 1 | 同上 | 1 | false / false | 38 / 6 |
| restart直前・seq21 | 10 | 1 | 同上 | 1 | false / false | 38 / 6 |
| RestorePlatformStates直後・seq31 | 10 | 1 | 同上 | 0 | true / true | 38 / 6 |
| restart完了・seq33 | 10 | 1 | 同上 | 0 | true / true | 38 / 6 |

float座標は読みやすく丸めている。未丸め値はstate.jsonl参照。

seq19のcheckpointPlatformStatesには実際に1件、Start=(180,-2.150000095)、End=(191,-2.150000095)、RopeLength=12が入っている。呼び出し元はログのstack traceでMainStageCheckpoint.OnCollisionStay2Dと確認した。seq28→29のRestoreRemovedHooks呼び出し中に中央がfalse→true、リスト1→0となった。

## Bindingの別観測

両Sceneのmerge直後はreleaseCount=2、0.8秒後は0。MainStageのcheckpoint保存前から既にreleaseCount=0。復元後はreleaseCount=0、外端wrapsCount=2。終了済みrelease演出は再演されず、外端wrapは再生成された。独立したbindingバグは未確認。

注意: checkpointPlatforms DTO内のreleaseCount/wrapsCount=0は診断DTOの既定値であり、実際のsnapshotにこれらのフィールドが保存されている意味ではない。実snapshotは3項目のみ。

## 証拠ファイル

- 成功ログ: Tutorial_runtime_attempt3.log、MainStage_runtime.log。
- Tutorial_02_before_merge.png、Tutorial_03_merged_before_checkpoint.png、Tutorial_04_after_restart.png。
- MainStage_02_before_merge.png、MainStage_03_merged_before_checkpoint.png、MainStage_05_checkpoint_captured.png、MainStage_06_after_restart.png。
- Camera.Render PNGは1600×900、ゲーム空間のみ（IMGUIを含まない）。MainStage復元後の画像でも統合1本橋の上に中央青Hookが復活している。
- state.jsonlはTutorial 37行 + MainStage 35行。sequence/frameはSceneごとにリセットされるためscene + sequenceで識別。
- 最後のTutorial seq36 / MainStage seq34はEditor終了のOnDisable/teardown。MainStageのそこでのgenerated0/UNASSIGNEDは終了処理であり、restart状態の根拠に使わない。

## 実行中の環境上の問題（Primary bugと分離）

初回Tutorial_runtime.logはsandbox下のライセンスIPC接続失敗でRuntimeに進まなかった。通常権限で再起動。Tutorial_runtime_elevated.logは診断用EncodeToPNGのimageconversion依存不足によるコンパイル失敗。隔離コピーへのモジュール追加後のattempt3で成功。

両成功ログにUnityEditor.Search.SearchDatabaseの起動時IndexationのArgumentOutOfRangeExceptionが各1件ある。stackはEditor Search内で、ゲームの橋/Hook/計測コードではない。後続Play Modeの生成・統合・着地保存・復元は完了した。今回これをゲームバグやCASE R5の確定原因にしない。
