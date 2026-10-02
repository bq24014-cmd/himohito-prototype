# Intro Transition Timeline

Before/AfterともWorking Treeから独立コピー。`StartSelectedStage()`を使い、既存の布演出、ゴール待機、紹介カメラをスキップせず通した。Cameraのprivate Advance・Transform・速度・orthographicSizeに診断から書き込んでいない。

## AFTER — 六時点

|時点|Far/Mid/Near|旧背景drawable|Camera X|Camera state|
|---|---|---:|---:|---|
|A Tutorial開始確定直後|すべて表示|0|63|布Opening、Previewはゴール待機、elapsed0|
|B Intro開始|すべて表示|0|63|布終了、Preview開始、elapsed0|
|C Intro中間|すべて表示|0|33.699593|Preview elapsed3.641701 / total7.283333秒|
|D Intro終了直前|すべて表示|0|-3.936172|終了約0.187秒前、Preview継続|
|E Gameplay開始直後|すべて表示|0|-4|Preview終了、body/mover/ropeを既存処理で復元|
|F Player操作後|すべて表示|0|-2.899876|通常追従、cached移動入力で歩行|

Preview中Y=0、Z=-10、orthographicSize=8.7でBefore/After一致。両版の全Preview CSVを既存PanProgress式と照合し、X最大誤差はBefore6.53e-6・After6.42e-6未満。速度・経路・ズーム仕様を変更していない。録画IO/Editor schedulingで自然なサンプル時刻はわずかに異なるため、Before/AfterのC/Dを同一フレームとは主張しない。

## BEFORE / AFTER

Before：A/B/C/Dは3層非表示・legacy6 renderer drawable。E/Fから新3層に切り替わる。開始確定後1,764観測framesのうち1,713で新3層が非表示。

After：開始確定後1,733観測framesで新3層非表示0、legacy drawable0。新旧同時drawable0。6時点を含む13 assertionsすべてPASS。Beforeの13 PASSは「不具合の再現期待と一致」という意味であり、Before版の採用PASSではない。

両版ともメニュー契約PASS：Stage Selectionおよび布Thread/Closing/CoveredのWaitingToStartでは新3層を隠してlegacyを維持。Aは布で隠れたTutorial開始確定時点。メニューのクリック直後をGameplay背景として扱っていない。

## Captureの制約

PNGは実Camera.Renderの1600x900 world画像。IMGUIのメニュー・布・HUDは含まない。したがってAは布の背後の背景状態を示す。布を含む実画面の切替感は新規exeのHuman Reviewで確認する。

連番は各39枚960x540、6時点とメニュー画像は各7枚1600x900。GIFは修正前後の実サンプルをPreview elapsed比の近いもの同士で並べ、640x360へ縮小、200ms/frameで提示。Realtime video・同一frame comparison・FPS測定ではない。

原データ：Evidence/TimelineBefore/frames.csv、TimelineAfter/frames.csv、各checks.txt・result.txt。画像と対応表：EvidenceManifest.json。
