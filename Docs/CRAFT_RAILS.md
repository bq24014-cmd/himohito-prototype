# 青い布巻きのレール床（2026-09-10）

## 変更する見た目

旧プラスチック風の青いレールを、青い布巻きの上下2本の棒、黄土色の縫い目、蜂蜜色の木製留め具へ変更する。普通の木製床と区別できる青色と、水平な歩行面を維持する。

実際の配置対象は本編第5区間の2枚：

| オブジェクト | 中心 | 大きさ |
|---|---|---|
| Main S05 Left Shelf | (93.6, -0.95) | 3 × 0.6 |
| Main S05 Right Shelf | (101, -0.95) | 3 × 0.6 |

現在のTutorial保存シーンにはレール床や旧 `Landing 1` は存在しない。チュートリアル側の既存の互換描画処理にも対応するが、床は追加・復活させない。懐中電灯台座など、レールでないオブジェクトの見た目は変更しない。

## 長さを変えても引き伸ばさない

`CraftRailPlatformVisual` は表示専用のコンポーネント。既存の `Blue Railway Platform Visual` 子を利用し、青い棒は左右の端を固定して中央をタイル表示する。Collider2Dの寸法・offsetと親の拡大率を読み取り、レール全体の横幅と接地する上端を合わせる。

木の留め具は別Spriteにし、一定間隔で完全な形を中央揃えに配置する。留め具までタイル画像へ焼き込むと、半端な長さの端で木が途中から切れるため分離した。短い床は留め具を減らし、長い床は増やす。布の編み目や木目を横へ引き伸ばさない。

基準の高さ0.6に対し、青い棒の描画範囲は高さ0.54で床上端に合わせ、木の留め具は高さ0.34で床下端に合わせる。高さが変わる場合も各パーツを縦横同率で拡大縮小する。留め具の基準間隔は1.0。実際の設定はコードの定数を参照する。

## 安全な反映範囲

- 本編の通常更新と第5区間の再挑戦時の復元から、同じ描画処理を呼ぶ。
- レール用コンポーネントがある対象だけ新素材へ切り替え、汎用的な旧画像の利用先には適用しない。
- 両素材パーツが利用できる場合だけ新デザインを表示し、欠落時は旧レール画像へ戻す。
- 子の描画だけを更新する。床のルート位置、回転、大きさ、Collider2D、PlatformEffector2D、OneWayRailPlatform、SwingPassThroughRailPlatformの設定は変更しない。
- 下からのすり抜け、非接続時の着地、振り子中の通過、橋作成時の支持、リスタートの動作は従来どおり。
- 音、光、カメラ、入力、移動物理、ヒモ残量50/99を変更しない。保存シーンの再生成・再保存もしない。

## 素材

- 採用：`Assets/Resources/Art/HimoHitoCraftRail-v1.png`（2パーツ）
- 生成：imagegenスキルの組み込み `image_gen`。外部API・CLIは未使用。
- 採用出力（制作PC）：`%USERPROFILE%/.codex/generated_images/019fdfa3-5ee1-7911-8a24-63282af5bae4/exec-6d9db446-10b0-4dd5-b291-d21a4e5f6a42.png`
- 上側56%の領域から青い棒2本、下側44%から木の留め具を切り出す。各領域で実際の絵の範囲を調べ、2ピクセルの余白を付ける。
- 均一な白背景で生成し、既存の白抜き・アルファ加重mipmap処理を使う。元から透過されたPNGとしては扱わない。縫い目は白抜きで消えない黄土色。
- 旧 `TutorialRailPlatform-v1.png` は削除・上書きせず、代替表示とレール以外の既存用途へ保持する。

### 生成1：素材の変更

入力1：`Assets/Resources/Art/TutorialRailPlatform-v1.png`（構造参考）。
入力2：`Docs/Images/CraftHook-Details-v1.png`（素材・画風参考）。

出力：`exec-085fe783-5f5f-4bc8-8de6-62a534aceff7.png`。組み上がった全体の素材検討用で、ゲームからは参照しない。

