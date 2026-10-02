# Tutorial全区間 2.5D Background Trial — 2026-10-01

最終判定: **T1 — Tutorial全体への2.5D展開成功、採用価値が高い**。Human Review「自然で見やすい。全体展開してよい」を2026-10-01に受領。Productionへは適用せず、今回のtrialで停止する。P1/R1とは別に全区間版の承認を記録した。後半の端延長に改善余地は残る。

## 目的と入力

成功済み第1区間のFar/Mid/NearとR1前景なじませを、同じ子供部屋としてTutorial全4区間に展開する。背景以外は変更しない。MainStageには展開しない。

開始Git: main、HEAD / origin/main `702a4bc044b0eb35bedb552753ca8495599bc1ee`。既存Working Treeを保護して新規隔離コピーへ展開するtrialであり、clean HEADのみからの再現を主張しない。

確認済みの元資料:

- `Docs/QA/Tutorial2p5DProof_20261001/`: P1、Human「奥行きが分かり、見やすい」。
- `Docs/QA/Tutorial2p5DForegroundRefine_20261001/`: R1、Human「自然になった・見やすい」。
- `Docs/VisualConcepts/Tutorial2p5D_20261001/`: 元の3層素材。

## 第1区間proofからの展開

PNGとshaderは前回の承認済み実体を無変更で再利用。Far/Mid/NearRefinedはZ=7/5/3、追従率0.99/0.92/0.88。床帯・玩具の局所影と反射色を引き継ぎ、支持影を5つの床へ展開。

背景はSectionごとの交換ではなく、固定原点X=-4からCameraXの連続関数で配置。後半で視差がclampされないようにFar/Mid/床帯の左右にmirror guardを加える。Nearの玩具は繰り返さない。

詳細は [01_Implementation.md](01_Implementation.md)。試作コードは `Evidence/TutorialAllBackground.cs`。ProductionのSource/Sceneへは追加していない。

## 全区間の構図と小さな差分

Farは月・窓外の夜空、Midは布壁・カーテン・ガーランド・棚・本、Nearは汽車・毛糸玉・積み木・糸巻き。

| 区間 | 連続移動で変わる構図 |
|---|---|
| 1 | 前回R1の窓、カーテン、汽車、毛糸玉、棚を維持 |
| 2 | 同じ部屋のまま、左の玩具が少し抜け、右の糸巻きが近づく |
| 3 | 毛糸・糸巻き・積み木の比重が増え、橋の背後は暗い布壁 |
| 4 | 棚・工作玩具側に寄る。統合橋と梁の領域を邪魔しない |

区間ごとの色変更や別画像への突然の切替はしない。絵の差分より、同じ部屋の連続した視差を優先した。

## 検証結果

Runtime02: **121 checks / 0 FAIL / 0 runtime errors**。実際の力・Collider・着地によるSection更新で4区間からGoal/Clearまで進行。体の強制移動で攻略を省略していない。

- 各区間の左右移動・ジャンプ・看板Open/Close。
- 第1/2区間の照準と振り子、E接続/解除。
- 第3区間の長さ7・E/Q・実橋歩行、第4区間の2本生成・F統合。
- Rで資源・橋・中心Hookを復帰、再統合からGoal/Clear、Stage Selection復帰。
- 実Section境界3回で背景変位=Follow×Camera変位を確認。
- Before/After切替でbody/velocity/joint/collider/resource/bridge/Camera状態が変わらないことを確認。
- MainStageにはtrialコンポーネントが存在しない。

これは既存入力キャッシュとAPIを使うcontrolled diagnostic。OS実キーボード、Human通しプレイ、看板/HUD文字の可読性確認の代わりではない。診断撮影のみ1/60秒の記録時計を使用し、Production fixedDeltaTimeは変更しない。

Runtime01は撮影処理負荷による短距離歩行のovershootで停止。検証側の記録時計と目標付近の入力調整で解決し、失敗ログも残した。Unity Editor Search起動例外1件はRuntimeエラーとは分離して記録。ゲーム本体を修正していない。

