# 変更理由を本人が説明するための短い要約

1. **速度から姿勢を選ぶ。** 時計で同じ動きを繰り返さず、実際の速度と、その少し前の速度との差を読む。速いと手足・糸端が遅れ、頂点では遅れが収まる。頭側を固定した描画メッシュだけを変形する。
2. **衝撃に応じて沈みを変える。** 最大落下速度を読む既存処理を利用し、膝曲げ絵と回復時間を変える。着地後に移動・ジャンプがあれば、その入力を止めずに通常動作へ渡す。
3. **見た目と物理を分ける。** 足先の不透明画素を絵のランドマークとして登録し、表示用の子だけを床上面へ合わせる。透明な余白を含む外枠は足先ではない。Root、Collider、Joint、速度、力は変えない。

## 質感

元の毛糸絵の編み目を保持し、ピンク部分だけ彩度・明暗差を整える。目の黒やハイライトを一律に暗くせず、上側と窓側のわずかな明暗を追加。全画面へ色フィルタを掛けていない。新PNGや全アニメーションの量産はなし。

## ファイル

- 既存表示：RopeBodyVisual.cs、RopeSwingPoseState.cs、RopeLandingFluff.cs、PlayerContactShadow.cs。
- 既存音：PrototypeAudioFeedback.cs（診断マーカーのみ）。
- 新しい表示：PlayerMotionPresentation.cs、PlayerSoleRegistration.cs、PlayerPresentationYarn.shader。
- 試験専用：PresentationCapture.cs、起動引数による現行/候補分岐。Productionへ混入させていない。
- [最終差分](CodeDiff.md)、[正確な差分JSON](CodeDiff.json)、[ファイルSHA](QA/ChangedFiles.json)、[禁止対象5 Sourceの同一性](QA/LockedSources.json)。

描画メッシュの分割はSprite OverrideGeometryを利用し、PhysicsShapeは変更しない。座標単位は[Unity 6.3公式API](https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Sprite.OverrideGeometry.html)のRectピクセル空間に従う。

## 制作中の訂正と保持

初期のMaterialPropertyBlock初期化とSprite Rect座標のエラーは修正し、失敗ログをローカル保持。非表示Windowの黒画像は無効として共有から除外。初回候補の外枠基準の足合わせは画像確認で不適切と分かり、不透明な足先登録へ修正した。最終Buildはr2で、候補の方向は1案のみ。以前のBuildや証拠は削除・上書きしていない。
