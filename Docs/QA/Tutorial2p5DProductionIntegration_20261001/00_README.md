# Tutorial 2.5D Background Production Integration

Task日2026-10-01（検証継続は2026-10-02 JST）。HimoHitoPrototypeのみ。DESERT LOOPは未参照・未変更。

基準main / HEAD / origin/main：`702a4bc044b0eb35bedb552753ca8495599bc1ee`。

P1 / R1 / T1とHuman承認済みの見た目を、既存Tutorial背景入口へ統合。3層、配置、追従、前景・接地処理は維持し、ゴール右の透明端/反転境界だけShaderで補正した。

Production Sourceは既存入口1件と新規背景component1件、Shader2件、承認済みPNG3枚＋`.meta`。Scene/Gameplay/Camera/Physics/BGM/既存Buildは変更しない。

## 資料

- 01_IntegrationPlan.md：開始確認と安全な隔離手順
- 02_SeamFix.md：画像端/反転境界の原因と最小修正
- 03_ProductionDiff.md：本体のSource/Assetと責務
- 04_AllSectionsRegression.md：127項目の最終Editor回帰と限界
- 05_Performance.md：Texture/CPU frame/build size/FPS観測と測定条件
- 06_HumanReviewPacket.md：新Build、画像、Human確認項目
- 07_FinalDecision.md：PI判定と停止条件
- Evidence/：原観測、診断Source、ログ、変更前Source、試行履歴
- Screenshots/：実Unity比較画像とsample GIF
- ProtectionResult.json：既存3866ファイルの保護照合
- ProductionManifest.json：正確な新Source/Asset SHAと隔離版比較

## 注意

Editor実物理/API回帰と人間のキーボード評価を区別する。今回Humanでイントロ中の旧背景表示を報告。**PI2 / 採用保留**。過去T1 Human承認を今回へ自動流用しない。Cameraではなく背景表示条件の問題であり、Human Review後の追加Source修正はこのTaskでは行わない。

Windows Buildは**現在のWorking Tree + documented external BGM**から作成。mainの未Commit Scene等の既存状態を含むため、「clean HEADだけ」のBuildとは呼ばない。Assets/Scenes/MainStage.unityの既存dirty状態はSHAまで保持する。

stage / commit / push未実施。Human Reviewで停止する。