```text
Use case: style-transfer
Asset type: production Unity 2D side-view platform sprite; one long toy rail.
Input image 1: old blue plastic rail, geometry/structural reference only.
Input image 2: current game blue cloth ring and honey-wood mount, exact materials/style reference only. Do NOT include rings/hooks/pink knots.
Primary request: replace the old plastic look with a high-quality handcrafted miniature rail made from blue woven cloth wrapped around a firm upper horizontal beam and a thinner lower parallel beam, linked by three evenly spaced small honey-colored wooden vertical retaining blocks. Restrained ochre hand stitches on the cloth and tiny recessed wooden pegs. The blue rail remains immediately distinguishable from normal honey-wood platforms.
Geometry: orthographic front elevation, two straight horizontal parallel bars, thick upper bar makes a level walkable top; thin lower bar and three wooden tie blocks underneath, with background-visible gaps between them. Rounded left and right ends only; upper edge perfectly level between end caps. Complete outer silhouette width five times its height. No perspective foreshortening, no slope or droop. Small bevel/self shadow gives depth but the walking edge remains clear. Closely preserve image 1's basic structure while using materials from image 2.
Composition: one centered long rail across the middle of a landscape canvas, entire object and both ends visible, generous plain margin above/below. Three evenly spaced wooden ties sit away from the far ends. No other objects.
Technical use: this image will be divided into left end-cap, repeating center, and right end-cap. Keep the first/last 10 percent of the object length free of wooden ties, knots, seams or decorative accents; they are simple blue rounded caps. Across the middle 80 percent, thickness of the horizontal bars, blue hue and brightness remain uniform for clean horizontal tiling. Fine textile texture should repeat at the same scale. Avoid a large brightness gradient along its length.
Materials: soft blue woven/corded fabric with visible tactile threads, not shiny plastic or metal; warm honey-amber wood with fine grain and modest rounded corners matching reference2, ochre stitches (not white), premium handmade toy-room aesthetic. Firm rail not a hanging rope bridge or fluffy pillow.
Lighting: warm gentle top-left lighting with restrained self-shading, no glow, no shadow outside the object.
Background: perfectly uniform pure white #FFFFFF in all empty areas and gaps. No checkerboard, floor, scenery, gradient or cast shadow. White will be removed by the game's existing runtime white-key process.
Constraints: isolated rail only, no text, no symbols, no bolts made of metal, no logos, no watermark, no stage screenshot, no people, no dangling yarn, no hooks/rings. Preserve straight horizontal walkable top and blue visual identity.
```

### 生成2：比率の調整

出力：`exec-f0bf995b-7f54-4d2b-a875-2432d4c173fc.png`。前回より太い形へ調整した検討用画像。指定の比率を厳密に満たす画像とは扱わず、最終的なゲーム寸法は変更しない。

```text
Use case: precise-object-edit
Input image: the newly designed blue cloth-wrapped toy rail, EDIT TARGET.
Keep its blue yarn fabric, ochre stitches, warm wooden ties, front-facing orthographic view, flat level walking top, white background and three evenly spaced wooden retainers.
Change ONLY its overall aspect ratio: the previous rail is much too long and thin (about 10:1). Make the COMPLETE rail including the wooden ties exactly about FIVE TIMES as wide as it is high (5:1). This means the whole structure should be approximately twice as tall relative to its horizontal length as it is now. Re-render the yarn fibres and wooden grain naturally; do not merely distort or stretch the pixels.
Concrete framing on a landscape canvas: entire rail from about5% to95% of canvas width; its total height should equal18% of the CANVAS WIDTH, not18% of canvas height. For example an object width1500 pixels should have total height300 pixels. The upper blue beam is about45% of object height, lower beam26%, gap between them15%, and each vertical wooden tie bridges both and projects slightly below.
Preserve three evenly spaced wooden ties, no new ties. Left and right rounded caps have no ties within first/last10% of length. Keep bar thickness/color/shading uniform along the repeating middle80%. Closely match the existing handmade textile material at a finer natural density.
Perfectly uniform white#FFFFFF background and empty gaps, no checkerboard, no floor, no shadows outside silhouette, no scenery, no text, no glow, no hooks. Do not change palette or the two-bar structure. The required correction is a sturdier5:1 silhouette rather than10:1.
```

### 生成3：採用したパーツ分離

