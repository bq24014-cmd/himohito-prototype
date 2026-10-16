# pon｜ヒモヒト 夜間レビュー・現時点の読解案

**実プレイ不可、A=0。08:00 JSTの最終報告ではない。** 以下は既存証拠と10/6固定Buildソースの確認、未実施比較の提案を分けた訂正案。資料保存の承認は、ゲーム/UI変更や削除の承認ではない。

| 朝に必要な項目 | 現在の結論 |
|---|---|
| 対象と到達 | FriendPlay_20261006_214500 / HimoHito_20261006。exe/Assemblyはmanifest一致。今回Tutorial/Mainとも未到達、画面/通常入力/音0。最新版・全経路承認済みとは呼ばない |
| 率直な総評 | 設計上、T4の形変更とS6の資源選択は作品の核として残す。改善を先に試すのはQの生成先案内と、後半失敗からの再接近。現区間を新たに丸ごと削除する確証はなく、一括削除は勧めない。A=0のため、操作感・楽しさの実評価は未実施。 |
| 良い/弱い場面 | **設計上の強み2件**：T4は中央をFで外すと、同じ合計長の橋が下がり、操作の理由と形の結果が結び付く。S6は上橋へ14を使うか、下の連続振り子で残量を保つかを選べる。**弱み2件**：Qの「この照準方向」は実際の対フック間生成を直接表さない。S4/S7で同じ橋→振り子案を再採用すると、後半の失敗修正にも前半の橋準備・接近が付随する。誤解や面倒さを実観測したという意味ではない。 |
| 区間の推奨 | S1共通導入KEEP。S7はS6→S9接続/CP9/F解放を保つREWORK。S10全体REMOVEは撤回し、CP10・箱への接近/開き・終点判定を残す限定MERGE比較へ |
| 最優先UI | Q生成先が対のフック間なのはコード確認済み。「この照準方向」による誤予測と訂正文の効果は未実証。消費と不可理由を保つ文案比較が先 |
| 朝最初の1件 | Q案内の生成先/消費予測比較を実施するかHumanが判断する。比較未実施であり、実証済み不具合の修正依頼ではない。Tutorial T3/T4と、事前パネルが無いMain直入の観察を分ける |
| GitHubと版 | 公開状態はLATEST.jsonとremote照合済みcommitの親報告を参照。初期42資料/当時の承認待ち表記は履歴。追補の文案を実装済みUIとは読まない |
| 未確認 | 通常キー全経路、最初の3分、Q/Fの予測理解、失敗/復帰時間、実Window/focus/クリック、音、候補2。静的な入力経路は実再現ではない |

Production・既存差分・凍結Build・過去証拠は保持。Unity起動、再Build、ゲーム入力、コード/Assets変更、reset/clean/stash/削除は行っていない。

## 読む順番

