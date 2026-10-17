# 手動専用Buildの起動方法

GitHubにはBuild一式を置かない。Windows側の新規 `PlayerSwingManualBuild_20261017` フォルダを使用する。過去の `PlayerSwingCandidate2_20261017/Build_r1`／`Build_r3` は自動撮影版なので、この手順では使わない。

```text
PlayerSwingManualBuild_20261017/
  START_Candidate2.cmd
  COMPARE_Candidate1.cmd
  README_手動操作.txt
  Build/
    PlayerSwingManual.exe
    PlayerSwingManual_Data/
    UnityPlayer.dll
    MonoBleedingEdge/
    ...（生成された付属ファイルをまとめて保持）
```

候補2のランチャーをダブルクリックする。比較するときは一度ゲームを終了し、候補1のランチャーを開く。ランチャーとBuildの相対配置は変えない。exeだけを別の場所へコピーしない。

候補2ランチャー：

```bat
@echo off
start "" /D "%~dp0Build" "%~dp0Build\PlayerSwingManual.exe" -a2Variant B -presentation candidate2 -screen-fullscreen 0 -screen-width 1280 -screen-height 720
```

候補1は同じ内容の `-presentation candidate1`。引数なしのexe直起動も手動・候補2が既定。`-presentationQA`、`-a2qa`、`-a2Smoke` 等の古い撮影用引数は不要。手動版のガードによりQA起動へ進まない。

## 操作と終了

A/D＝移動・加速、Space＝ジャンプ、W/S＝ヒモ長、矢印＝Hook照準、E＝接続／解除、Q＝橋、F＝既存の統合／文脈操作、R＝入口から再挑戦、Tab＝操作説明、Esc＝ポーズ。既存マウス補助も保持。

終了は右上の×か、ゲームを選択した状態でAlt+F4。新しい終了キー・入力モードはない。このOS終了操作自体も今回の画面操作としては未検証。

入口：残量24・選択長6・橋0。Bに出口CPはなく、出口後のRも入口へ戻る。起動時はこのA2 Bの入口へ直接入るため、Productionのステージ選択操作とは別の独立Proof。

## 記録

通常起動の読み取り記録はUnityのpersistentDataPath下の `ManualSessions/<candidate>_<JST日時>/`。個人情報を避けた表記では `%USERPROFILE%/AppData/LocalLow/HimoHitoIsolatedProofs/HimoHito A2 Two Valleys Proof/ManualSessions/`。これらの観測は入力・撮影・終了を実行しない。

実行確認はランチャーと同じexe／画角／候補条件を用いたプロセス起動で実施した。検証時だけ保存先を指定し、ウィンドウはHidden指定。Humanのダブルクリックやキー操作での成立を確認したものではない。画面取得・キー反応・音・面白さは未確認。