## Before / Afterと画像

`Screenshots/AllSections_Overview.png`: 各区間の実Unity表示。

`S1..S4_Standing_Comparison.png`: 左Before、右After。同じ物理状態で比較。Beforeは第1区間だけ前回R1、他区間は既存背景。Afterは全区間trial。

Standing/Walk/Jump/Attached/Bridge/Merged/Restored/Clear等のPNG、31枚の実ルートmotion frames、4本のGIFを保存。77枚の元captureは `Evidence/Runtime02/`。比較図は縮小・ラベルのみで、ゲーム描画を描き直していない。

Camera.Renderは既存Cameraの位置/zoomを維持。世界描画1600×900でIMGUI/HUDは含まない。GIFは640×360のsampleを800×450表示、固定180ms/frameで再生する説明用資料であり、実時間video/FPS計測ではない。

## 視認性 / 奥行き / 前景評価

全4区間の静止画では、ピンクの主人公と完成したBridge、青/緑Hook、木の足場を暗い布壁から識別できる。窓・壁・玩具の相対移動があり、Section境界で位相がリセットされない。未完成Bridgeのフェード中は暗く見えるが、既存演出そのままで比較ペアの状態は一致。

玩具の局所影と同じ位相の床帯により、汽車/毛糸玉の接地が前回R1同様になじむ。足場の下端は画面外なので、投影影で足場全体の物理的接地が解決したとは判定しない。

**残課題:** 第4区間ゴール右端にはmirror guardの縦の境界と反転棚の断片が見える。部屋の空白は出ないが、正式な横長panoramaではない。このためHuman確認前は暫定T2とした。後半の境界を明示したHuman確認で「自然で見やすい。全体展開してよい」を受領したため、全体展開の採用価値として最終T1にした。継ぎ目が技術的に消えたとは主張しない。

## パフォーマンス

最終Clear viewでwarm後15sampleずつのEditor Camera.Render CPU呼出時間中央値: Before **1.6387ms**、All **1.7573ms**。参考差+0.1186ms。GPU時間・standalone FPS・低性能PC性能を示す数字ではない。

3つの使用テクスチャのRuntimeMemorySize合計は47,695,488byte（約45.5MiB）。guardは同じテクスチャを共有、shadowは128×64。透明層のoverdrawとdrawcallは増えるため、本採用前にProfiler/standalone測定が必要。

## Build / Human Review / 本採用前

Windows Build: `output/Tutorial2p5DAllSections_20261001_Trial01/Game/`。Build Succeeded、429,267,980byte、49.35秒。exe SHAは `ChangeManifest.json`。新規出力で旧Buildを上書きしていない。

起動ランチャー3本を同出力の親に配置。ASCIIパスの専用起動コピー: `C:/Users/Public/HimoHito_Tutorial2p5DAllSections_20261001_Trial01/`。

Human手順は [02_HumanChecklist.md](02_HumanChecklist.md)、回答は [04_HumanReview.md](04_HumanReview.md)。全体の自然さ/視認性について承認済み。個別の操作チェックはHumanから項目別回答を得ていないので自動PASSを付けない。16:9以外、低性能PC、Profiler、本採用時のUI付き実操作回帰は別途必要。今回はProductionへ導入せずHuman Reviewで停止。commit/push/stageは実施しない。

## 保護 / 変更一覧

`Preparation.json`、`ChangeManifest.json`、`ProtectionResult.json`で保護と追加物を記録。変更は今回新設のQA、隔離trial、専用Build/起動コピーのみ。既存3495ファイル、Production Source/Scene、BGM、旧Build、既存QA/Art、未Commit変更を開始時SHAで比較する。DESERT LOOPにはアクセスしない。

終了時保護照合は **PASS**: 3495既存ファイルのSHA不一致0、Productionへの追加ファイル0、index空。mainのHEAD / origin/mainは開始値のまま。MainStageの既存未Commit Sceneも開始SHAのまま保持し、Tutorial Scene/BGM/旧output/既存QA/README/LEARNING_LOGも変更していない。
