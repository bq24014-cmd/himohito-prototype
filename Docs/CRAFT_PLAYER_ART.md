# プレイヤー素材：Craft 質感への統一

## 目的と適用範囲

承認済みの `Docs/Images/HimoHitoCraftPlayer-Concept-v1.png` を質感基準に、既存のポーズを毛糸の柔らかい編み目へ更新する。立ち絵1枚と12種・74コマのアニメーションを新しいバージョン付きResourcesへ追加。旧画像と.metaはフォールバック用に保持する。

- 対象：立ち姿、歩行、ジャンプ、着地、振り子、歩き出し／停止、待機／まばたき、照準、端でのバランス、足場を編む仕草、ゴール、壁押し、スイッチ押し。
- ユーザー確認済みのキャラクター形・色・目・頭上と背後のヒモを引き継ぐ。シートの順番・枚数・時間と状態遷移は維持する。
- PNGの背景は白。ゲームの既存ローダーが白を透過してスプライト化する。PNG自体がアルファ透過済みという意味ではない。
- `CraftPlayerArt` は新素材を優先し、存在しない／読めない／指定グリッドに合わない場合は旧素材へ戻す。
- 一部の素材だけを欠損させた新旧混在構成では、接続／解除の姿勢を補間する瞬間に見た目の位置や幅が変化する可能性がある。今回検証した配布構成では13素材すべてがそろっており、その混在は発生しない。
- ステージ配置、プレイヤー本体Transform、Rigidbody2D、Collider2D、入力、ジョイント、残量計算は変更しない。本編50／チュートリアル99を維持。
- 本体素材の導入時点では看板内は対象外。その後のユーザー承認で、全4区間の看板とZの説明画面も同じ立ち絵へ統一した。追加変更は [CRAFT_GUIDE_PLAYER_ART.md](CRAFT_GUIDE_PLAYER_ART.md) を参照。

## 検証

最終検証：隔離したUnity 6000.3.21f1でコンパイル成功、**1066項目成功・0失敗**。ログは `.codex_tmp/CraftUiCheck/CraftPlayerCheck-world.log`。

- 立ち絵と全74コマの新素材選択、白透過、セル境界、頭幅／頭頂／足裏／目／首の登録。
- 両ステージの実際のプレイヤー階層で、通常姿勢・35度・78度回転時の世界座標での縦横比と直交性。待機、照準、足場端のバランス、看板を見る姿勢も実際の描画更新から確認。
- 頭頂へのヒモ接続、分割した頭と胴体の首合わせ、コマの順番と既存の再生時間。
- 残量に応じた縮小でもプレイヤー本体のTransform・Rigidbody2D・Collider2Dを変えず、本編50／チュートリアル99を維持。
- MainStageの見た目を再適用しても、旧プレイヤーの矩形SpriteRendererを再表示しない。

実描画の確認で、第1回（861項目）では既存の縦横別倍率による頭のつぶれ、第2回（935項目）ではプレイヤー本体の倍率 `(0.8, 1.2, 1)` による縦伸びを発見した。新素材では基準姿勢の頭幅をそろえて均等拡大し、専用の子 `Craft Player Presentation` で親の縦横差を回転前に補償する。足元の高さを保ち、物理を持つ親は変更しない。旧素材へフォールバックするときは従来の親子関係・倍率を使う。既存のジャンプ／着地の意図した柔らかな伸縮は維持する。

実ローダーと実際の親子階層から描画した確認画像：

- [チュートリアル](Images/CraftPlayer-Tutorial-v1.png) ／ [本編](Images/CraftPlayer-MainStage-v1.png)
- [待機・照準などの姿勢比較](Images/CraftPlayer-NeutralPresentations-v1.png)
- [歩行](Images/CraftPlayer-Walk-v1.png) ／ [ジャンプ](Images/CraftPlayer-Jump-v1.png) ／ [振り子](Images/CraftPlayer-Swing-v1.png)

検証は隔離プロジェクトの実ローダーとSpriteRenderer描画を使用したもの。ユーザー操作による全区間通しプレイではない。実行用ビルド、Gitコミット／push、開いているUnityの操作は行わない。

## Unityでの確認方法

