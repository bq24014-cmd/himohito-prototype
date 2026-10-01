# Deterministic MainStage Scene Sync — 2026-10-01

**CASE D1 / MainStage Scene Cleanup = PASS。commit / pushは未実施。**

ユーザーの承認により判定基準を「Build後Scene不変」から「同じclean入力・既存Build手順から同一Sceneを生成」へ変更。
過去のFAIL記録は、その時点の不変条件での結果として保存。今回の結果が最新判定。

## 再現条件

**PASS: clean HEAD + documented external BGM dependency + Unity 6000.3.21f1 + existing deterministic MainStage Scene Sync**

- Repo: `bq24014-cmd/himohito-prototype`
- branch: `main`
- HEAD / origin/main: `4919233cd1d0815afb438bd35a7336492ec09548`
- 外部BGM: `Assets/Resources/Audio/YasashiiOdori.mp3`
- BGM SHA256: `25C52DA234D42C7A5E5CC91D45AEED14829F845EA15932A88AC94AB006781E83`
- Unity: `6000.3.21f1`、Windows Build Support、D3D11。
- 既存手順: `HimoHitoEditor.WindowsPrototypeBuilder.BuildFromCommandLine`。
- Scene生成: 既存MainStageSceneSync / MainStageFloorCollisionSetup / setup codeをそのまま実行。
- exeの日本語パス問題を避けるため、新規ASCII起動コピーを使用。

**HEADだけでSceneファイルが不変、とは扱わない。** 外部BGMなしのBuildは既存チェックで停止する。
この結果は現HEAD・Unity・Build手順での実測。別Unity版や違うEditor起動順まで保証しない。

## 独立A/B

`git archive HEAD Assets Packages ProjectSettings`から完全に別々のCopyA / CopyBを作成。
Library、Temp、既存Working Tree Scene、以前のBuildを入力に流用しない。
各copyの461 committedファイルのSHA一致と、BGMコピーSHA一致をBuild前に確認。
A/B Build前には診断Scriptを追加せず、同一の既存Build手順を実行。

新規コピー:

`.codex_tmp/MainStageCleanReproduction_20261001/DeterministicAB_20261001_154205/CopyA/`

`.codex_tmp/MainStageCleanReproduction_20261001/DeterministicAB_20261001_154205/CopyB/`

|項目|A|B|
|---|---|---|
|Build前Scene SHA|B6191C44EE8FC841784B4C10EB1940DBA5220EAE112165FD09353D32AF879151|同左|
|Build後Scene SHA|71D9DF053D63FD2F1B673CCC5A02FA26CCC6AE0956F3272A82C307FC97DE602A|同左|
|Build後Scene size|526,820 bytes|同左|
|Windows Build|SUCCESS / 379.7 MB|SUCCESS / 379.7 MB|
|Full controlled runtime|148 tests / 0 failures|148 tests / 0 failures|
|Focused controlled runtime|32 tests / 0 failures|32 tests / 0 failures|
|exe起動・開始|Human PASS|Human PASS|

Build後バイト完全一致。前Taskで生成されたSceneのSHAとも一致。
`git diff --no-index`を各before/afterへ実行し、A/B別ファイル名だけをheaderで統一した比較でも完全一致。
本文・hunk・値・fileID・serialization順は正規化していない。
normalized git diff SHA: `374F9E2B43292F3AA3E770BDAAD3A9869DF8CF2E30F3E2A20683ACAB631520F1`。
raw git diff、normalized diff、byte保存したbefore/after SceneをEvidenceに保持。

## Scene Syncが保存原因である直接証拠

A/B本Buildの入力を汚さないよう、別のclean TraceBuildProjectを作成。
追加したのはEditor限定の受動的なsceneSaving observer。既存Source、Scene、Syncを変更せず、同じBuilderを呼び出した。

観測した保存スタック:

`WindowsPrototypeBuilder.BuildWindowsPrototype → BuildPipeline.BuildPlayer → sceneOpened → MainStageSceneSync.OnSceneOpened → MainStageSceneSync.Sync → EditorSceneManager.SaveScene → sceneSaving observer`

MainStageSceneSync.Syncの49行はSaveScene、45行はApplyCurrentScene条件。
TraceBuildも成功し、生成Scene SHAはA/BのBuild後SHAと完全一致。
これにより今回のScene生成は既存Syncからの保存と確認できる。未知のGameplay変更を導入していない。

補助的なCauseProject（BuildせずSceneを直接open）は同じSync保存スタックを示したが、SHAは
`EEED2B03DE460B4AE3EBD4EB1DEE65B49543DA1C8BDE5E51A2D0028AF1B2318E`。
**手順が異なるこの結果を同じBuild試験として扱わない。** そのためBuild中のTrace観測を追加して一致を確認した。
「任意のEditor lifecycleで常に同一ファイル」との一般化はしない。

## Object / Component / Transform / Collider / setup / Visual

