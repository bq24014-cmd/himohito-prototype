# ヒモヒト：A2関連の固定レビュー入口

固定branch：`review/a2-two-valleys`。[LATEST.json](LATEST.json)の最新報告・画像・QAを同じcommitから読んでください。ZIPの再添付は不要です。

## 最新：元配置のR復帰後落下 — 状態同期1件

[20261017T195521_JST の報告](runs/20261017T195521_JST/Report.md)／[短い通常速度比較](runs/20261017T195521_JST/Videos/Retry_BeforeAfter_50fps_silent.mp4)／[原因の実測抜粋](runs/20261017T195521_JST/QA/CauseEvidence.json)／[手動起動](runs/20261017T195521_JST/Launch.md)。復帰後も古い物理位置履歴が残り、通常のすり抜け防止が入口への移動を壁横断と判定して押し戻す原因を確認。入力driver停止・Updateから共通Rでも再現。元地形のままR時の履歴／一時状態だけを同期し、失敗2条件がPASS、修正後9条件と既存33項目PASS。Rなしの長さ6／7軌道・入力・残量・Camera差0。OS実キー・Human手応え・音は未確認。斜面HOLD、腕要修正、A2不採用推奨を維持し、本番統合なし。

## 前回：谷1の壁衝突原因と出発床斜面1案 — HOLD

[20261017T191401_JST の報告](runs/20261017T191401_JST/Report.md)／[長さ7の同入力比較](runs/20261017T191401_JST/Videos/L7_Timed_BeforeAfter_50fps_silent.mp4)／[残る解除後の対岸衝突](runs/20261017T191401_JST/Videos/L7_EarlyRelease_BeforeAfter_50fps_silent.mp4)／[連続コマ](runs/20261017T191401_JST/Frames/)／[手動起動](runs/20261017T191401_JST/Launch.md)。接続中の出発側面と早い解除後の対岸側面を別々に特定。元壁を残す斜面で停止を減らす1案だが、継ぎ目接触・大きい衝撃・早期解除失敗が残るためHOLD。3攻略13/11/24・既存33項目PASSは品質採用の証明にしない。HumanはBodyPumpの腕輪郭を要修正・採用保留と評価。今回は腕・姿勢を変更せず、新案のHuman実操作は未確認。Production統合なし。

## 前回：編み腕を維持した身体加速・惰性の振り子演出1案

[20261017T160603_JST の報告](runs/20261017T160603_JST/Report.md)／[通常速度比較](runs/20261017T160603_JST/Videos/Main_BeforeAfter_30fps_silent.mp4)／[入力・惰性・制動比較](runs/20261017T160603_JST/Videos/Input_CoastDriveBrake_30fps_silent.mp4)／[連続コマ](runs/20261017T160603_JST/Frames/)／[新しい手動起動](runs/20261017T160603_JST/Launch.md)。基準は編み腕版3383ca9。入力・接線速度・振りの位相を共通の表示状態へまとめ、胴・腰・膝・肩を連動。415＋71フレームの物理・Camera・残量差0、非Development手動版35秒×2で自動driver0。OS操作は今回1回の一覧取得タイムアウトで未確認。通常画角の肩・肘はまだ弱いと自己評価し、Human採否・楽しさは未確認。Production統合なし。

## 前回：固定レビュー起動環境・追加の腕改修なし

[20261017T134411_JST の報告](runs/20261017T134411_JST/Report.md)／[固定ランチャー・復元方法](runs/20261017T134411_JST/LaunchAndUndo.md)／[起動・待受の観測](runs/20261017T134411_JST/QA/LaunchSummary.json)。同じ候補4を固定名HimoHitoReview.exeへ非Development包装し、候補3も同じEXEの別ランチャーで比較。3回の35秒待機で観測したTCP／UDP端点0、開発接続ログなし、既存Firewall規則は不変。許可追加・セキュリティ設定変更なし。Windows通知の非再表示はOS画面取得が使えず本人確認待ち。腕は一体感改善とのHuman評価、現候補を保持し正式採用・Production統合は保留。

