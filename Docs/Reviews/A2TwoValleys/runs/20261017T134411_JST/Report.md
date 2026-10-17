# 固定レビュー起動環境 — 腕の追加変更なし

対象候補は `3383ca9c6607ba81243d8947b01379c8724d9d31` の腕修正版のまま。Humanは身体との一体感の改善を評価し、現候補を保持、正式採用・Production統合は保留。フック巻き付き・足裏補正も保持する。

今回はゲーム内容ではなく、**固定パス・固定EXE名・非Development包装**だけを整えた。Windowsセキュリティ規則は追加・削除・変更していない。許可を広げる代わりに、手動レビューに不要な開発接続を除外した。

## 起動するもの

固定フォルダ：

```text
%USERPROFILE%\.codex\visualizations\2026\08\08\019fdfa3-5ee1-7911-8a24-63282af5bae4\HimoHitoReviewRuntime\
```

- **`START_Review.cmd`**：現在の腕修正版（candidate4）。
- `COMPARE_PreviousArms.cmd`：前回の線状腕（candidate3）。同じEXEを起動引数で切り替える。
- 固定EXEは `Windows\HimoHitoReview.exe`。Unity自身にこの名前でBuildさせ、`HimoHitoReview_Data`、MonoBleedingEdge、UnityPlayer.dll等を一式配置した。EXEだけの手動改名ではない。

[詳細な起動・終了・復元方法](LaunchAndUndo.md)。[腕の比較画像・動画](../20261017T125936_JST/Report.md)は前回資料を保持し、新しい腕素材や演出を作っていない。

## 現在のEXEと規則の実測

従来の最終EXEは同じvisualizationsルート内の

```text
PlayerIntegratedArmsProof_20261017\Build_r3\IntegratedArmsProof.exe
```

ローカルの `Evidence/Network_Before.json` には展開済みの絶対パスと規則名を保存した。GitHub資料には個人名入りのローカル絶対パスや端末IPは載せない。

- 現在のネットワーク分類はPublic。
- 上記r3の完全一致パスに、Public／Inbound／AllowのTCP・UDP規則が各1件。ポート・RemoteAddressは既存設定のAny。明示的なBlock規則は0。
- r1・r2にも、それぞれ別パス向けのTCP・UDP許可が存在した。EXE名が同じでも、パスが異なる許可は固定版へ引き継がれるものではない。
- 全プロファイルのFirewall有効・受信既定Block／送信既定Allow・通知有効を保持。既存の広い許可を今回さらに広げたり、旧Buildの規則を変更したりしていない。

Windowsのアプリ規則は完全な実行ファイルパスを使う。明示BlockはAllowより優先するため、許可追加の前に対象Blockの有無を調べた。[Microsoft公式：アプリ規則と優先順位](https://learn.microsoft.com/en-us/windows/security/operating-system-security/network-security/windows-firewall/rules)。

**パスが変わる構成と開発通信の存在は確認できたが、同一パスで通知が毎回出ることや、過去の各通知の直接原因を実画面で再現したわけではない。**

## 通信の用途と変更内容

元Buildは `BuildOptions.Development`。実起動ログにPlayer ConnectionのTCP接続情報（port55000）とUDPターゲット通知（multicast port54997）がある。Unityの開発・Profiler接続系の通信であり、今回調べたゲームSourceには手動レビューに必要な対戦・オンライン通信の実装は見つからなかった。ログの記載だけで全通信がこれだけと断定しない。

Unity公式ではDevelopment BuildがProfilerを有効にすると説明されている。[Unity 6000.3 API](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/BuildOptions.Development.html)。そこで隔離コピーにEditor専用Build入口だけを追加し、次を明示した。

- BuildOptions：None（Developmentなし）。
- Script Debugging：false。
- Autoconnect Profiler：false。
- Deep Profiling：false。

腕・フック・接地・物理・入力・地形・Camera・資源処理には一切変更しない。元の538ファイルはコピー後・Build後とも全ハッシュ一致。追加はEditor専用Builderとmetaのみ。[保持照合](QA/Protection.json)。

## 起動・停止・再起動の結果

同じ固定EXEで、修正版初回→外部停止→修正版再起動→外部停止→前回腕の比較起動を確認した。各回、初期化後35秒以上の無操作待機を観測した。

- 残量24・選択長6・橋0、エラー0、自動入力／撮影／QA driver 0、通常時計。
- 各回の対象PIDの観測サンプルではTCP／UDP端点0。Player Connection・UDP broadcastのログもない。
- 起動時に新しい固定EXE用Firewall規則は生じず、監査した既存規則とFirewallプロファイルは前後一致。
- 終了はパスとPIDを確認した外部プロセス停止。**実際の×・Alt+F4を押した正常終了の検証ではない。** 通常ランチャーは既存音を保持し、今回の待機診断だけ無音とした。

[起動観測の実測値](QA/LaunchSummary.json)／[Build identity](QA/BuildIdentity.json)／[Firewall前後比較](QA/FirewallSummary.json)／[ログ抜粋](QA/ImportantLogExtract.txt)。

## 追加許可と本人操作について

現在の固定手動版で、開発通信用の受信例外を追加する必要は観測されなかったため、**永続許可の新設は0件**。対象外アプリ・通知・Windows Firewall全体の変更も0件。必要な通信がない状態で、Publicへの受信許可を増やさない。

Humanの対象ゲーム限定の継続許可は保持・記録した。ただしcomputer-useスキルはWindowsセキュリティ設定の代理変更・セキュリティダイアログ操作を禁止している。必要な場合の設定は本人操作が必要で、今回はその代わりに設定を回避・弱体化する操作をしたのではなく、ゲーム側の不要な開発通信を包装から外した。

**OS画面取得ツールは今回も `computer-use request timed out: list_apps`。待機後の再試行と再初期化後も同じだった。Firewall許可不足とは断定せず、復旧したとも扱わない。**

したがって、通信許可画面の再表示がないことの実画面確認は未確認。プロセス生存・ソケット観測・規則不変だけで「絶対に通知は出ない」としない。本人が固定ランチャーを一度閉じて再起動し、通知が出ないことを確認すれば、この未確認を埋められる。もし出た場合は表示対象の完全パスと用途を確認してから、[本人用の確認手順](LaunchAndUndo.md)へ進む。

## 保護と停止

Productionのbranch／HEAD／tracked・untracked状態／stage、既存11保護ファイル、元腕Proof・Build_r3を保持。mainへのcommit・pushやゲーム本体のGit保存は行わない。レビューbranchにはこの報告・QA・包装手順だけを保存し、Build一式・Library・BGM・Windows許可画面の写真は載せない。

Technicalの起動環境は実測範囲で成立。**通知の非再表示は本人画面確認待ち、腕の正式採用・Production統合は保留**として停止する。
