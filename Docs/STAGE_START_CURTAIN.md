# ステージ開始・毛糸で引く布（2026-09-10）

## 目的と範囲

ステージ選択から遊び始める切り替えを、選んだおもちゃの島から伸びる毛糸で紺色の布を引く演出にする。布で画面を完全に覆った間に開始・読み込み・既存の紹介カメラの準備を行い、布を右へ送りながらステージを見せる。

選択時の約0.4秒の毛糸枠とは別の一度きりの開始演出で、枠の完成を待たずにEnter／「ここであそぶ」を受け付ける。ステージ内の足場・フック・プレイヤーの配置、移動物理、ヒモ残量、既存の紹介経路・ズーム、効果音、BGM、クリア記録の仕様は変更しない。新しい保存項目・ステージ・敵は追加しない。

## 入力と進行

| 段階 | 時間の目安 | 見た目と処理 |
| --- | --- | --- |
| 毛糸を伸ばす | 0.22秒 | 開始時に選んだ島を起点に、既存のピンクの毛糸が左右へ伸び、結び目が現れる。 |
| 布を引き込む | 0.58秒 | 不透明な紺色の布が左から右へ入り、全画面を覆う。折り返した縁・縫い目・結び目を伴う。 |
| 完全に覆って待つ | 少なくとも完全な描画フレームを挟む | 下の画面が見えない状態でチュートリアルの開始を確定、または本編の非同期読み込みを開始する。読み込み・準備の所要時間に応じて布を維持する。 |
| 布を開く | 0.62秒 | 紹介カメラをゴール側で待機させ、同じ布をさらに右へ送って左側からステージを見せる。 |
| 通常の紹介へ渡す | 既存の紹介時間 | 布が消えてから、従来のゴール側での待ち・開始地点へ戻る紹介を進める。 |

固定演出分は合計約1.42秒。これに、完全に覆ったフレームと読み込み・準備待ちが加わる。`Time.unscaledDeltaTime` で進めるため、メニューの停止状態でも動く。IMGUIのLayout／Repaint回数で進行させない。

受け付け後は選択対象を固定し、Enterやクリックの連打による重複開始、左右の選択・ページ移動、操作説明、Escによる終了を受け付けない。追加予定の島は開始演出に入らず、シーンがBuild Settingsにない場合も選択画面で案内する。布が残る間はゲーム操作と紹介のスキップを止め、開始に使ったEnterがそのまま紹介のスキップにならないようにする。

## 開始と既存のカメラ紹介

チュートリアルは `StageCatalog.TitleScenePath` のメニュー兼用シーンを再読み込みせず、既存コントローラの開始状態を完全な布の下で確定する。本編は `SceneManager.LoadSceneAsync` のSingle読み込みとし、布の管理オブジェクトを読み込みの前後で維持する。読み込み直後のプレイヤーは一時的に保持し、既存の `MainStagePreview` が元の操作状態を引き継いで紹介を準備する。

紹介は従来と同じゴール側へカメラを置く。布の開閉中は紹介の経過時間を進めず、待ち時間や移動を布の裏で消費しない。布自体はカメラを動かさず、既存の紹介を追加のズーム・上下動・揺れで変形しない。開き終わった後の紹介完了・スキップ・プレイヤー操作への復帰は既存の仕組みを使う。

新しくカタログへ登録するシーンには、現在の `PlayerMover`・`Rigidbody2D` と互換性のあるプレイヤー、カメラ、および `MainStagePreview` から参照できるゴール側の対象が必要。カタログ登録だけでは紹介用の配置・開始位置・残量設定は作成しない。紹介対象を準備できなければ布を開いて保持前の操作状態へ戻す（別シーンの読み込み側では警告も記録）。読み込みに失敗し元のメニューが残っている場合は、エラー案内を残して布を開く。外部から別のシーンへ切り替えられた場合も古い布を残さない。

## 布の描画と素材

- 採用画像：`Assets/Resources/Art/HimoHitoStageCurtainCloth-v1.png`。PNGヘッダーで確認した実寸は1254×1254。下記プロンプトの希望サイズ1024×1024とは異なるツール出力を、そのまま正方形タイルとして使用する。
- 生成：built-in `image_gen` による新規生成。外部APIキーやCLIによる画像生成は使用していない。
- 元出力（制作PC）：`%USERPROFILE%/.codex/generated_images/019fdfa3-5ee1-7911-8a24-63282af5bae4/exec-4ec171e5-616c-4505-82ab-3c7a12970f1d.png`。
- インポート：通常のTexture2D、sRGB、mipmapあり、Bilinear、Repeat、読み取り不要、アルファ不使用。拡大上限2048、NPOTサイズは維持する。
- 夜の子供部屋に合う低彩度の紺・青紫の細かな織り目だけを画像に含める。布の縁・折り返し・小さな暖色の縫い目はコードで描き、画像へ焼き込まない。
- 毛糸は `YarnRopeTexture.Load()`、結び目は `HimoHitoUiParts.MountingKnotSprite` を再利用する。文字、ロゴ、点滅、光、ズーム、上下動、新しい音は重ねない。読み込み中の布は静止する。

