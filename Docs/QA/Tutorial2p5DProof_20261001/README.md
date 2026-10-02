# Tutorial 第1区間 — Unity 2.5D動作Proof

作成: 2026-10-01。**隔離Unityコピーのみで仮導入。Production Source / Scene / Gameplayは変更なし。**

目的: 既存の夜の子供部屋の方向性を保ちながら、背景が「1枚絵」ではなく窓外・室内・手前に分かれて見えるか、実Unityで検証する。

最終判定は **P1 / 奥行きと視認性のHuman評価PASS**。Editor診断30項目PASSに加え、ユーザーから「奥行きが分かり、見やすい」と回答を得た。個別操作・他画面比率・区間境界のHuman確認は別途必要で、完成版へ本採用したものではない。

## 素材

無変更の入力素材: `Docs/VisualConcepts/Tutorial2p5D_20261001/` の `Tutorial2p5D_Far.png` / `Tutorial2p5D_Mid.png` / `Tutorial2p5D_Near.png`。元Docs素材は保持し、Resources用のコピーにだけUnity生成metaを追加。

- Far: 窓外の夜空・月・雲・木。
- Mid: 布壁・窓枠・カーテン・ガーランド・棚・本。
- Near: 汽車・毛糸玉・積み木・クッション・糸巻き。

Z=7/5/3、描画順=-130/-110/-90、カメラX追従率=0.99/0.92/0.78。右へCameraが移動すると、それぞれ小 / 中 / 大の量だけ画面上で左へ移動する。画像は有限なのでoverscanと端のClampを設けた。

背景用の追加カメラは使わず、既存Gameplayカメラを読むだけ。Gameplayは2Dのまま、3つの背景Sprite平面を3D座標の別Zへ置く。立体モデル・新しいCamera演出ではない。

元背景は削除せず、専用ルート / フラグで描画だけ切り替える。Tutorial第1区間のPlaying中だけ有効。第2区間以降・MainStageへは広げない。

## 比較・証拠

- `Screenshots/01_Standing_Comparison.png`: 同一状態のBefore / After。
- `Screenshots/03_Aim_Comparison.png`: 照準状態の比較。
- `Screenshots/04_Jump_Comparison.png`: ジャンプの比較。
- `Screenshots/05_Attached_Comparison.png`: Hook接続の比較。
- `Screenshots/ActualUnityMotion.gif`: 実Unity連番から作った動きのプレビュー。
- `Screenshots/Motion_000.png`〜`Motion_029.png`: 元の1600×900連番。
- `Evidence/RuntimeAttempt02/`: 最終診断のchecks / result / motion CSV / render timing CSV / 生PPM。
- `EvidenceManifest.json`: 実画像SHA・計測条件・CPUタイミング。
- `Preparation.json` / `ProtectionResult.json`: コピー条件・Production保護確認。

キャプチャは実Unity world camera描画であり、合成でGameplayを描き足していない。HUD / Stage SelectionのIMGUIはCamera.Renderキャプチャに含まれない。撮影のためにCamera位置・zoomは変更していない。

最初の `Evidence/Runtime/` は、診断が長さ6の届かない位置から狙い、さらに空中で接続を試したためFAILした検証手順の記録。背景・Gameplayを修正したのではなく、歩行終点を既存射程内へ移し、既存仕様どおり着地して接続するよう診断手順を直した。**最終結果はRuntimeAttempt02のみを採用する。**

## 評価

中央は暗い布壁の余白を維持し、ピンクの主人公とヒモ、青Hook、木製床を識別できる。毛糸 / フェルト / 木 / 絵本の世界観を維持し、独立した窓外と手前玩具の動きが奥行きを補う。

課題は有限画像端のClamp、区間境界の即時切替、圧縮なしTextureの追加メモリ、人間の操作・複数画面比率での未検証。詳しい判定とパフォーマンスの限定条件は [03_FinalDecision.md](03_FinalDecision.md)。

## 実装・Human確認

- [01_Implementation.md](01_Implementation.md): 導入・再現手順、Z / 追従率、範囲ガード。
- [02_HumanReview.md](02_HumanReview.md): 実キーボード確認項目と未確認事項。
- [05_HumanReviewResult.md](05_HumanReviewResult.md): ユーザー回答とP1判定。
- [04_BuildAndProtection.md](04_BuildAndProtection.md): Windows版・起動・保護結果。
- Proof起動: `output/Tutorial2p5DProof_20261001_Proof01/START_PROOF.bat`。
- Before比較: 同フォルダの `START_BASELINE.bat`。
- `Evidence/Tutorial2p5DBackground.cs`: Proof背景Sourceの保存用テンプレート。
- `Evidence/ProofRuntimeQA.cs` / `ProofEntry.cs`: 隔離診断・Build手順。

ProductionのREADME / Learning Logの既存未Commit変更も保護対象。元Scene・Scripts・Hook・足場・Collider・Physics・Rope/Bridge・Tutorial進行・看板・入力仕様・Section構成・Camera挙動を変更しない。

stage / commit / pushは行っていない。奥行き・視認性のHuman評価を記録し、ここで停止する。
