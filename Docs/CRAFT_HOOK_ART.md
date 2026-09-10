# フックの質感統一

## 方針

新しい布・毛糸の背景、蜂蜜色の木製足場へ合わせ、通常フックは青い毛糸、足場用フックは緑の毛糸を巻いた手芸用の輪にする。台は手で磨いた木目のある小さな木材とし、既存のピンクの固定結び目と垂れたヒモを残す。

輪と台は別のSpriteのまま。青のworldDiameterと緑のlocal 1×1への正規化を維持し、照準輪郭と橋端の巻き付けが参照する輪のboundsを広げない。本編第9区間・チュートリアル第4区間の中央フックは従来の青表示を維持する。フック・コライダー・接続点・各Setup・物理・ヒモ残量99／50・カメラ・音は変更しない。看板・HUD・操作ボタンの素材は今回未変更。

## 保存素材

- `Assets/Resources/Art/HimoHitoCraftHookBlue-v1.png`：1254×1254、青い輪。
- `Assets/Resources/Art/HimoHitoCraftHookGreen-v1.png`：1254×1254、同じ形の緑の輪。
- `Assets/Resources/Art/HimoHitoCraftHookMount-v1.png`：2172×724、蜂蜜色の木台。

imagegenスキルのbuilt-in `image_gen` モードを使用。初回の青い輪は透明指定が不透明なチェック模様になったため不採用。同ツールで背景のみ純白へ編集し、採用した青から色だけを緑へ変更した。木台は専用素材として新規生成した。PNGは生成結果をそのままバージョン付きの名前でコピーし、ファイル自体の後加工はしていない。旧素材も保持する。

Unity側は既存の `TutorialFirstSectionVisuals.LoadProcessedToySprite` を利用し、白背景を透明にして余白を詰める。外側と中心の穴も透明になる。元画像を読み取るため、新素材のImport設定ではRead/Writeを有効にする。加工後のSpriteは既存ローダーのキャッシュを共有し、通常の毎フレーム処理で再加工しない。専用素材が不在なら以前の輪・台の表示へ戻す。`HimoHitoUiParts` の共通アトラス自体は変更しない。

## 小さな表示での品質

最初の実景では高解像度の毛糸模様が約30〜45pxへ縮小され、点状にざらついて見えた。この3素材に限り `LoadProcessedToySprite(path, true)` を使い、透明度で重みを付けた縮小画像（mipmap）とTrilinearを有効にする。透明な白のRGBを平均へ混ぜず、各段階で透明境界のRGBを補完して白縁を抑える。非2乗サイズも面積で重みを付けて端まで含め、1pxまで生成する。

既定引数はfalseのまま。看板・プレイヤー・宝箱等の既存素材は従来の透過とBilinearを維持し、キャッシュもmipmap有無で分離する。元画像・Spriteの領域・輪の寸法・可視部分の元色・透明度は変更しない。

## 確認方法

チュートリアル・本編を再生する。編集画面では `HimoHito > Preview > Refresh Scene Visuals` で更新する。接続／解除、橋の生成、Fでの中央フック解除は従来どおり。接続時の0.42秒の動き、ピンクの結び目・橋端の巻き付けは別の既存演出を保持している。

## 検証結果

- Unity 6.3参照でEditor用・Player用コードのコンパイル成功。
- 隔離Unityのコピーシーンで649項目成功・0失敗（`Temp/CraftHookVisualCheck-final.log`）。青・緑全フックの専用素材使用、旧素材と同じ寸法・接続点・照準半径・巻き付け半径、中央Hookの青表示、木台の非表示／再表示と重複防止、接続時の動き、Q／Fの巻き付け描画、物理・資源・カメラ不変を確認。
- GPU読戻しで中心穴と四隅の透過、青／緑の区別、全mipmapと最終1pxの色を確認。新旧キャッシュの分離、元の可視色・alpha・Sprite領域も確認した。
- 初回はLineRendererの保守的なboundsを使う検査が失敗。実点と線幅より大きな範囲が返ることを記録し、実際のカメラでBakeMeshした線の丸端・角を含む全頂点で穴との位置関係を確認した。ゲーム側の台の配置は変更していない。
- 実セットアップ後のCamera画像を1920×1080・1280×720で両ステージ分、青緑拡大図を1920×960で取得し、5枚を目視確認。共有画像は `Docs/Images/CraftHook-Tutorial-v1.png`、`CraftHook-MainStage-v1.png`、`CraftHook-Details-v1.png`。
- 状態と時間を制御した描画検証であり、手操作の通しプレイではない。HUDなしのCameraプレビューを使用。元シーンの保存、配布ビルド、コミット、プッシュは行わない。

