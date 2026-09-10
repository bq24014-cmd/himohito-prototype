# 帰り道スイッチの素材統一

## 方針と変更範囲

本編第3区間、下ルートの帰り道を出すスイッチを、背景・木床・フック・看板・宝箱に続いて手作りの質感へそろえる。ピンクのボタンは柔らかな編み地、台は蜂蜜色の手磨き木材。台に巻いたヒモと右側の結び目・房を維持する。

- 採用画像：`Assets/Resources/Art/HimoHitoCraftRecoverySwitch-v1.png`（1774×887）。PNGと専用GUIDのmetaを追加。
- 旧画像：`Assets/Resources/Art/RecoverySwitch-v1.png`。削除せず、新画像が存在しない場合のフォールバックにする。
- `RecoverySwitchVisual.Refresh` の素材選択だけを変更。既存の白背景除去を使い、新素材はalpha加重mipmapとTrilinearで小さな表示を整える。旧素材は従来のフィルタリングを維持する。
- imagegenスキルのbuilt-in `image_gen` モードで既存スイッチを編集し、`HimoHitoCraftHookMount-v1.png` を木材の参照に使用。生成PNGの後加工はせず、そのままコピーして採用した。

## 維持する動作

スイッチルートの位置(45.6, -5.75)・scale(0.9, 0.35)、Triggerのsize(3, 5)、Fキーと接近条件、足場の即時有効化は変更しない。絵の幅1.1・底Y=-6.21、0.24秒で高さ75%まで沈む補間、押した際の0.85倍の暗さ、停止中の演出停止を維持する。高さは従来どおり素材の縦横比を保持する。

足場A〜Dの出現演出、手前の床へ戻った際の非表示・スイッチ復元、主人公が押すアニメーションは既存のまま。プレイヤー・床・フック・コライダーの移動、物理・ヒモ残量・音の変更はない。元シーン保存、再生開始位置の変更、配布ビルド、コミット、プッシュは行わない。

## 確認方法

Unityで再コンパイル後、本編第3区間の下側のスイッチで確認できる。停止中は `HimoHito > Preview > Refresh Scene Visuals` でも更新できる。横に立ってFで押し、手前床へ戻ると元に戻る。

## 検証結果

- Unity 6.3の隔離コピーでコンパイル・59確認成功、0失敗。ログは `.codex_tmp/CraftUiCheck/CraftSwitchCheck.log`。検証用Editorコードからinternalな区間定義へ直接アクセスしてしまった初回のコンパイルエラーは、実際の床Colliderの右端を参照する形へ直した。本体のアクセス範囲は変更しない。
- 新画像・Trilinearとmipmap・外側の透過と中央の不透明、二重Refreshでも子が1つ、描画順、旧Rendererの非表示を確認。処理後の縦横比は旧0.5356／新0.5478で、高さの差は約0.0135ユニット。幅・底位置を固定し、新素材を縦横に歪めない。
- 未押下・0.12秒・0.24秒で幅1.1、底Y=-6.21、高さ100%／87.5%／75%、白→0.85の色補間、deltaTime=0で停止を確認。
- 実際のControllerのメソッドを呼び、範囲外・ポーズ中・無効プレイヤーを拒否、有効な接近から4足場を即時表示、重複要求、手前床へ帰った際の非表示・白色復帰、離れた後の拒否、参照復元を確認。表示→帰還を3回繰り返し、途中の足場演出でも床ルートとColliderが動かないことを検証した。
- スイッチルート・Trigger・低い床のbounds・プレイヤー位置と速度・ヒモ残量の保持を確認。元の入力／コントローラーコードは未変更。新素材不在時のフォールバックは分岐と旧素材ロードを確認しており、新素材を外す実験は行っていない。
- `Docs/Images/CraftSwitch-Press-v1.png` は通常→途中→押下の拡大描画、`CraftSwitch-Detail-v1.png` は接地、`CraftSwitch-MainStage-v1.png` は本編第3区間のCamera描画。3枚を目視確認し、白背景・白い輪郭・浮きがないことを確認した。
- 状態と時間を制御した検証であり、キーボードによる通しプレイではない。制作中のUnityへ入力せず、元シーンも保存していない。

## 生成プロンプト（built-in）

```text
Use case: style-transfer
Asset type: single 2D interactive floor button sprite for a handmade yarn platformer.
Input image 1: EDIT TARGET, the existing oval pink knitted floor switch on a circular wooden base, with pink cord and two tassels at front-right. Input image 2: supporting WOOD MATERIAL REFERENCE only.
Change only materials and rendering style: base becomes warm honey maple matching image 2, natural hand-sanded tactile wood grain, softly rounded edges, restrained matte highlights, handmade stop-motion miniature quality. The top remains the same vivid rose-pink knitted cushion with exactly the same dome height, but fibers are softly tactile wool, not glossy plastic. Keep the wrapped pink cord and right-hand knot/tassel geometry and locations. Replace thick dark cartoon outline by natural subtle edge shading inside the SAME silhouette.
CRITICAL invariants: exact existing 2:1 landscape canvas, camera viewpoint, centered object placement, silhouette and overall object width/height ratio, top-to-base height ratio, contact bottom edge, same relative size and position. Do not enlarge the dome or add a platform below. One switch only, unpressed state. No new details, icons, letters, glow, particles, props or cast shadows outside the silhouette.
Background: perfectly uniform pure white #FFFFFF everywhere outside the switch so its existing game loader can key white to transparency. No checkerboard, gray haze, ground plane or gradient. No cropping, perspective change, watermark or text.
```
