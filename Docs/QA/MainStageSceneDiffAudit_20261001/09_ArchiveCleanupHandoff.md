# Archive / Production整理への引継ぎ — 2026-10-01

この追記は既存00〜08と生Evidenceを変更せず、今回のArchive検証・HEAD復元結果を記録する。

- CASE S4の提出用Working Tree SceneをZIP保存。
- 実ZIP展開後SHAは`57D2464751B5B9C2A78E6C56F34197E9BC2629983FADAA1DFF6E0BA0B4BC2284`で一致。
- Archive README / SHA256.txt / HEADとの差分patchも保存。
- 前回のRecheck_20261001.json、Runtime14 nodes一致、Visual3組0pixelの生Evidenceを保持。
- Archive PASS後、許可されたMainStage.unityのみHEAD版へ戻した。Scene diff=0。
- HEAD版Scene SHAは`B6191C44EE8FC841784B4C10EB1940DBA5220EAE112165FD09353D32AF879151`。
- 新規clean HEAD exportのEditor診断ではMainStage 148 assertions PASS。
- Windows Buildは外部配置BGM欠落で停止。SceneのRuntime差分が原因ではない。
- 総合Cleanup PASSは未成立。stage / commit / pushは未実施。

詳細: [MainStageCleanReproduction](../MainStageCleanReproduction_20261001/00_README.md)

この時点で過程の保存はローカル完了、GitHub保存はまだ完了していない。
既存監査の「Working Tree不変更」保護記録は**監査実施時点**の結果であり、その後の明示的許可によるHEAD復元とは区別する。
