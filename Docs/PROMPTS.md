# 生成画像プロンプト記録

## 2026-09-08 足場の端でバランスを取る仕草

- 手段：組み込みimagegen（CLI/API不使用）。参照：`Assets/Resources/Art/HimoHitoAim-v1.png`。
- 採用：`Assets/Resources/Art/HimoHitoEdgeBalance-v1.png`（1254×1254、3列×2行）。中立／踏ん張り始め／重心を戻す／収束／毛糸の戻り／落ち着いた姿勢。
- 元PNGは明るいチェック背景を含むため、既存Unityローダーで表示時に透過する。実行時に首の位置でアトラスを分け、胴体・脚を使用。頭は採用済みの照準素材から表示し、上下左右の視線を維持する。PNGそのものは加工しない。
- 生成元：`exec-3e24fc52-58c0-4dec-9b79-84179f14cdde.png`。以下が送信した最終プロンプト。

```text
Use case: stylized-concept.
Asset type: Unity 2D six-frame sprite sheet for a quiet ledge-balance reaction.
Input image 1: exact character identity, pink knit texture, palette, proportions and 3x2 sprite grid reference, not an edit target.
Primary request: Draw SIX full-body poses of the SAME armless pink yarn character standing at an IMPLIED ledge to its RIGHT (do NOT draw the ledge). Exactly 3 equal columns and 2 equal rows, reading left-to-right top row then bottom row. In every frame face RIGHT horizontally; head stays round, same size and neutral, because the game will overlay a separately aimed head. Focus animation on small knee bends, torso weight shifts and the two loose trailing yarn strands.
Frame 1 neutral standing, both short feet planted.
Frame 2 notices the edge: tiny knee bend, torso moves slightly BACK toward the LEFT while both soles stay planted.
Frame 3 strongest but restrained balance pose: rear/left leg supports weight, front/right knee bends subtly, torso leans back about 5 degrees, two trailing yarn strands lag gently to the right.
Frame 4 settles: rear leg straightens a little, torso halfway back to upright, trailing strands swing gently back left.
Frame 5 almost neutral, slight opposing yarn overshoot.
Frame 6 stable cautious upright standing, identical head size, torso length and BOTH sole positions to frame 1. No visible alarm, no big wobble, no jumping.
All six characters must have identical overall scale, head diameter and body proportions; no stretching or squashing. Allow only a small drawn posture change. Reference identity: large round pink knitted head, small oval knitted torso, TWO black bead eyes on the right side, one short yarn tuft on top, two short yarn legs/feet, two trailing yarn ends behind at the lower LEFT. NO ARMS, HANDS, MOUTH, NOSE, EYEBROWS, CLOTHES, PROPS.
Keep both feet on the same relative baseline and the same planted horizontal positions in all six cells. Crisp pink braided-stitch illustration with dark plum outline and warm soft highlights, matching the reference exactly. Entire figures and tails safely inside each cell, at least 28px empty padding. Square canvas, equal cell spacing. Genuinely transparent alpha background, not a painted checkerboard. No scenery, floor, shadows, ledge, UI, text, numbers, guides, cell borders, arrows, glow or extra objects.
```

## 2026-09-08 地上の照準に合わせた視線

- 手段：組み込みimagegen（CLI/API不使用）。参照：`Assets/Resources/Art/HimoHitoIdle-v1.png`。
- 採用：`Assets/Resources/Art/HimoHitoAim-v1.png`（1254×1254、3列×2行）。下／やや下／正面／やや上／上／中立の6コマ。目と頭上の毛糸で方向を表し、足と胴体は立ち姿を維持。
- 元PNGは明るいチェック背景を含むため、既存Unityローダーの背景キーで表示時に透過する。元PNGのアルファが透過済みとは扱わない。既存の頭幅正規化と足裏登録を再利用。
- 生成元：`exec-2efaab16-0e91-4f04-a1be-845aef2070ec.png`。以下が送信した最終プロンプト。

```text
Use case: stylized-concept.
Asset type: production 2D Unity character aiming-pose sprite sheet, exactly six full-body drawings, 3 equal columns by 2 equal rows.
Input image 1: identity, texture, drawing style, proportions, feet placement and grid reference only (existing HimoHito idle animation sheet). Create a new sibling aiming sheet, not an idle blink loop.
Primary request: the SAME little pink knitted yarn character standing in place, looking along its rope aiming direction. Keep the upright torso and BOTH feet planted in exactly the same pose and scale across all six cells. Only redraw the eyes, very slight head orientation and tiny top yarn tuft to communicate the gaze. Do not rotate or stretch the entire character.
Reading order, left to right top row then bottom row: 1 looking strongly DOWN-right at about 75 degrees; 2 looking a little DOWN-right at about 35 degrees; 3 looking horizontally RIGHT, neutral; 4 looking UP-right at about 40 degrees; 5 looking strongly UP-right at about 80 degrees, toward an overhead hook; 6 neutral horizontal RIGHT, matching frame 3 and the reference's last drawing as closely as possible.
Identity invariants: large round pink knitted head, small short oval knitted torso, two black bead eyes on the right side of the head, two short pink yarn legs and feet, one small top yarn tuft and two loose trailing yarn ends to the LEFT behind the body. Absolutely NO arms, NO hands, NO mouth, NO eyebrows, NO clothing, NO props. Preserve the pink palette, dark plum contour and detailed pink knit stitches of the reference. Keep head diameter, body proportions and leg lengths identical in every cell; do not squash the head even when looking up/down. Head remains mostly circular, the displaced eyes and slightly redirected tuft carry the expression.
Composition: square sprite sheet, 3x2 perfectly equal cells, one centered complete character per cell. Keep all character pixels including the top tuft and trailing ends at least 28 pixels inside each cell. Equal foot baseline relative to each cell, no contact shadow, no floor, no grid lines, no labels, no text, no arrows, no watermark. Genuinely transparent PNG background with alpha, not a painted checkerboard. Match reference crisp hand-drawn rendering; no lighting changes or new highlights between frames.
```

