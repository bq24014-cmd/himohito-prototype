# 手動操作用・腕改訂Proof

Windowsの保存先：

```text
%USERPROFILE%\.codex\visualizations\2026\08\08\019fdfa3-5ee1-7911-8a24-63282af5bae4\PlayerIntegratedArmsProof_20261017\
```

1. 修正版は `START_ArmsRevision.cmd` をダブルクリック。
2. 前回の線状の腕は `COMPARE_PreviousArms.cmd` をダブルクリック。
3. 一方をAlt+F4／ウィンドウ右上の×で終了してから、もう一方を比較する。

exeは `Build_r3\IntegratedArmsProof.exe`。**このexeを直接ダブルクリックしても修正版が手動で起動する。** `_Data`、MonoBleedingEdge、UnityPlayer.dllなど隣のファイルを分離しない。

どちらも同じ最終r3、1280×720、A2のB地形。通常時間、既存音、入口は残量24／選択長6／橋0。比較用は起動引数 `-presentation candidate3`、修正版は `candidate4`（省略時もcandidate4）。ランチャーに自動撮影フラグは含めない。

A/D移動・振り子加速、矢印照準、E接続／解除、W/S長さ、Spaceジャンプ、Q橋、F統合、R復帰、Tab説明、Escポーズ。既存キーを変更していない。Bの出口後もRは入口へ戻る既存仕様。

両表示の起動と35秒以上の無操作生存、自動driver不在は確認。OS入力応答と音の実聴は未確認。自動比較撮影は `-gestureCapture 1` を明示する別経路で、通常ランチャーでは起動しない。[観測結果](QA/ManualStartup.json)。Build本体はGitへ追加しない。
