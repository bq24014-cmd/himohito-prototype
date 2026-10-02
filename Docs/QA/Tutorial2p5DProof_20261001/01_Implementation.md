# 隔離Proof — 導入手順 / 実装メモ

## 隔離と再現

このProofはProductionへの採用ではない。現在のWorking Treeの `Assets` / `Packages` / `ProjectSettings` を専用コピーへ複製し、追加の背景コンポーネントだけを動かす。既存のMainStage Scene未Commit差分も含め、元プロジェクトには書き込まない。

基準Git HEAD: `702a4bc044b0eb35bedb552753ca8495599bc1ee`。Unity: `6000.3.21f1`。

`Evidence/Preparation.json` ではなく、このQAフォルダ直下の `Preparation.json` に実際のコピー先・画像SHA・BGM SHAを記録。既存BGMはコピー内で必要な外部依存を満たすために複製するだけで、原本やGit除外方針を変えない。

隔離Unityプロジェクト: `.codex_tmp/Tutorial2p5DProof_20261001/ProofProject/`。

1. `Evidence/Prepare.py` が保護記録を作り、隔離コピーを作成する。既存コピーの上書きは禁止。再試行は別名のコピーにする。
2. 同スクリプトが背景Source / 診断Source / Editor起動Sourceをコピー内だけへ配置。
3. `ProofEntry.Run` をUnityの `-executeMethod` で実行し、`-proofEvidence` に絶対パスを渡す。
4. Editor Play Modeへ移行後、Tutorialをロードして既存runtime setupを動かす。入力キャッシュへの診断注入で既存物理を検証する。OSの実キーボード操作・Human追試とは区別する。
5. `ProofEntry.Build` と新規の `-proofBuild` パスでWindows64 Buildを作る。既存出力には上書きしない。診断コードは `UNITY_EDITOR` 内のためPlayerに入らない。
6. `Evidence/PackageEvidence.py` でUnityのPPMキャプチャをPNG化し、比較・連番プレビューを整理。
7. `Evidence/Prepare.py verify` で開始時ファイルのSHA256を再照合する。

## 表示方式

追加ルート: `Tutorial Section 1 — 2.5D Proof ONLY`。SpriteのFull Rect平面を別々のZへ配置する。Gameplayは従来の2Dのまま。

|層|内容|Z|描画順|カメラX追従率|画面上相対変位|
|---|---|---:|---:|---:|---:|
|Far|窓外の夜空・月・雲・木|7|-130|0.99|カメラ変位 × -0.01|
|Mid|カーテン・窓枠・布壁・ガーランド・棚・本|5|-110|0.92|カメラ変位 × -0.08|
|Near|汽車・毛糸玉・クッション・積み木・糸巻き|3|-90|0.78|カメラ変位 × -0.22|

NearもGameplayより後ろに描く。既存Gameplayの描画順は変更しない。

Gameplayカメラは追加・編集しない。既存HorizontalCameraFollowの実際の位置をLateUpdateで読むだけ。背景平面にだけ相対位置と均一スケールを設定する。新たなRigidbody / Collider / Joint / 入力コンポーネントは追加しない。

入力画像は1920×1080、同一中央Pivot / 100 PPU / Full Rect / sRGB / alpha有効 / Bilinear / Mipmapなし / Clamp。試作では圧縮なしとして透過縁を優先する。

有限画像のため、横幅は `max(36, viewWidth+5, (viewHeight+0.8)*16/9)`。画面内を覆うoverscanを設け、相対移動は被覆余白にClampする。画像をミラー反転して繰り返さない。縦中心はカメラY+1.1として下側のおもちゃを見せる。この調整は背景のみで、カメラ・足場は動かさない。

## 既存背景との切替

- 元背景のGameObject / コンポーネント / 画像は削除しない。
- Tutorialの `Playing` かつ第1区間、ステージ見渡し・開始トランジションが終了した間だけProofを描く。
- 既存背景SpriteRendererの `forceRenderingOff` を一時的に切り替え、元の値を保管。
- 第2区間以降、選択画面、失敗、クリア、無効化、Scene退出では元の値に戻す。
- MainStageにはProofコンポーネントを作らない。
- `--tutorial2p5d-baseline` の起動引数でProofを無効にしてBefore状態を確認できる。ゲーム操作キーの仕様は変えない。

## 検証範囲と制限

同フレームでBefore→Afterを撮り、Rigidbody位置・速度・回転・simulated、全Collider、全Joint、カメラ位置・zoom、ロープ状態・選択長・区間が同じことを検査する。左右移動 / ジャンプ / 照準 / 接続は既存のメソッドと物理で実行する。

Screenshotは実Unityのworld camera描画。IMGUIのHUD・メニューは含まない。GIFは連番の低解像度プレビューで、動画の実FPS測定には使わない。Section 2切替は範囲ガードのcontrolled testであり、通常操作での第1区間クリアを主張するものではない。

本採用前には人間のキーボード操作、Section 1終端での切替、複数アスペクト比、通常BuildでのFPS / GPU時間、圧縮とメモリ、背景ルートのScene管理を別途確認する。Production採用 / commit / pushは今回行わない。
