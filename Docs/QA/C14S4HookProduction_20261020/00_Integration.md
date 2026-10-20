# C14 定規の緑リング Production統合

Human正式採用の「描画順だけの候補」をProductionへ統合した。S4の対象リングを描画順6→8にし、固定板・影・ピン・結びは従来の表示を保持する。旧6190090の斜め固定板案は不採用のまま保持し、回転-135度・移動+0.15は取り込んでいない。

## 承認元と変更

- 承認元：[`9c2f2851143ccaae69b864c1580ef2bd5949491a`](https://github.com/bq24014-cmd/himohito-prototype/blob/9c2f2851143ccaae69b864c1580ef2bd5949491a/Docs/Reviews/C14S4Hook/runs/20261020T110726_JST/Report.md)。
- 統合前main：`1ffac00416db372b53c6df04273bf11557ac8a7d`。
- 実装変更：`Assets/Scripts/MainS4RulerVisual.cs` 1本、26行追加。採用候補のソースとSHA256が完全一致する。
- 実装SHA256：`E2E64E1681445326CB824192A207E7B970C31A80FCB5DC775884D001D0999004`。
- 定規の素材・位置・寸法・Collider、HookのTransform・照準対象・接続点・橋端点は無変更。共通フック処理、入力、物理、残量、他区間の構成は変更していない。
- 比較切替、診断fixture、撮影、終了処理、Proof用設定はProductionへ追加していない。

## 統合ソースの確認結果

Unity `6000.3.21f1` 用RoslynでProduction Runtime 119本だけをコンパイルし、エラー0、既存CS0649警告4件。実行時は同じProductionソースに隔離診断fixtureのみを加え、既存非Development Playerへmanaged overlayとして配置した。**新規Unity Build成功・Editor Play成功とは扱わない。**

| 確認 | 結果 |
| --- | --- |
| 未接続・照準 | 実Unity画面と値を確認。照準対象は `(68.3,-0.65)` の対象緑Hookと一致。 |
| E接続 | 既存APIで接続成功。接続完了時、ヒモ終端・anchor・リング中心の差0。 |
| Q生成 | 既存APIで長さ5の橋を生成。残量45→40。橋の対象端点は同じanchor。 |
| R | 既存復帰APIで入口、残量45、選択長5、橋0、未接続、接地へ復帰。 |
| Scene再読込 | MainStage再読込後もリング描画順8。描画208件は初期・R後・再読込後とも採用候補と完全一致。 |
| 固定具7部品 | 固定板、影、左右ピン、結び、左右糸端の位置・角度・寸法・描画順等が採用候補と一致。定規・Hookの物理契約も一致。 |
| ログ | 正常完了、Error／Exception 0件。 |

[Verification.json](Evidence/Verification.json) に実行方式、状態値、再読込後の固定具7部品、ソース・実行DLLのハッシュを記録した。

## 保護と未確認

244ファイルを照合し、今回のMainS4RulerVisual.cs以外の243ファイルは無変更。C08・C20・C22/C23、既存MainStage Scene・README・学習ログ・QAログの差分、採用元と不採用の過去Proofを保持した。

確認は隔離fixtureからの既存E/Q/R API・Scene再読込であり、OS実キー・統合後Human再試遊・音・新規Unity Build・Editor Playは未実施。採用判断は今回のHuman正式承認として記録し、再承認待ちには戻さない。他の改修には着手していない。
