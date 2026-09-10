# 布と毛糸の子供部屋・ステージ背景

## 採用素材と表示

- 遠景：`Assets/Resources/Art/HimoHitoCraftRoomFar-v1.png`（1672 × 941、壁・窓・床の不透明背景）
- 中景：`Assets/Resources/Art/HimoHitoCraftRoomMid-v1.png`（1672 × 941、旗・棚・玩具の別レイヤー）
- 表示：`Assets/Scripts/HimoHitoCraftRoomBackground.cs`
- チュートリアルと本編の `Apply` から共通設定を適用。編集画面では `HimoHito > Preview > Refresh Scene Visuals` でも更新できる。

imagegenスキルの built-in `image_gen` で新規作成し、中景のみ同ツールで背景修正。初回の透過指定は実際にはRGBのチェック模様だったため不採用。中景の採用PNGは白背景で保存し、Unity読込時に明るい無彩色（全RGB >= 228、差 <= 15）だけを透明にする。チェック模様は採用素材に含まれない。画像全体の配置と縦横比は保持し、物体の外接矩形では切り抜かない。

白背景のアンチエイリアスが白い輪郭として残らないよう、読込時に外縁2pxだけ近傍の実色から白の混合を取り除き、自然な半透明へ補正する。透明側2pxにもRGBだけを延ばして拡大縮小時の白縁を防ぐ。3px以上内側の刺繍・クリームなどの色は維持する。この補正はUnityの生成Spriteのみで行い、PNGの元データは変更しない。

既存の部屋・遠景・メニュー画像は削除・上書きしない。新背景の画像を読み込めた場合のみ旧ステージ背景を非表示にする。画像欠損時は残してある旧背景に戻す。ステージ選択、操作説明、失敗・クリア画面の絵は変更しない。生成Spriteは共有し、再import時に交換・最後の使用終了やEditorのコード再読込時に解放する。

## 奥行きと変更しないもの

遠景はカメラ移動の98.5%、中景は93%に追従。画面上では遠景が1.5%、中景が7%だけ逆方向へ動き、奥ほど動きが小さい。各レイヤー3枚を左右反転して並べ、カメラ位置から絶対座標を計算して再利用する。大きなスクロール・紹介カメラ・再挑戦でも、移動差分の積み重ねによるずれや有限背景の端を出さない。

描画順は遠景−120、中景−100、既存のほこり−90、ゲームの床・主人公の順。縦位置はカメラと揃え、画面比率と表示高さに応じて均等拡大する。元のPNGの位置に依存する旗／星のUV揺れは、新アートには適用しない。

変更は背景の素材と描画位置のみ。カメラの位置制御・揺れ・ズーム、ステージ配置、当たり判定、操作、ヒモの残量計算、BGM・効果音はそのまま。

## 生成プロンプト（built-inモード）

検証：Unity 6.3のEditor用／Player用コードの参照コンパイル成功。隔離Unityのコピーシーンで71確認・0失敗（`Temp/CraftRoomBackgroundCheck-final.log`）、両シーン合計504通りのカメラ位置／高さ／ズーム／画面比率で背景被覆・カメラ不変を確認。白の除去・内側の色保持・描画順・6タイルの再利用も確認した。

実景プレビュー（HUDなしのCamera描画）：[チュートリアル](Images/CraftRoom-Tutorial-v1.png) ／ [本編](Images/CraftRoom-MainStage-v1.png)。手操作の通しプレイは未実施。

### 遠景・新規生成

```text
Use case: stylized-concept
Asset type: final opaque FAR BACKDROP for HimoHito, a 2D side-scrolling yarn-character game; wide 16:9 landscape.
Primary request: a lovingly handmade miniature child's bedroom at night, sharing the tactile felt, stitched fabric, pink wool and honey-colored wooden toy aesthetic of a premium stop-motion craft game. This is a straight-on room backdrop, NOT a top-down tabletop or menu.
Scene: a deep indigo and dusty plum felt wall with visible fine wool fibers, large subtly different stitched cloth panels, a few understated embroidered star motifs. A small softly moonlit wooden window left of center with gathered fabric curtains; very subdued distant wall details. Warm amber lamp glow fades from the left, cool blue moonlight from above. Horizontal dark wooden skirting and a little wooden floor near the bottom. Make the room feel spacious and cozy, richly tactile, quietly magical.
Composition: panoramic camera straight onto the wall. Mostly uninterrupted calm wall from x32% to x78% and across the gameplay center; window toward x18%, top half. No near shelves or loose toys: those will be drawn as a separate parallax layer. Full-bleed opaque image. Gentle near-uniform color at extreme left/right edges for mirrored repeats, no dominant object cut at either side. Detailed material craftsmanship but softly lit, subdued contrast, darker than the bright pink player and orange playable wooden platforms that will overlay this backdrop. Dark wooden skirting begins at 79% image height.
Constraints: no characters, no UI, no text, no logos, no watermark, no floating gameplay islands, no hooks, no rope bridges, no goal chest, no painted platforms. No overbright center or harsh glow. Entire canvas is the actual room, no frame or blank margin.
```

### 中景・新規生成（背景のみ次の編集で修正）

```text
Use case: stylized-concept
Asset type: final MIDGROUND PARALLAX OVERLAY for a nighttime child's bedroom in a 2D side-scrolling yarn game, wide landscape 16:9.
Primary request: one carefully composed scenery layer on GENUINELY TRANSPARENT background with real alpha, made of tactile handcrafted wooden shelves, cloth-bound books, soft wool balls and a few fabric pennants. Premium stop-motion miniature craft material quality, fine felt fuzz, visible embroidery, warm worn wood. Straight-on side-scroller viewing angle, modest depth on objects, not top-down.
Composition: preserve a full 16:9 landscape canvas. The middle 45% is mostly transparent empty space for gameplay. At upper right, around x72% y32%, a wooden wall shelf with two or three cloth-bound books, a teal yarn ball and a small wooden toy house, all fully within frame. At lower right around x79% y77%, a modest arrangement of wooden toy blocks and a coral yarn spool. At lower left x18% y79%, a small folded fabric cushion and wooden toy train with a wool ball. A gently drooping cord of five muted fabric pennants crosses the upper middle, around y13%. Every item part of the same single room arrangement with generous empty space; not a contact sheet, not icons in cells. Keep outermost 8% left/right canvas clear.
Lighting: soft warm amber from left and quiet cool moonlight from above, subdued color/brightness to remain behind bright pink character and orange playable platforms. Dark teal, dusty mauve, indigo, muted honey wood, restrained coral.
Constraints: real transparent background and transparent gaps between objects, no wall, no floor, no large opaque background shape, no gray checkerboard painted into pixels, no cast shadow on a solid backdrop. No characters, no text, no UI, no hooks, no gameplay rope bridges, no chest, no floating playable islands. Do not crop the objects or trim empty canvas.
```

### 中景・採用版の背景編集

```text
Use case: background-extraction
Edit target: the attached wide landscape craft-room scenery overlay. Change ONLY the technical background.
Replace every checkerboard square and gray/white background mark with exactly flat pure white RGB(255,255,255). No gradient, no background shadows, no residual checkerboard anywhere, including between pennants and around shelf brackets, toys and yarn. The game will key pure white to transparency.
Keep all existing object pixels, object colors, shapes, placements, full canvas dimensions and empty composition as faithfully as possible: five fabric pennants at top, shelf with books/yarn/toy house at upper right, cushions/train at bottom left, spool/blocks at bottom right. Do not move, crop, enlarge, relight or redraw the objects. Do not add objects or text. Clean precise silhouette edges. Output same 16:9 layout on solid pure white.
```
