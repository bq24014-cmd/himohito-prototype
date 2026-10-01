# Clean HEAD Windows Build

## 入力

基準HEADから`git archive HEAD Assets Packages ProjectSettings`を使って新規export。
既存の未Commit Source / Scene / 設定 / 過去Buildは使っていない。
461入力ファイル、Production Runtime Scripts 113件。入力SHA manifestとexport手順はEvidence/clean_input_*.jsonに保持。

作業場所:

`.codex_tmp/MainStageCleanReproduction_20261001/BuildProject/`

新規Build試行先:

`.codex_tmp/MainStageCleanReproduction_20261001/BuildProject/Builds/Windows/Game/`

このコピーのBuildsは実行前に存在しないことを確認。Production側や過去Buildの出力は上書きしていない。
Unity 6000.3.21f1 / Windows Standalone x64 / committed WindowsPrototypeBuilderを使用。

## 結果: FAIL

`Assets/Editor/BackgroundMusicBuildCheck.cs`の既存OnPreprocessBuildが停止:

> BuildFailedException: BGM「優しい踊り」がありません。READMEのBGM導入手順で公式音源を配置してください。

最終ログ: `Windows build failed: Failed, errors=2, warnings=0`。
Production Scriptsのコンパイルエラーはない。exeは未生成。
詳細はEvidence/CleanHeadBuild.log / build_result.json。

## 原因

`Assets/Resources/Audio/YasashiiOdori.mp3`は作業元には存在するが、.gitignoreにより意図的にGit対象外。
HEADにあるのは同名.metaだけで、音源実体はない。READMEにも外部配置と「未配置時はWindows Build停止」が明記されている。

作業元の音源SHA256:

`25C52DA234D42C7A5E5CC91D45AEED14829F845EA15932A88AC94AB006781E83`

今回は「clean HEADのみ」の条件を優先し、音源を検証コピーへ追加していない。
Buildチェックの回避、既存音源のGit追加、Gameplay追加修正は行っていない。
したがってScene依存ではなく、**READMEで管理されている外部素材依存**による停止。

Unityのimportにより、音源のない検証コピー内では孤立.metaが除去された。
元プロジェクトの.metaとBGMはそのまま。コピーのProduction SourceとMainStage SceneはHEADと同一のまま。

## exe / UI smoke

Stage Selection / Tutorial開始 / MainStage開始のWindows exe smokeはすべてNOT RUN（exeなし）。
computer-useのアプリ一覧取得もtimeoutしたが、Build失敗が独立した主停止理由であり、別手段で旧exeを起動して代用していない。
