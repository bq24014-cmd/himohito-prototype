# ステージ選択の小さなジオラマ（2026-09-10）

## 見える変化

- 練習の島：小さなヒモヒトが既存イラストのヒモ橋に沿って往復する。ゲーム内と同じ透過済みの8枚の歩行画像を使い、足裏を橋へ合わせる。端では少し休む。選択を外すとその場所で休み、再選択すると続きから歩く。
- 本編の島：選ぶと宝箱が約0.8秒で静かに開き、暖かい光が漏れる。選択中は開いたまま、外すと約0.5秒で閉じる。
- 島の台座、カメラ、選択用クリック領域は動かさない。音・残量50/99・物理・ステージの配置・クリア条件・記録は変更しない。
- Tabの説明、開始の布演出などでメニュー操作が無効な間は時計を止める。戻った瞬間に休止時間分を早送りしない。

## 拡張と描画

`StageSelectionDioramaState` はカタログの固定IDごとに時計を管理する。明示的な `mapArt: practice` と `mapArt: toybox` の遊べる島だけが対象で、追加予定・任意Sprite・空のmapArtのフォールバックは静止画のまま。島の追加、3件ごとのページ、CLEARの旗、選択枠の編み上がり、開始時の布演出は従来どおり。

`StageSelectionDioramaArt` は本編の静止した台座と宝箱の5つの開閉姿を別レイヤーで描く。蓋を除いた下側の本体幅・底の位置を基準に各姿を合わせ、蓋が開いて画像の高さが変わっても島や宝箱全体を拡大縮小しない。画像が欠けた場合は既存の島イラストへ戻す。

`StageDioramaPlayerArt` は `RopeBodyVisual.GetDioramaWalkFrame` の読み取り専用アクセサーで既存の歩行Spriteを取得する。実プレイヤーのコンポーネントを生成せず、同じシートの足裏ピボット・共通倍率を利用する。縦横比を保ち、表示高さ29（上限31）で描画する。

## 画像

採用素材：`Assets/Resources/Art/HimoHitoStageDiorama-v1.png`（1536×1024、6パーツ）。
元の `HimoHitoStageMapPieces-v1.png` は削除・上書きせず、練習島、布ボタン、本編のフォールバックに引き続き使用する。

imagegenスキルの built-in `image_gen` で作成。生成要求は透過だったが最初の出力は不透明な模様を含んだため不採用とし、背景だけを均一な白へ変更した2回目を採用した。既存の工作素材と同じ実行時の白抜き処理で使用する。外部API／CLIは未使用。

- 初回出力（制作PC）：`%USERPROFILE%/.codex/generated_images/019fdfa3-5ee1-7911-8a24-63282af5bae4/exec-ce47e67b-1707-48bc-9b7b-4e0f87480d88.png`
- 採用出力（制作PC）：`%USERPROFILE%/.codex/generated_images/019fdfa3-5ee1-7911-8a24-63282af5bae4/exec-323d20f5-1bcf-42a5-a7d7-262755301e57.png`

### 生成プロンプト

```text
Use case: precise-object-edit
Asset type: Unity 2D stage-selection diorama layered sprite atlas, 3 columns by 2 rows, six equal square cells, landscape 1536x1024.
Input image 1: reference and edit source for the MIDDLE rose felt island and its treasure chest. Keep its handcrafted miniature world, warm honey wood, dusty rose felt, muted blue cloth bands, pink braided yarn, soft fibers and fixed three-quarter camera.
Primary request: separate this island into a stationary base and a five-pose opening treasure-chest animation so we can animate only the lid, not the island.
Cell order left-to-right, top-to-bottom:
1) ONLY the rose felt island base, green felt leaves and three ascending wooden steps. Remove the treasure chest entirely. The highest step at back-right is an EMPTY flat wooden landing to support the separate chest. Keep the pink rope along the stairs but end it on the empty landing; no rope floating up into absent chest.
2) ONLY its wooden treasure chest, fully CLOSED.
3) ONLY the SAME chest, lid open by about 20 degrees.
4) ONLY the SAME chest, lid open by about 40 degrees.
5) ONLY the SAME chest, lid open by about 65 degrees.
6) ONLY the SAME chest, lid fully open about 85 degrees, a soft warm golden interior (no rays or sparkles).
Chest cells have NO island, NO plants, NO steps and NO hanging rope. Match the same original curved chest lid, blue bands, wood body, perspective and lighting in ALL five poses. Exact identical chest body, size, center, bottom baseline, camera, wood grain and blue bands across all five cells; ONLY lid angle changes. Chest body bottom at 84 percent of each square cell; body width 64 percent of cell; leave ample space above for the fully open lid. Entire chest visible and uncropped in all poses.
Cell1 island fills about90percent cell width with visible bottom; no extra props. Six isolated sprites with ample clear gutters; genuinely TRANSPARENT background, preserve clean soft silhouette; no checker pattern, no labels, no text, no grid lines, no watermark, no shadows outside the sprites. This is production animation art, not a scene illustration.
```

### 背景修正プロンプト

```text
Use case: precise-object-edit. Image 1 is the edit target sprite atlas. Change ONLY the noisy grey/white background to uniform solid pure white RGB255,255,255. Preserve the six sprites, their exact pixel positions, silhouette, materials, colors, camera and size unchanged. No checkerboard, no texture or speckling in the background, no shadows outside sprites, no labels or borders. Do not redesign, move, crop or resize any sprite. Uniform white is required for our runtime color-key import.
```

## 確認方法

UnityでPlayを停止して再開し、ステージ選択で練習／本編を左右に選び直す。練習の歩行、本編の開閉、追加予定で両方が落ち着くことを確認する。Tabの説明を開いて閉じると、その続きから再開する。配布ビルドやGitHubへのプッシュは行っていない。

描画例：[練習の島](Images/StageSelection-Diorama-Practice-v1.png)、[本編の島](Images/StageSelection-Diorama-Main-v1.png)。実際の表示メソッドへ時刻を渡して撮影した画像で、手動プレイの録画ではない。

## 検証結果

- Unity 6000.3.21f1の隔離プロジェクトでコンパイル成功。状態の19,106項目、実描画243項目／29場面が成功し、失敗0。ログは `.codex_tmp/CraftUiCheck/StageDioramaUi-3.log`。
- 30／60／144fps、端での休止、選択解除・再選択、説明と無効GUI中の停止、再開時の早送り防止、同時刻の複数描画、無効な時刻、カタログの並び替えと対象外項目を検証した。描画領域の画素比較で練習の移動／本編の開閉と、非選択・休止中の静止を確認した。
- 1280×720、960×720（4:3）、1260×540（21:9）、640×360で実レンダラーを撮影。CLEARタグ、文字、足元、宝箱の接地を確認。EditorWindowの26ポイントのタブ領域が縮尺でずれる撮影側の問題は、検証側の入力行列だけで補正した。ゲーム側の配置は変更していない。
- 宝箱の中間画像／本体幅が欠けた場合も、部分的に消すのではなく既存の島全体へ戻ることを検証した。
- 実Playモードの開始・連続シーンロード・布の被覆・紹介カメラの待機と操作復帰・無効な開始先などを再検証し、5,432チェック成功（フレーム単位の確認を含む）。ログは `.codex_tmp/CraftUiCheck/StageDiorama-CurtainFlow-1.log`。
- 本体シーン、移動／残量スクリプトのハッシュは作業前と一致。UIテストで使用した別company/productのクリア記録は復元済み。ユーザーの本番PlayerPrefs、制作中のUnity、配布ビルドには触れていない。
