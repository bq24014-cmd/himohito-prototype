# 帰り道スイッチを押す主人公の6コマ

- 生成方法：imagegenスキル、built-in image_gen（参照画像の編集）。
- 参照：`Assets/Resources/Art/HimoHitoWeave-v1.png`。
- 採用：`Assets/Resources/Art/HimoHitoSwitchPress-v1.png`（1536×1024、3列×2行）。
- 順序：立つ→かがむ→手を伸ばす→押す→起き上がる→立つ。
- 元PNGは白背景。既存のLoadAnimationFramesで白背景をアルファ0にしてから切り出す。チェック模様を透過として採用していない。
- 頭幅と足元を各コマで登録し、屈伸を縦に引き伸ばさない。反対側から押す場合は左右反転する。

## 初回生成プロンプト

Use case: stylized-concept. Asset type: Unity 2D animation sprite sheet, 3 columns by 2 rows, 1536x1024. Input image 1 is a CHARACTER AND STYLE REFERENCE ONLY. Create six full-body poses of exactly this pink knitted yarn character facing RIGHT, for crouching and pressing a low floor button with the near hand. Match huge round knitted head, two dark oval eyes, small narrow yarn body, tiny yarn feet, head tuft and trailing yarn tails, pink palette, black outlines and illustrated knit pattern. Keep head size identical in all six cells and feet on the same local baseline. Read order top left to bottom right: 0 neutral upright looking right; 1 knees partly bent, head looks down, arm reaching diagonally down-right; 2 deep crouch, leaning slightly forward, near hand palm down reaching to a point just ahead of the right foot at ankle height; 3 same crouch, hand slightly lower firmly pressing, tuft bends with effort; 4 recovering half standing, hand retracting; 5 neutral upright matching pose 0. Draw the CHARACTER ONLY, do not draw the button, props, floor, arrows, labels, text, panels or shadows. Six evenly spaced separate sprites entirely inside equal cells. Genuine transparent alpha background, NOT a painted checkerboard or white rectangle. Leave generous empty margin around every character. Preserve identity and detailed yarn silhouette; do not stretch the character between frames.

## 中間修正プロンプト（実機描画で手が低いため再調整）

Preserve this exact character sprite sheet and six poses. Make two precise production corrections: In the crouched top-right and bottom-left cells raise the forward extended hand 50 pixels above the feet, palm flat facing DOWN, fingers pointing right, so the hand presses on the TOP of an imaginary low button. Keep the arm diagonal downward, not horizontal and not palm-up. Do not draw any button. Replace every background checker square with a perfectly uniform clean pure WHITE (#FFFFFF) background, no grid, no grey, no pattern, no texture, no shadow anywhere outside the six characters; this project's sprite importer will remove white. Do not alter the character's head, eyes, feet, baseline, colors, yarn texture or 3-column 2-row layout. No text.

## 最終採用プロンプト

Use case: precise-object-edit. Image 1 is the EDIT TARGET: six-frame pink yarn character sheet. Image 2 is a CONTEXT REFERENCE showing the in-game red button: it is much higher than the reaching hand. Correct the sheet, output only the 3x2 character sheet on uniform pure white. The button top is approximately one third of the standing character height above the feet. Change poses 1,2,3,4 so the character bends its knees only slightly, keeps its head ABOVE the imagined button, and extends the near arm almost HORIZONTALLY to the RIGHT at WAIST height, flat palm facing DOWN, fingertips forward. In frame 2 the hand is at 40% of standing height above soles, in frame 3 pressing at 33% above soles. NOT near the ground. Keep soles on the exact same baseline per row. Pose 0 and 5 upright stay unchanged. Preserve character identity, pink knit texture, slim body, head size, black outlines, layout and white background. Draw no button, floor, shadows, text or guide lines. The character must press the TOP of the button, not the side or the floor.
