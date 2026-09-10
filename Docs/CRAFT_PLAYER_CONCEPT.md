# プレイヤー素材統一：待機姿勢の見本

## 状態

**承認済みのデザイン見本。** この紺背景PNG自体は確認用として保持し、ゲーム用の立ち絵とアニメーションへの展開は [CRAFT_PLAYER_ART.md](CRAFT_PLAYER_ART.md) に記録する。以下は「待機姿勢1枚を先に確認し、その後に全アニメーションへ展開」と合意して制作した時点の記録。

- 保存画像：`Docs/Images/HimoHitoCraftPlayer-Concept-v1.png`（1024×1536）。
- built-in `image_gen` モードを使用。元の立ち絵を編集対象、更新済みスイッチの毛糸とCraftRoomMidの背景素材を質感参照にした。
- 元の大きな丸い頭、黒い目2つ、ピンク色、短い脚、頭頂のヒモ1本、後ろのヒモ端2本を引き継ぎ、太い輪郭線を柔らかい毛糸の縁・立体的な編み目へ変更。
- 背景の紺色は比較用。このPNGは透過素材ではなく、Resourcesに入れない。初回の透過指定では市松模様が焼き込まれたRGB画像となったため、その画像は採用せず、同ツールで背景だけを紺色へ変更した。生成結果をそのままコピーし、外部スクリプトでの画素加工は行っていない。
- 見本は元画像とキャンバス内の占有サイズが異なる。実装する場合は顔・頭幅・首位置・足裏・頭頂の接続点を揃え、縦横を別々に伸ばさないよう確認する。既存の自動目検出が濃い編み目を誤認しないことも未検証。
- 元PNG・meta・スクリプト・シーン・アニメーション・操作・物理・残量計算は今回変更していない。Unity起動・ゲーム内描画確認・ビルドは行わず、見本の目視確認のみ。ユーザー確認後に透過素材と各アニメーションの制作へ進む。

## 参照素材

1. 編集対象：`Assets/Resources/Art/HimoHitoPlayer-v1.png`
2. 毛糸の参照：`Assets/Resources/Art/HimoHitoCraftRecoverySwitch-v1.png`
3. 世界観の参照：`Assets/Resources/Art/HimoHitoCraftRoomMid-v1.png`

`HimoHitoIdle-v1.png` の待機6コマも確認した。新しいデザインを採用する際は既存の表情・動作を維持する。

## 生成プロンプト（built-in）

### 質感変更

```text
Use case: style-transfer
Asset type: ONE standing player-character sprite concept for the 2D yarn platformer HimoHito; a visual-approval preview, not an animation sheet.
Input image 1: EDIT TARGET and strict character identity/pose/proportions reference, the existing pink HimoHito player standing and facing screen-right.
Input image 2: wool MATERIAL reference only, the soft pink knitted button. Do not copy its button shape or wooden base.
Input image 3: supporting ART-DIRECTION reference only, the handmade night-room toys and fabrics. Do not include any of its props.
Primary request: Change only the player's surface material and rendering finish to tactile, soft, matte knitted wool in the same handcrafted miniature/stop-motion world. Keep the original clear, vivid rose-pink hue, adding fine soft fibers within a readable large knit pattern. Gently soften the heavy dark cartoon ink outline into natural deep-pink edge shading, while retaining a crisp readable silhouette at small in-game scale. Soft warm upper-left illumination with sufficient neutral fill so all limbs and the face remain legible; no bloom, halo or dramatic backlight.
CRITICAL identity invariants: preserve image 1's exact standing pose, right-facing three-quarter profile, body proportions, round large head size, narrow torso length, short legs and feet size and placement, BOTH dark oval eyes with their tiny highlights in the SAME locations, exactly ONE twisted upright yarn strand on the crown, and exactly TWO dangling loose yarn ends behind the left side of the lower torso. Preserve their lengths, widths and paths. Preserve the existing unbroken outer silhouette and total occupied width/height ratio; no chubby redesign, no stretching. No mouth, nose, ears, eyebrows, clothes, new limbs or accessories. Do not turn the character to face the viewer.
Composition: the same portrait 2:3 canvas and central placement as image 1, full body and crown strand and loose ends all visible, generous existing empty margins, one neutral standing character only. No panels or variants.
Background: genuinely transparent alpha outside the character, including between legs and yarn ends. Remove the source image's pale checker pattern completely; DO NOT draw any checkerboard, white/gray background or floor. No external shadow, text, logo, watermark, scenery or props. Only the character cutout.
```

### 見本用背景への修正

```text
Use case: precise-object-edit
Input image 1: EDIT TARGET, the standing pink knitted HimoHito character concept.
Change ONLY the background. Remove every gray and white checker square and warped checker artifact. Replace the entire empty background with perfectly uniform solid dark muted navy #242039, a neutral review backdrop matching the game's dark felt room. This is a character approval image, not a transparent deliverable. The navy must also fill the gaps between legs and the two trailing yarn ends.
Keep the entire character exactly as it is: same pink hue, wool fibers, large round head, narrow body, eyes, one upright crown strand, two hanging loose ends, legs, right-facing pose, proportions, locations, occupied size, lighting and framing. Do not redraw, recolor, reshape, stretch, add or remove any character detail. Keep the original 1024x1536 portrait canvas.
No floor, cast shadow, texture or grain on the navy background, no gradients, glow, checkerboard, props, border, writing, labels or watermark. Character remains the only object.
```
