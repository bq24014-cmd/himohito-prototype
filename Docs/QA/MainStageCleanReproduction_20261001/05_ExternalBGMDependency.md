# 承認済み外部BGMによる続行

2026-10-01、ユーザーが既存BGMを外部依存として検証コピーにのみ配置することを承認。
前回の「HEADだけのBuild FAIL」は履歴として保持し、再現条件を明示的に拡張する。

## 原本

`C:\Users\田尻大翔\Documents\Codex\2026-08-07\new-chat\outputs\HimoHitoPrototype\Assets\Resources\Audio\YasashiiOdori.mp3`

- filename: YasashiiOdori.mp3
- size: 1,786,604 bytes
- SHA256: `25C52DA234D42C7A5E5CC91D45AEED14829F845EA15932A88AC94AB006781E83`
- Git対象ではない。git ls-filesとgit check-ignore -vで確認。
- 除外理由は既存.gitignoreコメントとREADMEに記載。音源単体の再配布を避ける方針をそのまま維持。

## 新規隔離コピー

HEAD `4919233cd1d0815afb438bd35a7336492ec09548`のAssets / Packages / ProjectSettingsを新しくexport。
前回の失敗BuildProjectや過去Buildは上書きしない。

Build入力プロジェクト:

`.codex_tmp/MainStageCleanReproduction_20261001/ExternalBGM_20261001_151833/BuildProject/`

BGMコピー先:

`BuildProject/Assets/Resources/Audio/YasashiiOdori.mp3`

同様に別のQAProjectにも外部BGMを配置。QAProjectは診断用で、Productionではない。
コピー後のBGM SHA、原本SHAが上記identityと一致。committed入力461ファイルとの差分0。
Source・Scene・Packages・ProjectSettingsに未Commit入力を混ぜていない。
詳細はEvidence/external_bgm_identity.json、external_build_paths.json、external_input_manifest.json。

## 最終的に許される成功表現

**PASS: clean HEAD + documented external BGM dependency**

「clean HEADだけで完全再現」とは表現しない。
Build結果とsmokeがすべて確認されるまでは、このPASSを宣言しない。

clone後の手順は[ExternalAudio_20261001](../../Dependencies/ExternalAudio_20261001.md)に記録。