再生中なら一度停止し、素材のインポートとコンパイルが終わってから再生する。チュートリアルまたは本編で、A／Dの歩行、Spaceのジャンプ、Eで接続した振り子を確認する。W／Sで長さを選び、Eで接続またはQで足場を作った後の残量に応じた縮小も確認できる。待機中のまばたき、矢印キーによる顔の向き、Qで編む仕草なども同じ新素材になる。通常の表示順や操作タイミングは変えていない。

## 素材一覧

| 用途 | 新しいResourcesファイル | 生成元 |
| --- | --- | --- |
| Player | `Assets/Resources/Art/HimoHitoCraftPlayer-v1.png` | `exec-eb301f38-1cb5-4375-8878-f061c21f8410.png` |
| Walk | `Assets/Resources/Art/HimoHitoCraftWalk-v1.png` | `exec-26622d48-61b5-4780-9e1f-f6559c99386f.png` |
| Jump | `Assets/Resources/Art/HimoHitoCraftJump-v1.png` | `exec-39acc1e9-5694-43b3-81a5-eb5d25f10794.png` |
| Swing | `Assets/Resources/Art/HimoHitoCraftSwing-v1.png` | `exec-ecf3f47c-d29f-40e0-a425-8efb29017b80.png` |
| Idle | `Assets/Resources/Art/HimoHitoCraftIdle-v1.png` | `exec-f4a234e2-454c-4ee8-818d-cb78f5cf7f7e.png` |
| Aim | `Assets/Resources/Art/HimoHitoCraftAim-v1.png` | `exec-1b52d62f-3a1a-44cb-a6ef-13f83e80fd2e.png` |
| WalkTransitions | `Assets/Resources/Art/HimoHitoCraftWalkTransitions-v1.png` | `exec-8877ced9-d3ee-441a-b69c-f7df14edb3f8.png` |
| Landing | `Assets/Resources/Art/HimoHitoCraftLanding-v1.png` | `exec-e4ee4751-e801-4512-8431-3680408f5554.png` |
| EdgeBalance | `Assets/Resources/Art/HimoHitoCraftEdgeBalance-v1.png` | `exec-9a16daa4-d074-4eac-a2f1-c12787bb7fff.png` |
| Weave | `Assets/Resources/Art/HimoHitoCraftWeave-v1.png` | `exec-0dfa56fe-7c0b-41dc-959b-030e99993a58.png` |
| Goal | `Assets/Resources/Art/HimoHitoCraftGoal-v1.png` | `exec-c8b6f885-4858-455f-9f26-52d86f7a305b.png` |
| WallPush | `Assets/Resources/Art/HimoHitoCraftWallPush-v1.png` | `exec-43eb063e-5e74-42d1-af22-2ac9d13ca30a.png` |
| SwitchPress | `Assets/Resources/Art/HimoHitoCraftSwitchPress-v1.png` | `exec-99f4efc0-bbb9-4dd0-94d1-57b7ee3112a1.png` |

生成元フォルダー（制作PCのユーザーフォルダー基準）：`%USERPROFILE%/.codex/generated_images/019fdfa3-5ee1-7911-8a24-63282af5bae4/`。生成結果は画素加工せずコピーし、Unity .metaを同伴。独立した透過生成やPython画像編集は行っていない。

## 生成プロンプト

imagegenスキルのbuilt-in `image_gen` モード。以下は実際に送信したプロンプトと入力順。

### Player

1. `Docs/Images/HimoHitoCraftPlayer-Concept-v1.png`

```text
Use case: precise-object-edit
Asset type: production standing player sprite for HimoHito.
Input image 1: EDIT TARGET, the approved pink knitted HimoHito on a navy backdrop.
Change ONLY the background to perfectly uniform PURE WHITE #FFFFFF. Remove ALL navy pixels outside the character, including spaces between feet and yarn ends. The game's existing white-key sprite loader will remove white.
Keep the character EXACTLY as it is: same size and position, full 2:3 portrait canvas, head/body/legs proportions, feet, two oval dark eyes and their highlights, one crown yarn strand, two hanging yarn ends, pink color, wool texture, highlights, right-facing standing pose. Do not redraw or add any character detail. No floor or cast shadow, checkerboard, gray halo, text or watermark.
```

### Walk

1. `Assets/Resources/Art/HimoHitoWalk-v3.png`
2. `Docs/Images/HimoHitoCraftPlayer-Concept-v1.png`