## 2026-09-08 ゴール時の喜び

- 手段：組み込みimagegen（CLI/API不使用）。参照：`Assets/Resources/Art/HimoHitoIdle-v1.png`。
- 採用：`Assets/Resources/Art/HimoHitoGoal-v1.png`（1254×1254、3列×2行）。膝を少し緩め、宝箱を見上げて微笑む目と毛糸の揺れを描く。
- 元PNGは明るいチェック背景を含む。既存Unity背景キーで表示時に透過し、頭幅と足裏を登録する。元PNGのアルファが透過済みとは扱わない。
- 生成元：`exec-6097b886-b375-443b-afa1-ff010399ab47.png`。以下が送信した最終プロンプト。

```text
Use case: stylized-concept
Asset type: Unity 2D game character sprite sheet, six-frame quiet goal celebration.
Input image: the approved idle sheet is the exact character identity, palette, knit texture, proportions and camera reference. Generate a new sibling sheet; do not change the reference.
Primary request: EXACTLY six isolated full-body drawings in an evenly spaced 3-column by 2-row grid on a square canvas, reading top row left to right then bottom row. A small armless pink yarn character has safely reached its toy chest and quietly looks up with relief and delight. Restrained, tender acting suitable for a 0.45-second finish animation.
Frame 1: neutral standing, black eyes open, facing right.
Frame 2: slight knee dip and soft blink, yarn tuft leans slightly forward.
Frame 3: knees straighten back to normal, head looks very slightly up-right, eyes begin a gentle happy closed arc; short tuft springs upright.
Frame 4: relaxed upright delighted pose, both eyes closed in soft upward happy arcs, tuft overshoots slightly backward.
Frame 5: happy eyes start opening toward up-right, tuft softly settles.
Frame 6: stable upright relaxed finish, open bright eyes looking slightly up-right, tuft near vertical.
Keep both feet touching the SAME sole baseline and the same horizontal planted positions in each local cell. Do not hop or float. Keep the round head diameter, torso size, camera, outline weight and scale identical across all six frames; tiny head lift through drawn posture only, NO squashing/stretching. Reference identity: round large pink knitted head, short oval knitted torso, TWO black eyes on the right, two short legs, small top yarn tuft, two loose yarn ends behind the left leg, dark plum outline. NO ARMS, HANDS, MOUTH, NOSE, EYEBROWS, CLOTHES OR ACCESSORIES.
Match reference crisp pink braided stitch illustration, soft warm highlights and dark outlines exactly. No lights/glows/flashes, stars, confetti, text, captions, numbers, borders, grid lines, floor, cast shadows, toy chest or other props.
Composition: six equal cells, each figure fully contained with safe margins on every side. Genuinely transparent alpha background; no checkerboard painted into the image. This is a production animation sheet, not six differently designed characters.
```

## 2026-09-08 Qで橋を編む仕草

- 手段：組み込みimagegen（CLI/API不使用）。既存プレイヤーと採用済み待機6コマを参照し、専用6コマを追加。
- 採用：`Assets/Resources/Art/HimoHitoWeave-v1.png`（1254×1254、3列×2行）。小さく膝を曲げ、頭頂の毛糸が前へしなり、戻る。両足を登録し、頭幅を一定にして表示。
- 元PNGは明るいチェック背景を含むため、既存のUnity背景キーで表示時に透過する。元画像のアルファが透過済みとは扱わない。
- 生成元：`exec-1dcf7f37-d685-4644-831c-cbad599d5ba7.png`。以下が送信した最終プロンプト。

```text
Use case: stylized-concept
Asset type: Unity 2D character sprite sheet, six-frame one-shot rope-weaving gesture.
Input images: Image 1 is the established player identity reference; Image 2 is the approved idle animation style and proportions reference. Generate a new sibling sheet, do not modify those references.
Primary request: EXACTLY six full-body poses in an evenly spaced 3-column by 2-row grid on a square canvas. This armless pink knitted yarn character lightly braces its knees while feeding yarn from the small tuft on top of its head to create a bridge, then relaxes. Read frames left to right, top row then bottom row.
Pose 1: upright neutral, both feet planted. Pose 2: small knee bend, head lowers a tiny amount without changing its round size, tuft bends gently forward. Pose 3: deepest but subtle knee bend, chest leans forward a little, tuft curves right in a small S. Pose 4: starts rising, tuft softly recoils left. Pose 5: nearly upright, tuft settling vertical. Pose 6: exactly neutral standing like pose 1.
Subject invariants: identical round pink yarn head, short oval torso, two black eyes facing screen-right, dark plum outline, two short legs, two loose yarn ends behind the feet. NO ARMS OR HANDS. Keep head size and torso length identical in all six frames. Draw the knee bend, do NOT squash/stretch the entire character. No new accessories or tools.
Composition: identical camera, rendering and character scale in every cell; same sole baseline and horizontal planted foot positions in each local cell. Six isolated figures fully inside their cells with at least 10% clear margin; no overlapping cells.
Style: match the crisp pink braided/knitted illustration and soft warm highlights in both references. Motions are restrained, readable in a small in-game sprite. Only a short tuft, not a full bridge or a long loose rope.
Background: genuinely transparent alpha, no checkerboard pattern painted into the image, no floor or cast shadow. No labels, text, numbers, panels, grid lines, scenery, props, sparks, glow, watermark.
```

