# Human Review Packet

## Build

新規Windows64 Build：

`output/Tutorial2p5DProductionIntegration_20261001_Production04/Game/HimoHitoProduction2p5D.exe`

日本語path起動問題を避ける専用コピー：

`C:/Users/Public/HimoHito_Tutorial2p5DProductionIntegration_20261001_Production04/HimoHitoProduction2p5D.exe`

`output/Tutorial2p5DProductionIntegration_20261001_Production04/Launch_HumanReview.ps1`で通常起動。性能測定flagを渡さず、QA probeは何も生成しない。過去Buildは上書きしない。検証コピー限定の会社名・product名を使用し、元ゲームのPlayerPrefsを共有しない。Production01〜03は検証過程でありHuman採用確認に使わない。

## 比較

- `Screenshots/AllSections_Overview.png`：全4区間。
- `Screenshots/S4_Clear_Comparison.png`：左は承認済みT1、右はProduction。同じGameplay状態。
- `Screenshots/GoalSeam_Closeup.png`：ゴール右端の実capture pixel拡大（描き直しなし）。
- `Screenshots/Section1_ActualMotion.gif`〜`Section4_ActualMotion.gif`：実routeのsample。録画そのもの/FPS評価ではない。
- `Screenshots/Goal_Far_Only.png` / `Goal_Mid_Only.png` / `Goal_NearRefined_Only.png`：層別確認。

画像は1600x900 world-camera描画でUI/HUDを除く。ゲーム画面を合成して新しい見た目を捏造していない。

## Human確認

1. Stage Selection→Tutorial開始。最初の窓/カーテン/玩具とPlayer/青Hookの見やすさ。
2. A/D移動、Spaceジャンプ、矢印照準/E接続。Near/Mid/Farの自然な奥行き。
3. 4区間を順に進め、背景が境界で跳ねたり二重表示されたりしないか。
4. 第3区間のQ橋、4区間の2本Q→F merge。看板・緑Hook・ヒモ橋が背景に埋もれないか。
5. R復帰、Clear、Stage Selection復帰。表示の取り残しがないか。
6. ゴール床の右端を左右に歩き、以前の細い縦線や床の切れ目が目立たないか。
7. 体感のFPS低下/引っかかり、HUDや操作を邪魔する前景がないか。

今回のHuman Reviewは **ISSUE REPORTED / 採用保留**。Production04で「最初のスタート地点に向かっていくカメラのときの画面が旧のものになってる」との報告。イントロ中の背景可視判定を次Taskで修正・追試する必要がある。P1/R1/T1への過去の承認を今回の最終承認に流用しない。

記録：PASS / FAIL / DIFFICULT BUT INTENDED / UNCLEAR。FAILは区間・操作・症状・再現率・画像/動画名を書く。
