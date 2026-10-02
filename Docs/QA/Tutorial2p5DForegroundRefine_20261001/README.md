# Tutorial 第1区間 — 2.5D Foreground Refinement

2026-10-01。前回 `Tutorial2p5DProof_20261001` のP1を保ったまま、床・汽車・毛糸玉・前景小物の統合だけを調整した隔離Proof。

## 目的と前回との差

部屋・窓・カーテン・壁・棚の良さを残し、「前景を貼り足した感じ」と、床上の小物が横滑りする感じを減らす。新しい部屋やギミックを作る工程ではない。

BeforeはProduction背景ではなく**前回P1のFar/Mid/Near**。Afterは同じGameplay状態に今回のrefinementを適用した実Unity描画。

## 調整したもの

- Near画像: 汽車、毛糸玉、クッション、積み木、糸巻きの局所光・接地影を既存配置のまま編集。元画像は保持。透過1920×1080。imagegenによる編集なので小物の細部はpixel完全一致ではない。
- Near追従率: 0.78→0.88。Midとの差を減らす。Far=0.99、Mid=0.92は維持。Gameplay Cameraに書き込みなし。
- 近い床帯: 既存Midの下17%を再利用してNearと同じX位相で描く。上端をalpha fadeし、彩度と色温度をわずかに調整。木目のぼかしはしていない。
- 足場の投影影: 既存Colliderを読み取り、背景床に薄い補助影を追加。画面外の物理下端からではなく、床画像の見える投影位置を基準にする。実際の支持面・足場形状は変えない。

詳細は [実装メモ](01_Implementation.md)。Far/Mid画像・Tint・Z・追従率は変更していない。調整はTutorial第1区間 / Playing / 見渡し終了後のみ。メニュー・失敗・第2区間以降では既存背景に復帰し、MainStageには導入しない。

## Gameplay / Production保護

ProductionのSource・Scene・Packages・ProjectSettingsは編集していない。Hook、足場位置・サイズ、Collider、Physics、Camera、入力、ヒモ残量、Tutorial進行、看板は変更なし。実行用C#・Shader・PNGは新規隔離コピーにだけ配置する。

開始時main / HEAD / origin/main: `702a4bc044b0eb35bedb552753ca8495599bc1ee`。

Production保護結果は `ProtectionResult.json`。開始時に存在した3,175ファイルについてSHA照合し、既存output・Build・BGM・前回Proof/QA/artを含めて保持を確認する。MainStageの既存未Commit状態はそのまま保護。stage / commit / pushなし。DESERT LOOPにはアクセスしない。

## 実行確認と比較

最終診断: `Evidence/Runtime_Attempt02/result.txt`。32項目PASS、failures=0、runtimeErrors=0。Tutorial開始、左右移動、ジャンプ、Hook照準・接続、既存DistanceJoint、3層の相対移動、Section 2復帰、MainStage非適用を確認。

5状態のBefore/After切替時に、Playerの位置・速度・回転、Camera、Section、選択ヒモ長、Collider・Jointの状態が一致。Editorの入力キャッシュを使ったcontrolled diagnosticであり、OS実キーボード確認やHuman承認ではない。

Unity SearchDatabaseの初期index例外1件は別記録 (`editor_search_errors.txt`)。Gameplay例外と混同せず、Productionの修正はしていない。

- `Screenshots/01_Standing_Before.png` / `01_Standing_After.png`: 初期表示。
- `Screenshots/01_Standing_Comparison.png`: 前回P1と今回の並列比較。
- `02_Right`, `03_Aim`, `04_Jump`, `05_Attached`: 同じく各2画像とComparison。
- `Screenshots/ActualUnityMotion.gif`: 実Unity30フレームのプレビュー。150ms/枚に再生整形しておりFPS測定ではない。
- `Screenshots/Motion_ContactSheet.png` / `Motion_000...029.png`: 移動中の実描画。
- `Evidence/Runtime/`: 初回の影位置を含む診断を上書きせず保持。

撮影は1600×900のWorld Camera.Render。Gameplay Cameraの位置・zoomを撮影用に変えず、HUDのIMGUIは画像に含まれない。Buildでは通常UIが表示される。

## 視認性 / なじみ評価

静止・移動・ジャンプ・接続の画像上、ピンクの主人公、青Hook、足場上面、照準、ヒモは背景から区別できる。中央のGameplay余白を維持し、前景小物を削除していない。

床と小物の横方向の同期、局所影・反射色には改善がある。ただし画面内の変化は控えめ。高いGameplay足場の物理下端は通常画面外で、投影影だけでは「部屋床に実際に立っている」空間関係を完全に解決しない。薄い影やAI画像の影基底を含め、最終的な自然さはHuman確認が必要。

**最終判定R1: Human確認「自然になった・見やすい」。** Agentの当初画像評価R2から、今回専用BuildのHuman全体評価を受けてR1に更新。足場の接地投影の制約は残す。Production本採用とは別であり、個別操作全項目のHuman PASSとは混同しない。詳細は [評価・最終判定](02_Evaluation_FinalDecision.md)。

## 問題点 / 本採用前の確認

1. 長い足場の下端を変えない条件では、物理床と背景床の投影の違いが残る。
2. 床帯は有限画像。第1区間の大きいカメラ移動で上端継ぎ目・二重木目が気にならないか実操作で確認する。
3. NearをMidに寄せたため前景parallaxは少し控えめ。窓・壁の奥行きは保持しているが、感じ方はHuman判断。
4. 今回Buildは比較用に元Nearもロードする。4画像のProfiler texture memoryは64,285,184 bytes（約61.31 MiB）。補助床はMidテクスチャを再利用し、補助影は128×64の小さい生成texture。最終採用時のメモリ削減は別工程。
5. CPU-call timingは `EvidenceManifest.json` に記録。Editor Camera.Renderの局所測定で、GPU timing / standalone FPS / 性能保証ではない。

## Human確認用Build

新規出力: `output/Tutorial2p5DForegroundRefine_20261001_Refine01/Game/`。

- `START_REFINE.bat`: 調整版。
- `START_PREVIOUS_P1.bat`: 前回P1表示 (`--foreground-before`)。

日本語パスの起動問題を避けるため、ランチャーは今回専用の `%PUBLIC%/HimoHito_Tutorial2p5DForegroundRefine_20261001_Refine01` にコピーする。前回Buildや前回Publicコピーは使わない。

操作確認表と今回のHuman回答は [Human Review](03_HumanReview.md)。Review結果を記録して停止し、今回の結果をProductionに本採用する変更やcommit / pushはしない。

## 保存物 / 再現

変更ファイル一覧は `ChangeManifest.json`。素材は `Assets/Tutorial2p5D_NearRefined.png`、生成promptと原出力は `GenerationPrompts.md` / `AssetManifest.json`。Sourceテンプレート・shader・生成Unity .meta・ログはEvidence内に保存。前回P1のassetsと保護ロジックをread-onlyで再利用する。

Unity `6000.3.21f1`、現在のWorking Treeコピー、既存外部BGM、前回P1素材、今回Proofテンプレートで再現する。HEADだけで完全再現する資料ではない。
