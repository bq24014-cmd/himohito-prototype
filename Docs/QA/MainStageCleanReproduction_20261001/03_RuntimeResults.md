# HEAD Scene / committed SourceのRuntime診断

## 証拠の範囲

別のHEAD export QAProjectで実施。Production Source 113件を改変していない。
診断コードのみ追加し、SceneはEdit Modeで開かずLoadSceneInPlayModeでロードした。
QA用company/productを分離し、Productionの進捗保存を変更していない。
位置fixture、公開/非公開API、cached movement値の注入を使うEditor検証。**実キーボード通しプレイやWindows exe smokeではない。**

## 確認結果

| 項目 | 今回の証拠 |
|---|---|
| Tutorial看板可読性修正 | 4看板の濃い文字色・キー確認、4 detail画像を目視確認 |
| Tutorial Clear中A/D足音抑制 | Clear到達後、cached A/D相当20試行、movement/jump/足音timerクリア、足音AudioSource再生なし |
| MainStage Clear中A/D足音抑制 | 同20試行、位置固定、足音AudioSource再生なし |
| Section 5遮光 | 橋なしunblocked、橋ありblocked、retry/rebuildを反復確認 |
| Section 9右Hook接続改善 | 承認済みnear座標で27 ray samplesが右Hookを選択、centerへの取り違え0、以前のAfter記録と同じ |
| Section 9 merge | 2橋生成→merge、追加消費なし、checkpoint10へ物理移動 |
| Checkpoint Restore Fix | 3回retry、merged橋count1/length12/端点一致、center同一instance非active、resource/選択長一致、collider増殖なし |
| Section 10 Clear | 長さ10橋生成、物理移動で宝箱Clear、Stage Selection復帰、MainStage再選択 |
| Tutorial看板3「99→92」 | committed Source文字列確認。新たなGUI全看板手動確認ではない |
| BGM音源実体 | FAIL: clean HEADに実体がない |

FullRuntime2: `tests=148 failures=0 runtimeErrors=1`。
148 assertionsはPASS。例外1件はUnityEditor.Search.SearchDatabaseのindex初期化処理で、Production Gameplay stackではない。
Editor batch timingはPlayer FPSではなく、性能合格判定に使っていない。
BGM singleton countのPASSはAudioClipが存在することやBGM可聴性のPASSではない。

FocusedRuntime2生結果: `tests=30 failures=2 runtimeErrors=1`。
28 PASS、実体欠落BGM assertion 1件、下記の範囲外追加assertion 1件。
こちらにもUnityEditor.Searchの例外1件。生FAILとログは消していない。

## Section 9の追加assertionの分類

診断コードがEの接続対象とFの除去対象を同一と要求したため、nearで追加assertionがFAILした。
しかしTryResolveCurrentAimHookはRopePlatformBuilderのF除去用で、E接続とは目的が異なる。
以前のHuman承認済みAfter CSVにも、Eが右Hook・Fがcenterとなる11 samplesが存在する。
今回も右Hook選択27 / E-F対象差11で同じ。H3の接続改善が戻った証拠ではなく、**過剰なQA assertion**として分類。
Evidence/runtime_summary.jsonに承認済みbaselineとの集計を保存。実装変更はしない。
left側の到達範囲外fixtureでは右選択0が承認済みbaselineと同じ。これも新しい不具合とは扱わない。

## 初回QAハーネスの不成立記録

- FullRuntime.log: 診断コードがHEADにないImageConversion moduleへ直接依存したコンパイルエラー。Productionではない。PPM exportに変更し、Packagesは変えず再実行。
- FocusedRuntime/: 初回はTitleの1-frame guard前の開始、Color32量子化を無視した色比較、範囲外/異なる用途の対象一致assertionが不適切。訂正後のFocusedRuntime2を主証拠とする。
- 画像はcamera framebufferのPPMからPNGへ変換したもので、Art修正ではない。pixel一致と双方のSHAをcapture_manifest.jsonで検証。
- QA内にlossless PNG、生ログを保存。重複する生PPMは新規`.codex_tmp/MainStageCleanReproduction_20261001/RawCaptures/`へ移動して保持し、Git保存予定資料のサイズを抑えた。削除・既存Build上書きなし。

## 判定限界

対象GameplayのHEAD Scene再現は確認できたが、Windows Build / exe / Audioの完全再現は成立していない。
今回の自動診断を新たなHuman-approved判定や総合Cleanup PASSに読み替えない。
