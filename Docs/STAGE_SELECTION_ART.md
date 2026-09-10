# ステージ選択・専用アート（2026-09-10）

## 採用と保存先

ユーザー要望「無理に既存の画像を使わず世界観に合う形」に合わせて新規制作。夜の子供部屋の工作台に、紺色の布製マップ・小さな玩具の島・毛糸の道を置く構成。既存の操作説明の紙背景、同じ宝箱の繰り返し、浮いた主人公マーカー、木製ボタンの流用をこの画面から外した。主人公／ゲーム内の画像は変更しない。

- `Assets/Resources/Art/HimoHitoStageMapBackground-v1.png`：専用背景。文字・道・ステージを焼き込まず再利用可能。
- `Assets/Resources/Art/HimoHitoStageMapPieces-v1.png`：左から練習場の島／宝箱への積み木の島／布ラベル。Unity側で3分割、余白を詰め、島は縦横比を維持。
- 画像生成：imagegenスキル、**built-in image_gen**。CLI/APIキーによる生成は使用しない。
- 元出力はCodexのgenerated_imagesに保持し、採用PNGを上記Assetsへコピーした。却下したチェッカーボード背景の版はプロジェクトへ取り込んでいない。
- 素材の透過指定が灰色チェック模様として描かれたため、同じツールで背景だけを純白へ修正。最終PNGはRGBの白背景。専用アトラスのローダーが無彩色の白を一度だけ透過してキャッシュし、ゲーム画面へ白背景を出さない。クリーム色の布・着色した繊維を保つ。読み取り可能・非圧縮でインポートする。
- ラベル、説明、ページ、選択枠は実装側。M PLUS Rounded 1cを維持。移動・物理・資源・音・シーン開始ロジックは変更しない。

## プロンプト1：背景（新規生成）

同日のメニュー統合では画像を再生成せず、ゲーム名・操作説明・ゲーム終了をこの布マップ上へ移設した。旧タイトル画面は描画しない。現在の実装プレビューは [StageSelection-UnifiedMenu-v1.png](Images/StageSelection-UnifiedMenu-v1.png)。以下のプロンプトは素材制作時の記録として維持する。

Use case: stylized-concept. Asset type: production-ready game stage-selection BACKGROUND ONLY for the original Japanese yarn-platformer HimoHito. Generate a new 16:9 landscape image, 1536x864 or larger.
Scene: a cozy child's nighttime craft table, seen almost top-down with only slight perspective, as a beautiful tactile stop-motion miniature game world. A very large midnight indigo and desaturated plum woven felt mat fills the central 90% of the canvas, with rounded corners and a delicate running stitch around its perimeter. Warm honey wooden tabletop is visible only at the outer edges. Soft amber lamplight spills from the upper left, soft cool moonlight on the right. Yarn has detailed soft fibers, wood is warm and gently worn. Restrained plush high-quality 3D picture-book rendering, matching an amigurumi toy game, not flat vector and not pixel art.
Composition matters for live UI: the central rectangle from x=12% to 90%, y=12% to 87% must be EMPTY uninterrupted dark felt with subtle low-contrast fabric texture only. All decoration must remain at the extreme margins: a partial rose-pink ball of yarn and loose thread at lower left corner, a tiny pair of wooden toy blocks in upper right corner, a few subdued embroidered stars only around the outer seam. Bottom right a cropped small wooden spool. Nothing bright or busy under future UI. No scene islands or buttons baked in, no ropes crossing the middle, no paper sheet.
Mood: inviting, warm, quiet nighttime adventure, crafted with care; generous visual breathing room. Original artwork. No characters, no text, no letters, no logos, no watermark. This is an actual reusable game asset, not a screenshot or mockup.

## プロンプト2：専用パーツ（新規生成）

Use case: stylized-concept. Asset type: a game sprite atlas, genuinely TRANSPARENT background with real alpha, for an original cozy yarn-and-wood toy platformer. New illustration, premium tactile stop-motion 3D picture-book look. Wide 3:2 canvas, exactly THREE separate objects arranged in ONE evenly spaced horizontal row, one object centered at x=16.67%, one at x=50%, one at x=83.33%. Each complete object wholly inside its own third of the canvas with at least 5% padding and clean separation. Every area outside objects transparent, no checkerboard, no matte.
Object 1 left: a small circular teal stitched felt toy island holding two honey-wood toy blocks with a short rose-pink twisted yarn bridge between them, a miniature blue hanging ring fixed to a small wooden arch, a little pink yarn ball beside it. This is a miniature beginner's yarn practice playground, instantly legible, with no character.
Object 2 middle: a small circular mauve/berry stitched felt toy island holding three wooden building blocks stepping upward toward a beautiful honey-wood toy treasure chest with blue hinge and handle, lid half open with a very restrained warm glow, pink yarn winding along the blocks. Readable strong toy silhouette, not a fantasy castle.
Object 3 right: one EMPTY cream linen luggage label for UI, landscape width:height 4:1, softly rounded corners, subtle visible cloth weave, a fine pink running stitch near the edge, a small teal-blue sewn button on each short end. No writing. The label lies flat facing the camera. It is a separate flat UI asset, not an island.
For the two islands: identical scale and 30-degree elevated three-quarter camera, soft top-left amber lighting, detailed wood grain, soft wool fibers and embroidery, short contact shadows only under their bases within the transparent silhouette, no background scene, no giant glow. Main colors dusty rose yarn, teal, honey wood, navy shadow. No people, no characters, no text, no symbols, no numbers, no logos, no watermark. No extra objects outside these three items. This is modular production game art, not an assembled menu.

## プロンプト3：パーツの背景のみ修正（編集）

参照画像はプロンプト2の出力。島とラベルのデザイン・位置・比率を維持する編集。

Edit target: the attached three-item sprite atlas. Preserve all three objects exactly: their design, position, size, color, detail, and empty space. Change ONLY the background. Remove the entire grey checkerboard pattern, the wrinkled fabric behind it, and background shadows. Replace every pixel outside the three objects with a perfectly uniform solid pure white RGB(255,255,255) studio background. Pure flat WHITE, not transparency and not a checkerboard. NO grey grid or texture anywhere. Clean antialiased silhouette edges, no cast shadows on the white background. Keep the islands' own felt bases and the cream fabric label fully intact. No extra items, no text. Same exact atlas composition and size. The white is a technical color-key background for a game importer.
