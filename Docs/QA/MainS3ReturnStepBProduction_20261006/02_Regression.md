# S3 Return Step B 回帰検証

既存隔離Project `.codex_tmp/MainStageVisualAudit_20261003/Project` で、今回のProduction追加4ファイルを同一SHAのまま読み込みました。Production本体のUnityは起動していません。新しい巨大なProjectコピー、Windows Buildは作成していません。

最終Run04はUnity 6000.3.21f1、Unity exit 0、66項目PASS、FAIL 0、Gameplay Error 0です。C#／Shader Compileエラーは検出されていません。

| 項目 | 結果 | 範囲 |
| --- | --- | --- |
| CompileとPlay | PASS | 隔離Editorで実行 |
| 接地とCollider接触 | PASS | Step Bで実Contact確認 |
| 歩行 | PASS | x=41から41.17104へ、接地維持 |
| ジャンプと空中と着地 | PASS | 上向き速度8.33469、その後着地 |
| 照準とHook接続と解除 | PASS | 既存Lower HookとRope API |
| ヒモ残量 | PASS | 接続・解除前後で一致 |
| ColliderとTransformとPhysics | PASS | 対象固定、描画切替で物理スナップショット一致 |
| Camera | PASS | 実Transform・ortho・aspect・follow設定一致 |
| 他20床 | PASS | Transform／Collider、描画state不変 |
| Start GroundとS4定規とStep A色鉛筆 | PASS | 採用済みPresentation維持 |
| Tutorial分離 | PASS | Marker ownerなし、Hybrid地形と2.5D背景維持 |
| 非表示と再Revealと再ロード | PASS | 二重生成なし、最終pose維持 |
| メッシュとUVと素材 | PASS | 採用Proofと一致 |
| 全幅上面とXY bounds | PASS | fake ledgeなし、歩行Y=-5.50 |
| Gameplay Error | PASS | 最終Run04は0件 |
| Human実キーと全区間通しプレイ | 未確認 | 今回の自動検証では代替しない |
| 新Windows Buildとその実行 | 未確認 | 今回未実施 |

診断ではPlayerを既存スイッチとStep Bへ配置し、入力fieldを注入して既存PlayerMoverを動かし、既存Switch／Rope APIを呼んでいます。人間の実キーボード操作や通しルートの難易度評価ではありません。解除時に測定した速度は(0, 0)で、非zeroのSwing momentum検証とはしません。

## 診断履歴

Run01の失敗は、Step A用QAから引き継いだGetTexture("_EndTex")読み取りがMarker Shaderに対して2件の診断エラーを発生させたものです。Productionの動作修正は行わず、QAの未使用プロパティ読み取りを除去しました。

Run02はGameplay Error 0ですが、追加したHasProperty否定判定1件がFAILでした。Shader宣言の不在とMaterialのHasProperty実測を同一視した不適切な診断でした。この仮定を除き、実際に使用しているShader Assetが意図したResources Assetと一致する検査へ訂正しました。

Run03は64項目PASSでしたが、最終stage監査で床一覧にStep Bの重複とStep Aの欠落が判明しました。当時の「他20床」ラベルは実際には19床の比較で、Step Aは存在確認のみでした。QA配列だけを訂正し、21床が一意であることと非対象20床の件数を明示チェックするRun04を最終結果としました。4Runの間でProduction4ファイルは同じSHAのままです。過去RunのResult／Progress／ログを保持し、書き換えていません。

各Runで既存UnityEditor.Search.SearchDatabaseのArgumentOutOfRangeException 1件を分離記録しています。Editor例外は未解決で、今回のVisual統合とは別の既存課題です。

記録は `Evidence/Run04/Result.json`、`Evidence/RunLogsFinal.zip` 内の `Run04.log` と診断Sourceにあります。4Runの生ログをbyte一致でZIP保存し、各entryのSHAをRunLogsFinalManifest.jsonに記録しています。ローカル生ログも変更せず保持しています。
