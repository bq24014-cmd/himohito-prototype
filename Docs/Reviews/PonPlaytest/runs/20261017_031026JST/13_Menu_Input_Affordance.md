# メニュー操作と見た目｜U04を具体化する差分

**A=0、親レビュー済み・保存準備中（remote照合前）。** 10/6固定ソースと既存5画像のみ。ゲーム画面・音・通常入力を新取得していない。「クリック処理あり/なし」はコード経路の区別で、実クリック成功/不具合の認定ではない。

| 画面 | 明示キーとクリック処理の対応 | 選択/見た目と注意 |
|---|---|---|
| タイトル | ←/→・A/Dで島選択、Enter開始、Tab説明、Esc終了。島の絵/名前は選択ボタン、下段3個は開始/説明/終了ボタン | 選択中は毛糸の輪＋「いま選んでいる場所」。名前の明るさは選択かホバーの両方で変わる。明るさだけをfocusとしない。未提供島も選べるが、開始は別途拒否される。 |
| ポーズ/戻る確認 | Esc再開、Tで確認、Tab説明。再開/選択、確認の取消/確定はWoodButton。音量±/スライダー/試聴/初期化にもIMGUI controlあり | 木板は実Button。確認Escはポーズへ戻り、再開ではない。音量操作の明示独自キーは無く、標準IMGUIのキーボード挙動は未確認。試聴の呼出しは実聴に数えない。 |
| ヘルプ | Tab/Escと下部「Tab / Esc 閉じる」のButtonが同じCloseHelpへ。上の操作キー一覧はLabel | 下部は文字風だがクリックreceiverあり。同系色のキー一覧はreceiverなし。ポーズから開いた説明を閉じても、ポーズは残る。 |
| 失敗 | 演出終了後R復帰、Esc選択。描画関数にはクリックreceiverなし | Rは木板のDrawWoodButtonLabel、EscはLabel。ポーズ/クリアの同系統の木板とはイベント実体が違う。押せそうかという印象は未試遊。 |
| Tutorialクリア | Enter/主Buttonで本編、Esc/副Buttonで選択。別のRキー分岐はTutorial再開始 | 表示する2木板は実Button。R再開始はこの2選択には表示されていない。直ちに新UIが必要な欠陥とはせず、隠れた補助キーとして区別。 |
| Mainクリア | R/主Buttonで本編最初、Esc/副Buttonで選択 | Mainの明示キー処理にEnterはない。ただしIMGUI標準focusによる起動を実行していないので「Enterが使えない」と断定しない。 |

**追加できた具体性**：①同じ木板でも、失敗はラベル・ポーズ/クリアはボタン。②逆にヘルプの文字風「閉じる」はボタン。③タイトルの選択印はホバーとは別に存在する。3点ともU04等の根拠を具体化し、指摘ID/優先順位を増やしていない。

**focusの限界**：ClothButton/WoodButtonの独自描画は主にpointer位置による色を使う。対象10ソース内のFocusControl/SetNextControlName/keyboardControl等は0件だが、IMGUI標準focusが存在しない証明にはならない。タイトルのstyle色もnormal/hover/focusedを同値にしている。OS focus、Alt+Tab復帰、カーソル、Tab巡回、Enter/Spaceでどのcontrolが反応するかはC。通常/ポーズのTabには説明を開く明示処理があるため、Tabを単なるfocus移動キーと案内しない。

**最小比較（全て未実施）**：タイトルはA/Dで選んだ島と別の島へpointerだけ移し、選択印/ホバー/Enterの対象を分ける。ヘルプは下部閉じる文字と操作キー行を区別して操作。ポーズはEscと再開木板、Tと選択木板、確認Escと取消木板を対応比較する。失敗のR木板とクリアの実Buttonの反応を同じ入力記録で比較し、キーボードの代替が必要か判断する。クリック対応を採る場合のヒモ用左クリックとの重複は既存08を先に確認する。新規実装/見本は作らない。

**画像の版**：E03/E04/E05は9/30静的fixture、E01/E08は旧UI参照で撮影Buildは画像単体では未確定。10/6の文言/receiverは固定ソースを優先する。E01のMain選択状態は起動時の初期選択の証明ではない。戻る確認とTutorialクリアの現物画像は今回の5枚には含まれない。古い残量・橋0・時間を今夜の成績にせず、静止画の明るさを実ホバー/focusの観測にしない。画像を再複製せず既存通常PNGへリンクする。

[タイトル](https://github.com/bq24014-cmd/himohito-prototype/blob/bdd9f8abd53a8f482bbf7079c03b81c5fec859c0/Docs/Reviews/PonPlaytest/runs/20261017_004500JST/evidence/images/E01_menu.png)／[ヘルプ](https://github.com/bq24014-cmd/himohito-prototype/blob/bdd9f8abd53a8f482bbf7079c03b81c5fec859c0/Docs/Reviews/PonPlaytest/runs/20261017_004500JST/evidence/images/E03_help_fixture.png)／[ポーズ](https://github.com/bq24014-cmd/himohito-prototype/blob/bdd9f8abd53a8f482bbf7079c03b81c5fec859c0/Docs/Reviews/PonPlaytest/runs/20261017_004500JST/evidence/images/E04_pause_fixture.png)／[失敗](https://github.com/bq24014-cmd/himohito-prototype/blob/bdd9f8abd53a8f482bbf7079c03b81c5fec859c0/Docs/Reviews/PonPlaytest/runs/20261017_004500JST/evidence/images/E05_failure_fixture.png)／[Mainクリア](https://github.com/bq24014-cmd/himohito-prototype/blob/bdd9f8abd53a8f482bbf7079c03b81c5fec859c0/Docs/Reviews/PonPlaytest/runs/20261017_004500JST/evidence/images/E08_clear_reference.png)。

[画面別のreceiver/表示/限界](evidence/menu-input-display-matrix.json)／[10入力hashと旧画像の出所](evidence/menu-evidence-index.json)／[短い差分ソース](evidence/menu-source-delta.md)。状態guard/閉じた後の遷移は[既存08](https://github.com/bq24014-cmd/himohito-prototype/blob/bdd9f8abd53a8f482bbf7079c03b81c5fec859c0/Docs/Reviews/PonPlaytest/runs/20261017_014618JST/08_UI_State_Addendum.md)を再利用。本文の「あり」は経路の存在、「未確認」は実際の操作/可読性。上位5案と朝の1件は変えない。
