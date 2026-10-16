# Human Retest — 2026-10-01

対象：Human Playtest修正 H1 / H2 / H3 / S2。

追試Build：`HumanPlaytestFixes_20261001_065300`。現在のWorking Treeから生成したWindows Buildで、人間による追試が完了した旨と「全項目問題なし・採用」をユーザーから受領しました。

この記録はユーザーによるHuman追試結果・採用判断です。過去の自動診断やexe起動スモークをHuman PASSとして扱ったものではありません。試行回数・スクリーンショット等の追加情報は今回報告されていないため、推測で補っていません。

| ID | 確認内容 | Human判定 |
| --- | --- | --- |
| H1 | Tutorial看板可読性 | PASS |
| H2-T | Tutorial Clear後A/D足音なし | PASS |
| H2-M | MainStage Clear後A/D足音なし | PASS |
| H3 | Section 9右Hook照準改善 | PASS |
| S2 | Tutorial看板3「99→92」 | PASS |

**Human Review = APPROVED**

**H1 / H2 / H3 / S2 = CLOSED**

## 保存対象

Production Sourceは以下の4件だけです。追加修正はありません。

- `Assets/Scripts/PlayerMover.cs`
- `Assets/Scripts/RopeController.cs`
- `Assets/Scripts/TutorialSectionGuide.cs`
- `Assets/Scripts/TutorialSignInscription.cs`

加えて `Docs/QA/HumanPlaytestFixes_20260930/` の原因確認・修正・回帰・Build/起動証拠と本Human追試承認記録を保存します。

## 記録の時系列と保護

`07_FinalDecision.md` と `HumanRetest_20261001_065300/` の資料にある「Human追試未実施」は、それぞれ作成時点の記録です。本書が、その後実施されたHuman追試の最終採用結果を追加します。以前の証拠や空の結果テンプレートは書き換えません。

Build本体は `output/HumanPlaytestFixes_20261001_065300/` に保持し、Gitへ追加しません。QA内のBuildログ・起動ログ・manifest情報は検証証拠であり、Build本体ではありません。

`Assets/Scenes/MainStage.unity` の既存未Commit変更、README / LEARNING_LOGの既存変更、output、.codex_tmp、過去Build、他の既存未追跡物は今回の保存対象外です。stageしません。

基準：branch `main`、開始時HEAD / origin/main = `7644fdcdf7adb793fcc6e3ac9d6540f317766ec6`。

今回のcheckpointに新しい実装・追加調整は含めません。
