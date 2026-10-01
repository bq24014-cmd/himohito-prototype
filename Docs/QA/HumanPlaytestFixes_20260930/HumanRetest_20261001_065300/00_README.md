# HumanPlaytestFixes — 修正後Human追試

- Build ID：`HumanPlaytestFixes_20261001_065300`
- Build方式：現在のproduction Working Treeを新規隔離コピーしてWindows x64を生成。以前の診断用コピーのInput差し替え・Audioカウンターは含みません。
- Unity：`6000.3.21f1`
- 基準HEAD / origin/main：`7644fdcdf7adb793fcc6e3ac9d6540f317766ec6`
- 未CommitのH1/H2/H3/S2修正とMainStage.unityを含みます。各入力ファイルのSHA-256はBuildManifest.json参照。
- 新規出力：`output/HumanPlaytestFixes_20261001_065300/`
- 起動：同フォルダーの `START_HUMAN_RETEST.cmd`。実行先は `C:\Users\Public\HimoHitoHumanRetest_20261001_065300\Game`。

本Taskでは追加実装・難易度・Scene・UI・Art・Audio調整、stage / commit / pushをしません。過去Build・既存QAを上書きしません。

## 何を確認するか

1. Tutorialの4つの看板の小さい絵・操作キー・床/矢印線の読みやすさ（H1）。
2. Tutorial Clear中にA/Dを押しても足音が鳴らない（H2）。
3. MainStage Clear中にA/Dを押しても足音が鳴らない（H2）。
4. Section 9の左橋生成後、2本目の右Hookを狙いEで正しく接続できる（H3）。
5. Tutorial看板3の消費7の残量例が99→92（S2）。

順序はTutorial → 本編を推奨。ステージ選択ではどちらも最初から選べます。通常プレイで確認し、テレポート・自動入力・無敵・検証用キーは使いません。本編には第8区間がなく、第7→9→10です。

## 判定・証拠

各項目は `PASS / FAIL / DIFFICULT BUT INTENDED / UNCLEAR`。Humanの未確認項目は空欄のままにし、起動成功や前Taskの自動検証をHuman PASSへ転記しないでください。

FAIL時はScene、Section、キー操作、発生内容、試行数/発生数、スクリーンショット/動画名、Playerログ名を記録します。スクリーンショットはHUDや狙っているフックが入る構図を推奨。音の問題は動画/録音があると助かります。

`01_ManualChecklist.md` を見て実操作し、`02_ResultTemplate.md` のコピーへ記入してください。記入済みの結果は元の空テンプレートと区別して保存します。

Build/起動/保護の実行証拠は、このQAフォルダーの `Evidence` に保存します。Humanによる5項目の判定は、このBuild作成Taskの対象外です。