- [ゲーム性・区間候補](01_Gameplay.md)、[UI案](02_UI.md)、[上位5案・朝の1件](03_Priorities.md)は訂正後の主資料として読む。
- 推奨範囲は[14](https://github.com/bq24014-cmd/himohito-prototype/blob/a31545d974ed4f5a1578b7c9a61698e4236f794e/Docs/Reviews/PonPlaytest/runs/20261017_032112JST/14_Merge_Removal_Refutation.md)、UIの条件付き案文は[12](https://github.com/bq24014-cmd/himohito-prototype/blob/a31545d974ed4f5a1578b7c9a61698e4236f794e/Docs/Reviews/PonPlaytest/runs/20261017_030141JST/12_UI_Copy_Conditions.md)、メニュー実体は[13](https://github.com/bq24014-cmd/himohito-prototype/blob/a31545d974ed4f5a1578b7c9a61698e4236f794e/Docs/Reviews/PonPlaytest/runs/20261017_031026JST/13_Menu_Input_Affordance.md)を優先する。
- 根拠は[06](https://github.com/bq24014-cmd/himohito-prototype/blob/a31545d974ed4f5a1578b7c9a61698e4236f794e/Docs/Reviews/PonPlaytest/runs/20261017_004500JST/06_T4_S10_Addendum.md)/[07](https://github.com/bq24014-cmd/himohito-prototype/blob/a31545d974ed4f5a1578b7c9a61698e4236f794e/Docs/Reviews/PonPlaytest/runs/20261017_004500JST/07_Resource_Retry_Addendum.md)/[09](https://github.com/bq24014-cmd/himohito-prototype/blob/a31545d974ed4f5a1578b7c9a61698e4236f794e/Docs/Reviews/PonPlaytest/runs/20261017_021315JST/09_FirstMinutes_InfoOrder.md)/[10](https://github.com/bq24014-cmd/himohito-prototype/blob/a31545d974ed4f5a1578b7c9a61698e4236f794e/Docs/Reviews/PonPlaytest/runs/20261017_023417JST/10_Retry_Learning.md)、未再現の入力候補は[08](https://github.com/bq24014-cmd/himohito-prototype/blob/a31545d974ed4f5a1578b7c9a61698e4236f794e/Docs/Reviews/PonPlaytest/runs/20261017_014618JST/08_UI_State_Addendum.md)、見せ場の比較計画は[11](https://github.com/bq24014-cmd/himohito-prototype/blob/a31545d974ed4f5a1578b7c9a61698e4236f794e/Docs/Reviews/PonPlaytest/runs/20261017_025050JST/11_Showcase_Prediction_Result.md)。
- 版・証拠の限界は[04](04_Evidence.md)、現文書のhashは[現行manifest](delivery-manifest.json)で確認する。[05](05_Cycles.md)・日時付き節・各runのstatusは当時の履歴。「未公開」「承認待ち」「未適用」「順位不変」を最新状態へ読み替えない。
- 03:43の適用manifestは449692c時点の履歴。後続更新された現文書へhashを書き換えない。最新判断はREADME・01・02・03、公開状態はLATESTを優先する。

## 評価区分

- **A：今回の通常入力による直接操作・画面観察。今回0件。**
- **B：複数の既存記録、画像、固定Buildソースを照合した所見。操作実証ではない。**
- **C：Human試遊・実Window確認が必要な仮説。面白さ・感情・時間・難易度の断定はここに含める。**

既存Human記録は「記録にそう書かれている」根拠として引用する。今回の試遊回数に加算しない。複数のAI検討周回は、複数人試遊ではない。PASSや到達可能性を面白さへ読み替えない。

## 範囲の扱い

大砲・ネズミProofは本編に含めない。A2 r1の不採用推奨は維持。候補2の未完成の効果を推定しない。候補2の担当者の識別と画面交代は未確認であり、操作が可能になっても交代確認前の入力はしない。

新たな敵・装飾・ステージ制作は提案の前提にしていない。削除は候補であり、Human承認・Production統合を代行しない。


## 親からの訂正・継続タスク停止

2026-10-17 00:54 JST：親が同一子タスクで継続を管理するとの指示を受領。今回子が作った時間単位の継続タスクだけを無効化し、成功/is_enabled=false/未実行を確認した。以前からの別予定は変更していない。資料をGitHubへ保存する前の親レビューは引き続き待機。


## Humanの環境承認後の再確認

Humanが00:57 JSTに環境を承認したとの親連絡を受け、新しい条件として接続を最小再確認した。初回・短い再確認とも `trusted Node process exited unexpectedly; kernel reset, rerun your request`。以前のHRESULTエラーとは変化したが、画面取得前に終了し、承認反映状態は観測できない。画面/通常入力/到達/音は引き続き0。候補2との交代も未確認のため入力なし。これ以上再試行しない。


r2：[T4/S10追補と親レビュー訂正](06_T4_S10_Addendum.md)。本文の断定と操作条件を修正した。旧版の実操作0を維持し、既存8周を実プレイ回数へ数えていない。


r3：[資源の選択とRの因果](07_Resource_Retry_Addendum.md)。指定長の机上追跡ではS6の節約差14はCP9で12へ縮み、その後も残る。Fは返却せず、Rは最新の受理済みCPへ残量・橋・除去フックを戻す。実通過の証拠ではない。Humanの01:09 JST再承認後の1回も画面ランタイム起動前に終了。実操作0を維持。


親レビュー後の軽微訂正：選択上限を `max(1,floor(min(14,R)))` と厳密表記。Rは「保存位置へ戻し、速度はゼロにする」。S6再接続の期限はunscaledTimeであり、ポーズで止まるとは仮定しない。実影響は未確認。今回の42ファイル保存に次のUI状態遷移追補は含めない。
