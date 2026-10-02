# 実装方式 — Tutorial全区間への安全なtrial展開

## 入力と隔離

前回P1のFar/Mid、R1のNearRefinedとFloorBlend shaderをbyte無変更で再利用。元PNGと過去Proofは編集しない。Production Working TreeのAssets/Packages/ProjectSettingsを新規隔離コピー `.codex_tmp/Tutorial2p5DAllSections_20261001/TrialProject` に複製した上で、trialコードだけ追加する。

Resources素材とUnity .metaは一緒にコピーし、生成されたtrialコードの .metaもEvidenceへ保存する。ProductionのSource/Scene/設定は編集しない。BGM原本は既存の外部依存をコピーして使用し、Gitへ追加しない。

## 引き継いだもの

- Far/Mid/Near: Z=7/5/3、order=-130/-110/-90。
- X追従率: 0.99/0.92/0.88。NearはR1の控えめな追従。
- 同比率36 world unit以上のoverscan、CameraY+1.1の配置。
- R1の小物の局所影・反射色。
- 既存Mid下17%を使った近い床帯。NearとXを同期し、上端alpha fade。
- 背景床の投影影。既存Collider.boundsを読み取るだけで、物理支持面は変更しない。

## 全区間に広げるための変更

背景配置を **区間番号によらないCameraXの連続関数** にした。

`layerX = -4 + follow * (cameraX + 4)`

開始基準はTutorialの既存開始位置X=-4で固定。区間到達時に原点をリセットしない。RでCameraが既存仕様どおり戻ると、背景も同じ関数で元の場所へ戻る。BackgroundはCameraを書き換えない。

第1区間のfinite image clampをそのまま全区間に使うと、後半のNear/Midの視差が止まる。そのためAllTutorialモードではclampを解除し、FarとMidの左右に同一テクスチャのmirror guardを配置した。タイル境界は同一端pixelが接する。床帯も同じguard方式。通常Gameplay範囲で見えるのは中央画像と端の狭い延長部分で、別世界の画像へ差し替えない。

Nearの汽車や毛糸玉は繰り返さない。1枚の透過画像をそのまま独立移動させ、玩具が画面左へ抜け、右の糸巻き・積み木が徐々に近づく配置を保つ。新しい玩具画像は作成しない。

背景そのものをSectionで交換しないため、Section番号更新で飛ぶ原因を作らない。第1～4区間は同じ窓・壁・棚・床を通して見る。差分はCamera移動による構図と小物の見える割合。区間ごとの派手な色変更・違う部屋は採用しない。

## 各区間の構図

| 場所 | Camera travelの目安 | 主な見え方 |
|---|---:|---|
| 第1区間開始 | 0 | 成功済みR1と同じ窓・カーテン、汽車・毛糸玉、右棚 |
| 第2区間開始 | 16 | 同じ部屋を連続移動。左の玩具が少し抜け、右側が近づく |
| 第3区間開始 | 31 | 糸巻き・毛糸・積み木側の比重が増す。橋の背後は暗い布壁 |
| 第4区間開始 | 43 | 棚・工作玩具側へ寄る。中央Hook、梁、2本橋の領域は余白を維持 |
| Goal付近 | 約67 | 同一層の視差を継続。外側guardで背景の空白を防ぐ |

R1の支持影を5つの既存床へ展開。足場のSprite・色・Transform・Colliderを変更しない。投影影で物理下端と背景床が完全に一致したと扱わない。

## 画面状態 / 復帰

- Tutorialプレイの全区間で有効。
- Failed / Clearing / Clearでも世界の背後を保ち、結果表示だけで背景を急に切り替えない。
- Stage Selection、開始カーテン、開始見渡し中は既存背景を保持。
- Scene変更時は元SpriteRendererのforceRenderingOffを復帰。
- MainStageにはコンポーネントを生成しない。

比較モード:

- default: AllTutorial。
- `--tutorial2p5d-before`: 第1区間のみ前回R1、後半はProduction背景。
- `--tutorial2p5d-production`: 全区間Production背景。

## 検証の性質

AllSectionsQAは過去の正常通しルートから、A/Dキャッシュ、既存ジャンプbuffer、E/Q/F相当public API、看板とRの既存methodを使う。フック/足場の配置変更、bodyの強制移動、新しいチェックポイントの捏造は行わない。E解除後の速度、実際の橋Collider、着地によるSection更新、実際のGoalを使う。

写真のBefore/After切替時にGameplay物理状態を照合。3つのSection境界では背景変位がFollow×Camera変位に一致するか記録。これらはcontrolled diagnosticであり、OS実キーボードやHuman通しプレイの代わりではない。

Runtime01では同期撮影IOが次frameのphysics catch-upを増やし、短距離歩行の検証がovershootした。Runtime02では診断だけ `Time.captureDeltaTime=1/60` の記録時計を使い、motion captureを640×360にする。歩行入力は目標近くで減らす。ProductionのfixedDeltaTime/forces/cameraを変更せず、standaloneにはこのEditor専用diagnosticが含まれない。動画資料は実時間FPS評価に使用しない。

## 既知のtrial制約

Mirror guardは安全な端埋めであり、新しい広い部屋panoramaではない。後半・極端な画面比率で棚/木目の対称性やNear素材端が気にならないか、実画像とHuman Reviewで評価する。完全に解決していなければT2とし、GameplayやCameraを変更して隠さない。