## 2026-09-07 待機中の小さな仕草

- 手段：組み込みimagegen（CLI/API不使用）。参照：`Assets/Resources/Art/HimoHitoPlayer-v1.png`。
- 採用：`Assets/Resources/Art/HimoHitoIdle-v1.png`（1254×1254、3列×2行）。小さな体重移動・まばたき・毛糸の揺れ、最後のコマが中立。
- 生成PNGは明るいチェック背景を含む。既存のUnity背景キー処理で表示時に透過し、頭幅と足裏を登録する。元PNGそのものが透過済みとは扱わない。

```text
Use case: stylized-concept. Asset type: production 2D game idle animation sprite sheet. Input image is the exact character identity/style reference, not a background. Create SIX closely matching hand-drawn animation poses, arranged in a strict 3 columns by 2 rows grid, equal cells, square full canvas. Pink knitted yarn doll from reference, same round large head, tiny oval torso, black eyes on right side, short legs, no arms, top yarn tuft, two loose yarn ends behind left leg, dark plum outlines and pink woven stitches. Every pose faces right in exactly the same camera and proportions. True transparent background, no ground/shadow, no grid lines, no captions, no other objects. Keep full character inside each cell with safe margins, same head width, same feet contact line. BOTH FEET remain planted at exactly the same place and orientation in all six poses. Only subtle idle acting, never walking: frame1 near neutral with tuft just beginning to lean slightly right; frame2 tiny relaxed knee/torso weight shift and tuft bends gently right; frame3 same slight shift with eyes softly half-blinking; frame4 torso returning, eyes open, tuft/loose yarn gently returning; frame5 near-neutral slight counter-sway of yarn ends; frame6 EXACT neutral reference standing pose. The head size and head vertical level stay constant, tiny body posture changes only, no body stretching, no whole-character tilt or floating, no large tail gestures. Preserve the reference knitted illustration style precisely. Six poses must read as one restrained, quiet continuous idle gesture, not separate expressive characters.
```

## 2026-09-07 踏み出し・立ち止まりの6ポーズ

- 手段：組み込みimagegen（CLI/API不使用）。既存主人公の見た目を参照し、地上移動のつなぎ専用に生成。
- 参照：`Assets/Resources/Art/HimoHitoPlayer-v1.png`。
- 採用：`Assets/Resources/Art/HimoHitoWalkTransitions-v1.png`（1254×1254、3列×2行）。上段は踏み出し、下段は停止〜毛糸の揺れ戻り〜落ち着き。
- 明るいチェック背景が残ったため、既存のUnity読込処理で中立色を除去する。元PNG自体は透過済みとは扱わない。
- 上部の頭幅でコマの大きさを正規化し、下部中央の足裏をピボットにする。最後のコマで足より下へ垂れる毛糸は接地基準から除外。通常歩行の既存サイズへ滑らかにつなぐ。

初回プロンプト：

```text
Use case: identity-preserve.
Asset type: Unity 2D character animation sheet for HimoHito.
Input: supplied pink yarn doll is the exact identity and illustration-style reference.
Make ONE square sheet containing SIX separate full-body poses in a strict 3-column by 2-row grid with equal cells. All face RIGHT. Preserve the same round pink knitted head, right-looking black bead eyes, short tuft, short torso and legs, loose yarn tails, dark plum outlines, warm pink highlights. SAME head size, head angle and torso proportions in every cell. Do not add arms, hands, clothes, props, ropes or facial expressions.
These are very subtle grounded starting/stopping motions, not a jump or running loop. Foot contact baseline is identical near 90 percent of EVERY cell, with ample clear margins.
Top row is START: 1) weight shifts onto the front foot, back heel rises slightly, tails hang with a slight left bend; 2) back toes push gently, front foot steps a small distance right with knee slightly bent, tails follow left; 3) first modest walking stride, front foot planted to the right and rear heel raised, tails trail softly left.
Bottom row is STOP: 4) front foot slightly forward and firmly planted for braking, back foot drawing closer, loose tails overshoot a little RIGHT from inertia; 5) feet almost together, knees barely flexed, tails curved softly right; 6) feet together in the original relaxed standing stance, tails settled nearly straight down, with the reference's little left bend.
No exaggerated splits, no new squash of the head or torso, no tilted/rotated whole drawings (a tiny lean is added by the engine). Only legs and loose yarn move. All feet remain near the contact baseline and at least one foot planted per pose. White eye glints preserved.
Genuinely transparent alpha background. No checkerboard, floor, ground line, shadows, grid, labels, text, frame numbers, scenery or extra objects.
```

採用した修正プロンプト（初回シートを編集）：

```text
Use case: precise-object-edit. Keep this exact 3x2 pink yarn doll sprite sheet, all 6 head sizes, faces, torso proportions, colors, same positions, same style, and entire top row completely unchanged. Change ONLY loose yarn tails in the first TWO cells of the BOTTOM row. Bottom-left (frame 4): loose yarn tails overshoot FORWARD toward the RIGHT side of the image, curling softly to the right behind the planted front foot, not to the left. Bottom-middle (frame 5): tails curl just a little to the RIGHT and downward, almost settled. Bottom-right remains the reference relaxed pose, tails downward and slightly left. Maintain original tail lengths and gentle curved shapes. Do not add limbs. Both lower-row frame 4 and 5 feet remain on the same ground baseline, do not stretch torso or head. All other artwork must remain unchanged. Output genuinely transparent alpha background with no checkerboard or white fill. No text, grid, floor, labels, extra objects.
```


## 2026-09-07 振り子専用の6ポーズ

