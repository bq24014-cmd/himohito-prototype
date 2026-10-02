# Tutorial Terrain Hybrid — Production Integration

判定: **TP1 APPROVED — 技術検証PASS / Human Review PASS**。今回のProduction版について「問題なし・採用してよい」を受領。[承認記録と保存範囲](07_HumanApprovalAndCheckpoint.md)を参照。

TA1 APPROVEDの5床を本体 `Assets` へ正式配置した。これは隔離Proofだけの更新ではない。Production版の確認後、個別stage・監査・commit / pushへ進む承認を受領した。

## 結果

- Tutorial通常床5個のみ。承認済みPNGとmetaは12ファイルすべてbyte一致。
- ProductionはRuntime component 1個と必要素材のみ。F7・比較切替・診断APIは入れない。
- 本体を起動せず、配置後の本体Assets/Packages/ProjectSettingsから新規検証コピーを作成。全区間の自動回帰 **941 PASS / 0 FAIL / ゲーム実行エラー0**。
- 59組の同一frame/Camera比較。5床投影矩形union（境界3px）の外は差分0pixel。
- Windows Build成功。ビルド済みAssembly-CSharp.dllを再監査し、Proof/QA型・F7用API・地形からの入力呼び出しの混入なし。
- 既存MainStage Scene / README / 学習ログ、背景、BGM、旧QA、旧Buildを保護。証跡は `Evidence/ProtectionEnd.json`。

## 読む順序

1. [移植内容](01_ProductionIntegration.md)
2. [本体差分とファイル一覧](02_ProductionDiff.md)
3. [Gameplay回帰](03_GameplayRegression.md)
4. [Visual回帰](04_VisualRegression.md)
5. [Human Review手順](05_HumanReviewPacket.md)
6. [判定と未承認事項](06_FinalDecision.md)

比較: [5床一覧](Screenshots/Final/AllFiveTerrain_Comparison.png)、[全区間Overview](Screenshots/Final/Overview_Comparison.png)。

Build: `output/TutorialTerrainHybridProduction_20261003/Windows/HimoHitoTutorialTerrainHybridProduction.exe`（Repository root基準）。[専用ランチャー](LaunchReview.ps1)も使用可能。今回はF7を搭載しない。

## Evidenceの境界

自動回帰は実Physicsに対するcached input / API経路であり、OSキーボードによるHumanプレイではない。world-camera画像にはScreen Space Overlay UIを含まない。最終実操作・印象確認はHumanに依頼する。

既知のUnityEditor.Search起動時例外1件を `Runtime01/editor_search_errors.txt` に保存。ゲーム例外と分離して扱う。ビルド後の初回DLL検査は検査側Reflection例外で中断し、検査のみ修正した `PlayerAssemblyAudit02` で同一DLLをPASS確認した。初回結果は上書きしていない。

実装・QA工程にはProduction採用済み2.5D背景変更、MainStage地形展開、新しい絵の生成、cleanupを含まない。Human承認後のcheckpointはSource・素材・必要QAのみを対象とし、Build/BGM/隔離copy/生PPMを含めない。
