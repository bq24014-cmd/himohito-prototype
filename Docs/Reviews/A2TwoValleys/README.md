# ヒモヒト：A2関連の固定レビュー入口

固定branch：`review/a2-two-valleys`。[LATEST.json](LATEST.json)の最新報告・画像・QAを同じcommitから読んでください。ZIPの再添付は不要です。

## 最新：接続→身体を使う振り子の新候補1案

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