```text
Use case: style-transfer
Asset type: production HimoHito 2D player animation sprite sheet, Walk.
Input image 1: EDIT TARGET. Preserve its frame grid, every pose, expression, gaze, head shape and size, slender body, neck join, feet, crown strand, two trailing yarn ends, and each character's exact location/relative scale within its cell.
Input image 2: APPROVED MATERIAL AND CHARACTER STYLE REFERENCE ONLY. Use its vivid rose-pink, thick but soft knitted wool with tiny fibers, tactile matte rounded strands, dark oval eyes and soft warm highlights. Do NOT copy its standing pose or its dark background.
Change only the surface material and shading of the characters in image 1, translating flat dark-ink cartoon lines to the soft handmade knitted finish of image 2. Keep poses and proportions from image 1 strictly. Soft warm upper-left light, subtle neutral fill, no deep black crevices except the eyes. Both eyes stay separate from knitted seams.
Layout: EXACTLY eight frames, four columns by two rows, landscape canvas 3:2. Preserve the alternating leg stride of every frame. Cells equal size, same reading order top row then bottom row, same character count and margins as image 1. No parts may cross cell boundaries.
Background must be PURE UNIFORM WHITE #FFFFFF in every empty pixel, even between feet and yarn ends. The game keys white to transparent; no checkerboard, gray floor shadow, gray haze, gradients, dark background or cast shadow. No drawn cell borders, labels, numbers, props, scenery, platforms, added accessories, mouth/nose/ears, new limbs or frames. Character-only sprites. Preserve the differences in every frame; do not repeat one pose.
```

### Jump

1. `Assets/Resources/Art/HimoHitoJump-v2.png`
2. `Docs/Images/HimoHitoCraftPlayer-Concept-v1.png`

```text
Use case: style-transfer
Asset type: production HimoHito 2D player animation sprite sheet, Jump.
Input image 1: EDIT TARGET. Preserve its frame grid, every pose, expression, gaze, head shape and size, slender body, neck join, feet, crown strand, two trailing yarn ends, and each character's exact location/relative scale within its cell.
Input image 2: APPROVED MATERIAL AND CHARACTER STYLE REFERENCE ONLY. Use its vivid rose-pink, thick but soft knitted wool with tiny fibers, tactile matte rounded strands, dark oval eyes and soft warm highlights. Do NOT copy its standing pose or its dark background.
Change only the surface material and shading of the characters in image 1, translating flat dark-ink cartoon lines to the soft handmade knitted finish of image 2. Keep poses and proportions from image 1 strictly. Soft warm upper-left light, subtle neutral fill, no deep black crevices except the eyes. Both eyes stay separate from knitted seams.
Layout: EXACTLY six frames, three columns by two rows, square canvas. Preserve the distinct crouch, rise, extended body, descent, standing and final crouch poses and vertical offsets. Cells equal size, same reading order top row then bottom row, same character count and margins as image 1. No parts may cross cell boundaries.
Background must be PURE UNIFORM WHITE #FFFFFF in every empty pixel, even between feet and yarn ends. The game keys white to transparent; no checkerboard, gray floor shadow, gray haze, gradients, dark background or cast shadow. No drawn cell borders, labels, numbers, props, scenery, platforms, added accessories, mouth/nose/ears, new limbs or frames. Character-only sprites. Preserve the differences in every frame; do not repeat one pose.
```

### Swing

1. `Assets/Resources/Art/HimoHitoSwing-v2.png`
2. `Docs/Images/HimoHitoCraftPlayer-Concept-v1.png`

