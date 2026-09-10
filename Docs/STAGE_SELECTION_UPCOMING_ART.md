# ステージ追加予告の島（2026-09-10）

ユーザーの「他のステージ実装予定というのが分かるようにしたい」に合わせ、既存2ステージの島と背景を保ち、3つ目の予告用イラストを追加した。具体的な新ステージの地形や公開日を約束せず、布をかぶせて中身を隠した小さな玩具の島で表現する。

## 素材と扱い

- 採用素材：`Assets/Resources/Art/HimoHitoStageMapUpcoming-v1.png`（同名.metaも保持）。
- 生成：imagegenスキルの **built-in image_gen**。CLI・APIキーは使用しない。
- 元の生成画像と修正版はCodexのgenerated_imagesに保持。初回は透過指定がチェック模様になったため不採用にし、同じツールで背景だけを純白へ修正した。
- 採用PNGは白背景のRGB画像。StageSelectionViewが既存の3パーツと共通の白背景除去処理で透過し、余白を詰めたSpriteを一度だけ作って保持する。島の縦横比・色付きの繊維・クリーム色の縫い目を維持し、Resources原本を書き換えない。
- `StageCatalog.json` の `upcoming` 枠から `Art/HimoHitoStageMapUpcoming-v1` を参照する。`available: false`、シーンパスは空。既存の2ステージと共に表示するが、今後の追加を知らせる枠でありプレイ可能なステージではない。
- 「COMING SOON / 追加予定」は画像に焼き込まずUIのM PLUS Rounded 1cで表示。予告へ続く道だけ毛糸を途切れさせ、開始ボタンも無効化する。カメラ・アニメーション・物理・資源計算・音は変更しない。
- Unityで描画した実装プレビュー：[StageSelection-Upcoming-v1.png](Images/StageSelection-Upcoming-v1.png)。

## 生成プロンプト

```text
Use case: stylized-concept.
Asset type: ONE production game sprite for an upcoming destination on the cloth stage-selection map of HimoHito, an original cozy yarn-and-wood toy platformer.
Primary request: an as-yet-unrevealed miniature toy island, covered in fabric, to show that new stages will be added later. This is an icon only, NOT a full menu or mockup.
Subject: a small thick circular dusty-lavender felt toy base with cream running stitches around its side. On the base is one mysterious low rounded mound under a soft muted teal woven cloth, with tasteful draped folds and a stitched hem. The concealed shape must NOT reveal a specific stage, castle or treasure chest. A loose rose-pink yarn loop lies at the base, implying a journey yet to be woven. No character. Keep the silhouette compact and instantly readable at 200px.
Style: premium tactile stop-motion miniature / 3D picture-book game rendering, detailed wool fibers and woven threads, softly worn handcrafted materials, restrained colors, warm amber upper-left light and very subtle cool light from right. Same visual family as plush amigurumi, honey wooden toys, pink twisted yarn, teal and dark-indigo felt. Camera elevated 30 degrees in three-quarter view; round base appears elliptical.
Composition: centered single object, square canvas, the entire base and cloth visible with modest 8% transparent margin; object fills canvas. No scene background, no floor or external cast shadow.
Background: GENUINELY TRANSPARENT with actual alpha outside the isolated object. No white matte, no grey checkerboard, no grid. Preserve tiny fiber-edge alpha.
Constraints: no text, no letters, no question mark, no lock, no UI controls, no logo, no watermark, no glowing effects, no extra islands. Original art, not an existing game's icon.
```

## 背景のみ修正したプロンプト

編集対象：初回に生成した布をかぶせた島。採用中の背景・既存島・主人公は編集していない。

```text
Use case: background-extraction. Edit target: the supplied isolated cloth-covered toy island. Change ONLY the backdrop: completely remove the grey checkerboard and the cloth-like ripples on that backdrop. Replace EVERY pixel outside the toy island with perfectly flat pure WHITE RGB(255,255,255), including the empty corners, with no cast shadow, pattern, grid, folds, gradient or noise. White is a technical color-key matte to remove in the game's existing sprite loader. Preserve the island, teal draped cloth, purple felt base, cream stitches, pink yarn loop, their colors, proportions, lighting and all object details exactly. Keep the full object inside the canvas with the same framing. No new objects, no text, no watermark. The backdrop must be plain white, NOT an illustration of transparency.
```
