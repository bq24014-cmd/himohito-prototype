# Final Decision

## TP1 APPROVED — Production移植・技術QA PASS / Human Review PASS

承認済みTutorial5床のHybridを、本体Production Assetsへ配置した。先行Proofだけに存在する状態は解消。新規13ファイルのみ、既存Productionファイルは変更していない。

今回のProduction Buildに対する最終Human Reviewで **「問題なし・採用してよい」** を受領。技術的TP1に加えてHuman採用を承認。Source・素材・必要QAの個別stage／監査／commit／pushも同質問への回答として承認された。質問と原文は[07](07_HumanApprovalAndCheckpoint.md)を参照。

## 根拠

- 5床: Start Ground / Tutorial Landing / Tutorial T2 Landing / Tutorial T3 Landing / Tutorial T4 Goal Floor。
- Runtime Source1＋script meta1＋folder meta1＋PNG5＋image meta5。承認済み非コード12ファイル完全一致。PNG再生成なし。
- Runtimeの差はProof用公開API／表示切替状態の除去。Scene・Gameplay・背景・MainStageを変えない。
- 941 PASS / 0 FAIL / game runtime errors 0。自然Intro→S1〜4→Merge→Retry→再構築→Goal→Clear→Stage Selection。MainStageでは非適用。
- 59組の同一Camera/frame画像で、地形投影mask（境界3px）外0pixel。5床の上面・外周・Gameplay component状態一致。
- 全5床とGoal継ぎ目について主担当・独立技術VisualレビューPASS。Human印象評価を代替しない。
- Windows Build Succeeded: 452,434,382 bytes、86.0578124秒。新規出力で旧Build未上書き。
- exe PID 11072起動、応答あり、入力初期化あり、初期logの例外/エラーなし。その後今回のProduction版にHuman総合PASSを受領。個々のキー操作の動画・詳細ログは未取得。
- ビルド済みAssembly-CSharp.dll 156 typesを再監査。QA/Proof型、削除した公開API、地形componentのInput呼出し、UnityEditor参照の混入なし。
- ProtectionEnd PASS: 元477ファイル全SHA一致、新規は許可13件のみ。旧Docs/output6,223件の存在・サイズ・更新時刻一致。
- BGM、本体MainStage Scene・README・学習ログの既存未保存差分を維持。既存2.5D背景の全ファイルもSHA不変。

## 失敗・限界の記録

1. 初回PNGパッケージ実行は相対pathの基準を重複指定して入力を見つけられず中断。Production・生画像は無変更。正しいRuntime01基準で59pairを正常生成した。
2. 初回built DLL監査はCecilの継承property `DeclaringType` のReflection取得でAmbiguousMatchException。Build自体はSucceeded。検査専用Editorコードのproperty検索だけを修正し、同じDLL SHA `2D74107A4709606F588981D6A550816F12FE9EE34247D29896FAB31B6AB1D302` を `Evidence/PlayerAssemblyAudit02/PLAYER_ASSEMBLY_AUDIT.json` でPASS。初回FAIL結果もBuildフォルダに残し、再ビルド／バイナリ差替えはしていない。
3. Editor Search起動時例外1件は既知分類として別記。Unity全体の例外0とは表現しない。
4. Gameplay自動経路はcached input/API。実操作・見やすさの総合確認は後続Human PASSとして別記し、全キー配線や全画面の独立した網羅試験を主張しない。MainStageは非適用チェックのみで、本編通しQAではない。
5. 定量FPS/GPU/メモリ回帰、live assembly reload自体は未実施。Disable/EnableとScene切替・fresh import・Buildを確認。

## 移植・QA終了時のGit記録

Human承認前のHEAD / origin/main: `c21463ce60f63ccbff78486a98210eac3aa21b1f`、main。移植・QA終了時点ではstage空、commit / push未実施。承認後checkpoint開始時にもfetchして同一SHAを確認した。

tracked modifiedは開始時と同じ5件（MainStage / README / LEARNING_LOG＋旧QAのExeA.log / ExeB.log）。今回Production追加13件は未stage。旧QAログを今回の修正と混同しない。

今回Production版へのHuman承認を受領したため、新規Productionと必要QAを個別stage・監査する。既存MainStage / README / 学習ログ／旧QAログ2件、BGM実体、output、隔離cacheは除外。push前にremoteを再確認し、想定外進行があれば停止する。

予定commit message: `feat: adopt tutorial hybrid terrain visuals`。

**承認済みcheckpoint以外は実施しない。追加Gameplay修正・MainStage展開・cleanup禁止。push後のSHA照合・保護確認を報告して停止する。**
