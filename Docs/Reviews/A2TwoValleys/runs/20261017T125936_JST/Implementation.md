# 隔離変更の範囲と理由

Productionではなく、元ProofからAssets／Packages／ProjectSettingsだけを新しい隔離領域へコピー。元のLibraryやBuildの複製はしない。元Proofは不変。

| ファイル | 変更理由 |
|---|---|
| `Assets/PlayerPresentation/IntegratedWoolArms.cs`＋meta（追加） | 前回のLineRendererでは胴より細く、別のヒモに見える。既存の人形の胴の編み目を使う連続メッシュ、固定長の肩・肘・手、丸い先端へ置換。物理コンポーネントなし |
| `ActiveSwingArms.cs` | 前回表示は比較分岐として保持。新案だけ上記メッシュを使用。入力と速度に応じた腕の構えを小さく整理し、R／表示終了で確実に非表示。胴の傾き計算は前回のまま |
| `PlayerMotionPresentation.cs` | 新案の既定選択、元腕の画素を抑制するshaderパラメータ、非表示spriteへの腕描画を除外。足裏補正式は不変 |
| `Assets/Resources/PlayerPresentationYarn.shader` | 新腕が表示される間、原画の外側の腕領域のalphaのみ抑える。RGB色調・頭の変形処理は変更しない。新腕も同じshaderの色調を使用 |
| `ManualPresentationSession.cs` | 読み取り観測の候補名をcandidate4既定へ変更。操作・自動撮影を加えない |
| `ConnectionGestureCapture.cs` | 新腕の可視数・固定骨長誤差の観測項目だけを追加。元の撮影・入力条件は保持 |

PNG／既存meta／Scene／Packages／ProjectSettingsと、HookYarnKnotVisual／RopeBodyVisual／PlayerSoleRegistrationはbyte不変。主要5コードも不変。Camera・地形・資源設定・入力キーは変更していない。[正確な差分](CodeDiff.json)／[保持確認](QA/SourceProtection.json)。

「速度から姿勢を選ぶ」：実速度・入力方向・最下点への向き・接続／接地を読み、加速・制動・惰性・頂点を分ける。新しい周期アニメ時計を加えない。

「見た目と物理を分ける」：腕のMeshRendererとshaderだけを変える。Rigidbody／Collider／Jointを腕に追加しない。PlayerのRoot・速度・力・入力・残量へ書き込まない。

肩は胴の内側、腕の編み目は既存 `Art/HimoHitoCraftPlayer-v1` の胴中央の画素を参照。新しい画像生成や原画への描き足しではない。原画の顔・頭を切り貼りせず、腕用のUVとメッシュをコードで構成する。

フック巻き付きと足裏補正は前回の良かった部分として維持するが、前回Proof全体や候補2が正式採用済みとは扱わない。
