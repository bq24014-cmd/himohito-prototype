# Regression / Performance / Protection

## 既存回帰

前TaskのEvidence/IntegrationQA.csとReferenceTrial.csをbyteコピー。127項目のハーネス本体は変更せず再実行。

結果：**checks127 / failures0 / runtimeErrors0 / physicalTransitions3 / motionFrames35**。

- Section1〜4：左右歩行、ジャンプ・着地、看板開閉、Hook照準・E接続。
- 実物理でSection1→2、2→3、3→4。背景追従位相と境界連続性PASS。
- Section3長さ7の橋、Section4の2橋とF merge、center Hook消失、R checkpoint復元、再生成・再merge。
- 実ルートで宝箱に到達、Clearingの操作ロック・背景、Clearの背景。
- Clear→Stage Selectionでlegacy復元、MainStageでは新Tutorial背景componentなし。
- Before(T1 trial)/After(Production)比較の間でplayer/physics/collider/joint/rope/Camera状態不変。

Evidence/Regression/result.txtとchecks.txtに全記録。Editor SearchDatabaseの起動時例外1件は前Taskと同じEditor indexing stackで分離記録され、ゲームRuntimeエラーは0。隠してPASSにしていない。

## 継ぎ目

既存Mid edge-clamp / Floor edge-clamp・sprite flip shaderのassert PASS。PNG/Shaderは変更禁止・開始時SHAで保護。S4_ClearとGoal各レイヤー画像は再取得。境界位置はEvidence/Regression/seam_position.txtに記録。

## 表示条件修正の負荷

Intro After：最大owned drawable15（3主層＋4guard＋3floor＋5support影）。legacy drawable最大0、同時drawable0。Before Gameplayにも同じ15 rendererがあり、今回新Renderer・Texture・Materialを追加していない。

上記はenabled/activeInHierarchy/forceRenderingOffで判定した描画対象Renderer数でありGPU draw call数ではない。旧背景の6rendererは新背景表示中forceRenderingOffなので、重複描画し続ける状態ではない。

既存127ハーネスの3有効Texture counter：48,174,888 bytes（約45.94MiB）。Editor内計測値でtotal VRAMではない。PNG identity、圧縮設定、Shader、背景構成は今回変更なし。前Taskのcounterとの小差を今回の素材追加と解釈しない。

Camera.Render CPUとEditor frame timingも既存127ハーネスで取得。legacyと3層の比較であり、今回条件のBefore/After同一フレーム比較ではない。イントロには既存新3層の描画費が乗るが、メニューやGameplayのRenderer構成は変更なし。体感FPS・布の切替感はHuman Reviewで最終確認する。

実測中央値：Camera.Render CPU-call（各15 samples）legacy1.5301ms / 新3層1.7119ms。Editor frame interval（各120 samples）legacy3.61975ms / 新3層3.3336ms。カメラ画像取得処理を含むEditor限定の参考値であり、GPU負荷・通常Windows Gameplay FPS・今回条件変更だけの差として扱わない。Evidence/TimingSummary.jsonに分布とmaxを記録。

Windows実exeでもスキップしないイントロ確認13 assertions PASS。2,156観測framesで3層非表示0、legacy drawable0、新旧同時drawable0。最大owned15。通常プレイ時には起動フラグなしなので診断objectを生成しない。

新規Windows Build：Succeeded / 420,989,994 bytes / 102.771秒。前TaskのProduction04は420,986,444 bytes。差は3,550 bytes（隔離QA helper内容・再Build metadataも含み、表示条件変更だけのサイズとは分離できない）。素材増加なし。新規出力のため過去Buildは保持。

## Git・ファイル保護

開始時の5,434既存ファイル（Assets / Packages / ProjectSettings / Docs / output / Builds等）をSHA256で記録。今回許可する既存ファイル変更はTutorialCraftRoomLayers.csだけ。全Sourceのうち表示条件以外を変えていないか、Before Sourceとのdiff・SHAと終了時ProtectionResultで確認する。

既存MainStage Sceneには未Commit変更がある。SHA `57D2464751B5B9C2A78E6C56F34197E9BC2629983FADAA1DFF6E0BA0B4BC2284`を保持する。Tutorial SHA `DC1BED54CC083256CE895041B1C93578101E19B9738E7CF260619E8F7E1E4B92`、BGM SHA `25C52DA234D42C7A5E5CC91D45AEED14829F845EA15932A88AC94AB006781E83`も保護。

global git diff --checkには既存MainStage.unityのm_Name行末spaceが含まれる。今回範囲外なので整理しない。今回Sourceの条件patchを個別監査する。indexは空、commit/pushなし。README/LEARNING_LOGを含む既存差分も変更しない。
