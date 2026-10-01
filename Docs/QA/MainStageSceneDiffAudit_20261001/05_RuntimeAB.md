# Isolated Runtime A/B

判定：**SCENE DIFF RUNTIME-REDUNDANT**（今回の差分対象・setup完了後）。

## 条件と安全策

- Unity `6000.3.21f1`、Windows、batchmode + D3D11。
- 新規隔離project：`.codex_tmp/MainStageSceneDiffAudit_20261001/Project`。
- A＝最新HEAD Scene、B＝Working Tree Scene。同一projectで順次起動、差し替えるのは隔離コピーのSceneだけ。
- Production runtime Scripts 113件、Editor Scripts、Packages、ProjectSettingsは最新HEADと照合。同一入力から実施し、A/B後にもScene以外の入力hash変更0。`isolated_inputs_verification.json`参照。
- Git LFSの11音声payloadはHEADのLFS oidと照合。既存ignore対象の`YasashiiOdori.mp3`はHEADに含まれないresourceで、現Working TreeからA/B両方へ同じバイトをコピーした。Scripts/Packages/ProjectSettingsの差ではない。
- QA専用`SceneAuditEntry.cs`をコピー側Editorに追加。Production Sourceには導入していない。
- Edit modeは未保存のEmpty Sceneで開始。MainStageはPlay mode内でロードした。MainStageSceneSyncによるEdit mode保存を避けた。
- 診断から`ApplyCurrentScene`を直接追加呼出しして結果を均していない。通常のOnEnable / sceneLoaded setupが完了した30フレーム後にsnapshot。
- Aの実行後Scene SHAはHEAD入力と一致、BもWorking Tree入力と一致。SceneSyncで比較前に両方が同じSceneへ保存された結果ではない。

## 対象

4 root＋全descendant、計14ノード：第5区間左右Shelf（描画子＋各3 Tie）、第2区間Front/Far Spike（各Face）。

以下を比較した：world/local transform、rotation、scale、active state、layer/tag、component構成、BoxCollider2D size/offset/enabled/trigger/effector、SpriteRenderer sprite/texture/rect/size/bounds/color/sorting/draw/tile mode、MonoBehaviourの可視serialized値。Unity instanceID等の一時参照は階層名/型名へ正規化。Gameplay比較対象を全Sceneへ広げていない。

| 結果 | A | B |
| --- | --- | --- |
| 対象ノード数 | 14 | 14 |
| snapshot生成 | 成功 | 成功 |
| 3局所render生成 | 成功 | 成功 |
| Runtime property差 | 0 | 0 |

`runtime_comparison.json`のdifferencesは空。Collider変更・Hook変更・Bridge状態の差は本Scene diffに含まれていない。第5区間のCraftRail component・子TieはHEAD Sceneからも通常のruntime setupで生成され、棘Faceのscale/Renderer.sizeも同じ値になった。

## Log上の注意

A/BともEditor起動時に`UnityEditor.Search.SearchDatabase`由来の`ArgumentOutOfRangeException`が1件あった。スタックはEditor検索indexの初期化で、Production gameplay/診断snapshotには入っていない。その後MainStageロード・`SCENE_AUDIT_CAPTURE_PASS`・画像保存・正常終了まで到達した。ログを削除せず`A_unity.log` / `B_unity.log`に保存。**「全ログに例外なし」とは報告しない**。この監査で検索index問題を修正していない。

## 範囲

起動時の対象状態比較であり、全区間のHuman操作、再checkpoint回帰、初期ロード数フレームの過渡表示、別PC、配布Buildを新たにPASSしたものではない。Source authorityと今回の全Serialized差分を組み合わせてS4を判断する。