```text
Use case: style-transfer
Asset type: production HimoHito 2D player animation sprite sheet, Swing.
Input image 1: EDIT TARGET. Preserve its frame grid, every pose, expression, gaze, head shape and size, slender body, neck join, feet, crown strand, two trailing yarn ends, and each character's exact location/relative scale within its cell.
Input image 2: APPROVED MATERIAL AND CHARACTER STYLE REFERENCE ONLY. Use its vivid rose-pink, thick but soft knitted wool with tiny fibers, tactile matte rounded strands, dark oval eyes and soft warm highlights. Do NOT copy its standing pose or its dark background.
Change only the surface material and shading of the characters in image 1, translating flat dark-ink cartoon lines to the soft handmade knitted finish of image 2. Keep poses and proportions from image 1 strictly. Soft warm upper-left light, subtle neutral fill, no deep black crevices except the eyes. Both eyes stay separate from knitted seams.
Layout: EXACTLY six frames, three columns by two rows, square canvas. Preserve each frame's changing leg bend and trailing yarn direction; DO NOT add a rope or a hook. Cells equal size, same reading order top row then bottom row, same character count and margins as image 1. No parts may cross cell boundaries.
Background must be PURE UNIFORM WHITE #FFFFFF in every empty pixel, even between feet and yarn ends. The game keys white to transparent; no checkerboard, gray floor shadow, gray haze, gradients, dark background or cast shadow. No drawn cell borders, labels, numbers, props, scenery, platforms, added accessories, mouth/nose/ears, new limbs or frames. Character-only sprites. Preserve the differences in every frame; do not repeat one pose.
```

### Idle

1. `Assets/Resources/Art/HimoHitoIdle-v1.png`
2. `Docs/Images/HimoHitoCraftPlayer-Concept-v1.png`

```text
Use case: style-transfer
Asset type: production HimoHito 2D player animation sprite sheet, Idle.
Input image 1: EDIT TARGET. Preserve its frame grid, every pose, expression, gaze, head shape and size, slender body, neck join, feet, crown strand, two trailing yarn ends, and each character's exact location/relative scale within its cell.
Input image 2: APPROVED MATERIAL AND CHARACTER STYLE REFERENCE ONLY. Use its vivid rose-pink, thick but soft knitted wool with tiny fibers, tactile matte rounded strands, dark oval eyes and soft warm highlights. Do NOT copy its standing pose or its dark background.
Change only the surface material and shading of the characters in image 1, translating flat dark-ink cartoon lines to the soft handmade knitted finish of image 2. Keep poses and proportions from image 1 strictly. Soft warm upper-left light, subtle neutral fill, no deep black crevices except the eyes. Both eyes stay separate from knitted seams.
Layout: EXACTLY six frames, three columns by two rows, square canvas. Preserve the breathing offsets and each eye expression, including the half-closed blink in top-right. Cells equal size, same reading order top row then bottom row, same character count and margins as image 1. No parts may cross cell boundaries.
Background must be PURE UNIFORM WHITE #FFFFFF in every empty pixel, even between feet and yarn ends. The game keys white to transparent; no checkerboard, gray floor shadow, gray haze, gradients, dark background or cast shadow. No drawn cell borders, labels, numbers, props, scenery, platforms, added accessories, mouth/nose/ears, new limbs or frames. Character-only sprites. Preserve the differences in every frame; do not repeat one pose.
```

### Aim

1. `Assets/Resources/Art/HimoHitoAim-v1.png`
2. `Docs/Images/HimoHitoCraftPlayer-Concept-v1.png`

```text
Use case: style-transfer
Asset type: production HimoHito 2D player animation sprite sheet, Aim.
Input image 1: EDIT TARGET. Preserve its frame grid, every pose, expression, gaze, head shape and size, slender body, neck join, feet, crown strand, two trailing yarn ends, and each character's exact location/relative scale within its cell.
Input image 2: APPROVED MATERIAL AND CHARACTER STYLE REFERENCE ONLY. Use its vivid rose-pink, thick but soft knitted wool with tiny fibers, tactile matte rounded strands, dark oval eyes and soft warm highlights. Do NOT copy its standing pose or its dark background.
Change only the surface material and shading of the characters in image 1, translating flat dark-ink cartoon lines to the soft handmade knitted finish of image 2. Keep poses and proportions from image 1 strictly. Soft warm upper-left light, subtle neutral fill, no deep black crevices except the eyes. Both eyes stay separate from knitted seams.
Layout: EXACTLY six frames, three columns by two rows, square canvas. Preserve each distinct gaze, head tilt and crown-strand angle. Cells equal size, same reading order top row then bottom row, same character count and margins as image 1. No parts may cross cell boundaries.
Background must be PURE UNIFORM WHITE #FFFFFF in every empty pixel, even between feet and yarn ends. The game keys white to transparent; no checkerboard, gray floor shadow, gray haze, gradients, dark background or cast shadow. No drawn cell borders, labels, numbers, props, scenery, platforms, added accessories, mouth/nose/ears, new limbs or frames. Character-only sprites. Preserve the differences in every frame; do not repeat one pose.
```

