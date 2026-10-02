# Cleanup前の安全checkpoint — 2026-10-02

容量監査の結論・数値・Git保存状況は、監査実施時点の記録。その後、ユーザーがTutorial 2.5D Production採用一式のGitHub checkpointを承認した。

Production commit: `39c199498edf49a20b20b7f0488fc46639054c88`

message: `feat: adopt tutorial 2.5D background`

## 保存範囲

- 承認済み背景入口Source、新規背景Source/.meta、2 Shader/.meta、3 PNG/.meta、フォルダ.metaの計14 Productionファイル。
- P1 / R1 / T1 / PI2からIF1への過程、Human最終承認、127項目回帰・Build/起動結果、比較PNG/GIFと検証手順。
- 画像原案、前景調整素材。
- QAの保存/除外Pathと、開始時・commit前のProduction/Build保護照合。

Productionファイルは既存の承認SHAと14/14一致。新しいGameplay/Camera/Scene/BGM変更や再Buildは行わない。

本資料を含む容量監査は、背景の採用と目的が異なるため `docs: add disk usage audit` の別commitに分離する。push完了の確定はpush後のHEAD/origin/main照合で行う。

## 引き続き未保存・保持

- MainStage.unity、README.md、Docs/LEARNING_LOG.mdの既存差分は今回対象外。
- BGM実体はGit除外・必須外部依存を維持。
- output、Builds、.codex_tmp、Unity caches、以前のWindows Buildはstageしない。
- 2.5D QAの生Capture PPM、試作用ImportedMetadata、診断用Source snapshot、未保存ログはローカル保持。詳細は `../../QA/Tutorial2p5DIntroFix_20261002/Evidence/Checkpoint/LocalOnly.json`。
- 無関係な旧QA、ArchiveExpansion、__pycache__はstageしない。

監査報告の「未保存」は当時の状態として残す。本checkpointによってProduction一式が保存されても、全フォルダ・全Evidenceを保存したという意味ではない。

## Cleanupの境界

今回、削除・移動・rename・zip・disk cleanupは1件も行わない。

push後にHEAD/origin/main一致と保護照合が成功すれば、安全checkpointの目的は完了する。その後のcleanupは別承認が必要。候補Pathを再確認し、Unity/Buildの利用中状態を再確認する。未保存Scene差分・BGM原本・生Evidence等を一括削除しない。

前回候補の71.739 GiBは点-in-timeの論理容量であり、今後の実削除による空き容量保証ではない。
