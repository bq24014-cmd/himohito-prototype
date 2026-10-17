# 既存Buildの起動経路監査（実行はしていない）

対象のローカルProof root名：`PlayerSwingCandidate2_20261017`。個人名を含む絶対パスは共有資料から除いた。

| 用途 | 既存exeの相対位置 | 候補切替 |
|---|---|---|
| 前回主比較 | Build_r1/PlayerSwingProof.exe | `-presentation candidate1` / `-presentation candidate2` |
| 前回補助撮影 | Build_r3/PlayerSwingProof.exe | 同じ |

既存 `run_capture.ps1` は `-presentationQA flow` または `regression`／`cycle` と出力先を渡して自動撮影するランチャー。実キー専用ランチャーではない。

`PlayerPresentationProof.Bootstrap()` はMainStage読み込み時にPresentationCaptureを自動追加し、撮影時計を30fpsへ設定する。PresentationCapture.Start()は自動入力ドライバーを無条件追加する。

`-presentationQA` は、確認した現行ソースではsmoke／regression／cycleだけが個別分岐。省略や未知の値は通常の自動flowへ進む。`manual` や `human` という未定義の値を渡しても、実キー専用モードになると判断してはならない。

したがって、現状の既存Buildについて「この引数で通常の実キー試遊ができる」という起動方法は提供できない。今回Buildは起動せず、ゲームファイル・ランチャーとも変更していない。新しい起動経路の作成は、今回の追加実装禁止に従って保留した。
