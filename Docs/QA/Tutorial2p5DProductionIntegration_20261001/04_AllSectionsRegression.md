# All Sections Regression

最終Editor回帰：`Evidence/Runtime05/result.txt` / `checks.txt`。standalone起動経路補完後のSourceで再確認。

**127 checks PASS、failures0、runtimeErrors0、物理的Section遷移3回、motion frames35。** Unity Editor検索DBのstartup Error1件はstack別に記録。ゲームのruntime Errorではないが、診断ログから除去していない。

## 操作

| 区間 | 検証内容 | 結果 |
| --- | --- | --- |
| 1 | 左右歩行、通常ジャンプと着地、Hook照準、接続、振り子で進行、離脱、Section2へ実着地、R復帰、看板pause/resume | PASS |
| 2 | 左右歩行、ジャンプ、照準、接続、離脱、Section3へ実着地、checkpoint99/選択6、R復帰、看板 | PASS |
| 3 | 左右歩行、ジャンプ、右緑Hook、E/Q長さ7、橋を歩いてSection4到達、R復帰、看板 | PASS |
| 4 | 左右歩行、ジャンプ、左右E/Q（6+6）、F merge、center消失、Rでpre-merge checkpoint復元、再merge、実ゴール到達、Clearing→Clear | PASS |

橋・Mergeのない第1/2区間に新しい仕様を追加していない。checkpointは通常のルートと着地で更新。Rは全4区間で検証。第4区間Rは進行前checkpointの仕様通りに橋/center/残量を戻す。

## 背景

- 同一状態でT1とProductionを切り替えて撮影。Player位置/速度/回転、Camera位置/orthographic size、Collider/Joint、resource、橋、removed Hook、進行/outcomeの一致を各pairで検証。
- 1→2→3→4の3境界で、各レイヤーのdxがfollow * Camera dxになることを確認。section切替で位相リセットなし。
- 背景root以下にCollider/Rigidbody追加なし。
- legacy背景非表示、disable/Stage Selection復帰でoriginal visibility復元。
- 看板を開いてpause中も背景が維持され、Cameraはその状態から動かない。
- ClearingとClearで背景を維持。Clear→Stage Selectionで旧背景を復元。
- MainStage開始では新コンポーネントなし、既存背景あり。
- Far/Mid/Near層別描画、全区間standing/walk/jump/attach/橋/merge/restored/clearを保存。
- Player/Hook/看板/橋のworld-camera画像を目視。手前玩具が主操作領域を覆わず、床右端の線を修正できている。

## 限界

これは既存入力cache/APIを使う**Editor専用の実物理ルート診断**。OSキーボード入力を模擬したHuman Playtestではない。撮影用clock1/60は診断だけで、Production fixedDeltaTimeは変更しない。GIFは抽出frameの固定間隔再生でFPS測定に使わない。

UI/HUDを含む実Window、移動中の継ぎ目の印象と実入力の最終確認はHuman Reviewに残す。以前のT1 Human PASSを今回のHuman PASSに読み替えない。

Human feedback：イントロgoal→startのCamera中に旧背景が表示される。上記の「introは旧背景へ戻す」期待値では自動QAが問題扱いしていなかった。Humanは新3層での統一を期待するため、全体判定をPI2へ更新。プレイ中の127checks PASSとは区別する。