### WalkTransitions

1. `Assets/Resources/Art/HimoHitoWalkTransitions-v1.png`
2. `Docs/Images/HimoHitoCraftPlayer-Concept-v1.png`

```text
Use case: style-transfer
Asset type: production HimoHito 2D player animation sprite sheet, WalkTransitions.
Input image 1: EDIT TARGET. Preserve its frame grid, every pose, expression, gaze, head shape and size, slender body, neck join, feet, crown strand, two trailing yarn ends, and each character's exact location/relative scale within its cell.
Input image 2: APPROVED MATERIAL AND CHARACTER STYLE REFERENCE ONLY. Use its vivid rose-pink, thick but soft knitted wool with tiny fibers, tactile matte rounded strands, dark oval eyes and soft warm highlights. Do NOT copy its standing pose or its dark background.
Change only the surface material and shading of the characters in image 1, translating flat dark-ink cartoon lines to the soft handmade knitted finish of image 2. Keep poses and proportions from image 1 strictly. Soft warm upper-left light, subtle neutral fill, no deep black crevices except the eyes. Both eyes stay separate from knitted seams.
Layout: EXACTLY six frames, three columns by two rows, square canvas. Preserve starting, striding, braking and settled poses; keep both trailing yarn ends flowing in their original direction. Cells equal size, same reading order top row then bottom row, same character count and margins as image 1. No parts may cross cell boundaries.
Background must be PURE UNIFORM WHITE #FFFFFF in every empty pixel, even between feet and yarn ends. The game keys white to transparent; no checkerboard, gray floor shadow, gray haze, gradients, dark background or cast shadow. No drawn cell borders, labels, numbers, props, scenery, platforms, added accessories, mouth/nose/ears, new limbs or frames. Character-only sprites. Preserve the differences in every frame; do not repeat one pose.
```

### Landing

1. `Assets/Resources/Art/HimoHitoLanding-v1.png`
2. `Docs/Images/HimoHitoCraftPlayer-Concept-v1.png`

```text
Use case: style-transfer
Asset type: production HimoHito 2D player animation sprite sheet, Landing.
Input image 1: EDIT TARGET. Preserve its frame grid, every pose, expression, gaze, head shape and size, slender body, neck join, feet, crown strand, two trailing yarn ends, and each character's exact location/relative scale within its cell.
Input image 2: APPROVED MATERIAL AND CHARACTER STYLE REFERENCE ONLY. Use its vivid rose-pink, thick but soft knitted wool with tiny fibers, tactile matte rounded strands, dark oval eyes and soft warm highlights. Do NOT copy its standing pose or its dark background.
Change only the surface material and shading of the characters in image 1, translating flat dark-ink cartoon lines to the soft handmade knitted finish of image 2. Keep poses and proportions from image 1 strictly. Soft warm upper-left light, subtle neutral fill, no deep black crevices except the eyes. Both eyes stay separate from knitted seams.
Layout: EXACTLY six frames, three columns by two rows, landscape canvas 3:2. Preserve the extended descent, knee bends, deep crouch, rebound and standing poses with their distinct vertical offsets. Cells equal size, same reading order top row then bottom row, same character count and margins as image 1. No parts may cross cell boundaries.
Background must be PURE UNIFORM WHITE #FFFFFF in every empty pixel, even between feet and yarn ends. The game keys white to transparent; no checkerboard, gray floor shadow, gray haze, gradients, dark background or cast shadow. No drawn cell borders, labels, numbers, props, scenery, platforms, added accessories, mouth/nose/ears, new limbs or frames. Character-only sprites. Preserve the differences in every frame; do not repeat one pose.
```

### EdgeBalance

1. `Assets/Resources/Art/HimoHitoEdgeBalance-v1.png`
2. `Docs/Images/HimoHitoCraftPlayer-Concept-v1.png`

