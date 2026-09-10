# 看板・HUD・操作ボタンの素材統一

## 変更範囲

背景・木床・フックに続き、チュートリアル4区間の看板と、チュートリアル／本編共通のHUD・木製操作ボタン・ゲージ枠をそろえる。看板は蜂蜜色の木目、HUDは濃紺の縫い目付きフェルト、ボタンと枠は既存のフック木台と同じ木材を使う。

看板の説明図・小さなプレイヤー・毛糸・アニメーション・Z操作、フォント、HUDの数値・消費予告・残量位置の結び目を維持する。プレイヤー物理、フックと床の配置、橋の計算、残量99／50、音、カメラは変更しない。操作説明・ポーズ・失敗・クリア・ステージ選択の共通WoodButton呼び出しにも同じ木材を適用する。各画面の背景画像は変更しない。

この木材・UI素材導入後、看板内の小さなプレイヤーとZ説明画面のプレイヤーもゲーム本体のCraft立ち絵に統一した。図の配置やアニメーションは維持し、変更と検証は [CRAFT_GUIDE_PLAYER_ART.md](CRAFT_GUIDE_PLAYER_ART.md) に記録する。

## 保存素材と生成方法

- `Assets/Resources/Art/HimoHitoCraftGuideSign-v1.png`：1254×1254。既存看板の板・支柱・ピン・ピンクの結び目を維持し、木材だけを編集した専用素材。
- `Assets/Resources/Art/HimoHitoCraftHudPanel-v1.png`：1774×887。縫い目付きの紺色フェルトの下地。
- `Assets/Resources/Art/HimoHitoCraftHookMount-v1.png`：既存の2172×724木台をUIにも再利用。PNGとワールド用アトラスSpriteは変更しない。

imagegenスキルのbuilt-in `image_gen` モードを使用。看板は既存看板と新木台を参照した編集、HUDは新規生成。生成結果をPNGのまま保存し、画像ファイルへの後加工はしない。旧素材を保持し、新素材が欠けた場合は旧描画へ戻す。

既存のUnityローダーで白を透明にし、alpha加重mipmapとTrilinearを使用する。HUDの元画像には実際のalphaが付いている。看板はローダーの切り詰めたSprite領域ではなく、処理後Textureの全正方形をSpriteに使う。説明図の正規化座標と看板を見る仕草の支点を変えないためである。

HUD・ボタンは9分割描画で角を固定し、幅に応じて中央だけを伸ばす。HUD内部には文字用の余白を設ける。ゲージの枠は従来の30px領域と内側12pxの残量トラックを維持し、上下の木材と端の結び目だけを描く。

## 確認方法

Unityの再コンパイル後、チュートリアルと本編を再生する。Sceneビューの看板は `HimoHito > Preview > Refresh Scene Visuals` から更新できる。チュートリアルでは4枚の看板の動く図とZで開く説明を、両ステージでは左上のゲージとTab／Esc等の画面にある木製ボタンを確認する。

## 検証結果

- Unity 6.3の隔離プロジェクトで実スクリプトをコンパイルし、77確認成功・0失敗。ログは `.codex_tmp/CraftUiCheck/CraftUiCheck-final.log`。
- 4枚の看板の専用素材・元の幅2.30・正方形キャンバス・角の透過・mipmap・小さなプレイヤー、説明アニメーションの進行と停止、挨拶後の元位置復帰、重複防止、既存コライダー位置・残量99の保持を確認。
- 実HUDコンポーネントの99／99・50／50・8／99と共通ボタンを隔離EditorWindowのRenderTextureへ描画。長い文字、残量の色と結び目、狭いウィンドウ、下端の余白を目視確認。初回は下端が縫い目に近かったため内側余白を調整した。
- `Docs/Images/CraftUi-Tutorial-v1.png` はコピーシーンのCamera描画と実HUD、`CraftUi-Signs-v1.png` は実看板4枚の拡大比較、`CraftUi-HudButtons-v1.png` は実HUDと共通ボタンを並べた検証用比較、`CraftUi-LowResource-v1.png` は残量8の描画。Editor由来の上端余白を含む。比較画像のボタン配置はゲーム画面の新レイアウトではない。
- 状態と時間を制御した検証で、手操作の通しプレイではない。元シーンの保存、配布ビルド、コミット、プッシュは行わない。

