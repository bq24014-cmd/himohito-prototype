# External Audio Dependency — 2026-10-01

## 再現条件

`clean HEAD + documented external BGM dependency`

BGM実体は意図的にGit対象外。**clean HEADだけで完全再現できる、とは扱わない。**
`Assets/Editor/BackgroundMusicBuildCheck.cs`はBGM未配置のWindows Buildを停止する。
Editor実行はBGM欠落警告とともに継続するため、Editor起動成功だけでは音付きBuildの再現確認にならない。

## 新規clone後の配置

1. 本プロジェクトのREADME「BGMの別途配置」に従って音源を用意する。
2. cloneしたUnityプロジェクトのルートから見て、次の位置に配置する。

   `Assets/Resources/Audio/YasashiiOdori.mp3`

3. Gitに同梱された`YasashiiOdori.mp3.meta`は変更しない。
4. Unity 6000.3.21f1でimport完了を待ち、AudioClipとして認識されていることを確認する。
5. Windows Build Supportを用意し、READMEのBuild手順を実行する。

READMEが参照する配布元は「優しい踊り」／GANOの[公式配布ページ](https://dova-s.jp/bgm/detail/3126)、トラック1。
この記述は既存READMEの配置案内を転記したもので、このTaskで新規ダウンロードや配布元の規約再審査は行っていない。
利用条件は配布元を確認する。今回検証に使用したのは、ユーザーが許可したローカル既存音源のみ。

## 今回の既存ファイルidentity

- filename: `YasashiiOdori.mp3`
- file size: `1,786,604 bytes`
- SHA256: `25C52DA234D42C7A5E5CC91D45AEED14829F845EA15932A88AC94AB006781E83`
- Git tracked: **false**（git ls-filesで対象なし）
- ignore: `.gitignore` 42行 `/Assets/Resources/Audio/YasashiiOdori.mp3`
- 除外理由の根拠: .gitignoreコメント「DOVA-SYNDROME: licensed for use in the game, not standalone redistribution.」とREADME「音源単体の再配布を避けるためGit管理外」。推測による理由ではない。

原本の絶対パス・コピー先・コピー前後SHAは、
`Docs/QA/MainStageCleanReproduction_20261001/Evidence/external_bgm_identity.json`と`external_build_paths.json`に記録。

## 保護方針

- BGM原本は改変しない。検証コピーのResources位置へだけ配置する。
- Source / Scene / Production Packages / Production ProjectSettingsは変更しない。
- BGM実体をGitへ追加しない。ignore方針も変更しない。
- Windows配布物の既存Buildは上書きしない。
- 今回の文書に含むSHAは既存音源identityであり、すべての将来の公式配布ファイルが同一SHAだと保証するものではない。

Buildとsmokeの実測結果・合否はMainStageCleanReproductionの最新記録を参照。
