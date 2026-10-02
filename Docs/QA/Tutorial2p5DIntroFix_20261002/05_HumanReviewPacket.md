# Human Review Packet

## 比較資料

- Screenshots/Intro_BeforeAfter_Overview.png：Intro開始・中間・Gameplay再開を左右比較。
- Screenshots/Intro_BeforeAfter.gif：ゴール→スタートの実連番比較。
- Screenshots/A_TutorialCommitted_Comparison.png〜F_AfterMovement_Comparison.png：六時点。
- Screenshots/Before/、After/：元画像と39枚ずつの連番PNG。
- Screenshots/Regression/：全4区間、R復帰、橋、Merge、Clear、ゴール端の再取得画像。

World-camera画像なので布/HUDは含まない。図のラベル以外、ゲーム内画像を描き直したり素材生成したりしていない。Before/Afterは同じProduction Working Treeの表示条件だけが異なる独立コピー。時間の近い実サンプルを比較しており、全点がframe完全一致するという資料ではない。

## 新しいWindows Build

`output/Tutorial2p5DIntroFix_20261002_Review01/Game/HimoHitoIntroFix.exe`

ランチャー：`output/Tutorial2p5DIntroFix_20261002_Review01/RunReview.ps1`。Gameフォルダを丸ごと保管する。過去Buildは上書きしていない。

Buildと実exe起動、Stage Selection→Tutorial開始→スキップしないIntro→Gameplay復帰は自動確認PASS。Evidence/StandaloneIntroの13 checks、2,156観測framesで新3層消失0、新旧二重drawable0。

通常起動には`--intro-qa-output`を付けない。明示フラグがない限り隔離用probeはobjectを作らず、入力やCameraに作用しない。127項目ハーネスはEditor限定でrelease中に存在しない。

## Human確認項目

1. Stage SelectionでTutorialを選んでEnter。
2. Space/EnterでIntroを飛ばさず、布が開いてからゴール→スタートまで見る。
3. 背景が途中で旧版へ戻らないこと、Intro終了時に絵柄が切り替わる感じがないこと。
4. A/D・Space・Eで通常操作。主人公/Hook/床が見やすいこと。
5. 余裕があればSection1〜4、R、橋・F merge、Clear、Stage Selectionへ戻る。
6. ゴール右端の縦線や前景の継ぎ目が悪化していないこと。
7. 体感で新しいカクつき、二重背景、背景消失がないこと。

回答分類：PASS / FAIL / UNCLEAR。FAILなら時点（布Opening / Intro開始・中間・終了 / Gameplay / Restart）、操作、症状、再現率、画像や動画名を記録する。

**Human Review = APPROVED**。2026-10-02、今回の通常起動Buildで「Tutorialを開始し、Introをスキップせず、背景が途中で旧版へ戻らず、操作開始時に切替感がないか」を確認する質問に対し、ユーザーが「問題なし・採用してよい」と回答。

承認範囲は今回のIntro不統一解消と採用。全4区間の各操作を今回Humanが再実施したという証拠としては扱わない（127項目の全4区間回帰は自動実行）。前Task/P1/R1/T1の承認を流用していない。