```text
Use case: style-transfer
Asset type: production HimoHito 2D player animation sprite sheet, EdgeBalance.
Input image 1: EDIT TARGET. Preserve its frame grid, every pose, expression, gaze, head shape and size, slender body, neck join, feet, crown strand, two trailing yarn ends, and each character's exact location/relative scale within its cell.
Input image 2: APPROVED MATERIAL AND CHARACTER STYLE REFERENCE ONLY. Use its vivid rose-pink, thick but soft knitted wool with tiny fibers, tactile matte rounded strands, dark oval eyes and soft warm highlights. Do NOT copy its standing pose or its dark background.
Change only the surface material and shading of the characters in image 1, translating flat dark-ink cartoon lines to the soft handmade knitted finish of image 2. Keep poses and proportions from image 1 strictly. Soft warm upper-left light, subtle neutral fill, no deep black crevices except the eyes. Both eyes stay separate from knitted seams.
Layout: EXACTLY six frames, three columns by two rows, square canvas. Preserve subtle lean-back, forward head offset, balancing and recovered standing variations. Cells equal size, same reading order top row then bottom row, same character count and margins as image 1. No parts may cross cell boundaries.
Background must be PURE UNIFORM WHITE #FFFFFF in every empty pixel, even between feet and yarn ends. The game keys white to transparent; no checkerboard, gray floor shadow, gray haze, gradients, dark background or cast shadow. No drawn cell borders, labels, numbers, props, scenery, platforms, added accessories, mouth/nose/ears, additional limbs beyond those in input 1, or extra frames. Character-only sprites. Preserve the differences in every frame; do not repeat one pose.
```

### Weave

1. `Assets/Resources/Art/HimoHitoWeave-v1.png`
2. `Docs/Images/HimoHitoCraftPlayer-Concept-v1.png`

```text
Use case: style-transfer
Asset type: production HimoHito 2D player animation sprite sheet, Weave.
Input image 1: EDIT TARGET. Preserve its frame grid, every pose, expression, gaze, head shape and size, slender body, neck join, feet, crown strand, two trailing yarn ends, and each character's exact location/relative scale within its cell.
Input image 2: APPROVED MATERIAL AND CHARACTER STYLE REFERENCE ONLY. Use its vivid rose-pink, thick but soft knitted wool with tiny fibers, tactile matte rounded strands, dark oval eyes and soft warm highlights. Do NOT copy its standing pose or its dark background.
Change only the surface material and shading of the characters in image 1, translating flat dark-ink cartoon lines to the soft handmade knitted finish of image 2. Keep poses and proportions from image 1 strictly. Soft warm upper-left light, subtle neutral fill, no deep black crevices except the eyes. Both eyes stay separate from knitted seams.
Layout: EXACTLY six frames, three columns by two rows, square canvas. Preserve the crouching / concentration variations and the exact waving curve of each frame's crown strand. Cells equal size, same reading order top row then bottom row, same character count and margins as image 1. No parts may cross cell boundaries.
Background must be PURE UNIFORM WHITE #FFFFFF in every empty pixel, even between feet and yarn ends. The game keys white to transparent; no checkerboard, gray floor shadow, gray haze, gradients, dark background or cast shadow. No drawn cell borders, labels, numbers, props, scenery, platforms, added accessories, mouth/nose/ears, additional limbs beyond those in input 1, or extra frames. Character-only sprites. Preserve the differences in every frame; do not repeat one pose.
```

### Goal

1. `Assets/Resources/Art/HimoHitoGoal-v1.png`
2. `Docs/Images/HimoHitoCraftPlayer-Concept-v1.png`

```text
Use case: style-transfer
Asset type: production HimoHito 2D player animation sprite sheet, Goal.
Input image 1: EDIT TARGET. Preserve its frame grid, every pose, expression, gaze, head shape and size, slender body, neck join, feet, crown strand, two trailing yarn ends, and each character's exact location/relative scale within its cell.
Input image 2: APPROVED MATERIAL AND CHARACTER STYLE REFERENCE ONLY. Use its vivid rose-pink, thick but soft knitted wool with tiny fibers, tactile matte rounded strands, dark oval eyes and soft warm highlights. Do NOT copy its standing pose or its dark background.
Change only the surface material and shading of the characters in image 1, translating flat dark-ink cartoon lines to the soft handmade knitted finish of image 2. Keep poses and proportions from image 1 strictly. Soft warm upper-left light, subtle neutral fill, no deep black crevices except the eyes. Both eyes stay separate from knitted seams.
Layout: EXACTLY six frames, three columns by two rows, square canvas. Preserve the closed-eye head bow in top-middle, closed happy curved eyes in top-right and bottom-left, and open eyes in other frames. Cells equal size, same reading order top row then bottom row, same character count and margins as image 1. No parts may cross cell boundaries.
Background must be PURE UNIFORM WHITE #FFFFFF in every empty pixel, even between feet and yarn ends. The game keys white to transparent; no checkerboard, gray floor shadow, gray haze, gradients, dark background or cast shadow. No drawn cell borders, labels, numbers, props, scenery, platforms, added accessories, mouth/nose/ears, additional limbs beyond those in input 1, or extra frames. Character-only sprites. Preserve the differences in every frame; do not repeat one pose.
```

