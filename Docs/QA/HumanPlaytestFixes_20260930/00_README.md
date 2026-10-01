# Human Playtest Fixes — 2026-09-30

対象は HimoHitoPrototype の H1（ゲーム内看板）、H2（クリア中の足音）、H3（本編第9区間の2本目照準）、既知 S2（看板3の残量例）だけです。新機能・Scene・難易度変更、commit、push は行いません。

基準：main / HEAD / origin/main = `7644fdcdf7adb793fcc6e3ac9d6540f317766ec6`。開始時 fetch 後も一致。

既存 MainStage.unity、output/、過去QA、.codex_tmp/ は保護対象。Evidence/sha_start.json に開始時の 10,876 ファイルの SHA-256 を記録しています。診断はこのTask専用の新しい隔離Unityコピーで行い、本体のScene・ProjectSettingsをUnityで保存しません。

## 証拠の種類

- Humanの実操作結果は 01_HumanFindings.md の通り。
- 今回の自動診断は、隔離コピーで PlayerMover の Input.GetKey(A/D)、Input.GetButtonDown(Jump) の戻り値を供給する入力seamです。Update / FixedUpdate / AudioSource.PlayOneShot の実経路を検証しますが、OSキー送信・人間の実操作ではありません。
- Runtime PNG は Camera.Render によるゲーム内描画です。world は実カメラ、detail は確認用拡大。IMGUIの全画面UIは含まれません。看板の画像・配置・フォントサイズを加工していません。
- Windows computer-use の list_apps が2回、カーネル再初期化後にも1回タイムアウトしたため、OS実キーによる追試は未実施です。
- 診断コードは .codex_tmp/HumanPlaytestFixes_20260930/Project のみ。本番Scriptsには入力seamや計測カウンターを入れません。

結果・確定範囲は 07_FinalDecision.md を参照してください。
