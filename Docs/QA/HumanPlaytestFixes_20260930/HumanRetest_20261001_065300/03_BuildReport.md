# Build / Startup / Protection Report

Build ID：`HumanPlaytestFixes_20261001_065300`

## 結果

- Windows x64 Build：**PASS**。Unity `6000.3.21f1`、`Build Finished, Result: Success.` を確認。
- 新規出力：`output/HumanPlaytestFixes_20261001_065300/`
- 専用起動：`START_HUMAN_RETEST.cmd` → `Launch-HumanPlaytest.ps1`。
- 実行先：`C:\Users\Public\HimoHitoHumanRetest_20261001_065300\Game\HimoHitoPrototype.exe`。
- Game：145ファイル、398,145,906 bytes（約379.7 MiB）。
- exe起動スモーク：**PASS**。専用ランチャーのコピー/ハッシュ照合後、82.4秒間生存。Unity初期化、Assemblyロード、Direct3D初期化を確認。Playerログの例外/致命的エラーは検出0。
- 終了：今回起動したPID 24904とexeパスを照合し、このスモークプロセスだけを意図的に停止。ユーザーのUnityや別ゲームは停止していません。

スモークはbatchmode起動確認です。GUIの目視確認・実キーボードでのHuman追試は **未実施**。5項目のHuman判定をPASSとしていません。

## 修正済みWorking Treeの反映

Root productionのAssets / Packages / ProjectSettingsを新規隔離コピーし、開始時462ファイルの一致を確認してから既存WindowsPrototypeBuilderを実行。以前の診断用コピーや、Input差し替え/Audioカウンターは使用していません。

Build後もコピー側Assets 438ファイルが元の入力と一致。Runtimeは本体と同じ113 Scriptsで、Assetsの変更差分0。TutorialとMainStageを含み、未CommitのMainStage.unityも同じバイト列を採用しています。

H1/H2/H3/S2のSource4件とMainStage.unityの入力SHA-256は `BuildManifest.json` に収録。今回の `Assembly-CSharp.dll` は `0EE54CD72172C4362B2BB3679B387527D217F7CA1DBEE0A8ECEC4F5DED27F5A3`、旧Human Buildとは異なります。Unityのengine exe自体は同じSHAになるため、exe SHAだけで修正の反映を判定していません。

## 保護

- HEAD / origin/main：`7644fdcdf7adb793fcc6e3ac9d6540f317766ec6`、branch `main` のまま。
- 本体Source・Scene・Assets・Settings・README・LEARNING_LOG・既存Docs・旧output・旧Buildの保護対象2,386ファイル：SHA-256差分0。
- MainStage.unityの既存未Commit変更を保持。
- H1/H2/H3/S2の既存未Commit変更を保持。追加修正なし。
- index空。stage / commit / pushなし。
- 過去Build / ランチャー / QAの上書きなし。新規QAは既存Fixesフォルダー内の独立HumanRetestサブフォルダーに収録。
- DESERT LOOPへのアクセスなし。

キャッシュLibraryは照合対象外。隔離コピー内のUnityインポート/Build生成物と新規Packet/QAだけを追加しています。既存の設定・クリア印は初期化していません。

`git diff --check` は既存未CommitのMainStage.unity内の `m_Name: ` 行末空白を報告しました。Sceneは開始時とSHA-256一致しており、今回発生した変更ではありません。追加修正禁止のため空白もそのまま保持しています。Source4件/README/LEARNING_LOGに限定したdiffチェックは問題なし。結果は `Evidence/GitDiffCheck.txt` に記録。

## Humanが次に確認する5項目

1. H1：Tutorialの看板1～4の可読性。
2. H2-T：Tutorial Clear後のA/Dで足音なし。
3. H2-M：MainStage Clear後のA/Dで足音なし。
4. H3：Section9の2本目右Hook照準。
5. S2：Tutorial看板3の99→92。

推奨順は看板1からTutorialを進め、看板3の説明、Tutorial Clearの音、本編第9区間、本編Clearの音。`01_ManualChecklist.md` と `02_ResultTemplate.md` を使用してください。

## Evidence

- `Evidence/WorkingTreeBuild.log`
- `Evidence/copy_verification.json`
- `Evidence/build_source_verification.json`
- `Evidence/build_packet.json`
- `Evidence/runtime_assembly_comparison.json`
- `Evidence/BuildAndSmoke.json`
- `Evidence/PlayerSmoke.log`
- `Evidence/git_start.json` / `Evidence/sha_start.json`
- `Evidence/protection_result.json`
- `Evidence/GitDiffCheck.txt`

起動時にDirect3D12のinfo queue照会警告がありましたが、以前のBuildにもある同じログで、その後Renderer/Assemblyは正常初期化。Unity Editorのライセンス接続は初回handshake後に復旧し、Buildは成功しています。追加修正は行っていません。

BuildとHuman追試Packet完成。ここで停止します。
