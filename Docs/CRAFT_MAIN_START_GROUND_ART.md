# 本編開始床のクラフト積層

2026-10-03にHumanが採用したMain Start Ground改善版。承認済みTutorialと同じ夜の子供部屋の素材感を使い、箱4個、本2冊、厚い折り布、右側へ垂れる藍色の布で構成する。

対象はMainStageの `Main Start Ground` 1床だけ。保存シーンを編集せず、Play時に `MainStartGroundVisual` が描画用の子だけを追加する。床の中心(-7.25,-9.85)、大きさ8×10、Collider、歩行上端(-4.85)、Rigidbody2D、操作、振り子、残量、カメラ、他区間、Tutorialを変更しない。

素材は `Assets/Resources/Art/MainStartGround/Start.png`。独立試作Revision03の最終PNGをそのまま採用し、画像生成時のmeta/GUIDを維持した。上蓋を画像側で平らに修正したため、上部だけを横へ広げるUV変形は使わない。透過欠けのない幅広い上端行を選び、長方形のUVと4頂点の描画メッシュを元の床矩形へ合わせる。

元の床描画はRendererの表示だけを抑える。素材欠落や床の形状不一致時は元の描画を残す。コンポーネント無効化・破棄時は元の表示を復元し、自分で追加した描画子・Mesh・Materialだけを解放する。

本番実装にはF7/F8比較、旧版PNG、Proof用UI、ランチャー、検証入力を含めない。プログラムによるPlay検証とHumanの実キー操作結果は区別する。
