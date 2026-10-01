# 提出時snapshot照合

過去の説明だけでなく、以下の実ファイルを読んで確認した。

- `.codex_tmp/SubmissionBuild20260911/Assets/Scenes/MainStage.unity`
- `.codex_tmp/SubmissionBuild20260911/source-manifest.json`のMainStage entry。
- 同snapshotの`QA_README.md`の作成経緯。

| 比較物 | SHA-256 |
| --- | --- |
| 現在Working Tree Scene | `57D2464751B5B9C2A78E6C56F34197E9BC2629983FADAA1DFF6E0BA0B4BC2284` |
| 提出用snapshot実Scene | `57D2464751B5B9C2A78E6C56F34197E9BC2629983FADAA1DFF6E0BA0B4BC2284` |
| 提出用source-manifest entry | `57D2464751B5B9C2A78E6C56F34197E9BC2629983FADAA1DFF6E0BA0B4BC2284` |
| 最新HEAD Scene | `B6191C44EE8FC841784B4C10EB1940DBA5220EAE112165FD09353D32AF879151` |

**WORKING TREE = SUBMISSION SCENE**。SHA一致に加えバイト列の直接比較でも一致。HEADのみ異なり、差分内容は02の表に尽くされる。

証拠コピー：`Evidence/Submission_MainStage.unity`、`submission_manifest_scene_entry.json`、`submission_provenance_readme.txt`、`serialized_summary.json`。

snapshot READMEでは、提出buildはその後のQA script投入前に成功し、元revisionは`f0b0709b446a7f4372f2223aad38acaebaf8c6af`＋既存Scene変更だったと記録されている。これは現Sceneの歴史的provenanceを支える。snapshot projectには後から診断設定が入ったので、このprojectを再配布用に再buildすることは推奨しない。

今回確認した一致は**保存されている提出用Source snapshot**との一致。応募先アップロード済みZIPを新たにダウンロードして検査したものではない。提出履歴全体の再検証やBinary内Sceneの逆解析は行っていない。
