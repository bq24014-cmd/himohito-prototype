# Final decision

**Checkpoint Restore Fix = PASS**

指定したA〜Fの制御Runtime回帰は全て成功。重大失敗条件（参照失効、橋復元崩れ、保存残量の変化、未解除Hookのinactive化、リスト増殖、Tutorial進行回帰、Scene変更の必要）は観測されていない。追加のProduction修正を重ねていない。

## 1. Git保護

開始時git fetch origin成功（最初のsandbox実行はFETCH_HEAD permission denied、その後承認された実行で取得）。git status/branch/HEAD/origin/main確認。main、HEAD=origin/main=f0b0709b446a7f4372f2223aad38acaebaf8c6af。終了時も同一、stagedなし。

開始SHA対象1453ファイルを終了照合し、意図したSource3ファイル以外の1450ファイルは同一。Production Assets/Packages/ProjectSettings、root管理ファイル、既存Docs、output、前TaskのQAおよびRepro隔離コピーを含む。予期しない変更0。

MainStage.unityの開始/終了SHA256: 57D2464751B5B9C2A78E6C56F34197E9BC2629983FADAA1DFF6E0BA0B4BC2284。開始時からの未Commit差分はそのまま保持。全Scene/.metaのProductionと診断コピーSHAも一致。

その他の古い.codex_tmp 2935ファイルには開始SHAを取得していないため、開始終了SHA一致の主張に含めない。終了時inventoryでは全て開始基準時刻以前の更新時刻で、今回の操作は新規Fix診断ディレクトリ内のみ。これらも削除・上書きしない。

変更Sourceのgit diff --checkはPASS。全体のdiff --checkは既存MainStage.unityのm_Name等の末尾空白を検出。Sceneの開始/終了SHA同一を確認しており、今回はその既存差分を修正していない。

## 2〜4. Source / snapshot / identity

変更: RopePlatformBuilder.cs、MainStageRespawnOnFall.cs、PrototypeRunController.csのみ。

snapshotへ解除済みHookのGameObject[]を追加。橋のsnapshotと同時取得し、復元後にinactive状態とremovedHooksリストへ戻す。参照配列はlive listとは独立。

HookはR中にDestroy/recreateされない現行経路をSourceで確認し、同一Scene・同一Play sessionの参照方式を採用。Runtimeで中央Hook InstanceIDと有効性がR各回で一致。永続IDへの変更は行わない。

## 5〜9. MainStage / repeated R / unmerged / rope / selection

統合済みcheckpoint10からRを3回。中央Hook false/false→false/false、橋1→1、removedHooks1→1。全回で同じ中央参照、端点(180,-2.15)→(191,-2.15)、橋長12。残量38・選択長6・区間10が復元。

未統合2本も保存時の橋形状/長さで復元、removedHooks0、中央Hookを含む全通常Hook状態が不変。

橋生成なし/F未使用ケースは残量50・選択7・橋0・removedHooks0を復元、全Hook状態不変。

## 10. Tutorial

開始から区間1→2→3→4の物理進行、長さ7のT3橋、区間2/4入口R、T4の2本生成/F統合を確認。merge後checkpointは追加せず、最後のRで既存の統合前T4入口へ正常復元。残量92・選択7・既存T3橋1本・中央Hook active・解除リスト0が正しい。

## 11. 警告 / Exception

- 修正SourceのC# compile error/warningなし。
- NullReferenceExceptionなし。ゲームSource由来の新規Exceptionなし。
- 各起動ログにUnityEditor.Search.SearchDatabase起動時ArgumentOutOfRangeExceptionあり。前TaskのMainStage_runtime.log/Tutorial_runtime_attempt3.logにも同じstackが存在する既存Editor問題。ゲーム復元の失敗とは区別し、未解消のまま記録。
- Licensingのaccess token更新エラーあり。Editorは起動/compile/PlayModeテスト完了しているが、ライセンス関連ログが完全にcleanという意味ではない。
- Gitの隔離instrumentationにCRLF→LF通知あり。Production差分の空白チェックとは別。

## 12〜13. 判定 / STOP

A〜F PASS。診断操作による同一session Runtime範囲の結果。手動キーボード・配布build・Sceneを跨ぐsaveは対象外。

commit/push/stageは未実施。HEADとorigin/mainは不変。ここで停止し、所有者の確認を待つ。