- 手段：組み込みimagegen（CLI/API不使用）。元の立ち絵を参照して作成し、脚の変化が弱かった原案を再編集。
- 参照：`Assets/Resources/Art/HimoHitoPlayer-v1.png`。
- 採用：`Assets/Resources/Art/HimoHitoSwing-v2.png`（1254×1254、3列×2行、各セル418×627）。
- 全身の回転だけでなく、脚とほどけた毛糸が左へなびく→中立→右へなびく6枚。頭頂ピボットと頭の幅を実行時に正規化。
- 生成結果はRGBで明るい背景が残ったため、既存アニメーションと同じUnity側の中立色キー処理で透過する。元PNG自体はアルファ付きではない。

初回プロンプト：

```text
Use case: identity-preserve. Create a production Unity 2D animation sprite sheet of this EXACT pink knitted yarn doll. The supplied image is the identity/style reference. ONE square image with exactly SIX equal cells in a precise 3 columns by 2 rows grid, no borders or labels. Each cell contains ONE full-body doll, always facing RIGHT, upright torso and same head orientation as reference. Preserve identical round pink knitted head, two black bead eyes visible on right, tiny single upright yarn tuft at crown, dark plum outline, tiny torso, two short knitted legs, dangling loose yarn tails. NO added hands, no new rope overhead, no props. Head size, head position within each cell, crown attachment point, torso proportions, lighting and camera MUST be identical in all six cells. Only lower legs and loose tails change in SMALL steps, like soft yarn inertia while hanging from the top of its head, NOT a running cycle. Reading order: 1 legs and loose tails trail moderately LEFT; 2 trail slightly LEFT; 3 hang almost straight with a tiny LEFT bend; 4 hang almost straight with a tiny RIGHT bend; 5 trail slightly RIGHT; 6 trail moderately RIGHT. Feet stay below torso in every pose, never extended horizontally; knee bends small. Do not rotate or resize the head or torso between frames. Warm pink plies, black-plum contours, the exact same charming illustrated character. Identical generous transparent margins per cell; fully uncropped crown and feet. Transparent alpha background, no drawn checkerboard, no floor, shadows, motion lines, text or frame numbers.
```

採用した修正プロンプト（初回の6コマ画像を編集対象として使用）：

```text
Use case: precise-object-edit. Edit this 3x2 pink yarn-doll animation sprite sheet. KEEP THE TOP TWO THIRDS OF EACH DOLL EXACTLY UNCHANGED: identical head, eyes facing right, tuft, torso, style, scale and placement in all 6 cells. Change ONLY the legs and the loose yarn tails below the torso to create 6 DIFFERENT hanging pendulum poses. These are NOT standing poses: both feet are off the ground and relaxed, NO flat-footed standing in any cell. Reading order left to right, top row then bottom: (1) both legs lean left at 25 degrees with gently bent knees, loose tails trail left, (2) both legs lean left at 15 degrees with relaxed knees, tails slightly left, (3) legs hang nearly straight with left foot subtly behind, tails just left of vertical, (4) legs hang nearly straight with right foot subtly behind, tails just right of vertical, (5) both legs lean RIGHT at 15 degrees and tails trail RIGHT, (6) both legs lean RIGHT at 25 degrees with gently bent knees and all loose tails stream RIGHT (not left!). Knees remain below the torso, feet never above hips. Keep original leg length and thickness. Nothing else changes, no motion lines, labels or grid. Make background genuinely transparent alpha, no checkerboard pattern.
```


## HimoHitoTitleBackground-v1.png

- 用途：タイトル画面背景
- 生成日：2026-09-04
- 使用ツール：Codex built-in image generation
- 参照画像：`Assets/Resources/Art/TutorialNightChildRoom-v1.png`
- 出力：`Assets/Resources/Art/HimoHitoTitleBackground-v1.png`

```text
Use case: illustration-story
Asset type: 16:9 Unity title-screen background for the 2D game ヒモヒト
Input image: use the attached existing bedroom background strictly as the style, palette, material, lighting, texture, and world-building reference; create a new composition rather than painting over it.
Primary request: a wide side-view of the same quiet child’s bedroom at night. Place a wooden toy box slightly right of center with its lid half open, warm golden light spilling from inside onto the wooden floor. A single loose dusty-pink yarn thread runs out of the box across the floor toward the lower-left corner and exits the frame.
Scene details: only objects that already exist in the reference world—picture books, felt star, knitted ball, wooden toy blocks, toy train, shelves, and curtains. Keep the left half dark, calm, uncluttered, and largely empty so Unity can overlay the game title and menu there.
Style/medium: 2D hand-painted storybook game illustration; flat-ish forms, soft gradients, gentle rim light, subtle paper grain; knitted wool, felt, painted wood, and matte plastic textures.
Composition/framing: exact 16:9 landscape; main subject on the right third; large clean negative space across the left half; no important object at screen edges.
Lighting/mood: one warm lamp light from the upper left, otherwise deep indigo-violet nighttime; quiet, expectant, a little lonely but warm.
Color palette: #1A1430 to #2B2150, dusty pink #FF5C8A, toy blue #3E8FD0, wooden orange #E0862F, warm light #FFD9A0.
Constraints: no text, no letters, no logos, no watermark, no UI, no people, no human characters; maintain visual continuity with the reference image.
Avoid: photorealistic, 3D render, CGI, anime face, harsh contrast, neon colors, cluttered composition, busy center, lens flare.
```

## HimoHitoAppIcon-v1.png

- 用途：Windows版exeアイコン／スプラッシュロゴ
- 生成日：2026-09-04
- 使用ツール：Codex built-in image generation
- 参照画像：`Assets/Resources/Art/HimoHitoPlayer-v1.png`
- 出力：`Assets/Resources/Art/HimoHitoAppIcon-v1.png`

