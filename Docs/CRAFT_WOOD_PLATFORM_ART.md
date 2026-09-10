# 木製足場の質感統一

## 方針

新しい布・毛糸の子供部屋に合わせ、面積が大きい木床を最初の改修対象とする。背景の棚より明るい蜂蜜色の木、手で磨いた木目、接地できる上面の明確さを共通の基準にする。縦長の床に横長素材を引き伸ばさず、ワールド空間で木目の密度を保つ。

今回の対象はチュートリアルと本編の木製足場・木の梁・戻り足場。青いレール、通常フックと足場用フックの色分け、毛糸橋、看板、UI、宝箱、主人公は変更しない。床の位置・幅・高さ・当たり判定、残量99／50、物理、音、カメラ、チェックポイントは維持する。

## 段階的なデザイン統一

1. **背景（前回完了）**：フェルトの壁、刺繍、毛糸と木製玩具の遠景／中景。
2. **木製足場（今回）**：広い面の木材、木目密度、上面と陰影をそろえる。
3. **接続物（後続作業で対応）**：青／緑の機能色と穴の見やすさを保ち、リング・木製台の質感を合わせる。詳細は [CRAFT_HOOK_ART.md](CRAFT_HOOK_ART.md)。
4. **看板（後続作業で対応）**：動く説明図は保ち、板・支柱・縁の素材を合わせる。詳細は [CRAFT_SIGN_UI_ART.md](CRAFT_SIGN_UI_ART.md)。
5. **HUD・操作部（後続作業で対応）**：数字・操作の読みやすさを保ち、布ラベルと木材の表現を合わせる。同資料に実装と素材を記録。

木床の改修時点では3以降は未変更。その後、フック、看板、HUD・操作部を更新した。各段階でゲーム内の大きさ・色・見分けやすさを確認してから次に進める。

## 採用素材

- `Assets/Resources/Art/HimoHitoCraftWoodBlocks-v1.png`：1254 × 1254、大きな木製積み木の面。
- `Assets/Resources/Art/HimoHitoCraftWoodGrain-v1.png`：2172 × 724、天板・薄棚用の連続した横木目。

imagegenスキルのbuilt-in `image_gen` で新規制作。どちらも背景除去の必要がない不透明な材料テクスチャ。旧素材は残し、新しいバージョン名で追加する。Mirror wrapとmipmapを使い、縮小時の細かなちらつきを抑える。PNGの後加工は行わない。

## 描画の構成

- `CraftWoodPlatformVisual` が床ルートの既存BoxCollider2Dの長方形を読み、子の `Craft Wood Body` に表示専用のMeshを作る。ルートの位置・サイズ・当たり判定へは書き込まない。
- 大きい床は4×4ワールド単位で積み木模様を繰り返す。高さ1.2未満の薄棚は横木目だけに切り替え、どちらも上面を基準にUVを合わせる。上面は同じ木目を使い、既存の陰影とは別サブメッシュにする。
- 本体は元の描画順+1、上面は+2、接地影は従来の+3。`WoodenPlatformDepthVisual` は残し、既存の着地による最大0.055／0.28秒の上面の沈みを維持する。
- 帰り道の上昇／フェードが操作する子TransformとMaterialPropertyBlockを上書きしない。表示の更新を何度呼んでも子を増やさず、Mesh／Materialは所有コンポーネントの無効化・破棄時に解放する。
- 旧画像は削除せず、新描画の成功後に隠す。素材欠損時や表示部品だけを無効化した場合は元の表示へ戻す。床全体が非アクティブなら隠したままにする。

## 確認方法

Unityを再生するとチュートリアル・本編の木床に共通適用される。編集画面は `HimoHito > Preview > Refresh Scene Visuals` で更新できる。看板・フック・UIの見た目は今回変更していない。

## 検証結果

- Unity 6000.3.21f1の実アセンブリでEditor用・Player用コードのコンパイル成功。配布用ビルドは作成していない。
- 隔離したUnityのコピーシーンで285項目成功・0失敗（`Temp/CraftWoodPlatformCheck.log`）。チュートリアル6床、本編の大床・薄棚・梁・細い柱、戻り足場4段で素材・UV・描画順・冪等性を確認した。
- 既存の接地影、着地による上面の沈み、戻り足場の途中中断と3回の再表示、第5区間のレール復帰、表示部品の無効化／削除と復帰を確認。床の位置・サイズ・コライダー・プレイヤーの物理・残量99／50・カメラ・背景の状態が検査前後で一致した。
- `Docs/Images/CraftWood-Tutorial-v1.png` と `CraftWood-MainStage-v1.png` は、実ステージのセットアップをコピーシーンに適用したCamera描画。HUDなしの見た目確認用で、手操作の通しプレイではない。戻り足場・薄棚・細柱も含めた5枚を目視確認した。元シーンは保存していない。

## 生成プロンプト（built-inモード）

### 木製積み木の面

```text
Use case: stylized-concept
Asset type: seamless tileable 2D game texture, opaque square full-bleed, timber block faces for a yarn-character platformer set in a handcrafted child's bedroom.
Primary request: the front surface of a wall assembled from large rectangular wooden toy building blocks. Tactile premium stop-motion craft quality, warm honey maple and softly aged amber beech wood, smooth fine matte woodgrain with a few natural curved grain lines, gently worn bevels. Blocks form a restrained running-bond arrangement, TWO broad horizontal rows across the square and roughly two large blocks across, not tiny bricks. Each block has visible lengthwise wood fibers, broad calming surfaces and very subtle variation in warm color. Thin dark joinery lines and tiny round bevel highlights only.
Composition: orthographic completely straight-on front texture, infinitely continuous material extending all the way to every edge. Seamlessly tileable on ALL FOUR sides, no outside frame, border, vignette, perspective, visible top surfaces, background or margins. Blocks at edges continue beyond crop so repeating makes an uninterrupted wooden toy structure.
Lighting: diffuse soft neutral-warm light, retain natural wood texture. Medium amber wood brighter than a dark indigo felt room but NOT oversaturated orange, yellow plastic, highly polished or shiny. No global directional gradient or shadow across the tile.
Constraints: no text, letters, logos, screws, nails, metal, cracks, spikes, ropes, plants, characters, symbols, checkerboard, alpha or blank area. Only wood material. This is a repeatable texture, NOT a finished island or perspective cube render.
```

### 天板・薄棚の木目

```text
Use case: stylized-concept
Asset type: opaque seamless wooden material texture for a 2D yarn-game platform top and narrow wooden shelves, wide landscape 3:1.
Primary request: one continuous close-up surface of warm honey maple toy wood with elegant long horizontal wood grain. Hand-sanded matte finish with delicate tactile fibers, subtle undulating grain streaks, very small natural color variation, premium handmade stop-motion miniature quality. The color should match warm honey/amber wooden toy blocks in a cozy nighttime child's room; saturated enough to read as playable foreground against deep indigo felt, but no glossy orange plastic.
Composition: completely straight-on FLAT MATERIAL covering every pixel edge to edge. Long grain runs from left to right throughout. Seamless horizontal edges with uniform exposure across the entire canvas.
Constraints: ONLY the wood surface, not a perspective plank or a shelf object. No outer contour, bevel, frame, cast shadow, border, margin, background, seams between boards, joints, knots larger than a few pixels, cracks, screws, nails, paint, text, logos, objects or transparency. No highlights baked to the top or bottom of canvas. This will be mapped onto geometry with shading handled separately.
```
