# 候補2／候補1 — 実キー入力を妨げない手動専用隔離Build

手動版 r1を新規作成。**Compile／Windows Build／両候補の起動・無入力待機を確認。画面取得・OSキー反応・Human実操作は未確認。** 自動攻略の結果を実キー成功へ読み替えていない。

対象は[前回の起動経路監査](../20261017T104353_JST/Report.md)で見つかった自動撮影専用構成の解消だけ。演出の追加改善、Q文言変更、地形変更、Production統合は行っていない。A2 r1の不採用推奨、候補1／2の採否未承認を維持する。

## 起動するもの

ローカルの新規フォルダ `PlayerSwingManualBuild_20261017` に以下を用意した。[正確な相対配置・起動／終了方法](Launch.md)。既存PCの絶対位置は依頼者への完了返信で案内する。Build本体はGitHubへアップロードしていない。

- `START_Candidate2.cmd`：候補2。通常はこちらをダブルクリック。
- `COMPARE_Candidate1.cmd`：候補1。同じexe・同じA2 B・同じ1280×720で比較。
- `Build/PlayerSwingManual.exe`：引数なしで直接開いても、既定は候補2の手動版。
- 終了：ウィンドウ右上の×、または選択中のゲームでAlt+F4。Escは既存ポーズ。

一方を終了してから、もう一方を開く。新しいゲーム内切替UI・入力モードは追加していない。

## 最小変更

表示の初期化を `PresentationCapture.Start()` から切り離し、A2入口の準備後に表示用 `PlayerMotionPresentation` だけを追加する。候補2の `LateUpdate`／描画メッシュ計算はそのまま。姿勢は既存 `PlayerMover.MovementInput` と実際の速度・接続状態を読むため、自動ドライバーがなくても通常入力に応じる構成。

`Time.captureFramerate=0` とし、オフライン30fps時計を撤去した。`A2Fixture` の自動撮影・QA登録・smoke終了も遮断。`PresentationCapture`、`A2ScriptedQA`、`A2InputDriver` は未使用コードとして残るが、手動版で起動・入力注入できないガードを設け、両実行でインスタンス0件を確認した。[起動経路監査](StartupAudit.md)／[変更理由](CodeDiff.md)／[正確なコード差分と新規表示bootstrap](CodeDiff.json)。

**既存A2の入口初期化は残る。** 地形作成、入口への一度の初期配置、残量24・選択6・CP設定は元のProofの契約。これは自動攻略や試験中のテレポートとは区別する。入口準備後の観測は読み取りのみで、接続・解除・移動・ポーズ・終了を行わない。

## 実測結果

| 項目 | 確認結果 |
|---|---|
| Unity 6000.3.21f1 Compile／Windows Build | 成功、Buildエラー0・警告0。出力535,574,817 bytes |
| 既定候補2 | 候補指定引数なしで起動。`secondCandidate=true`、表示コンポーネント1 |
| 候補1 | `-presentation candidate1` で起動。`secondCandidate=false`、表示コンポーネント1 |
| 自動入力／QA／撮影 | 両方の全4観測点で各0コンポーネント。新規PNG／JPGなし |
| 無入力待機 | 各初期化後35秒以上、プロセス生存。残量24・選択6・橋0・未接続・入力0を維持 |
| 実時間 | `captureFramerate=0`、`captureDeltaTime=0`、`timeScale=1`。scaled／realtimeの経過差0.25秒未満 |
| 入口の状態 | 接地・Mover／Rope enabled・Body simulated、記録したGameplay Error 0 |
| 同条件比較 | A2地形・Physics／Camera設定が両候補で一致（Unity instanceIDを除外） |
| 通常入力の受付経路 | 元のBuilt-in Input Manager読取コードを保持し、自動入力の上書きなし。コード上確認 |
| OSキー→画面反応、ダブルクリックそのもの | **未確認**。今回sky再試行0回、画面取得0、OS入力0 |
| E／Q／F／R／Escの実操作、動く演出の見た目・音・手応え | **未確認**。この作業でAPI攻略・自動撮影を繰り返していない |

[待機結果](QA/ManualReadiness.json)／[候補2最終観測](QA/DefaultCandidate2_Manual_03_Idle35s.json)／[候補1最終観測](QA/Candidate1_Manual_03_Idle35s.json)／[Build SHA・全出力manifest](QA/BuildIdentity.json)／[重要ログ抜粋](QA/ImportantLogExtract.txt)。shaderのサポートと表示コンポーネントの生成は確認したが、実画面の品質確認の代わりにはしない。

検証は通常のstandaloneをプロセスとして起動し、状態JSONを読むだけ。ゲーム内に自動終了はない。35秒記録後、検証シェルが自分で起動した新BuildのPIDとexe絶対パスを照合して終了した。既存Buildや他のプロセスは停止していない。[候補2プロセス記録](QA/DefaultCandidate2_Process.json)／[候補1](QA/Candidate1_Process.json)。

Buildの初回起動は継続していたが、権限境界内のプロセス照会で見えず、追加のEditor起動確認を1回行った。この追加プロセスは同一Projectを開く前にreturn code 1で終了。元のプロセスが完成させたBuildだけを採用し、新規出力は1組。追加確認の失敗をBuild成功へ数えない。

## 保持と未確認

[保護照合](QA/SourceProtection.json)：指定5主要コード、全Scene・Packages・ProjectSettings、既存Art／Shader／音・影・着地の素材と処理を保持。差分は隔離コピーの起動処理4ファイル＋新規bootstrap／metaの2ファイルだけ。振り子演出本体の計算は同一。Productionのbranch／HEAD／Git差分／stage／未追跡一覧と選定11ファイル、凍結A2・候補1／2のSource、旧r1/r3 exe・Assemblyのhashも開始時と一致。

main・Production・ponのbranch／成果物への変更なし。共有はこの新規runと固定入口だけ。Runtime Source／Build／Library／BGMはGitへ追加しない。

## 過去の比較動画を保持

今回の新規動画・画面画像はない。[既存の通常速度比較・JPEG連続コマ](../20261017T011344_JST/Report.md)／[主比較動画](../20261017T011344_JST/Videos/SideBySide_30fps_silent.mp4)／[左右頂点補助動画](../20261017T011344_JST/Videos/FreeCycle_SideBySide_30fps_silent.mp4)は保持している。これらは**過去の自動入力／内部API撮影**であり、新しい手動版のOS操作証拠ではない。

**手動専用Buildの準備完了。実キーでの成立・反応・Visualの採否はHuman確認待ち。ここで停止する。**
