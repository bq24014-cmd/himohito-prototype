# Checkpoint時のdiff --check

2026-10-02。採用済みファイルは編集せず、stageした内容を監査した。

- Production C# / Shader本体の `git diff --cached --check` はPASS。
- Assets全体の同チェックはexit 2。警告は新規画像/フォルダのUnity生成.metaにある空のYAML値（userData、assetBundleName、customData等）の末尾空白。承認時SHAと一致する原本をそのまま保持する。
- QAを含む全体チェックには、過去の検証ログ/patch等にも空白警告がある。古いEvidenceは修正・整形しない。
- この警告を「全ファイルdiff --check PASS」として扱わない。Gameplayのコンパイルエラーや回帰失敗という意味でもない。
- indexの対象Pathは個別manifestと照合し、Scene・BGM binary・Build・一時Unity Projectが混ざっていないことを別に確認する。

詳細は `Evidence/Checkpoint/StageAuditProduction.json`。QAの古い「commit未実施」等の記述は当時の履歴として保存し、本checkpointの実施状況と区別する。