- 保存されたScene: A/Bとも135 GameObjects、731 serialized documents。全ファイルbyte一致なので全serialized値・参照・順序一致。
- Unityでopen後の全Scene snapshot: A/Bとも367 objects / 1,078 components。保存Sceneに含まれない生成描画子も含む。
- Component型 / serialized field、Transform、Collider、Section setup、Visual setupを全Sceneで比較: **意味的差分0**。
- Inspector raw snapshotには58箇所のprocess固有参照内部ID差。親のreferenceは階層・型またはasset GUID/localIDで保持し、そのreferenceの内部m_FileID等だけを除外。TransformやColliderやsetup値を丸めたり無視したりしていない。
- これらはsnapshot採取中の一時参照で、保存Scene自体のbyte差ではない。したがってD2ではなくD1。

## 過去CASE S4の14 nodes

同じ4 roots＋descendants（S05左右Shelf＋各Tie、S02 Front/Far Spike＋Face）を通常runtime setup後に採取。
追加のApplyCurrentScene呼び出しで結果を均していない。

- A / B: **14 / 14 nodes**。
- Transform、active、component、BoxCollider、SpriteRenderer、serialized MonoBehaviour fields: **差分0**。
- 以前のCASE S4 HEAD側Runtime14 snapshotと今回Aも **差分0**。
- `S05_Rails` / `S02_FrontSpike` / `S02_FarSpike`: 1280×720 render、A/Bとも **0 different pixels**。

## Runtime smoke / exe

各copyでBuildを終えた後に診断Scriptを追加し、生成Sceneからテスト。追加診断を含むexeは作成していない。
元のcommitted Source差分0、その他Scene差分0。Runtime後もA/BのMainStage SHAはBuild後SHAを維持。

各180 assertionsで確認:

- Tutorial看板4区間の暗色インク・可読性、看板3「99 → 92」。
- Tutorial / MainStage Clear中各20回cached A/D試行で移動・ジャンプ・足音timer停止、footstep AudioSource silent。
- Section5光Hazard / 橋遮光 / retry。
- Section6上route / lower air-chain API / retry、Section7複合進行。
- Section9右Hook resolverとE highlight、2橋生成、F merge、center消失、checkpoint10、Rを3回、merged橋・resource・collider保持。
- Section10 long bridge / 宝箱 / Clear / Stage Selection return。
- BGM clip / loop / playing / singleton。

controlled診断はOSキーボード通しplaytestではない。
各Full / Focused Editor runにUnityEditor.Search.SearchDatabaseの起動index例外が1件。既存のEditor-only例外として生ログとstackを保存。
0 gameplay assertions failuresだが、「全ログ例外0」とは表現しない。batch時間はPlayer FPSではない。

A/B新規Buildを各145ファイルSHA照合して別output / ASCIIパスへ配置し、両exe起動・Player D3D11 / Input初期化を確認。
computer-use Windows backendは規定の再試行・再初期化後もtimeoutしたので入力自動化を停止。
両exeのStage Selection表示・開始操作への確認質問にユーザーが **「はい」** と回答。この範囲をHuman PASSと記録。
全ステージの新たなexe Human通し承認へ拡張しない。

Build:

`output/DeterministicSceneSyncAB_20261001_154205/A/Game/`

`output/DeterministicSceneSyncAB_20261001_154205/B/Game/`

ASCII exe:

`C:\Users\Public\HimoHitoDeterministicAB_20261001_154205_A\HimoHitoPrototype.exe`

`C:\Users\Public\HimoHitoDeterministicAB_20261001_154205_B\HimoHitoPrototype.exe`

## 保護 / Git

Production・BGM原本・旧output / Build・過去Docs等、開始時2333ファイルのSHA再照合で **変更0**。
Production MainStage.unityはHEAD一致、Source / Scene / Packages / ProjectSettings差分0。
MainStageSceneSync、MainStageFloorCollisionSetup、setup codeの改修・同期停止は行っていない。
既存README / LEARNING_LOGの未Commit状態、過去Build、Submission Archiveは保持。
index空、HEAD / origin/mainは基準SHAのまま。**stage / commit / pushなし**。

今回QAフォルダは約84 MB、最大Evidenceファイルは約1.85 MB。BGM原本・Build・LibraryをQA保存対象へ混ぜていない。
大きいInspector raw / normalized JSONはlossless SceneSnapshots.zipへまとめ、展開内容SHAを検証。raw originalsとPPMは今回taskのRawEvidenceへ移動して保持、同一pixelのPNGをGit保存候補へ残した。

## 最終12項目

1. A/B Build前SHA: 両方HEAD `B6191C44…879151`。
2. A/B Build後SHA: 両方 `71D9DF05…DE602A`。
3. Build後byte: 完全一致。
4. git diff: headerのcopy別名以外、完全一致。
5. 原因: MainStageSceneSync→SaveScene。Build中stackと同一生成SHAで確認。
6. Build A/B: 両方SUCCESS。
7. exe A/B: 両方起動成功・Human表示開始PASS。
8. Runtime: 各180 assertions PASS、14 nodes一致、局所render3組0pixel差。
9. CASE: **D1**。
10. Cleanup: **PASS**。
11. Git保存: **可能な状態**。まだstage / commit / pushしていない。既存保護対象を含めずDocs候補のみを後続で明示指定する。
12. 新機能検討: **進める状態**。このTaskでは実装を追加せずSTOP。

根拠: `Evidence/DeterministicSceneSync/final_result.json`、`git_diff_comparison.json`、`TraceBuild/scene_saving_stack.txt`、`SceneSnapshots.zip`、各Build / Runtime / exe logs。