```text
Use case: logo-brand
Asset type: Windows game executable icon for the Unity game 「ヒモヒト」
Primary request: Create a polished square app icon based on the pink yarn character in the reference image.
Input image: the reference is the character design and color/style reference; preserve its recognizable round knitted head, tiny dark eyes, short knitted body, and single yarn strand rising from the head.
Scene/backdrop: deep midnight navy circular-to-square backdrop with a very subtle soft violet glow; no room scenery.
Style/medium: warm hand-crafted children’s toy game icon, clean 2D painted illustration, soft yarn texture, bold readable silhouette.
Composition/framing: centered close-up bust/upper-body portrait, character fills roughly 72% of the square, generous safe margin so it remains clear at 16x16 pixels.
Color palette: vivid warm pink character, dark plum outline, midnight navy background, tiny warm cream highlight.
Constraints: square 1:1, no text, no letters, no watermark, no checkerboard pattern, no transparency grid, no extra objects, no border frame, no photorealism.
```

## HimoHitoControlsBackground-v1.png

- 用途：タイトル画面およびゲーム中の操作説明背景
- 生成日：2026-09-04
- 使用ツール：Codex built-in image generation
- 参照画像：`TutorialNightChildRoom-v1.png`、`HimoHitoTitleBackground-v1.png`
- 出力：`Assets/Resources/Art/HimoHitoControlsBackground-v1.png`

```text
2D game illustration for a picture-book style puzzle platformer.
Setting: a child's bedroom at night, seen as a cozy toy-box world.
Palette: deep indigo-violet base (#1A1430 to #2B2150), dusty pink yarn (#FF5C8A),
toy-blue painted plastic (#3E8FD0), warm wooden-block orange (#E0862F),
one warm lamp light (#FFD9A0) as the only light source, coming from the upper left.
Materials: knitted wool, felt, painted wood, matte plastic toys.
Rendering: flat-ish shapes with soft gradients, gentle rim light, subtle paper grain,
no harsh shadows, hand-painted storybook feel, calm and quiet.
No text, no letters, no logos, no watermark, no UI, no human characters.
16:9 aspect ratio.

Scene: a close, calm corner of the same bedroom. A large sheet of pale cream
drawing paper is pinned flat to the wall with two wooden pegs, softly lit by the
lamp. The paper is completely blank and evenly lit, occupying the central 70
percent of the frame. Around only the outer edges: wooden toys, a small knitted
pink yarn doll looking up at the paper, a spool of yarn, and coloured pencils.
The paper has faint fibre texture but no drawings, lines, symbols, or text.
Composition: paper front-facing and centred, with a large clean rectangular area
for Unity to overlay the Japanese controls.

Avoid: photorealistic, 3D render, CGI, anime face, text, letters, numbers,
symbols, watermark, signature, harsh contrast, neon colors, cluttered centre,
lens flare, perspective-distorted paper, torn paper, ruled paper.
```

## HimoHitoEndingBackground-v1.png

- 用途：本編およびチュートリアルのクリア画面背景
- 生成日：2026-09-04
- 使用ツール：Codex built-in image generation
- 参照画像：`HimoHitoTitleBackground-v1.png`、`HimoHitoControlsBackground-v1.png`、`TutorialNightChildRoom-v1.png`
- 出力：`Assets/Resources/Art/HimoHitoEndingBackground-v1.png`

```text
Use case: illustration-story
Asset type: 16:9 Unity game ending / clear-screen background
Primary request: Create the Part D image 3 ending scene for HimoHito, matching the supplied game artwork exactly in world, palette, material treatment, and storybook rendering.
Input images: Image 1 is the title-screen style and toy-box reference; Image 2 is the knitted pink doll and warm paper-light reference; Image 3 is the bedroom environment and background reference.

2D game illustration for a picture-book style puzzle platformer.
Setting: a child's bedroom at night, seen as a cozy toy-box world.
Palette: deep indigo-violet base (#1A1430 to #2B2150), dusty pink yarn (#FF5C8A),
toy-blue painted plastic (#3E8FD0), warm wooden-block orange (#E0862F),
one warm lamp light (#FFD9A0) as the only light source, coming from the upper left.
Materials: knitted wool, felt, painted wood, matte plastic toys.
Rendering: flat-ish shapes with soft gradients, gentle rim light, subtle paper grain,
no harsh shadows, hand-painted storybook feel, calm and quiet.
No text, no letters, no logos, no watermark, no UI, no human characters.
16:9 aspect ratio.

Scene: the same wooden toy box, lid fully open, seen slightly from above and to the side. A very small knitted doll made of dusty pink yarn is settling safely inside the box among other familiar toys: one felt star, one wooden toy train, and one knitted ball. The pink doll must look visibly thinner and more unravelled than in the controls reference, with a narrow yarn body but still clearly recognizable as the same gentle doll. Warm golden light fills the box from within and spills softly onto the wooden floor. Behind the box, one long continuous dusty-pink yarn trail leads away from the doll and recedes into the upper-left darkness of the bedroom, clearly marking the path the doll travelled and the yarn it left behind.
Mood: arrival, warmth, quiet accomplishment, with a small note of sacrifice.
Composition: toy box centred in the lower third, lid open, dark bedroom surrounding it, yarn trail receding toward upper-left. Preserve a calm, uncluttered dark area across the upper centre and upper right for Unity to overlay CLEAR and run statistics. Keep all important subjects away from the outer 7 percent safe margins.
Lighting/mood: only the warm upper-left lamp and the soft glow from inside the box; quiet bedtime atmosphere.
Constraints: game-world objects only; the doll is inside the box; its body is thin and unravelled; the path is shown by one continuous yarn trail; text overlay area stays legible and dark.
Avoid: photorealistic, 3D render, CGI, anime face, text, letters, numbers, symbols, watermark, signature, harsh contrast, neon colors, cluttered composition, busy upper center, lens flare, extra characters, giant doll, severed yarn, multiple yarn trails, daylight.
```

