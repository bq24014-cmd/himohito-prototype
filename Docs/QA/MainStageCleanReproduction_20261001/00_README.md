# MainStage Clean Reproduction — 2026-10-01

## 最新判定: CASE D1 / Cleanup PASS

ユーザーが生成Sceneのdeterminismを判定基準として承認。
独立clean HEAD A/BでBuild前Scene=HEAD、Build後Scene byte一致、git diff一致。
両Build成功、両exe起動・Human開始確認PASS、各180 Runtime assertions PASS。
全Scene semantic状態一致、過去CASE S4対象14 nodes一致、局所描画3組0pixel差。
Build中の保存stackから既存MainStageSceneSyncが原因と確認。Production / 既存2333ファイル変更0。

**PASS: clean HEAD + documented external BGM dependency + Unity 6000.3.21f1 + existing deterministic MainStage Scene Sync**

「HEADだけでScene fileが不変」とは表現しない。stage / commit / push未実施。
詳細と最終12項目: [08_DeterministicSceneSync.md](08_DeterministicSceneSync.md)。

以下は旧判定基準での結果を制作・検証過程として保持。

## 以前の結果（外部BGM配置の続行承認後）

Windows Build成功、exe起動、Stage Selection / Tutorial / Main開始のHuman確認PASS。
Editor診断180項目PASS、Productionの既存2186ファイル変更0。
ただし隔離BuildコピーのMainStage Sceneが既存同期処理で自動保存され、検証コピーのScene差分0が未成立。
**Cleanup = FAIL / STOP。stage / commit / push未実施。**

- [外部BGM identity](05_ExternalBGMDependency.md)
- [Build / smoke / 自動Scene更新](06_ExternalBuildAndSmoke.md)
- [旧不変条件での12項目判定](07_ExternalFinalDecision.md)

以下は最初の「BGMなし純HEAD Build」の結果を過程として保持したもの。

## 結論

**MainStage Scene Cleanup = FAIL（Windows Build未成立）。Commit / Push未実施。**

提出時SceneのArchive検証はPASS。許可されたMainStage.unityだけをHEAD版へ戻し、Scene diff=0。
ただし、文字通りHEAD内のファイルだけをexportしたWindows Buildは、Git対象外のBGMがないため既存Buildチェックで停止した。
このためA「clean HEADのみでHuman-approved状態を再現」は未成立。B「Working Tree Scene依存が残る」を示す結果ではない。

- 基準branch: `main`
- 基準HEAD / origin/main: `4919233cd1d0815afb438bd35a7336492ec09548`
- Production Source、Gameplay、UI、Audio、Art、他Sceneの修正なし。
- Windows exeは生成されていない。exe起動・実キーボード・Build UI smokeは未実施。
- Editor診断はexe smokeや新たなHuman承認に置き換えない。
- 既存output、過去Build、README、LEARNING_LOG等を保持。indexは空。

## 資料

- [Archive検証](01_ArchiveVerification.md)
- [clean Buildと外部依存](02_CleanBuild.md)
- [Runtime検証と限界](03_RuntimeResults.md)
- [最終報告15項目](04_FinalDecision.md)
- [既存のScene A/B監査](../MainStageSceneDiffAudit_20261001/00_README.md)

実行コード・入力manifest・生ログ・結果・スクリーンショットはEvidence/に保持。
今回作成した診断コードは別コピーのAssets/QAとAssets/Editorにのみ配置し、Productionへは導入していない。

## 停止条件と次に必要な判断

既存BGMは意図的にGit対象外。Buildチェックの削除、BGMのGit追加、無音Build、旧Build流用は行わなかった。
再開する場合は、README記載の外部BGM配置をclean再現の前提に含めてよいかを確認する必要がある。
指定のPASS条件成立までstage / commit / pushは行わない。
