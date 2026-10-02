# 評価と最終判定

## 判定

**R1 — 前景統合の改善成功。Human確認「自然になった・見やすい」。**

前回のP1は背景奥行きの判定として維持する。Agent静止画像評価では当初R2（接地感の改善が限定的）だったが、今回専用Buildに対するHuman Reviewで前景の自然さ・視認性の改善が確認されたため、最終R1に更新する。足場と背景床の投影ルールが完全に一致したという判定ではない。Gameplayの変更やProduction採用は行わず、このrefinement Proofで停止する。

## 評価ポイント

| 項目 | 結果 | 根拠 / 残る点 |
|---|---|---|
| 床が自然に見えるか | 一部改善 | Mid床を近い床帯に再利用し、Near小物と横位相を合わせた。大きい移動時の継ぎ目はHuman確認が必要。 |
| 汽車・毛糸玉・小物が浮かないか | 一部改善 | 車輪・クッション・積み木下の局所影、木材と毛糸の反射色を調整。静止比較の差は控えめで、完全な空間一体化とは言えない。 |
| 足場の接地感 | 限定的改善 | 背景床面に投影補助影を追加。ただし足場の本当の物理下端は通常画面外。足場と床の投影ルールの違いは影だけでは解消しない。 |
| 背景奥行き | 維持 | Far/Mid画像・色・Z・追従率を維持。Near=0.88はMid=0.92、Far=0.99と独立している。連番で相対移動を確認。 |
| 主人公 / Hookの見やすさ | 画像評価・Humanで維持 | ピンクの主人公・ヒモ、青Hook、足場上面、照準が中央余白から読める。Humanから全体に対し「見やすい」と回答。 |
| 暗く / ぼかしただけか | いいえ | 局所影・反射色、Near追従、床帯同期を調整。壁全体のTint/blurは変更していない。全景の明るさはほぼ維持。 |
| 第1区間のrefinementか | 成立 | 既存P1の3層を残し、前景/床補助のみ追加。第2区間復帰とMainStage非適用を診断。 |

## 実行Evidence

- 最終診断 `Evidence/Runtime_Attempt02`: 32 PASS / 0 FAIL / 0 Runtime Error。
- 初回 `Evidence/Runtime`: 32 PASS、影が画面外で弱い配置。上書きせず保持した。
- 5状態でBefore/After切替時のPlayer、Camera、Section、選択ヒモ長、Collider、Joint状態が一致。
- 原画像は実Unity `Camera.Render` の1600×900。Beforeは前回P1、Afterは今回調整。HUDのIMGUIは除外。
- 30枚のPNGとGIF、motion.csvを保存。GIFの150ms/枚はプレビュー速度で、FPS評価ではない。
- Editor SearchDatabase startup例外1件を別記録。GameplayのRuntime Errorには該当しない。

## 性能メモ

同じEditor Camera.Render呼出、10回warmup後にモード交互で各15サンプル。中央値: 前回P1 **2.7994 ms**、調整版 **2.8564 ms**。これは呼出のCPU時間であり、GPU処理時間やWindows Buildの平均FPSではない。外れ値もあり、性能回帰なしとは断言しない。

比較Buildの4つのロード済み画像: **64,285,184 bytes / 約61.31 MiB**。元Nearと調整Nearを両方保持している。床補助面は既存Mid textureを再利用し、影textureは128×64 RGBA。3個の補助SpriteRendererを追加したため、低性能PCでの実測は本採用前に必要。

## 本採用前に必要なHuman確認

1. START_REFINEとSTART_PREVIOUS_P1で、移動中の床/玩具の滑りを比較。
2. 汽車・毛糸玉の影が「マット」や独立した台に見えないか。
3. 床帯の上端と木目が大きい左右移動で二重に見えないか。
4. 足場の接地感がR1と呼べる改善になったか。投影の違いが残る場合はR2を維持する。
5. A/D、Space、矢印、Eの通常操作と視認性。第2区間復帰も確認。

Human Review結果は `03_HumanReview.md` に分離する。今回の回答は「自然になった・見やすい」。前回の「奥行きが分かり、見やすい」という承認を今回の承認として流用しない。全体評価のPASSであり、個別操作すべてのHuman回帰PASSと混同しない。

## 保護とSTOP

Productionと前回Proofの原本を変更しない。新規隔離Project/Build/QAだけを追加。元Near/Far/Mid、BGM原本、既存output/Build、MainStageの既存未Commit差分を保持。commit / push未実施。保護結果は `ProtectionResult.json`、変更一覧は `ChangeManifest.json`。

Human Review回答を記録してここで停止。追加Gameplay修正・新しい背景設計・Production本採用・Git保存を行わない。