## HimoHitoFailureBackground-v1.png

- 用途：本編およびチュートリアルの失敗画面背景
- 生成日：2026-09-04
- 使用ツール：Codex built-in image generation
- 参照画像：`HimoHitoTitleBackground-v1.png`、`HimoHitoEndingBackground-v1.png`、`TutorialNightChildRoom-v1.png`
- 出力：`Assets/Resources/Art/HimoHitoFailureBackground-v1.png`

```text
Create a production-ready 16:9 failure-screen background illustration for the Unity 2D puzzle-platformer HimoHito, matching the three supplied project references exactly in visual language and room design.

STYLE BASE:
2D game illustration for a picture-book style puzzle platformer. Setting: a child's bedroom at night, seen as a cozy toy-box world. Palette: deep indigo-violet base (#1A1430 to #2B2150), dusty pink yarn (#FF5C8A), toy-blue painted plastic (#3E8FD0), warm wooden-block orange (#E0862F), one warm lamp light (#FFD9A0) as the only light source, coming from the upper left. Materials: knitted wool, felt, painted wood, matte plastic toys. Rendering: flat-ish shapes with soft gradients, gentle rim light, subtle paper grain, no harsh shadows, hand-painted storybook feel, calm and quiet.

SCENE:
The bedroom floor at night, seen from very low and close, almost at floor level. A short loose piece of dusty-pink yarn lies alone on the wooden floorboards in the lower third, its end slightly frayed. A few subtle dust motes float in the dim light. Far in the background, out of focus, the same wooden toy box from the supplied references is visible in the upper-right area, its inner warm light now dim and distant, clearly unreachable. Everything else falls into soft indigo shadow.

MOOD:
A quiet setback. Not cruel, not tragic, not frightening; gently encouraging another try. The yarn should feel precious and recoverable, not dead or destroyed.

COMPOSITION / UI SAFE AREA:
The yarn is the foreground focal point across the lower third. Keep the center-left and central upper half calm, dark, and uncluttered so Unity can overlay large Japanese failure text and retry instructions with excellent readability. The toy box must remain small and softly blurred in the upper right. Strong depth-of-field distinction between the close yarn and distant toy box.

OUTPUT:
One clean 16:9 raster illustration, no borders. No text, no letters, no numbers, no logos, no watermark, no UI, no human characters, no knitted doll character. Avoid photorealistic, 3D render, CGI, anime face, harsh contrast, neon colors, cluttered composition, busy center, lens flare. Taxonomy: illustration-story.
```

## HimoHitoFarBackground-v1.png

- 用途：チュートリアルと本編の遠景視差レイヤー
- 生成日：2026-09-04
- 使用ツール：Codex built-in image generation
- 参照画像：`Assets/Resources/Art/TutorialNightChildRoom-v1.png`
- 出力：`Assets/Resources/Art/HimoHitoFarBackground-v1.png`

```text
Use case: illustration-story
Asset type: horizontally repeatable far-background layer for a Unity 2D side-scrolling puzzle platformer, 16:9 landscape
Input image: use the supplied HimoHito child-bedroom artwork only as the exact style, palette, wallpaper-pattern, material, and nighttime-lighting reference

Primary request: create only the far back wall of the same child's bedroom, horizontally seamless and suitable for repeated tiling. The entire image should feel several metres behind the playable area.

Scene/backdrop: faint indigo wallpaper with the same small-leaf pattern. Include only very soft distant silhouettes: a tall bookshelf silhouette toward the right, a window with pale blue moonlight toward the left, and a subtle hanging paper garland along the top edge. Everything must be heavily soft-focused, low contrast, quiet, and noticeably darker than the supplied main background. No foreground objects and absolutely no floor or baseboard.

Style/medium: 2D hand-painted picture-book game illustration, flat-ish shapes, soft gradients, subtle paper grain, calm and quiet. Materials should read only faintly because this is a distant layer.

Composition/framing: flat and even, no strong focal point, no central subject. Keep the central 55 percent mostly wallpaper and shadow. The left and right edges must be plain matching wallpaper with identical brightness, color, and leaf-pattern rhythm so copies connect without a visible seam. Do not place the window, bookshelf, garland endpoint, highlight, or shadow directly on either edge.

Parallax intent: designed to move at 78 percent of camera travel behind the existing background, so distant silhouettes must remain subtle and must not compete with gameplay.

Color palette: deep indigo-violet #1A1430 to #2B2150, very muted toy-blue #3E8FD0, only a faint pale moon glow. No warm lamp focal point.

Constraints: one clean opaque 16:9 raster image; no floor; no foreground toys; no character; no pink yarn; no text; no letters; no numbers; no logos; no watermark; no UI; no human characters. Keep both edge strips visually matching for horizontal tiling.

Avoid: photorealistic, 3D render, CGI, anime, sharp objects, strong contrast, bright light, neon colors, clutter, busy center, perspective floor, baseboard, visible seams, vignette that darkens the edges differently.
```

## HimoHitoUiParts-v1.png

- 用途：Part D画像⑥。HUDゲージ、結び目、接続リング、操作ボタン、失敗画面のほつれ端
- 生成日：2026-09-04
- 使用ツール：Codex built-in image generation
- 参照画像：`HimoHitoTitleBackground-v1.png`、`TutorialRopeAnchorRing-v1.png`、`TutorialBlockPlatform-v1.png`
- 出力：`Assets/Resources/Art/HimoHitoUiParts-v1.png`
- 加工：生成時の白背景をUnity読込時に透明化し、1枚のシートから各パーツを切り出して表示

