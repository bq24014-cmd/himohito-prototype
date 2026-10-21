# C17 S5青フックのProduction統合

2026年10月21日、Humanの「良い感じ」という評価と正式採用・統合承認により、レビューcommit `ce25a9cf336305e0654317ee09074a6ec02e3962` の修正を統合した。開始時mainは `0130f68488fcbde83c2f83dc72acd7adeaba4dee`、stageは空、既存未commit差分は5件。

## 変更範囲

`Assets/Scripts/MainStageVisuals.cs`だけに14行追加。S5照明元 `Main S05 Flashlight Source/Craft Wood Body` のRendererへ`forceRenderingOff = true`を設定する。統合ソースは承認済み隔離候補とバイト一致。

余分な床用木片1つの描画だけを抑え、共通リング・固定具、GameObject、Transform、Collider、実接続点、床の分類、照明・光遮蔽・減光は変更していない。診断fixture・比較切替・自動入力・撮影・終了処理・設定変更はProductionに含めていない。既存metaは不変。

## 統合ソースでの確認

ProductionのRuntime Source119本だけをUnity 6.3アセンブリ／RoslynでCompile成功。別途、同じ統合ソースにProduction外の既存QAfixtureを加え、**既存Unity 6000.3.21f1非Development Player＋managed overlay**で一巡確認した。新規Unity Build成功ではない。両Compileとも既存CS0649警告4件、新規Compileエラー0、Player実行エラー0。

| 確認 | 結果 |
|---|---|
| S5の通常表示・接続・スイング・解除・R・再読込 | 採用候補と全画面ピクセル一致 |
| 他区間の青フック | 全フックのSprite・描画順・寸法・固定具・接続点等の記録が一致。S4通常全画面、S1フック周辺もピクセル一致 |
| E接続・スイング・解除 | 既存API＋自動A/Dで成功。青接続点 `(102, 2.95)` を維持 |
| Q生成・橋通行 | 実際の既存生成処理で成功。消費6、残量45→39、右側レールまで歩行 |
| 遮蔽・減光 | 未遮蔽alpha0.62→橋で遮蔽0.08→R後0.62。位置・設定・判定を保持 |
| R | 接地、速度0、入口 `(93.6, -0.035)`、残量45・選択長6・橋0・未接続に復帰 |
| Scene再読込 | 対象木片だけの描画抑制を再適用。固定具とリングは採用候補のまま |
| 同一条件の記録 | 採用候補との471サンプルでRoot位置・速度・入力・anchor・ヒモ長・残量・Camera・遮蔽／減光が完全一致 |

全画面9枚中7枚はピクセル一致。残るS1は「第1区間」表示の有無だけが異なり、Q後は橋付近30ピクセルに最大10/255の差があった。Q後の微小差の詳細原因は未確定。全9枚のフックと固定板周辺、部品設定、物理記録は一致しており、全画面完全一致と拡大解釈していない。

## 保護と未確認

C06/C07・C08・C14・C20・C22/C23を含む対象外252ファイル、既存Scene・README・学習ログ・QAログの5差分、既存Proofの対象ソース／DLLハッシュを保持。Project／Libraryのフル複製、削除、reset／clean／stashは行っていない。C18はHOLDのままで照準補助は未実装。

API・自動入力による確認であり、統合後のOS実キー、現在のdirty SceneのUnity Editor Play、新規Windows Build、全区間通し、音は未確認。既存PlayerのScene／Resourcesを再利用した検証であり、現在のdirty Sceneを再Buildした確認ではない。既存配布Buildは更新していない。

[採用元の比較資料](https://github.com/bq24014-cmd/himohito-prototype/blob/ce25a9cf336305e0654317ee09074a6ec02e3962/Docs/Reviews/C17S5Hook/runs/20261020T234000_JST/Report.md) / [固定入口と統合確認資料](https://github.com/bq24014-cmd/himohito-prototype/blob/codex/review-c17-s5-hook/Docs/Reviews/C17S5Hook/LATEST.json)