`StageStartCurtainView` は渡されたビューポート幅・高さと島の正規化座標だけで描画する。布の織り目は約240画面単位で繰り返し、画面サイズに合わせて大きく引き伸ばさない。布の矩形は画面の上下左右より大きくし、16:9に限らず縦長・横長の余白も不透明に覆う。素材にアルファが含まれたり読めなかったりしても、背後の不透明な紺色で読み込み中の画面を隠す。

描画時は画面全体の座標と手前の描画深度を使い、終了時に `GUI.matrix`・`GUI.color`・`GUI.enabled`・`GUI.depth` を復元する。時間・入力・読み込みはこの表示クラスへ持ち込まない。

## 生成プロンプト（原文）

```text
Use case: stylized-concept
Asset type: seamless tileable game fabric texture, opaque square 1024x1024
Primary request: a close-up straight-on square of softly woven midnight indigo cotton-linen cloth for a handcrafted yarn-and-wood toy world. Delicate visible warp and weft, tiny organic fuzzy fibers, softly rounded threads, tactile high-quality miniature stop-motion craft-game material. Restrained blue-violet/navy palette with slightly warm fiber highlights. Flat uniform diffuse lighting and uniform exposure over the entire square. Full-bleed fabric only, no hem, folds, shadows, vignette, embroidery, objects, text, logos or border. Seamless matching edges, no focal motif, no transparency. This will tile on a moving screen curtain, so the grain must be subtle, even and continuous.
```

## 関連コード

- `Assets/Scripts/PrototypeRunController.cs`：開始要求の検査、チュートリアルの開始確定、メニュー操作の制御。
- `Assets/Scripts/StageSelectionView.cs`：選択中の島の位置を画面内の正規化座標として渡す。
- `Assets/Scripts/StageStartTransition.cs`：演出の段階、完全な被覆待ち、シーン読み込み、プレイヤー保持と解除。
- `Assets/Scripts/StageStartCurtainView.cs`：布・毛糸・縫い目・結び目の描画。
- `Assets/Scripts/MainStagePreview.cs`：既存紹介の準備と、布が開き終わるまでの待機。

## 確認方法

UnityのPlayを停止して再開し、ステージ選択でチュートリアルまたは本編を選び、Enter／「ここであそぶ」を押す。毛糸→布で覆う→布が右へ抜ける→ゴールからスタートへの紹介、の順に進む。紹介は従来どおりSpace／Enterでスキップ可能。切り替え中の入力は受け付けないため、布が消えた後に押し直す。

紹介カメラを持たない状態での検証では、布の開き終わりまで主人公を止め、開いた時点で操作を戻す。Unityの非同期読み込み自体はキャンセルできないため、読み込み途中の演出オブジェクトを外部のデバッグ操作で破棄しても、進行中の読み込みは完了しうる。通常のプレイUIからは中断操作を設けていない。

## 検証結果

- 隔離したUnity 6000.3.21f1でコンパイル成功。元のUnity・シーン保存・本番PlayerPrefsには触れず、製品ビルドは作成していない。
- 実Playモードでチュートリアル開始、本編への2回連続ロード、紹介の停止と操作復帰を検証。連打・選択変更・ヘルプAPIの拒否、追加予定、存在しないシーン、閉じた後のシーン再検査、紹介対象がないときの復帰、読み込み前の演出破棄、無関係なAdditive通知も確認。`StageCurtainFlow-2.log` は4654件成功（各フレームの停止状態チェックを含む）。実キー連打の手動操作ではなく、開始APIと紹介終了メソッドを呼ぶ動線テスト。
- 実レンダラーの15状態・142項目を検証し、16:9、4:3、21:9、640×360で、完全被覆時は全画素が背後の色に依存しないこと、開始前／終了時に何も残らないこと、GUI状態復元を確認。`StageCurtainUi-6.log` は0失敗。
- 確認画像：[布を引き込む途中](Images/StageStart-Curtain-v1.png)。実際の描画メソッドへ中間値を渡した画像で、クリア印は隔離環境の検証用記録。読み込み瞬間のスクリーンキャプチャではない。
- EditorWindowの描画先は外枠の高さとIMGUIのScreen.heightが一致しない。撮影先をScreenサイズへ合わせ、タブ領域を除いた実ゲーム領域を切り出すことで、検証用の黒帯を取り除いた。製品のレイアウトや被覆判定をこの撮影都合で変えていない。