```text
Use case: stylized-concept
Asset type: Unity 2D game UI sprite sheet with transparent background
Primary request: Create one clean sprite sheet containing exactly five isolated,
separate UI assets for the game HimoHito, all matching the supplied nighttime
children's-bedroom picture-book artwork: (1) a long horizontal HUD gauge frame
made from one warm orange wooden toy stick, with small dusty-pink yarn wraps tied
around both ends and an empty transparent middle; (2) one simple dusty-pink yarn
knot shown front-on; (3) one small toy-blue painted wooden connector ring with a
clear center hole, front-on; (4) one wide rounded rectangular button plate made
from warm orange toy wood, completely blank; (5) one short frayed end of dusty-pink yarn.
Input images: Image 1 is the main world and lighting reference; Image 2 is the
connector ring material and shape reference; Image 3 is the warm wooden toy material reference.
Scene/backdrop: genuinely transparent background, no checkerboard pattern rendered into the image.
Style/medium: flat-ish hand-painted 2D picture-book game art, soft gradients,
subtle paper grain, matte painted wood and knitted yarn; consistent with the supplied references.
Composition/framing: arrange the five assets in one evenly spaced horizontal row,
each fully visible with generous empty transparent space between items; no overlap;
straight-on orthographic presentation; consistent scale suitable for cutting into sprites.
Lighting/mood: soft upper-left warm light, gentle rim light, no cast shadows outside each object.
Color palette: deep indigo only in tiny shaded details, dusty pink yarn #FF5C8A,
toy-blue #3E8ED0, warm wooden orange #E0862F, pale warm highlight #FFD9A0.
Materials/textures: knitted wool fibers, softly painted wooden toy surfaces.
Constraints: exactly five objects; no text, no letters, no numbers, no icons beyond
the described objects; crisp isolated silhouettes; preserve true alpha transparency;
no perspective; no human characters; no extra decorations; no borders around the sheet.
Avoid: checkerboard backdrop, opaque white or grey backdrop, photorealism, 3D render,
CGI, neon colors, UI text, labels, watermark, signature, clutter.
```

## HimoHitoKeyVisual-v1.png

- 用途：第7回資料Part D画像⑦。README先頭、応募フォーム、プレイ動画のサムネイル
- 生成日：2026-09-05
- 使用ツール：Codex built-in image generation
- 参照画像：`HimoHitoTitleBackground-v1.png`、`HimoHitoPlayer-v1.png`、`TutorialHookConnector-v1.png`
- 出力：`Docs/Images/HimoHitoKeyVisual-v1.png`

```text
Use case: ads-marketing
Asset type: 16:9 game key visual for the top of a README, application form thumbnail, and gameplay-video thumbnail.
Input images: Image 1 is the required bedroom world, palette, lighting, and painterly picture-book style reference. Image 2 is the exact small pink knitted yarn doll character reference. Image 3 is the blue toy connector design reference; use a compact connector/ring form at each bridge endpoint.
Primary request: Create one cinematic hero image that explains the game idea at a glance: a small pink knitted yarn doll stands at the center of a bridge made from its own dusty-pink yarn. The yarn bridge stretches between two blue toy connectors and sags gently under the doll's weight in a smooth catenary curve. Below the bridge is deep darkness. Far behind and above, a warm wooden toy box glows small in the bedroom.
Scene/backdrop: a child's bedroom at night, cozy toy-box world, consistent with Image 1.
Subject: the doll must be small in the frame; the sagging yarn bridge is the main subject. Match the yarn texture, silhouette, and colors of Image 2 closely.
Style/medium: 2D game illustration for a picture-book puzzle platformer; flat-ish shapes, soft gradients, gentle rim light, subtle paper grain, hand-painted storybook feel; calm and quiet.
Composition/framing: wide 16:9. Bridge spans the lower-middle third. Large quiet dark negative space above. Doll centered on the lowest part of the bridge. Blue connectors clearly visible at both ends. The warm toy box is distant, small, and secondary in the upper background.
Lighting/mood: one warm lamp/toy-box glow from the upper left; otherwise deep indigo-violet night. Mood: a small creature crossing a gap it made from itself.
Color palette: deep indigo-violet #1A1430 to #2B2150, dusty pink yarn #FF5C8A, toy-blue painted plastic #3E8FD0, warm wooden orange #E0862F, warm lamp light #FFD9A0.
Materials/textures: knitted wool, felt, painted wood, matte plastic toys.
Constraints: no text, no letters, no logos, no watermark, no UI, no human characters; preserve the existing project's visual language; readable at thumbnail size.
Avoid: photorealism, 3D render, CGI, anime face, harsh contrast, neon colors, cluttered composition, busy center, lens flare, extra characters, multiple dolls, realistic humans.
```

## TutorialGuideSign-v1.png

- 用途：チュートリアルT1～T4の各区間開始地点に置く、`Z`説明用の木製看板
- 生成日：2026-09-05
- 使用ツール：Codex built-in image generation
- 参照画像：ユーザー提示の一本脚木製看板、`TutorialNightChildRoom-v1.png`、`TutorialBlockPlatform-v1.png`、`HimoHitoUiParts-v1.png`
- 出力：`Assets/Resources/Art/TutorialGuideSign-v1.png`
- 加工：生成結果の中立色チェック模様だけを透過し、Unity Sprite用の実アルファへ整えた

