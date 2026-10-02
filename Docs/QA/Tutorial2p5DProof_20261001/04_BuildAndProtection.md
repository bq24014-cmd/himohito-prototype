# Windows Proof / 保護確認

## Build

- 出力: `output/Tutorial2p5DProof_20261001_Proof01/Game/`。
- Unity `6000.3.21f1`、Windows x64、既存StageCatalogを使用。
- Result: **Succeeded**。約53.11秒、420,964,546 bytes。
- exe SHA256: `837986201D89BE25426B8415CB3BC945F077E5B13F4722D6763D04EBD38C5472`。
- 診断Sourceは `UNITY_EDITOR` 内。Standaloneに自動プレイハーネスは含まれない。
- Company / Productは隔離コピーだけ変更し、既存ゲームのPlayerPrefsと分離。

## 起動

Proof: `output/Tutorial2p5DProof_20261001_Proof01/START_PROOF.bat`。
Before比較: 同フォルダの `START_BASELINE.bat`。

日本語実行パスでの既知Scene遷移問題を避け、試作版専用の `C:/Users/Public/HimoHito_Tutorial2p5D_20261001_Proof01/` へコピーして起動する。既存の `%PUBLIC%/HimoHitoPrototype` や過去Buildにはコピーしない。

今回の初回コピー先は存在しないことを確認して作成。exe SHAは上記と一致。
起動PID: 24032。Window title: `Tutorial2p5DProof_20261001`。MainWindowHandle: 4066498。Responding: True。
PlayerログでEngine / D3D11 / Input初期化を確認。その後ユーザーは「奥行きが分かり、見やすい」と回答。個別操作・区間境界などの独立したHuman結果は未確認。

同時に2つのゲームを開かず、一方を終了してから他方のランチャーを起動すると比較しやすい。Baseline引数は背景Proofを無効にするだけで、Gameplayや操作キーは変えない。

## Production保護

`ProtectionResult.json` 最終照合:

- 保護対象2,860ファイル、SHA256不一致 **0**。
- Productionへの新規ファイル **0**。
- Assets / Scripts / Scene / Packages / ProjectSettings / BGM原本 / 元Docs素材 / 既存output / 過去Build / root README / Learning Logを維持。
- MainStage Production SHA: `57D2464751B5B9C2A78E6C56F34197E9BC2629983FADAA1DFF6E0BA0B4BC2284`。
- Tutorial Production SHA: `DC1BED54CC083256CE895041B1C93578101E19B9738E7CF260619E8F7E1E4B92`。
- BGM原本SHA: `25C52DA234D42C7A5E5CC91D45AEED14829F845EA15932A88AC94AB006781E83`。
- MainStage.unity / README.md / LEARNING_LOG.md の既存未Commit変更はそのまま。
- HEAD / origin/main: `702a4bc044b0eb35bedb552753ca8495599bc1ee`。
- Index空、stage / commit / pushなし。
- DESERT LOOPには触れていない。

隔離コピーの元 `Assets/Scripts/*` / Packagesも開始時コピーと一致。ProjectSettings.assetの差分は隔離したCompany / Productのみ。今回の隔離コピーのTutorial / MainStage SceneはBuild後もProductionの開始時コピーとbyte一致している。これは今回のコピーについての確認であり、Scene Sync全般が不変という主張ではない。

`git diff --check` は既存未Commit MainStage.unityのUnity serialization末尾空白を報告した。このSceneも開始時SHAと一致しているため今回発生した変更ではなく、修正禁止の保護対象としてそのまま保持。

## 保存対象

新規追加はQAフォルダ（Sourceテンプレート・Unity生成meta記録・画像・連番・ログ・README・判定・manifest）、専用Windows Proof出力、隔離作業フォルダのみ。Productionに背景を本採用していない。

原画像は元Docs内にあり、別コピーをUnityへ取り込んだ。metaは `Evidence/ImportedMetadata/` に記録。完全な保存ファイル一覧は `ChangeManifest.json`。

**停止条件到達: Proof完成 / 奥行き・視認性のHuman評価PASS記録済み。追加Gameplay修正、commit、pushを実行しない。**
