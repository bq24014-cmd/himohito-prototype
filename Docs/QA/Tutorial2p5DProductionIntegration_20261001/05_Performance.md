# Performance

同じUnity 6000.3.21f1、Windows64 release、同じWorking Tree起点（変更前/変更後）、同じ不活性QA probe。外部BGMも同じSHA。詳細はBuildSummary.json / StandalonePerformance.json / PerformanceSummary.json / raw CSV。

## Build size

| | BuildReport bytes | MiB |
| --- | ---: | ---: |
| Baseline05 | 398151260 | 379.71 |
| Production04 | 420986444 | 401.48 |
| 増加 | 22835184 | **21.78（+5.74%）** |

3枚の新背景とShader/Sourceによる増加。両比較Buildには同じ診断probeを含む。Production側の隔離Resourcesには比較用の小さなlegacy shaderも含むため、差分は純粋なProduction assetだけの厳密なsizeではない。BuildReport値で比較し、Build時間を性能改善の根拠にしない。

## Texture memory

Editorで有効なFar/Mid/NearRefinedの`Profiler.GetRuntimeMemorySizeLong`合計：**47695488 bytes ≈45.49 MiB**。床は同じMid Textureを再使用。影は128x64 RGBA32（pixel data32KiB）のみ。PNG、import設定はT1と同じで、今回圧縮品質を変更していない。

Windows releaseで列挙したTexture2Dの同counter合計は両方99123026 bytesだった。最終Production probeは3層visible/legacyHiddenを必須確認してPASSしているが、この合計値は新しいimported Textureの増分を反映していない。**メモリ増加0とは解釈しない。VRAMの正確な差はこのrelease測定では未取得**。Editorの有効3枚counterを明記し、memory auditとtexture圧縮の検討は次Task候補に留める。

## Frame / main thread

Intel Iris Xe / D3D11。Stage1停止状態、1600x900、vsync0、targetFrameRate=-1。比較を順に実行。240framesを記録し最初の10framesを両方除外（230使用）。隠した診断Windowが止まらないよう、明示的なprobe flagでだけrunInBackground=true。通常Humanの設定は変更していない。

| | frame中央値 | p95 | Main Thread中央値 | 平均frame時間からのFPS |
| --- | ---: | ---: | ---: | ---: |
| Before | 2.597 ms | 3.873 ms | 2.589 ms | 364.7 |
| Production | 2.461 ms | 3.632 ms | 2.455 ms | 387.9 |

観測範囲で明らかなFPS悪化なし。差は測定変動の範囲も含むため、背景変更で高速化したとは結論しない。Main Thread counterは待機とCSV書込みを含み、純CPU busy time/GPU timingではない。全区間の実キーボードプレイ、低性能PC、実表示Windowの保証ではない。

EditorのClear画面で同じCamera.Renderを交互に15samplesずつ測定。最終Runtime05では旧背景中央値1.852ms→Production2.035ms（+0.184ms）。Camera.RenderのCPU呼出し時間でありframe totalではない。GIFの180ms/frameをFPSに換算しない。

## 判定

今回の最低限記録はTexture counter、Build増加、実Build frame/Main Thread、見えるFPS悪化の有無。CPU/frame budgetについてこの限定測定では重大な退行を認めない。体感の引っかかり/見やすさはHuman Reviewへ。
