# 隔離候補2の変更範囲

| ファイル | 種類 | 内容 |
|---|---|---|
| Assets/PlayerPresentation/SwingPresentationState.cs + .meta | 表示用・新規 | 実速度、接線方向、下向き整列、既存入力から首下／腰／膝の角度を選ぶ。周期時計なし |
| Assets/PlayerPresentation/PlayerMotionPresentation.cs | 表示用 | candidate2分岐と姿勢uniform。候補1時は従来メッシュ。candidate2は20×48のrender-onlyメッシュ |
| Assets/Resources/PlayerPresentationYarn.shader | 表示用 | 既存fragment色調処理は不変。首下／腰／膝を剛体回転でskin。物理shapeを書き換えない |
| Assets/Scripts/RopeBodyVisual.cs | 表示用・読み取り口のみ | 現在の接続／解除演出状態と、既存画像landmarkからのrig座標を公開。元の姿勢選択・描画Transform処理は変更なし |
| Assets/PlayerPresentation/PresentationCapture.cs | 隔離診断専用 | 候補2状態を記録。振り子途中のポーズ／再開と、状態モデルの検査を追加 |

RopeSwingPoseState、RopeLandingFluff、PlayerContactShadow、CraftPlayerArt、PrototypeAudioFeedback、元のArt／音声は候補1から不変。5指定Core Sourceも不変。新しいキーや入力モードはない。

メッシュの曲げは表示用で、頭より下だけの変位。新しい全身scale変更やphysics root移動はない。既存の候補1の小さな遅れ・空中bracing・着地はそのまま。解除時だけ最後の追加曲げを既存settle時間へ受け渡し、次の空中絵には持ち越さない。

このコードはProduction未採用。GitHubには実行用.cs/.shader/.metaを置かず、review runの差分文書としてだけ保存する。Build、Library、BGM、巨大な全ログも共有対象外。
