# Production差分・保護

branch: `main`。移植開始HEAD / origin/main: `c21463ce60f63ccbff78486a98210eac3aa21b1f`。開始fetch実施。移植・QA終了まではstage / commit / pushなし。Production版Human承認後のcheckpointは[07](07_HumanApprovalAndCheckpoint.md)に記録。

開始時からtracked modifiedは5件: `Assets/Scenes/MainStage.unity`、`README.md`、`Docs/LEARNING_LOG.md`、既存 `Docs/QA/MainStageCleanReproduction_20261001/Evidence/DeterministicSceneSync/ExeA.log` / `ExeB.log`。3つの明示保護差分だけでなく、この旧QAログ2件も変更・stageしない。

## 新規追加13ファイル

```text
Assets/Scripts/TutorialTerrainHybridVisual.cs
Assets/Scripts/TutorialTerrainHybridVisual.cs.meta
Assets/Resources/Art/TutorialTerrainHybrid.meta
Assets/Resources/Art/TutorialTerrainHybrid/Start.png
Assets/Resources/Art/TutorialTerrainHybrid/Start.png.meta
Assets/Resources/Art/TutorialTerrainHybrid/Landing1.png
Assets/Resources/Art/TutorialTerrainHybrid/Landing1.png.meta
Assets/Resources/Art/TutorialTerrainHybrid/BridgeBank.png
Assets/Resources/Art/TutorialTerrainHybrid/BridgeBank.png.meta
Assets/Resources/Art/TutorialTerrainHybrid/MergeBank.png
Assets/Resources/Art/TutorialTerrainHybrid/MergeBank.png.meta
Assets/Resources/Art/TutorialTerrainHybrid/Goal.png
Assets/Resources/Art/TutorialTerrainHybrid/Goal.png.meta
```

Runtime Source SHA256: `6D1A10060B01F6F9FB97A3852D5A830B401CCFAE1A50F5162C90863C84328F8C`。

非コード12ファイルは承認済みProductionCandidateと全byte一致。新規meta GUID7件は本体全Assetsの検索でそれぞれ1件、衝突なし。コード差分は診断API削除とprivate状態化のみで、床対象・素材・crop・UV・描画構成は同一。

## 保護

開始時の既存477ファイル（Assets / Packages / ProjectSettings / README / LEARNING_LOG）のSHA256を終了時に照合する。追加許容は上記13件のみ。MainStage Scene / README / 学習ログの既存未保存差分は保持し、上書き・修正・stageしない。

既存Docs / output 6,223ファイルは存在・サイズ・LastWriteTimeUtcを照合する。これは旧Build全バイナリのSHA再計算ではない。BGMはAssets内のSHA保護対象でありGit追加対象ではない。

外部BGM `Assets/Resources/Audio/YasashiiOdori.mp3` SHA256:
`25C52DA234D42C7A5E5CC91D45AEED14829F845EA15932A88AC94AB006781E83`。

Tutorial Scene SHA256:
`DC1BED54CC083256CE895041B1C93578101E19B9738E7CF260619E8F7E1E4B92`。

判定証跡: `Evidence/ProtectionStart.json` / `ProtectionEnd.json` / `FinalSourceIdentity.json`。

`ProtectionStart.json` の旧出力ファイル全metadata一覧はローカル保持。Gitには `ProtectionEnd.json`、`FinalSourceIdentity.json` と承認後の `CheckpointStart.json` を保存し、元477＋承認済み13の計490ファイルのハッシュ照合、旧6,223成果物のmetadata保護件数を記録する。

## 隔離コピー照合

Build後487ファイルbyte一致。差分1件は隔離ProjectSettings.assetのcompanyName/productNameのみ。元のSource / Scene / 背景 / Shader / BGM / Packagesは一致。初回コピー記録後にfolder metaの末尾空白を承認版と揃えたが、最終記録でProduction・隔離・承認版一致を確認済み。

`git diff --check` が検出する既存MainStage.unityのtrailing whitespaceは保護対象であり修正しない。今回の新規Runtime Sourceと文書の検査は別記録する。

## 保存対象ではないもの

今回のBuild、隔離project、生成Library、BGM実体、旧Proof・生Evidenceを無差別stageしない。将来の承認後も新規13ファイル＋必要QAを個別監査してstageし、既存未保存5ファイルは除外する。