## 前回：腕だけの改訂・編み目を胴へつなぐ1案

[20261017T125936_JST の報告](runs/20261017T125936_JST/Report.md)／[通常速度比較](runs/20261017T125936_JST/Videos/Main_SideBySide_30fps_silent.mp4)／[修正版単体動画](runs/20261017T125936_JST/Videos/IntegratedArms_Main_30fps_silent.mp4)／[通常画角と連続コマ](runs/20261017T125936_JST/Frames/)／[手動起動](runs/20261017T125936_JST/Launch.md)。Humanが評価したフック巻き付き・足裏補正を保持し、却下された線状の腕だけを胴と同じ編み目の表示メッシュへ見直した。自動比較318フレームの軌道・Camera・入力・残量等の差0、手動起動35秒・自動driver0。OS操作はツール一覧取得タイムアウトで未確認。Human採用は未承認。

対象ゲームの画面取得・既存入力・起動終了・比較録画・ログ確認の[継続許可メモ](runs/20261017T125936_JST/OperationPermission.md)を保存。許可とツール可用性は区別する。

## 前回：接続→身体を使う振り子の新候補1案

[20261017T120508_JST の報告](runs/20261017T120508_JST/Report.md)／[通常速度比較動画](runs/20261017T120508_JST/Videos/Main_SideBySide_30fps_silent.mp4)／[画像](runs/20261017T120508_JST/Figures/)／[手動起動方法](runs/20261017T120508_JST/Launch.md)。接続接地時の足裏補正、リングの縁への巻き付き、入力／速度に対応する肩・肘・胴を1案にまとめた。自動比較の軌道差0、最終手動版の35秒待機を確認。腕の質感等はまだ弱く、Human採用・OS操作・音は未確認。候補2・旧資料・Productionは保持。

## 前回：候補2／候補1 手動操作専用Build

[20261017T110609_JST の報告](runs/20261017T110609_JST/Report.md)／[起動方法](runs/20261017T110609_JST/Launch.md)。新しい隔離Buildで表示初期化と自動QAを分離し、候補2を既定にした。両候補のCompile／Build／起動・35秒以上待機・自動driver不在を確認。画面取得・OSキー反応・実操作感は未確認。Build本体はWindows側のみ保持、Human確認待ち。過去の自動動画・凍結Build・Productionは保持。

## 前回：候補2 OS実操作検証 — 未実施／阻害要因

[20261017T104353_JST の報告](runs/20261017T104353_JST/Report.md)。アプリ一覧取得が初回＋再試行1回ともタイムアウトし、画面・OSキー操作は未成立。また既存候補2 Buildは自動撮影専用構成で実キー専用起動経路が未準備と確認。追加実装・再Build・内部API代用・新規自動QAなし。今回の録画／画像／実操作批評はありません。Human採用は未承認のまま判断待ちです。

## 前回：振り子演出候補2（自動撮影比較）

[20261017T011344_JST の比較資料](runs/20261017T011344_JST/Report.md)。候補1／候補2の通常画角動画、連続JPEG、拡大補助、実測・隔離差分。Human実キー人数0、Visual採否未承認。Production統合なし。

## 以前の資料を保持

- [候補1・質感／振り子／着地](runs/20261017T000045_JST/Report.md)。これも正式採用済みではありません。
- [A2 r1本人の否定評価](runs/20261016T184703_JST/Report.md)。不採用推奨を維持。
- [A2 r1元の攻略／QA](runs/20261016T181643_JST/Report.md)。今回の演出比較を地形やワールドの採用へ読み替えません。

過去runは不変。レビュー資料とLATESTを同じcommitで更新。Unity Project・Build・Library・BGM・巨大な全ログは共有しません。mainへの反映・Production統合・ponのreview branch変更なし。
