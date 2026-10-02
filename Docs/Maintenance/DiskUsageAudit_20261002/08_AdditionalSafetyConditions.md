# 追加安全条件 — 2026-10-02

今回の追加確認では、**新しい追記文書/Evidenceだけ**をこの監査フォルダ内に作成した。前回作成した監査文書・CSV・JSON・scriptを含め、既存ファイルは編集/上書きしていない。

Production / Scene / Source / Shader / 画像 / BGM / Git index等への書込み、削除・移動・圧縮・cleanup・stage・commit・pushは実施していない。既存Unityを起動/終了する操作もしていない。

## 1. GitHub保存済みの判定を分離

Pathの包含と「現在のWorking Tree内容が保存されているか」は別物として記録した。

各ファイルを以下の列で区別する：

- local indexでtrackedか、untracked / ignoredか。
- Working Treeがmodified / clean / missingか。
- HEAD treeにそのPathが含まれるか。
- origin/main treeにそのPathが含まれるか。
- 現在内容がHEAD / origin/mainと一致するか（Gitによる改行正規化等を含む比較）。
- 現在のtracked内容が、実確認したGitHub mainにあるか。
- clean HEAD内容が過去履歴としてremoteに残るか（現在mainへの一致とは分離）。

`git ls-remote`で再確認したGitHub main：

| Repo | main SHA |
| --- | --- |
| HimoHito | `702a4bc044b0eb35bedb552753ca8495599bc1ee` |
| DESERT LOOP | `e6dc6acf0227382cc94398e6c1d36f716ff3bbb0` |

fetchをしていない。8つの調査Repoでlocal origin/mainがこの実確認値と一致することを確認した。

### HimoHitoの重要な具体例

| ファイル/素材 | 現在状態 | HEADにPath | origin/mainにPath | 現在内容のGitHub保存 |
| --- | --- | --- | --- | --- |
| MainStage.unity | tracked / modified | あり（旧版） | あり（旧版） | **未保存** |
| HimoHitoCraftRoomBackground.cs | tracked / modified | あり（旧版） | あり（旧版） | **未保存** |
| TutorialCraftRoomLayers.cs | untracked | なし | なし | **未保存** |
| TutorialBackgroundEdge / Floor shader | untracked | なし | なし | **未保存** |
| Tutorial2p5DのPNG / .meta | untracked | なし | なし | **未保存** |
| README / LEARNING_LOG変更 | tracked / modified | あり（旧版） | あり（旧版） | **未保存** |
| YasashiiOdori.mp3 | ignored | なし | なし | **未保存・必須外部依存** |

最新2.5D QA等も未追跡であり、Git管理Repo内にあることだけで保存済みとは判定しない。全てKEEP / DO NOT DELETE YETを維持。

untracked現物と同名Pathがorigin/mainにある場合も、Pathだけでは現物の一致を保証できない。そのケースは「未照合・保持」とし、保存済み扱いしない。

### 詳細Evidence

- `Evidence/AdditionalSafety_AllRepoFileStates.csv`: 8 Repo / 38,875行。tracked、modified、HEAD/origin包含、現在内容の一致を個別記録。
- `Evidence/AdditionalSafety_FolderGitStates.csv`: 201フォルダ。tracked/untracked/modified数とHEAD/origin包含件数を分離。
- `Evidence/AdditionalSafety_Protected2p5DGitStates.json`: 未保存Production 2.5Dの具体的判定。
- `Evidence/AdditionalSafety_GitStateSummary.json`: Scopeと件数。

これは前回の「GitHub保存済みsubset」を更に詳しく分解した追記。フォルダ全体が保存済み、あるいは削除可という意味にはしない。

## 2. 同時に扱える非重複Pathのみ集計

候補60 Path（LOW57 + MEDIUM3）について、各Pathの親/子に別候補が存在しないことを再検査しPASS。

- LOW: 75,436,970,367 bytes = 70.256153 GiB。
- MEDIUM: 1,592,631,903 bytes = 1.483254 GiB。
- 合計: **77,029,602,270 bytes = 71.739408 GiB**。
- 最新Tutorial2p5D関連の候補数: **0**。

親のH/.codex_tmp / D/Logs / visualizationsの容量を追加加算していない。Sourceや未保存rawも候補合計に含めない。条件付き候補であり、現在使用中のUnityやexeを確認せず削除可能という意味ではない。

容量は前回の点-in-time論理サイズを用いた重複検査・再加算であり、全ルートを再scanした最新実占有量ではない。実行する将来Taskでは対象Pathと容量を直前に再確認する必要がある。

## 3. Cドライブ空き容量

| 記録 | 測定時刻 JST | Free bytes | Free GiB |
| --- | --- | ---: | ---: |
| 前回監査で最初に保存した値 | 正確な測定時刻は記録なし | 23,608,430,592 | 21.987064 |
| 今回の追加安全監査・開始測定 | 2026-10-02 07:21:30 | 25,035,255,808 | 23.315899 |
| 今回の追加安全監査・終了測定 | 2026-10-02 07:31:46 | 24,997,052,416 | 23.280319 |

今回の開始→終了の差: **-38,203,392 bytes**（約-0.035580 GiB）。監査資料の新規作成と他の稼働中アプリ/システムも空き容量に影響し得る。差分の全ての原因を特定したものではない。

前回の厳密な「scan開始直前/終了直後」の空き容量は、当時の測定時刻が揃っていないため後から復元できない。最初の既存記録を開始時刻付きの値へ捏造せず、この限界を明示した。今回の追加監査では開始/終了を時刻付きで記録した。

終了値は、この追記Markdownの新規保存直前の測定。測定後の数KiBの文書作成や他processの更新による瞬間的変動はあり得る。

`Evidence/AdditionalSafety_DriveStart.json` / `AdditionalSafety_DriveEnd.json` / `AdditionalSafety_CDriveTimeline.json` に値・測定方法・時刻・制限を保存。ディスククリーンアップは実施していない。

## 4. 書込み禁止の保護確認

`Evidence/AdditionalSafety_Verification.json`：

- HimoHito Production / Git identity開始・終了一致: PASS。
- DESERT LOOP Production / Git identity開始・終了一致: PASS。
- 前回の監査資料38ファイルもSHA一致: PASS。
- 候補60 Pathのpairwise非重複: PASS。
- 再加算値が前回候補合計に一致: PASS。
- 新規2.5D候補なし: PASS。

GitのOptional locksを無効化し、外部diff/textconvを実行しない読取コマンドを用いた。今回書き込んだのは、この監査内の**新しいAdditionalSafety Evidenceと本追記文書だけ**。既存ファイルは編集していない。

稼働中のCodex/Unity/OS等が自ら書き込む内容までPC全体のbyte不変を保証するものではない。監査AgentによるProduction書込み・cleanupはなし。追加条件を記録して、ここでSTOPする。
