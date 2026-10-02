# Foreground refinement 実装メモ

前回のP1 Proofを基準とする見た目の限定調整。新しい背景の作り直しではない。

## 変更

1. **Nearの光・接地影**: built-in imagegenスキルの編集で、既存の汽車・毛糸玉・クッション・積み木・糸巻きを保持したまま、木材の強い独立ハイライトと局所影を調整。影が広いマットに見えた初回結果は、影だけを小さくする2回目編集を行った。1920×1080へ同比率contain正規化し、alphaを維持。元Nearは無変更で保持。
2. **Nearの追従率**: 0.78→0.88。室内Midは0.92、Farは0.99のまま。Midとの相対速度差を0.14→0.04に減らし、玩具が床上で大きく滑って見えることを抑える。Gameplay Cameraは変更しない。
3. **近い床の同期**: 既存Mid画像の下17%の床ピクセルだけをSprite Rectで再利用し、Nearと同じXへ配置。下部Floor stripはNearグループの補助面であり、室内全体や3層構造を置き換えない。上縁はshaderでalphaをfadeし、Midの奥側床につなぐ。
4. **床の明暗整理**: Floor stripのshaderだけで彩度0.94と微小なRGB補正。ぼかさず木目を維持。壁 / 窓 / 棚 / カーテンの画像・Tint・Z・追従率は変更しない。
5. **足場の投影接地影**: 第1区間の2つの既存床Collider.boundsを読み取るだけで、背景の床面へ薄い楕円影を描く。既存PlayerContactShadowと同様のcode-native alpha gradient。初回の物理下端基準では影が画面外になったため、2回目で既存画像の床帯下端から32%の投影位置へ配置。足場のSprite / Transform / Collider / Physicsは編集しない。影は足場の真の物理接触面を追加するものではない。

Floor stripはZ=3.1、order=-95。投影影はZ=3.05、order=-94。NearはZ=3、order=-90。いずれもGameplayより後ろ。

## 範囲・復帰

前回と同じTutorial第1区間 / Playing / 見渡し終了後だけ有効。メニュー、失敗、区間2以降、Scene終了で補助床・影も無効。MainStageにProofコンポーネントを作らない。

`--foreground-before` の起動引数で**前回P1**を表示する。`--tutorial2p5d-baseline` は前回と同じProduction背景への無効化引数として保持。操作キーの追加は行わない。

## 隔離

SourceテンプレートはこのQAのEvidence内に保存し、実行は `.codex_tmp/Tutorial2p5DForegroundRefine_20261001/RefineProject` 内だけ。ProductionのAssets / Scene / Scripts / Packages / ProjectSettingsは変更禁止。前回ProofのSource・QA・画像・Buildも保持。

BuildだけCompany / Productを隔離し、既存ゲームのPlayerPrefsに混ぜない。SceneのRuntime setupは既存コードのまま。

## 再現手順

- `Evidence/NormalizeAssets.js`: imagegen出力のcanvas正規化のみ。画像編集はimagegenで実施。
- `Evidence/Prepare.py`: 前回の保護ロジックをread-only再利用し、新しい隔離コピーを作成。
- Unity `6000.3.21f1` / `ProofEntry.Run` / `-proofEvidence <absolute path>`: Editorのcontrolled diagnostic。
- `Evidence/PackageEvidence.py`: 実Unity PPM→PNG、同状態比較、連番GIF化。AIでScreenshotを描き直さない。
- `ProofEntry.Build` / `-proofBuild <new absolute folder>`: 新規Windows64 Build。
- `Evidence/CollectFinal.py` / `Prepare.py verify`: メタデータ・変更一覧・保護照合。

Editor診断は入力キャッシュと既存public methodを使う。OS実キーボードやHuman承認と混同しない。撮影はWorld Camera.Renderであり、HUDのIMGUIは含まれない。
