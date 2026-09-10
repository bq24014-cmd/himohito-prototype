# 工作の世界になじむトゲ（2026-09-10）

## 見える変化

ステージ上のトゲを、赤いフェルト面、黄土色の縫い目、蜂蜜色の木の縁を持つ三角形へ変更した。現在の布巻きフック・木製床・毛糸のプレイヤーと質感をそろえ、赤い危険色と上向きの三角形は維持する。

チュートリアルの3本、本編の2本が対象。根元に台座や飾りを追加せず、元と同じ範囲へ描く。本編の低く横長のトゲも、元の横幅・高さを維持する。看板上とZ説明画面の小さな模式図は今回変更しない。

## 実装と維持するもの

- `ToySpikeVisual.GetSharedSprite` だけで、新素材→旧素材→既存の図形の順に選択する。
- 既存の `LoadProcessedToySprite` で白背景を抜き、実際の絵の範囲へ切り詰める。縫い目は白抜きされない黄土色にした。縮小時は既存のアルファ加重mipmap処理を使い、明るい縁やちらつきを抑える。
- 子オブジェクト名、中央ピボット、元の縦横それぞれのサイズ合わせ、描画順を維持する。ルート位置・回転・大きさ、Collider2D、Trigger、接続解除・返却条件は変更しない。
- 接触時の0.4秒の根元を支点にした揺れ、停止中の静止、無効化時の姿勢復元、既存の4本の毛糸繊維を維持する。音・光・カメラ・移動物理・残量50/99は変更しない。
- 既存の保存シーンやセットアップ処理は書き換えず、再生開始または `HimoHito > Preview > Refresh Scene Visuals` で表示を更新する。
- 既存のチュートリアルには各トゲに2個のToySpikeVisualがある。今回は素材変更に限定し、既存コンポーネントは削除しない。

## 素材と出典

- 採用素材：`Assets/Resources/Art/HimoHitoCraftSpike-v1.png`
- 生成方法：imagegenスキルの組み込み `image_gen`。外部API・CLIは未使用。
- 生成出力（制作PC）：`%USERPROFILE%/.codex/generated_images/019fdfa3-5ee1-7911-8a24-63282af5bae4/exec-ba26ff34-2ba0-4f40-ad04-e583016003b8.png`
- 入力1（形状参考）：`Assets/Resources/Art/WoodenToySpike-v1.png`
- 入力2（素材・画風参考）：`Docs/Images/CraftHook-Details-v1.png`
- 均一な白背景で生成し、実行時に背景を除去する。元から透過されたPNGとしては扱わない。
- 旧素材は上書き・削除せず、欠落時の代替表示として保持する。

### 生成プロンプト

```text
Use case: style-transfer / precise-object-edit
Asset type: one production sprite for a Unity 2D handcrafted yarn platformer hazard.
Input image 1: the old red wooden triangular spike; geometry and front-facing silhouette reference. Replace its material/art, not its role.
Input image 2: the game's current honey-wood and braided cloth hook artwork; STYLE AND MATERIAL reference only. Do not include hooks, rings, mounts or knots in this asset.
Primary request: redesign the single upward-pointing triangular hazard as a beautiful miniature wooden toy triangle with a rich warm crimson-red felt inset and a narrow honey-amber wooden bevel around its perimeter. Tiny restrained ochre blanket stitches follow the inset edge. Clearly tactile short felt fibres, warm crafted wooden grain, subtle dimensional highlights and lower-right self shading consistent with input 2. It should belong in a premium handmade felt-and-yarn toy room, not look like a flat traffic sign.
Composition: one centered symmetrical upright triangle, orthographic straight-on camera, horizontal flat bottom, distinct pointed tip, roughly equilateral outline matching image 1; whole triangle occupies about 78 percent of the square canvas, all sides visible with even clear margin.
Danger readability: saturated warm brick-crimson/red remains the large majority of the triangle face; sharp triangular silhouette immediately legible at 30 pixels high. Firm thin toy slab, not a puffy pillow or rounded cone. Keep border thin, with no base wider than the triangle, no pedestal, no cloth tails or loose decoration outside its outline. The tip should remain visibly pointed.
Background: perfectly uniform pure white #FFFFFF, no shadow outside the object, no checkerboard, no gradient, no floor or scenery. White will be removed by the game's existing white-key sprite importer. Use ochre-gold stitches (not near-white) so stitches will survive that key.
Lighting: gentle warm upper-left light, controlled textured highlights, quiet dark red edge contrast, no glow or sparkle.
Constraints: single isolated triangle only, no letters, no symbols, no exclamation mark, no extra spikes, no frame, no watermark, no game screenshot. Do not copy any hook or ring from image 2. Preserve the old triangle's upright full-width bottom silhouette and proportions; change materials and craftsmanship only.
```

## 確認方法

1. Unityの画像インポート・コンパイルが終わってから再生する。
2. チュートリアル第2区間の谷にある3本、または本編第2区間の大小のトゲを見る。
3. 編集画面で確認したい場合は `HimoHito > Preview > Refresh Scene Visuals` を実行する。ステージ再生成は不要。

## 検証

隔離したUnity 6000.3.21f1で旧素材の基準438項目、新素材の編集時528項目が成功。両シーンの全70個のCollider2D（チュートリアル17、本編53）と5本のトゲの構成・座標・回転・スケール・判定形状・boundsを比較し、変更がないことを確認した。

実際のEditor更新メニューを繰り返し呼んでも絵の子オブジェクトやコンポーネントが増えないこと、新素材のロードと透過・mipmap・描画順、従来の接触揺れ・停止・復元も確認した。本番シーン、プレイヤー設定・記録、既存の未コミット変更は保持する。

実Playモードでも374項目が成功。接続状態を用意して実際のTrigger処理を呼び、ヒモ解除、残量保持、線速度維持、解除後の接触継続で揺れを再開しないこと、実時間のLateUpdateで0.4秒後に元の姿勢へ戻ることを確認した。元のRigidbody2Dは回転固定なので角速度は前後とも0。検証の初回は非ゼロ角速度を要求したテスト側の前提を修正し、制約や制作コードは変更しなかった。

ログ：`.codex_tmp/CraftUiCheck/CraftSpike-Baseline-1.log`、`CraftSpike-Edit-1.log`、`CraftSpike-Play-3.log`。手動通しプレイ、配布ビルド、commit・pushは行っていない。

- [チュートリアルの実描画](Images/CraftSpike-Tutorial-v1.png)
- [本編の実描画](Images/CraftSpike-MainStage-v1.png)
- [拡大比較：上が旧素材、下が新素材](Images/CraftSpike-Details-v1.png)

画像は隔離コピーの実際のカメラ描画。全体画像ではカメラを第2区間へ横移動し、元の表示倍率を保った。拡大比較のみ詳細を見せるためズームしており、ゲームのカメラは変更しない。