```text
Use case: stylized-concept
Asset type: production-ready Unity 2D world sprite for a tutorial sign.
Primary request: Preserve the reference sign's essential silhouette: one wide
horizontal wooden board made from three joined planks, supported by exactly one
centered vertical post. Keep it mostly front-facing for a side-view platform game.
Match the established HimoHito artwork with warm orange-brown painted toy wood,
softly rounded handmade edges, subtle grain, cozy picture-book rendering, a warm
upper-left highlight, and soft indigo shadow. Wrap a small amount of dusty-pink
yarn around the board/post joint and add one small toy-blue painted tack. Leave
the central board blank for an interaction symbol.
Composition: isolated single sign, centered, entire post visible, transparent
background, readable at small size, no cast shadow outside the object.
Avoid: text, letters, numbers, logos, arrows, multiple signs, extra posts,
background scenery, checkerboard, white background, pixel art, Minecraft style,
photorealism, 3D render, CGI, neon colors, watermark.
```

## 2026-09-07 接続ヒモ用の毛糸テクスチャ

- 手段：組み込みimage_gen（CLI/API不使用）
- スタイル参照：`Assets/Resources/Art/HimoHitoPlayer-v1.png`の頭頂の毛糸
- 採用素材：`Assets/Resources/Art/HimoHitoYarnRope-v1.png`
- 生成素材はRGBで背景が焼き込まれていたため、Unity側の`YarnRopeTexture`で背景除去・縦余白除去・繰り返し境界の調整を行う。原画像そのものに透過情報はない。

初回プロンプト：主人公の頭頂の毛糸に合わせた、水平で左右端まで連続するピンクの撚り糸。柔らかい手描き、マゼンタの溝、暖かなピンクのハイライト、濃いプラムの輪郭。文字・結び目・背景なし、左右シームレス、透明PNGのゲーム用テクスチャ。

採用した再生成のプロンプト：

```text
Edit target: supplied rope texture. Remove ALL white gray checkerboard background and output genuine alpha transparency, NOT a drawn checkerboard. Preserve the pink twisted illustrated yarn. Crop tightly to yarn vertically with tiny transparent margins. Make the left and right edges match perfectly for repeating seamless texture. Single straight horizontal continuous cord runs from left to right edge with no end caps; no top/bottom empty composition. Keep the same hand-painted pink plies and plum outline, no other elements. Transparent PNG game texture.
```

## 2026-09-07 帰り道スイッチ

組み込みimage_gen使用。参照はTutorialGuideSign-v1.png。採用素材はAssets/Resources/Art/RecoverySwitch-v1.png。背景はUnityの既存透過処理で除去。

```text
Create ONE isolated game sprite for HimoHito, matching the reference's warm hand-painted toy wood and pink twisted yarn. A small squat floor push-button: rounded honey wooden base, raised raspberry-pink knitted cushion button on top, a tiny twisted pink yarn tie on its side. Straight front view, very slight top visibility, horizontal silhouette width:height about 2:1. Readable at tiny size, rounded edges and rich cozy wood grain, soft highlights, dark plum/brown contours. No signpost, no text, no letters, no icons, no metal, no scenery, no floor shadow. Entire object uncropped, centered with modest empty padding. Transparent background actual alpha preferred; otherwise pure white background, never checkerboard. Do not include the reference sign; use only its art style.
```
## 2026-09-07 ゴール宝箱の開閉

組み込みimagegen使用。参照：Assets/Resources/Art/TutorialToyBoxGoal-v2.png。
採用素材：Assets/Resources/Art/ToyBoxOpening-v1.png（2×2、左上から閉／開き始め／途中／全開）。生成画像の明るい中立色の背景は既存素材と同じ基準で実行時に透過し、各コマの底と幅をそろえる。

```text
Use case: precise-object-edit. Create a production Unity 2D sprite sheet of this EXACT wooden toy chest opening. Preserve golden orange wood, blue U-shaped handle, two blue hinges, front view, rounded handmade toy aesthetic from reference. Output a square sheet with FOUR equal cells in a 2 by 2 grid, no lines or labels or text. Reading order: top-left CLOSED lid seated shut; top-right lid opening about 25 degrees; bottom-left lid about 55 degrees open; bottom-right fully open like reference. Each cell has EXACT SAME chest base width, base height, handle, camera, and base bottom alignment. Only lid angle changes. Base is centered horizontally, chest base bottom at 90 percent of cell height, base width 78 percent of cell width; fully open fits under 10 percent top margin. Leave generous clear separation between cells. Real transparent alpha background, no checkerboard, no floor, no cast shadows outside chest, no glow, no particles, no gold or contents. Four separate nonoverlapping evenly sized animation frames, do not create any extra objects. Match reference chest design and finish closely.
```
## 2026-09-07 木製の棘

組み込みimagegen使用。スタイル参照：Assets/Resources/Art/TutorialBlockPlatform-v1.png。
採用素材：Assets/Resources/Art/WoodenToySpike-v1.png。背景は既存の中立色除去処理で透過。

```text
Use case: stylized-concept. Asset type: ONE isolated Unity 2D hazard sprite. Reference image is STYLE AND MATERIAL ONLY, not a layout to reproduce. Create a single upright triangular wooden spike block for a cozy nighttime toy-room game starring a pink yarn doll. Front orthographic view, symmetric broad base and one sharp apex at top centre; silhouette an isosceles triangle, width about equal to height. Rich muted vermilion-red painted wood, warm subtle visible wood grain, slightly worn honey-wood bevel along edges, deep burgundy outline/shadow and soft warm highlights. Match the reference toy blocks' hand-painted polished wooden finish, detailed but readable at small size. Keep hazard visibly sharp and red, not a soft cushion or safe orange platform. Entire shape within frame, flat horizontal bottom edge. No separate stand/base, no multiple spikes, no nails, no yarn, no text, no scenery, no floor or cast shadow. Transparent alpha background, not a painted checkerboard. Center one object with narrow clear margins.
```
