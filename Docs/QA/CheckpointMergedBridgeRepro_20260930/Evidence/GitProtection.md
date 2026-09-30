# Git protection

開始時: git fetch origin、git status、git branch --show-current、git rev-parse HEAD、git rev-parse origin/main、git log -5 --onelineを確認。

fetchはsandboxのFETCH_HEAD書込制限、およびownershipのsafe.directory警告により通常実行が失敗した後、通常権限で `git -c safe.directory=<このHimoHitoPrototypeの絶対パス> fetch origin` を実行して成功。global Git設定は変更しない。pull/merge/rebaseなし。

開始・終了ともmain、Local HEAD / fetched origin/mainはf0b0709b446a7f4372f2223aad38acaebaf8c6af。

開始時の未コミット: Assets/Scenes/MainStage.unity。未追跡: output/。stagedなし。

終了時: 上記はそのまま、要求されたDocs/QAだけ新規追加（未追跡）。診断コピー/runnerは.gitignore対象の.codex_tmp内。

Assets/Packages/ProjectSettings/outputの464ファイルを開始・終了SHA256で照合、全一致。既知のMainStage SHA:

57D2464751B5B9C2A78E6C56F34197E9BC2629983FADAA1DFF6E0BA0B4BC2284

詳細: sha_start.json / sha_end_comparison.json / environment_and_git.json。Unityで開いたのは隔離コピーのみで、診断Scene2件も元SceneとSHA一致。

reset / clean / stash / checkout上書き / add / commit / pushは未実施。DESERT LOOPへのアクセスなし。git diff --cached --statは空。

終了時git diff --checkは、既存未コミットMainStage.unityの空m_Name行の末尾空白を報告した。Sceneの開始/終了SHAが完全一致しているため今回追加した差分ではない。保護ルールに従い整形・修正しない。
