# 外部BGM付きWindows Build / Smoke結果

## Windows Build

HEAD `4919233cd1d0815afb438bd35a7336492ec09548`の新規exportに、承認済み外部BGMを配置。
既存のWindowsPrototypeBuilder.BuildFromCommandLineをそのまま実行。
`Evidence/ExternalBGMBuild.log`で `Build Finished, Result: Success.` と379.7 MBの出力を確認。

新規保存先:

`output/MainStageCleanReproduction_ExternalBGM_20261001_151833/Game/HimoHitoPrototype.exe`

exe SHA256: `837986201D89BE25426B8415CB3BC945F077E5B13F4722D6763D04EBD38C5472`

日本語パスの既知制約を避けるため、今回専用の新規ASCII起動コピーも作成:

`C:\Users\Public\HimoHitoCleanRepro_ExternalBGM_20261001_151833\HimoHitoPrototype.exe`

145ファイルすべてのコピーSHA一致。古いBuild / output / 既存Publicフォルダを上書きしていない。
既存Builderの汎用START_HIMOHITO.batは使用せず、専用パスから起動。

## exe確認の根拠

process ID 6388、起動日時2026-10-01T15:29:54+09:00、PlayerログでD3D11 / Mono / Input初期化を確認。
computer-useスキルのWindows補助機能は再試行・再初期化後もtimeoutしたため、それ以上のUI自動操作を停止。
agentがexe画面を見た、実キーボードを操作した、とは扱わない。

ユーザーへの確認質問:

> Stage Selection表示 → Tutorial開始 → Stage Selectionへ戻る → MainStage開始が正常にできるか

回答: **「完璧」**。この質問の範囲をHuman確認PASSとして記録。
Stage 5～10の新しいexe通しHuman承認まで意味を拡張しない。

## Editor controlled smoke

診断コードは別QAProjectにのみ追加。Production source / Sceneには追加しない。
Full: **148 assertions / 0 failures**。Focused: **32 assertions / 0 failures**。

|確認項目|結果・根拠|
|---|---|
|Tutorial看板可読性|PASS: 4看板のE/W/S/Q/F暗色インク、PNG目視確認|
|Clear中A/D足音なし|PASS: Tutorial / Main各20回のcached A/D入力診断、footstep timer停止・AudioSource silent|
|Section 5遮光|PASS: 橋なしHazard / 橋の遮光 / checkpoint retry|
|Section 9右Hook|PASS: near 27/27、center 28/28、resolverとE highlight一致|
|Section 9 merge|PASS: 2橋Q生成、F merge、追加消費なし、実物理checkpoint10|
|Checkpoint Restore|PASS: Rを3回、merged橋len12・消したHook inactive・resource・collider維持|
|Section 10 Clear|PASS: long bridge len10生成・実物理移動・宝箱・Clear state・Stage Selection return|
|Tutorial看板3|PASS: committed sourceの「99 → 92」、93ではない|
|BGM|PASS: clip YasashiiOdori loaded、loop、isPlaying、samples=3287808 / stereo / 44100Hz|

これはOS keyboard / Human通しplaytestではない。controlled setupによる位置設定と既存API・物理・既存Update/FixedUpdate入力ガードの検証。
右HookのF除去用resolverはE接続用resolverと用途が異なる。前回の誤った同一性条件を繰り返さず、E highlightとE resolverを比較。F用結果もCSVに保存。

各Editor runで `UnityEditor.Search.SearchDatabase` 起動index例外が1件記録された。
生ログを保持し、0 runtime errorsとは表現しない。Production gameplay stackではなく、assertion失敗は0。
Batch Editor timingsはPlayer FPSではない。

## 重要: BuildコピーのScene自動更新

Build前: exportした461 committed inputsは全SHA一致。
Build後: Production / QAProjectのSceneはHEAD一致だが、**BuildProjectのMainStage.unityだけ不一致**。

- HEAD / Production: `B6191C44EE8FC841784B4C10EB1940DBA5220EAE112165FD09353D32AF879151`
- BuildProject after Build: `71D9DF053D63FD2F1B673CCC5A02FA26CCC6AE0956F3272A82C307FC97DE602A`
- diff: **697 insertions / 117 deletions**。空白だけでなくSprite size / scale等を含む。

既存 `MainStageSceneSync` はEdit Mode sceneOpenedでApplyCurrentScene→SaveSceneする。
Buildログ700～717付近にMainStage open→同Scene reimport。手作業の修正ではなく既存Build経路での生成・保存と整合する。
ただし今回、生成差分のRuntime / Visual等価性を新たに監査していないため、過去CASE S4の等価判定をこの差分へ流用しない。

指定の「検証コピーのScene差分0」を満たせていない。自動生成後にSceneを戻して隠す、同期コードを無効化する、Productionを修正することは行わない。
patch / reference / statをEvidence/external_build_scene_generated.*に保存。

## 保護

2186既存ファイルのSHA再照合で変更0。Production Assets / Scripts / Scenes / ProjectSettings / Packages / BGM原本 / 過去Build / 既存output / README / LEARNING_LOGを保護。
両検証コピーのcommitted Source差分0。Production MainStage Scene diff0、QAProject Scene diff0。
BuildProject Scene diffのみ上記の自動生成差分。stage / commit / pushなし。
