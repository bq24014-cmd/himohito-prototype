# Human Approval / Production Checkpoint

## TP1 APPROVED

今回のProduction Build `TutorialTerrainHybridProduction_20261003` に対して、ユーザーから直接以下の回答を受領した。

質問:

> Production版「TutorialTerrainHybridProduction_20261003」を起動しました。今回はF7比較なしです。TutorialのIntroから全4区間・Clearまで遊び、5床の見やすさ、Goal床の継ぎ目、背景とのなじみに問題がないか確認してください。このProduction版を採用し、commit／pushへ進めてよいですか？

回答:

> 問題なし・採用してよい

結果: **Human Review PASS / Production採用APPROVED / checkpoint承認**。

先行TA1候補への承認とは別の、新しいProduction版への回答である。総合承認として記録し、操作ごとの動画や個別実測結果が存在するとは主張しない。941自動assertions、59pair比較、Build／DLL監査の証拠も維持する。

## 保存するもの

- 本体Runtime Source / metaと5枚のPNG / meta、folder meta（13ファイル）。
- 本QAの判定・移植説明・回帰・Human承認資料、再実行用検証コード、テキスト結果、SHA・比較manifest。
- 代表比較画像: 全体Overview、5床一覧、5床個別、Intro中間／終了直前、Merge、Clearing／Clear。

個別ファイルの確定リストとSHAは `Evidence/CheckpointStagePlan.json`。新規Runtime Sourceを変更せず、Human承認とGit保存範囲のみ文書化した。

## ローカル保持するもの

本QA全体は生画像等を含め約1.1 GiB。Gitには必要証跡を選別し、118枚の生PPMと選別対象外のPNGは**削除せずローカル保持**する。全59pairの結果・画像SHAは比較manifestへ保存済み。生画像を共有する必要が生じた場合は別途選別する。

Build、隔離Unity project、生成cache、BGM実体はGitへ追加しない。以前からのMainStage.unity / README.md / LEARNING_LOG.md / ExeA.log / ExeB.logの未保存差分は除外・保持。旧QA／旧Buildを上書きしない。

各種 `*Start/End.json`、`FinalState.json`、比較sheetの「Human Review pending」は**自動QA実施時点のスナップショット**として保持。現在の採用状態は本書と06_FinalDecisionを正とする。初回DLL監査FAILは検査コードのReflection例外であり、同一DLLをAudit02でPASSした履歴を消さない。

cached diff-checkの既知警告: 承認版とSHA一致するUnity metaの空値末尾スペース28行と、原文 `editor_search_errors.txt` の末尾空行1件。素材metaと生の診断記録を整形せず保持する。Source／文書／他の証拠は厳密なdiff-checkを通し、これ以外の警告は停止条件とする。

## Git手順

開始branch main、HEAD / origin/main: `c21463ce60f63ccbff78486a98210eac3aa21b1f`。fetchで確認済み、開始stageなし。

1. 承認済みSource／素材SHA、既存差分／BGM／出力保護を照合。
2. 確定manifestの各pathを個別にgit add。無関係ファイルをstageしない。
3. cached name-only / stat / status / diff-checkとSHAを監査。
4. `feat: adopt tutorial hybrid terrain visuals` でcommit。
5. fetchしてremoteが開始SHAのままである場合のみpush。
6. fetch後HEAD == origin/main、未保存差分／BGM／旧QA／Build保護を再確認し報告。

自己参照を避けるため、このcommit自身のSHAは本書へ埋め込まない。実行結果は最終チャット報告とローカルcheckpoint evidenceに残す。
