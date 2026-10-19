# C08 再現環境

このrunは原因調査であり、修正版Buildはありません。Humanへ同じ試遊のやり直しを要求するものではありません。

## 容量を増やさない隔離方式

既存の `PlayerGroundingProof_20261019/Build` の非Development Unity Playerをホストとして再利用し、新しい `C08BridgeEndpointProof_20261020/Runtime` に独立したmanaged-code overlayを作りました。Engineと画像など144ファイルは内容を変更しないハードリンク、`Assembly-CSharp.dll` はリンクせず新規にCompileした別ファイルです。元Proofのファイルは書き換えていません。

新しいAssemblyはProductionの119本のRuntimeソースそのままと、このrunの [診断fixture](Diff/DiagnosticFixture.cs.txt) から作成しました。比較元のUnityProofにあるTutorialとMainStage Sceneソースも現在のProductionとハッシュ一致です。既存Playerデータを再利用しているため、**新しいUnity BuildPipeline Build成功、Editor Play成功とは報告しません**。実行はUnity Playerです。

撮影に使ったAssemblyは、容量99の初期診断が `70DA6497DFE08108A514FB9135E9D445B435CBFD025F90BF8E86394545EF72C3`、最後の容量50歩行動画が `F5AC2EBDF1EB60123D33978F11BF89357A1B8E740A65B6B4EB830B6E0685B255`。差は診断初期容量だけで、Productionソースに変更はありません。

## 自動診断と手動起動の分離

`-c08QA 1` を付けた実行だけが診断driver、固定撮影時計、撮影、終了を有効にします。動画はこの自動診断です。

静止測定と歩行動画の橋は既存の `EnsureAuthoredPlatform` APIで診断配置しています。T4の統合相当橋は長さ12と両端点を指定し、中央Hookを非表示にした復元状態であり、今回Fキーで統合を通した記録ではありません。実生成の別記録だけは既存の接続・Q生成メソッドを呼んでいます。診断ヘッダを表示し、QA中だけ区間案内を停止しています。カメラの追従コード、ズーム値、画面解像度はゲーム側のままです。

QA引数を付けずにローカルの隔離EXEを起動する経路もありますが、今回はOSキー反応まで確認していません。以下は再現位置を用意するための参考であり、「手動確認PASS」ではありません。Productionや既存の固定レビューランチャーは使いません。

```powershell
# <visualization-root> はこのチャットの既存ローカル資料ルート
& '<visualization-root>/C08BridgeEndpointProof_20261020/Runtime/PlayerGroundingReview.exe' `
  -screen-fullscreen 0 -screen-width 1280 -screen-height 720 `
  -c08Bridge T4_merged -c08Remaining 7
```

この経路は長さ12の統合相当橋と入口残量7/50を診断APIで用意するだけで、自動入力・撮影・自動終了を始めません。通常のA/D、Space、矢印、E/Q/F/R、Tab、Escを読むゲームコードはProductionと同じです。診断初期配置と通常プレイを混同しないでください。終了はウィンドウの閉じるボタンです。

修正候補はまだないため、修正版の再Buildや新しいレビュー用固定ランチャーは作っていません。既存Buildや各Proofはすべて保持しています。

## 無効な記録の扱い

初期の古いCompiler試行、未Compileの起動、非表示ウィンドウで黒くなった撮影はローカルに保持し、今回の確認成功には数えていません。また最初の幅計測はLineRendererのwidthCurveを反映していなかったため、今回の数値には使っていません。公開JSONは幅曲線を反映した4記録に限定しています。