### WallPush

1. `Assets/Resources/Art/HimoHitoWallPush-v1.png`
2. `Docs/Images/HimoHitoCraftPlayer-Concept-v1.png`

```text
Use case: style-transfer
Asset type: production HimoHito 2D player animation sprite sheet, WallPush.
Input image 1: EDIT TARGET. Preserve its frame grid, every pose, expression, gaze, head shape and size, slender body, neck join, feet, crown strand, two trailing yarn ends, and each character's exact location/relative scale within its cell.
Input image 2: APPROVED MATERIAL AND CHARACTER STYLE REFERENCE ONLY. Use its vivid rose-pink, thick but soft knitted wool with tiny fibers, tactile matte rounded strands, dark oval eyes and soft warm highlights. Do NOT copy its standing pose or its dark background.
Change only the surface material and shading of the characters in image 1, translating flat dark-ink cartoon lines to the soft handmade knitted finish of image 2. Keep poses and proportions from image 1 strictly. Soft warm upper-left light, subtle neutral fill, no deep black crevices except the eyes. Both eyes stay separate from knitted seams.
Layout: EXACTLY six frames, three columns by two rows, landscape canvas 3:2. Preserve BOTH forward pushing yarn arms in the first five poses, matching all original elbows, hands, leaning torsos, extended knees and feet. Last pose is relaxed standing with arms retracted. Cells equal size, same reading order top row then bottom row, same character count and margins as image 1. No parts may cross cell boundaries.
Background must be PURE UNIFORM WHITE #FFFFFF in every empty pixel, even between feet and yarn ends. The game keys white to transparent; no checkerboard, gray floor shadow, gray haze, gradients, dark background or cast shadow. No drawn cell borders, labels, numbers, props, scenery, platforms, added accessories, mouth/nose/ears, additional limbs beyond those in input 1, or extra frames. Character-only sprites. Preserve the differences in every frame; do not repeat one pose.
```

### SwitchPress

1. `Assets/Resources/Art/HimoHitoSwitchPress-v1.png`
2. `Docs/Images/HimoHitoCraftPlayer-Concept-v1.png`

```text
Use case: style-transfer
Asset type: production HimoHito 2D player animation sprite sheet, SwitchPress.
Input image 1: EDIT TARGET. Preserve its frame grid, every pose, expression, gaze, head shape and size, slender body, neck join, feet, crown strand, two trailing yarn ends, and each character's exact location/relative scale within its cell.
Input image 2: APPROVED MATERIAL AND CHARACTER STYLE REFERENCE ONLY. Use its vivid rose-pink, thick but soft knitted wool with tiny fibers, tactile matte rounded strands, dark oval eyes and soft warm highlights. Do NOT copy its standing pose or its dark background.
Change only the surface material and shading of the characters in image 1, translating flat dark-ink cartoon lines to the soft handmade knitted finish of image 2. Keep poses and proportions from image 1 strictly. Soft warm upper-left light, subtle neutral fill, no deep black crevices except the eyes. Both eyes stay separate from knitted seams.
Layout: EXACTLY six frames, three columns by two rows, landscape canvas 3:2. Preserve the long reaching arm, open palm, progressively crouched pressing pose, and recovery shown in each original cell. First and last cells have retracted arms. Cells equal size, same reading order top row then bottom row, same character count and margins as image 1. No parts may cross cell boundaries.
Background must be PURE UNIFORM WHITE #FFFFFF in every empty pixel, even between feet and yarn ends. The game keys white to transparent; no checkerboard, gray floor shadow, gray haze, gradients, dark background or cast shadow. No drawn cell borders, labels, numbers, props, scenery, platforms, added accessories, mouth/nose/ears, additional limbs beyond those in input 1, or extra frames. Character-only sprites. Preserve the differences in every frame; do not repeat one pose.
```
