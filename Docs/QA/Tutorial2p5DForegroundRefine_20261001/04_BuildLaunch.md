# Build / Launch evidence

Unity: `6000.3.21f1`, StandaloneWindows64, Direct3D11。

## Build

- 新規出力: `output/Tutorial2p5DForegroundRefine_20261001_Refine01/Game/`
- 結果: **Succeeded**
- BuildReport totalSize: **429,267,997 bytes**
- BuildReport totalTime: **55.4806139 seconds**
- exe: `HimoHitoForegroundRefine.exe`
- exe SHA256: `837986201D89BE25426B8415CB3BC945F077E5B13F4722D6763D04EBD38C5472`
- ログ: `Evidence/UnityBuild.log`。Build結果本体もGame/BUILD_RESULT.txtに保存。

## 起動

- 実起動コピー: `C:/Users/Public/HimoHito_Tutorial2p5DForegroundRefine_20261001_Refine01/`
- コピーexe SHA256はBuild原本と一致。
- PID **29148** / ProcessName **HimoHitoForegroundRefine**。
- MainWindowTitle **Tutorial2p5DForegroundRefine_20261001**。
- MainWindowHandle **1641992** / Responding **True**。
- Player_Refine.logでDirect3D11初期化、assembly load、input initializedを確認。

この記録はexe起動・応答の確認。OS上のStage Selection表示や人間の操作評価を自動的にPASSとしない。実Unity描画とcontrolled-input確認は別のRuntime_Attempt02 Evidenceに保存している。

比較用ランチャー2つを新規出力フォルダに追加。前回のPublicコピー / Buildを上書きしない。今回のGUIゲームだけをHuman Reviewのため開いたまま停止する。

## 保護

隔離Build後のTutorial / MainStage Scene SHAはProduction原本と一致。隔離コピーの既存Source / PackagesはProduction原本と一致。既存ProjectSettings.assetとの差は比較ゲームのcompanyName/productName隔離だけ。Production原本はその設定変更も含めて一切変更しない。

全3,175既存ファイルの終了照合は `ProtectionResult.json`。新規Buildは今回のoutput追加であり、既存Buildは保持。Git index空、commit / pushなし。