## 生成プロンプト（built-inモード）

### 青い輪：初回（チェック背景のため不採用）

```text
Use case: stylized-concept
Asset type: isolated 2D game sprite, square canvas, circular blue rope attachment ring for a handmade yarn platformer.
Primary request: a small circular wooden craft hoop tightly and neatly wrapped in BLUE cotton yarn. Premium tactile stop-motion miniature, thick soft twisted blue fibers around a sturdy round hoop, restrained fine fuzz, slightly imperfect hand-wrapped texture. Medium azure blue with lighter blue fiber ridges and deep blue inner rim. Matte and soft, not glossy plastic or shiny metal.
Composition: straight-on orthographic front view, exactly circular outer silhouette and concentric circular empty hole. Outer diameter about 80% of canvas, inner hole diameter 60% of outer diameter. Centered. Ring ONLY, no mounting board, no attachments, no bow, loose strings, ornaments, handles, screws or clasp. It must be legible at 40 pixels across.
Background: genuinely transparent alpha outside the ring AND through its central hole. No checkerboard, grid, white disk in hole, backdrop, horizon, cast shadow or ground.
Lighting: soft neutral-warm light from upper left, gentle dimensional inner shading entirely on the blue material. Keep an even circular silhouette.
Constraints: one ring only, blue is its functional game color. No text, logo, UI, sparkles, glow or perspective tilt. No character.
```

### 青い輪：背景修正（採用）

```text
Use case: background-extraction
Input image: edit target, the blue yarn wrapped circular ring just created.
Change ONLY the checkerboard background, both outside the ring and THROUGH its central hole, to one completely uniform pure white #FFFFFF. Remove every checkerboard square. The object shape, size, placement, soft blue yarn fibers, shading, exact outer diameter and inner hole diameter must stay unchanged. No new shadow, grey haze, vignette, text or any extra objects. Save the blue ring centered on pure white so a game engine can key out that background. Do not put a solid disc inside the ring; the empty hole must be exactly the same pure white as the outside.
```

### 緑の輪：青から色のみ変更（採用）

```text
Use case: precise-object-edit
Input image: edit target, blue yarn-wrapped circular game hook on white.
Change ONLY the blue yarn color to clear jade / mint GREEN for the rope-platform anchor variant. Medium jade-green base, soft light mint fiber ridges, deep teal-green shadows. It must read unambiguously GREEN next to the original blue ring at tiny game scale; not turquoise blue, not yellow or neon.
Keep the identical circular silhouette, central hole size and position, thickness, fiber pattern, lighting, cotton texture, framing, padding and canvas size. Keep the pure white #FFFFFF background outside AND through the hole completely unchanged. Do not add anything. No shadows outside the ring.
```

### 木台（採用）

```text
Use case: stylized-concept
Asset type: single isolated small wooden mounting plaque sprite for a handmade yarn-platformer, landscape 3:1 canvas.
Primary request: a rounded rectangular piece of honey maple / amber beech toy wood, front-facing with softly worn rounded corners and shallow beveled edge. Rich tactile horizontal wood fibers, tiny natural grain curves and subtle color variation. Hand-sanded matte finish, premium stop-motion miniature craft rather than shiny orange plastic.
Composition: one board only, centered and filling about 88% of width with comfortable white margins, visible wood aspect ratio about 2.75:1. Completely straight-on orthographic view, no perspective skew. Gentle thickness along lower edge inside the object's silhouette. Keep a broad calm grain surface readable even when the board is only 65 pixels wide.
Lighting: soft neutral-warm upper left, restrained highlight along bevel and shadow on bottom edge. No shadow cast onto the background.
Background: perfectly uniform PURE WHITE #FFFFFF for game-engine cutout; no checkerboard or ground.
Constraints: NO nails, screw holes, pins, rope, knot, hook, ring, fabric, inscription, letters, text, label, symbol, border frame, UI or extra decoration. The game adds fasteners separately. One solid uninterrupted wood plate only.
```