## 2026-09-10：HUD最下段のはみ出し修正

実プレイのスクリーンショットで「Tab 操作説明」が布枠の下へはみ出していたため、両ステージ共通の高さを174から206へ変更。内余白は左右14、上12、下14とし、文字の大きさ・30pxゲージ・表示内容は維持した。共通の `CompactHudHeight` を使い、チュートリアルと本編に同じ修正を適用する。

隔離Unity 6000.3.21f1でコンパイル成功、88項目成功・0失敗（`.codex_tmp/CraftUiCheck/HudFitCheck.log`）。Editor用スキンではなく実ゲームの `EditorSkin.Game` と指定フォントを使い、実HUDの99／99・50／50・8／99・1／50を描画した。各ラベルのGUILayout実座標から、上下左右の内余白・行の重複なし・ゲージ高さ30・容量99／50の維持を確認。最下段の下端はチュートリアル183、本編180で、206の枠内に23／26の余裕がある。[通常残量の描画](Images/Hud-Fit-Full-v1.png)、[少ない残量の描画](Images/Hud-Fit-Low-v1.png)。比較用ウィンドウでの確認であり、手動通しプレイや配布ビルドは実施しない。

## 生成プロンプト（built-inモード）

### 看板（既存のTutorialGuideSign-v1とCraftHookMount-v1を参照）

```text
Use case: style-transfer
Asset type: existing square 2D tutorial sign sprite for a handmade yarn platformer.
Input image 1 is the EDIT TARGET: a square image containing three horizontal wooden planks, a blue pin, a centered wooden post, and pink rope tied around the post. Input image 2 is a WOOD MATERIAL STYLE REFERENCE only, not a replacement object.
Change only the wooden material of image 1 to the honey maple / hand-sanded warm toy wood of image 2: tactile natural grain, restrained matte highlights, soft rounded edges, handcrafted stop-motion feel, less glossy orange. Keep the same three planks and their gaps. Broad central board face must remain clear for a moving game diagram.
Critical invariants: preserve the exact square canvas, object silhouette, total occupied size, post width, bottom position, plank boundary positions, the blue pin position, and the exact pink rope knot/tail shape and location. Board spans approximately x=11%-89%, y=7.5%-54.5% of canvas; post to y=93.5%. No crop or reframing. Do not add an illustration, text, buttons, symbols, nails, decorations, border, or additional rope.
Keep lighting gently from upper left. Replace the entire empty/background area with perfectly uniform pure white #FFFFFF outside the object so the existing game loader can key it to transparency. NO checkerboard pattern, gray haze, ground shadow or backdrop. The wood and yarn remain fully colored. One sign only.
```

### HUD下地

```text
Use case: stylized-concept
Asset type: single blank 2D game HUD backing panel, wide landscape 2:1 rectangle, for a handmade yarn and wooden toy platformer.
Primary request: a neatly cut rectangular patch of deep ink-navy felt with gently rounded corners, subtle dense wool fibers, softly padded thickness and a fine muted warm-cream running stitch around the inner perimeter. Small restrained handmade imperfections, premium tactile stop-motion craft style. Clean flat central 80% for readable mint, yellow and cream game text.
Composition: orthographic straight-on front view, centered isolated panel about 90% canvas width and 85% height. Even border and corner radius. No perspective. Fine fiber detail should be calm, not noisy. Broad center evenly lit very dark navy #1D2035 with a gentle fabric texture, NOT black.
Lighting: soft warm top-left, subdued rim highlight and slim bottom edge shadow entirely within silhouette.
Background: perfectly uniform pure white #FFFFFF outside the panel for game-engine transparency keying. No cast shadow on background, checkerboard or gray backdrop.
Constraints: one fabric rectangle only, no wooden frame, knots, buttons, pins, rope, icons, symbols, text, diagrams, controls, lettering or objects. No glowing border, shine or ornament.
```
