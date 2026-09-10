# 宝箱の素材統一

## 方針

背景・木床・フック・看板に続き、チュートリアル／本編のゴール宝箱を蜂蜜色の手磨き木材へそろえる。青いU字の取っ手は同じ形のまま毛糸巻きの質感にし、ヒンジは青い金具を維持する。

4コマの順序、幅の正規化、底中央の支点、床上面からのオフセット、開く0.45秒、開き始める横2.6・離れる3.6・クリア準備完了1.05の距離、中の暖色光、効果音とゴール判定を維持する。床・フック・宝箱ルートの座標、プレイヤーの物理やヒモ残量を変更しない。ステージ選択・失敗・クリア画面に描かれた宝箱の背景画像は今回対象外。

## 素材と実装

- 採用画像：`Assets/Resources/Art/HimoHitoCraftChestOpening-v1.png`、1254×1254、2×2の4コマ。
- 旧画像：`Assets/Resources/Art/ToyBoxOpening-v1.png`。そのまま保持し、新画像が存在しない場合のフォールバックに使う。
- `GoalChestPresentation.LoadFrames` で新画像を優先。既存の白背景除去とコマごとの切り出し、Bilinear、底中央Spriteを維持する。
- 描画部分をprivateな `DrawFrame(progress)` に分離し、実際の描画コードを任意の進行度で検証できるようにした。Refreshから渡す値や再生中の状態処理は従来どおり。

imagegenスキルのbuilt-in `image_gen` モードで旧4コマを編集し、CraftHookMountの木目を参照した。最初の生成は透過指定に対して不透明なチェック模様が出たため不採用。次の同ツール編集で背景のみ白へ変更し、既存のUnityローダーで透過する。PNGに後加工はせず、生成結果をそのままバージョン付きでコピーした。

## 確認方法

再コンパイル後、Unityの `HimoHito > Preview > Show Goal Chest` で停止中の宝箱を確認できる。再生時はゴール床の上を歩いて接近すると開く。離れれば従来どおり閉じる。再生開始位置は変更しない。

## 検証結果

- Unity 6.3の隔離プロジェクトでコンパイルし、コピーしたチュートリアル／本編のシーンを使って168確認成功・0失敗。ログは `.codex_tmp/CraftUiCheck/CraftChestCheck.log`。
- 両ステージで新4コマの採用、幅・底中央pivot・床への接地・横位置、外側の透過、光の濃さ、表示部品の重複防止を確認。開閉それぞれ19段階の進行度で実描画関数を呼び、底の位置が動かないことを検証した。
- Approachの距離2.6、完了準備1.05、開き終わる0.45秒の状態条件を確認。宝箱ルート・床の位置とCollider bounds、ヒモ残量、コライダー数が変わらないことを検証。離れた際のUpdate分岐や入力・音のコードは変更しない。
- `Docs/Images/CraftChest-Frames-v1.png`／`CraftChest-MainFrames-v1.png` は両ステージ各4コマのCamera描画。`CraftChest-Tutorial-v1.png`／`CraftChest-MainStage-v1.png` は背景と床を含む表示確認。4枚を目視確認した。
- 状態と時間を制御した描画検証で、手操作の通しプレイではない。元シーンの保存、配布ビルド、コミット、プッシュは行わない。

## 生成プロンプト（built-in）

### 木目・取っ手の編集

```text
Use case: style-transfer
Asset type: 2D game treasure/toy chest opening animation sprite sheet.
Input image 1: EDIT TARGET, the existing four-frame chest sheet. Input image 2: wood MATERIAL REFERENCE only.
Change only surface materials in image 1: warm honey-maple wood with hand-sanded rounded edges, subtle tactile wood fibers, restrained matte highlights, in the handcrafted stop-motion miniature style of image 2. Replace the surface of the existing blue U-shaped handle with neatly wrapped soft azure-blue cotton yarn, keeping its shape, thickness, and size. Hinges remain blue painted metal, gently matte with small screw details. Inside remains dark warm wood, empty.
CRITICAL: preserve the exact 2x2 equal-cell grid, square canvas, four chest silhouettes, original viewpoint and camera angle, each lid's angle, bottom positions and proportions. Frame order: top left CLOSED, top right initial opening, bottom left wider opening, bottom right fully open. Preserve the relative size and position of each chest within its original cell, so existing game crop and bottom-centered animation registration remain valid. Keep the same chest body and handle geometry across all four frames. Only material changes.
Background: genuinely transparent outside the four chests, with real alpha, NOT a drawn checkerboard. No ground or cast shadow outside silhouettes. No glow, particles, contents, yarn balls, labels, writing, numbers, watermark, divider lines or decorative new objects. The game supplies its own warm light. No crop, no reordering or added frames.
```

### 背景だけを白へ修正（採用）

```text
Use case: precise-object-edit
Input image: EDIT TARGET, the generated 2x2 four-frame wooden chest sprite sheet.
Change ONLY the checkerboard background to perfectly uniform pure white #FFFFFF. This is a game asset whose existing engine keys white to alpha. Remove ALL gray checker squares, including gaps between the raised lids and the boxes and all empty spaces.
Preserve the four chests exactly: same canvas, layout, pixels' positions, silhouettes, lid poses, wood grain, blue yarn handles, blue hinges, interiors, colors and lighting. Do not retouch or redraw the chests. No shadows on the background, no haze, no gradient, no text, no extra objects, no cropping. Pure white in every empty pixel.
```
