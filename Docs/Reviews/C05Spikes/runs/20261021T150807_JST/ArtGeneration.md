# C05 素材と生成記録

built-in image_genを使用。CLI／APIキー方式は使用していない。旧素材を編集・上書きせず、新規の通常形と横長形を同じ候補の形状別素材として作成した。元生成PNGは変更せず、隔離先Assets/Resources/Artと手動PlayerのReviewAssetsへ配置した。

| 新素材 | 画像寸法 | meta GUID |
|---|---|---|
| [HimoHitoCraftSpike-v2.png](Assets/HimoHitoCraftSpike-v2.png) | 1254×1254 | da461fa982b34ae3ae6832a937ef143a |
| [HimoHitoCraftSpikeWide-v2.png](Assets/HimoHitoCraftSpikeWide-v2.png) | 1915×821 | 2abcb5ac1e8449af9456c26cbcd3af5d6 |

metaは各PNGに併置。readable、非圧縮、Clamp、元寸法を保持する既存形式に合わせた。最終SHA-256とPlayerで読んだSHA-256は[Comparison.json](QA/Comparison.json)にある。旧HimoHitoCraftSpike-v1／WoodenToySpike-v1は保持。

参照画像の役割は以下のとおり。すべて既存Productionの画像で、生成前に目視した。

1. Assets/Resources/Art/HimoHitoCraftSpike-v1.png：形状の参考だけ。太い光沢枠は模倣しない。
2. Assets/Resources/Art/HimoHitoCraftHookGreen-v1.png：毛糸・手作り質感の参考。
3. Assets/Resources/Art/TutorialTerrainHybrid/Landing1.png：採用済みの床・木・布の画風参考。

transparent_background=true。以下が両素材共通の生成指示で、その後に各形状の指示を付けた。

```text
Use case: stylized-concept. Asset type: transparent-background orthographic Unity 2D game hazard sprite. Input 1 is the current spike shape reference ONLY; input 2 is yarn-hook material/style reference; input 3 is the approved books/cardboard terrain material/style reference. Create a new, single simple upward-pointing triangular handmade wooden hazard with an adhered deep warm red woven felt front face. Keep a clearly sharp apex and flat horizontal base. A solid thin wooden core is visible as a restrained raw wood edge and a narrow shaded thickness on the right, NOT a raised glossy frame. This is a small physical children's-room craft object matching the grain, soft shadows and tactile cloth of the references, not a warning-sign icon. Red felt has visible broad natural fibre bundles, no tiny dense pattern. Muted warm wood, visible quiet grain, matte edge, soft upper-left light, warm dark lower-right shading. Keep the outer silhouette an exact simple triangle; no stand, no added base, no legs, no decorative outline, no gilding, no dark thick border, no repeated gold stitches, no extra objects, no symbols or text, no background scene, no cast shadow outside the silhouette. Transparent alpha background. Full object visible with modest transparent margin. Preserve red danger colour and upward triangle. Do NOT copy the old thick rounded glossy golden rim. Render ready-to-use raster sprite, no comparison layout.
```

通常形の追加指示：

```text
The triangle's actual object width:height should be approximately 1:1, centred with one apex at the top centre and two base corners on one horizontal line. Straight-on view without perspective skew.
```

横長形の追加指示：

```text
The triangle's actual object width:height MUST be approximately 7:3 (2.33:1): a deliberately broad low triangle, not a normal triangle stretched after rendering. Woven fibres and wood grain retain natural proportions and scale; absolutely no dense stitches. One apex centred at top and two far-spaced base corners on one horizontal line. Straight-on view without perspective skew.
```

生成画像の寸法比と実物部分の比は完全には一致しないため、最終表示は既存の幅・高さへ正規化している。横長の同画風専用素材を使い、旧通常素材を約2倍横へ引き延ばす方法にはしていない。極薄alphaノイズによる縮みは、元画像を加工せず新素材だけのSprite切り出し範囲で修正した。これは完成Artの正式採用ではない。
