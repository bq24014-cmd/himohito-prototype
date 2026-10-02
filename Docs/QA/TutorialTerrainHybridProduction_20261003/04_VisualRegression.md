# Visual Regression

結果: **技術的Visual QA PASS**。承認済み5床の構成・crop・色・描画順を維持。新しい画像生成や模様の描き直しは行っていない。後続のProduction Build Human評価は **PASS / 採用承認**。原文を[07](07_HumanApprovalAndCheckpoint.md)に記録。

## 比較資料

- [全4区間Overview](Screenshots/Final/Overview_Comparison.png)
- [5床比較](Screenshots/Final/AllFiveTerrain_Comparison.png)
- [S1 Start](Screenshots/Final/Comparison_S1_Standing.png)
- [S2 Landing](Screenshots/Final/Comparison_S2_Standing.png)
- [S3 Bridge bank](Screenshots/Final/Comparison_S3_Standing.png)
- [S4 Merge bank](Screenshots/Final/Comparison_S4_Standing.png)
- [S4 Goal floor](Screenshots/Final/Comparison_S4_WideStanding.png)
- [Merge後](Screenshots/Final/Comparison_S4_Merged.png)
- [Clearing](Screenshots/Final/Comparison_S4_Clearing.png)

BEFOREは現行実行中の旧木Renderer、AFTERはProduction Hybrid。隔離QAが同一frame・同一Cameraで一時的にRendererのみ切り替え、直後に元のHybrid状態へ戻した。Productionは比較機能を持たない。

## 幾何と視認性

| 床 | 確認結果 |
| --- | --- |
| Start Ground | 工作箱の水平な蓋＋本の積層。Playerの足元と上面を明確に分離。旧木柱の反復感を減らした |
| Tutorial Landing | 最上段の本、箱、布帯の配置がStartと異なる。歩行幅、旗、看板を保持 |
| Tutorial T2 Landing | 大箱と厚い本の配分が他床と異なる。緑HookとBridge始点の前に装飾がはみ出さない |
| Tutorial T3 Landing | 濃紺の本と月印、厚紙箱・本で変化を維持。Rope/Bridgeと上面が読める |
| Tutorial T4 Goal Floor | 横長専用構成。箱・横積み本を単純Stretchせず配置。上面は連続し、箱の継ぎ目は穴／段差ではなく材質境界として読める |

四角いCollider輪郭はGameplay契約として維持する。足場にない張り出し、隙間、装飾による偽の乗り場は追加しない。quad四辺はColliderから構築し、walk lineはy=0.35。Runtimeチェックでは全5床の上面・外周・root/physicsが保存された。

主担当と独立レビュアーが実際のOverview・5床一覧・各床原寸画像・Merge・Clearingを確認。新たなfake ledge/fake hole、Player/Hook/Ropeの遮蔽、背景との色調不一致は見当たらなかった。木柱の四角い外形そのものは変えず、内部の本／箱の積層で地形感を出す承認方針を維持している。

## Pixel比較

`Evidence/ImageComparison_Runtime01.json`:

- 59 pairs、118枚の1920×1080 lossless PNG。比較シート等を含め出力PNG184枚。
- 欠落moments 0。
- 各frameの5床Collider投影矩形のunion（外周3pixel拡張）外で、59組すべて差分 **0 pixel**。
- 生PPM→PNGでRGB SHA256一致。画像の修整・消去・再描画は行わない。比較sheetのみ同条件で縮小・ラベル追加。
- 独立再確認で118PNGと根拠CSV2件のSHA256不一致0。

上記118枚／184枚は生成・検証した全成果物数。checkpointでは代表比較12枚を選別保存し、残りPNG・生PPMはローカルに保持する。全59pairのhash／pixel比較結果はmanifestに保存する。

この比較は「同期frameで描画変更が地形範囲に限定される」証拠。mask内部でのPlayer等の不可視化がないことや、プレイ中の読みやすさは別途Renderer状態検証・実画像評価・Human確認を併用する。pixel 0だけからGameplay全体や全Camera状態の完全同一を主張しない。

## 背景 / MainStage

Far / Mid / Near、Shader、PNG、parallax、Intro Fix、seam Fixは本体SHA一致。Intro2509サンプルでは開始確定後の新背景非表示0・旧背景二重表示0。S1〜4、Goal付近、Clearingの連続表示を確認。

MainStageはHybrid manager / Hybrid rendererなしをruntime確認。MainStage Smallを含む本編への展開なし。本体MainStage Sceneの既存未保存差分も開始時SHAのまま。

## 限界

world-camera画像はScreen Space Overlay UIを含まない。HUD・Clear UI・入力の感触・動作中の継ぎ目・フレーム落ちは、今回のWindowsレビュー版でHumanに確認を依頼する。定量FPS/GPU負荷測定とlive assembly reloadは今回実施していない。