木の留め具が途中で切れることを防ぐため、同じ素材を青い棒と留め具へ分離した。

```text
Use case: precise-object-edit
Asset type: two-part production sprite atlas for a2D game, with separated parts that the engine will assemble.
Input image: edit target, the new blue cloth rail with three wooden retainers.
Keep the exact blue yarn-wrapped two-bar design, ochre stitches, honey wood appearance, front-facing orthographic view, flat upper walking edge, fine fibre scale and pure white empty background.
Required edit: SEPARATE the wooden retainers from the blue rail.
Top half of the canvas: show ONLY the two straight blue horizontal bars, same positions relative to each other and same rounded left/right ends. Erase all three wooden blocks completely from this upper rail; seamlessly reconstruct blue fibres on the lower bar where the blocks used to overlap. Keep the uninterrupted clean white gap between bars. Blue bar pair should span roughly90%canvas width, with its complete height around22%canvas height, centered at y=28% ofcanvas. Keep a generous white margin. This part must contain NO wood, pegs, rings, ties or connectors between bars.
Lower half: one isolated honey-wood retaining block, vertically oriented rounded rectangle, with one dark inset wooden peg at centre, identical to one of the original blocks. Centre at x=50%,y=76% ofcanvas. Keep its natural proportions and size approximately equal to the previous original block, about7%canvas width and17%canvas height. It is NOT attached to any blue yarn. No other wooden pieces.
Exactly two separated asset groups: the top blue rail pair, and the bottom single wooden retainer. Do not flatten the artwork into one assembledrail. Do not include duplicate rails or additional frames. Do not add labels.
Uniform white#FFFFFF background throughout, including between the upper two blue bars and around lower wood block. No floor, gradient, checkerboard, exterior castshadow, scenery, text, logo, watermark. Preserve materials and perspective; only detach wood into its separate asset area.
```

## 確認方法

Unityのインポート・コンパイル後に再生し、本編第5区間の短い青レール2枚で確認する。編集画面は `HimoHito > Preview > Refresh Scene Visuals` で更新できる。ステージ再生成は不要。

### 実装した表示

- [本編第5区間の全景](Images/CraftRail-MainStage-v1.png)
- [旧デザインと新デザインの比較（上：旧／下：新）](Images/CraftRail-Details-v1.png)
- [長さによる見え方（上から幅1.2・3・9）](Images/CraftRail-Lengths-v1.png)

いずれも隔離Unityで実際の描画処理から出力した画像。長さ比較だけは検証用の床を使い、本番ステージには追加していない。

## 検証

Unity 6000.3.21f1の隔離コピーでコンパイルし、旧状態の74項目と変更後の編集時182項目が成功した。幅1.2・3・9で端部の幅0.27と中央テクスチャの繰り返し幅約3.354が共通であり、留め具は1・3・9本、間隔1.0のまま描画される。実レール2枚、対象外の木製小物、繰り返し更新、旧画像への復帰、片方のSprite欠落からの再生成、描画子の再利用を確認した。

検証用の新規BoxCollider2DはUnityによる初期寸法の推定に依存しないようsize=1・offset=0を明示した。本番のCollider2Dや物理設定の修正ではない。

実Playモードも134項目成功。既存コンポーネントの支持判定、非接続時の衝突、通常フックでの振り子中の通過、橋用フックでの衝突維持、速度・回転制約の保持、実際のRハンドラによるチェックポイント・残量・レールの復元、1回のシーン再読み込みを確認した。実入力による通しプレイではなく、制御された状態から既存ハンドラを呼ぶ検証である。動くPlayerのCollider boundsが物理同期で更新される差は、固定床の不変条件と分離した。

元のTutorial/MainStageシーン、PlayerMover、RopeResource、OneWayRailPlatform、SwingPassThroughRailPlatform、MainStageSectionFiveSetup、MainStageRespawnOnFallのSHA256は着手前後で一致。手動通しプレイ、配布ビルド、commit／pushは今回の対象外。

今回の変更ファイルの空白チェックは成功。全体の`git diff --check`には既存のMainStage.unityの空の`m_Name:`行の末尾空白が残るが、シーンは変更・整形していない。
